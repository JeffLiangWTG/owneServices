using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
{
	class CLMessageChooserLookupsTest : TestCaseWithFactory
	{
		public void TestSeaAmendReasonList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "SEA";
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();

			var chooser = new CLMessageChooser(header, items, MessageSubTypeCodes.Codes.Change);

			var list = chooser.Lookups.AmendReasonList;

			AssertType<AmendReasonCodeList>(list);
			AssertEquals(13, list.Count);
		}

		public void TestAmendTypeList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();

			var chooser = new CLMessageChooser(header, items, MessageSubTypeCodes.Codes.Change);

			var list = chooser.Lookups.AmendTypeList;

			AssertType<AmendTypeCodeList>(list);
			AssertEquals(2, list.Count);
		}

		public void TestAirAmendReasonList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = "AIR";
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();

			var chooser = new CLMessageChooser(header, items, MessageSubTypeCodes.Codes.Change);

			var list = chooser.Lookups.AmendReasonList;

			AssertType<AirAmendReasonCodeList>(list);
			AssertEquals(5, list.Count);
		}
	}
}
