using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaBillCollection>(header.Bills);
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = GetNewBusinessObject();
			bizObj.FillWithValidTestData();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.COManifest.IAsycudaManifestHeader>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestDefaultGetTypes()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Colombia, COManifestTypes.Codes.MAN);
			CombineAssertions(() =>
			{
				AssertEquals("Default Bill Type", typeof(AsycudaBill), header.GetBillType());
				AssertEquals("Default Container Type", typeof(AsycudaContainer), header.GetContainerType());
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_JobReference = "C1234";
			return header;
		}

		public void TestManifestNature()
		{
			var argmanifestTypes = new COManifestTypes().All;
			var man = argmanifestTypes.FirstOrDefault(x => x.Code == COManifestTypes.Codes.MAN);
			AssertEquals(ShipmentTypeList.Codes.Import23, man.ManifestNatures.CodesAsString);
		}

		public void TestRegistrationDetails_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			AssertEquals(false, header.RegistrationDateInfo.ReadOnly);
			AssertEquals(false, header.RegistrationNumberInfo.ReadOnly);
		}

		public void TestCustomsStatus_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			AssertEquals(false, header.RegistrationStatusInfo.ReadOnly);
		}

		public void TestMessageStatus_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			AssertEquals(false, header.AMA_MessageStatusInfo.ReadOnly);
		}

		public void TestClearDeliveryMode()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.DeliveryMode = CODeliveryModeList.Codes._1;

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Delivery Mode cleared", string.Empty, header.DeliveryMode);
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);
	}
}
