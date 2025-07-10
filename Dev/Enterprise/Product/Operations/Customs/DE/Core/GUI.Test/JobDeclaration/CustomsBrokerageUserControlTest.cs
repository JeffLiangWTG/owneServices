using System;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.EU.GUI.Testing;
using Enterprise.Customs.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.DE.GUI.Testing
{
	[TestedType(typeof(CustomsBrokerageUserControl))]
	class CustomsBrokerageUserControlTest : CustomsBrokerageUserControlAbstractTest<CustomsBrokerageUserControl>
	{
		public void TestInvoiceLinesUserControl_WarehouseAdjustment()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			control.MainTabControl.SelectedTab = control.InvoiceLinesTabPage;
			AssertType<WarehouseAdjustmentInvoiceLineUserControl>(control.InvoiceLinesUserControl);
		}

		public void TestPackingUserControl_Export()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Export;
			control.MainTabControl.SelectedTab = control.PackingTabPage;
			AssertType<ExportCustomsPackingUserControl>(control.Packing);
		}

		public void TestPackingUserControl_Import()
		{
			declaration.JE_MessageType = DEJobMessageTypeList.Codes.Import;
			control.MainTabControl.SelectedTab = control.PackingTabPage;
			AssertType<BaseCustomsPackingUserControl>(control.Packing);
		}

		protected override Type JobDeclarationUserControlType => typeof(JobDeclarationUserControl);

		protected override Type ImportSupplierHeaderUserControlType => typeof(ImportSupplierHeaderUserControl);

		protected override Type ExportSupplierHeaderUserControlType => typeof(ExportSupplierHeaderUserControl);

		protected override Type ImportInvoiceLineUserControlType => typeof(ImportInvoiceLineUserControl);

		protected override Type ExportInvoiceLineUserControlType => typeof(ExportInvoiceLineUserControl);

		protected override Type MiscOptionsUserControlType => typeof(MiscOptionsUserControl);

		protected override Type MessageUserControlType => typeof(EntryMessageUserControl);

		protected override Type EntryInstructionDetailsUserControlType => typeof(EntryInstructionDetailsUserControl);
	}
}
