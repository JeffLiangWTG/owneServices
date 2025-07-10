using System.ComponentModel;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class StorageMainComparerTest : TestCaseWithDocumentFactory
	{
		public void TestIsTopLevelParentIsAlwaysTop()
		{
			StorageMain x = MasterFactory.New<StorageMain>();
			x.SM_Type = Core.Constants.DocManagerCodes.Shipment;
			x.IsTopLevelParent = true;

			StorageMain y = MasterFactory.New<StorageMain>();
			y.SM_Type = Core.Constants.DocManagerCodes.Organisation;

			StorageMain z = MasterFactory.New<StorageMain>();
			z.SM_Type = "UNA";

			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(x);
			AssertEquals("Properties count should be greater than 0", true, properties.Count > 0);
			PropertyDescriptor descriptor = properties[StorageMainSchema.Constants.SM_Type];
			StorageMainComparer comparer = new StorageMainComparer(descriptor, ListSortDirection.Ascending);

			Assert("Top level parent should be ranked above all others - pass in as first param", comparer.Compare(x, y) < 0);
			Assert("Top level parent should be ranked above all others - pass in as second param", comparer.Compare(y, x) > 0);
		}
	}
}
