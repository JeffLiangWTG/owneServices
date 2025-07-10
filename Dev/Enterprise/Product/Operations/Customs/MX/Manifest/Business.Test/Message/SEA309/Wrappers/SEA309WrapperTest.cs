using System;
using System.Linq;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	public class SEA309WrapperTest : TestCaseWithFactory
	{
		AsycudaManifestHeader header;
		GlbCompany company;

		public void TestSEA309Wrapper()
		{
			PopulateManifestHeader(true);

			ISea309SOManifest wrapper = new SEA309Wrapper(header.Bills[0], "ORG", ZString.Empty);
			IHeader iheader = wrapper.Header;
			IM10ManifestIdentifyingInformation m10 = wrapper.M10;
			IPortInformation port = wrapper.AP4.FirstOrDefault().P4;

			IALX1 alx = wrapper.AP4.FirstOrDefault().ALX1.FirstOrDefault();
			IM11ManifestBillLadingDetails m11 = alx.M11;

			IN9ReferenceIdentification n9HBL = alx.N9.ElementAt(1);
			IN9ReferenceIdentification n9CustomsOffice = alx.N9.ElementAt(0);

			IAN1408 shipper = alx.AN1408.ElementAt(1);
			IAN1408 consignee = alx.AN1408.ElementAt(0);
			IAN1408 notify = alx.AN1408.ElementAt(2);

			IAVID aVID = alx.AVID.FirstOrDefault();
			IVIDConveyanceIdentification vid = aVID.VID;
			IAN1 an1 = aVID.AN1.FirstOrDefault();

			IAN10 an10 = an1.AN10.FirstOrDefault();
			IN10QuantityDescription packa = an10.N10;
			IHazardousInfo h1 = an10.AH.FirstOrDefault().H1;

			IISAInterchangeControlHeader isa = wrapper.ISA;

			CombineAssertions(() =>
			{
				AssertEquals("ADMINVUCEM13", iheader.User);
				AssertEquals("9974567890", iheader.Password);
				AssertEquals("http://54.183.24.126:7001/manifiestoMaritimoRespuestaMock/services/ManifiestoMaritimo355SO?WSDL", iheader.UniformResourceLocator);

				AssertEquals("", m10.ManifestReferenceIdentification);
				AssertEquals("SI", m10.ApplicationType);
				AssertEquals("", m10.StandCarrierAlphaCode);
				AssertEquals("", m10.CountryCode);
				AssertEquals("", m10.VesselCode);
				AssertEquals("", m10.VesselName);
				AssertEquals("", m10.VoyageNumber);
				AssertEquals("", m10.ReferenceIdentification);
				AssertEquals("W", m10.ManifestTypeCode);

				AssertEquals("HOUSELIGADAMASTER0001", m11.HBLNumber);
				AssertEquals("30505", m11.LocationIdentifier);
				AssertEquals("400", m11.Quantity);
				AssertEquals("PCS", m11.ManifestUnitCode);
				AssertEquals("20", m11.Weight);
				AssertEquals("K", m11.WeightUnitCode);
				AssertEquals("25", m11.Volume);
				AssertEquals("X", m11.VolumeUnitQualifier);
				AssertEquals("30", m11.HBLCode);
				AssertEquals("9992", m11.StandardCarrierAlphaCode);
				AssertEquals("30505", m11.LocationIdentifier3);

				AssertEquals("HO", n9HBL.ReferenceIdentification);
				AssertEquals("9992MANIFESTNUMBER", n9HBL.ReferenceIdentification2);

				AssertEquals("MXI", n9CustomsOffice.ReferenceIdentification);
				AssertEquals("0801", n9CustomsOffice.ReferenceIdentification2);

				AssertEquals("SH", shipper.N1.EntityIdentifierCode);
				AssertEquals("LALA", shipper.N1.Name);
				AssertEquals("NNN8912251", shipper.N1.IdentificationCode);
				AssertEquals("AV PLAZA", shipper.N3.FirstOrDefault().AddressInformation);
				AssertEquals("ARTEGA Y MEXICO", shipper.N3.FirstOrDefault().AddressInformation2);
				AssertEquals("MONTEVIDEO", shipper.N4.CityName);
				AssertEquals("", shipper.N4.StateCode);
				AssertEquals("06700", shipper.N4.PostalCode);
				AssertEquals("UY", shipper.N4.CountryCode);
				AssertEquals("LALA", shipper.PER.Name);

				AssertEquals("TIN", consignee.N1.EntityIdentifierCode);
				AssertEquals("TIN", consignee.N1.IdentificationCodeQualifier);
				AssertEquals("MADERA SA de CV", consignee.N1.Name);
				AssertEquals("NNN891228BE2", consignee.N1.IdentificationCode);
				AssertEquals("AV REMEDIOS", consignee.N3.FirstOrDefault().AddressInformation);
				AssertEquals("ARTEGA Y TINOCO", consignee.N3.FirstOrDefault().AddressInformation2);
				AssertEquals("CIUDAD DE MEXICO", consignee.N4.CityName);
				AssertEquals("CMX", consignee.N4.StateCode);
				AssertEquals("06700", consignee.N4.PostalCode);
				AssertEquals("MX", consignee.N4.CountryCode);
				AssertEquals("MADERA SA de CV", consignee.PER.Name);

				AssertEquals("N1", notify.N1.EntityIdentifierCode);
				AssertEquals("LORENA", notify.N1.Name);
				AssertEquals("NNN89122", notify.N1.IdentificationCode);
				AssertEquals("AV ARTEAGA", notify.N3.FirstOrDefault().AddressInformation);
				AssertEquals("PLAZA Y TINOCO", notify.N3.FirstOrDefault().AddressInformation2);
				AssertEquals("CALIFORNIA", notify.N4.CityName);
				AssertEquals("CAL", notify.N4.StateCode);
				AssertEquals("06700", notify.N4.PostalCode);
				AssertEquals("US", notify.N4.CountryCode);
				AssertEquals("LORENA", notify.PER.Name);
				AssertEquals("55427634", notify.PER.CommunicationData);

				AssertEquals("AC", vid.EquipmentDescriptionCode);
				AssertEquals("9594601", vid.EquipmentNumber);
				AssertEquals("AAA12345678", vid.SealNumber);
				AssertEquals("BB987456321", vid.SealNumber2);
				AssertEquals("", vid.EquipmentLenght);
				AssertEquals("", vid.Height);
				AssertEquals("", vid.Width);
				AssertEquals("40GP", vid.EquipmentType);

				AssertEquals("400", packa.Quantity);
				AssertEquals("PERCHEROS", packa.Description);
				AssertEquals("MADERA", packa.MarksAndNumbers);
				AssertEquals("940350", packa.CommodityCode);
				AssertEquals("700", packa.CustomsShipmentValue);
				AssertEquals("K", packa.WeightUnitCode);
				AssertEquals("4000", packa.Weight);
				AssertEquals("BOX", packa.ManifestUnitCode);
				AssertEquals("US", packa.CountryCode);

				AssertEquals("1008", h1.HazardousMaterialCode);
				AssertEquals("U", h1.HazardousMaterialCodeQualifier);
				AssertEquals("2.3", h1.HazardousMaterialClassCode);
				AssertEquals("DESCRIPTION", h1.HazardousMaterialDescription);
				AssertEquals("-12", h1.FlashpointTemperature);

				AssertEquals("", isa.AuthorizationInformation);
				AssertEquals("", isa.SecurityInformation);
			});
		}

		public void TestCarrierCode_M11()
		{
			PopulateManifestHeader(false);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "123456";

			var orgAddress = org.MainAddress;
			orgAddress.Address1 = "address";

			var orgCusCode3 = orgAddress.CustomsCodes.AddNew();
			orgCusCode3.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode3.OK_CustomsRegNo = "2022";
			orgCusCode3.OK_RN_NKCodeCountry = "AU";

			header.AMA_OA_Carrier = orgAddress.PK;

			ISea309SOManifest wrapper = new SEA309Wrapper(header.Bills[0], "ORG", ZString.Empty);

			IALX1 alx = wrapper.AP4.FirstOrDefault().ALX1.FirstOrDefault();
			IM11ManifestBillLadingDetails m11 = alx.M11;

			AssertEquals("2022", m11.StandardCarrierAlphaCode);

			var orgCusCode = orgAddress.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode.OK_CustomsRegNo = "1234";
			orgCusCode.OK_RN_NKCodeCountry = "US";

			header.AMA_OA_Carrier = orgAddress.PK;

			wrapper = new SEA309Wrapper(header.Bills[0], "ORG", ZString.Empty);
			alx = wrapper.AP4.FirstOrDefault().ALX1.FirstOrDefault();
			m11 = alx.M11;

			AssertEquals("1234", m11.StandardCarrierAlphaCode);

			var orgCusCode2 = orgAddress.CustomsCodes.AddNew();
			orgCusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode2.OK_CustomsRegNo = "9992";
			orgCusCode2.OK_RN_NKCodeCountry = "MX";

			header.AMA_OA_Carrier = orgAddress.PK;

			wrapper = new SEA309Wrapper(header.Bills[0], "ORG", ZString.Empty);
			alx = wrapper.AP4.FirstOrDefault().ALX1.FirstOrDefault();
			m11 = alx.M11;

			AssertEquals("9992", m11.StandardCarrierAlphaCode);
		}

		public void TestCarrierCode_M13()
		{
			PopulateManifestHeader(false);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "123456";

			var orgAddress = org.MainAddress;
			orgAddress.Address1 = "address";

			var orgCusCode3 = orgAddress.CustomsCodes.AddNew();
			orgCusCode3.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode3.OK_CustomsRegNo = "2022";
			orgCusCode3.OK_RN_NKCodeCountry = "AU";

			header.AMA_OA_Carrier = orgAddress.PK;

			ISea309SOManifest wrapper = new SEA309Wrapper(header.Bills[0], "CHG", ZString.Empty);
			IALX1 alx = wrapper.AP4.FirstOrDefault().ALX1.FirstOrDefault();
			IM13ManifestAmendmentDetails m13 = alx.M13;

			AssertEquals("2022", m13.StandardCarrierAlphaCode);

			var orgCusCode = orgAddress.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode.OK_CustomsRegNo = "1234";
			orgCusCode.OK_RN_NKCodeCountry = "US";

			header.AMA_OA_Carrier = orgAddress.PK;

			wrapper = new SEA309Wrapper(header.Bills[0], "CHG", ZString.Empty);
			alx = wrapper.AP4.FirstOrDefault().ALX1.FirstOrDefault();
			m13 = alx.M13;

			AssertEquals("1234", m13.StandardCarrierAlphaCode);

			var orgCusCode2 = orgAddress.CustomsCodes.AddNew();
			orgCusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			orgCusCode2.OK_CustomsRegNo = "9992";
			orgCusCode2.OK_RN_NKCodeCountry = "MX";

			header.AMA_OA_Carrier = orgAddress.PK;

			wrapper = new SEA309Wrapper(header.Bills[0], "CHG", ZString.Empty);
			alx = wrapper.AP4.FirstOrDefault().ALX1.FirstOrDefault();
			m13 = alx.M13;

			AssertEquals("9992", m13.StandardCarrierAlphaCode);
		}

		protected override void SetUp()
		{
			company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Mexico;

			var testItem = new MXWsVucem(new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty), Factory);
			testItem.AirModeWSResponse = "http://127.0.0.1/wsdl?";
			testItem.AirModeWSUsername = "ADMINVUCEM1";
			testItem.AirModeWSPassword = "9974567891";
			testItem.SeaModeWSResponse = "http://54.183.24.126:7001/manifiestoMaritimoRespuestaMock/services/ManifiestoMaritimo355SO?WSDL";
			testItem.SeaModeWSUsername = "ADMINVUCEM13";
			testItem.SeaModeWSPassword = "9974567890";
			MXCustomsDataRegistry.Instance.WSVucem.SetValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, testItem);

			Factory.Save();
		}

		void PopulateManifestHeader(ZBool populateCarrier)
		{
			header = Factory.NewWithValidTestData<AsycudaManifestHeader>();

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			company.Branches.Add(branch);

			if (populateCarrier)
			{
				var org = Factory.New<OrgHeader>();
				org.OH_Code = "123456";

				var orgAddress = org.MainAddress;
				orgAddress.Address1 = "address";

				var orgCusCode3 = orgAddress.CustomsCodes.AddNew();
				orgCusCode3.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				orgCusCode3.OK_CustomsRegNo = "2022";
				orgCusCode3.OK_RN_NKCodeCountry = "AU";

				var orgCusCode = orgAddress.CustomsCodes.AddNew();
				orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				orgCusCode.OK_CustomsRegNo = "1234";
				orgCusCode.OK_RN_NKCodeCountry = "US";

				var orgCusCode2 = orgAddress.CustomsCodes.AddNew();
				orgCusCode2.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
				orgCusCode2.OK_CustomsRegNo = "9992";
				orgCusCode2.OK_RN_NKCodeCountry = "MX";

				header.AMA_OA_Carrier = orgAddress.PK;
			}

			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Mexico;
			header.AMA_LloydsNumber = "VCODE";
			header.AMA_VesselName = "VESSELNUMBER";
			header.AMA_Voyage = "VOYAGE";
			header.AMA_MasterBill = "MANIFESTNUMBER";
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_CustomsDischargePort = "35701";
			header.AMA_CustomsLoadPort = "30505";
			header.AMA_RL_NKPortOfLoading = "22581";
			header.AMA_E_ARV = new ZDate(2019, 10, 31);
			header.AMA_E_DEP = new ZDate(2019, 11, 12);
			header.AMA_CustomsOffice = "0801";
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;
			header.AMA_GB = branch.PK;
			header.AMA_CarrierCode = "1245";

			CreateAndPopulateHouseBill();

			Factory.Save();
		}

		void CreateAndPopulateHouseBill()
		{
			var bill = header.Bills.AddNew();

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
			org2.OH_RL_NKClosestPort = "UY";
			org2.OH_FullName = "LALA";
			var orgAddress2 = org2.MainAddress;
			org2.CustomsCodes.AddNew(MexicoOrgCusCodeInfo.OrgCusCodes.RFC, "NNN8912251");

			orgAddress2.OA_Address1 = "AV PLAZA";
			orgAddress2.OA_Address2 = "ARTEGA Y MEXICO";
			orgAddress2.OA_City = "MONTEVIDEO";
			orgAddress2.OA_State = "MVD";
			orgAddress2.OA_PostCode = "06700";
			orgAddress2.OA_RN_NKCountryCode = "UY";
			orgAddress2.OA_Phone = "4442763477";
			orgAddress2.OA_Email = "LALA@GMAIL.COM";

			bill.ABL_OA_Shipper = orgAddress2.PK;

			var org3 = Factory.New<OrgHeader>();
			org3.OH_RL_NKClosestPort = "US";
			org3.OH_FullName = "LORENA";
			var orgAddress3 = org3.MainAddress;
			org3.CustomsCodes.AddNew(MexicoOrgCusCodeInfo.OrgCusCodes.RFC, "NNN89122");

			orgAddress3.OA_Address1 = "AV ARTEAGA";
			orgAddress3.OA_Address2 = "PLAZA Y TINOCO";
			orgAddress3.OA_City = "CALIFORNIA";
			orgAddress3.OA_State = "CAL";
			orgAddress3.OA_PostCode = "06700";
			orgAddress3.OA_RN_NKCountryCode = "US";
			orgAddress3.OA_Phone = "55427634";
			orgAddress3.OA_Email = "LORENA@GMAIL.COM";

			bill.ABL_OA_NotifyParty = orgAddress3.PK;

			bill.ABL_BillNumber = "HOUSELIGADAMASTER0001";
			bill.ABL_ManifestQty = 400;
			bill.ABL_ManifestUQ = "PCS";
			bill.ABL_GrossWeight = 20.00;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_Volume = 25.000;
			bill.ABL_VolumeUQ = "M3";
			bill.ABL_RL_NKOrigin = "USLAX";

			bill.ABL_PrepaidCollect = Core.Constants.PaymentType.Prepaid;

			CreateAndPopulatePack();
		}

		void CreateAndPopulatePack()
		{
			var pack = header.Bills[0].Packs.AddNew();

			var refcontainer = Factory.New<RefContainer>();
			refcontainer.RC_Code = "30";
			refcontainer.RC_ISOType = "40GP";
			refcontainer.RC_Length = 1200;
			refcontainer.RC_Height = 6000;
			refcontainer.RC_Width = 4000;

			var container = header.Containers.AddNew();
			container.ACN_ContainerNumber = "9594601";
			container.ACN_Seal1 = "AAA12345678";
			container.ACN_Seal2 = "BB987456321";
			container.ACN_EmptyFullIndicator = "L";
			container.ACN_RC_ContainerType = refcontainer.PK;

			pack.APA_WeightUQ = "KG";
			pack.APA_Weight = 4000;
			pack.APA_PackUQ = "BOX";
			pack.APA_PackQty = 400;
			pack.APA_MarksAndNumbers = "MADERA";
			pack.APA_GoodsDescription = "PERCHEROS";
			pack.APA_CommodityCode = "940350";
			pack.LinePrice = 700;
			pack.ContainerPK = container.PK;

			var undg = pack.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			undg.UNDGSubstance.DG_UNNO = "1008";
			undg.UNDGSubstance.DG_Class = "2.3";
			undg.UNDGSubstance.DG_PSN = "DESCRIPTION";
			undg.UNDGSubstance.DG_FlashPoint = "-12 cc";
		}
	}
}
