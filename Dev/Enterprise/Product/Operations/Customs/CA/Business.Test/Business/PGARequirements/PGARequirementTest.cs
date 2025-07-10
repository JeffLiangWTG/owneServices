using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using NUnit.Framework;
using static Enterprise.Customs.Business.BaseCusClassification;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(PGARequirement))]
	sealed class PGARequirementTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsEffective()
		{
			CusClassPartPivot pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			var collection1 = new PGARequirementCollection(new PGARequirementProvider(pivot));
			pivot.CCA_CFIAIndicator = YesNoList.Codes.Yes;
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			collection1.Populate();

			var pgaRequirement = collection1.PGARequirement(PGACodes.Codes.CFIA);
			AssertEquals("PGARequirement effective on CusClassPartPivot HTE", false, pgaRequirement.IsEffective);

			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			AssertEquals("PGARequirement effective on CusClassPartPivot HTI", true, pgaRequirement.IsEffective);

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_CFIAInd = "Y";
			var collection2 = new PGARequirementCollection(new PGARequirementProvider(invoiceLine));
			collection2.Populate();

			pgaRequirement = collection2.PGARequirement(PGACodes.Codes.CFIA);
			Assert("PGARequirement effective on IID declaration", pgaRequirement.IsEffective);

			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.PARS;

			invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.CA_DFOInd = "Y";
			collection2 = new PGARequirementCollection(new PGARequirementProvider(invoiceLine));
			collection2.Populate();

			pgaRequirement = collection2.PGARequirement(PGACodes.Codes.DFO);
			Assert("PGARequirement effective on non-IID declaration", !pgaRequirement.IsEffective);
		}

		public void TestSetDefaultValueForIndicatorWhenTariffChanged()
		{
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "TC", "3824700308", "TPR");
			CARefTariffTestHelper.CreateOrGetExistingTariff4Testing(Factory, "CNSC", "8531101011");
			declaration.CA_ServiceOption = ACROSSServiceOptions.Codes.IID;
			invoiceLine.JI_Tariff = "3824700308";
			AssertEquals("Y", invoiceLine.CA_TCInd);

			var tcPGAHeader = invoiceLine.TCPGAHeader;
			AssertNotNull(tcPGAHeader);
			AssertEquals("Y", tcPGAHeader.CA_TPRProgramInd);

			invoiceLine.JI_Tariff = "8531101011";
			AssertEquals("Y", invoiceLine.CA_CNSCInd);

			var pgaRequirement = invoiceLine.PGARequirements.Cast<PGARequirement>().FirstOrDefault(p => p.AgencyCode == PGACodes.Codes.CNSC);
			AssertNotNull(pgaRequirement);
			AssertEquals("ALL", pgaRequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault().ProgramCode);

			var cnscPGAHeader = invoiceLine.CNSCPGAHeader;
			AssertNotNull(cnscPGAHeader);
			AssertEquals("Y", cnscPGAHeader.CA_AllProgramInd);

			#region CusClassification
			var classification = Factory.New<CusClassification>();
			classification.CC_ClassificationType = ClassificationType.IMP;
			classification.CC_TariffNum = "3824700308";
			AssertEquals("Y", classification.CCA_TCIndicator);

			tcPGAHeader = classification.TCPGAHeader;
			AssertNotNull(tcPGAHeader);
			AssertEquals("Y", tcPGAHeader.CA_TPRProgramInd);

			classification.CC_TariffNum = "8531101011";
			AssertEquals("Y", classification.CCA_CNSCIndicator);

			pgaRequirement = classification.PGARequirements.Cast<PGARequirement>().FirstOrDefault(p => p.AgencyCode == PGACodes.Codes.CNSC);
			AssertNotNull(pgaRequirement);
			AssertEquals("ALL", pgaRequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault().ProgramCode);

			cnscPGAHeader = classification.CNSCPGAHeader;
			AssertNotNull(cnscPGAHeader);
			AssertEquals("Y", cnscPGAHeader.CA_AllProgramInd);
			#endregion
		}

		public void TestProgram()
		{
			var pgaRequirement = invoiceLine.PGARequirements.Cast<PGARequirement>().FirstOrDefault(p => p.AgencyCode == PGACodes.Codes.TC);
			AssertNotNull(pgaRequirement);

			var vprProgramRequirement = pgaRequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(p => p.ProgramCode == TCPGADepartmentCodes.Codes.VPR);
			var tprProgramRequirement = pgaRequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().FirstOrDefault(p => p.ProgramCode == TCPGADepartmentCodes.Codes.TPR);

			AssertNotNull(vprProgramRequirement);
			AssertNotNull(tprProgramRequirement);

			AssertEquals(string.Empty, pgaRequirement.Program);

			vprProgramRequirement.DeclareYes = true;
			AssertEquals(TCPGADepartmentCodes.Descriptions.VPR, pgaRequirement.Program);

			tprProgramRequirement.DeclareYes = true;
			AssertEquals("MULTIPLE", pgaRequirement.Program);
		}

		public void TestCopyPersistentValueForNoIndicator()
		{
			var pivot = Factory.NewWithValidTestData<CusClassPartPivot>();
			pivot.CCA_HCIndicator = YesNoList.Codes.No;
			pivot.HCPGAHeader.CA_BBCProgramInd = YesNoList.Codes.No;
			var hcPGARequirement = pivot.PGARequirements.PGARequirement(PGACodes.Codes.HC);

			Factory.Save();

			AssertEquals(YesNoList.Codes.No, hcPGARequirement.Indicator);
			AssertEquals(YesNoList.Codes.No, hcPGARequirement.ProgramCodeRequirements.Cast<PGAProgramRequirement>().First(x => x.ProgramCode == HCPGADepartmentCodes.Codes.BBC).Indicator);

			invoiceLine.PGARequirements.CopyPersistentValuesFrom(pivot.PGARequirements);
			AssertEquals(YesNoList.Codes.No, invoiceLine.PGARequirements.PGARequirement(PGACodes.Codes.HC).ProgramCodeRequirements.Cast<PGAProgramRequirement>().First(x => x.ProgramCode == HCPGADepartmentCodes.Codes.BBC).Indicator);
		}

		#region Setup

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoice = declaration.Invoices.AddNew();
			invoiceLine = invoice.JobComInvoiceLines.AddNew();
			pgaRequirement = (PGARequirement)invoiceLine.PGARequirements.FirstOrDefault();
		}

		JobDeclaration declaration;
		JobComInvoiceLine invoiceLine;
		PGARequirement pgaRequirement;

		protected override BusinessObject GetNewBusinessObject()
		{
			return pgaRequirement;
		}

		#endregion
	}
}
