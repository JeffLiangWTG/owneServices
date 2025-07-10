using System.Linq;
using CargoWise.Customs.CO.MessageContracts;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	class PackWrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;

		public void TestPackWrapper()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.TravelDocumentType = COWrappersConstants.ContainerNumberNeeded;

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "CAIU305178-6";

			var bill = header.Bills.AddNew();
			var pack = CreateAndPopulatePack(bill);
			pack.ContainerPK = header.Containers[0].PK;

			Factory.Save();

			IManifest wrapper = new ManifestWrapper(header, MessageSubTypeCodes.Codes.Original, header.GetValidDocumentIDs(header.Bills.Count + 1));
			var p = wrapper.Master.Houses.ElementAt(0).Item.Packs.ElementAt(0);

			CombineAssertions(() =>
			{
				AssertEquals(container.ACN_ContainerNumber, p.ContainerNumber);
				AssertEquals(1, p.PackageQty);
				AssertEquals(20m, p.Weight);
				AssertEquals(7645.54m, p.Volume);
			});

			pack = CreateAndPopulatePack(bill);
			pack.ContainerPK = header.Containers[0].PK;

			var container2 = header.Containers.AddNew();
			container2.ACN_ContainerNumber = "CAIU305178-7";

			pack = CreateAndPopulatePack(bill);
			pack.ContainerPK = header.Containers[1].PK;

			Factory.Save();

			p = wrapper.Master.Items.ElementAt(0).Packs.ElementAt(0);

			CombineAssertions(() =>
			{
				AssertEquals(container.ACN_ContainerNumber, p.ContainerNumber);
				AssertEquals(1, p.PackageQty);
				AssertEquals(20m, p.Weight);
				AssertEquals(7645.54m, p.Volume);
			});

			p = wrapper.Master.Items.ElementAt(1).Packs.ElementAt(0);

			CombineAssertions(() =>
			{
				AssertEquals(container2.ACN_ContainerNumber, p.ContainerNumber);
				AssertEquals(1, p.PackageQty);
				AssertEquals(20m, p.Weight);
				AssertEquals(7645.54m, p.Volume);
			});
		}

		AsycudaPack CreateAndPopulatePack(AsycudaBill bill)
		{
			var pack = bill.Packs.AddNew();

			pack.APA_LineNo = 1;
			pack.APA_PackQty = 1;
			pack.APA_Volume = 10000m;
			pack.APA_VolumeUQ = Core.Constants.Volume.CubicYards;
			pack.APA_Weight = 100000m;
			pack.APA_WeightUQ = Core.Constants.Weight.MetricCarat;

			return pack;
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
