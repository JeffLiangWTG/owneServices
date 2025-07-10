using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(PartyComplianceWrapperFilteredCollection))]
	public class PartyComplianceWrapperFilteredCollectionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestComplianceViewPartyStatusWrapperFilteredCollection()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var unknownHeader = Factory.NewWithValidTestData<OrgHeader>();
			unknownHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			var unknownParty = new ScreeningParty(parent, "", unknownHeader);

			var notScreenedHeader = Factory.NewWithValidTestData<OrgHeader>();
			notScreenedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			var notScreenedParty = new ScreeningParty(parent, "", notScreenedHeader);

			var canceledHeader = Factory.NewWithValidTestData<OrgHeader>();
			canceledHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Canceled;
			var canceledParty = new ScreeningParty(parent, "", canceledHeader);

			var clearHeader = Factory.NewWithValidTestData<OrgHeader>();
			clearHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			var clearParty = new ScreeningParty(parent, "", clearHeader);

			var matchedHeader = Factory.NewWithValidTestData<OrgHeader>();
			matchedHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
			var matchedParty = new ScreeningParty(parent, "", matchedHeader);

			var screeningParties = new[]
			{
				unknownParty, notScreenedParty, canceledParty, clearParty, matchedParty
			};
			var collections = new PartyComplianceWrapperFilteredCollection(screeningParties);
			var expectedProcessedParties = new[] { canceledParty, clearParty, matchedParty };
			var expectedUnProcessedParties = new[] { unknownParty, notScreenedParty };

			CombineAssertions(() =>
			{
				AssertEquals(expectedProcessedParties.Length, collections.ProcessedParties.Count);
				AssertContainsExactElementsInAnyOrder(expectedProcessedParties.Select(party => party.Key), collections.ProcessedParties.Cast<PartyComplianceWrapper>().Select(party => party.Key));

				AssertEquals(expectedUnProcessedParties.Length, collections.UnprocessedParties.Count);
				AssertContainsExactElementsInAnyOrder(expectedUnProcessedParties.Select(party => party.Key), collections.UnprocessedParties.Cast<PartyComplianceWrapper>().Select(party => party.Key));

				AssertEquals(false, unknownParty.IsCurrentScreeningStatusValid);
				AssertEquals(false, notScreenedParty.IsCurrentScreeningStatusValid);
				AssertEquals(true, canceledParty.IsCurrentScreeningStatusValid);
				AssertEquals(true, clearParty.IsCurrentScreeningStatusValid);
				AssertEquals(true, matchedParty.IsCurrentScreeningStatusValid);
			});
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var parent = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var party = new ScreeningParty(parent, "", header);

			return new PartyComplianceWrapperFilteredCollection(new[] { party });
		}
	}
}
