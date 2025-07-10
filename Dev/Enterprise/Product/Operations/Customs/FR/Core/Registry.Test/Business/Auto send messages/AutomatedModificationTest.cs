using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Registry.Testing
{
	[TestedType(typeof(AutomatedModification))]
	class AutomatedModificationTest : RegistryBusinessObjectTemplateTestCase<AutomatedModification>
	{
		protected override bool RequiresFactory => true;

		protected override bool RequiresFallbackLevel => true;

		protected override AutomatedModification GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override AutomatedModification GetBusinessObjectToSerialise()
		{
			automatedModification = new AutomatedModification();
			automatedModification.TimeByDefault = ZDateTime.Empty;
			return automatedModification;
		}

		public void TestSetEnableAutomatedValidation()
		{
			automatedModification = new AutomatedModification();
			automatedModification.TimeByDefault = ZDateTime.Today;
			automatedModification.EnableAutomatedModification = true;
			AssertEquals("Enabling automated validation should default Date by default.", ZDateTime.Today, automatedModification.TimeByDefault);

			automatedModification.EnableAutomatedModification = false;
			AssertEquals("Disabling Automated Modification should empty DateByDefault.", ZDateTime.Empty, automatedModification.TimeByDefault);
		}

		public void TestReadOnly()
		{
			automatedModification = new AutomatedModification();
			Assert(automatedModification.TimeByDefaultInfo.ReadOnly);

			automatedModification.EnableAutomatedModification = true;
			Assert(!automatedModification.TimeByDefaultInfo.ReadOnly);
		}

		public void TestDateByDefaultValidation()
		{
			automatedModification = new AutomatedModification();
			automatedModification.TimeByDefault = ZDateTime.Empty;
			AssertEquals("Empty value should be allowed.", 0, automatedModification.TimeByDefaultInfo.Notifications.Count());

			automatedModification.EnableAutomatedModification = true;
			automatedModification.TimeByDefault = ZDateTime.Empty;
			AssertHasErrorContaining("A value should be entered - error.", automatedModification.TimeByDefaultInfo, MandatoryValidation.MustBeEntered);

			automatedModification.TimeByDefault = ZDateTime.Today;
			AssertNoErrorContaining("A value should be entered - no error.", automatedModification.TimeByDefaultInfo, MandatoryValidation.MustBeEntered);
		}

		AutomatedModification automatedModification;
	}
}
