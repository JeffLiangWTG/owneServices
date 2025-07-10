using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IL.Business.Testing
{
	partial class CusEntryHeaderTest
	{
		public void TestCH_BGMReference()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "TST0001";
			var declarationWithShipment = Factory.New<JobDeclaration>();
			declarationWithShipment.JE_JS = shipment.PK;

			var header1 = declarationWithShipment.ActiveEntryHeaders.AddNew();
			var header2 = declarationWithShipment.ActiveEntryHeaders.AddNew();

			AssertEquals("Prereq: BGM Reference is empty", ZString.Empty, header1.CH_BGMReference);
			AssertEquals("Prereq: BGM Reference is empty", ZString.Empty, header2.CH_BGMReference);

			Factory.Save();

			AssertEquals("BGM Reference should have expected value", "TST0001/1", header1.CH_BGMReference);
			AssertEquals("BGM Reference should have expected value", "TST0001/2", header2.CH_BGMReference);

			header2.CH_MessageType = JobMessageTypeList.Codes.Import;
			header1.CH_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			AssertEquals("BGM Reference should remain with expected value", "TST0001/1", header1.CH_BGMReference);
			AssertEquals("BGM Reference should remain with expected value", "TST0001/2", header2.CH_BGMReference);

			var declarationWithoutShipment = Factory.New<JobDeclaration>();
			declarationWithoutShipment.JE_DeclarationReference = "TST0002";

			var header3 = declarationWithoutShipment.ActiveEntryHeaders.AddNew();
			var header4 = declarationWithoutShipment.ActiveEntryHeaders.AddNew();

			AssertEquals("Prereq: BGM Reference is empty", ZString.Empty, header3.CH_BGMReference);
			AssertEquals("Prereq: BGM Reference is empty", ZString.Empty, header4.CH_BGMReference);

			Factory.Save();

			AssertEquals("BGM Reference should have expected value", "TST0002/1", header3.CH_BGMReference);
			AssertEquals("BGM Reference should have expected value", "TST0002/2", header4.CH_BGMReference);

			header4.CH_MessageType = JobMessageTypeList.Codes.Import;
			header3.CH_MessageType = JobMessageTypeList.Codes.Import;

			Factory.Save();

			AssertEquals("BGM Reference should remain with expected value", "TST0002/1", header3.CH_BGMReference);
			AssertEquals("BGM Reference should remain with expected value", "TST0002/2", header4.CH_BGMReference);
		}

		public override void TestTotalDutyAmount()
		{
			var factory = Factory;
			CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(factory, GlbCompany.CurrentCompany.GC_RN_NKCountryCode, Universal.Constants.RateTypes.Duty, FeeTypeList.Codes.A00);
			var baseJobDeclaration = factory.New<JobDeclaration>();
			var cusEntryHeader = baseJobDeclaration.CustomsEntryHeaders.AddNew();
			var cusEntryLine = cusEntryHeader.MergedLines.AddNew();
			cusEntryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts).CF_ChargeAmount = 200m;
			var cusEntryLine2 = cusEntryHeader.MergedLines.AddNew();
			cusEntryLine2.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts).CF_ChargeAmount = 300m;
			cusEntryLine2.Fees.GetOrAddFeeByFeeType(Constants.EntryLineFee.VATFeeTypeCode).CF_ChargeAmount = 100m;
			AssertEquals("Total Duty Amount", 500m, cusEntryHeader.TotalDutyAmount);
		}
	}
}
