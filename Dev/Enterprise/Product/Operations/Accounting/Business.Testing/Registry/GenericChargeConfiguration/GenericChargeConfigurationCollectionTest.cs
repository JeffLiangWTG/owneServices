using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(GenericChargeConfigurationCollection))]
	public class GenericChargeConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<GenericChargeConfigurationCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override GenericChargeConfigurationCollection GetCollectionToTest()
		{
			return new GenericChargeConfigurationCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GenericChargeConfiguration();
		}

		#endregion
	}
}
