using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using NUnit.Framework;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.EU.EMCS.DataTransfer.Testing
{
	class EMCSDeclarationDataObjectReaderTest : OrganizationAddressTestHelper
	{
		[TestDate(2018, 3, 19, 12, 12, 12)]
		public void TestReadIntoBusinessObject()
		{
			var classificationJonno = Factory.New<CusClassification>();
			classificationJonno.FillWithValidTestData();
			classificationJonno.CC_TariffNum = "0002.02.02 1";
			classificationJonno.CC_LookupCode = "JONNO";
			classificationJonno.CC_ClassificationType = "BTH";
			Factory.SaveForTesting();

			var jobData = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			jobData.MessagingApplicationCode = new CodeDescriptionPair { Code = "EMC" };
			jobData.DeclarantType = new CodeDescriptionPair { Code = EMCSEntryTypeList.Codes.Consignee };
			jobData.MessageType = new CodeDescriptionPair { Code = EMCSJobDeclaration.EMCSMessageTypeCode };
			jobData.MessageSubType = new CodeDescriptionPair { Code = EMCSDestinationTypeList.Codes.DestinationDirectDelivery };
			jobData.TransportMode = new CodeDescriptionPair { Code = Core.Constants.TransportModes.Air };
			jobData.PaymentMethod = new CodeDescriptionPair { Code = DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration };
			jobData.EntryStatus = new EntryStatus { Code = EntryStatusList.Codes.REG };
			jobData.MessageStatus = new CodeDescriptionPair { Code = Common.Shared.MessageStatusList.Codes.AwaitingOriginal };
			jobData.SetDateCollection(() => new List<Date>(new[]
			{
				Date.New(DateType.EntryAuthorisation, false, new ZDateTime(2018, 09, 09, 11, 11, 11)),
				Date.New(DateType.EntrySubmitted, false, new ZDateTime(2018, 09, 09, 10, 10, 10)),
				Date.New(DateType.Departure, false, new ZDateTime(2018, 09, 09, 12, 12, 12))
			}));
			jobData.SetAddInfoCollection(() => new List<AddInfo>(new[]
			{
				AddInfo.New("JourneyTime", "2H"),
				AddInfo.New("TransportArrangement", EMCSTransportArrangementList.Codes.OwnerOfGoods),
				AddInfo.New("OriginType", EMCSOriginTypeList.Codes.Import),
				AddInfo.New("DeferredSubmission", EMCSDeferredSubmissionList.Codes.Yes),
				AddInfo.New("GuarantorType", EMCSGuarantorTypeList.Codes.OwnerOfTheExciseProducts)
			}));
			jobData.CommercialInfo = new CommercialInfo
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
				{
					new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
					{
						InvoiceNumber = "INV1234", InvoiceDate = new ZDateTime(2018, 09, 09, 09, 09, 09),
					}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
						new DataObjectList<CommercialInvoiceLine>
						{
							new CommercialInvoiceLine
							{
								LineNo = 1, BrandName = "brand", ClassificationCode = "JONNO", CountryOfOrigin = new Country { Code = "AU" }, CustomsQuantity = 100m, CustomsQuantityUnit = new CodeDescriptionPair6Char { Code = EMCSCustomsQuantityTypeList.Codes.FifteenLitre },
								LocalDescription = "line desc", InvoiceQuantity = 111m, InvoiceQuantityUnit = new CodeDescriptionPair { Code = "VQ" }, NetWeight = 121m, PartNo = "partno", Weight = 122m, WeightUnit = new UnitOfWeight { Code = "KG" },
								AddInfoCollection = new List<AddInfo>
								{
									AddInfo.New("ExciseProductCode", EMCSJobComInvoiceLine.ExciseProductCode_W200),
									AddInfo.New("FiscalMark", "mark"),
									AddInfo.New("FiscalMarkUsed", "Y"),
									AddInfo.New("AlcoholicStrength", "1"),
									AddInfo.New("DegreePlato", "2"),
									AddInfo.New("SizeOfProducer", "3"),
									AddInfo.New("Density", "4"),
									AddInfo.New("WineCategory", EMCSWineCategoryList.Codes.ImportedWine),
									AddInfo.New("GrowingZone", EMCSGrowingZoneList.Codes.A),
									AddInfo.New("WineCountryOrigin", Core.Constants.CountryCodes.Canada),
									AddInfo.New("WineDetailsComments", "Wine Details Comments")
								},
								CustomsReferenceCollection = new List<CustomsReference>
								{
									new CustomsReference { Type = new CodeDescriptionPair { Code = Business.CusCodeDataTypeList.Codes.WineCode }, SubType = new CodeDescriptionPair35Char { Code = "123" } },
									new CustomsReference { Type = new CodeDescriptionPair { Code = Business.CusCodeDataTypeList.Codes.WineCode }, SubType = new CodeDescriptionPair35Char { Code = "456" } }
								}
							}
						}))
				})
			};
			jobData.SetCustomsSupportingInformationCollection(() => new List<CustomsSupportingInformation>(new[]
			{
				new CustomsSupportingInformation { Type = new CodeDescriptionPair6Char { Code = Common.EU.CusSupportingInfoTypeList.Codes.ImportSad }, Category = new CodeDescriptionPair { Code = Common.EU.CusSupportingInfoTypeList.Codes.ImportSad }, Description = "SAD123" },
				new CustomsSupportingInformation { Type = new CodeDescriptionPair6Char { Code = Common.EU.CusSupportingInfoTypeList.Codes.ImportSad }, Category = new CodeDescriptionPair { Code = Common.EU.CusSupportingInfoTypeList.Codes.ImportSad }, Description = "SAD456" },
				new CustomsSupportingInformation { Type = new CodeDescriptionPair6Char { Code = Common.EU.CusSupportingInfoTypeList.Codes.Certificate }, Category = new CodeDescriptionPair { Code = Common.EU.CusSupportingInfoTypeList.Codes.Certificate }, Description = "cert1 desc", ReferenceNumber = "cert1 ref" },
				new CustomsSupportingInformation { Type = new CodeDescriptionPair6Char { Code = Common.EU.CusSupportingInfoTypeList.Codes.Certificate }, Category = new CodeDescriptionPair { Code = Common.EU.CusSupportingInfoTypeList.Codes.Certificate }, Description = "cert2 desc", ReferenceNumber = "cert2 ref" }
			}));
			jobData.SetNoteCollection(() => new DataObjectList<Note> { new Note { Description = "Special Instructions", NoteText = "SpecialInst" } });
			jobData.SetCustomsReferenceCollection(() => new List<CustomsReference>(new[]
			{
				new CustomsReference { Type = new CodeDescriptionPair { Code = EU.Business.CusCodeDataTypeList.Codes.OfficeCode }, SubType = new CodeDescriptionPair35Char { Code = "CAD" }, Reference = "office1" },
				new CustomsReference { Type = new CodeDescriptionPair { Code = EU.Business.CusCodeDataTypeList.Codes.OfficeCode }, SubType = new CodeDescriptionPair35Char { Code = "DEL" }, Reference = "office2" },
				new CustomsReference { Type = new CodeDescriptionPair { Code = EU.Business.CusCodeDataTypeList.Codes.OfficeCode }, SubType = new CodeDescriptionPair35Char { Code = "DIS" }, Reference = "office3" }
			}));
			var container1 = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "transport1",
				Seal = "seal1"
			};
			container1.SetAddInfoCollection(() => new List<AddInfo> { AddInfo.New("UnitCode", EMCSTransportUnitCodeList.Codes.Container), AddInfo.New("SealDetails", "seal1 details"), AddInfo.New("Comment", "seal1 comment") });
			var container2 = new Container(DefaultDataObjectWriterStrategy.TestInstance)
			{
				ContainerNumber = "transport2",
				Seal = "seal2"
			};
			container2.SetAddInfoCollection(() => new List<AddInfo> { AddInfo.New("UnitCode", EMCSTransportUnitCodeList.Codes.Tractor), AddInfo.New("SealDetails", "seal2 details"), AddInfo.New("Comment", "seal2 comment") });
			jobData.SetContainerCollection(() => new DataObjectList<Container>
			{
				container1, container2
			});
			var orgINTHEMSYD = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			var orgWUFSHIJNB = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var orgCRAHOLSYD = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			jobData.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				CreateOrganizationAddress(orgINTHEMSYD, nameof(DocAddressType.SupplierDocumentaryAddress)),
				CreateOrganizationAddress(orgWUFSHIJNB, nameof(DocAddressType.ImporterDocumentaryAddress)),
				CreateOrganizationAddress(orgCRAHOLSYD, nameof(DocAddressType.GoodsOwner)),
				CreateOrganizationAddress(orgCRAHOLSYD, nameof(DocAddressType.CarrierAgent)),
				CreateOrganizationAddress(orgCRAHOLSYD, nameof(DocAddressType.Transporter)),
				CreateOrganizationAddress(orgCRAHOLSYD, nameof(DocAddressType.DispatchWarehouse)),
				CreateOrganizationAddress(orgCRAHOLSYD, nameof(DocAddressType.DestinationWarehouse))
			});

			var reader = new EMCSDeclarationDataObjectReader(jobData, Logger, Factory);
			var emcs = (EMCSJobDeclaration)reader.ReadIntoBusinessObject();

			CombineAssertions(() =>
			{
				AssertEquals("JE_DeclarantType", EMCSEntryTypeList.Codes.Consignee, emcs.JE_DeclarantType);
				AssertEquals("JE_MessageType", EMCSJobDeclaration.EMCSMessageTypeCode, emcs.JE_MessageType);
				AssertEquals("JE_MessageSubType", EMCSDestinationTypeList.Codes.DestinationDirectDelivery, emcs.JE_MessageSubType);
				AssertEquals("JE_TransportMode", Core.Constants.TransportModes.Air, emcs.JE_TransportMode);
				AssertEquals("JE_PaymentMethod", DefermentMethodList.Codes.ConsigneesAccountConsigneeCompletingTheDeclaration, emcs.JE_PaymentMethod);
				AssertEquals("EADNumber", ZString.Empty, emcs.EADNumber);
				AssertEquals("JE_EntryStatus", ZString.Empty, emcs.JE_EntryStatus);
				AssertEquals("JE_MessageStatus", ZString.Empty, emcs.JE_MessageStatus);
				AssertEquals("JE_EntryAuthorisationDate", ZDateTime.Empty, emcs.JE_EntryAuthorisationDate);
				AssertEquals("JE_EntrySubmittedDate", ZDateTime.Empty, emcs.JE_EntrySubmittedDate);

				AssertEquals("JourneyTimeNumericPart", 2, emcs.JourneyTimeNumericPart);
				AssertEquals("JourneyTimeFormatPart", JourneyTimeUnitList.Codes.Hours, emcs.JourneyTimeFormatPart);
				AssertEquals("ZG_GuarantorType", EMCSGuarantorTypeList.Codes.OwnerOfTheExciseProducts, emcs.ZG_GuarantorType);
				AssertEquals("ZG_TransportArrangement", EMCSTransportArrangementList.Codes.OwnerOfGoods, emcs.ZG_TransportArrangement);
				AssertEquals("ZG_OriginType", EMCSOriginTypeList.Codes.Import, emcs.ZG_OriginType);
				AssertEquals("ZG_IsDeferredSubmission", EMCSDeferredSubmissionList.Codes.Yes, emcs.ZG_DeferredSubmission);
				var invoice = emcs.Invoices[0];
				AssertEquals("InvoiceNumber", "INV1234", invoice.JZ_InvoiceNumber);
				AssertEquals("InvoiceDate", new ZDateTime(2018, 09, 09, 09, 09, 09), invoice.JZ_InvoiceDate);
				AssertEquals("JE_DateAtOrigin", new ZDateTime(2018, 09, 09, 12, 12, 12), emcs.JE_DateAtOrigin);

				AssertEquals("SadCodes.Count", 2, emcs.ImportSADNumbers.Count);
				AssertEquals("SadCodes[0].CSI_Description", "SAD123", emcs.ImportSADNumbers[0].CSI_Description);
				AssertEquals("SadCodes[1].CSI_Description", "SAD456", emcs.ImportSADNumbers[1].CSI_Description);
				AssertEquals("SpecialInstructions", "SpecialInst", emcs.SpecialInstructions);

				emcs.CustomsOffices.Load();
				AssertEquals("CustomsOffices.Count", 3, emcs.CustomsOffices.Count);
				AssertEquals("CustomsOffices[0].CY_Code", "CAD", emcs.CustomsOffices[0].CY_Code);
				AssertEquals("CustomsOffices[0].CY_Data", "office1", emcs.CustomsOffices[0].CY_Data);
				AssertEquals("CustomsOffices[1].CY_Code", "DEL", emcs.CustomsOffices[1].CY_Code);
				AssertEquals("CustomsOffices[1].CY_Data", "office2", emcs.CustomsOffices[1].CY_Data);
				AssertEquals("CustomsOffices[2].CY_Code", "DIS", emcs.CustomsOffices[2].CY_Code);
				AssertEquals("CustomsOffices[2].CY_Data", "office3", emcs.CustomsOffices[2].CY_Data);

				AssertEquals("Documents.Count", 2, emcs.Documents.Count);
				AssertEquals("Documents[0].CSI_Description", "cert1 desc", emcs.Documents[0].CSI_Description);
				AssertEquals("Documents[0].CSI_ReferenceNumber", "cert1 ref", emcs.Documents[0].CSI_ReferenceNumber);
				AssertEquals("Documents[1].CSI_Description", "cert2 desc", emcs.Documents[1].CSI_Description);
				AssertEquals("Documents[1].CSI_ReferenceNumber", "cert2 ref", emcs.Documents[1].CSI_ReferenceNumber);

				AssertEquals("CusContainers.Count", 2, emcs.CusContainers.Count);
				AssertEquals("CusContainers[0].ZG_UnitCode", EMCSTransportUnitCodeList.Codes.Container, emcs.CusContainers[0].ZG_UnitCode);
				AssertEquals("CusContainers[0].CO_ContainerNumber", "transport1", emcs.CusContainers[0].CO_ContainerNumber);
				AssertEquals("CusContainers[0].CO_Seal", "seal1", emcs.CusContainers[0].CO_Seal);
				AssertEquals("CusContainers[0].SealDetails", "seal1 details", emcs.CusContainers[0].SealDetails);
				AssertEquals("CusContainers[0].Comment", "seal1 comment", emcs.CusContainers[0].Comment);
				AssertEquals("CusContainers[1].ZG_UnitCode", EMCSTransportUnitCodeList.Codes.Tractor, emcs.CusContainers[1].ZG_UnitCode);
				AssertEquals("CusContainers[1].CO_ContainerNumber", "transport2", emcs.CusContainers[1].CO_ContainerNumber);
				AssertEquals("CusContainers[1].CO_Seal", "seal2", emcs.CusContainers[1].CO_Seal);
				AssertEquals("CusContainers[1].SealDetails", "seal2 details", emcs.CusContainers[1].SealDetails);
				AssertEquals("CusContainers[1].Comment", "seal2 comment", emcs.CusContainers[1].Comment);

				AssertEquals("Invoices.Count", 1, emcs.Invoices.Count);
				AssertEquals("Invoices[0].JobComInvoiceLines.Count", 1, emcs.Invoices[0].JobComInvoiceLines.Count);

				AssertOrganizationAddress("Supplier", orgINTHEMSYD, emcs.Supplier);
				AssertOrganizationAddress("Importer", orgWUFSHIJNB, emcs.Importer);
				AssertOrganizationAddress("OwnerDocumentaryAddress", orgCRAHOLSYD, emcs.OwnerDocumentaryAddress.Organisation);
				AssertOrganizationAddress("DispatchWarehouseDocumentaryAddress", orgCRAHOLSYD, emcs.DispatchWarehouseDocumentaryAddress.Organisation);
				AssertOrganizationAddress("DestinationWarehouseDocumentaryAddress", orgCRAHOLSYD, emcs.DestinationWarehouseDocumentaryAddress.Organisation);
				AssertOrganizationAddress("CarrierAgentDocumentaryAddress", orgCRAHOLSYD, emcs.CarrierAgentDocumentaryAddress.Organisation);
				AssertOrganizationAddress("TransporterDocumentaryAddress", orgCRAHOLSYD, emcs.TransporterDocumentaryAddress.Organisation);

				var invoiceLine = emcs.Invoices[0].JobComInvoiceLines[0];
				AssertEquals("JI_LineNo", (ZShort)1, invoiceLine.JI_LineNo);
				AssertEquals("JI_BrandName", "brand", invoiceLine.JI_BrandName);
				AssertEquals("ZG_ExciseProductCode", EMCSJobComInvoiceLine.ExciseProductCode_W200, invoiceLine.ZG_ExciseProductCode);
				AssertEquals("JI_CountryOfOrigin", Core.Constants.CountryCodes.Australia, invoiceLine.JI_CountryOfOrigin);
				AssertEquals("JI_CustomsQuantity", 100m, invoiceLine.JI_CustomsQuantity);
				AssertEquals("JI_CustomsUnitQty", EMCSCustomsQuantityTypeList.Codes.FifteenLitre, invoiceLine.JI_CustomsUnitQty);
				AssertEquals("JI_NDescription", "line desc", invoiceLine.JI_NDescription);
				AssertEquals("JI_InvoiceQuantity", 111m, invoiceLine.JI_InvoiceQuantity);
				AssertEquals("JI_InvoiceUQ", "VQ", invoiceLine.JI_InvoiceUQ);
				AssertEquals("JI_NetWeight", 121m, invoiceLine.JI_NetWeight);
				AssertEquals("JI_NetWeightUQ", "KG", invoiceLine.JI_NetWeightUQ);
				AssertEquals("JI_PartNo", "partno", invoiceLine.JI_PartNo);
				AssertEquals("JI_Weight", 122m, invoiceLine.JI_Weight);
				AssertEquals("JI_WeightUQ", "KG", invoiceLine.JI_WeightUQ);
				AssertEquals("JI_CC", classificationJonno.PK, invoiceLine.JI_CC);
				AssertEquals("ZG_FiscalMark", "mark", invoiceLine.ZG_FiscalMark);
				AssertEquals("ZG_FiscalMarkUsed", ZBool.True, invoiceLine.ZG_FiscalMarkUsed);
				AssertEquals("ZG_AlcoholicStrength", 1m, invoiceLine.ZG_AlcoholicStrength);
				AssertEquals("ZG_DegreePlato", 2m, invoiceLine.ZG_DegreePlato);
				AssertEquals("ZG_SizeOfProducer", 3m, invoiceLine.ZG_SizeOfProducer);
				AssertEquals("ZG_Density", 4m, invoiceLine.ZG_Density);
				AssertEquals("ZG_WineCategory", EMCSWineCategoryList.Codes.ImportedWine, invoiceLine.ZG_WineCategory);
				AssertEquals("ZG_GrowingZone", EMCSGrowingZoneList.Codes.A, invoiceLine.ZG_GrowingZone);
				AssertEquals("ZG_WineCountryOrigin", Core.Constants.CountryCodes.Canada, invoiceLine.ZG_WineCountryOrigin);
				AssertEquals("JI_WineDetailsComments", "Wine Details Comments", invoiceLine.JI_WineDetailsComments);

				invoiceLine.OperationCodeDataCollection.Reload(true);
				AssertEquals("OperationCodeDataCollection.Count", 2, invoiceLine.OperationCodeDataCollection.Count);
				AssertEquals("OperationCodeDataCollection[0].CY_Code", "123", invoiceLine.OperationCodeDataCollection[0].CY_Code);
				AssertEquals("OperationCodeDataCollection[1].CY_Code", "456", invoiceLine.OperationCodeDataCollection[1].CY_Code);
			});
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}

		OrganizationAddress CreateOrganizationAddress(OrgHeader org, ZString addressType)
		{
			var orgAddress = org.MainAddress;
			var orgContact = org.Contacts[0];
			return new OrganizationAddress
			{
				AddressType = addressType,
				OrganizationCode = org.OH_Code,
				CompanyName = org.OH_FullName,
				AddressOverride = false,
				Address1 = orgAddress.OA_Address1,
				Address2 = orgAddress.OA_Address2,
				City = orgAddress.OA_City,
				State = orgAddress.OA_State,
				Postcode = orgAddress.OA_PostCode,
				Country = new Country(),
				Contact = orgContact.OC_ContactName,
				Email = orgContact.OC_Email,
				Fax = orgContact.OC_Fax,
				Mobile = orgContact.OC_Mobile,
				Phone = orgContact.OC_Phone
			};
		}

		void AssertOrganizationAddress(string message, OrgHeader expected, OrgHeader actual)
		{
			AssertEquals($"{message}.OH_Code", expected.OH_Code, actual.OH_Code);
			AssertEquals($"{message}.OH_FullName", expected.OH_FullName, actual.OH_FullName);
			var expectedAddress = expected.MainAddress;
			var actualAddress = actual.MainAddress;
			AssertEquals($"{message}.OA_Address1", expectedAddress.OA_Address1, actualAddress.OA_Address1);
			AssertEquals($"{message}.OA_Address2", expectedAddress.OA_Address2, actualAddress.OA_Address2);
			AssertEquals($"{message}.OA_City", expectedAddress.OA_City, actualAddress.OA_City);
			AssertEquals($"{message}.OA_State", expectedAddress.OA_State, actualAddress.OA_State);
			AssertEquals($"{message}.OA_PostCode", expectedAddress.OA_PostCode, actualAddress.OA_PostCode);
			AssertEquals($"{message}.OA_State", expectedAddress.OA_State, actualAddress.OA_State);
			var expectedContact = expected.Contacts[0];
			var actualContact = actual.Contacts[0];
			AssertEquals($"{message}.OC_ContactName", expectedContact.OC_ContactName, actualContact.OC_ContactName);
			AssertEquals($"{message}.OC_Email", expectedContact.OC_Email, actualContact.OC_Email);
			AssertEquals($"{message}.OC_Fax", expectedContact.OC_Fax, actualContact.OC_Fax);
			AssertEquals($"{message}.OC_Mobile", expectedContact.OC_Mobile, actualContact.OC_Mobile);
			AssertEquals($"{message}.OC_Phone", expectedContact.OC_Phone, actualContact.OC_Phone);
		}
	}
}
