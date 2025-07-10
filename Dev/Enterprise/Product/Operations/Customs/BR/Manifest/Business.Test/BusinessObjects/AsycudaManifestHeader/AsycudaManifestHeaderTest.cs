using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Manifest.Business.Test
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaBillCollection>(header.Bills);
		}

		public void TestCustomsOwnNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.CustomsOwnNumber = "1BR11111111255555555555555555554444";
			Factory.Save();

			AssertEquals(35, header.CustomsOwnNumberInfo.MaxLength);
			AssertEquals("1BR11111111255555555555555555554444", header.CustomsOwnNumber);

			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, header.MasterBill.PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Brazil.CustomsOwnNumber);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, header.AMA_RN_NKCountry);

			var cusEntryNum1 = Factory.LoadTop1<CusEntryNumber>(query);

			AssertEquals("1BR11111111255555555555555555554444", cusEntryNum1.CE_EntryNum);

			header.CustomsOwnNumber = ZString.Empty;
			Factory.Save();

			var cusEntryNum2 = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals(null, cusEntryNum2);
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = GetNewBusinessObject();
			bizObj.FillWithValidTestData();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaManifestHeader>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestDefaultGetTypes()
		{
			var header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Brazil, BRManifestTypes.Codes.MUCR);
			CombineAssertions(() =>
			{
				AssertEquals("Default Bill Type", typeof(AsycudaBill), header.GetBillType());
				AssertEquals("Default Container Type", typeof(AsycudaContainer), header.GetContainerType());
			});
		}

		public void TestRegistrationDetails_ReadOnly()
		{
			var header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Brazil, BRManifestTypes.Codes.MUCR);

			AssertEquals(true, header.RegistrationDateInfo.ReadOnly);
			AssertEquals(true, header.RegistrationNumberInfo.ReadOnly);

			header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Brazil, BRManifestTypes.Codes.MER);

			AssertEquals(false, header.RegistrationDateInfo.ReadOnly);
			AssertEquals(false, header.RegistrationNumberInfo.ReadOnly);
		}

		public void TestCustomsStatus_ReadOnly()
		{
			var header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Brazil, BRManifestTypes.Codes.MUCR);
			AssertEquals(true, header.RegistrationStatusInfo.ReadOnly);

			header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Brazil, BRManifestTypes.Codes.MER);
			AssertEquals(false, header.RegistrationStatusInfo.ReadOnly);
		}

		public void TestMessageStatus_ReadOnly()
		{
			var header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Brazil, BRManifestTypes.Codes.MUCR);
			AssertEquals(true, header.AMA_MessageStatusInfo.ReadOnly);

			header = ASYCUDA.Business.AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Brazil, BRManifestTypes.Codes.MER);
			AssertEquals(false, header.AMA_MessageStatusInfo.ReadOnly);
		}

		public void TestDeconsolidateAddress_ReadOnly()
		{
			var header = Factory.New<AsycudaManifestHeader>();

			header.AMA_ManifestType = BRManifestTypes.Codes.MUCR;
			AssertEquals(true, header.AMA_OA_DeconsolidateAddressInfo.ReadOnly);
			
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;
			AssertEquals(false, header.AMA_OA_DeconsolidateAddressInfo.ReadOnly);
		}

		public void TestLookups()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaManifestHeaderLookups>(header.Lookups);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			return header;
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);
	}
}
