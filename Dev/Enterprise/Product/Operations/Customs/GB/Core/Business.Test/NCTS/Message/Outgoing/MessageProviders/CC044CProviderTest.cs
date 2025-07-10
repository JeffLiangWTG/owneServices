using System;
using System.Globalization;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(CC044CProvider))]
	sealed class CC044CProviderTest : NctsHeaderProviderAbstractTest<CC044CProvider>
	{
		public void TestMessageRecipient()
		{
			CombineAssertions(() =>
			{
				var arrivalMovementHeader = nctsHeader.ArrivalMovementHeader;
				arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = "GB";
				AssertEquals("Code should handle expected value GB", "NTA.GB", Provider.MessageRecipient);

				arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = "XI";
				AssertEquals("Code should handle expected value XI", "NTA.XI", Provider.MessageRecipient);

				arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = "ZZZ";
				AssertEquals("Code should be truncated to two characters", "NTA.ZZ", Provider.MessageRecipient);

				arrivalMovementHeader.DestinationCustomsOfficeCodeForArrival = ZString.Empty;
				AssertEquals("Code should handle empty string", "NTA.", Provider.MessageRecipient);
			});
		}

		public void TestOtherThingsToReport()
		{
			nctsHeader.ArrivalMovementHeader.OtherThingsToReport = "other things";
			AssertEquals("other things", Provider.OtherThingsToReport);
		}

		public void TestTraderIdentificationNumber()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;
			nctsHeader.DestinationTrader.Organisation.CustomsCodes.AddRange(Factory.CreateOrgCusCode(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "trader", Core.Constants.CountryCodes.UnitedKingdom));
			AssertEquals("GBTRADER", Provider.TraderIdentificationNumber);
		}

		public void TestTraderIdentificationNumber_With_XI_Prefix()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			nctsHeader.DestinationTrader.OrganisationPK = orgHeader.PK;
			nctsHeader.DestinationTrader.Organisation.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "XI175521246821", Core.Constants.CountryCodes.UnitedKingdom);
			AssertEquals("XI175521246821", Provider.TraderIdentificationNumber);
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

		public void TestStateOfSealsOK()
		{
			nctsHeader.ArrivalMovementHeader.BM_StateOfSealsBoolean = true;
			AssertEquals(true, Provider.StateOfSealsOK);
		}

		public void TestUnloadingdate()
		{
			nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = ZDateTimeOffset.Empty;
			AssertEquals(DateTime.MinValue, Provider.Unloadingdate);
			var date = new DateTime(1994, 2, 1, 12, 10, 9, 876);
			nctsHeader.ArrivalMovementHeader.BM_UnloadingDate = date;
			AssertEquals(date.ToString(@"yyyy-MM-dd'T'hh:mm:ss", CultureInfo.InvariantCulture), Provider.Unloadingdate.ToString(@"yyyy-MM-dd'T'hh:mm:ss", CultureInfo.InvariantCulture));
			AssertEquals("No Milliseconds", 0, Provider.Unloadingdate.Millisecond);
		}

		public void TestUnloadingRemark()
		{
			nctsHeader.ArrivalMovementHeader.BM_UnloadingRemarks = "unloading remark";
			AssertEquals("unloading remark", Provider.UnloadingRemark);
		}

		public void TestHasDeclaredSeals()
		{
			AssertEquals("Should return false as there are no sealed containers", false, Provider.HasDeclaredSeals);

			var container = nctsHeader.ArrivalMovementHeader.Header.ArrivalHeaderContainers.AddNew();
			container.SequenceNumber = 1;
			container.BC_Mode = Core.Constants.ContainerModes.Containerised;
			container.BC_ContainerNum = "TEST";
			container.BC_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			var seal = container.Seals.AddNew();
			seal.BK_SealNumber = "TEST";

			AssertEquals("Should return true as there is a declared sealed container",true, Provider.HasDeclaredSeals);

			container.BC_UnloadedState = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("Should return false as there is only a new, not-expected sealed container",false, Provider.HasDeclaredSeals);

			nctsHeader.ArrivalMovementHeader.Header.ArrivalHeaderContainers.RemoveAll();

			var incident = nctsHeader.ArrivalMovementHeader.Header.EnRouteIncidents.AddNew();
			var incContainer = incident.IncidentContainers.AddNew();
			var seal2 = incContainer.Seals.AddNew();
			seal2.BK_SealNumber = "TEST";

			AssertEquals("Should return true as there is a sealed incident container",true, Provider.HasDeclaredSeals);
		}

		public void TestConsignment()
		{
			AssertNotNull(Provider.Consignment);
		}

		protected override string MessageType => Constants.MessageTypes.CC044C;

		protected override string MovementType => NctsMovementType.Codes.Arrival;
	}
}
