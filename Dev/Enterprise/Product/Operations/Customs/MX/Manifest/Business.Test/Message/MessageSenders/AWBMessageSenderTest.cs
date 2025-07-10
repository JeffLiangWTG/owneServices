using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public class AWBMessageSenderTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;
		GlbCompanyCredential credential;

		public void TestSendManifest()
		{
			PopulateManifestHeader();

			var messageSubType = MessageSubTypeCodes.Codes.Original;

			var bill = header.Bills[0];
			var messageSender = new AWBMessageSender(bill, new HouseWaybillWrapper(bill, messageSubType), messageSubType);

			var messageResult = messageSender.SendMessage();

			AssertEquals(messageResult, "Message sent successfully");

			AssertEquals(MessageStatusCodeList.Codes.Awaiting, bill.ABL_MessageStatus);
			AssertEquals(CustomsStatusList.Codes.SNT, bill.ABL_BillStatus);

			var createdMessage = bill.Messages.FirstOrDefault() as MXMessage;
			AssertNotNull("Should have created a message.", createdMessage);

			AssertEquals("Should have set a correct EM_MessageOwner.", "ADMINVUCEM13", createdMessage.EM_MessageOwner);
			AssertEquals("Should have set a correct EM_GP.", credential.PK, createdMessage.EM_GP);
			AssertEquals("Message Sub Type should be Empty.", "", createdMessage.EM_MessageSubType);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var testItem = new MXWsVucem(new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			testItem.AirModeWSResponse = "http://127.0.0.1/wsdl?";
			testItem.AirModeWSUsername = "ADMINVUCEM1";
			testItem.AirModeWSPassword = "9974567891";
			testItem.SeaModeWSResponse = "http://54.183.24.126:7001/manifiestoMaritimoRespuestaMock/services/ManifiestoMaritimo355SO?WSDL";
			testItem.SeaModeWSUsername = "ADMINVUCEM2";
			testItem.SeaModeWSPassword = "9974567890";

			MXCustomsDataRegistry.Instance.WSVucem.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, testItem);
			MXCustomsDataRegistry.Instance.MXTestingSystem.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);

			GlbCompany.CurrentCompany.GC_CustomsRegistrationNo = "9992";
		}

		void PopulateManifestHeader()
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			SetCredentials();

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "123456";

			var orgAddress = org.MainAddress;
			orgAddress.Address1 = "address";

			var orgCusCode = orgAddress.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode.OK_CustomsRegNo = "1245";
			orgCusCode.OK_RN_NKCodeCountry = "MX";

			header.AMA_ManifestType = "MAN";
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			header.AMA_Voyage = "VOYAGE";
			header.AMA_MasterBill = "MANIFESTNUMBER";
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_RL_NKPortOfLoading = "MXACA";
			header.AMA_RL_NKPortOfDischarge = "USLAX";
			header.AMA_E_ARV = new ZDate(2019, 10, 31);
			header.AMA_E_DEP = new ZDate(2019, 11, 12);
			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			header.AMA_GB = GlbBranch.CurrentBranch.PK;
			header.AMA_OA_Carrier = orgAddress.PK;
			header.AMA_MasterBillIssueDate = ZDate.Today;

			CreateAndPopulateHouseBill();
		}

		void CreateAndPopulateHouseBill()
		{
			AsycudaBill bill = header.Bills.AddNew();

			var org = Factory.New<OrgHeader>();
			org.OH_RL_NKClosestPort = "MX";
			org.OH_FullName = "MADERA SA de CV";
			var orgAddress = org.MainAddress;
			org.CustomsCodes.AddNew(MexicoOrgCusCodeInfo.OrgCusCodes.RFC, "NNN891228BE2");

			orgAddress.OA_Address1 = "AV REMEDIOS";
			orgAddress.OA_Address2 = "ARTEGA Y TINOCO";
			orgAddress.OA_City = "CIUDAD DE MEXICO";
			orgAddress.OA_State = "CMX";
			orgAddress.OA_PostCode = "06700";
			orgAddress.OA_RN_NKCountryCode = "MX";
			orgAddress.OA_Phone = "5542763477";
			orgAddress.OA_Email = "TANIA@GMAIL.COM";

			bill.ABL_OA_Consignee = orgAddress.PK;

			var org2 = Factory.New<OrgHeader>();
			org2.OH_RL_NKClosestPort = "MX";
			org2.OH_FullName = "LALA";
			var orgAddress2 = org2.MainAddress;
			org2.CustomsCodes.AddNew(MexicoOrgCusCodeInfo.OrgCusCodes.RFC, "NNN8912251");

			orgAddress2.OA_Address1 = "AV PLAZA";
			orgAddress2.OA_Address2 = "ARTEGA Y MEXICO";
			orgAddress2.OA_City = "CIUDAD DE MEXICO";
			orgAddress2.OA_State = "CMX";
			orgAddress2.OA_PostCode = "06700";
			orgAddress2.OA_RN_NKCountryCode = "MX";
			orgAddress2.OA_Phone = "4442763477";
			orgAddress2.OA_Email = "LALA@GMAIL.COM";

			bill.ABL_OA_Shipper = orgAddress2.PK;

			var org3 = Factory.New<OrgHeader>();
			org3.OH_RL_NKClosestPort = "MX";
			org3.OH_FullName = "LORENA";
			var orgAddress3 = org3.MainAddress;
			org3.CustomsCodes.AddNew(MexicoOrgCusCodeInfo.OrgCusCodes.RFC, "NNN89122");

			orgAddress3.OA_Address1 = "AV ARTEAGA";
			orgAddress3.OA_Address2 = "PLAZA Y TINOCO";
			orgAddress3.OA_City = "CIUDAD DE MEXICO";
			orgAddress3.OA_State = "CMX";
			orgAddress3.OA_PostCode = "06700";
			orgAddress3.OA_RN_NKCountryCode = "MX";
			orgAddress3.OA_Phone = "55427634";
			orgAddress3.OA_Email = "LORENA@GMAIL.COM";

			bill.ABL_OA_NotifyParty = orgAddress3.PK;

			bill.ABL_BillNumber = "HOUSELIGADAMASTER0001";
			bill.ABL_ManifestQty = 400;
			bill.ABL_ManifestUQ = "PCS";
			bill.ABL_GrossWeight = 20;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_Volume = 25;
			bill.ABL_VolumeUQ = "CC";
			bill.ABL_RL_NKOrigin = "USLAX";
			bill.ABL_RX_NKCustomsValueCurrency = "USD";
			bill.ABL_RX_NKFreightValueCurrency = "USD";
			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			CreateAndPopulatePack(bill);
		}

		void CreateAndPopulatePack(AsycudaBill bill)
		{
			AsycudaPack pack = bill.Packs.AddNew();

			pack.APA_WeightUQ = "KG";
			pack.APA_Weight = 4000;
			pack.APA_PackUQ = "BOX";
			pack.APA_PackQty = 400;
			pack.APA_MarksAndNumbers = "MADERA";
			pack.APA_GoodsDescription = "PERCHEROS";
			pack.APA_CommodityCode = "940350";
			pack.LinePrice = 700;
			pack.LinePriceCurrency = "USD";
		}

		void SetCredentials()
		{
			credential = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_UserID = "ADMINVUCEM13";
			credential.GP_CurrentPassword = "9974567890";

			GlbCompany.CurrentCompany.Factory.Save();
		}
	}
}
