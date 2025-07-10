using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.DE.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.GUI
{
	public partial class ExportStatusRequestForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a status Request, this constructor is just for the designer", true)]
		public ExportStatusRequestForm()
		{
		}

		public ExportStatusRequestForm(StatusRequest statusRequest)
			: base(statusRequest)
		{
			PlugIns.Add(ControllerIDs.eDocsPlugIn);
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			SetControlVisibilityForViewMode();
		}

		public override string FormCaption => Res.GetString("06A84034-5AAE-4105-843A-3DCD0F349186", "Status Request");

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (statusRequest != null)
			{
				statusRequest.ModuleInfo.ValueChanged -= ModuleInfoOnValueChanged;
			}

			base.SetDataBinding(dataSource, dataMember);

			statusRequest = dataSource as StatusRequest;

			if (statusRequest != null)
			{
				statusRequest.ModuleInfo.ValueChanged += ModuleInfoOnValueChanged;
			}
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			//Don't want to trigger handleSaveButton, because we will save in the Send Button.
		}

		protected override void Dispose(bool disposing)
		{
			if (statusRequest != null)
			{
				statusRequest.ModuleInfo.ValueChanged -= ModuleInfoOnValueChanged;
			}

			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		void ModuleInfoOnValueChanged(object sender, EventArgs e)
		{
			if (statusRequest != null)
			{
				roleDropEdit.DescriptionBox.Text = statusRequest.Lookups.RoleList.GetDescriptionFromCode(roleDropEdit.CodeBox.Text);
			}
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			if (CheckIsOKToSend())
			{
				try
				{
					StatusRequestSender.New(statusRequest).Send();
					statusRequest.Factory.Save();
					Globals.Message.Show(MessagingMenuExtension.MessageHasBeenSent);
				}
				catch (ZSaveException ex)
				{
					ZExceptionReporting.HandleSaveException(ex);
				}
				Close();
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Confirmation String")]
		bool CheckIsOKToSend()
		{
			var result = false;
			statusRequest.RunPreSaveValidation();
			if (!statusRequest.HasErrors)
			{
				ZString messageErrors = new ZNotificationCollector(statusRequest, false, false, ZNotificationCollector.PropertyDescriptionType.HumanReadableName).ToMessageListString();
				var warningForSuperUser = Res.GetString("adc5e52a-b294-4932-9ac7-bdf74a858e57", "It is likely that your message(s) will be rejected as they have the following message errors:")
										+ System.Environment.NewLine + System.Environment.NewLine
										+ messageErrors + System.Environment.NewLine
										+ Res.GetString("58a6cd4e-f253-49de-b84d-be91ff4718fc", "Do you want to send the message(s) despite these message errors?");

				result = messageErrors.IsEmpty || Globals.Message.ShowConfirmation(warningForSuperUser, Res.GetString("1a434d2f-369b-48e2-b763-d4b2ea0020eb", "Send Message"), "Yes", MessageBoxIcon.Question) == DialogResult.OK;
			}
			return result;
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		void SetControlVisibilityForViewMode()
		{
			if (DisplayMode == ODisplayMode.ReadOnly)
			{
				SendButton.Visible = false;
				CancelButton2.GetExtension<ILabelCaptionRenderer>().Caption = ResString.GetMultilingualString("6D744CFA-1806-4CF7-9939-5FDEBF20703C", "Close");
				identificationFindBox.Visible = false;
				roleDropEdit.Visible = false;
			}
		}

		StatusRequest statusRequest;
	}
}
