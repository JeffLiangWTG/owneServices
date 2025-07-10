using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(PGAProgramRequirement))]
	sealed class PGAProgramRequirementTest : NonPersistentBusinessObjectTestCase
	{
		public void TestDeclareYes()
		{
			PGAProgramRequirement requirement = (PGAProgramRequirement)GetNewBusinessObject();

			requirement.DeclareYes = true;
			AssertRequirement(requirement, true, false, false, YesNoList.Codes.Yes);
		}

		public void TestDeclareYesWhenTariffFlagged()
		{
			CARefTariffTestHelper.AnIncludedCodeForTesting(Factory, PGACodes.Codes.ECCC, "6005902100", ECCCPGADepartmentCodes.Codes.WEN);
			invoiceLine.JI_Tariff = "6005902100";

			var pgaProvider = new PGARequirementProvider(invoiceLine);
			var pgaRequirement = new PGARequirement(Factory, PGACodes.Codes.ECCC, pgaProvider);

			var wenProgramRequirement = pgaRequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == ECCCPGADepartmentCodes.Codes.WEN);
			wenProgramRequirement.DeclareYes = true;

			var veeProgramRequirement = pgaRequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == ECCCPGADepartmentCodes.Codes.VEE);
			veeProgramRequirement.DeclareYes = true;

			AssertEquals(YesNoList.Codes.Yes, pgaProvider.GetIndicatorInfo(PGACodes.Codes.ECCC).Value);
			Assert(wenProgramRequirement.IsProgramRequired);
			Assert(!veeProgramRequirement.IsProgramRequired);

			wenProgramRequirement.DeclareYes = false;
			veeProgramRequirement.DeclareYes = false;

			AssertEquals(YesNoList.Codes.No, wenProgramRequirement.Indicator);
			AssertEquals(string.Empty, veeProgramRequirement.Indicator);
		}

		public void TestHasInvoiceLinesWithPGA()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = YesNoList.Codes.Yes;
			invoiceLine.CA_CNSCInd = YesNoList.Codes.Yes;
			invoiceLine.CA_GACInd = YesNoList.Codes.Yes;

			Assert(header.HasInvoiceLinesWithCFIAPGA);
			Assert(declaration.HasInvoiceLinesWithCFIAPGA);
			Assert(header.HasInvoiceLinesWithCNSCPGA);
			Assert(declaration.HasInvoiceLinesWithCNSCPGA);
			Assert(header.HasInvoiceLinesWithGACPGA);
			Assert(declaration.HasInvoiceLinesWithGACPGA);

			invoiceLine.PGARequirements.Cast<PGARequirement>().First(x => x.AgencyCode == PGACodes.Codes.CFIA).ProgramCodeRequirements[0].Indicator = YesNoList.Codes.No;
			invoiceLine.PGARequirements.Cast<PGARequirement>().First(x => x.AgencyCode == PGACodes.Codes.CNSC).ProgramCodeRequirements[0].Indicator = YesNoList.Codes.No;
			invoiceLine.PGARequirements.Cast<PGARequirement>().First(x => x.AgencyCode == PGACodes.Codes.GAC).ProgramCodeRequirements[0].Indicator = YesNoList.Codes.No;

			Assert(!header.HasInvoiceLinesWithCFIAPGA);
			Assert(!declaration.HasInvoiceLinesWithCFIAPGA);
			Assert(!header.HasInvoiceLinesWithCNSCPGA);
			Assert(!declaration.HasInvoiceLinesWithCNSCPGA);
			Assert(!header.HasInvoiceLinesWithGACPGA);
			Assert(!declaration.HasInvoiceLinesWithGACPGA);

			invoiceLine.PGARequirements.Cast<PGARequirement>().First(x => x.AgencyCode == PGACodes.Codes.CFIA).ProgramCodeRequirements[0].Indicator = YesNoList.Codes.Yes;
			invoiceLine.PGARequirements.Cast<PGARequirement>().First(x => x.AgencyCode == PGACodes.Codes.CNSC).ProgramCodeRequirements[0].Indicator = YesNoList.Codes.Yes;
			invoiceLine.PGARequirements.Cast<PGARequirement>().First(x => x.AgencyCode == PGACodes.Codes.GAC).ProgramCodeRequirements[0].Indicator = YesNoList.Codes.Yes;

			Assert(header.HasInvoiceLinesWithCFIAPGA);
			Assert(declaration.HasInvoiceLinesWithCFIAPGA);
			Assert(header.HasInvoiceLinesWithCNSCPGA);
			Assert(declaration.HasInvoiceLinesWithCNSCPGA);
			Assert(header.HasInvoiceLinesWithGACPGA);
			Assert(declaration.HasInvoiceLinesWithGACPGA);

			invoiceLine.Delete();
			Assert(!header.HasInvoiceLinesWithCFIAPGA);
			Assert(!declaration.HasInvoiceLinesWithCFIAPGA);
			Assert(!header.HasInvoiceLinesWithCNSCPGA);
			Assert(!declaration.HasInvoiceLinesWithCNSCPGA);
			Assert(!header.HasInvoiceLinesWithGACPGA);
			Assert(!declaration.HasInvoiceLinesWithGACPGA);
		}

		public void TestSetIndicatorWithSetterSuspender()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CCA_HCIndicator = "Y";
			var programProvider = new PGARequirementProvider(pivot);
			var requirement = new PGARequirement(Factory, "HC", programProvider);

			var programRequirement = requirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(x => x.ProgramCode == "API");
			var hc = programRequirement.ProgramRequirementProvider as HCPGAHeader;
			hc.SetterSuspender.SuspendSetting(HCPGAHeader.Schema.CA_APIProgramInd);
			programRequirement.Indicator = "Y";
			Assert(!YesNoList.IsYes(hc.CA_APIProgramInd));

			hc.SetterSuspender.ResumeSetting(HCPGAHeader.Schema.CA_APIProgramInd);
			programRequirement.Indicator = "Y";
			hc = programRequirement.ProgramRequirementProvider as HCPGAHeader;
			Assert(YesNoList.IsYes(hc.CA_APIProgramInd));
		}

		public void TestUpdatePGAIndicator()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			var header = declaration.Invoices.AddNew();
			var invoiceLine = (JobComInvoiceLine)header.InvoiceLines.AddNew();
			invoiceLine.PGARequirements.Cast<PGARequirement>().First(x => x.AgencyCode == PGACodes.Codes.HC).ProgramCodeRequirements[0].Indicator = YesNoList.Codes.No;
			Assert(YesNoList.IsNo(invoiceLine.CA_HCInd));

			var programCodeRequirement = invoiceLine.PGARequirements.Cast<PGARequirement>().First(x => x.AgencyCode == PGACodes.Codes.HC).ProgramCodeRequirements[1];

			programCodeRequirement.Indicator = YesNoList.Codes.Yes;
			Assert(YesNoList.IsYes(invoiceLine.CA_HCInd));
			programCodeRequirement.Indicator = YesNoList.Codes.No;
			Assert(YesNoList.IsNo(invoiceLine.CA_HCInd));

			programCodeRequirement.ShouldUpdateIndicatorEvent += (value, newValue, description) => false;

			programCodeRequirement.Indicator = YesNoList.Codes.Yes;
			Assert(YesNoList.IsYes(invoiceLine.CA_HCInd));
			var hcHeader = (HCPGAHeader)invoiceLine.PGARequirements.Cast<PGARequirement>().First(x => x.AgencyCode == PGACodes.Codes.HC).PGAHeader;
			hcHeader.CA_IntendedUseCodeBBC = "TEST";
			hcHeader.CA_CategoryBBC = "TEST";
			programCodeRequirement.Indicator = YesNoList.Codes.No;
			Assert(YesNoList.IsYes(invoiceLine.CA_HCInd));

			hcHeader.CA_IntendedUseCodeBBC = "";
			hcHeader.CA_CategoryBBC = "";
			programCodeRequirement.Indicator = YesNoList.Codes.No;
			Assert(YesNoList.IsNo(invoiceLine.CA_HCInd));
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var invoiceLineLoaded = factory2.Load<JobComInvoiceLine>(invoiceLine.PK);
			Assert(YesNoList.IsNo(invoiceLineLoaded.CA_HCInd));
		}
		public void TestDeclareNo()
		{
			PGAProgramRequirement requirement = (PGAProgramRequirement)GetNewBusinessObject();

			requirement.DeclareNo = true;
			AssertRequirement(requirement, false, true, false, YesNoList.Codes.No);
		}

		public void TestDeclareNotApplicable()
		{
			PGAProgramRequirement requirement = (PGAProgramRequirement)GetNewBusinessObject();

			requirement.DeclareYes = true;
			AssertRequirement(requirement, true, false, false, YesNoList.Codes.Yes);
			requirement.DeclareYes = false;
			AssertRequirement(requirement, false, false, true, string.Empty);
		}

		void AssertRequirement(PGAProgramRequirement requirement, ZBool declareYes, ZBool declareNo, ZBool declareNotApplicable, ZString indicator)
		{
			AssertEquals(declareYes, requirement.DeclareYes);
			AssertEquals(declareNo, requirement.DeclareNo);
			AssertEquals(declareNotApplicable, requirement.DeclareNotApplicable);
			AssertEquals(indicator, requirement.Indicator);
		}

		public void TestEnsureActivePGAHeaderIfNecessaryAndDeactivePGAHeaderWhenAllProgramsAreNA()
		{
			AssertEquals(string.Empty, pgaRequirement.Indicator);

			programRequirement.DeclareYes = true;
			AssertEquals(YesNoList.Codes.Yes, pgaRequirement.Indicator);

			programRequirement.DeclareYes = false;
			AssertEquals(string.Empty, pgaRequirement.Indicator);
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			pgaRequirement = (PGARequirement)invoiceLine.PGARequirements.FirstOrDefault();
			programRequirement = (PGAProgramRequirement)pgaRequirement.ProgramCodeRequirements.FirstOrDefault();
		}

		JobComInvoiceLine invoiceLine;
		PGAProgramRequirement programRequirement;
		PGARequirement pgaRequirement;

		protected override BusinessObject GetNewBusinessObject()
		{
			return programRequirement;
		}

		#endregion
	}
}
