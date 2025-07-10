using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Registry.Testing
{
	[TestedType(typeof(TransactionsTypesToExportRegistryItem))]
	internal class TransactionsTypesToExportRegistryItemTest : StronglyTypedRegistryItemTestCase<TransactionsTypesToExportBusinessObject>
	{
		[TestDate(2006, 1, 1, 1, 1, 50)]
		public override void TestCasting()
		{
			base.TestCasting();
		}

		protected override TransactionsTypesToExportBusinessObject ValidValue
		{
			get
			{
				TransactionsTypesToExportBusinessObject bizObj = new TransactionsTypesToExportBusinessObject();
				bizObj.ARAdjustmentNote = true;
				bizObj.ARCreditNote = true;
				bizObj.ARInvoice = true;
				bizObj.ARNonJobRelated = true;
				bizObj.ARJobRelated = false;
				return bizObj;
			}
		}

		protected override StronglyTypedRegistryItem<TransactionsTypesToExportBusinessObject, TransactionsTypesToExportBusinessObject> GetNewRegistryItem()
		{
			return new TransactionsTypesToExportRegistryItem("", "", "", "", RegistryStorageFlags.System);
		}
	}
}
