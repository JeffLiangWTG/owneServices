using System;
using Enterprise.Customs.FR.Business.NCTS;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5.Testing
{
	class InvalidationWrapperTest : Customs.Business.Testing.DataProviderTestCase<InvalidationWrapper>
	{
		[TestDate(2024, 08, 29)]
		public void TestRequestDateAndTime()
		{
			AssertEquals("Request date and time should be now.", new DateTime(2024, 08, 29), Provider.RequestDateAndTime);
		}

		public void TestJustification()
		{
			AssertEquals("Justification must be equal to sending object justification", "This is the justification", Provider.Justification);
		}

		public void TestDecisionDateAndTime()
		{
			AssertNull("Decision date and time should be null.", Provider.DecisionDateAndTime);
		}

		public void TestInitiatedByCustoms()
		{
			AssertEquals("InitiatedByCustoms should be false.", false, Provider.InitiatedByCustoms);
		}

		public void TestDecision()
		{
			AssertEquals("Decision should be false.", false, Provider.Decision);
		}

		public void TestJustificationReglementaire()
		{
			AssertEquals("JustificationReglementaire should be equal to justification code.", "1", Provider.JustificationReglementaire);
		}

		protected override InvalidationWrapper GetProvider()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.LocalReferenceNumber = "LRN001";

			var movementheader = nctsHeader.MovementHeader;
			movementheader.BM_RN_NKCountryOfDispatch = "TR";

			nctsHeader.MovementHeader.CustomsOffices.RemoveAndDeleteAll();
			var departureCustomsOffice = nctsHeader.MovementHeader.CustomsOffices.AddNew();
			departureCustomsOffice.CY_Code = "DEP";
			departureCustomsOffice.CY_Data = "FR000000";

			var sendingObject = new TP5MessageSendingObject(nctsHeader);
			sendingObject.JustificationCode = "1";
			sendingObject.Justification = "This is the justification";

			return InvalidationWrapper.New(sendingObject);
		}
	}
}
