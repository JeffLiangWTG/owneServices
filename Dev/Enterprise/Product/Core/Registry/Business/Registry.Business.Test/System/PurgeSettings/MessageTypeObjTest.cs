using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(MessageTypeObj))]
	sealed class MessageTypeObjTest : RegistryBusinessObjectTemplateTestCase<MessageTypeObj>
	{
		public void TestPurgeTime()
		{
			var settings = GetBusinessObjectToClone();
			settings.PurgeTime = 0;
			AssertHasError(settings.PurgeTimeInfo, "1 week is the minimum allowed.");

			settings.PurgeTime = 235;
			AssertNoErrors(settings.PurgeTimeInfo);
		}

		public void TestPurgeTimeUnit()
		{
			var settings = GetBusinessObjectToClone();
			settings.PurgeTimeUnit = new ZGuid();
			AssertHasError(settings.PurgeTimeUnitInfo, "Please enter a value.");

			settings.PurgeTimeUnit = new ZGuid("e1119f02-9992-48e9-b9ca-8019f2e0be9e");
			AssertHasError(settings.PurgeTimeUnitInfo, "Enter a valid selection.");

			settings.PurgeTimeUnit = TimeUnit.Month;
			AssertNoErrors(settings.PurgeTimeUnitInfo);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override MessageTypeObj GetBusinessObjectToClone()
		{
			return (MessageTypeObj)GetNewBusinessObject();
		}

		protected override MessageTypeObj GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MessageTypeObj();
		}
	}
}
