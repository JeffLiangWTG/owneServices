using System;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(CC015CHeaderProvider))]
	sealed class CC015CHeaderProviderTest : NctsHeaderProviderAbstractTest<CC015CHeaderProvider>
	{
		protected override string MessageType => Constants.MessageTypes.CC015C;

		protected override string MovementType => NctsMovementType.Codes.Departure;

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CC015CHeaderProvider(null));
		}

		public void TestConsignment()
		{
			AssertNotNull(Provider.Consignment);
			AssertType<NCTSConsignmentProvider>(Provider.Consignment);
		}

		public void TestTransitOperation()
		{
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
			var movementHeader = nctsHeader.MovementHeader;
			movementHeader.IsSimplifiedNctsProcedure = CargoWise.Types.ZBool.True;
			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T2;
			movementHeader.BM_PaperlessInbondNum = "LRN";
			movementHeader.Header.BH_ApplicationCode = Customs.Common.CusInBondApplicationCodeList.Codes.NCTS5;
			AssertAdditionalDeclarationType(NctsTypeOfAdditionalDeclarationList.Codes.D);
			AssertEquals("Departure", true, nctsHeader.IsDepartureMovement);
			AssertEquals(Enterprise.Customs.Common.EU.NctsMoveHeaderType.Codes.Departure, nctsHeader.MovementHeader.BM_SubApplicationCode);
			AssertEquals(Customs.Common.CusInBondApplicationCodeList.Codes.NCTS5, nctsHeader.BH_ApplicationCode);
			AssertEquals("'A' for message type Depart (015), and 'D' for message type 170 (prelodge)", "D", provider.TransitOperation.AdditionalDeclarationType);
			AssertEquals("LRN", provider.TransitOperation.LRN);
			nctsHeader.BH_HeaderType = NctsMovementType.Codes.DepartureAndArrival;
			AssertEquals("DepartureAndArrival", true, nctsHeader.IsDepartureMovement);
			AssertNotNull(provider.TransitOperation.AdditionalDeclarationType);
		}

		void AssertAdditionalDeclarationType(string additionalDeclarationTypeCode)
		{
			nctsHeader.MovementHeader.BM_AdditionalDeclarationType = additionalDeclarationTypeCode;
			AssertEquals("Type is " + additionalDeclarationTypeCode, additionalDeclarationTypeCode, GetProvider().TransitOperation.AdditionalDeclarationType);
		}
	}
}
