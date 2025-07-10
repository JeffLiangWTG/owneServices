using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	[TestedType(typeof(UPEPrintBatchCollection))]
	public class UPEPrintBatchCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override void SetUp()
		{
			UPEDataRegistry.Instance.EnableUPECustomisations = true;
			base.SetUp();
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new UPEPrintBatchCollection(Factory);
		}
	}
}
