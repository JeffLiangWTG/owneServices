using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Messaging.GUI
{
	public partial class EDIInterchangeForm : ZTemplateForm
	{
		readonly List<int> addedMenuItemList;

		public EDIInterchangeForm(EDIInterchange interchange)
			: base(interchange)
		{
			InitializeComponent();
			addedMenuItemList = new List<int>();
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			AddInterchangeModificationMenuItem();
			UpdateSaveToDiskUI();
			EventTabPage.TabVisible = Interchange.ShouldShowInterchangeEventsTab;
		}

		void AddInterchangeModificationMenuItem()
		{
			if (IsInterchangeModificationAllowed)
			{
				MainMenu.MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("E728DC17-CFDD-4432-A0C7-F3DACACB3E3E", "Modify Interchange"), new EventHandler(InterchangeModification_Click)));
			}
		}

		void InterchangeModification_Click(object sender, EventArgs e)
		{
			if (Interchange != null)
			{
				var form = new EDIInterchangeModificationForm(new BusinessObjectFactory().Load<EDIInterchange>(Interchange.PK));
				form.FormClosing += new FormClosingEventHandler(form_FormClosing);
				ZFormModaliser.Show(form, this);
			}
		}

		void form_FormClosing(object sender, FormClosingEventArgs e)
		{
			Interchange.Refresh();
			UpdateSaveToDiskUI();
			var form = sender as EDIInterchangeModificationForm;
			if (form != null)
			{
				form.FormClosing -= new FormClosingEventHandler(form_FormClosing);
			}
		}

		bool IsInterchangeModificationAllowed
		{
			get
			{
				bool result = false;
				if (Interchange != null && Interchange.IsTransmitInterchange && Interchange.EI_Status == EDIInterchange.Status.Queued)
				{
					var group = BusinessEntity.Factory.Load<GlbGroup>(SystemDataRegistry.Instance.MessageModificationBeforeSendingAuthorisationGroup.Value);
					result = GlbStaff.CurrentUser.GS_IsDeveloper || (SystemDataRegistry.Instance.AllowMessageModificationBeforeSending.Value && (group != null && group.Staff.Contains(GlbStaff.CurrentUser.PK)));
				}
				return result;
			}
		}

		protected EDIInterchange Interchange
		{
			get { return BusinessEntity as EDIInterchange; }
		}

		public override string FormCaption
		{
			get { return Res.GetString("560590dd-a00d-47ce-94e2-4a20dd3b5abb", "Interchange : {0}", Interchange.EI_InterchangeNum); }
		}

		protected override bool SupportsEDocs
		{
			get { return false; }
		}

		protected EDIInterchangeEDIMessageCollection ContainedMessages
		{
			get { return BusinessEntity as EDIInterchangeEDIMessageCollection; }
		}

		void GridEDIMessages_DoubleClick(object sender, EventArgs e)
		{
			if (EDIMessagesGrid.SelectedElements.Length > 0)
			{
				var message = (EDIMessage)EDIMessagesGrid.SelectedElements[0];
				OpenRelatedItem(message);
			}
		}

		void OpenRelatedItem(BusinessObject message)
		{
			if (message != null)
			{
				var controller = ZControllerFactory.Create(ControllerIDs.Messaging.EDIMessage);
				controller.ShowEditForm(message);
			}
		}

		#region SaveContentToDiskAction

		void UpdateSaveToDiskUI()
		{
			if (Interchange.SaveToDisk)
			{
				string sizeText = LargeMessageHelper.SizeInKb(LargeMessageHelper.DetailTextSizeLimit).ToString() + "KB";
				zLabelTruncateNotification.Text = Res.GetString("25d5d5b7-749f-4514-b702-c721d1d30e04", "Content is greater than {0}, and only the first {0} will be displayed.", sizeText);

				zLabelTruncateNotification.Visible = true;
			}
			else
			{
				zLabelTruncateNotification.Visible = false;
			}

			if (zLabelTruncateNotification.Visible)
			{
				zTextBoxInterchangeText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(ControlDpiScalingHelper.UnscaleFromCurrentDpiX(zTextBoxInterchangeText.Location.X), ControlDpiScalingHelper.UnscaleFromCurrentDpiY(zTextBoxInterchangeText.Location.Y) + 26);
			}

			if (addedMenuItemList.Count == 0)
			{
				addedMenuItemList.Add(MainMenu.MenuItems[2].MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("A9503557-2618-4727-BD93-D2830DF386FC", "Save To Disk"), new EventHandler(zButtonSaveContentToDisk_Click))));
				addedMenuItemList.Add(MainMenu.MenuItems[2].MenuItems.Add(new ZMenuItem(ResString.GetMultilingualString("25266730-85BC-4286-BD0A-C7FC365B798D", "Save Body To Disk"), new EventHandler(zButtonSaveBodyTextToDisk_Click))));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "FileName part no need to translate")]
		void zButtonSaveContentToDisk_Click(object sender, EventArgs e)
		{
			using (var fileDialog = new ZSaveFileDialog())
			{
				fileDialog.FileName = GetInterchangeFileName("Content");

				if (fileDialog.ShowDialog() == DialogResult.OK)
				{
					SaveContentToFile(fileDialog.OpenFile());
				}
			}
		}

		void zButtonSaveBodyTextToDisk_Click(object sender, EventArgs e)
		{
			using (var fileDialog = new ZSaveFileDialog())
			{
				fileDialog.FileName = GetInterchangeFileName("BodyText");

				if (fileDialog.ShowDialog() == DialogResult.OK)
				{
					SaveBodyTextToFile(fileDialog.OpenFile());
				}
			}
		}

		string GetInterchangeFileName(string suffix)
		{
			StringBuilder output = new StringBuilder();
			output.Append(Interchange.EI_ApplicationCode).Append("_");
			output.Append(Interchange.EI_ReceiveTransmit).Append("_");
			output.Append(Interchange.EI_InterchangeType).Append("_");
			output.Append(Interchange.EI_InterchangeNum).Append("_");
			output.Append(suffix);

			if (Interchange.EI_ApplicationCode == EDIInterchange.ApplicationCodes.XMS)
			{
				output.Append(".xml");
			}
			else
			{
				output.Append(".txt");
			}

			return output.ToString();
		}

		void SaveContentToFile(Stream fileStream)
		{
			using (var sw = new StreamWriter(fileStream))
			{
				sw.Write(Interchange.EI_HeaderText);

				using (var reader = Interchange.GetEI_BodyTextReader())
				{
					sw.AddStream(reader);
					sw.Flush();
				}

				sw.Write(Interchange.EI_FooterText);
			}
		}

		void SaveBodyTextToFile(Stream fileStream)
		{
			using (var sw = new StreamWriter(fileStream))
			{
				using (var reader = Interchange.GetEI_BodyTextReader())
				{
					sw.AddStream(reader);
					sw.Flush();
				}
			}
		}

		#endregion
	}
}
