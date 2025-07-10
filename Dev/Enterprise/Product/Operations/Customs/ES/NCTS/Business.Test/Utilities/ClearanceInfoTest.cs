using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(ClearanceInfo))]
	public class ClearanceInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadNew()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when cusNCTSHeader is null", () => ClearanceInfo.LoadNew(null));
				var cusNCTSHeader = CreateNCTS();
				AssertNoExceptionThrown("No exception expected", () => ClearanceInfo.LoadNew(cusNCTSHeader));

				var clearanceInfo = ClearanceInfo.LoadNew(cusNCTSHeader);
				AssertEquals("ClearanceNumber is CE_EntryNum from cusNCTSHeader", mrnCode, clearanceInfo.ClearanceNumber);
				AssertEquals("ClearanceDate is CE_IssueDate from cusNCTSHeader", ZDateTime.Today, clearanceInfo.ClearanceDate);
				AssertEquals("ArrivalLimitDate is CE_ExpiryDate from cusNCTSHeader", ZDateTime.Today, clearanceInfo.ArrivalLimitDate);
			});
		}

		public void TestCSVCodeIsValidOrEmpty()
		{
			var cusNCTSHeader = CreateNCTS();

			CombineAssertions(() =>
			{
				AssertEquals("Input CsvCode returns false when code is not valid", false, ClearanceInfo.CSVCodeIsValidOrEmpty("+--**__aa/#€&@"));
				AssertEquals("Input CsvCode returns true when code is valid", true, ClearanceInfo.CSVCodeIsValidOrEmpty("CLEARANCE1234567"));
				AssertEquals("Input CsvCode returns true when code is Empty", true, ClearanceInfo.CSVCodeIsValidOrEmpty(ZString.Empty));
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cusNCTSHeader = CreateNCTS();
			var updateCSVClearance = ClearanceInfo.LoadNew(cusNCTSHeader);
			return updateCSVClearance;
		}

		CusEntryNumber CreateNCTS()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = ApplicationReference;
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.EffectiveMessageStatus = OriginalMessageStatus;
			nctsHeader.MovementHeader.BM_CustomsStatus = OriginalEntryStatus;
			return SetEntryNumber(nctsHeader);
		}

		const string mrnCode = "20ES00999830001277";
		ZString OriginalMessageStatus => "INI";
		ZString OriginalEntryStatus => "INI";
		ZString ApplicationReference => "Reference";

		CusEntryNumber SetEntryNumber(NctsHeader nctsHeader)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = mrnCode;
			newEntryNumber.CE_IssueDate = ZDateTime.Today;
			newEntryNumber.CE_ExpiryDate = ZDateTime.Today;

			return newEntryNumber;
		}
	}
}
