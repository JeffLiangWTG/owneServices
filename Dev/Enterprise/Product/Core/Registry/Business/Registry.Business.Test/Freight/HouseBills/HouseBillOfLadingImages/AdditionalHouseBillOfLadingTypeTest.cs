using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AdditionalHouseBillOfLadingType))]
	sealed class AdditionalHouseBillOfLadingTypeTest : RegistryBusinessObjectTestCaseBase
	{
		public void TestDefaults()
		{
			var hblType = GetNewBusinessObject() as AdditionalHouseBillOfLadingType;
			Assert(!hblType.Enable);
			Assert(!hblType.EnableMessaging);
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetBusinessObjectToClone();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new AdditionalHouseBillOfLadingType(new FallbackLevel(Guid.NewGuid(), Guid.Empty, Guid.Empty))
			{
				Code = "AKT",
				Description = (NoResString)"AKT Description",
				Enable = false,
				EnableMessaging = false,
			};
		}
	}
}
