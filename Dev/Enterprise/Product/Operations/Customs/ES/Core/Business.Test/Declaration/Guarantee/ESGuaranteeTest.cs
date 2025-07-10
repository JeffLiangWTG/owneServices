using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(ESGuarantee))]
	class ESGuaranteeTest : EU.Business.Declaration.Testing.GuaranteeForDeclarationTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			return declaration.Guarantees.AddNew();
		}

		public new void TestLookups()
		{
			var guarantee = (ESGuarantee)GetNewBusinessObject();
			AssertType<ESGuaranteeLookups>(guarantee.Lookups);
		}

		public void TestPW_BondFiledPortDefault()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("ES"))
			{
				var guaranteHeader1 = GuaranteesTestHelper.CreateGuaranteesHeaderDetailWithRuleCUS(Factory, ZGuid.Empty, "Gua1", "Office1");
				var guaranteHeader2 = GuaranteesTestHelper.CreateGuaranteesHeaderDetailWithRuleCUS(Factory, ZGuid.Empty, "Gua2", "Office3");
				GuaranteesTestHelper.CreateGuaranteesRuleDetailWithRuleCUS(guaranteHeader2, "Office4");

				JobDeclaration declaration = base.Factory.New<JobDeclaration>();
				declaration.JE_MessageType = MessageTypeList.Codes.Import;

				var entryInstructionH2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstructionH2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstructionH2.CEI_Style = IMPDeclarationTypeList.Codes.H2;

				var entryInstructionNoH2 = declaration.CustomsEntryInstructions.AddNew();
				entryInstructionNoH2.CEI_SubStyle = EntrySubStyleList.Codes.A;
				entryInstructionNoH2.CEI_Style = ZString.Empty;

				CombineAssertions(() =>
				{
					ESGuarantee esGuarantee = declaration.Guarantees.AddNew();
					esGuarantee.PW_BondNumber = "Gua1";
					AssertEquals("With guarantee but not entry instruction", string.Empty, esGuarantee.PW_BondFiledPort);

					esGuarantee.EntryInstructionID = entryInstructionH2.PK;
					AssertEquals("With guarantee and with entry instruction H2", "Office1", esGuarantee.PW_BondFiledPort);

					esGuarantee.PW_BondFiledPort = ZString.Empty;
					esGuarantee.PW_BondNumber = ZString.Empty;
					AssertEquals("Without guarantee and with entry instruction H2", string.Empty, esGuarantee.PW_BondFiledPort);

					esGuarantee.PW_BondNumber = "Gua2";
					AssertEquals("With guarantee and with entry instruction H2 and one CUS", "Office3", esGuarantee.PW_BondFiledPort);

					esGuarantee.PW_BondFiledPort = ZString.Empty;
					esGuarantee.EntryInstructionID = entryInstructionNoH2.PK;
					AssertEquals("With guarantee and with entry instruction no H2", string.Empty, esGuarantee.PW_BondFiledPort);

					GuaranteesTestHelper.CreateGuaranteesRuleDetailWithRuleCUS(guaranteHeader2, "Office5");
					GuaranteesTestHelper.CreateGuaranteesRuleDetailWithRuleCUS(guaranteHeader2, "Office6");
					esGuarantee.PW_BondNumber = "Gua2";
					esGuarantee.EntryInstructionID = entryInstructionH2.PK;
					AssertEquals("With guarantee and with entry instruction H2 and the guarantee exist more once", "Office3", esGuarantee.PW_BondFiledPort);

					esGuarantee.PW_BondFiledPort = "AAAA";
					esGuarantee.PW_BondNumber = "Gua1";
					AssertEquals("With change of values if exist PW_BondFiledPort, the value is not changed", "AAAA", esGuarantee.PW_BondFiledPort);
				});
			}
		}
	}
}
