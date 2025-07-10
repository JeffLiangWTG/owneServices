using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.CA.Registry;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusStatementLineCharge))]
	sealed class CusStatementLineChargeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestChargeTypeDescription()
		{
			statementHeader.B2_StatementNumber = "DN-123456789-1";
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, false))
			{
				statementLineCharge.B4_ChargeType = string.Empty;
				AssertEquals(string.Empty, statementLineCharge.ChargeTypeDescription);

				statementLineCharge.B4_ChargeType = "XXX";
				AssertEquals("XXX", statementLineCharge.ChargeTypeDescription);

				statementLineCharge.B4_ChargeType = "DTY";
				AssertEquals(EntryChargeTypeList.Descriptions.TotalDutyAmount, statementLineCharge.ChargeTypeDescription);
			}

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				statementLineCharge.B4_ChargeType = string.Empty;
				AssertEquals(string.Empty, statementLineCharge.ChargeTypeDescription);

				statementLineCharge.B4_ChargeType = "XXX";
				AssertEquals("XXX", statementLineCharge.ChargeTypeDescription);

				statementLineCharge.B4_ChargeType = "DTY";
				AssertEquals(CARMDailyNoticeChargeTypeList.Descriptions.Duties, statementLineCharge.ChargeTypeDescription);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return statementLineCharge;
		}

		protected override void SetUp()
		{
			base.SetUp();
			statementHeader = Factory.New<CusStatementHeader>();
			statementLine = statementHeader.StatementLines.AddNew();
			statementLineCharge = statementLine.Charges.AddNew();
		}

		CusStatementHeader statementHeader;
		CusStatementLine statementLine;
		CusStatementLineCharge statementLineCharge;
	}
}
