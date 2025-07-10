using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(TrackAndTraceNotificationsRule))]
	sealed class TrackAndTraceNotificationsRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("NotifySenderOnSuccessOrAcknowledgement", true, Rule.NotifySenderOnSuccessOrAcknowledgement);
			AssertEquals("NotifySenderOnError", true, Rule.NotifySenderOnError);
			AssertEquals("NotifySenderOnDiscrepancy", true, Rule.NotifySenderOnDiscrepancy);
			AssertEquals("NotifyGroupOnSuccessOrAcknowledgement", false, Rule.NotifyGroupOnSuccessOrAcknowledgement);
			AssertEquals("NotifyGroupOnError", false, Rule.NotifyGroupOnError);
			AssertEquals("NotifyGroupOnDiscrepancy", false, Rule.NotifyGroupOnDiscrepancy);
			AssertEquals("GroupForSuccessOrAcknowledgement", ZGuid.Empty, Rule.GroupForSuccessOrAcknowledgement);
			AssertEquals("GroupForErrorsAndDiscrepancies", ZGuid.Empty, Rule.GroupForErrorsAndDiscrepancies);

			AssertNotNull(Rule.VisibilityProvider);
			AssertEquals(true, Rule.VisibilityProvider.NotifyGroupOnDiscrepancyEnabled);
			AssertEquals(true, Rule.VisibilityProvider.NotifySenderOnDiscrepancyEnabled);
			AssertEquals(true, Rule.VisibilityProvider.NotifySenderOnErrorEnabled);
			AssertEquals(true, Rule.VisibilityProvider.NotifySenderOnSuccessOrAcknowledgementEnabled);
		}

		public void TestListValidation()
		{
			Rule.RunPreSaveValidation();
			AssertNoErrors(Rule);

			Rule.GroupForSuccessOrAcknowledgement = ZGuid.Invalid;
			Rule.GroupForErrorsAndDiscrepancies = ZGuid.Invalid;
			Rule.RunPreSaveValidation();
			AssertHasErrors(Rule.GroupForSuccessOrAcknowledgementInfo);
			AssertHasErrors(Rule.GroupForErrorsAndDiscrepanciesInfo);

			Rule.GroupForSuccessOrAcknowledgement = ZGuid.NewZGuid();
			Rule.GroupForErrorsAndDiscrepancies = ZGuid.NewZGuid();
			Rule.RunPreSaveValidation();
			AssertHasErrors(Rule.GroupForSuccessOrAcknowledgementInfo);
			AssertHasErrors(Rule.GroupForErrorsAndDiscrepanciesInfo);

			Rule.GroupForSuccessOrAcknowledgement = ZGuid.Empty;
			Rule.GroupForErrorsAndDiscrepancies = ZGuid.Empty;
			Rule.RunPreSaveValidation();
			AssertNoErrors(Rule);
		}

		public void TestMandatoryValidation()
		{
			Rule.RunPreSaveValidation();
			AssertNoErrors(Rule);

			Rule.NotifyGroupOnSuccessOrAcknowledgement = true;
			Rule.GroupForSuccessOrAcknowledgement = ZGuid.Empty;
			Rule.GroupForErrorsAndDiscrepancies = ZGuid.Empty;
			Rule.RunPreSaveValidation();
			AssertHasErrors(Rule.GroupForSuccessOrAcknowledgementInfo);
			AssertNoErrors(Rule.GroupForErrorsAndDiscrepanciesInfo);

			Rule.NotifyGroupOnError = true;
			Rule.RunPreSaveValidation();
			AssertHasErrors(Rule.GroupForSuccessOrAcknowledgementInfo);
			AssertHasErrors(Rule.GroupForErrorsAndDiscrepanciesInfo);

			Rule.NotifyGroupOnSuccessOrAcknowledgement = false;
			Rule.NotifyGroupOnError = false;
			Rule.RunPreSaveValidation();
			AssertNoErrors(Rule);
		}

		public void TestReadOnlyProperties()
		{
			AssertEquals(true, Rule.GroupForErrorsAndDiscrepanciesInfo.ReadOnly);
			AssertEquals(true, Rule.GroupForSuccessOrAcknowledgementInfo.ReadOnly);

			Rule.NotifyGroupOnSuccessOrAcknowledgement = true;
			AssertEquals(true, Rule.GroupForErrorsAndDiscrepanciesInfo.ReadOnly);
			AssertEquals(false, Rule.GroupForSuccessOrAcknowledgementInfo.ReadOnly);

			Rule.NotifyGroupOnError = true;
			AssertEquals(false, Rule.GroupForErrorsAndDiscrepanciesInfo.ReadOnly);
			AssertEquals(false, Rule.GroupForSuccessOrAcknowledgementInfo.ReadOnly);

			Rule.NotifyGroupOnError = false;
			Rule.NotifyGroupOnDiscrepancy = true;
			AssertEquals(false, Rule.GroupForErrorsAndDiscrepanciesInfo.ReadOnly);
			AssertEquals(false, Rule.GroupForSuccessOrAcknowledgementInfo.ReadOnly);

			Rule.NotifyGroupOnSuccessOrAcknowledgement = false;
			Rule.NotifyGroupOnError = false;
			Rule.NotifyGroupOnDiscrepancy = false;
			AssertEquals(true, Rule.GroupForErrorsAndDiscrepanciesInfo.ReadOnly);
			AssertEquals(true, Rule.GroupForSuccessOrAcknowledgementInfo.ReadOnly);
		}

		public void TestReadOnlyPropertiesAreResetToDefault()
		{
			AssertEquals(ZGuid.Empty, Rule.GroupForErrorsAndDiscrepancies);
			AssertEquals(ZGuid.Empty, Rule.GroupForSuccessOrAcknowledgement);

			Rule.NotifyGroupOnSuccessOrAcknowledgement = true;
			Rule.NotifyGroupOnError = true;
			Rule.NotifyGroupOnDiscrepancy = true;
			ZGuid groupACK = ZGuid.NewZGuid();
			ZGuid groupERR = ZGuid.NewZGuid();

			Rule.GroupForSuccessOrAcknowledgement = groupACK;
			Rule.GroupForErrorsAndDiscrepancies = groupERR;

			AssertEquals(groupERR, Rule.GroupForErrorsAndDiscrepancies);
			AssertEquals(groupACK, Rule.GroupForSuccessOrAcknowledgement);

			Rule.NotifyGroupOnSuccessOrAcknowledgement = false;
			AssertEquals(groupERR, Rule.GroupForErrorsAndDiscrepancies);
			AssertEquals(ZGuid.Empty, Rule.GroupForSuccessOrAcknowledgement);

			Rule.NotifyGroupOnError = false;
			AssertEquals(groupERR, Rule.GroupForErrorsAndDiscrepancies);
			AssertEquals(ZGuid.Empty, Rule.GroupForSuccessOrAcknowledgement);

			Rule.NotifyGroupOnDiscrepancy = false;
			AssertEquals(ZGuid.Empty, Rule.GroupForErrorsAndDiscrepancies);
			AssertEquals(ZGuid.Empty, Rule.GroupForSuccessOrAcknowledgement);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new TrackAndTraceNotificationsRule();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new TrackAndTraceNotificationsRule();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		TrackAndTraceNotificationsRule Rule
		{
			get { return rule ?? (rule = new TrackAndTraceNotificationsRule(new TrackAndTraceNotificationsRuleVisibilityProvider())); }
		}
		TrackAndTraceNotificationsRule rule;

		#endregion
	}
}
