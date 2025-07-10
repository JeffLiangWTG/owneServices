using System;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(IntercompanyClearingConfigurationCollection))]
	public class IntercompanyClearingConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<IntercompanyClearingConfigurationCollection>
	{
		public void TestAddDefaultValues()
		{
			IntercompanyClearingConfigurationCollection collection = new IntercompanyClearingConfigurationCollection();
			collection.AddDefaultValues(Guid.Empty);
			AssertEquals("Collection must contain two records", 2, collection.Count);
			AssertEquals("Company must be SIN", "SIN", collection[0].Company);
			AssertEquals("Company must be EDI", "EDI", collection[1].Company);
		}

		#region Implementation

		protected override IntercompanyClearingConfigurationCollection GetCollectionToTest()
		{
			return new IntercompanyClearingConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IntercompanyClearingConfiguration();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new IntercompanyClearingConfigurationCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}
