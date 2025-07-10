using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.MXManifest.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestHeader()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(bill.Header);
		}

		public void TestPacks()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaPackCollection>(bill.Packs);
		}

		public void TestGetCountryCode()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(Core.Constants.CountryCodes.Mexico, bill.GetCountryCode());
		}

		public void TestCreateNewAsycudaPackCollection()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertType<AsycudaPackCollection>(bill.CreateNewAsycudaPackCollection());
		}

		public void TestGetPackTypeCore()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(typeof(AsycudaPack), bill.GetPackTypeCore());
		}

		public void TestIfABL_GrossWeightUQChangedTotalPacksGrossWeightChange()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_GrossWeight = 5;

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Weight = 3;
			pack1.APA_WeightUQ = Core.Constants.Weight.Kilograms;

			AssertEquals("TotalPacksGrossWeight should be equal to the sum of the weights of the packs.", (ZDecimal)3, bill.TotalPacksGrossWeight);

			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Hectograms;

			AssertEquals("TotalPacksGrossWeight should be updated if ABL_GrossWeightUQ change.", (ZDecimal)30, bill.TotalPacksGrossWeight);
		}

		public void TestIfABL_VolumeUQChangedTotalPacksVolumeChange()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			bill.ABL_VolumeUQ = Core.Constants.Volume.Litre;
			bill.ABL_Volume = 5;

			var pack1 = bill.Packs.AddNew();
			pack1.APA_Volume = 3;
			pack1.APA_VolumeUQ = Core.Constants.Volume.Litre;

			AssertEquals("TotalPacksVolume should be equal to the sum of the volume of the packs.", (ZDecimal)3, bill.TotalPacksVolume);

			bill.ABL_VolumeUQ = Core.Constants.Volume.CubicCentimeters;
			AssertEquals("TotalPacksVolume should be updated if ABL_VolumeUQ change.", (ZDecimal)3000, bill.TotalPacksVolume);
		}

		public void TestBillCantBeDeletedWhenStatusIsAccepted()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "ACP";

			AssertEquals(false, bill.CanDelete);
			AssertEquals("This Bill is already sent and accepted." + System.Environment.NewLine + "If you need to cancel it from Customs, you must use the Cancel option from Manifest menu.", bill.ReasonForNotAbleToDelete);

			bill.ABL_BillStatus = "";
			AssertEquals(true, bill.CanDelete);
		}

		public void TestBillCantBeDeletedWhenStatusIsSent()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "SNT";

			AssertEquals(false, bill.CanDelete);
			AssertEquals("This Bill is already sent and it is awaiting for a response.", bill.ReasonForNotAbleToDelete);

			bill.ABL_BillStatus = "";
			AssertEquals(true, bill.CanDelete);
		}

		public void TestBillCantBeDeletedWhenStatusIsCancelled()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BillStatus = "CAN";

			AssertEquals(false, bill.CanDelete);
			AssertEquals("This Bill is canceled. Must be kept for history purposes.", bill.ReasonForNotAbleToDelete);

			bill.ABL_BillStatus = "";
			AssertEquals(true, bill.CanDelete);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			return header.Bills.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header.Bills.AddNew();
		}

		class AsycudaBillForTest : AsycudaBill
		{
			public AsycudaBillForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new ZString GetCountryCode() => base.GetCountryCode();
			public new ManifestBase.IAsycudaPackCollection<ManifestBase.AsycudaPack, ManifestBase.AsycudaBill> CreateNewAsycudaPackCollection() => base.CreateNewAsycudaPackCollection();
			public new Type GetPackTypeCore() => base.GetPackTypeCore();
		}
		#endregion
	}
}
