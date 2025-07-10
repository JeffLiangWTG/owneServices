namespace Enterprise.Accounting.Business.Testing
{
	using Enterprise.Registry.Business;
	using Enterprise.Registry.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(JobClosureConfigurationHeader))]
	public class JobClosureConfigurationHeaderTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new JobClosureConfigurationHeader();
			return result;
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
			get { return true; }
		}

		protected new JobClosureConfigurationHeader BizObj
		{
			get { return (JobClosureConfigurationHeader)base.BizObj; }
		}

		#endregion
	}
}
