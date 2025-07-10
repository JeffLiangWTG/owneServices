using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.CL.Manifest.Business.Testing
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
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.CLManifest.IAsycudaManifestHeader>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestDefaultGetTypes()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Chile, CLManifestTypes.Codes.MAN);
			CombineAssertions(() =>
			{
				AssertEquals("Default Bill Type", typeof(AsycudaBill), header.GetBillType());
				AssertEquals("Default Arrival Header Type", typeof(AsycudaArrivalHeader), header.GetArrivalHeaderType());
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

		public void TestRegistrationNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.RegistrationNumber = "REG";

			var entryNum = CusEntryNumber.Load(header, CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, Core.Constants.CountryCodes.Chile, false);
			AssertNotNull("CusEntryNumber created", entryNum);
			AssertEquals("CE_EntryNum is set", "REG", entryNum.CE_EntryNum);
			Assert("CE_IssueDate is set", entryNum.CE_IssueDate.AddMinutes(1) > ZDateTime.Now);

			entryNum.CE_EntryNum = "456";
			AssertEquals("456", header.RegistrationNumber);
			entryNum.CE_EntryType = "";
			AssertEquals("", header.RegistrationNumber);

			AssertEquals(false, header.RegistrationDateInfo.ReadOnly);
			AssertEquals(false, header.RegistrationNumberInfo.ReadOnly);
		}

		public void TestGetMessageSendingNotification_NotExists()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Chile, CLManifestTypes.Codes.MAN);
			var message = header.MessageSendingNotificationHelper.GetNotifications();
			AssertNotContains(ValidationsConstants.MustBeLoggedInUnderCLToSendCLMessages, message);
		}

		public void TestGetNewMessageChooser()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();

			AssertType<MessageChooser>("Default message chooser type.", header.GetNewMessageChooser(items, ZString.Empty, false));
			AssertType<CLMessageChooser>("Should return CLMessageChooser.", header.GetNewMessageChooser(items, MessageSubTypeCodes.Codes.Original, false));
		}

		public void TestAMA_MasterBill_ReadOnly()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals(false, header.AMA_MasterBillInfo.ReadOnly);

			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			AssertEquals(false, header.AMA_MasterBillInfo.ReadOnly);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals(true, header.AMA_MasterBillInfo.ReadOnly);

			bill.ABL_BillStatus = CustomsStatusList.Codes.ERR;

			AssertEquals(false, header.AMA_MasterBillInfo.ReadOnly);
		}

		public void TestRegistrationDetails()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			AssertEquals(false, header.RegistrationStatusInfo.ReadOnly);
			AssertEquals(false, header.RegistrationDateInfo.ReadOnly);
			AssertEquals(false, header.RegistrationNumberInfo.ReadOnly);

			bill.ABL_BillStatus = CustomsStatusList.Codes.ACP;

			AssertEquals(false, header.RegistrationStatusInfo.ReadOnly);
			AssertEquals(false, header.RegistrationDateInfo.ReadOnly);
			AssertEquals(false, header.RegistrationNumberInfo.ReadOnly);

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			AssertEquals(true, header.RegistrationStatusInfo.ReadOnly);
			AssertEquals(true, header.RegistrationDateInfo.ReadOnly);
			AssertEquals(true, header.RegistrationNumberInfo.ReadOnly);

			bill.ABL_BillStatus = CustomsStatusList.Codes.ERR;

			AssertEquals(false, header.RegistrationStatusInfo.ReadOnly);
			AssertEquals(false, header.RegistrationDateInfo.ReadOnly);
			AssertEquals(false, header.RegistrationNumberInfo.ReadOnly);
		}

		public void TestArrivalHeaders()
		{
			var header = (AsycudaManifestHeader)GetNewBusinessObject();
			AssertType<AsycudaArrivalHeaderCollection>(header.ArrivalHeaders);
			AssertEquals(typeof(AsycudaArrivalHeader), header.GetArrivalHeaderType());
		}
	}
}
