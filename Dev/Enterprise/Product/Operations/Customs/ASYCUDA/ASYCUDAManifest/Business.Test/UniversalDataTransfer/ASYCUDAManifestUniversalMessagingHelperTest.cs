using System.Collections.Generic;
using System.Linq;
using System.Xml;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.ASYCUDAManifest.Business.UniversalDataTransfer.Testing
{
	sealed class ASYCUDAManifestUniversalMessagingHelperTest : TestCaseWithFactory
	{
		[TestDate(2017, 04, 21)]
		public void TestVUContainerCodeMapping()
		{
			AsycudaManifestUniversalMessagingHelperTest.PrepareCusCodeDataForTesting(Factory);

			const string countryCode = "VU";
			const string containerISOCode = "20G1";
			const string containerVUCustomCode = "20GP";

			var refMapType = Factory.New<RefCusMapType>();
			refMapType.ZZP_MapType = RefCusMapTypeList.Codes.CTYPE;
			refMapType.ZZP_Direction = "OUT";
			refMapType.ZZP_Description = "Container Type Mapping";
			refMapType.ZZP_IsReadonly = false;

			var refContainer = Factory.NewWithValidTestData<RefContainer>();
			refContainer.RC_ISOType = containerISOCode;

			var refCusMapType = Factory.New<ZZRefCusMapCombined>();
			refCusMapType.ZZM_CW1orCommercialValue = containerISOCode;
			refCusMapType.ZZM_CustomsValue = containerVUCustomCode;
			refCusMapType.ZZM_IsSystem = false;
			refCusMapType.ZZM_ZZP_NKMapType = RefCusMapTypeList.Codes.CTYPE;
			refCusMapType.ZZM_ZZZ_NKDataGrouping = countryCode;
			refCusMapType.ZZM_StartDate = new ZDate(2010, 1, 1);
			refCusMapType.ZZM_EndDate = new ZDate(2079, 06, 06);

			Factory.Save();

			var customCode = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(
				Factory,
				countryCode,
				RefCusMapTypeList.Codes.CTYPE,
				containerISOCode,
				ZDateTime.Today);
			AssertEquals("Custom mapping code", containerVUCustomCode, customCode);

			var source = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting("VUVLI", "ASY", Factory);
			source.AMA_RN_NKConveyanceNationality = ZString.Empty;
			source.AMA_VesselName = "ADMIRALENGRACHT";
			source.Vessel.RV_CarrierCode = "CC";
			source.Vessel.RV_YearOfConstruction = 1979;
			source.Vessel.RV_RadioCallSign = "KBBL";
			source.Vessel.RV_NetRegisterTon = 69000;
			source.AMA_MasterInformation = "Captain Kirk";
			source.AMA_MasterBill = "Man-Fred";
			source.RegistrationDate = ZDateTime.BrettsBirthday;
			source.RegistrationNumber = "Entry 123";
			source.RegistrationStatus = "ST1";
			source.AMA_Trailer1RegNo = "TRAILER001";
			source.AMA_Trailer2RegNo = "TRAILER002";
			source.AMA_RN_NKTrailer1RegCountry = "TR";
			source.AMA_RN_NKTrailer2RegCountry = "ZA";

			source.Containers[0].ACN_StowageLocation = "MISSION";
			source.Containers[0].ACN_GoodsWeight = 69m;
			source.Containers[0].ACN_GoodsWeightUQ = Core.Constants.Weight.Pounds;
			source.Containers[0].ACN_RC_ContainerType = refContainer.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "MASTER123";
			source.SetParent(consol);
			source.Bills[0].ABL_ManifestUQ = "DJC";
			var pack = source.Bills[0].Packs.AddNew();
			pack.APA_GoodsDescription = "Desc";
			pack.APA_MarksAndNumbers = "Marks";
			pack.APA_CommodityCode = "COM";
			pack.APA_PackQty = 11;
			pack.APA_PackUQ = "PX";
			pack.APA_Volume = 3m;
			pack.APA_VolumeUQ = "M3";
			pack.APA_Weight = 6.9m;
			pack.APA_WeightUQ = "KG";
			pack.ContainerPK = source.Containers[0].PK;
			pack.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "0004", "a", "IMO").First().PK;
			new ASYCUDAManifestUniversalMessagingHelper(new TestLoggerForTest()).SendViaEHub(source, "", MessageSubTypeCodes.Codes.Original, new List<IMessageParent>());
			var queuedMessage = source.Messages[0];
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(queuedMessage.EM_MessageText);
			Assert(xmlDocument.InnerXml != null);
			var xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);
			xmlnsManager.AddNamespace("n", "http://www.cargowise.com/Schemas/Universal/2011/11");

			var nodes = xmlDocument.SelectNodes("/n:UniversalShipment/n:Shipment/n:ContainerCollection/n:Container/n:ContainerType/n:ISOCode", xmlnsManager);
			AssertNotNull("nodes should not be null", nodes);
			AssertEquals("Mapped VU custom code", containerVUCustomCode, nodes[0].InnerText);
		}

		[TestDate(2020, 02, 25)]
		public void TestVUSealPartCodeMapping()
		{
			const string countryCode = Core.Constants.CountryCodes.Vanuatu;
			const string sealPartyCode = "SOT";
			const string sealPartyCustomCode = "CTO";

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusMapType(RefCusMapTypeList.Codes.STYPE, MapDirectionList.Codes.BTH, "Description", false);

			var refCusMapType = Factory.New<ZZRefCusMapCombined>();
			refCusMapType.ZZM_CW1orCommercialValue = sealPartyCode;
			refCusMapType.ZZM_CustomsValue = sealPartyCustomCode;
			refCusMapType.ZZM_IsSystem = false;
			refCusMapType.ZZM_ZZP_NKMapType = RefCusMapTypeList.Codes.STYPE;
			refCusMapType.ZZM_ZZZ_NKDataGrouping = countryCode;
			refCusMapType.ZZM_StartDate = new ZDate(2010, 1, 1);
			refCusMapType.ZZM_EndDate = new ZDate(2079, 06, 06);

			Factory.Save();

			var customCode = ZZRefCusMapCombined.MapCW1CodeToCustomsCode(
				Factory,
				countryCode,
				RefCusMapTypeList.Codes.STYPE,
				sealPartyCode,
				ZDateTime.Today);
			AssertEquals("Custom mapping code", sealPartyCustomCode, customCode);

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = countryCode;
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = countryCode;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_ManifestType = "ASY";

			var container = header.Containers.AddNew();
			container.ACN_SealingPartyType = sealPartyCode;

			new ASYCUDAManifestUniversalMessagingHelper(new TestLoggerForTest()).SendViaEHub(header, "", MessageSubTypeCodes.Codes.Original, new List<IMessageParent>());
			var queuedMessage = header.Messages[0];
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(queuedMessage.EM_MessageText);
			Assert(xmlDocument.InnerXml != null);
			var xmlnsManager = new XmlNamespaceManager(xmlDocument.NameTable);
			xmlnsManager.AddNamespace("n", "http://www.cargowise.com/Schemas/Universal/2011/11");

			var node = xmlDocument.SelectSingleNode("/n:UniversalShipment/n:Shipment/n:ContainerCollection/n:Container/n:AddInfoCollection/n:AddInfo/n:Value[../n:Key='ACN_SealingPartyType']/text()", xmlnsManager);
			AssertNotNull("node should not be null", node);
			AssertEquals("Mapped VU Seal Party custom code", sealPartyCustomCode, node.InnerText);
		}

		public void TestSendViaEHub()
		{
			var source = AsycudaManifestUniversalMessagingHelperTest.SetUpManifestForEhubTest(Factory, "VUVLI", "ASY");
			new ASYCUDAManifestUniversalMessagingHelper(new TestLoggerForTest()).SendViaEHub(source, "", MessageSubTypeCodes.Codes.Original, new List<IMessageParent>());
			var queuedMessage = source.Messages[0];
			var interchange = queuedMessage.Interchange;
			var interchangeText = interchange.EI_BodyText;

			AssertEquals("UDM", queuedMessage.EM_ApplicationCode);
			AssertEquals("XUS", queuedMessage.EM_MessageType);
			AssertEquals("XUS", queuedMessage.EM_MessageSubType);
			AssertEquals("TRX", queuedMessage.EM_ReceiveTransmit);
			AssertEquals("SNT", queuedMessage.EM_Status);
			AssertEquals("HQU", queuedMessage.Interchange.EI_Status);
			AssertEquals("ASYCUDA", queuedMessage.Interchange.EI_To);
			AssertNotEquals("", queuedMessage.Interchange.eHubID);
			AssertMultilineASCIIEquals("queuedMessage.EM_MessageText", string.Format(AsycudaManifestUniversalMessagingHelperTest.SampleUxml, interchange.EI_SessionGUID, interchange.EI_InterchangeNum, queuedMessage.EM_MessageNum), queuedMessage.EM_MessageText);
			AssertContains(">ZVU<", interchangeText);
			AssertContains("</UniversalInterchange>", interchangeText);
			AssertContains("</UniversalInterchange>", interchangeText);
			AssertContains("</UniversalShipment>", interchangeText);
			AssertContains("<EmailSubject>recipient.user@forwarder.com</EmailSubject>", interchange.EI_HeaderText);
			AssertContains("Interpretation contains HTML", @"<span class=""start-tag"">", queuedMessage.EM_MessageInterpretation);
			AssertEquals("Haschanges", false, queuedMessage.HasChanges);
		}

		public void TestSendViaEHubWithPackageMappings()
		{
			var factoryForSetup = new BusinessObjectFactory();
			AsycudaManifestUniversalMessagingHelperTest.PrepareCusCodeDataForTesting(factoryForSetup);

			AsycudaBillForRegularBillValidationTest.FindOrMakeRefPackAndCusCodeList("DJC", "AAA", "VU", factoryForSetup);
			AsycudaBillForRegularBillValidationTest.FindOrMakeRefPackAndCusCodeList("DJC", "BBB", "SB", factoryForSetup);
			AsycudaBillForRegularBillValidationTest.FindOrMakeRefPackAndCusCodeList("DJC", "CCC", "SB", factoryForSetup, alsoMakeZzRecordToo: false);
			factoryForSetup.Save();

			var vuMessage = GetMessageWithPackageMappings("VUVLI", "ASY", "SBHIR");

			AssertContains(@"<CustomsPackType>              <Code>AAA</Code>", vuMessage);
			AssertContains(@"<PackType>              <Code>DJC", vuMessage);

			var sbMessage = GetMessageWithPackageMappings("SBHIR", "ASY", "VUVLI");

			AssertContains("Should contain BBB, not CCC; although two maps exist only the BBB one is in the target list",
				@"<CustomsPackType>              <Code>BBB</Code>", sbMessage);
			AssertContains(@"<PackType>              <Code>DJC", sbMessage);
		}

		public void TestLogMessageStatusChangeEvent()
		{
			var countryCode = Core.Constants.CountryCodes.Vanuatu;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, countryCode, "Singapore", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var source = AsycudaManifestUniversalMessagingHelperTest.SetUpManifestForEhubTest(Factory, "VUVLI", "ASY");
			var bill = source.Bills[0];
			var pack = bill.Packs[0];
			pack.PackedItemRelationshipOverrideForTesting = ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.Many;

			var packedItem = pack.PackedItemForTesting();
			new ASYCUDAManifestUniversalMessagingHelper(new TestLoggerForTest()).SendViaEHub(source, ZString.Empty, MessageSubTypeCodes.Codes.Original, new List<IMessageParent> { packedItem });

			AssertEquals(MessageStatusCodeList.Codes.Sent, packedItem.API_MessageStatus);
			var logQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.MessageStatusChangeCode);
			logQuery.AddToFilter(StmALogSchema.SL_Reference, MessageStatusCodeList.Codes.Sent);

			Assert("pack has event which type is 'MSC' and reference is 'SNT'", pack.Logs.HasLogWith(logQuery));
		}

		string GetMessageWithPackageMappings(ZString localPortCode, ZString manifestType, ZString foreignPortCode)
		{
			var header = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting(localPortCode, manifestType, Factory);
			header.AMA_RL_NKPortOfLoading = localPortCode;
			header.AMA_RL_NKPortOfDischarge = foreignPortCode;
			header.Bills[0].ABL_ManifestUQ = "DJC";

			var pack = header.Bills[0].Packs.AddNew();
			pack.APA_PackUQ = "DJC";
			pack.ContainerPK = header.Containers[0].PK;
			new ASYCUDAManifestUniversalMessagingHelper(new TestLoggerForTest()).SendViaEHub(header, string.Empty, MessageSubTypeCodes.Codes.Original, new List<IMessageParent>());

			var messages = header.Messages;
			messages.Reload(true);

			return messages[0].EM_MessageText.Replace("\r\n", ZString.Empty);
		}

		sealed class TestLoggerForTest : INotifications
		{
			public void Add(INotification notification)
			{
			}
		}
	}
}
