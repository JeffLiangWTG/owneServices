using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.FR.Business.CusTempStorage;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.CusTempStorage
{
	public partial class TempStorageRegisterForm : ZTemplateForm
	{
		public TempStorageRegisterForm()
		{
			InitializeComponent();
		}

		public TempStorageRegisterForm(CusTempStorageRegHeader header)
			: base(header)
		{
			ToggleReOpenRegisterActionMenuItem(RegHeader);
			header.SRH_StatusInfo.ValueChanged += SRH_Status_ValueChanged;
		}

		void ToggleReOpenRegisterActionMenuItem(CusTempStorageRegHeader header)
		{
			var menuItems = ActionsMenuItem.MenuItems;
			var reOpenMenuItem = menuItems.Find("ReOpenMenuItem", true).FirstOrDefault();
			var isDeclarationClosed = header.IsDeclarationClosed;
			if (reOpenMenuItem == null && isDeclarationClosed)
			{
				reOpenMenuItem = new ZMenuItem(ResString.GetMultilingualString("e6beb631-98eb-47d8-aa35-9d170a895a81", "Re-open for manual adjustment"), ReOpenRegisterActionMenuItemClick);
				reOpenMenuItem.Name = "ReOpenMenuItem";
				menuItems.Add(reOpenMenuItem);
			}
			else if (reOpenMenuItem != null && !isDeclarationClosed)
			{
				menuItems.Remove(reOpenMenuItem);
			}
		}

		void SRH_Status_ValueChanged(object sender, EventArgs e)
		{
			ToggleReOpenRegisterActionMenuItem((CusTempStorageRegHeader)sender);
		}

		void ReOpenRegisterActionMenuItemClick(object sender, EventArgs e)
		{
			if (Env.Security.FRTempStorageRegisterReOpening.IsAllowed)
			{
				if (Globals.Message.ShowConfirmation(ResString.GetMultilingualString("1052ba02-bfe0-4832-8db6-77ae4bc2dc16", "Are you sure to re-open this Temp. Reg. Header?"), FormCaption, "YES", MessageBoxIcon.Question, MessageBoxButtons.OKCancel) == DialogResult.OK)
				{
					RegHeader.ReOpen();
				}
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("5b9e000b-5244-4655-af3c-4e6565d418cb", "You don't have the proper security right to re-open this Temp. Storage Register Header."));
			}
		}

		public override string FormCaption
		{
			get
			{
				var caption = Res.GetString("298CC13C-EA58-40BE-94B3-35C8B76F19D8", "Temp. Storage Register");
				var ddtNumber = RegHeader?.SRH_Reference;
				var jobReference = RegHeader?.SRH_InternalReference;
				if (!string.IsNullOrEmpty(ddtNumber))
				{
					caption += $" - {ddtNumber}/{jobReference}";
				}
				return caption;
			}
		}

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();

			if (result == ContinueWithSave.Yes && RegHeader.CusTempStorageRegLines.SelectMany(x => x.CusTempStorageRegLineTransactions).Any(t => !t.IsInDatabase))
			{
				var message = Res.GetString("A54D2E03-9918-4A88-AA3D-B819BB2B0A9C", "Transactions cannot be amended once saved. Do you want to continue saving the transactions?");
				var caption = Res.GetString("7CDB2C88-6FF7-4F23-B02E-943362FEE419", "Warning: Transactions cannot be amended");
				var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

				result = dialogResult == DialogResult.Yes ? ContinueWithSave.Yes : ContinueWithSave.No;
			}

			return result;
		}

		protected override bool SupportsEDocs => false;

		CusTempStorageRegHeader RegHeader => (CusTempStorageRegHeader)BusinessEntity;

		protected override bool AllowNew => false;
	}
}
