using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(InvalidationType02Provider))]
	sealed class InvalidationType02ProviderTest : DataProviderTestCase<InvalidationType02Provider>
	{
		[TestDate(2023, 09, 05, 11, 00, 00, 789)]
		public void TestRequestDateAndTime()
		{
			AssertEquals(new DateTime(2023, 09, 05, 11, 00, 00), Provider.RequestDateAndTime);
			AssertEquals("No Milliseconds", 0, ((DateTime)Provider.RequestDateAndTime).Millisecond);
		}

		public void TestDecisionDateAndTime()
		{
			AssertNull(provider.DecisionDateAndTime);
		}

		public void TestDecision()
		{
			AssertNull(provider.Decision);
		}

		public void TestInitiatedByCustoms()
		{
			AssertEquals(expected: true, provider.InitiatedByCustoms);
			header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.MrnAllocated;
			AssertEquals(expected: false, provider.InitiatedByCustoms);
		}

		public void TestJustification()
		{
			AssertEquals("Justification", "I changed my mind", Provider.Justification);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Departure);
			header.MovementHeader.BM_CustomsStatus = NCTS5DepartureCustomsStatusList.Codes.CancellationRequestedByCustoms;
			header.ExplanationToCustomsForWhyCancelling = justification;

			provider = new InvalidationType02Provider(header, justification);
		}

		InvalidationType02Provider provider;
		NctsHeader header;
		readonly string justification = "I changed my mind";

		protected override InvalidationType02Provider GetProvider() => provider;
	}
}
