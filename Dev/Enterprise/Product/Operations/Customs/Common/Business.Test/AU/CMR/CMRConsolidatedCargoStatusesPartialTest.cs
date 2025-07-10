using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.Shared;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Common.AU.CMR.Testing
{
	class CMRConsolidatedCargoStatusesTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestAllClearStatus()
		{
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.AllClearStatus.ContainsCode(CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased), Is.True);
		}

		[ExpectNoExceptions]
		public void TestIsCargoAbbreviatedStatusDescriptionRequiredFor()
		{
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.SeePackingDetails), Is.True);
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms), Is.True);
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine), Is.True);
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.DclallowedCargoMayBeDeconsolidatedThisStatusValueOnlyAppliesToAirCargo), Is.True);
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl), Is.True);
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed), Is.True);
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus), Is.True);
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement), Is.True);
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoAbbreviatedStatusDescriptionRequiredFor(CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn), Is.True);
		}

		[ExpectNoExceptions]
		public void TestGetShortDescriptionFromCode()
		{
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.Acsseized).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.Aqisseized).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.Clear).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.Clearhrm).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.Condclear).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.DclallowedCargoMayBeDeconsolidatedThisStatusValueOnlyAppliesToAirCargo), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.Dclallowed).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.Sububmov).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.Tranship).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.Held).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.SeePackingDetails), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.SeePackingDetails).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.Transhphrm).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.Transit).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.GetShortDescriptionFromCode(CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn), Is.EqualTo(CMRConsolidatedCargoStatuses.ShortDescriptions.Withdrawn).Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestIsCargoStatusClearFor()
		{
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased), Is.True);
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement), Is.True);
			NUnit.Framework.Assert.That(CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.SeePackingDetails), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.DclallowedCargoMayBeDeconsolidatedThisStatusValueOnlyAppliesToAirCargo), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction), Is.True);
			NUnit.Framework.Assert.That(!CMRConsolidatedCargoStatuses.IsCargoStatusClearFor(CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn), Is.True);
		}

		public void ConvertCustomsStatusReasonCodeToGenericCustomsStatus()
		{
			var helper = new CMRConsolidatedCargoStatuses();
			NUnit.Framework.Assert.That(helper, Is.Not.EqualTo(default(CMRConsolidatedCargoStatuses)));

			var customsStatus = helper.ConvertCustomsStatusReasonCodeToGenericCustomsStatus(CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement);
			NUnit.Framework.Assert.That(customsStatus, Is.EqualTo(CustomsStatusCodeList.Codes.Cleared).Using(CustomComparers.TypeComparison));
		}

		public void ConvertCustomsStatusReasonCodeToGenericCustomsStatus_InvalidReasonCode()
		{
			var helper = new CMRConsolidatedCargoStatuses();
			NUnit.Framework.Assert.That(helper, Is.Not.EqualTo(default(CMRConsolidatedCargoStatuses)));

			var customsStatus = helper.ConvertCustomsStatusReasonCodeToGenericCustomsStatus("BLA");
			NUnit.Framework.Assert.That(customsStatus, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison));
		}

		public void GetCustomsStatusReasonDescription()
		{
			var helper = new CMRConsolidatedCargoStatuses();
			NUnit.Framework.Assert.That(helper, Is.Not.EqualTo(default(CMRConsolidatedCargoStatuses)));

			var customsStatusReasonDescription = helper.GetCustomsStatusReasonDescription(CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus);
			NUnit.Framework.Assert.That(customsStatusReasonDescription, Is.EqualTo(CMRConsolidatedCargoStatuses.Descriptions.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus).Using(CustomComparers.TypeComparison));
		}

		public void GetCustomsStatusReasonDescription_InvalidReasonCode()
		{
			var helper = new CMRConsolidatedCargoStatuses();
			NUnit.Framework.Assert.That(helper, Is.Not.EqualTo(default(CMRConsolidatedCargoStatuses)));

			var customsStatusReasonDescription = helper.GetCustomsStatusReasonDescription("BLA");
			NUnit.Framework.Assert.That(customsStatusReasonDescription, Is.EqualTo(string.Empty).Using(CustomComparers.TypeComparison));
		}
	}
}
