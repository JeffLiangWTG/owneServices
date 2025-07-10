using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CusUnderbondDataObjectWriterTest : OrganizationAddressTestHelper
	{
		public void TestMappings()
		{
			var aplVessel = Factory.New<RefVessel>();
			aplVessel.RV_Code = "APL VESSEL";
			aplVessel.RV_LloydsNumber = "9832343";
			aplVessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Jamaica;
			aplVessel.RV_RadioCallSign = "CALLME";

			var underbond = Factory.New<CusUnderbond>();
			underbond.C4_MAWB = "OB1";
			underbond.C4_Status = CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived;
			underbond.C4_FlightNo = "F123";
			underbond.C4_PiecesManifested = 9;
			underbond.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Road;
			underbond.C4_MovementReason = CMRUnderbondRequestCodes.Codes.Transshipment;
			underbond.C4_ArrivalDate = new ZDateTime(2017, 10, 1);
			underbond.C4_Outurned = new ZDateTime(2017, 9, 1);
			underbond.UnderbondStatus.Code = CMRBaseStatuses.Codes.NotSent;

			underbond.C4_UnderbondBySeaVoyage = "12345";
			underbond.C4_IsMoveFromDischarge = ZBool.True;
			underbond.C4_UnderbondBySeaVessel = aplVessel.RV_Code;

			var dischargeAddress = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var originAddress = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			var destinationAddress = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);

			dischargeAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "DI123", Core.Constants.CountryCodes.Australia);
			originAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "OR123", Core.Constants.CountryCodes.Australia);
			destinationAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "DE123", Core.Constants.CountryCodes.Australia);

			underbond.C4_OA_DischargeAddress = dischargeAddress.MainAddress.PK;
			underbond.C4_OA_OriginAddress = originAddress.MainAddress.PK;
			underbond.C4_OA_DestinationAddress = destinationAddress.MainAddress.PK;

			underbond.C4_DischargePremiseID = "DI123";
			underbond.C4_OriginPremiseID = "OR123";
			underbond.C4_DestinationPremiseID = "DE123";
			underbond.C4_ResponsiblePartyID = "RE123";

			var outturn1 = Factory.New<CusOutturn>();
			outturn1.C5_HouseBill = "HB1";
			var outturn2 = Factory.New<CusOutturn>();
			outturn2.C5_HouseBill = "HB2";
			underbond.Outturns.AddRange(outturn1, outturn2);

			Factory.SaveForTesting();

			var writer = new CusUnderbondDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, underbond)));
			var underbondData = writer.GetDataObject(underbond);

			CombineAssertions(delegate
			{
				AssertEquals("underbondData.WayBillNumber", "OB1", underbondData.WayBillNumber);
				AssertNotNull("underbondData.WayBillType", underbondData.WayBillType);
				AssertEquals("underbondData.WayBillType.Code", WayBillTypeList.Codes.Master, underbondData.WayBillType.Code);
				AssertEquals("underbondData.WayBillType.Description", WayBillTypeList.Descriptions.Master, underbondData.WayBillType.Description);
				AssertEquals("underbondData.VoyageFlightNo", "F123", underbondData.VoyageFlightNo);
				AssertEquals("underbondData.TotalNoOfPacks", 9, underbondData.TotalNoOfPieces);
				AssertNotNull("underbondData.TransportMode", underbondData.TransportMode);
				AssertEquals("underbondData.TransportMode.Code", CMRUnderbondModeOfMovement.Codes.Road, underbondData.TransportMode.Code);
				AssertEquals("underbondData.TransportMode.Description", CMRUnderbondModeOfMovement.Descriptions.Road, underbondData.TransportMode.Description);
				AssertNotNull("underbondData.ShipmentType", underbondData.ShipmentType);
				AssertEquals("underbondData.ShipmentType.Code", CMRUnderbondRequestCodes.Codes.Transshipment, underbondData.ShipmentType.Code);
				AssertEquals("underbondData.ShipmentType.Description", CMRUnderbondRequestCodes.Descriptions.Transshipment, underbondData.ShipmentType.Description);
				AssertEquals("underbondData.EntryStatus.Code", CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived, underbondData.EntryStatus.Code);
				AssertEquals("underbondData.EntryStatus.Description", CMRUnderbondStatuses.Descriptions.ExpectedCargoArrivalAdviceReceived, underbondData.EntryStatus.Description);

				AssertDate("ArrivalDate", underbondData, DateType.Arrival, new ZDateTime(2017, 10, 1));
				AssertDate("UnpackDate", underbondData, DateType.Unpack, new ZDateTime(2017, 9, 1));

				AssertAdditionalReference(underbondData.AdditionalReferenceCollection[0], "DI123",
					CodeDescriptionPairForTesting.New(
						Outturn.Constants.AdditionalReference.EntryType.Codes.DischargePremiseID,
						Outturn.Constants.AdditionalReference.EntryType.Descriptions.DischargePremiseID));
				AssertAdditionalReference(underbondData.AdditionalReferenceCollection[1], "OR123",
					CodeDescriptionPairForTesting.New(
						Outturn.Constants.AdditionalReference.EntryType.Codes.OriginPremiseID,
						Outturn.Constants.AdditionalReference.EntryType.Descriptions.OriginPremiseID));
				AssertAdditionalReference(underbondData.AdditionalReferenceCollection[2], "DE123",
					CodeDescriptionPairForTesting.New(
						Outturn.Constants.AdditionalReference.EntryType.Codes.DestinationPremiseID,
						Outturn.Constants.AdditionalReference.EntryType.Descriptions.DestinationPremiseID));
				AssertAdditionalReference(underbondData.AdditionalReferenceCollection[3], "RE123",
					CodeDescriptionPairForTesting.New(
						DataTransfer.Universal.Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
						DataTransfer.Universal.Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID));

				AssertAddInfo("UnderbondBySeaVoyage", underbondData, Outturn.Constants.AddInfoType.UnderbondBySeaVoyage, "12345");
				AssertAddInfo("IsMoveFromDischarge", underbondData, Outturn.Constants.AddInfoType.IsMoveFromDischarge, "Y");
				AssertAddInfo("UnderbondBySeaVessel", underbondData, Outturn.Constants.AddInfoType.UnderbondBySeaVessel, "APL VESSEL");

				AssertNotNull("HB1", underbondData.SubShipmentCollection.FirstOrDefault(x => x.WayBillNumber.GetValueOrDefault() == "HB1"));
				AssertNotNull("HB2", underbondData.SubShipmentCollection.FirstOrDefault(x => x.WayBillNumber.GetValueOrDefault() == "HB2"));

				AssertEntryNumber(underbondData, Outturn.Constants.EntryNumber.EntryType.Codes.UnderbondStatus,
					CodeDescriptionPairForTesting.New(
						Outturn.Constants.EntryNumber.EntryType.Codes.UnderbondStatus,
						Outturn.Constants.EntryNumber.EntryType.Descriptions.UnderbondStatus));
			});

			AssertOrganizationBO_CRAHOLSYD("DischargeAddress", underbondData.OrganizationAddressCollection[0], Outturn.Constants.AddressType.DischargeAddress);
			AssertOrganizationBO_INTHEMSYD("OriginAddress", underbondData.OrganizationAddressCollection[1], Outturn.Constants.AddressType.OriginAddress);
			AssertOrganizationBO_WUFSHIJNB("DestinationAddress", underbondData.OrganizationAddressCollection[2], Outturn.Constants.AddressType.DestinationAddress);
		}

		static void AssertAddInfo(ZString message, UShipment shipment, ZString addInfoType, ZString expectedValue)
		{
			var addinfo = shipment.AddInfoCollection.FirstOrDefault(x => x.Key.GetValueOrDefault() == addInfoType);
			AssertNotNull(message, addinfo);
			AssertEquals(message, expectedValue, addinfo.Value);
		}

		static void AssertAdditionalReference(AdditionalReference additionalReferenceDataObject, ZString referenceNumber, ICodeDescription type, string contextInformation = Core.Constants.CountryCodes.Australia)
		{
			AssertNotNull("Precondition: additionalReferenceDataObject", additionalReferenceDataObject);
			AssertEquals("additionalReferenceDataObject.ContextInformation", contextInformation, additionalReferenceDataObject.ContextInformation);
			AssertEquals("additionalReferenceDataObject.ReferenceNumber", referenceNumber, additionalReferenceDataObject.ReferenceNumber);
			AssertNotNull("additionalReferenceDataObject.Type", additionalReferenceDataObject.Type);
			AssertEquals("additionalReferenceDataObject.Type.Code", type.Code, additionalReferenceDataObject.Type.Code);
			AssertEquals("additionalReferenceDataObject.Type.Description", type.Description, additionalReferenceDataObject.Type.Description);
		}

		static void AssertDate(string message, UShipment shipment, DateType dateType, ZDateTime expectedDate, bool expectedIsEstimate = false)
		{
			var date = shipment.DateCollection.First(x => x.Type.GetValueOrDefault() == dateType);
			AssertEquals(message, expectedDate, date.Value.GetValueOrDefault());
			AssertEquals(message, expectedIsEstimate, date.IsEstimate.GetValueOrDefault());
		}

		static void AssertEntryNumber(UShipment shipment, ZString entryType, ICodeDescription type, string expectedCountryOfIssue = Core.Constants.CountryCodes.Australia)
		{
			var entryNumber = shipment.EntryNumberCollection.First(x => x.Type.Code.GetValueOrDefault() == entryType);
			AssertNotNull("Precondition: entryNumber", entryNumber);
			AssertEquals("entryNumber.CountryOfIssue.Code", expectedCountryOfIssue, entryNumber.CountryOfIssue.Code.GetValueOrDefault());
			AssertNotNull("entryNumber.Type", entryNumber.Type);
			AssertEquals("entryNumber.Type.Code", type.Code, entryNumber.Type.Code);
			AssertEquals("entryNumber.Type.Description", type.Description, entryNumber.Type.Description);
		}
	}
}
