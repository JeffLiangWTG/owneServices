using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(CASSFileImportDefaultTaxID))]
	public class CASSFileImportDefaultTaxIDTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new CASSFileImportDefaultTaxID();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new CASSFileImportDefaultTaxID();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}
	}
}
