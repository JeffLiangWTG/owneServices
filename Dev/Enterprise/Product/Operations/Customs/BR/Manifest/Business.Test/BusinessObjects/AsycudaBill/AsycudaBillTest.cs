using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaBill))]
	public class AsycudaBillTest : ManifestBase.Testing.AsycudaBillTest
	{
		public void TestIAsycudaBill()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.BRManifest.IAsycudaBill>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaBill>(bizObj.PK).GetType());
		}

		public void TestPacks()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaPackCollection>(bill.Packs);
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

		public void TestTaxes()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertType<AsycudaTaxCollection>(bill.AsycudaTaxes);
		}

		public void TestCreateNewAsycudaTaxCollection()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertType<AsycudaTaxCollection>(bill.CreateNewAsycudaTaxCollection());
		}

		public void TestGetAsycudaTaxTypeCore()
		{
			var bill = Factory.New<AsycudaBillForTest>();
			AssertEquals(typeof(AsycudaTax), bill.GetAsycudaTaxTypeCore());
		}

		public void TestDocumentType()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertNoExceptionThrown(() =>
			{
				bill.DocumentType = "DEU";
				Factory.Save();
				bill.DocumentType = "UCR";
				Factory.Save();
			});
		}

		public void TestFRTMode()
		{
			var bill = (AsycudaBill)GetNewBusinessObject();
			AssertNoExceptionThrown(() =>
			{
				bill.FRTMode = "HH";
				Factory.Save();
				bill.FRTMode = "PP";
				Factory.Save();
				bill.FRTMode = "HP";
				Factory.Save();
				bill.FRTMode = "PH";
				Factory.Save();
			});
		}

		public void TestGetWarningBeforeBeingDeletedForSentBills()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_MessageStatus = MessageStatusCodeList.Codes.Accepted;
			var bill1 = header.Bills.AddNew();
			bill1.ABL_BillNumber = "HB1";

			AssertEquals("This Bill is already sent to Customs.", bill1.GetWarningBeforeBeingDeleted());

			header.AMA_MessageStatus = MessageStatusCodeList.Codes.NotSent;
			AssertNotEquals("This Bill is already sent to Customs.", bill1.GetWarningBeforeBeingDeleted());
		}

		public void TestConsigneeRegNoType()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "FULL NAME 1";
			var orgAddress1 = org1.MainAddress;
			orgAddress1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			org1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, "12345678");

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			org2.OH_FullName = "FULL NAME 2";
			var orgAddress2 = org2.MainAddress;
			orgAddress2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "123123");

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Consignee = orgAddress1.PK;
			AssertEquals("CPF", bill.ABL_ConsigneeRegNoType);

			bill.ABL_OA_Consignee = orgAddress2.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNoType);

			header.AMA_ManifestType = BRManifestTypes.Codes.MUCR;

			bill.ABL_OA_Consignee = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNoType);
		}

		public void TestConsigneeRegNo()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "FULL NAME 1";
			var orgAddress1 = org1.MainAddress;
			orgAddress1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			org1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.IndividualTaxPayerRegistration, "12345678");

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			org2.OH_FullName = "FULL NAME 2";
			var orgAddress2 = org2.MainAddress;
			orgAddress2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "123123");

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;
			var bill = header.Bills.AddNew();

			bill.ABL_OA_Consignee = orgAddress1.PK;
			AssertEquals("12345678", bill.ABL_ConsigneeRegNo);

			bill.ABL_OA_Consignee = orgAddress2.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNo);

			header.AMA_ManifestType = BRManifestTypes.Codes.MUCR;

			bill.ABL_OA_Consignee = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_ConsigneeRegNo);
		}

		public void TestNotifyPartyRegNoType()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "FULL NAME";
			var orgAddress1 = org1.MainAddress;
			orgAddress1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			org1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "12345678");

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			org2.OH_FullName = "FULL NAME 2";
			var orgAddress2 = org2.MainAddress;
			orgAddress2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "123123");

			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;
			var bill = header.Bills.AddNew();

			bill.ABL_OA_NotifyParty = orgAddress1.PK;
			AssertEquals("CJN", bill.ABL_NotifyPartyRegNoType);

			bill.ABL_OA_NotifyParty = orgAddress2.PK;
			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNoType);

			header.AMA_ManifestType = BRManifestTypes.Codes.MUCR;

			bill.ABL_OA_NotifyParty = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNoType);
		}

		public void TestNotifyPartyRegNo()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "OH1";
			org1.OH_FullName = "FULL NAME";

			var orgAddress1 = org1.MainAddress;
			orgAddress1.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			org1.CustomsCodes.AddNew(BrazilOrgCusCodeInfo.OrgCusCodes.CNPJ, "12345678");

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "OH2";
			org2.OH_FullName = "FULL NAME 2";
			var orgAddress2 = org2.MainAddress;
			orgAddress2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Brazil;
			org2.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "123123");

			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = BRManifestTypes.Codes.MER;
			var bill = header.Bills.AddNew();

			bill.ABL_OA_NotifyParty = orgAddress1.PK;
			AssertEquals("12345678", bill.ABL_NotifyPartyRegNo);

			bill.ABL_OA_NotifyParty = orgAddress2.PK;
			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNo);

			header.AMA_ManifestType = BRManifestTypes.Codes.MUCR;

			bill.ABL_OA_NotifyParty = orgAddress1.PK;
			AssertEquals(ZString.Empty, bill.ABL_NotifyPartyRegNo);
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			var bill = header.Bills.AddNew();
			return bill;
		}

		public void TestCustomsOwnNumber()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.CustomsOwnNumber = "1BR11111111255555555555555555554444";
			Factory.Save();

			AssertEquals(35, bill.CustomsOwnNumberInfo.MaxLength);
			AssertEquals("1BR11111111255555555555555555554444", bill.CustomsOwnNumber);

			var query = new ZQuery(CusEntryNumSchema.CE_ParentID, bill.PK);
			query.AddToFilter(CusEntryNumSchema.CE_EntryType, CusEntryNumberTypes.Brazil.CustomsOwnNumber);
			query.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, header.AMA_RN_NKCountry);

			var cusEntryNum1 = Factory.LoadTop1<CusEntryNumber>(query);

			AssertEquals("1BR11111111255555555555555555554444", cusEntryNum1.CE_EntryNum);

			bill.CustomsOwnNumber = ZString.Empty;
			Factory.Save();

			var cusEntryNum2 = Factory.LoadTop1<CusEntryNumber>(query);
			AssertEquals(null, cusEntryNum2);
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
			public new ManifestBase.IAsycudaTaxCollection<ManifestBase.AsycudaTax, ManifestBase.AsycudaBill> CreateNewAsycudaTaxCollection() => base.CreateNewAsycudaTaxCollection();
			public new Type GetAsycudaTaxTypeCore() => base.GetAsycudaTaxTypeCore();
		}
	}
}
