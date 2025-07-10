using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Manifest.Business.Testing
{
	[TestedType(typeof(IcsOfficeCodeCollection))]
	public class IcsOfficeCodeCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var parent = Factory.New<AsycudaManifestHeader>();
			return new IcsOfficeCodeCollection(parent);
		}
	}
}
