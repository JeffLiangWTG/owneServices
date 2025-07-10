using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.AFR.GUI
{
	public partial class JPAFRBLLFunctionForm : ZChildForm
	{
		public JPAFRBLLFunctionForm()
		{
		}

		public JPAFRBLLFunctionForm(BLLFunction bllFunction)
			: base(bllFunction)
		{
			functionCode = BusinessEntity.FunctionCode;
			BusinessEntity.SelectedBills.CountChanged += SelectedBillNumbers_CountChanged;
			BusinessEntity.AvailableBills.CountChanged += AvailableBillNumbers_CountChanged;
			billNumberDropEdit.CaptionResourceString = JPM_BillOfLadingNumberCaption;
			selectedGroupBox.CaptionResourceString = SelectedGridCaption;
		}

		void AvailableBillNumbers_CountChanged(object sender, CargoWise.EntityFramework.CollectionCountChangedEventArgs e)
		{
			addButton.Enabled = BusinessEntity.SelectEnabled;
		}

		void SelectedBillNumbers_CountChanged(object sender, CargoWise.EntityFramework.CollectionCountChangedEventArgs e)
		{
			addButton.Enabled = BusinessEntity.SelectEnabled;
			removeButton.Enabled = BusinessEntity.UnselectEnabled;
			SendButton.Enabled = BusinessEntity.SendEnabled;
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			addButton.DataBindings.RemoveBinding(IsEnabledForBindingString);
			removeButton.DataBindings.RemoveBinding(IsEnabledForBindingString);
			SendButton.DataBindings.RemoveBinding(IsEnabledForBindingString);

			if (dataSource != null)
			{
				addButton.DataBindings.Add(new KBinding(IsEnabledForBindingString, BindingSource.DataSource, "SelectEnabled"));
				removeButton.DataBindings.Add(new KBinding(IsEnabledForBindingString, BindingSource.DataSource, "UnselectEnabled"));
				SendButton.DataBindings.Add(new KBinding(IsEnabledForBindingString, BindingSource.DataSource, "SendEnabled"));
			}
		}

		void AddButton_Click(object sender, EventArgs e)
		{
			var bill = (BLLFunctionBill)availableGrid.GetCurrent();
			if (bill != null)
			{
				BusinessEntity.SelectBill(bill);
			}
		}

		void RemoveButton_Click(object sender, EventArgs e)
		{
			var bill = (BLLFunctionBill)selectedGrid.GetCurrent();
			if (bill != null)
			{
				BusinessEntity.UnselectBill(bill);
			}
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			var validation = JPAFRMessageSendingValidation.New(BusinessEntity, null);
			var notifications = validation.CheckBusinessObjectLevelValidation();

			if (notifications.ContainsError())
			{
				Globals.Message.ShowError(notifications.ErrorNotificationsAsString(), FormCaption);
			}
			else if (!notifications.ContainsWarning() || Globals.Message.Show(notifications.NotificationsAsString(), FormCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				BusinessEntity.Factory.Save();
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		ResourceStringData JPM_BillOfLadingNumberCaption =>
			functionCode != BLLFunctionCode.RegisterMerge && functionCode != BLLFunctionCode.CancelMerge
			? Res.GetData("BD049D91-E65F-4921-9321-98EA3AC58277", "Original Bill No.")
			: Res.GetData("A9AE73DE-8A97-455C-8A25-8570AB9FC5B9", "New Bill No.");

		ResourceStringData SelectedGridCaption
		{
			get
			{
				ResourceStringData result;
				switch (functionCode)
				{
					case BLLFunctionCode.RegisterSplit:
					case BLLFunctionCode.CancelSplit:
						result = Res.GetData("07351526-57AD-48CD-AC4E-BF8F21EAA0E0", "Split Bills");
						break;
					case BLLFunctionCode.RegisterSwitch:
					case BLLFunctionCode.CancelSwitch:
						result = Res.GetData("D67A8418-B3E9-4A7E-B026-C0ACC66A68AC", "New Bill");
						break;
					case BLLFunctionCode.RegisterMerge:
					case BLLFunctionCode.CancelMerge:
						result = Res.GetData("D1610C8B-F6E2-4A1E-A106-657A88B2E867", "Merge Bills");
						break;
					default:
						result = ResourceStringData.Empty;
						break;
				}
				return result;
			}
		}

		public override string FormCaption
		{
			get
			{
				string result = ResString.GetMultilingualString("3FE75F9B-77C8-4076-97F7-4DE3F03163AC", "Bill Manipulation (BLL) - ");
				switch (functionCode)
				{
					case BLLFunctionCode.RegisterSplit:
						result += ResString.GetMultilingualString("D038BEDF-CD2D-469A-968A-F70D230C55F4", "Register Split");
						break;
					case BLLFunctionCode.RegisterSwitch:
						result += ResString.GetMultilingualString("F9A9590A-206F-4545-90E6-A45B2519D728", "Register Switch");
						break;
					case BLLFunctionCode.RegisterMerge:
						result += ResString.GetMultilingualString("47872253-CE74-47F2-93CA-8B47754B3CC5", "Register Merge");
						break;
					case BLLFunctionCode.CancelSplit:
						result += ResString.GetMultilingualString("BB63A58D-6D10-4E99-ACF2-9EC082E862BA", "Cancel Split");
						break;
					case BLLFunctionCode.CancelSwitch:
						result += ResString.GetMultilingualString("34380382-C8CD-4989-B19F-1275C76AC242", "Cancel Switch");
						break;
					case BLLFunctionCode.CancelMerge:
						result += ResString.GetMultilingualString("682D547B-A5F3-4903-B1E2-B52FA3067112", "Cancel Merge");
						break;
					default:
						result = ResString.GetMultilingualString("B52F316E-F7C1-43F7-9E0E-A77C2643AD6A", "Bill Manipulation (BLL)");
						break;
				}
				return result;
			}
		}

		public override string FormVerb => "";

		public new BLLFunction BusinessEntity => (BLLFunction)base.BusinessEntity;

		readonly BLLFunctionCode functionCode;

		const string IsEnabledForBindingString = "IsEnabledForBinding";
	}
}
