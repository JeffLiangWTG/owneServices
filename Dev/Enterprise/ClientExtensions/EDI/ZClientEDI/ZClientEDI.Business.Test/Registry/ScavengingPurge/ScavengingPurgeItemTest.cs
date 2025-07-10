using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ScavengingPurgeItem))]
	class ScavengingPurgeItemTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestPurgeTime()
		{
			var settings = (ScavengingPurgeItem)GetBusinessObjectToClone();
			settings.PurgeTime = 0;
			AssertHasError(settings.PurgeTimeInfo, "1 week is the minimum allowed.");

			settings.PurgeTime = 235;
			AssertNoErrors(settings.PurgeTimeInfo);
		}

		public void TestPurgeTimeUnit()
		{
			var settings = (ScavengingPurgeItem)GetBusinessObjectToClone();
			settings.PurgeTimeUnit = new ZGuid();
			AssertHasError(settings.PurgeTimeUnitInfo, "Please enter a value.");

			settings.PurgeTimeUnit = new ZGuid("e1119f02-9992-48e9-b9ca-8019f2e0be9e");
			AssertHasError(settings.PurgeTimeUnitInfo, "Enter a valid selection.");

			settings.PurgeTimeUnit = ScavengingPurgeItem.TimeUnit.Month;
			AssertNoErrors(settings.PurgeTimeUnitInfo);
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new ScavengingPurgeItem
			{
				Code = "XYZ",
				Description = (NoResString)"Scavenging Item Description",
				PurgeTime = 10,
				PurgeTimeUnit = ScavengingPurgeItem.TimeUnit.Month
			};
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
