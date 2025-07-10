using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(CC170CProvider))]
	sealed class CC170CProviderTest : NctsHeaderProviderAbstractTest<CC170CProvider>
	{
		public void TestLimitDate()
		{
			CombineAssertions(() =>
			{
				nctsHeader.MovementHeader.BM_ExportDate = new ZDateTime(1994, 2, 1);
				AssertEquals(new ZDateTime(1994, 2, 1), Provider.LimitDate);

				nctsHeader.MovementHeader.BM_ExportDate = ZDateTime.Empty;
				AssertEquals("LimitDate should be MinValue from Empty", DateTime.MinValue, Provider.LimitDate);

				nctsHeader.MovementHeader.BM_ExportDate = new ZDateTime(DateTime.MinValue);
				AssertEquals("LimitDate should be MinValue from MinValue", DateTime.MinValue, Provider.LimitDate);

				nctsHeader.MovementHeader.BM_ExportDate = new DateTime(2024, 12, 11, 10, 9, 8, 765);
				AssertEquals(new ZDateTime(2024, 12, 11, 10, 9, 8).ToString(@"yyyy-MM-dd'T'hh:mm:ss", CultureInfo.InvariantCulture), Provider.LimitDate.ToString(@"yyyy-MM-dd'T'hh:mm:ss", CultureInfo.InvariantCulture));
				AssertEquals("no milliseconds", 0, Provider.LimitDate.Millisecond);
			});
		}

		public void TestHolderOfTheTransitProcedureIdentificationNumber()
		{
			AssertNull(Provider.HolderOfTheTransitProcedureIdentificationNumber);
		}

		public void TestHolderOfTheTransitProcedureIdentificationNumberNoTIRInBondEntryType()
		{
			CreatePrincipal();
			nctsHeader.MovementHeader.BM_InBondEntryType = "OTH";
			AssertEquals("GBHOLDERID", Provider.HolderOfTheTransitProcedureIdentificationNumber);
		}

		public void TestConsignment()
		{
			AssertNotNull(Provider.Consignment);
			AssertNotNull(Provider.Consignment.PlaceOfLoading.UnLocode);
		}

		protected override string MessageType => Constants.MessageTypes.CC170C;

		protected override string MovementType => NctsMovementType.Codes.Departure;
	}
}
