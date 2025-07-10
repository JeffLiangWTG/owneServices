using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module.Testing
{
	sealed class B2AdjustmentsFilterStripControlTest : ZFilterStripControlTest
	{
		public void TestFilterGridColorContextKey()
		{
			var decs = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			using (var module = new B2AdjustmentsModule())
			using (var filterControl = new B2AdjustmentsFilterStripControl(decs, module.FilterBusinessObject))
			{
				filterControl.OnLoad_Exposed();
				AssertEquals(ModuleIDs.Customs.CA.B2Adjustments.Name, filterControl.FilteredGrid.ColorContextKey);
			}
		}

		public void TestFilteredGridFields()
		{
			var decs = new JobDeclarationCollection(Factory, GlbCompany.CurrentCompany.PK);
			var filterBO = new JobDeclarationFilterBusinessObject();

			using (var filterControl = new B2AdjustmentsFilterStripControl(decs, filterBO))
			{
				filterControl.Show();
				AssertNotNull("Importer", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_OH_Importer));
				AssertNotNull("Transaction #", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.DeclarationNumber));
				AssertNotNull("B2 Type", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_B2Type));
				AssertNotNull("Original Transaction #", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_OriginalTransactionNo));
				AssertNotNull("Branch", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_GB));
				AssertNotNull("Broker", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_GS_NKCusAgent));
				AssertNotNull("Port Of Clearance", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_CustomsOffice));
				AssertNotNull("Accounting Date", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_K84AccountingDate));
				AssertNotNull("Release Date", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.JE_EntryAuthorisationDate));
				AssertNotNull("Date Submitted", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_B2SubmissionDate));
				AssertNotNull("Date Confirmed", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_ConfirmedDate));
				AssertNotNull("Date Accepted", filterControl.FilteredGrid.GetColumnStyle(JobDeclaration.Schema.CA_B2AcceptedDate));
				AssertNotNull("Amount Due Importer", filterControl.FilteredGrid.GetColumnStyle("AmountDueImporter"));
				AssertNotNull("Amount Due CBSA", filterControl.FilteredGrid.GetColumnStyle("AmountDueCBSA"));
			}
		}
	}
}
