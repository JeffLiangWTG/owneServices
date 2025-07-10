using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaManifestHeaderDocWrapperTest : TestCaseWithFactory
	{
		public void TestNew_ExcludeZAOutturnGateInOut()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_RL_NKDischargePort = "VUAUY";
			consol1.JK_UniqueConsignRef = "C456";

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_RL_NKDischargePort = "VUAUY";
			consol2.JK_UniqueConsignRef = "C789";

			var testHeader1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			testHeader1.AMA_JobReference = "VV1";
			testHeader1.AMA_ParentId = consol1.PK;
			testHeader1.AMA_ParentTableCode = consol1.TablePrefix;
			var testHeader2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			testHeader2.AMA_ApplicationCode = "OUT";
			testHeader2.AMA_JobReference = "VV2";
			testHeader2.AMA_ParentId = consol2.PK;
			testHeader2.AMA_ParentTableCode = consol2.TablePrefix;

			Factory.Save();

			AssertNotNull(AsycudaManifestHeaderDocWrapper.New(consol1, "", ""));
			AssertNull(AsycudaManifestHeaderDocWrapper.New(consol2, "", ""));
		}

		public void TestNew()
		{
			var consol = Factory.New<ForwardingConsol>();
			AssertNull(AsycudaManifestHeaderDocWrapper.New(consol, "", ""));
			var manifest1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var wrapper = AsycudaManifestHeaderDocWrapper.New(manifest1, "", "");
			AssertEquals(manifest1, wrapper.Manifest);
			manifest1.SetParent(consol);
			wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "", "");
			AssertEquals(manifest1, wrapper.Manifest);

			var manifest2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest2.SetParent(consol);
			var manifest3 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest3.SetParent(consol);
			manifest1.AMA_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-2);
			manifest2.AMA_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-3);
			manifest3.AMA_SystemCreateTimeUtc = ZDateTime.Today.AddMonths(-1);

			wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "", "");
			AssertEquals(manifest2, wrapper.Manifest);
		}

		public void TestNew_CheckCountry_UsingAdditionalMatchFilter()
		{
			var consol = Factory.New<ForwardingConsol>();

			var manifestER = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestER.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
			manifestER.SetParent(consol);

			var manifestZA = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifestZA.SetParent(consol);

			var manifestZA2 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifestZA2.SetParent(consol);

			var manifestZA3 = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
			manifestZA3.SetParent(consol);

			manifestZA.AMA_SystemCreateTimeUtc = ZDateTime.UtcNow;
			manifestZA2.AMA_SystemCreateTimeUtc = manifestZA.AMA_SystemCreateTimeUtc.AddHours(-1);
			manifestZA3.AMA_SystemCreateTimeUtc = manifestZA.AMA_SystemCreateTimeUtc.AddHours(1);

			manifestER.AMA_TransportMode = "SEA";
			manifestZA.AMA_TransportMode = "AIR";
			manifestZA2.AMA_TransportMode = "SeA";
			manifestZA3.AMA_TransportMode = "SEA";

			manifestER.AMA_ManifestType = "ABC";
			manifestZA.AMA_ManifestType = "ABC";
			manifestZA2.AMA_ManifestType = "aBc";
			manifestZA3.AMA_ManifestType = "DEF";

			var wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMANMOD|ERABCSEA");
			AssertEquals(manifestER, wrapper.Manifest);
			AssertNull(AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMANMOD|ERABC"));

			wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMAN|ERABC");
			AssertEquals(manifestER, wrapper.Manifest);
			AssertNull(AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMAN|ER"));

			wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMOD|ERSEA");
			AssertEquals(manifestER, wrapper.Manifest);
			AssertNull(AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMOD|ER"));

			wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTY|ER");
			AssertEquals(manifestER, wrapper.Manifest);
			AssertNull(AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTY|VU"));

			wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMANMOD|ZAABCSEA");
			AssertEquals(manifestZA2, wrapper.Manifest);

			wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMANMOD|ZAABCAIR");
			AssertEquals(manifestZA, wrapper.Manifest);
			AssertNull(AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMANMOD|ZAABC"));

			wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMAN|ZAABC");
			AssertEquals(manifestZA2, wrapper.Manifest);

			wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMAN|ZADEF");
			AssertEquals(manifestZA3, wrapper.Manifest);
			AssertNull(AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMAN|ZA"));

			wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMOD|ZASEA");
			AssertEquals(manifestZA2, wrapper.Manifest);

			wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMOD|ZAAIR");
			AssertEquals(manifestZA, wrapper.Manifest);
			AssertNull(AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTYMOD|ZA"));

			wrapper = AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTY|ZA");
			AssertEquals(manifestZA2, wrapper.Manifest);
			AssertNull(AsycudaManifestHeaderDocWrapper.New(consol, "HELLO WORLD", "AdditionalMatch=ASYCTY|VU"));
		}

		public void TestNew_CheckCountry_UsingMenuItemName()
		{
			AssertCountry_EUManifest();
			AssertCountry_INManifest();
			AssertCountry_TRManifest();
			AssertCountry_ZAManifest();

			void AssertCountry_EUManifest()
			{
				var manifestEU = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				manifestEU.AMA_RN_NKCountry = Core.Constants.CountryCodes.Spain;
				var wrapper = AsycudaManifestHeaderDocWrapper.New(manifestEU, AsycudaManifestHeaderDocWrapper.EUICS2ManifestDocument, "");
				AssertEquals(manifestEU, wrapper.Manifest);
				var manifestNonEU = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.TWManifest.IAsycudaManifestHeader>();
				manifestNonEU.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
				AssertNull(AsycudaManifestHeaderDocWrapper.New(manifestNonEU, AsycudaManifestHeaderDocWrapper.EUICS2ManifestDocument, ""));
			}

			void AssertCountry_ZAManifest()
			{
				var consol = Factory.New<ForwardingConsol>();

				var manifestZA = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>();
				manifestZA.SetParent(consol);
				var wrapper = AsycudaManifestHeaderDocWrapper.New(manifestZA, AsycudaManifestHeaderDocWrapper.ZAManifestWithBarcode, "");
				AssertEquals(manifestZA, wrapper.Manifest);
				wrapper = AsycudaManifestHeaderDocWrapper.New(consol, AsycudaManifestHeaderDocWrapper.ZAManifestWithBarcode, "");
				AssertEquals(manifestZA, wrapper.Manifest);
			}

			void AssertCountry_TRManifest()
			{
				var manifestTR = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader>();
				manifestTR.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
				var wrapperTR = AsycudaManifestHeaderDocWrapper.New(manifestTR, AsycudaManifestHeaderDocWrapper.TRManifestBillsforEMANIF, "");
				AssertEquals(manifestTR, wrapperTR.Manifest);
				wrapperTR = AsycudaManifestHeaderDocWrapper.New(manifestTR, AsycudaManifestHeaderDocWrapper.TRManifestBills, "");
				AssertEquals(manifestTR, wrapperTR.Manifest);
				wrapperTR = AsycudaManifestHeaderDocWrapper.New(manifestTR, AsycudaManifestHeaderDocWrapper.TRManifestBillsLines, "");
				AssertEquals(manifestTR, wrapperTR.Manifest);
				wrapperTR = AsycudaManifestHeaderDocWrapper.New(manifestTR, AsycudaManifestHeaderDocWrapper.TRManifestPrint, "");
				AssertEquals(manifestTR, wrapperTR.Manifest);
			}

			void AssertCountry_INManifest()
			{
				var manifestIN = (AsycudaManifestHeader)Factory.New<Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaManifestHeader>();
				manifestIN.AMA_RN_NKCountry = Core.Constants.CountryCodes.India;
				var wrapperIN = AsycudaManifestHeaderDocWrapper.New(manifestIN, AsycudaManifestHeaderDocWrapper.INManifestAirCGM, "");
				AssertEquals(manifestIN, wrapperIN.Manifest);
			}
		}

		public void TestManifestDate()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var message1 = manifest.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message1.EM_SystemCreateTimeUtc = new ZDateTime(2017, 3, 11);
			var message2 = manifest.Messages.AddNew();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message2.EM_SystemCreateTimeUtc = new ZDateTime(2017, 3, 12);
			var message3 = manifest.Messages.AddNew();
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_SystemCreateTimeUtc = new ZDateTime(2017, 3, 13);
			AssertEquals(Env.Time.GetLocalTimeFromUtc(new DateTime(2017, 3, 12)), AsycudaManifestHeaderDocWrapper.New(manifest, "", "").ManifestDate);
		}

		public void TestDriver()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var person1 = manifest.Persons.AddNew();
			person1.CPN_IsPassenger = true;
			AssertEquals(person1, AsycudaManifestHeaderDocWrapper.New(manifest, "", "").Driver);
			var person2 = manifest.Persons.AddNew();
			person2.CPN_IsPassenger = true;
			var person3 = manifest.Persons.AddNew();
			person3.CPN_IsPassenger = false;
			AssertEquals(person3, AsycudaManifestHeaderDocWrapper.New(manifest, "", "").Driver);
		}

		public void TestTotalGrossWeight()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();
			pack1.APA_PackQty = 1;
			pack2.APA_PackQty = 2;
			var item11 = pack1.PackedItems.AddNewPackedItem();
			var item12 = pack1.PackedItems.AddNewPackedItem();
			var item21 = pack2.PackedItems.AddNewPackedItem();
			var item22 = pack2.PackedItems.AddNewPackedItem();

			item11.API_GrossWeight = new ZDecimal(5.15);
			item12.API_GrossWeight = new ZDecimal(500);
			item21.API_GrossWeight = new ZDecimal(600);
			item22.API_GrossWeight = new ZDecimal(5.15);

			item11.API_GrossWeightUQ = "KG";
			item12.API_GrossWeightUQ = "G";
			item21.API_GrossWeightUQ = "G";
			item22.API_GrossWeightUQ = "KG";

			AssertEquals(new ZString("11.40"), AsycudaManifestHeaderDocWrapper.New(manifest, "", "").TotalGrossWeightInKilograms);
		}

		public void TestTotalPackQuantity()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifest.Bills.AddNew();
			var pack1 = bill.Packs.AddNew();
			var pack2 = bill.Packs.AddNew();
			pack1.APA_PackQty = 1;
			pack2.APA_PackQty = 2;
			AssertEquals((ZInt)3, AsycudaManifestHeaderDocWrapper.New(manifest, "", "").TotalPackQuantity);
		}

		public void TestLoadAndDischargePorts()
		{
			var manifest = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifest.AMA_RL_NKPortOfLoading = "SGSIN";
			manifest.AMA_RL_NKPortOfDischarge = "AUSCL";
			AssertEquals(AsycudaManifestHeaderDocWrapper.New(manifest, "", "").LoadPortName, "Singapore");
			AssertEquals(AsycudaManifestHeaderDocWrapper.New(manifest, "", "").DischargePortName, "Clare");
			manifest.AMA_RL_NKPortOfLoading = "AUSCL";
			manifest.AMA_RL_NKPortOfDischarge = "SGSIN";
			AssertEquals(AsycudaManifestHeaderDocWrapper.New(manifest, "", "").LoadPortName, "Clare");
			AssertEquals(AsycudaManifestHeaderDocWrapper.New(manifest, "", "").DischargePortName, "Singapore");
		}

		public void TestCustomsLoadAndCustomsDischargePorts()
		{
			AddCustomsPort();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var manifest = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Turkey, "ATAIHR");
				manifest.AMA_RL_NKPortOfLoading = "TRIST";
				manifest.AMA_CustomsLoadPort = "TRIST-001";
				manifest.AMA_RL_NKPortOfDischarge = "AUSCL";
				AssertEquals(AsycudaManifestHeaderDocWrapper.New(manifest, "", "").DischargePortName, "Clare");
				AssertEquals(AsycudaManifestHeaderDocWrapper.New(manifest, "", "").CustomsLoadPortName, "AHIRKAPI DEMİR MEVKİİ");

				manifest.AMA_RL_NKPortOfLoading = "SGSIN";
				manifest.AMA_RL_NKPortOfDischarge = "TRIST";
				manifest.AMA_CustomsDischargePort = "TRIST-002";
				manifest.AMA_Nature = ShipmentTypeList.Codes.Import23;
				AssertEquals(AsycudaManifestHeaderDocWrapper.New(manifest, "", "").LoadPortName, "Singapore");
				AssertEquals(AsycudaManifestHeaderDocWrapper.New(manifest, "", "").CustomsDischargePortName, "ATAKÖY MARİNA");
			}
		}

		public void TestCompanyName()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var wrapper = AsycudaManifestHeaderDocWrapper.New(header, "TR Carrier Manifest", "");
				AssertEquals("Eagle Datamation International", wrapper.CompanyName);
			}
		}

		public void TestCompanyAddress()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var wrapper = AsycudaManifestHeaderDocWrapper.New(header, "TR Carrier Manifest", "");
				AssertEquals("10 HUTCHESON STREET ALBION QLD 4010 AUSTRALIA", wrapper.CompanyAddress);
			}
		}
		public void TestConveyanceNationality()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Turkey))
			{
				AddVessel();
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_VesselName = "TestCode";
				var wrapper = AsycudaManifestHeaderDocWrapper.New(header, "TR Carrier Manifest", "");
				AssertEquals("DE", wrapper.ConveyanceNationality);
			}
		}

		void AddCustomsPort()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.Port, "PORT");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, RefCusCodeListTypes.Codes.Port, "TRIST-001", "AHIRKAPI DEMİR MEVKİİ", new ZDateTime(2019, 10, 31), new ZDateTime(2028, 1, 30));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Turkey, RefCusCodeListTypes.Codes.Port, "TRIST-002", "ATAKÖY MARİNA", new ZDateTime(2019, 10, 31), new ZDateTime(2028, 10, 30));

			var map1 = Factory.New<RefLocoMap>();
			map1.RY_IsSystem = (ZBool)true;
			map1.RY_LocalPortCode = "TRIST-001";
			map1.RY_RL_NKLocoPort = "TRIST";
			map1.RY_RN = new ZGuid("A12B3D34-08FE-4AF5-A8D6-5B99E9AAB655");
			map1.RY_IsSystem = (ZBool)true;
			map1.RY_SystemUsage = "CUS";

			var map2 = Factory.New<RefLocoMap>();
			map2.RY_IsSystem = (ZBool)true;
			map2.RY_LocalPortCode = "TRIST-002";
			map2.RY_RL_NKLocoPort = "TRIST";
			map2.RY_RN = new ZGuid("A12B3D34-08FE-4AF5-A8D6-5B99E9AAB655");
			map2.RY_IsSystem = (ZBool)true;
			map2.RY_SystemUsage = "CUS";

			var map3 = Factory.New<RefLocoMap>();
			map3.RY_IsSystem = (ZBool)true;
			map3.RY_LocalPortCode = "TRIZT-001";
			map3.RY_RL_NKLocoPort = "TRIZT";
			map3.RY_RN = new ZGuid("A12B3D34-08FE-4AF5-A8D6-5B99E9AAB655");
			map3.RY_IsSystem = (ZBool)true;
			map3.RY_SystemUsage = "CUS";

			var map4 = Factory.New<RefLocoMap>();
			map4.RY_IsSystem = (ZBool)true;
			map4.RY_LocalPortCode = "TRIZT-002";
			map4.RY_RL_NKLocoPort = "TRIZT";
			map4.RY_RN = new ZGuid("A12B3D34-08FE-4AF5-A8D6-5B99E9AAB655");
			map4.RY_IsSystem = (ZBool)true;
			map4.RY_SystemUsage = "CUS";

			Factory.Save();
		}

		void AddVessel()
		{
			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "TestVessel";
			vessel.RV_Code = "TestCode";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Germany;
			Factory.Save();
		}
	}
}
