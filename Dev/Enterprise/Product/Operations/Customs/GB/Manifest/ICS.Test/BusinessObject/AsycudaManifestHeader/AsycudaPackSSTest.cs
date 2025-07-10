using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.ICS.Business.Testing
{
	[TestedType(typeof(AsycudaPackSS))]
	public class AsycudaPackSSTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.New<AsycudaManifestHeaderSS>();
			header.SuspendCheckBusinessObjectType();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			return pack;
		}
	}
}
