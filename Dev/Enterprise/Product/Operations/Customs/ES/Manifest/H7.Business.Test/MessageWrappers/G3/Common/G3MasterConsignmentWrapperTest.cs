using System.Linq;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	[TestsSubclassesOf(typeof(G3MasterConsignmentWrapper))]
	public abstract class G3MasterConsignmentWrapperTest<TWrapper> : DataProviderTestCase<TWrapper> where TWrapper : G3MasterConsignmentWrapper
	{
		public virtual void TestPreviousDocument()
		{
			header.AMA_MasterInformation = "MI0003";

			CombineAssertions(() =>
			{
				var previousDocument = Provider.PreviousDocument;
				AssertNotNull("Expected filled Previous Document list", previousDocument);
				AssertEquals("Expected one filled Previous Document item", 1, previousDocument.Count);
				AssertEquals("Expected filled Previous Document Name", "337", previousDocument.FirstOrDefault().Name);
				AssertEquals("Expected filled Previous Document Number", "MI0003", previousDocument.FirstOrDefault().Number);

				AssertSame("Cached Previous Document", previousDocument, Provider.PreviousDocument);
			});
		}

		public void TestTransportDocument()
		{
			header.TransportDocumentType = "Doc";
			header.TransportDocumentReference = "12345";

			CombineAssertions(() =>
			{
				var transportDocument = Provider.TransportDocument;
				AssertNotNull("Expected filled Transport Document list", transportDocument);
				AssertEquals("Expected filled Transport Document name", "Doc", transportDocument.Name);
				AssertEquals("Expected filled Transport Document number", "12345", transportDocument.Number);

				AssertSame("Cached Transport Document", transportDocument, Provider.TransportDocument);
			});
		}

		public void TestReceptacle()
		{
			AssertEquals("Expected filled Receptacle", "12345", Provider.Receptacle);
		}

		public void TestLocationOfGoods()
		{
			AssertNullOrEmpty("Pre-condition: Location of Goods has no authorization number", header.CusGoodsLocation.Address.AuthorisationNumber);
			AssertNull("Location of Goods should be null when there is no authorization number", Provider.LocationOfGoods);

			header.CusGoodsLocation.Address.AuthorisationNumber = "12345";
			var reloadWrapper = GetProviderCore();

			CombineAssertions("Location of Goods is added when there authorization number is filled", () =>
			{
				var locationOfGoods = reloadWrapper.LocationOfGoods;
				AssertNotNull("Expected filled Location Of Goods", locationOfGoods);
				AssertEquals("B", locationOfGoods.Type);
				AssertEquals("Y", locationOfGoods.Qualifier);
				AssertEquals("12345", locationOfGoods.Coded.AuthorisationNumber);

				AssertSame("Cached Location Of Goods", locationOfGoods, reloadWrapper.LocationOfGoods);
			});
		}

		public void TestTransportEquipmentContainers()
		{
			AssertEquals("Expected empty Transport Equipment Containers", 0, Provider.TransportEquipmentContainers.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();

			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.MasterBill.ABL_BillNumber = "12345";
			header.Bills.AddNew();
			header.Bills.AddNew();
		}

		protected abstract TWrapper GetProviderCore();

		protected sealed override TWrapper GetProvider()
		{
			return GetProviderCore();
		}

		protected AsycudaManifestHeader header;
	}
}
