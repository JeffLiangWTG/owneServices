using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(CrossTradeDebtorConfigurationHeader))]
	public class CrossTradeDebtorConfigurationHeaderTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var result = new CrossTradeDebtorConfigurationHeader();
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

		protected new CrossTradeDebtorConfigurationHeader BizObj
		{
			get { return (CrossTradeDebtorConfigurationHeader)base.BizObj; }
		}

		#endregion
	}
}
