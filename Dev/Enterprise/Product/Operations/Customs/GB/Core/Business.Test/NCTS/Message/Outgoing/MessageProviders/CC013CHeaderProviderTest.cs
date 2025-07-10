using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(CC013CHeaderProvider))]
	sealed class CC013CHeaderProviderTest : NctsHeaderProviderAbstractTest<CC013CHeaderProvider>
	{
		protected override string MessageType => Constants.MessageTypes.CC013C;

		protected override string MovementType => NctsMovementType.Codes.Departure;

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CC013CHeaderProvider(null));
		}

		public void TestConsignment()
		{
			AssertNotNull(Provider.Consignment);
			AssertType<NCTSConsignmentProvider>(Provider.Consignment);
		}

		public void TestTransitOperation()
		{
			nctsHeader.ArrivalMrnFromUser = "MRN";
			nctsHeader.MovementHeader.BM_PaperlessInbondNum = "LRN";

			nctsHeader.MovementHeader.BM_EntryDate = ZDateTime.Now;
			AssertEquals(null, Provider.TransitOperation.LRN);
			AssertEquals("MRN", Provider.TransitOperation.MRN);

			nctsHeader.MovementHeader.BM_EntryDate = ZDateTime.Empty;
			AssertEquals("LRN", Provider.TransitOperation.LRN);
			AssertEquals(null, Provider.TransitOperation.MRN);
		}
	}
}
