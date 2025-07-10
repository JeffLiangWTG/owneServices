using System;

namespace Enterprise.Customs.JP.GUI.Testing
{
	class CustomsBrokerageUserControlBaseOnlyTest : CustomsBrokerageUserControlAbstractTest<CustomsBrokerageUserControl>
	{
		protected override Type ImportSupplierHeaderUserControl => typeof(ImportSupplierHeaderUserControl);

		protected override Type ExportSupplierHeaderUserControl => typeof(ExportSupplierHeaderUserControl);

		protected override Type ImportInvoiceLineUserControl => typeof(ImportInvoiceLineUserControl);

		protected override Type ExportInvoiceLineUserControl => typeof(ExportInvoiceLineUserControl);

		protected override Type MiscOptionsUserControlType => null;

		protected override Type MessageUserControlType => typeof(EntriesTabUserControl);

		protected override Type ContainerUserControlType => typeof(CustomsCusContainersWithTrackingAndAdditionalSealUserControl);

		public void TestEntryInstructionsTabVisibleForCountry()
		{
			AssertEquals(true, control.EntryInstructionsTabVisibleForCountry);
		}

		public void TestEntryInstructionUserControl()
		{
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			control.LoadEntryInstructionDetailsTabPage();
			AssertType<ImportEntryInstructionUserControl>("Import", control.CustomsEntryInstructionUserControl);
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Export;
			control.LoadEntryInstructionDetailsTabPage();
			AssertType<ExportEntryInstructionUserControl>("Export", control.CustomsEntryInstructionUserControl);
		}
	}
}
