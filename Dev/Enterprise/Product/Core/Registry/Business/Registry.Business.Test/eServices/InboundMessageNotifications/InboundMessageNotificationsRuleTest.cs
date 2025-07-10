using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.eServices;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(InboundMessageNotificationsRule))]
	sealed class InboundMessageNotificationsRuleTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestDefaultValues()
		{
			AssertEquals("NotifyGroupWhenUserFound", true, Rule.NotifyGroupWhenUserFound);
			AssertEquals("NotifyUserType", InboundMessageNotificationsNotifyUserTypeList.Codes.None, Rule.NotifyUserType);
			AssertEquals("NotifyGroup", ZGuid.Empty, Rule.NotifyGroup);
		}

		public void TestIsEmpty()
		{
			AssertEquals(true, Rule.IsEmpty);
		}

		public void TestValidateNotifyUsers()
		{
			Rule.NotifyUserType = InboundMessageNotificationsNotifyUserTypeList.Codes.AllUsersEditing;
			ZPropertyInfo info = Rule.NotifyUserTypeInfo;
			Assert("should not contain any notification", !info.HasNotifications());

			Rule.NotifyUserType = "XXX";
			info = Rule.NotifyUserTypeInfo;
			Assert("must contain notification which is also an error", info.HasNotifications() && info.HasErrors());
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new InboundMessageNotificationsRule();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return new InboundMessageNotificationsRule();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		InboundMessageNotificationsRule Rule
		{
			get { return rule ?? (rule = new InboundMessageNotificationsRule()); }
		}
		InboundMessageNotificationsRule rule;

		#endregion
	}
}
