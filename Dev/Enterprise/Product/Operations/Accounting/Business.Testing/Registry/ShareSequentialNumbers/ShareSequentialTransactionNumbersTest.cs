using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ShareSequentialTransactionNumbers))]
	public class ShareSequentialTransactionNumbersTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ShareSequentialTransactionNumbers result = new ShareSequentialTransactionNumbers();
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

		protected new ShareSequentialTransactionNumbers BizObj
		{
			get { return (ShareSequentialTransactionNumbers)base.BizObj; }
		}

		#endregion
	}
}
