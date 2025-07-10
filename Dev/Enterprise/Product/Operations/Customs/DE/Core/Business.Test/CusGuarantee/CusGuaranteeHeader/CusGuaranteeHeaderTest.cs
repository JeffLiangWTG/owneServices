using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CusGuaranteeHeader))]
	public class CusGuaranteeHeaderTest : EU.Business.Testing.CusGuaranteeHeaderAbstractTest
	{
		public void TestValidation()
		{
			AssertType<CusGuaranteeHeaderValidation>(guaranteeHeader.Validation);
		}

		public void TestCPH_UnitOfMeasure()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Default value", GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency, guaranteeHeader.CPH_UnitOfMeasure);

				guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
				AssertEquals("Set to EUR", Core.Constants.CurrencyCodes.EuropeanUnion, guaranteeHeader.CPH_UnitOfMeasure);
			});
		}

		public void TestSubTypeReadOnly()
		{
			CombineAssertions(() =>
			{
				GlbStaff.CurrentUser.GS_IsController = false;
				AssertEquals("SubType is read only when has no sub type list", true, guaranteeHeader.CPH_SubType_ReadOnly);
				guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
				AssertEquals("SubType isn't read only when has sub type list", false, guaranteeHeader.CPH_SubType_ReadOnly);
			});
		}

		public void TestIsTRAGuaranteeType()
		{
			CombineAssertions(() =>
			{
				guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
				AssertEquals("CPH_Type is TRA", ZBool.True, guaranteeHeader.IsTRAGuaranteeType);

				guaranteeHeader.CPH_Type = "COM";
				AssertEquals("CPH_Type isn't TRA", ZBool.False, guaranteeHeader.IsTRAGuaranteeType);
			});
		}

		public void TestCusGuaranteeRulesType()
		{
			AssertType(typeof(CusGuaranteeRuleCollection<CusGuaranteeRule>), guaranteeHeader.CusGuaranteeRules);
		}

		public void TestAdditionalAccessCodesType()
		{
			AssertType(typeof(CusGuaranteeRuleCollection<CusGuaranteeRule>), guaranteeHeader.AdditionalAccessCodes);
		}

		protected override void SetUp()
		{
			base.SetUp();
			guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		}
		CusGuaranteeHeader guaranteeHeader;
	}
}
