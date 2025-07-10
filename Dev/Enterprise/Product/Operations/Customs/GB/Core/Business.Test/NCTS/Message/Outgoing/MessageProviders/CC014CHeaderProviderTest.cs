using System;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(CC014CHeaderProvider))]
	sealed class CC014CHeaderProviderTest : NctsHeaderProviderAbstractTest<CC014CHeaderProvider>
	{
		protected override string MessageType => Constants.MessageTypes.CC014C;

		protected override string MovementType => NctsMovementType.Codes.Departure;

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CC014CHeaderProvider(null, ZString.Empty));
		}

		public void TestTransitOperation()
		{
			nctsHeader.MovementHeader.BM_EntryDate = ZDateTime.Now;
			AssertEquals(null, Provider.TransitOperation.LRN);
			AssertEquals("MRN", Provider.TransitOperation.MRN);

			nctsHeader.MovementHeader.BM_EntryDate = ZDateTime.Empty;
			AssertEquals("LRN", Provider.TransitOperation.LRN);
			AssertEquals(null, Provider.TransitOperation.MRN);
		}

		public void TestInvalidation()
		{
			AssertNotNull(Provider.Invalidation);
			AssertType<InvalidationType02Provider>(Provider.Invalidation);
		}

		public void TestHolderOfTheTransitProcedure()
		{
			AssertNotNull(Provider.HolderOfTheTransitProcedure);
		}

		public void TestJustification()
		{
			AssertEquals("Justification", "I changed my mind", Provider.Invalidation.Justification);
		}

		protected override void SetUp()
		{
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(MovementType);
			nctsHeader.ArrivalMrnFromUser = "MRN";
			nctsHeader.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			nctsHeader.MovementHeader.BM_Phase = GB_NCTS5DeparturePhaseList.Codes.CancellationAmendment;
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN";
		}

		public void TestMessageRecipient()
		{
			var movementHeader = nctsHeader.MovementHeader;
			AssertEquals("No DepartureCustomsOffice", ZString.Empty, nctsHeader.DepartureCustomsOfficeCode);
			NCTSTestHelper.CreateCustomsOfficeForTest(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "GB000011", ZDateTime.Empty, true);
			AssertEquals("Has DepartureCustomsOffice", "GB000011", movementHeader.DepartureCustomsOfficeCode);
			AssertEquals("NTA.GB", Provider.MessageRecipient);
		}

		protected override CC014CHeaderProvider GetProvider() => new CC014CHeaderProvider(nctsHeader, "I changed my mind");
	}
}
