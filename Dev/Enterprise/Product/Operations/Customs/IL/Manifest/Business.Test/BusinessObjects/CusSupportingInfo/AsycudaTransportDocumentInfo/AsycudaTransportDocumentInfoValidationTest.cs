using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IL.Business;
using Enterprise.Customs.IL.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IL.Manifest.Business.Testing
{
	sealed class AsycudaTransportDocumentInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCSI_CodeUserInterfaceValidation_CC_BR1_WCO_090()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var transportDocumentIL2 = bill.TransportDocuments.AddNew();
			transportDocumentIL2.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL2;

			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			AssertCC_BR1_WCO_090(transportDocumentIL2, true);
			var transportDocumentIL1 = bill.TransportDocuments.AddNew();
			transportDocumentIL1.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL1;
			transportDocumentIL2.Validation.ValidateCSI_CodeUserInterface();
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.Road;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);
			var transportDocumentIL3 = bill.TransportDocuments.AddNew();
			transportDocumentIL3.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL3;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			bill.TransportDocuments.RemoveAndDelete(transportDocumentIL1);
			bill.TransportDocuments.RemoveAndDelete(transportDocumentIL3);

			header.AMA_TransportMode = Core.Constants.TransportModes.Air;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.AirSea;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.All;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.Unknown;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.BorderWaterBorne;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.Auto;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.Courier;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.Mail;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.Other;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.OwnPropulsion;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.PassengerHandCarried;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.Pedestrian;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.RollOnRollOff;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.Storage;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.Truck;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);

			header.AMA_TransportMode = Core.Constants.TransportModes.WarehouseHandling;
			AssertCC_BR1_WCO_090(transportDocumentIL2, false);
		}

		public void TestCSI_ReferenceNumberUserInterfaceValidation_CC_BR1_WCO_091()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var transportDocument = bill.TransportDocuments.AddNew();

			transportDocument.CSI_ReferenceNumberUserInterface = "123";
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL1;
			AssertCC_BR1_WCO_091(transportDocument, true, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL2;
			AssertCC_BR1_WCO_091(transportDocument, true, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL3;
			AssertCC_BR1_WCO_091(transportDocument, false, true);

			transportDocument.CSI_ReferenceNumberUserInterface = "I123";
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL1;
			AssertCC_BR1_WCO_091(transportDocument, true, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL2;
			AssertCC_BR1_WCO_091(transportDocument, true, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL3;
			AssertCC_BR1_WCO_091(transportDocument, false, true);

			transportDocument.CSI_ReferenceNumberUserInterface = "I123456789";
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL1;
			AssertCC_BR1_WCO_091(transportDocument, false, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL2;
			AssertCC_BR1_WCO_091(transportDocument, false, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL3;
			AssertCC_BR1_WCO_091(transportDocument, false, true);

			transportDocument.CSI_ReferenceNumberUserInterface = "A123456789";
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL1;
			AssertCC_BR1_WCO_091(transportDocument, true, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL2;
			AssertCC_BR1_WCO_091(transportDocument, true, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL3;
			AssertCC_BR1_WCO_091(transportDocument, false, true);

			transportDocument.CSI_ReferenceNumberUserInterface = "I123456789012345";
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL1;
			AssertCC_BR1_WCO_091(transportDocument, true, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL2;
			AssertCC_BR1_WCO_091(transportDocument, true, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL3;
			AssertCC_BR1_WCO_091(transportDocument, false, false);

			transportDocument.CSI_ReferenceNumberUserInterface = "A123456789012345";
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL1;
			AssertCC_BR1_WCO_091(transportDocument, true, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL2;
			AssertCC_BR1_WCO_091(transportDocument, true, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL3;
			AssertCC_BR1_WCO_091(transportDocument, false, true);

			transportDocument.CSI_ReferenceNumberUserInterface = "I1234567890123456";
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL1;
			AssertCC_BR1_WCO_091(transportDocument, true, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL2;
			AssertCC_BR1_WCO_091(transportDocument, true, false);
			transportDocument.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL3;
			AssertCC_BR1_WCO_091(transportDocument, false, true);
		}

		public void TestCSI_ReferenceNumberUserInterfaceMandatory()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var transportDocument = bill.TransportDocuments.AddNew();

			transportDocument.Validation.ValidateCSI_ReferenceNumberUserInterface();
			AssertHasMessageErrorContaining(transportDocument.CSI_ReferenceNumberUserInterfaceInfo, "not entered");

			transportDocument.CSI_ReferenceNumberUserInterface = "123";
			AssertNoMessageErrorContaining(transportDocument.CSI_ReferenceNumberUserInterfaceInfo, "not entered");
		}

		public void TestCheckCSI_CodeUserInterfaceDuplicationValidation()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var list = new TransportDocsTypeList();
			foreach (var code in list.GetAllCodes())
			{
				AssertDuplicationError(bill, code);
			}
		}

		public void TestCheckCSI_ReferenceNumberUserInterface_UniqueIL1ForwarderDeal()
		{
			const string exepctedMesage = "There is another Global Manifest with the same Manifest Number and Forwarder Deal Number – Review Manifest";
			var factory = Factory;
			var (orgAddressShippingAgent, orgAddressDeclarant) = CreatePartners();
			var headerA = CreateNewHeader("headerA", orgAddressShippingAgent, orgAddressDeclarant);
			var billA1 = headerA.Bills.AddNew();
			billA1.ABL_SequenceNumber = 1;

			var billA2 = headerA.Bills.AddNew();
			billA2.ABL_SequenceNumber = 2;

			headerA.AMA_ManifestNumber = "123456";
			headerA.AMA_TransportMode = "SEA";
			factory.Save();

			var transportDocumentA2IL1 = billA2.TransportDocuments.Single(x => x.CSI_Code == "IL1");
			transportDocumentA2IL1.CSI_ReferenceNumberUserInterface = "I123456A01";
			AssertHasError("When headerA Is Sea, and CSI_ReferenceNumberUserInterface exist in the same AsycudaManifestHeader", transportDocumentA2IL1.CSI_ReferenceNumberUserInterfaceInfo, $"{exepctedMesage} 'headerA'");

			headerA.AMA_TransportMode = "AIR";
			transportDocumentA2IL1.Validation.ValidateCSI_ReferenceNumberUserInterface();
			AssertNoError("When headerA Is not Sea, and CSI_ReferenceNumberUserInterface exist in the same AsycudaManifestHeader", transportDocumentA2IL1.CSI_ReferenceNumberUserInterfaceInfo, $"{exepctedMesage} 'headerA'");

			transportDocumentA2IL1.CSI_ReferenceNumberUserInterface = "I123456A02";
			headerA.AMA_TransportMode = "SEA";
			factory.Save();

			var headerB = CreateNewHeader("headerB", orgAddressShippingAgent, orgAddressDeclarant);
			var billB1 = headerA.Bills.AddNew();
			billB1.ABL_SequenceNumber = 1;

			headerA.AMA_ManifestNumber = "123456";
			headerA.AMA_TransportMode = "SEA";
			factory.Save();

			var transportDocumentB1IL1 = billB1.TransportDocuments.Single(x => x.CSI_Code == "IL1");
			transportDocumentB1IL1.CSI_ReferenceNumberUserInterface = "I123456A01";
			AssertHasError("When headerB Is Sea, same ManifestNumber as headerB, and CSI_ReferenceNumberUserInterface exist in different AsycudaManifestHeader", transportDocumentB1IL1.CSI_ReferenceNumberUserInterfaceInfo, $"{exepctedMesage} 'headerA'");

			headerA.AMA_TransportMode = "AIR";
			transportDocumentB1IL1.Validation.ValidateCSI_ReferenceNumberUserInterface();
			AssertNoError("When headerB Is not Sea, same ManifestNumber as headerB, and CSI_ReferenceNumberUserInterface exist in different AsycudaManifestHeader", transportDocumentB1IL1.CSI_ReferenceNumberUserInterfaceInfo, $"{exepctedMesage} 'headerA'");

			headerA.AMA_ManifestNumber = ZString.Empty;
			headerA.AMA_TransportMode = "SEA";
			billB1 = headerA.Bills.AddNew();
			billB1.ABL_SequenceNumber = 1;
			transportDocumentB1IL1 = billB1.TransportDocuments.AddNew();
			transportDocumentB1IL1.CSI_CodeUserInterface = "IL1";
			transportDocumentB1IL1.CSI_ReferenceNumberUserInterface = "I123456A01";
			AssertNoError("When headerB Is Sea, not same ManifestNumber as headerB, and CSI_ReferenceNumberUserInterface exist in different AsycudaManifestHeader", transportDocumentB1IL1.CSI_ReferenceNumberUserInterfaceInfo, $"{exepctedMesage} 'headerA'");
		}

		public void TestCheckCSI_ReferenceNumberUserInterface3FirstDigits_EqualsFor_IL1FDN_And_IL2PDN()
		{
			const string exepctedMesage = "Shipping Agent in Forwarder Deal Number (IL1) do not match to Parent Deal Number (IL2)";
			var factory = Factory;
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var transportDocument1 = bill.TransportDocuments.AddNew();
			var transportDocument2 = bill.TransportDocuments.AddNew();

			transportDocument1.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL3;
			transportDocument1.CSI_ReferenceNumberUserInterface = "I123456789";
			AssertNoMessageError("When there Is no document with IL1 Type", transportDocument1.CSI_ReferenceNumberUserInterfaceInfo, exepctedMesage);

			transportDocument1.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL1;
			transportDocument1.Validation.ValidateCSI_ReferenceNumberUserInterface();
			AssertNoMessageError("When there Is a document with IL1 Type but no document with IL2 Type", transportDocument1.CSI_ReferenceNumberUserInterfaceInfo, exepctedMesage);

			transportDocument2.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL2;
			transportDocument2.CSI_ReferenceNumberUserInterface = "I987654321";
			AssertHasMessageError("When there Is a document with IL1 Type and document with IL2 Type and the first 3 digits in the references are different", transportDocument2.CSI_ReferenceNumberUserInterfaceInfo, exepctedMesage);

			transportDocument2.CSI_ReferenceNumberUserInterface = "I123654321";
			AssertNoMessageError("When there Is a document with IL1 Type and document with IL2 Type and the first 3 digits in the references are the same", transportDocument2.CSI_ReferenceNumberUserInterfaceInfo, exepctedMesage);
		}

		public void TestValidateAll()
		{
			const string exepctedMesage1 = "[CC_BR1_WCO_090] You must enter additional record of type \"IL1\" in case of using \"IL2\"";
			const string exepctedMesage2 = "Shipping Agent in Forwarder Deal Number (IL1) do not match to Parent Deal Number (IL2)";
			var factory = Factory;
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var transportDocumentIL2 = bill.TransportDocuments.AddNew();
			transportDocumentIL2.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL2;
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			factory.Save();

			var loadTransportDocumentIL2 = factory.LoadTop1<AsycudaTransportDocumentInfo>(new ZQuery(CusSupportingInfoSchema.PK, transportDocumentIL2.PK));
			AssertNotNull("Prerequisite", loadTransportDocumentIL2);
			loadTransportDocumentIL2.Validation.ValidateAll();
			AssertHasMessageError("When there Is a document with IL2 Type and not document with type IL1", loadTransportDocumentIL2.CSI_CodeUserInterfaceInfo, exepctedMesage1);
			AssertNoMessageError("When there Is a document with IL1 Type and document with IL2 Type and the first 3 digits in the references are different", loadTransportDocumentIL2.CSI_ReferenceNumberUserInterfaceInfo, exepctedMesage2);

			var transportDocumentIL1 = bill.TransportDocuments.AddNew();
			transportDocumentIL1.CSI_CodeUserInterface = TransportDocsTypeList.Codes.IL1;
			transportDocumentIL1.CSI_ReferenceNumberUserInterface = "I123456789";
			transportDocumentIL2.CSI_ReferenceNumberUserInterface = "I987654321";

			loadTransportDocumentIL2.Validation.ValidateAll();
			AssertNoMessageError("When there Is a document with IL2 Type and not document with type IL1", loadTransportDocumentIL2.CSI_CodeUserInterfaceInfo, exepctedMesage1);
			AssertHasMessageError("When there Is a document with IL1 Type and document with IL2 Type and the first 3 digits in the references are different", loadTransportDocumentIL2.CSI_ReferenceNumberUserInterfaceInfo, exepctedMesage2);
		}

		AsycudaManifestHeader CreateNewHeader(string jobReference, OrgAddress orgAddressShippingAgent, OrgAddress orgAddressDeclarant)
		{
			var factory = Factory;
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "IL";
			header.AMA_JobReference = jobReference;
			header.AMA_TransportMode = "SEA";
			header.AMA_ManifestNumber = "123456";
			header.AMA_OA_ShippingAgent = orgAddressShippingAgent.PK;
			header.AMA_OA_Declarant = orgAddressDeclarant.PK;

			return header;
		}

		(OrgAddress, OrgAddress) CreatePartners()
		{
			var factory = Factory;
			var orgHeaderShippingAgent = factory.NewWithValidTestData<OrgHeader>();
			var orgAddressShippingAgent = orgHeaderShippingAgent.MainAddress;
			orgHeaderShippingAgent.OH_Code = "SA";
			orgHeaderShippingAgent.CustomsCodes.AddNew("CCC", "123", "IL");

			var orgHeaderDeclarant = factory.NewWithValidTestData<OrgHeader>();
			var orgAddressDeclarant = orgHeaderDeclarant.MainAddress;
			orgHeaderDeclarant.OH_Code = "DEC";
			orgHeaderDeclarant.CustomsCodes.AddNew("CMP", "456", "IL");
			return (orgAddressShippingAgent, orgAddressDeclarant);
		}

		static void AssertDuplicationError(AsycudaBill bill, string code)
		{
			var transportDocument1 = bill.TransportDocuments.AddNew();
			transportDocument1.CSI_CodeUserInterface = code;
			AssertNoError(transportDocument1.CSI_CodeUserInterfaceInfo, ValidationCaptions.AsycudaTransportDocumentInfo.RuleTypeDuplication);

			var transportDocument2 = bill.TransportDocuments.AddNew();
			transportDocument2.CSI_CodeUserInterface = code;
			AssertHasError(transportDocument2.CSI_CodeUserInterfaceInfo, ValidationCaptions.AsycudaTransportDocumentInfo.RuleTypeDuplication);
		}

		void AssertCC_BR1_WCO_090(AsycudaTransportDocumentInfo transportDocument, bool expectIL1Message)
		{
			transportDocument.Validation.ValidateCSI_CodeUserInterface();
			if (expectIL1Message)
			{
				AssertHasMessageError(transportDocument.CSI_CodeUserInterfaceInfo, "[CC_BR1_WCO_090] You must enter additional record of type \"IL1\" in case of using \"IL2\"");
			}
			else
			{
				AssertNoMessageError(transportDocument.CSI_CodeUserInterfaceInfo, "[CC_BR1_WCO_090] You must enter additional record of type \"IL1\" in case of using \"IL2\"");
			}
		}

		void AssertCC_BR1_WCO_091(AsycudaTransportDocumentInfo transportDocument, bool expectIL1IL2Message, bool expectIL3Message)
		{
			transportDocument.Validation.ValidateCSI_ReferenceNumberUserInterface();
			if (expectIL1IL2Message)
			{
				AssertHasMessageError(transportDocument.CSI_ReferenceNumberUserInterfaceInfo, "[CC_BR1_WCO_091] Ocean Deal Number format is invalid, must start with \"I\" and following 9 chars");
			}
			else
			{
				AssertNoMessageError(transportDocument.CSI_ReferenceNumberUserInterfaceInfo, "[CC_BR1_WCO_091] Ocean Deal Number format is invalid, must start with \"I\" and following 9 chars");
			}

			if (expectIL3Message)
			{
				AssertHasMessageError(transportDocument.CSI_ReferenceNumberUserInterfaceInfo, "[CC_BR1_WCO_091] Ocean Deal Number format is invalid, must start with \"I\" and following 15 chars");
			}
			else
			{
				AssertNoMessageError(transportDocument.CSI_ReferenceNumberUserInterfaceInfo, "[CC_BR1_WCO_091] Ocean Deal Number format is invalid, must start with \"I\" and following 15 chars");
			}
		}
	}
}
