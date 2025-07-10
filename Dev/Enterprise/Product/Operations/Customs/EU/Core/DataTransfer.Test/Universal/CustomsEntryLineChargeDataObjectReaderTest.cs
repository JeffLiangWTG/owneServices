using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	class CustomsEntryLineChargeDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestCusEntryLineFeeFieldMappingsDoesNotCalculateChargeAmount()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			var entryLineChargeDataObject = new UniversalCustoms.EntryLineCharge
			{
				Amount = 0m,
				BaseValue = 135.62m,
				Rate = 0.68m,
				RateOverrideReason = new CodeDescriptionPair { Code = "ADD", Description = "ADD DESC" },
			};

			var entryLineChargeBO = new CustomsEntryLineChargeDataObjectReader(entryLineChargeDataObject, logger, CurrentCompanyHelper, entryLine).ReadIntoBusinessObject();
			CombineAssertions("Assert mappings", () =>
			{
				AssertEquals("entryLineChargeBO.CF_RateOverrideReasonCode", "ADD", entryLineChargeBO.CF_RateOverrideReasonCode);
				AssertEquals("entryLineChargeBO.CF_ChargeAmount", 0.0m, entryLineChargeBO.CF_ChargeAmount);
				AssertEquals("entryLineChargeBO.CF_BaseValue", 135.62m, entryLineChargeBO.CF_BaseValue);
				AssertEquals("entryLineChargeBO.CF_Rate", 0.68m, entryLineChargeBO.CF_Rate);
			});
		}
	}
}
