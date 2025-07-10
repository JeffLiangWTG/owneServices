using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	public class NctsHeaderGeneratorTest : TestCaseWithFactory
	{
		[TestDate(2023, 04, 13, 17, 07, 32)]
		public void TestGenerateArrivalFromDeparture()
		{
			var departureHeader = getDepartureForFullTest();
			var generator = new NctsHeaderGenerator();
			var arrivalHeader = generator.GenerateArrivalFromDeparture(departureHeader);

			AssertEquals("Even though it is a TIR declaration, BM_DischargeType should not be filled in for DE.", ZString.Empty, arrivalHeader.ArrivalMovementHeader.BM_DischargeType);
			AssertEquals("In DE EffectiveMessageStatus should always be MAN", NctsMessageStatusList.Codes.ArrivalNotificationNotSent, arrivalHeader.EffectiveMessageStatus);
		}

		NctsHeader getDepartureForFullTest()
		{
			var departureHeader = Factory.New<NctsHeader>();
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureHeader.EffectiveMessageStatus = NctsMessageStatusList.Codes.DepartureDeclarationNotSent;
			departureHeader.MovementReferenceEntryNumber.CE_EntryNum = "123";
			departureHeader.DestinationCustomsOfficeCodeForDeparture = "ABC001";

			var departureMovement = departureHeader.MovementHeader;
			departureMovement.BM_InBondEntryType = NctsConstants.NctsTypeOfDeclaration.Codes.TirDeclaration;

			return departureHeader;
		}
	}
}
