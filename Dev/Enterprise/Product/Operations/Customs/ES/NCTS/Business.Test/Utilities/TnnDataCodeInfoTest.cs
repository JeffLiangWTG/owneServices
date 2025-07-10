using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(TnnDataCodeInfo))]
	public class TnnDataCodeInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestLoadNew()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>("Exception should be thrown when NctsHeader is null", () => TnnDataCodeInfo.LoadNew(null));
				var nctsHeader = CreateNCTS();
				AssertNoExceptionThrown("No exception expected", () => TnnDataCodeInfo.LoadNew(nctsHeader));

				var tnnDataCodeInfo = TnnDataCodeInfo.LoadNew(nctsHeader);
				AssertEquals("AcceptanceDate from NctsHeader", ZDate.Today.AddDays(-1), tnnDataCodeInfo.AcceptanceDate);
				AssertEquals("ClearanceDate from NctsHeader", ZDate.Today, tnnDataCodeInfo.ClearanceDate);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var cusNCTSHeader = CreateNCTS();
			var tnnDataCodeInfo = TnnDataCodeInfo.LoadNew(cusNCTSHeader);
			return tnnDataCodeInfo;
		}

		NctsHeader CreateNCTS()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_JobReference = "Reference";
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			nctsHeader.EffectiveMessageStatus = OriginalMessageStatus;
			nctsHeader.MovementHeader.BM_CustomsStatus = OriginalEntryStatus;
			SetEntryNumberMRN(nctsHeader);
			SetEntryNumberCLR(nctsHeader);
			return nctsHeader;
		}

		ZString OriginalMessageStatus => "INI";
		ZString OriginalEntryStatus => "INI";

		CusEntryNumber SetEntryNumberMRN(NctsHeader nctsHeader)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "20ES00999830001277";
			newEntryNumber.CE_IssueDate = ZDateTime.Today.AddDays(-1);

			return newEntryNumber;
		}
		CusEntryNumber SetEntryNumberCLR(NctsHeader nctsHeader)
		{
			var newEntryNumber = CusEntryNumber.LoadOrCreate(nctsHeader, CusEntryNumberTypes.Spain.ClearanceCSV, Core.Constants.CountryCodes.Spain);
			newEntryNumber.CE_EntryNum = "CSV22223333";
			newEntryNumber.CE_IssueDate = ZDateTime.Today;

			return newEntryNumber;
		}
	}
}
