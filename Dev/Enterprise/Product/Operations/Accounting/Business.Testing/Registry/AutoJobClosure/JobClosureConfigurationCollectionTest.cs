namespace Enterprise.Accounting.Business.Testing
{
	using CargoWise.EntityFramework;
	using Enterprise.Registry.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(JobClosureConfigurationCollection))]
	public class JobClosureConfigurationCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<JobClosureConfigurationCollection>
	{
		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override JobClosureConfigurationCollection GetCollectionToTest()
		{
			return new JobClosureConfigurationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new JobClosureConfiguration();
		}

		#endregion
	}
}

