using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ShareSequentialReferenceNumbers))]
	public class ShareSequentialReferenceNumbersTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ShareSequentialReferenceNumbers result = new ShareSequentialReferenceNumbers();
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

		protected new ShareSequentialReferenceNumbers BizObj
		{
			get { return (ShareSequentialReferenceNumbers)base.BizObj; }
		}

		#endregion
	}
}
