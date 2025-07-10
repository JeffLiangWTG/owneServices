using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(OrgBarcodeMaskCollection))]
	sealed class TestOrgBarcodeMaskCollection : RegistryBusinessObjectCollectionTemplateTestCase<OrgBarcodeMaskCollection>
	{
		protected override OrgBarcodeMaskCollection GetCollectionToTest()
		{
			return BarcodeCollection;
		}

		OrgBarcodeMaskCollection barcodeCollection;

		OrgBarcodeMaskCollection BarcodeCollection
		{
			get
			{
				if (barcodeCollection == null)
				{
					barcodeCollection = new OrgBarcodeMaskCollection();
				}
				return barcodeCollection;
			}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new OrgBarcodeMask(Factory, Collection);
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}
	}
}
