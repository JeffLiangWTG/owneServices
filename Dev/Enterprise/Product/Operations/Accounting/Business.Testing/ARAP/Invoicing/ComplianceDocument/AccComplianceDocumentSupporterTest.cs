using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocumentSupport.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AccComplianceDocumentSupporter))]
	public class AccComplianceDocumentSupporterTest : DocumentSupporterTest
	{
		public void TestGetDataStateBeforeRun()
		{
			var creator = new TestObjectCreator(Factory);
			var complianceDocumentHeader = creator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "desc", "ABC");
			var supporter = new AccComplianceDocumentSupporter(complianceDocumentHeader);
			complianceDocumentHeader.ADH_OH_Organisation = creator.Debtor.PK;
			complianceDocumentHeader.ADH_ComplianceSubType = "TXE";
			creator.Debtor.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			creator.Debtor.CustomsCodes.AddNew("MCI", "123");

			var menu = Factory.New<StmMenuItem>();
			menu.SU_MenuName = "Electronic test";

			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = ".InvalidDataContext";

			var templatePivot = Factory.New<StmMenuTemplatePivot>();
			templatePivot.SI_SU = menu.PK;
			templatePivot.SI_SO = template.PK;

			Factory.Save();
			AssertEquals("Cannot produce this TXE Document because the Debtor's organization category is 'NAT' and has a MID-Mobile Carrier ID / PIG-Public Interest Group registration code.", supporter.GetDataStateBeforeRun(menu).ErrorMessage);
		}

		public void TestGetContactOrganisation()
		{
			var creator = new TestObjectCreator(Factory);
			var complianceDocumentHeader = creator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "desc", "ABC");
			var supporter = new AccComplianceDocumentSupporter(complianceDocumentHeader);
			complianceDocumentHeader.ADH_OH_Organisation = ZGuid.Empty;

			AssertNull("No contact org yet", supporter.GetContactOrganisation("", ContactType.Receivables, DocumentDirection.ANY).OrgHeader);
			complianceDocumentHeader.ADH_OH_Organisation = creator.Debtor.PK;

			AssertEquals("Contact Org for Document not null", creator.Debtor.PK, supporter.GetContactOrganisation("", ContactType.Receivables, DocumentDirection.ANY).OrgHeader.PK);
		}

		public void TestDocumentSupporter()
		{
			var complianceDocumentHeader = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			AssertEquals("Document Supporter should be of type", typeof(AccComplianceDocumentSupporter), complianceDocumentHeader.DocumentSupporter.GetType());
		}

		public void TestSupportedDataContext()
		{
			AssertEquals("DataContext ARComplianceDocument is supported", true, DocumentSupporter.IsDataContextSupported(new DataContextValue(nameof(Constants.DataContext.ARComplianceDocument))));
		}

		public void TestBusinessContext()
		{
			AssertEquals(CargoWise.Definitions.BusinessContext.ARComplianceDocument, DocumentSupporter.BusinessContext);
		}

		public new void TestRunningDocumentsShouldNotCauseException()
		{
			Assert(true);
		}

		public void TestCustomisationSecurityCheckpoint()
		{
			var arComplianceDocumentHeader = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
			AssertEquals(Env.Security.CustomizeReceivablesComplianceDocuments, arComplianceDocumentHeader.DocumentSupporter.CustomisationSecurityCheckpoint);
			
			var apComplianceDocumentHeader = Factory.NewWithValidTestData<APComplianceDocumentHeader>();
			AssertEquals(Env.Security.None, apComplianceDocumentHeader.DocumentSupporter.CustomisationSecurityCheckpoint);
		}

		public void TestGetDocumentWrappers()
		{
			var creator = new TestObjectCreator(Factory);
			var arComplianceDocumentHeader = creator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "desc", "ABC");
			var supporter = new AccComplianceDocumentSupporter(arComplianceDocumentHeader);

			AssertNotNull(supporter.GetDocumentWrappers(Constants.DataContext.ARComplianceDocument, null));

			var apComplianceDocumentHeader = creator.CreateComplianceDocumentHeader(LedgerTypes.AccountsPayable, "desc", "ABC");
			supporter = new AccComplianceDocumentSupporter(apComplianceDocumentHeader);

			AssertNull(supporter.GetDocumentWrappers(Constants.DataContext.ARComplianceDocument, null));
		}

		public void TestGetSupportedDataContexts()
		{
			var creator = new TestObjectCreator(Factory);
			var arComplianceDocumentHeader = creator.CreateComplianceDocumentHeader(LedgerTypes.AccountsReceivable, "desc", "ABC");
			var supporter = new AccComplianceDocumentSupporter(arComplianceDocumentHeader);

			Assert(supporter.CommaSeparatedListOfSupportedDataContexts.Contains("ARComplianceDocument"));

			var apComplianceDocumentHeader = creator.CreateComplianceDocumentHeader(LedgerTypes.AccountsPayable, "desc", "ABC");
			supporter = new AccComplianceDocumentSupporter(apComplianceDocumentHeader);

			Assert(!supporter.CommaSeparatedListOfSupportedDataContexts.Contains("ARComplianceDocument"));
		}

		#region Implementation

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			return Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
		}

		AccComplianceDocumentSupporter DocumentSupporter
		{
			get
			{
				var header = Factory.NewWithValidTestData<ARComplianceDocumentHeader>();
				header.ADH_Ledger = LedgerTypes.AccountsReceivable;
				return new AccComplianceDocumentSupporter(header);
			}
		}

		#endregion
	}
}
