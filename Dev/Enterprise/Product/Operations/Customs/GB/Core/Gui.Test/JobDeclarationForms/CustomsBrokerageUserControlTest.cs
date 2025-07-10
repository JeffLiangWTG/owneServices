using System;
using System.Linq;
using Enterprise.Customs.EU.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	[TestedType(typeof(CustomsBrokerageUserControl))]
	class CustomsBrokerageUserControlTest : CustomsBrokerageUserControlAbstractTest<CustomsBrokerageUserControl>
	{
		public void TestEntryInstructionsTabPage_CDS()
		{
			CombineAssertions(() =>
			{
				control.MainTabControl.SelectedTab = control.EntryInstructionDetailsTabPage;
				AssertEquals("EntryInstructionDetailsUserControl is contructed", true, control.EntryInstructionDetailsTabPage.Controls.OfType<EntryInstructionDetailsUserControl>().Any());
				AssertEquals("Tab relevant", true, control.EntryInstructionDetailsTabPage.TabRelevant);
				AssertEquals("Controls", 1, control.EntryInstructionDetailsTabPage.Controls.Count);
			});
		}

		protected override Type JobDeclarationUserControlType => typeof(JobDeclarationUserControl);

		protected override Type ImportSupplierHeaderUserControlType => typeof(GBImportSupplierHeaderUserControl);

		protected override Type ExportSupplierHeaderUserControlType => typeof(GBExportSupplierHeaderUserControl);

		protected override Type ImportInvoiceLineUserControlType => typeof(ImportInvoiceLineUserControl);

		protected override Type ExportInvoiceLineUserControlType => typeof(ExportInvoiceLineUserControl);

		protected override Type MiscOptionsUserControlType => typeof(MiscOptionsUserControl);

		protected override Type MessageUserControlType => typeof(MessageUserControl);

		protected override Type EntryInstructionDetailsUserControlType => typeof(EntryInstructionDetailsUserControl);

		protected override Type ContainersUserControlType => typeof(CusContainerUserControl);

		protected override void SetUp()
		{
			base.SetUp();
			declaration.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
		}
	}
}
