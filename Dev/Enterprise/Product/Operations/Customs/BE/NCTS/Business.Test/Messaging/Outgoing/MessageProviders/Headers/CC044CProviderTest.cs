using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(CC044CProvider))]
	sealed class CC044CProviderTest : NctsHeaderProviderAbstractTest<CC044CProvider>
	{
		public void TestOtherThingsToReport()
		{
			nctsHeader.ArrivalMovementHeader.OtherThingsToReport = "other things";
			AssertEquals("other things", Provider.OtherThingsToReport);
		}

		public void TestTraderIdentificationNumber()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;
			nctsHeader.DestinationTrader.Organisation.CustomsCodes.AddRange(Factory.CreateOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "trader", Core.Constants.CountryCodes.Belgium));
			AssertEquals("BEtrader", Provider.TraderIdentificationNumber);
		}

		public void TestConform()
		{
			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
			AssertEquals(true, Provider.Conform);
		}

		public void TestUnloadingCompletion()
		{
			nctsHeader.ArrivalMovementHeader.BM_UnloadingCompleted = true;
			AssertEquals(true, Provider.UnloadingCompletion);
		}

		public void TestStateOfSeals()
		{
			CombineAssertions(() =>
			{
				nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
				var containerOnHeader = nctsHeader.ArrivalHeaderContainers.AddNew();
				containerOnHeader.Seals.AddNew();
				containerOnHeader.BC_UnloadedState = "MIS";
				CreateProvider();
				AssertEquals("BM_StateOfSealsBoolean: true, Seals on Header: true, Seals on Incident: false", "1", provider.StateOfSeals);

				containerOnHeader.Seals.RemoveAll();
				var containerOnIncident = nctsHeader.EnRouteIncidents.AddNew().IncidentContainers.AddNew();
				containerOnIncident.Seals.AddNew();
				CreateProvider();
				AssertEquals("BM_StateOfSealsBoolean: true, Seals on Header: false, Seals on Incident: true", "1", provider.StateOfSeals);

				nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
				CreateProvider();
				AssertEquals("BM_StateOfSealsBoolean: false, Seals on Header: false, Seals on Incident: true", "0", provider.StateOfSeals);

				nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
				containerOnIncident.Seals.RemoveAll();
				CreateProvider();
				AssertEquals("BM_StateOfSealsBoolean: true, Seals on Header: false, Seals on Incident: false", null, provider.StateOfSeals);

				nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = false;
				CreateProvider();
				AssertEquals("BM_StateOfSealsBoolean: false, Seals on Header: false, Seals on Incident: false", null, provider.StateOfSeals);
			});
		}

		public void TestUnloadingdate()
		{
			CombineAssertions(() =>
			{
				var date = new DateTime(1994, 2, 1);
				nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = date;
				AssertEquals(date, Provider.Unloadingdate);

				nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = ZDateTimeOffset.Empty;
				AssertEquals("Unloadingdate should be MinValue from Empty", DateTime.MinValue, Provider.Unloadingdate);

				nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = new ZDateTimeOffset(DateTime.MinValue);
				AssertEquals("Unloadingdate should be MinValue from MinValue", DateTime.MinValue, Provider.Unloadingdate);
			});
		}

		public void TestUnloadingRemark()
		{
			nctsHeader.ArrivalMovementHeader.BM_UnloadingRemarks = "unloading remark";
			AssertEquals("unloading remark", Provider.UnloadingRemark);
		}

		public void TestConsignment()
		{
			nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
			var transportInfo = nctsHeader.ArrivalMovementHeader.ArrivalTransportInfos.AddNew();
			transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
			var house = nctsHeader.Bills.AddNew();
			house.UnloadedStatus = NctsUnloadedStateListForHouseConsignment.Codes.DEC;
			var container = nctsHeader.ArrivalHeaderContainers.AddNew();
			container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			var seal = container.Seals.AddNew();
			seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DEC;

			CombineAssertions(() =>
			{
				var provider = new CC044CProvider(nctsHeader);
				AssertNull("No consignment has to be passed when all unloaded states are equal to DEC and BM_NoChangesToReport = true", provider.Consignment);

				nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
				provider = new CC044CProvider(nctsHeader);
				AssertNull("BM_NoChangesToReport = false, house = DEC", provider.Consignment);

				house.UnloadedStatus = NctsUnloadedStateListForHouseConsignment.Codes.DIF;
				provider = new CC044CProvider(nctsHeader);
				AssertNotNull("BM_NoChangesToReport = false, house = DIF", provider.Consignment);

				nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
				provider = new CC044CProvider(nctsHeader);
				AssertNull("BM_NoChangesToReport = true, house = DIF", provider.Consignment);

				house.UnloadedStatus = NctsUnloadedStateListForHouseConsignment.Codes.DEC;
				transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DIF;
				provider = new CC044CProvider(nctsHeader);
				AssertNotNull("ArrivalTransportInfo.TPM_TransportState = DIF", provider.Consignment);

				transportInfo.TPM_TransportState = NctsUnloadedStateList.Codes.DEC;
				container.BC_UnloadedState = NctsUnloadedStateList.Codes.DIF;
				provider = new CC044CProvider(nctsHeader);
				AssertNotNull("ArrivalHeaderContainers.BC_UnloadedState = DIF", provider.Consignment);

				container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DIF;
				provider = new CC044CProvider(nctsHeader);
				AssertNotNull("ArrivalHeaderContainers.Seals.BK_UnloadingState = DIF", provider.Consignment);

				seal.BK_UnloadingState = NctsUnloadedStateList.Codes.DAM;
				provider = new CC044CProvider(nctsHeader);
				AssertNull("ArrivalHeaderContainers.Seals.BK_UnloadingState = DAM", provider.Consignment);
			});
		}

		protected override string MessageType => Constants.MessageTypes.CC044C;

		protected override string MovementType => NctsMovementType.Codes.Arrival;
	}
}
