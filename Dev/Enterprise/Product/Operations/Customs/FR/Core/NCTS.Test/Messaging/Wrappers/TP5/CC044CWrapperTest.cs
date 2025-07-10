using System;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class CC044CWrapperTest : Customs.Business.Testing.DataProviderTestCase<CC044CWrapper>
	{
		public void TestMessageSender()
		{
			AssertEquals("MessageSender should equal to OPE.FR.", FRConstants.NCTSMessage.Operator, Provider.MessageSender);
		}

		public void TestMessageRecipient()
		{
			AssertEquals("MessageRecipient should equal to NTA.FR.", FRConstants.NCTSMessage.NationalAdministration, Provider.MessageRecipient);
		}

		[TestDate(2024, 02, 09, 12, 11, 10)]
		public void TestPreparationDateAndTime()
		{
			AssertEquals("PreparationDateAndTime should equal to DateTime.Now.", new DateTime(2024, 02, 09, 12, 11, 10), Provider.PreparationDateAndTime);
		}

		public void TestCorrelationIdentifier()
		{
			AssertEquals("CorrelationIdentifier should return an empty string.", ZString.Empty, Provider.CorrelationIdentifier);
		}

		public void TestTransitOperation()
		{
			AssertEquals("TransitOperation should be mapped to NCTS Header.", "MRN001", Provider.TransitOperation.MRN);
		}

		public void TestCustomsOfficeOfDestinationActual()
		{
			AssertEquals("CustomsOfficeOfDestinationActual should be using CustomsOfficeWrapper.", "FR000001", Provider.CustomsOfficeOfDestinationActual.ReferenceNumber);
		}

		public void TestTraderAtDestination()
		{
			AssertContainsExactElementsInAnyOrder("TraderAtDestination should be using TraderWrapper.", "FR0123456789", Provider.TraderAtDestination.IdentificationNumber);
		}

		public void TestUnloadingRemark()
		{
			AssertEquals("UnloadingRemark should be using UnloadingRemarkWrapper.", false, Provider.UnloadingRemark.Conform);
		}

		public void TestMessageEnveloppe()
		{
			AssertEquals("MessageEnveloppe should be using MessageEnveloppeWrapper - TransactionId.", "~CORRELATIONID~", Provider.MessageEnveloppe.TransactionId);
			AssertEquals("MessageEnveloppe should be using MessageEnveloppeWrapper - SchemaId.", "IE044", Provider.MessageEnveloppe.SchemaId);
		}

		public void TestConsignment()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
			var transportInfo = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			transportInfo.TPM_TransportState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			var house = nctsHeader.Bills.AddNew();
			house.UnloadedStatus = EU.NCTS.Business.NctsUnloadedStateListForHouseConsignment.Codes.DEC;
			var container = nctsHeader.ArrivalHeaderContainers.AddNew();
			container.BC_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
			var seal = container.Seals.AddNew();
			seal.BK_UnloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;

			CombineAssertions(() =>
			{
				var provider = CC044CWrapper.New(nctsHeader);
				AssertNull("No consignment has to be passed when all unloaded states are equal to DEC and BM_NoChangesToReport = true", provider.Consignment);

				nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
				provider = CC044CWrapper.New(nctsHeader);
				AssertNull("BM_NoChangesToReport = false, house = DEC", provider.Consignment);

				house.UnloadedStatus = EU.NCTS.Business.NctsUnloadedStateListForHouseConsignment.Codes.DIF;
				provider = CC044CWrapper.New(nctsHeader);
				AssertNotNull("BM_NoChangesToReport = false, house = DIF", provider.Consignment);

				nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
				provider = CC044CWrapper.New(nctsHeader);
				AssertNull("BM_NoChangesToReport = true, house = DIF", provider.Consignment);

				house.UnloadedStatus = EU.NCTS.Business.NctsUnloadedStateListForHouseConsignment.Codes.DEC;
				transportInfo.TPM_TransportState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
				provider = CC044CWrapper.New(nctsHeader);
				AssertNotNull("ArrivalTransportInfo.TPM_TransportState = DIF", provider.Consignment);

				transportInfo.TPM_TransportState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
				container.BC_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
				provider = CC044CWrapper.New(nctsHeader);
				AssertNotNull("ArrivalHeaderContainers.BC_UnloadedState = DIF", provider.Consignment);

				container.BC_UnloadedState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DEC;
				seal.BK_UnloadingState = EU.NCTS.Business.NctsUnloadedStateList.Codes.DIF;
				provider = CC044CWrapper.New(nctsHeader);
				AssertNotNull("ArrivalHeaderContainers.Seals.BK_UnloadingState = DIF", provider.Consignment);
			});
		}

		protected override CC044CWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			nctsHeader.BH_JobReference = "MRN0121";

			var arrivalmovementheader = nctsHeader.ArrivalMovementHeader;
			arrivalmovementheader.BM_RN_NKCountryOfDispatch = "TR";

			arrivalmovementheader.BM_NoChangesToReport = false;
			arrivalmovementheader.BM_UnloadingRemarks = "Unloading Renarks";
			arrivalmovementheader.BM_GrossWeight = 999.99m;

			var office = arrivalmovementheader.CustomsOffices.AddNew();
			office.CY_Code = OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival;
			office.CY_Data = "FR000001";

			arrivalmovementheader.DestinationCustomsOfficeCodeForArrival = "FR000001";

			var destinationTrader = Factory.New<OrgHeader>();
			destinationTrader.OH_Language = "FR";
			nctsHeader.DestinationTrader.E2_OA_Address = destinationTrader.MainAddress.PK;

			var cusCode = destinationTrader.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			cusCode.OK_CustomsRegNo = "0123456789";

			var mrn = CusEntryNumber.LoadOrCreate(nctsHeader, "MRN", Core.Constants.CountryCodes.France);
			mrn.CE_EntryNum = "MRN001";

			return CC044CWrapper.New(nctsHeader);
		}
	}
}
