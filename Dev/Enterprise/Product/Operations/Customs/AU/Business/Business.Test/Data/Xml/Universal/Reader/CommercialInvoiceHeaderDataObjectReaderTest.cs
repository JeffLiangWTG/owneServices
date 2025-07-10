using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UniversalAddInfoGroup = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs.AddInfoGroup;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CommercialInvoiceHeaderDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestQuarantineExDocProcess()
		{
			var orgH = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var orgH2 = GetOrganizationBO_INTHEMSYD(Factory.BOFactory);

			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			shipment.DataContext = dataContext;
			shipment.CommercialInfo = new UniversalCustoms.CommercialInfo();
			shipment.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>();
			shipment.MessageType = new CodeDescriptionPair() { Code = Common.AU.AUJobMessageTypeList.Codes.Quarantine, Description = Common.AU.AUJobMessageTypeList.Descriptions.Quarantine };
			var invoice = new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoice.AddInfoGroupCollection = new List<UniversalAddInfoGroup>();

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var organisationAddress = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CompanyName = orgH.OH_FullName,
				AddressType = nameof(MasterFiles.Integration.DocAddressType.AQISProcessingEstablishment),
				Address1 = orgH.MainAddress.OA_Address1,
				AddressShortCode = orgH.MainAddress.OA_Address1,
				City = orgH.MainAddress.City,
				Email = "benny.banana@wufu.co.za",
				Postcode = "12345",
				Country = new Country { Code = Core.Constants.CountryCodes.Australia },
			};
			shipment.OrganizationAddressCollection.Add(organisationAddress);

			var organisationAddress2 = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CompanyName = orgH2.OH_FullName,
				AddressType = nameof(MasterFiles.Integration.DocAddressType.AQISProcessingEstablishment),
				Address1 = orgH2.MainAddress.OA_Address1,
				AddressShortCode = orgH2.MainAddress.OA_Address1,
				City = orgH2.MainAddress.City,
				Email = "org2@wufu.co.za",
				Postcode = "1502",
				Country = new Country { Code = Core.Constants.CountryCodes.Australia },
			};
			shipment.OrganizationAddressCollection.Add(organisationAddress2);

			var addInfoGroup1 = new UniversalAddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Constants.InvoiceHeader.Codes.QH, Description = Constants.InvoiceHeader.Descriptions.QH },
				AddInfoCollection = new List<UniversalAddInfo>()
				{
					new UniversalAddInfo() { Key = Constants.InvoiceHeader.Keys.ProduceType, Value = "DAI" },
					new UniversalAddInfo() { Key = Constants.InvoiceHeader.Keys.ImportedProductFlag, Value = "NO" },
					new UniversalAddInfo() { Key = Constants.InvoiceHeader.Keys.LastAmendDateTime, Value = "2019-08-29T09:23:00.001+10:00" }
				}
			};
			invoice.AddInfoGroupCollection.Add(addInfoGroup1);

			shipment.CommercialInfo.CommercialInvoiceCollection.Add(invoice);

			invoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<UniversalCustoms.CommercialInvoiceLine>());
			var invoiceLine = new UniversalCustoms.CommercialInvoiceLine();
			invoice.CommercialInvoiceLineCollection.Add(invoiceLine);
			invoiceLine.AddInfoCollection = new List<UniversalAddInfo>();
			invoiceLine.AddInfoGroupCollection = new List<UniversalAddInfoGroup>();

			var addInfoGroup2 = new UniversalAddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Constants.InvoiceLine.Codes.QL, Description = Constants.InvoiceLine.Descriptions.QL },
				AddInfoCollection = new List<UniversalAddInfo>()
				{
					new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.GrossMetricWeight, Value = "1" }
				},
				AddInfoGroupCollection = new List<UniversalAddInfoGroup>()
				{
					new UniversalAddInfoGroup()
					{
						Type = new CodeDescriptionPair() { Code = Constants.InvoiceLine.Codes.NPD, Description = Constants.InvoiceLine.Descriptions.NPD },
						AddInfoCollection = new List<UniversalAddInfo>()
						{
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.ProcessingType, Value = "CU" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.EstablishmentIndicator, Value = "PK" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.StartDate, Value = "2019-08-01 00:00:00.000" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.EndDate, Value = "2019-08-02 00:00:00.000" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.DepurationDate, Value = "2019-08-03 00:00:00.000" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.HarvestArea, Value = "HAREA" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.InspectionRequestedDate, Value = "2019-08-04 00:00:00.000" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.LeaseNumber, Value = "LEASENUM" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.TreatmentCode, Value = "TRC" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.TreatmentInformation, Value = "TRINFO" },
						},
						OrganizationAddressCollection = new List<OrganizationAddress>()
						{
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) {
														CompanyName = "WUFU SHIPPING LINE",
														AddressType = nameof(MasterFiles.Integration.DocAddressType.AQISProcessingEstablishment),
														Address1 = "Level 2, Building G",
														AddressShortCode = "Level 2, Building G",
														City = "Johannesburg",
														Email = "benny.banana@wufu.co.za",
														Postcode = "12345",
														Country = new Country { Code = Core.Constants.CountryCodes.Australia },
														}
						}
					}
				}
			};

			var groupNPD2 = new UniversalAddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Constants.InvoiceLine.Codes.NPD, Description = Constants.InvoiceLine.Descriptions.NPD },
				AddInfoCollection = new List<UniversalAddInfo>()
						{
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.ProcessingType, Value = "C2" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.EstablishmentIndicator, Value = "P2" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.StartDate, Value = "2019-08-02 00:00:00.000" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.EndDate, Value = "2019-08-03 00:00:00.000" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.DepurationDate, Value = "2019-08-04 00:00:00.000" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.HarvestArea, Value = "HAREA2" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.InspectionRequestedDate, Value = "2019-08-05 00:00:00.000" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.LeaseNumber, Value = "LEASENUM2" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.TreatmentCode, Value = "TR2" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.TreatmentInformation, Value = "TRINFO2" },
							new UniversalAddInfo() { Key = Constants.InvoiceLine.Keys.RemoveEntry, Value = "true" },
						},
				OrganizationAddressCollection = new List<OrganizationAddress>()
						{
							new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance) {
														CompanyName = orgH2.OH_FullName,
														AddressType = nameof(MasterFiles.Integration.DocAddressType.AQISProcessingEstablishment),
														Address1 = orgH2.MainAddress.OA_Address1,
														AddressShortCode = orgH2.MainAddress.OA_Address1,
														City = orgH2.MainAddress.City,
														Email = "org2@wufu.co.za",
														Postcode = "1502",
														Country = new Country { Code = Core.Constants.CountryCodes.Australia },
														}
						}
			};
			addInfoGroup2.AddInfoGroupCollection.Add(groupNPD2);

			invoiceLine.AddInfoGroupCollection.Add(addInfoGroup2);

			var reader = new JobDeclarationDataObjectReader(shipment, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			var invoiceBO = (JobComInvoiceHeader)declarationBO.Invoices[0];
			var quarantineHeader = invoiceBO.QuarantineExDocHeader;
			AssertEquals("QuarantineExDocHeaderSchema.QH_ProduceType", "DAI", quarantineHeader.QH_ProduceType);
			AssertEquals("QuarantineExDocHeaderSchema.QH_ImportedProductFlag", "NO", quarantineHeader.QH_ImportedProductFlag);
			AssertEquals("QuarantineExDocHeaderSchema.LastAmendDateTime", new ZDateTimeOffset(2019, 8, 29, 9, 23, 0, 1, TimeSpan.FromHours(10)), quarantineHeader.QH_LastAmendDateTime);

			var invoiceLineBO = (JobComInvoiceLine)declarationBO.Invoices[0].JobComInvoiceLines[0];
			var quarantineLine = invoiceLineBO.QuarantineExDocLine;
			AssertEquals("QuarantineExDocLineSchema.QL_GrossMetricWeight", (ZDecimal)1m, quarantineLine.QL_GrossMetricWeight);

			var qurantineExDocEstablishmentAndTime = invoiceLineBO.QuarantineExDocLine.Processes[0];
			AssertEquals("QuarantineExExtablishmentAndTime.EE_ProcessingType", "CU", qurantineExDocEstablishmentAndTime.EE_ProcessingType);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_EstablishmentIndicator", "PK", qurantineExDocEstablishmentAndTime.EE_EstablishmentIndicator);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_StartDate", new ZDateTime(2019, 08, 01), qurantineExDocEstablishmentAndTime.EE_StartDate);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_EndDate", new ZDateTime(2019, 08, 02), qurantineExDocEstablishmentAndTime.EE_EndDate);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_Depuration", new ZDateTime(2019, 08, 03), qurantineExDocEstablishmentAndTime.EE_Depuration);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_InspectionRequestedDate", new ZDateTime(2019, 08, 04), qurantineExDocEstablishmentAndTime.EE_InspectionRequestedDate);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_HarvestArea", "HAREA", qurantineExDocEstablishmentAndTime.EE_HarvestArea);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_LeaseNumber", "LEASENUM", qurantineExDocEstablishmentAndTime.EE_LeaseNumber);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_TreatmentCode", "TRC", qurantineExDocEstablishmentAndTime.EE_TreatmentCode);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_TreatmentInfo", "TRINFO", qurantineExDocEstablishmentAndTime.EE_TreatmentInfo);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_EstablishmentPostedStatus", "", qurantineExDocEstablishmentAndTime.EE_EstablishmentPostedStatus);
			AssertNotNull("QuarantineExExtablishmentAndTime.EE_E2_Address", qurantineExDocEstablishmentAndTime.EE_E2_Address);

			var qurantineExDocEstablishmentAndTime2 = invoiceLineBO.QuarantineExDocLine.Processes[1];
			AssertEquals("QuarantineExExtablishmentAndTime.EE_ProcessingType", "C2", qurantineExDocEstablishmentAndTime2.EE_ProcessingType);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_EstablishmentIndicator", "P2", qurantineExDocEstablishmentAndTime2.EE_EstablishmentIndicator);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_StartDate", new ZDateTime(2019, 08, 02), qurantineExDocEstablishmentAndTime2.EE_StartDate);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_EndDate", new ZDateTime(2019, 08, 03), qurantineExDocEstablishmentAndTime2.EE_EndDate);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_Depuration", new ZDateTime(2019, 08, 04), qurantineExDocEstablishmentAndTime2.EE_Depuration);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_InspectionRequestedDate", new ZDateTime(2019, 08, 05), qurantineExDocEstablishmentAndTime2.EE_InspectionRequestedDate);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_HarvestArea", "HAREA2", qurantineExDocEstablishmentAndTime2.EE_HarvestArea);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_LeaseNumber", "LEASENUM2", qurantineExDocEstablishmentAndTime2.EE_LeaseNumber);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_TreatmentCode", "TR2", qurantineExDocEstablishmentAndTime2.EE_TreatmentCode);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_TreatmentInfo", "TRINFO2", qurantineExDocEstablishmentAndTime2.EE_TreatmentInfo);
			AssertEquals("QuarantineExExtablishmentAndTime.EE_EstablishmentPostedStatus", "", qurantineExDocEstablishmentAndTime2.EE_EstablishmentPostedStatus);
			AssertNotNull("QuarantineExExtablishmentAndTime.EE_E2_Address", qurantineExDocEstablishmentAndTime2.EE_E2_Address);
		}

		public void TestAQISLoadingEstablishment()
		{
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				MessageType = new CodeDescriptionPair() { Code = Common.AU.AUJobMessageTypeList.Codes.Quarantine, Description = Common.AU.AUJobMessageTypeList.Descriptions.Quarantine },
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>()
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							OrganizationAddressCollection = new List<OrganizationAddress>()
							{
								new OrganizationAddress()
								{
									CompanyName = "My Company Pty Ltd",
									Address1 = "ADDRESS LINE 1",
									AddressType = "AQISLoadingEstablishment"
								}
							}
						}
					}
				}
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			var invoiceBO = (JobComInvoiceHeader)declarationBO.Invoices[0];
			var loadingEstablishment = invoiceBO.AQISLoadingEstablishmentLocation;
			AssertEquals("CompanyName", "My Company Pty Ltd", loadingEstablishment.CompanyName);
			AssertEquals("Address1", "ADDRESS LINE 1", loadingEstablishment.Address1);
		}

		public void TestBondedWhsQuantityDoesNotGetImported()
		{
			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				CommercialInfo = new UniversalCustoms.CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<UniversalCustoms.CommercialInvoiceHeader>()
					{
						new UniversalCustoms.CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<UniversalCustoms.CommercialInvoiceLine>()
							{
								new UniversalCustoms.CommercialInvoiceLine()
								{
									BondedWarehouseQuantity = 100m,
									BondedWarehouseQuantityUnit = new CodeDescriptionPair() { Code = "UNT" }
								}
							}))
					}
				}
			};

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			var invoiceBO = declarationBO.Invoices[0];
			var invoiceLineBO = invoiceBO.JobComInvoiceLines[0];
			AssertEquals("invoiceLineBO.JI_BondedWhsQuantity", ZDecimal.Zero, invoiceLineBO.JI_BondedWhsQuantity);
			AssertEquals("invoiceLineBO.JI_BondedWhsUnitQty", ZString.Empty, invoiceLineBO.JI_BondedWhsUnitQty);
		}
	}
}
