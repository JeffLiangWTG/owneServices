using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Business.Testing
{
	sealed class SeaMessageSenderTest : TestCaseWithFactory
	{
		[TestDate(2023, 01, 01, 13, 00, 00)]
		public void TestSendSeaMessage()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_JobReference = "MAN0000001";
			Factory.Save();

			header = CreateAndPopulateManifestHeader(false);

			var messageSender = new SeaMessageSender(header, "ORG", header.GetValidDocumentIDs(header.Bills.Count + 1));
			var messageResult = messageSender.CreateMessage();

			Assert("CodCpt 1 means Original, 2 is for Amend", messageResult.Contains("<CodCpt>1</CodCpt>"));

			Assert("ideDoc of Transaction Number", messageResult.Contains("ideDoc=\"11667803932049\""));

			Assert("ndv is the Master Bill Number", messageResult.Contains("ndv=\"MEDUQ2394375\""));

			Assert("Container included", messageResult.Contains("<contenedor contp=\"CAIU305178-6\" />"));
		}

		[TestDate(2023, 01, 01, 13, 00, 00)]
		public void TestAmendSeaMessage()
		{
			var header = CreateAndPopulateManifestHeader(true);

			var messageSender = new SeaMessageSender(header, "CHG", header.GetValidDocumentIDs(header.Bills.Count + 1));
			var messageResult = messageSender.CreateMessage();

			Assert("CodCpt 1 means Original, 2 is for Amend", messageResult.Contains("<CodCpt>2</CodCpt>"));

			Assert("ideDoc of Transaction Number", messageResult.Contains("ideDoc=\"11667803932049\""));

			Assert("ndv is the Master Bill Number", messageResult.Contains("ndv=\"MEDUQ2394375\""));

			Assert("Container included", messageResult.Contains("<contenedor contp=\"CAIU305178-6\" />"));
		}

		AsycudaManifestHeader CreateAndPopulateManifestHeader(bool isMessageStatusAccepted = false)
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Voyage = "UC1103";
			header.AMA_CustomsOffice = "25";
			header.AMA_RL_NKPortOfLoading = "CO8SG";
			header.AMA_RL_NKPortOfDischarge = "DEABC";
			header.AMA_MasterBillIssueDate = new ZDate(2022, 06, 06);
			header.DeliveryMode = CODeliveryModeList.Codes._4;

			if (isMessageStatusAccepted)
			{
				header.AMA_MessageStatus = "ACP";
			}

			CreateAndPopulateHouseBills(header);
			CreateAndPopulateMasterBill(header);

			Factory.Save();

			return header;
		}

		void CreateAndPopulateHouseBills(AsycudaManifestHeader header)
		{
			var bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "(H)QRHW20050055C";
			bill.ABL_BillIssueDate = new ZDate(2022, 06, 06);
			bill.ABL_GrossWeight = 50;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_Volume = 70;
			bill.ABL_VolumeUQ = "M3";
			bill.ABL_ManifestQty = 3;

			CreateAndPopulatePackforTheFirstBill(header);

			bill = header.Bills.AddNew();

			bill.ABL_BillNumber = "(H)XXXX19960621X";
			bill.ABL_BillIssueDate = new ZDate(2022, 07, 07);
			bill.ABL_GrossWeight = 10;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_Volume = 15;
			bill.ABL_VolumeUQ = "M3";
			bill.ABL_ManifestQty = 2;

			CreateAndPopulatePackforTheSecondBill(bill);
		}

		void CreateAndPopulateMasterBill(AsycudaManifestHeader header)
		{
			var bill = header.MasterBill;

			bill.ABL_BillNumber = "MEDUQ2394375";
			bill.ABL_CustomsDischargePort = "COBOG";
			bill.ABL_BillIssueDate = new ZDate(2022, 06, 21);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "123459";
			orgHeader.OH_FullName = "Party";
			orgHeader.CustomsCodes.AddNew(UruguayOrgCusCodeInfo.OrgCusCodes.RUT, "21.336.937.002-3");

			var orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "1346";
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Uruguay;

			bill.ABL_OA_Consignee = orgAddress.PK;
		}

		void CreateAndPopulatePackforTheFirstBill(AsycudaManifestHeader header)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Colombia;

			var bill = header.Bills[0];

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "CAIU305178-6";
			container.ACN_GoodsWeight = 20;
			container.ACN_GoodsWeightUQ = "KG";

			container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "AAXX125231-1";
			container.ACN_GoodsWeight = 10;
			container.ACN_GoodsWeightUQ = "KG";

			var pack = bill.Packs.AddNew();
			pack.ContainerPK = header.Containers[0].PK;
			pack.APA_PackQty = 10;
			pack.APA_PackUQ = "PCS";
			pack.APA_Weight = 20;
			pack.APA_WeightUQ = "KG";
			pack.APA_Volume = 20;
			pack.APA_VolumeUQ = "M3";
			pack.IsHazardous = true;

			var substance = Factory.NewWithValidTestData<UNDGSubstance>();
			substance.DG_Code = "12345";

			var undg = pack.UNDGs.AddNew();
			undg.SubstancePK = substance.PK;
			undg.DI_OC_DGContact = Factory.NewWithValidTestData<OrgContact>().PK;
			undg.DGContact.OC_ContactName = "DGContact";
			undg.DGContact.OC_Phone = "092702171";
			undg.DGContact.OC_OA_OrgAddress = orgAddress.PK;
			undg.DI_IMOClass = "1";

			pack = bill.Packs.AddNew();
			pack.ContainerPK = header.Containers[0].PK;
			pack.APA_PackQty = 1;
			pack.APA_Weight = 20;
			pack.APA_WeightUQ = "KG";
			pack.APA_Volume = 20;
			pack.APA_VolumeUQ = "M3";

			pack = bill.Packs.AddNew();
			pack.ContainerPK = header.Containers[1].PK;
			pack.APA_PackQty = 1;
			pack.APA_Weight = 10;
			pack.APA_WeightUQ = "KG";
			pack.APA_Volume = 30;
			pack.APA_VolumeUQ = "M3";
		}

		void CreateAndPopulatePackforTheSecondBill(AsycudaBill bill)
		{
			var container = bill.Header.Containers.AddNew();
			container.ACN_ContainerNumber = "BDGA123456-7";
			container.ACN_GoodsWeight = 10;
			container.ACN_GoodsWeightUQ = "KG";

			var pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			pack.APA_PackQty = 1;
			pack.APA_Weight = 4;
			pack.APA_WeightUQ = "KG";
			pack.APA_Volume = 7;
			pack.APA_VolumeUQ = "M3";

			pack = bill.Packs.AddNew();
			pack.ContainerPK = container.PK;
			pack.APA_PackQty = 1;
			pack.APA_Weight = 6;
			pack.APA_WeightUQ = "KG";
			pack.APA_Volume = 8;
			pack.APA_VolumeUQ = "M3";
		}

		protected override void SetUp()
		{
			base.SetUp();

			var cusTransactionNumber = Factory.NewWithValidTestData<CusTransactionNumber>();
			cusTransactionNumber.TN_TransactionReference = "11667803932049";
			cusTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			cusTransactionNumber.TN_GC_Company = GlbCompany.CurrentCompany.PK;

			cusTransactionNumber = Factory.NewWithValidTestData<CusTransactionNumber>();
			cusTransactionNumber.TN_TransactionReference = "11667803932056";
			cusTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			cusTransactionNumber.TN_GC_Company = GlbCompany.CurrentCompany.PK;

			cusTransactionNumber = Factory.NewWithValidTestData<CusTransactionNumber>();
			cusTransactionNumber.TN_TransactionReference = "11667803932057";
			cusTransactionNumber.TN_Type = CusTransactionNumberTypeList.Codes.ColombiaManifest;
			cusTransactionNumber.TN_GC_Company = GlbCompany.CurrentCompany.PK;

			GlbCompany.CurrentCompany.GC_BusinessRegNo = "410658947244";

			var pack = Factory.New<CusRefPacks>();
			pack.RP_CustomsPack = "BT";
			pack.RP_Type = "GMB";
			pack.RP_CustomsCountry = "CO";
			pack.RP_ConversionFactor = 2;
			pack.RP_CommercialPack = "PCS";

			resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		Lazy<EmbeddedResourceRetriever> resourceRetriever;
	}
}
