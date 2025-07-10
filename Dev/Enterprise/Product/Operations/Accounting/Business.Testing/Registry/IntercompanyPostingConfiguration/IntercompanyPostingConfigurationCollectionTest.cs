using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(IntercompanyPostingConfigurationCollection))]
	public class IntercompanyPostingConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<IntercompanyPostingConfigurationCollection>
	{
		public void TestAddDefaultValues()
		{
			IntercompanyPostingConfigurationCollection collection = new IntercompanyPostingConfigurationCollection();
			collection.AddDefaultValues(Env.CurrentCompany.PK);
			AssertEquals("Collection must contain single record", 1, collection.Count);
			AssertEquals("Company must be SIN", "SIN", collection[0].Company);

			collection = new IntercompanyPostingConfigurationCollection();
			collection.AddDefaultValues(Guid.Empty);
			AssertEquals("Collection must contain single record", 2, collection.Count);
			AssertEquals("Company must be SIN", "SIN", collection[0].Company);
			AssertEquals("Company must be SIN", "EDI", collection[1].Company);
		}

		#region Implementation

		protected override IntercompanyPostingConfigurationCollection GetCollectionToTest()
		{
			return new IntercompanyPostingConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new IntercompanyPostingConfiguration();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected new IntercompanyPostingConfigurationCollection Collection
		{
			get { return base.Collection; }
		}

		#endregion
	}
}
