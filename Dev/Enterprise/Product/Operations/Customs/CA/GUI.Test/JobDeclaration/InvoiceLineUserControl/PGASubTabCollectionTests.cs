using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using ClassificationTypeList = Enterprise.Customs.CA.Business.ClassificationTypeList;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.GUI.Testing
{
	sealed class PGASubTabCollectionTests : TestCaseWithFactory
	{
		public void TestUpdateForInvoiceLine()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var pgaRequirement = invoiceLine.PGARequirements.Cast<PGARequirement>().FirstOrDefault(x => x.AgencyCode == PGACodes.Codes.HC);

			TestUpdate(pgaRequirement);
		}

		public void TestUpdateForCusClassPartPivot()
		{
			CusClassPartPivot pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;

			var pgaRequirement = pivot.PGARequirements.Cast<PGARequirement>().FirstOrDefault(x => x.AgencyCode == PGACodes.Codes.HC);

			TestUpdate(pgaRequirement);
		}

		public void TestPGARequirementsAreDisposedInPGASubTab()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var pgaRequirement = invoiceLine.PGARequirements.Cast<PGARequirement>().FirstOrDefault(x => x.AgencyCode == PGACodes.Codes.HC);
			var requirements = pgaRequirement.ProgramCodeRequirements;
			using (var subTabControl = new ZTabControl())
			{
				var programCodes = new HCPGADepartmentCodes();
				var subTabCollection = new PGASubTabCollectionTest(subTabControl, null, PGACodes.Codes.HC, programCodes, o => new ZUserControl());
				subTabCollection.Update(pgaRequirement.PGAHeader);
				foreach (string programCode in programCodes.GetAllCodes())
				{
					var requirement = requirements.Cast<PGAProgramRequirement>().Single(r => r.ProgramCode == programCode);
					requirement.Indicator = "Y";
					subTabCollection.Update(pgaRequirement.PGAHeader);
				}
				AssertNotNull("Should have programRequirements", subTabCollection.PGAProgramRequirementsExposed);
				subTabCollection.Dispose();
				AssertNull("programRequirements disposed.", subTabCollection.PGAProgramRequirementsExposed);
			}
		}

		public void TestDatasourceIsUpdated()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "TC", "4011200022", "TPR");
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "TC", "4011100011", "TPR");
			var part = GetPart();
			importPivot.CI_TariffNum = "4011200022";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_OH_Supplier = supplier.PK;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			Factory.Save();

			invoiceLine.JI_Tariff = "4011100011";
			var tcPGA = invoiceLine.TCPGAHeader;
			tcPGA.CA_ProductClass = "TC01";

			using (var tabControl = new ZTabControl())
			{
				var pgaTabCollection = new PGATabCollection(tabControl, null, true);
				var pgaRequirements = invoiceLine.PGARequirements;
				pgaTabCollection.Update(pgaRequirements);

				var pgaRequirement = pgaRequirements.Cast<PGARequirement>().Single(r => r.AgencyCode == PGACodes.Codes.TC);

				AssertEquals("Y", pgaRequirement.ProgramCodeRequirements[0].Indicator);

				AssertEquals(1, tabControl.TabPages.Count);
				AssertEquals(1, tabControl.TabPages[0].Controls.Count);

				var tcUserControl = (TCUserControl)tabControl.TabPages[0].Controls[0];
				var tcTPRUserControl = tcUserControl.Controls[0].Controls[0].Controls[0] as TCTPRUserControl;
				AssertEquals(tcPGA, tcTPRUserControl.CurrentDataItem);

				invoiceLine.JI_PartNo = part.OP_PartNum;

				AssertEquals("4011200022", invoiceLine.JI_Tariff);

				AssertEquals(1, tabControl.TabPages.Count);
				AssertEquals(1, tabControl.TabPages[0].Controls.Count);

				tcUserControl = (TCUserControl)tabControl.TabPages[0].Controls[0];
				tcTPRUserControl = tcUserControl.Controls[0].Controls[0].Controls[0] as TCTPRUserControl;

				var dataSource = tcTPRUserControl.CurrentDataItem as TCPGAHeader;

				AssertEquals("Data Source of TCTPRUserControl is updated.", invoiceLine.TCPGAHeader, dataSource);
				Assert("Data Source of TCTPRUserControl is updated.", !dataSource.IsDeleted);
				AssertEquals("Data Source of TCTPRUserControl is updated.", tcUserControl.CurrentDataItem, dataSource);
			}
		}

		OrgHeader importer;
		OrgHeader supplier;
		CusClassPartPivot importPivot;

		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		void TestUpdate(PGARequirement pgaRequirement)
		{
			var requirements = pgaRequirement.ProgramCodeRequirements;
			using (var subTabControl = new ZTabControl())
			{
				var codelist = new TCPGADepartmentCodes();
				var subTabCollection = new PGASubTabCollection(subTabControl, null, PGACodes.Codes.HC, codelist, o => new ZUserControl());
				subTabCollection.Update(pgaRequirement.PGAHeader);
				AssertEquals(0, subTabControl.TabPages.Count);

				var programCodes = new HCPGADepartmentCodes();
				subTabCollection = new PGASubTabCollection(subTabControl, null, PGACodes.Codes.HC, programCodes, o => new ZUserControl());
				subTabCollection.Update(pgaRequirement.PGAHeader);
				foreach (string programCode in programCodes.GetAllCodes())
				{
					var requirement = requirements.Cast<PGAProgramRequirement>().Single(r => r.ProgramCode == programCode);
					requirement.Indicator = "Y";
					subTabCollection.Update(pgaRequirement.PGAHeader);
					AssertEquals(programCode, 1, subTabControl.TabPages.Count);
					AssertEquals(programCode, programCodes.GetDescriptionFromCode(programCode), subTabControl.TabPages[0].Text);

					requirement.Indicator = "N";
					subTabCollection.Update(pgaRequirement.PGAHeader);
					AssertEquals(programCode, 0, subTabControl.TabPages.Count);
				}
			}
		}

		Business.OrgSupplierPart GetPart()
		{
			importer = Factory.New<OrgHeader>();
			importer.FillWithValidTestData();
			supplier = Factory.New<OrgHeader>();
			supplier.FillWithValidTestData();

			var part = Factory.New<Business.OrgSupplierPart>();
			part.OP_PartNum = "APART";
			part.OP_Desc = "A typical part";
			part.OP_StockKeepingUnit = "KG";
			part.RelatedOrganisations.AddOwner(importer);
			part.RelatedOrganisations.AddSupplier(supplier);

			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = BaseCusClassification.ClassificationType.IMP;
			importPivot = part.PivotsForBinding.AddNew();
			importPivot.CI_ChildType = Business.ClassificationTypeList.Codes.HTI;
			importPivot.CI_CC = classification.PK;
			Factory.Save();

			return part;
		}

		sealed class PGASubTabCollectionTest : PGASubTabCollection
		{
			public PGASubTabCollectionTest(ZTabControl tabControl, ZGrid relatedGrid, string agencyCode, CodeDescriptionPairList programCodeList, Func<string, ZUserControl> createSubTabUserControl) : base(tabControl, relatedGrid, agencyCode, programCodeList, createSubTabUserControl)
			{
			}

			internal PGAProgramRequirementCollection PGAProgramRequirementsExposed => pgaProgramRequirements;
		}
	}
}
