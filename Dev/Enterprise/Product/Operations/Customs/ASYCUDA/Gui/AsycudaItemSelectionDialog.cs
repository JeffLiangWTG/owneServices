using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using ResString = Enterprise.Customs.ASYCUDA.Gui.ResString;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	public partial class AsycudaItemSelectionDialog : ZChildForm
	{
		[Obsolete("This constructor is just for the designer")]
		public AsycudaItemSelectionDialog()
		{
			InitializeComponent();
		}

		public AsycudaItemSelectionDialog(MessageChooser messageChooser, string itemsType)
			: base(messageChooser)
		{
			this.itemsType = itemsType;
			InitializeComponent();
			ItemsGroupBox.Text = ResString.GetMultilingualString("1FA47803-FE2A-4696-8967-AB216DDF0668", "Manifest - {0}", BusinessEntity.Header.ManifestNumber);
			Name = FormattableString.Invariant($"{itemsType}ToSendDialog");
			Text = IsBillsForDeclaration ? ResString.GetMultilingualString("605FDB70-CE91-4931-AECC-3C084A4C679A", "Bills to create Declarations") : ResString.GetMultilingualString("7235f472-77ad-4a42-a8f8-9248fb00453d", "{0} to Send", itemsType);
			ItemsGrid.SetColumnCaption("Description", Text);
		}

		public new MessageChooser BusinessEntity => (MessageChooser)base.BusinessEntity;

		public const string BillsForDeclarationItemType = "BillsForDeclaration";

		public bool IsBillsForDeclaration => itemsType == BillsForDeclarationItemType;

		public ISelectionItem[] GetSelectedItems() => BusinessEntity.GetSelectedItems();

		void CancelButton_Click(object sender, EventArgs e)
		{
			Close();
			DialogResult = DialogResult.Cancel;
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			var count = BusinessEntity.SelectedCount;
			if (count > 0)
			{
				if ((!IsBillsForDeclaration || count == 1 || Globals.Message.Show(ResString.GetMultilingualString("FBC43E3C-BF14-4375-9B9E-2DB7E286AC6A", "You have selected to create {0} Declarations. Would you like to proceed?", count), "Create Declarations", MessageBoxButtons.YesNo, DialogResult.No) == DialogResult.Yes) && IsValidToSend())
				{
					OnSent();
					var header = BusinessEntity.Header;
					if (header.IsBillLockEnabled)
					{
						header.RefreshBillLock();
					}
					DialogResult = DialogResult.OK;

					Close();
				}
			}
			else
			{
				Globals.Message.ShowError(ResString.GetMultilingualString("{C1BF2A96-95F7-483E-9900-3FA99EE6F684}", "Cannot send as nothing has been selected."));
			}
		}

		protected virtual void OnSent()
		{
		}

		protected virtual bool IsValidToSend()
		{
			var result = true;
			var securityCheckPoint = Env.Security.GlobalManifestSendWithMessageErrors;

			if (!IsBillsForDeclaration && !securityCheckPoint.IsAllowed && HasMessageErrorsOnSelectedItems())
			{
				Globals.Message.ShowError(securityCheckPoint.ErrorMessageForNotAllowed);
				result = false;
			}

			return result;
		}

		protected virtual bool HasMessageErrorsOnSelectedItems()
		{
			return GetSelectedItems().Any(x => ((BusinessObject)x).HasMessageErrors);
		}

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			SelectAll();
		}

		public void SelectAll()
		{
			BusinessEntity.SelectAll();
		}

		void DeselectAllButton_Click(object sender, EventArgs e)
		{
			DeSelectAll();
		}

		public void DeSelectAll()
		{
			BusinessEntity.DeSelectAll();
		}

		readonly string itemsType;

		public ZString SeletedItem => BusinessEntity.SelectedDescription;

#if DEBUG
		public void SelectOnlyBillNodes_ForTestOnly(Func<ZGuid, bool> shouldCheck)
		{
			foreach (MessageChooserItem node in BusinessEntity.ChooserItems)
			{
				node.Checked = shouldCheck(node.BizO.PK);
			}
		}
#endif
	}
}
