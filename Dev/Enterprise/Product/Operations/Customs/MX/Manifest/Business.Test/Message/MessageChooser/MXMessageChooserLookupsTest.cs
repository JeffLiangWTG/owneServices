using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.MX.Manifest.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	class MXMessageChooserLookupsTest : TestCaseWithFactory
	{
		public void TestReasonList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();

			var chooser = new MXMessageChooser(header, items, MessageSubTypeCodes.Codes.Original);

			var list = chooser.Lookups.ReasonList;
			var cachedList = chooser.Lookups.ReasonList;

			AssertType<SEAReasonCodes>(list);
			AssertSame("Should be cached.", cachedList, list);
		}
	}
}
