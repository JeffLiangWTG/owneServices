using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.IL.Business.Testing
{
	public static class CusEntryHeaderTestHelper
	{
		public static Mock<T> CreateMoqEntryForFeesTest<T>(BusinessObjectFactory factory)
			where T : CusEntryHeader
		{
			var entryMock = factory.NewMoq<T>();
			entryMock.Protected().Setup<bool>("TaxFeePaymentCodeIsDeferredCore", ItExpr.IsAny<ZString>())
				.Returns((ZString cF_MethodOfPayment) =>
				{
					return cF_MethodOfPayment == "X" || cF_MethodOfPayment == "Y";
				});
			return entryMock;
		}

		public static T SetupEntryForFeesTest<T>(BusinessObjectFactory factory)
			where T : CusEntryHeader
		{
			var entryMock = CreateMoqEntryForFeesTest<T>(factory);
			var entry = entryMock.Object;
			var entryLine1 = entry.MergedLines.AddNew();
			var fees1duty = entryLine1.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
			fees1duty.CF_ChargeAmount = 200m;
			fees1duty.CF_MethodOfPayment = "X";

			var entryLine2 = entry.MergedLines.AddNew();
			var fees2duty = entryLine2.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts);
			fees2duty.CF_ChargeAmount = 300m;
			fees2duty.CF_MethodOfPayment = "Z";
			var fees2Vat = entryLine2.Fees.GetOrAddFeeByFeeType(Constants.EntryLineFee.VATFeeTypeCode);
			fees2Vat.CF_ChargeAmount = 100m;
			fees2Vat.CF_MethodOfPayment = "Z";

			var entryLine3 = entry.MergedLines.AddNew();
			var fees3Vat = entryLine3.Fees.GetOrAddFeeByFeeType(entry.TaxCode);
			fees3Vat.CF_ChargeAmount = 50m;
			fees3Vat.CF_MethodOfPayment = "X";

			var fees3Djc = entryLine3.Fees.GetOrAddFeeByFeeType("DJC");
			var fees3Lsc = entryLine3.Fees.GetOrAddFeeByFeeType("LSC");
			var fees3JohnLocke = entryLine3.Fees.GetOrAddFeeByFeeType("JL");
			var fees3Juno = entryLine3.Fees.GetOrAddFeeByFeeType("JNO");
			fees3Djc.CF_ChargeAmount = 10;
			fees3Djc.CF_MethodOfPayment = "X";
			fees3Lsc.CF_ChargeAmount = 5;
			fees3Lsc.CF_MethodOfPayment = "Y";
			fees3JohnLocke.CF_ChargeAmount = 20;
			fees3JohnLocke.CF_MethodOfPayment = "Y";
			fees3Juno.CF_ChargeAmount = 30;
			fees3Juno.CF_MethodOfPayment = "Z";

			entryLine3.Fees.GetOrAddFeeByFeeType(Core.Constants.Customs.CusEntryFeeTypes.DutyAmount).CF_ChargeAmount = 900m;  // exclude this
			return entry;
		}

		public static void CreateAdditionalInfos(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code, GlbCompany.CurrentCompany.Country.RN_Desc, eun);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "Additional Information");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Direction", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("Level", "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var cusCode1 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9001", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode1.Attributes.AddNew("Direction", "IMPORT");
			cusCode1.Attributes.AddNew("Direction", "EXPORT");
			cusCode1.Attributes.AddNew("Level", "ITEM");

			var cusCode2 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9002", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode2.Attributes.AddNew("Direction", "IMPORT");
			cusCode2.Attributes.AddNew("Direction", "EXPORT");
			cusCode2.Attributes.AddNew("Level", "HEADER");

			var cusCode3 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9003", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode3.Attributes.AddNew("Direction", "IMPORT");
			cusCode3.Attributes.AddNew("Direction", "EXPORT");
			cusCode3.Attributes.AddNew("Level", "ITEM");
			cusCode3.Attributes.AddNew("Level", "HEADER");

			var cusCode4 = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "9004", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCode4.Attributes.AddNew("Direction", "IMPORT");
			cusCode4.Attributes.AddNew("Direction", "EXPORT");
			cusCode4.Attributes.AddNew("Level", "HEADER");

			factory.Save();
		}
	}
}
