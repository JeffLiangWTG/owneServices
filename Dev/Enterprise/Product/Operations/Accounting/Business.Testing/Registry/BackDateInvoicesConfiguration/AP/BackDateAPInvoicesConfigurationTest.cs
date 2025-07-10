using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(BackDateAPInvoicesConfiguration))]
	public class BackDateAPInvoicesConfigurationTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			BackDateAPInvoicesConfiguration result = new BackDateAPInvoicesConfiguration();

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

		protected new BackDateAPInvoicesConfiguration BizObj
		{
			get { return (BackDateAPInvoicesConfiguration)base.BizObj; }
		}

		#endregion

	}
}
