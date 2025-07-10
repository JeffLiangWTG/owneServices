using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(IntercompanyClearingConfiguration))]
	public class IntercompanyClearingConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			IntercompanyClearingConfiguration result = new IntercompanyClearingConfiguration();

			result.Company = "EDI";
			result.ClearingGLAccount = result.GLAccounts[0].PK;

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
			get { return false; }
		}

		protected new IntercompanyClearingConfiguration BizObj
		{
			get { return (IntercompanyClearingConfiguration)base.BizObj; }
		}

		protected virtual IntercompanyClearingConfigurationCollection GetAuthorisationSettingsCollection()
		{
			return new IntercompanyClearingConfigurationCollection();
		}

		#endregion
	}
}
