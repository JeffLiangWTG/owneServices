using System;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(ECWCCMLineProvider))]
	sealed class ECWCCMLineProviderTest : ImportDecLineProviderAbstractTest<ECWCCMLineProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new ECWCCMLineProvider(null));
		}

		public void TestSequenceNumber()
		{
			entryLine.CL_LineNumber = 2;
			AssertEquals(2, Provider.SequenceNumber);
		}

		public void TestInwardMovementRegistrationNumber()
		{
			invoiceLine.JI_PreviousEntryNumber = "EN123456";
			AssertEquals("EN123456", Provider.InwardMovementRegistrationNumber);
		}

		public void TestInwardMovementSequenceNumber()
		{
			invoiceLine.JI_PreviousEntryLineNumber = 1;
			AssertEquals(1, Provider.InwardMovementSequenceNumber);
		}

		public void TestOutwardMovementCompletionType()
		{
			invoiceLine.JI_Procedure = "4210";
			AssertEquals("42", Provider.OutwardMovementCompletionType);
		}

		public void TestOutwardMovementCompletionRegistrationNumber()
		{
			var mrnCusEntryNumber = CusEntryNumber.LoadOrCreate(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnCusEntryNumber.CE_EntryNum = "EN123456";
			AssertEquals("EN123456", Provider.OutwardMovementCompletionRegistrationNumber);
		}

		public void TestOutwardMovementCompletionRegistrationNumber_NoMovementReferenceNumber()
		{
			AssertNull(Provider.OutwardMovementCompletionRegistrationNumber);
		}

		public void TestOutwardMovementDecisiveDate()
		{
			var mrnCusEntryNumber = CusEntryNumber.LoadOrCreate(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnCusEntryNumber.CE_ExpiryDate = new ZDateTime(2021, 06, 18);
			AssertEquals(new DateTime(2021, 06, 18), Provider.OutwardMovementDecisiveDate);
		}

		public void TestOutwardMovementDecisiveDate_NoMovementReferenceNumber()
		{
			AssertNull(Provider.OutwardMovementDecisiveDate);
		}

		public void TestOutwardMovementDecisiveDate_InvalidDate()
		{
			var mrnCusEntryNumber = CusEntryNumber.LoadOrCreate(invoiceLine, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnCusEntryNumber.CE_ExpiryDate = ZDateTime.Invalid;
			AssertNull(Provider.OutwardMovementDecisiveDate);
		}

		public void TestOutwardMovementAmount()
		{
			invoiceLine.JI_BondedWhsQuantity = 23.5m;
			invoiceLine.JI_BondedWhsUnitQty = "KGMG";

			CombineAssertions(() =>
			{
				AssertEquals("Quantity", 23.5m, Provider.OutwardMovementAmount.Quantity);
				AssertEquals("MeasurementUnit", "KGM", Provider.OutwardMovementAmount.MeasurementUnit);
				AssertEquals("Qualifier", "G", Provider.OutwardMovementAmount.Qualifier);
			});
		}

		protected override ECWCCMLineProvider GetProvider() => new ECWCCMLineProvider(entryLine);

		new IECWCCMLine Provider => base.Provider;
	}
}
