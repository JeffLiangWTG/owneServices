using CargoWise.Customs.CO.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class HeaderWrapperTest : TestCaseWithFactory
	{
		[TestDate(2022, 10, 18, 12, 00, 00)]
		public void TestHeaderWrapper()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			PopulateManifestHeader(header);

			Factory.Save();

			var validDocumentIDs = header.GetValidDocumentIDs(header.Bills.Count + 1);

			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, validDocumentIDs);
			var manifest = wrapper.Header;

			CombineAssertions(() =>
			{
				AssertEquals("2022", manifest.Year);
				AssertEquals(true, manifest.IsFirstDownload);
				AssertEquals(1, manifest.ShippingNumber);
				AssertEquals("18/10/2022", manifest.ShippingDate.ToShortDateString());
				AssertEquals("1/01/2022", manifest.InitialDate.ToShortDateString());
				AssertEquals("31/12/2022", manifest.FinalDate.ToShortDateString());
				AssertEquals((double)1, manifest.TotalValue);
			});

			wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Change, validDocumentIDs);
			manifest = wrapper.Header;

			CombineAssertions(() =>
			{
				AssertEquals(false, manifest.IsFirstDownload);
				AssertEquals(1, manifest.ShippingNumber);
			});
		}

		void PopulateManifestHeader(AsycudaManifestHeader header)
		{
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.TravelDocumentType = "1";

			CreateAndPopulateHouseBill("BILL1", header);
			CreateAndPopulateHouseBill("BILL2", header);
			CreateAndPopulateHouseBill("BILL3", header);
		}

		void CreateAndPopulateHouseBill(ZString billNumber, AsycudaManifestHeader header)
		{
			var bill = header.Bills.AddNew();
			bill.ABL_BillNumber = billNumber;
		}

		protected override void SetUp()
		{
			base.SetUp();

			var cusTransactionNumber = Factory.NewWithValidTestData<CusTransactionNumber>();
			cusTransactionNumber.TN_TransactionReference = "11667803932049";
			cusTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			cusTransactionNumber.TN_GC_Company = GlbCompany.CurrentCompany.PK;
		}
	}
}
