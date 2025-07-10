using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.NCTS;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	public class FRNctsMessageTest : TestCaseWithFactory
	{
		public void TestNctsMessageSenderExistForFR()
		{
			SetupNctsCC015ForTest(Factory);

			var chooser = new NctsMessageSenderChooser(cc015, new NctsMessageFunctionSet.DeclarationDataMessage());
			AssertNotNull(chooser);

			var messageSender = chooser.CountrySpecificChooser;
			AssertType<NctsMessageSender>(messageSender);
			AssertEquals("FR", chooser.NctsDomainCountryCode);
		}

		public void TestNctsDTBuilderExistForFR()
		{
			SetupNctsCC015ForTest(Factory);

			var chooser = new NctsNativeBuilderChooser(cc015, new NctsMessageFunctionSet.DeclarationDataMessage());
			AssertNotNull(chooser);

			var nativeBuilder = chooser.CountrySpecificChooser;
			AssertType<NctsDTMessageBuilder>(nativeBuilder);
			AssertEquals("FR", chooser.NctsDomainCountryCode);
		}

		NctsHeader SetupNctsCC015ForTest(BusinessObjectFactory factory)
		{
			cc015 = factory.New<NctsHeader>();
			cc015.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			cc015.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			cc015.MovementHeader.BM_InBondEntryType = "T1";
			cc015.MovementHeader.BM_RL_NKDestinationPort = "IT";
			cc015.MovementHeader.BM_LocationOfGoods = "Pre-Lodged";
			cc015.MovementHeader.BM_LocationOfGoodsCode = "954131533-GB60DEP"; // TODO Port of presentation - GBLHRBAC
			cc015.MovementHeader.BM_RL_NKForeignDestPort = "GBDVR";
			cc015.BH_RL_NKImportLoadPort = Core.Constants.CountryCodes.France;
			cc015.MovementHeader.BM_TOLCarrierID = "NC15REG";
			cc015.MovementHeader.BM_EntryDate = ZDateTime.Today;

			NCTSTestHelper.SetupContainersAndSealsForTest(cc015);

			var departureOffice = cc015.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDeparture);
			departureOffice.CY_Data = "FR000060";
			var destinationOffice = cc015.CustomsOffices.Cast<EU.NCTS.Business.NctsEuOfficeCode>().FirstOrDefault(x => x.CY_Code == EuOfficeCodesTypes.Codes.OfficeOfDestination);
			destinationOffice.CY_Data = "IT021300";

			cc015.MovementHeader.BM_MethodOfPayment = "A";
			cc015.MovementHeader.BM_ExportTransportMode = ModeOfTransportList.Codes._3_RoadTransport;

			return cc015;
		}
		NctsHeader cc015;
	}
}
