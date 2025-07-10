using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	partial class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
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
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.ARManifest.IAsycudaManifestHeader>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_JobReference = "C1234";
			return header;
		}

		public void TestDefaultGetTypes()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Argentina, ARManifestTypes.Codes.MAN);
			CombineAssertions(() =>
			{
				AssertEquals("Default Bill Type", typeof(AsycudaBill), header.GetBillType());
				AssertEquals("Default Container Type", typeof(AsycudaContainer), header.GetContainerType());
			});
		}

		public void TestRegistrationDetails()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			AssertEquals(true, header.RegistrationStatusInfo.ReadOnly);
			AssertEquals(true, header.RegistrationDateInfo.ReadOnly);
			AssertEquals(true, header.RegistrationNumberInfo.ReadOnly);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals(false, header.RegistrationStatusInfo.ReadOnly);
			AssertEquals(false, header.RegistrationDateInfo.ReadOnly);
			AssertEquals(false, header.RegistrationNumberInfo.ReadOnly);
		}

		public void TestGetMessageSendingNotification_NotExists()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Argentina, ARManifestTypes.Codes.MAN);
			var message = header.MessageSendingNotificationHelper.GetNotifications();
			AssertNotContains(ValidationsConstants.MustBeLoggedInUnderARToSendARMessages, message);
		}

		public void TestGetMessageSendingNotification_Exists()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Uruguay))
			{
				var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Argentina, ARManifestTypes.Codes.MAN);
				var message = header.MessageSendingNotificationHelper.GetNotifications();
				AssertContains(ValidationsConstants.MustBeLoggedInUnderARToSendARMessages, message);
			}
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);
	}
}
