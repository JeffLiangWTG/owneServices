using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Registry.Testing
{
	[TestedType(typeof(TransactionsTypesToExportBusinessObject))]
	internal class TransactionsTypesToExportBusinessObjectTest : RegistryBusinessObjectTemplateTestCase
	{
		#region Overrides
		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return GetBusinessObjectToSerialise();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return BizObj;
		}

		protected override bool RequiresFactory
		{
			get
			{
				return false;
			}
		}

		protected override bool RequiresFallbackLevel
		{
			get
			{
				return false;
			}
		}

		protected new TransactionsTypesToExportBusinessObject BizObj
		{
			get
			{
				return (TransactionsTypesToExportBusinessObject)base.BizObj;
			}
		}
		#endregion
	}
}
