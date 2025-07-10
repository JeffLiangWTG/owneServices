using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Customs.Testing
{
	[TestedType(typeof(GroupNotification))]
	sealed class GroupNotificationTest : RegistryBusinessObjectTemplateTestCase<GroupNotification>
	{
		public void TestValidateSendMode()
		{
			var notif = new GroupNotification();
			notif.SendMode = "XXX";
			AssertHasErrorContaining(notif.SendModeInfo, ListValidation.InvalidCodeError);

			notif.SendMode = Core.Constants.EmailTo.NominatedGroup;
			AssertNoErrorContaining(notif.SendModeInfo, ListValidation.InvalidCodeError);

			notif.SendMode = ZString.Empty;
			AssertHasErrorContaining(notif.SendModeInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestValidateGroupPK()
		{
			var notif = new GroupNotification();
			notif.SendMode = Core.Constants.EmailTo.StaffMemberAndNominatedGroup;
			notif.SendGroupPK = ZGuid.Empty;
			AssertHasErrorContaining(notif.SendGroupPKInfo, MandatoryValidation.MustBeEntered);

			notif.SendGroupPK = new Guid("5D5E824D-9CCD-48D5-856E-979DCA24521E");
			AssertHasErrorContaining(notif.SendGroupPKInfo, ListValidation.InvalidCodeError);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override GroupNotification GetBusinessObjectToClone()
		{
			return new GroupNotification();
		}

		protected override GroupNotification GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}
	}
}
