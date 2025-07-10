namespace Enterprise.Accounting.Business.Testing
{
	using Enterprise.Accounting.Business;
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.Integration;
	using Enterprise.Registry.Business.Testing;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Environment.Testing;
	using NUnit.Framework;

	[TestedType(typeof(JobClosureConfigurationRegistryItem))]
	public class JobClosureConfigurationRegistryItemTest : StronglyTypedRegistryItemTestCase<JobClosureConfigurationHeader>
	{
		protected override StronglyTypedRegistryItem<JobClosureConfigurationHeader, JobClosureConfigurationHeader> GetNewRegistryItem()
		{
			return new JobClosureConfigurationRegistryItem("", null, null, null, RegistryStorageFlags.System, RegistryOptions.Default, new JobClosureConfigurationHeader());
		}
	}

	[TestedType(typeof(JobClosureConfigurationRegistryDataType))]
	public class JobClosureConfigurationRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<JobClosureConfigurationRegistryDataType>
	{
		#region Implementation

		protected override JobClosureConfigurationRegistryDataType GetNewDataType()
		{
			return new JobClosureConfigurationRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "JobClosureConfigurationRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			JobClosureConfigurationHeader copy = new JobClosureConfigurationHeader();

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(copy, DataType.Serialise(copy))
			};
		}

		#endregion
	}
}
