using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using CusEntryLineFee = Enterprise.Customs.CA.Business.CusEntryLineFee;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class CustomsEntryLineDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPopulateCADSequenceData()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			entryLine.CL_GoodsShipmentSequence = 1;
			entryLine.CL_CommoditySequence = 2;
			Factory.Save();

			var writer = new CustomsEntryLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new UniversalDataObjectWriterHelper(declaration));
			var result = writer.GetDataObject(entryLine);
			AssertEquals(ZString.Empty, GetAddInfoValueWithKey(result, "GoodsShipmentSequence"));
			AssertEquals(ZString.Empty, GetAddInfoValueWithKey(result, "CommoditySequence"));

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				result = writer.GetDataObject(entryLine);
				AssertEquals("1", GetAddInfoValueWithKey(result, "GoodsShipmentSequence"));
				AssertEquals("2", GetAddInfoValueWithKey(result, "CommoditySequence"));
			}
		}

		public void TestPopulateAllFees()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			var invoice = declaration.Invoices.AddNew();
			invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());

			var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
			SetupCusEntryLineFee(entryLine.Fees.AddNew(), 102.23m, Core.Constants.USCustoms.FeeCodes.Blueberry, "CW1");
			SetupCusEntryLineFee(entryLine.ConfirmedFees.AddNew(), 1.23m, Core.Constants.USCustoms.FeeCodes.Blueberry, "CUS");
			Factory.Save();

			var writer = new CustomsEntryLineDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new UniversalDataObjectWriterHelper(declaration));
			var result = writer.GetDataObject(entryLine);
			var entryLineCharges = result.EntryLineChargeCollection;
			AssertEquals(1, entryLineCharges.Count);
			AssertEquals("CUS", entryLineCharges[0].Source);

			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Enterprise.Customs.Universal.Constants.FunctionalityTypes.CarmR2, Core.Constants.CountryCodes.Canada, ZDateTime.Now, true))
			{
				result = writer.GetDataObject(entryLine);
				entryLineCharges = result.EntryLineChargeCollection;
				AssertEquals(2, entryLineCharges.Count);
				AssertEquals("CUS", entryLineCharges[0].Source);
				AssertEquals("CW1", entryLineCharges[1].Source);
			}
		}

		void SetupCusEntryLineFee(CusEntryLineFee entryLineFee, ZDecimal chargeAmount, ZString chargeType,ZString chargeSource)
		{
			entryLineFee.CF_ChargeAmount = chargeAmount;
			entryLineFee.CF_ChargeType = chargeType;
			entryLineFee.CF_BaseValue = 135.62m;
			entryLineFee.CF_Rate = 0.68m;
			entryLineFee.CF_RateOverrideReasonCode = "OTH";
			entryLineFee.CF_MethodOfPayment = "PPD";
			entryLineFee.CF_MethodOfCalculation = "SUM";
			entryLineFee.CF_Source = chargeSource;
		}

		ZString GetAddInfoValueWithKey(UniversalDataBuss.DataObjects.Universal.Customs.EntryLine entryLine, ZString key)
		{
			return entryLine.AddInfoCollection.ToList().FirstOrDefault(x => x.Key.GetValueOrDefault() == key)?.Value ?? ZString.Empty;
		}
	}
}
