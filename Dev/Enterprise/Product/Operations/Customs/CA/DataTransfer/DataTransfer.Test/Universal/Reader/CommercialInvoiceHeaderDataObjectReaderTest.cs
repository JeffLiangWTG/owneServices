using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.Common.CA;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using AddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using JobMessageTypeList = Enterprise.Customs.CA.Business.JobMessageTypeList;

namespace Enterprise.Customs.CA.DataTransfer.Universal.Testing
{
	partial class DeclarationDataObjectReaderTest
	{
		public void TestDocAddressesReloadFromLocalCache()
		{
			var org = new UnmatchedOrganisation(Factory.BOFactory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);
			var unmatchedOrg = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);

			var consigneeOrg = Factory.New<OrgHeader>();
			consigneeOrg.OH_Code = "JOEYYIN";
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var groupHeader = declaration.AllGroupHeaders.First() as JobComInvoiceGroupHeader;
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceNumber = "JOEYYIN";
			invoice.JZ_JZ_GroupInvoiceFK = groupHeader.PK;
			invoice.JZ_OH_Consignee = consigneeOrg.PK;

			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);
			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceHeaderDataObject.InvoiceNumber = "JOEYYIN";

			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration));
			invoiceHeaderDataObject.Supplier = invoiceHeaderDataObject.AddOrgAddress(writingManager, consigneeOrg, AddressTypes.Supplier);
			invoiceHeaderDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.FinalConsigneeAddress)
			});

			declarationDataObject.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() {
					invoiceHeaderDataObject.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									HarmonisedCode = "00000000"
								}
							}))) }
			};

			Factory.SaveForTesting();
			AssertEquals(consigneeOrg.PK, invoice.FinalConsigneeAddress.OrganisationPK);

			var helper = new UniversalCustomsDataObjectProvider().GetNewUniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Canada);
			var reader = new CommercialInvoiceHeaderDataObjectReader(invoiceHeaderDataObject, Logger, helper, groupHeader);
			_ = reader.ReadIntoBusinessObject(true);
			Factory.SaveForTesting();
			AssertEquals(unmatchedOrg.PK, invoice.FinalConsigneeAddress.OrganisationPK);
		}

		public void TestAddInfoColumnsToDBColumnsMapping()
		{
			var consignee = CreateOrganisation("CONSIGNEE", "ABC#@1");
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

			var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				OwnerRef = "OW234#$#",
				MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
				CommercialInfo = new CommercialInfo()
				{
					Name = "GROUP",
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									AddInfoCollection = new List<AddInfo>()
									{
										new AddInfo() { Key = "Qty2", Value = "100.15" },
										new AddInfo() { Key = "Qty2UM", Value = "KG" },
										new AddInfo() { Key = "Qty3", Value = "251.65" },
										new AddInfo() { Key = "Qty3UM", Value = "NO" }
									},
									HarmonisedCode = "00000000"
								}
							})))
					})
				}
			};

			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declaration = reader.ReadIntoBusinessObject();
			AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
			var invoiceLine = declaration.InvoiceLines[0];

			AssertEquals(100.15000m, invoiceLine.JI_CustomsSecondQuantity);
			AssertEquals("KG", invoiceLine.JI_CustomsSecondUnitQty);
			AssertEquals(251.65000m, invoiceLine.JI_CustomsThirdQuantity);
			AssertEquals("NO", invoiceLine.JI_CustomsThirdUnitQty);
		}

		public void TestFillOrganizations()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var consigneeOrg = Factory.New<OrgHeader>();
			consigneeOrg.OH_Code = "INCCONSIGNEE";

			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);
			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);

			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration));
			invoiceHeaderDataObject.Supplier = invoiceHeaderDataObject.AddOrgAddress(writingManager, consigneeOrg, AddressTypes.Supplier);
			invoiceHeaderDataObject.OrganizationAddressCollection = null;

			declarationDataObject.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() { invoiceHeaderDataObject }
			};

			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				var newDeclarationBO = new BusinessObjectFactory().Load<JobDeclaration>(declarationBO.PK);
				AssertEquals(1, newDeclarationBO.Invoices.Count);

				var invoiceBO = newDeclarationBO.Invoices.OfType<JobComInvoiceHeader>().First();
				AssertEquals(1, invoiceBO.DocAddresses.Count);
				AssertEquals(consigneeOrg.OH_Code, invoiceBO.Supplier.OH_Code);
			});
		}

		public void TestFillOrganizations_FinalConsigneeAddressUNMATCHED()
		{
			var org = new UnmatchedOrganisation(Factory.BOFactory);
			org.IsEnabled = true;
			OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, org);
			var unmatchedOrg = Factory.Load<OrgHeader>(OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.Value.Organisation);

			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var consigneeOrg = Factory.New<OrgHeader>();
			consigneeOrg.OH_Code = "CONSIGNEE";

			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);
			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);

			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration));
			invoiceHeaderDataObject.Supplier = invoiceHeaderDataObject.AddOrgAddress(writingManager, consigneeOrg, AddressTypes.Supplier);
			invoiceHeaderDataObject.OrganizationAddressCollection.Add(new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddressType = nameof(DocAddressType.FinalConsigneeAddress),
				CompanyName = "TEST1",
				City = "TEST2",
				Postcode = "TEST3",
				Country = new Country() { Code = "CA" }
			});

			declarationDataObject.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() { invoiceHeaderDataObject }
			};

			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				var newDeclarationBO = new BusinessObjectFactory().Load<JobDeclaration>(declarationBO.PK);
				AssertEquals(1, newDeclarationBO.Invoices.Count);

				var invoiceBO = newDeclarationBO.Invoices.OfType<JobComInvoiceHeader>().First();
				AssertEquals(2, invoiceBO.DocAddresses.Count);
				AssertEquals(consigneeOrg.OH_Code, invoiceBO.Supplier.OH_Code);
				AssertEquals("UNMATCHED", invoiceBO.FinalConsigneeAddress.Organisation.OH_Code);
				AssertEquals(unmatchedOrg.Addresses.FirstOrDefault().PK, invoiceBO.FinalConsigneeAddress.E2_OA_Address);
			});
		}

		public void TestImportInvoiceWhenAddInfoValueIsEmpty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var consigneeOrg = Factory.New<OrgHeader>();
			consigneeOrg.OH_Code = "INCCONSIGNEE";
			invoice.JZ_OH_Consignee = consigneeOrg.PK;

			var exporterOrg = Factory.New<OrgHeader>();
			exporterOrg.OH_Code = "INCEXPORTER";
			invoice.ExporterDocumentaryAddress.OrganisationPK = exporterOrg.PK;

			var shipperOrg = Factory.New<OrgHeader>();
			shipperOrg.OH_Code = "INCSHIPPER";
			invoice.SupplierPickupDeliveryAddress.OrganisationPK = shipperOrg.PK;

			var manufacturerOrg = Factory.New<OrgHeader>();
			manufacturerOrg.OH_Code = "INCMANF";
			var manufacturerAddress = manufacturerOrg.Addresses.AddNew();
			manufacturerAddress.Address1 = "MANUFACRER ADDRESS";
			invoice.JZ_OA_ManufacturerAddress = manufacturerAddress.PK;

			var buyerOrg = Factory.New<OrgHeader>();
			buyerOrg.OH_Code = "INBUYER";
			invoice.JZ_OH_Buyer = buyerOrg.PK;

			var originatorOrg = Factory.New<OrgHeader>();
			originatorOrg.OH_Code = "INCORIGIN";
			originatorOrg.MainAddress.OA_Address1 = "ORIGINATOR ADDRESS";
			invoice.CommercialInvoiceOriginator.E2_OA_Address = originatorOrg.MainAddress.PK;

			invoice.SupplierDocumentaryAddress.E2_AddressOverride = true;
			invoice.SupplierDocumentaryAddress.E2_CompanyName = "VENDOR";
			invoice.SupplierDocumentaryAddress.E2_Address1 = "VENDOR ADDRESS";

			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);
			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				NetWeight = 2,
				NetWeightUQ = new UnitOfWeight() { Code = "KG" },
				ValuationDateOverride = new ZDateTime(2022, 02, 16),
				AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo() { Key = "CountryOfOrigin", Value = "US" },
					new AddInfo() { Key = "ProvinceOfOrigin", Value = "CA" }
				}
			};

			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration));
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.Consignee, Constants.AddressType.Consignee);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.ExporterDocumentaryAddress);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.SupplierPickupDeliveryAddress.Organisation, Constants.AddressType.Shipper);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.ManufacturerAddress, Constants.AddressType.Manufacturer);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.SupplierPickupDeliveryAddress);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.SupplierDocumentaryAddress);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.FinalConsigneeAddress);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.CommercialInvoiceOriginator);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.BuyerDocumentaryAddress);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.ExporterDocumentaryAddress);

			invoiceHeaderDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
			{
				new CommercialInvoiceLine()
				{
					CountryOfOrigin = new Country() { Code = "US" },
					StateOfOrigin = new State() { Code = "TX" },
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo() { Key = "99TariffCode", Value = "9912" },
						new AddInfo() { Key = "ValueForDutyCode", Value = "13" },
						new AddInfo() { Key = "AuthorityNumber", Value = "123432314333" },
						new AddInfo() { Key = "TRSNumber", Value = "321543321321" },
						new AddInfo() { Key = "CompliantCompletion", Value = "Y" },
						new AddInfo() { Key = "CompliantImportDate", Value = "Y" },
						new AddInfo() { Key = "ProvinceOfOrigin", Value = "MN" }
					}
				}
			});

			invoiceHeaderDataObject.CustomsReferenceCollection = new List<CustomsReference>()
			{
				new CustomsReference() { Type = new CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.CCN, Description = CusCodeDataTypeList.Descriptions.CCN }, Reference = "HHH111" },
				new CustomsReference() { Type = new CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.CCN, Description = CusCodeDataTypeList.Descriptions.CCN }, Reference = "HHH222" }
			};

			declarationDataObject.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() { invoiceHeaderDataObject }
			};

			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				var newDeclarationBO = new BusinessObjectFactory().Load<JobDeclaration>(declarationBO.PK);
				AssertEquals(1, newDeclarationBO.Invoices.Count);

				var invoiceBO = newDeclarationBO.Invoices.OfType<JobComInvoiceHeader>().First();
				AssertEquals("Net Weight", 2m, invoiceBO.JZ_NetWeight);
				AssertEquals("Net Weight UQ", "KG", invoiceBO.JZ_NetWeightUQ);
				AssertEquals("Date of Direct Shipment", "16-Feb-22 00:00", invoiceBO.JZ_ValuationDateOverride.ToLongTimeString());
				AssertEquals("Country/Region Of Origin", "US", invoiceBO.JZ_RN_NKDefaultOrigin);

				AssertEquals("2 CargoControlNumbers", 2, invoiceBO.CargoControlNumbersList.Count);
				Assert("CCN 1", invoiceBO.CargoControlNumbersList.Any(x => x.J2_ReferenceNumber == "HHH111"));
				Assert("CCN 2", invoiceBO.CargoControlNumbersList.Any(x => x.J2_ReferenceNumber == "HHH222"));

				AssertEquals("INCCONSIGNEE", invoiceBO.Consignee.OH_Code);
				AssertEquals("INBUYER", invoiceBO.BuyerDocumentaryAddress.Organisation.OH_Code);
				AssertEquals("INCEXPORTER", invoiceBO.ExporterDocumentaryAddress.Organisation.OH_Code);
				AssertEquals("MANUFACRER ADDRESS", invoiceBO.ManufacturerAddress.Address1);
				AssertEquals("INCORIGIN", invoiceBO.CommercialInvoiceOriginator.Address.Header.OH_Code);
				AssertEquals("INCSHIPPER", invoiceBO.SupplierPickupDeliveryAddress.Organisation.OH_Code);
				AssertEquals("INCCONSIGNEE", invoiceBO.FinalConsigneeAddress.Organisation.OH_Code);
				AssertEquals(true, invoiceBO.SupplierDocumentaryAddress.E2_AddressOverride);
				AssertEquals("VENDOR ADDRESS", invoiceBO.SupplierDocumentaryAddress.Address1);

				AssertEquals(1, invoiceBO.InvoiceLines.Count);
				var invoiceLineBO = invoiceBO.InvoiceLines.OfType<JobComInvoiceLine>().First();
				AssertEquals("Tariff Code", "9912", invoiceLineBO.CA_99TariffCode);
				AssertEquals("Value for Duty Code", "13", invoiceLineBO.CA_ValueForDutyCode);
				AssertEquals("Special Auth/Pmt", "123432314333", invoiceLineBO.CA_AuthorityNumber);
				AssertEquals("TRS Number", "321543321321", invoiceLineBO.CA_TRSNumber);
				AssertEquals("Goods Origin", "US", invoiceLineBO.JI_CountryOfOrigin);
				AssertEquals("State", "TX", invoiceLineBO.JI_StateOrRegionOfOrigin);
				Assert("Compliant Completion", invoiceLineBO.CA_CompliantCompletion);
				Assert("Import Date Compliant", invoiceLineBO.CA_CompliantImportDate);
			});
		}

		public void TestImportInvoice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			var consigneeOrg = Factory.New<OrgHeader>();
			consigneeOrg.OH_Code = "INCCONSIGNEE";
			invoice.JZ_OH_Consignee = consigneeOrg.PK;

			var exporterOrg = Factory.New<OrgHeader>();
			exporterOrg.OH_Code = "INCEXPORTER";
			invoice.ExporterDocumentaryAddress.OrganisationPK = exporterOrg.PK;

			var shipperOrg = Factory.New<OrgHeader>();
			shipperOrg.OH_Code = "INCSHIPPER";
			invoice.SupplierPickupDeliveryAddress.OrganisationPK = shipperOrg.PK;

			var manufacturerOrg = Factory.New<OrgHeader>();
			manufacturerOrg.OH_Code = "INCMANF";
			var manufacturerAddress = manufacturerOrg.Addresses.AddNew();
			manufacturerAddress.Address1 = "MANUFACRER ADDRESS";
			invoice.JZ_OA_ManufacturerAddress = manufacturerAddress.PK;

			var buyerOrg = Factory.New<OrgHeader>();
			buyerOrg.OH_Code = "INBUYER";
			invoice.JZ_OH_Buyer = buyerOrg.PK;

			var originatorOrg = Factory.New<OrgHeader>();
			originatorOrg.OH_Code = "INCORIGIN";
			originatorOrg.MainAddress.OA_Address1 = "ORIGINATOR ADDRESS";
			invoice.CommercialInvoiceOriginator.E2_OA_Address = originatorOrg.MainAddress.PK;

			invoice.SupplierDocumentaryAddress.E2_AddressOverride = true;
			invoice.SupplierDocumentaryAddress.E2_CompanyName = "VENDOR";
			invoice.SupplierDocumentaryAddress.E2_Address1 = "VENDOR ADDRESS";

			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);
			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo() { Key = "NetWeight", Value = "24.900" },
					new AddInfo() { Key = "NetWeightUQ", Value = "KT" },
					new AddInfo() { Key = "CountryOfOrigin", Value = "US" },
					new AddInfo() { Key = "ProvinceOfOrigin", Value = "CA" }
				}
			};

			var writingManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration));
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.Consignee, Constants.AddressType.Consignee);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.ExporterDocumentaryAddress);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.SupplierPickupDeliveryAddress.Organisation, Constants.AddressType.Shipper);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.ManufacturerAddress, Constants.AddressType.Manufacturer);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.SupplierPickupDeliveryAddress);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.SupplierDocumentaryAddress);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.FinalConsigneeAddress);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.CommercialInvoiceOriginator);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.BuyerDocumentaryAddress);
			invoiceHeaderDataObject.AddOrgAddress(writingManager, invoice.ExporterDocumentaryAddress);

			invoiceHeaderDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
			{
				new CommercialInvoiceLine()
				{
					CountryOfOrigin = new Country() { Code = "US" },
					StateOfOrigin = new State() { Code = "TX" },
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo() { Key = "99TariffCode", Value = "9912" },
						new AddInfo() { Key = "ValueForDutyCode", Value = "13" },
						new AddInfo() { Key = "AuthorityNumber", Value = "123432314333" },
						new AddInfo() { Key = "TRSNumber", Value = "321543321321" },
						new AddInfo() { Key = "CompliantCompletion", Value = "Y" },
						new AddInfo() { Key = "CompliantImportDate", Value = "Y" },
						new AddInfo() { Key = "ProvinceOfOrigin", Value = "MN" }
					}
				}
			});

			invoiceHeaderDataObject.CustomsReferenceCollection = new List<CustomsReference>()
			{
				new CustomsReference() { Type = new CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.CCN, Description = CusCodeDataTypeList.Descriptions.CCN }, Reference = "HHH111" },
				new CustomsReference() { Type = new CodeDescriptionPair() { Code = CusCodeDataTypeList.Codes.CCN, Description = CusCodeDataTypeList.Descriptions.CCN }, Reference = "HHH222" }
			};

			declarationDataObject.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() { invoiceHeaderDataObject }
			};

			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			Factory.SaveForTesting();

			CombineAssertions(delegate
			{
				var newDeclarationBO = new BusinessObjectFactory().Load<JobDeclaration>(declarationBO.PK);
				AssertEquals(1, newDeclarationBO.Invoices.Count);

				var invoiceBO = newDeclarationBO.Invoices.OfType<JobComInvoiceHeader>().First();
				AssertEquals("Net Weight", 24.9m, invoiceBO.JZ_NetWeight);
				AssertEquals("Net Weight UQ", "KT", invoiceBO.JZ_NetWeightUQ);
				AssertEquals("Country/Region Of Origin", "US", invoiceBO.JZ_RN_NKDefaultOrigin);

				AssertEquals("2 CargoControlNumbers", 2, invoiceBO.CargoControlNumbersList.Count);
				Assert("CCN 1", invoiceBO.CargoControlNumbersList.Any(x => x.J2_ReferenceNumber == "HHH111"));
				Assert("CCN 2", invoiceBO.CargoControlNumbersList.Any(x => x.J2_ReferenceNumber == "HHH222"));

				AssertEquals("INCCONSIGNEE", invoiceBO.Consignee.OH_Code);
				AssertEquals("INBUYER", invoiceBO.BuyerDocumentaryAddress.Organisation.OH_Code);
				AssertEquals("INCEXPORTER", invoiceBO.ExporterDocumentaryAddress.Organisation.OH_Code);
				AssertEquals("MANUFACRER ADDRESS", invoiceBO.ManufacturerAddress.Address1);
				AssertEquals("INCORIGIN", invoiceBO.CommercialInvoiceOriginator.Address.Header.OH_Code);
				AssertEquals("INCSHIPPER", invoiceBO.SupplierPickupDeliveryAddress.Organisation.OH_Code);
				AssertEquals("INCCONSIGNEE", invoiceBO.FinalConsigneeAddress.Organisation.OH_Code);
				AssertEquals(true, invoiceBO.SupplierDocumentaryAddress.E2_AddressOverride);
				AssertEquals("VENDOR ADDRESS", invoiceBO.SupplierDocumentaryAddress.Address1);

				AssertEquals(1, invoiceBO.InvoiceLines.Count);
				var invoiceLineBO = invoiceBO.InvoiceLines.OfType<JobComInvoiceLine>().First();
				AssertEquals("Tariff Code", "9912", invoiceLineBO.CA_99TariffCode);
				AssertEquals("Value for Duty Code", "13", invoiceLineBO.CA_ValueForDutyCode);
				AssertEquals("Special Auth/Pmt", "123432314333", invoiceLineBO.CA_AuthorityNumber);
				AssertEquals("TRS Number", "321543321321", invoiceLineBO.CA_TRSNumber);
				AssertEquals("Goods Origin", "US", invoiceLineBO.JI_CountryOfOrigin);
				AssertEquals("State", "TX", invoiceLineBO.JI_StateOrRegionOfOrigin);
				Assert("Compliant Completion", invoiceLineBO.CA_CompliantCompletion);
				Assert("Import Date Compliant", invoiceLineBO.CA_CompliantImportDate);
			});
		}

		public void TestImportCFIADataIncludeAIRSValidationNumbers()
		{
			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.Import, B3EntryTypeList.Codes.Confirming);
			declarationDataObject.SetDateCollection(() => new List<Date>()
			{
				new Date() { Type = DateType.WarehouseRelease, IsEstimate = false, Value = new ZDateTime(2015, 9, 28, 0, 34, 0) },
				new Date() { Type = DateType.EntryAuthorisation, IsEstimate = false, Value = new ZDateTime(2015, 10, 1) }
			});
			var cfiaIndAddInfo = new AddInfo();
			cfiaIndAddInfo.Key = "CFIAInd";
			cfiaIndAddInfo.Value = YesNoList.Codes.Yes;
			var cfiaDataObject = new AddInfoGroup();
			cfiaDataObject.Type = new CodeDescriptionPair() { Code = CusAddInfoTypeAttribute.Codes.CACFIAPGAHeader };
			cfiaDataObject.CustomsReferenceCollection = new List<CustomsReference>(new[] {
				new CustomsReference()
				{
					Type = new CodeDescriptionPair() { Code = "RNA", Description = "AIRS Number" },
					Reference = "AIRS Number 01",
					SubType = new CodeDescriptionPair35Char() { Code = "102" }
				},
				new CustomsReference()
				{
					Type = new CodeDescriptionPair() { Code = "RNA", Description = "AIRS Number" },
					Reference = "AIRS Number 02",
					SubType = new CodeDescriptionPair35Char() { Code = "114" }
				}
			});

			declarationDataObject.CommercialInfo = new CommercialInfo()
			{
				Name = "GROUP",
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
					{
						new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
						{
							InvoiceNumber = "INV23",
						}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
							new DataObjectList<CommercialInvoiceLine>(new[]
							{
								new CommercialInvoiceLine()
								{
									LineNo = 1,
									AddInfoCollection = new List<AddInfo>(new[]
									{
										cfiaIndAddInfo
									}),
									AddInfoGroupCollection = new List<AddInfoGroup>(new[]
									{
										cfiaDataObject
									})
								}
							})))
					})
			};

			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();
			AssertEquals("declaration.InvoiceLines.Count", 1, declarationBO.InvoiceLines.Count);
			var invoiceLine = declarationBO.InvoiceLines[0];

			var cfia = invoiceLine.CFIAPGAHeader;
			AssertNotNull("CFIAPGAHeader", cfia);
			AssertEquals(2, cfia.AIRSRegistrationNumbers.Count);
			var first = cfia.AIRSRegistrationNumbers[0];
			AssertEquals("102", first.CY_Code);
			AssertEquals("AIRS Number 01", first.CY_Data);
			var second = cfia.AIRSRegistrationNumbers[1];
			AssertEquals("114", second.CY_Code);
			AssertEquals("AIRS Number 02", second.CY_Data);
		}

		public void TestLVXInvoice()
		{
			var declarationDataObject = SetupDeclaration(JobMessageTypeList.Codes.LVSForConsolidation, B3EntryTypeList.Codes.NoB3);
			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				InvoiceDate = new ZDateTime(2015, 10, 5),
				AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo() { Key = "CountryOfOrigin", Value = "AU" }
				}
			};
			invoiceHeaderDataObject.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>()
			{
				new CommercialInvoiceLine()
				{
					AddInfoCollection = new List<AddInfo>()
					{
						new AddInfo() { Key = "99TariffCode", Value = "9901" },
						new AddInfo() { Key = "AuthorityNumber", Value = "SPECIAL AUTH" },
						new AddInfo() { Key = "TRSNumber", Value = "TRS NO" }
					}
				}
			});

			declarationDataObject.CommercialInfo = new CommercialInfo()
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>() { invoiceHeaderDataObject }
			};

			Factory.SaveForTesting();

			var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			CombineAssertions(delegate
			{
				AssertEquals(1, declarationBO.Invoices.Count);

				var invoiceBO = declarationBO.Invoices.OfType<JobComInvoiceHeader>().First();
				AssertEquals("Carrier Date", new ZDateTime(2015, 10, 5), invoiceBO.JZ_InvoiceDate);
				AssertEquals("Country/Region of Origin", "AU", invoiceBO.JZ_RN_NKDefaultOrigin);

				AssertEquals(1, invoiceBO.InvoiceLines.Count);
				var invoiceLineBO = invoiceBO.InvoiceLines.OfType<JobComInvoiceLine>().First();
				AssertEquals("Tariff Code", "9901", invoiceLineBO.CA_99TariffCode);
				AssertEquals("Special Auth/Pmt", "SPECIAL AUTH", invoiceLineBO.CA_AuthorityNumber);
				AssertEquals("TRS Number", "TRS NO", invoiceLineBO.CA_TRSNumber);
			});
		}

		public void TestImporterSetFromCommercialInvoiceLevelOrganizations()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Canada))
			{
				var importer = CreateOrganisation("PET", "Peter");
				importer.OH_RL_NKClosestPort = "CABLO";

				var writeManager = new DataWritingManager(new ActionInfo(null, Factory.New<DummyBusinessObject>()));

				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					MessageType = new CodeDescriptionPair()
					{
						Code = "LVX",
						Description = "LVX"
					},
					WayBillNumber = "OB323",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				};

				declarationDataObject.SetAddInfoCollection(() => new List<AddInfo>
					{
						new AddInfo { Key = CAAddInfoSchema.CA_ServiceOption.Name.Substring(3), Value = ACROSSServiceOptions.Codes.IID }
					});
				declarationDataObject.SetContainerCollection(() => new DataObjectList<Container>(new[]
				{
					new Container(DefaultDataObjectWriterStrategy.TestInstance) { ContainerNumber = "CONT1" }
				}));
				declarationDataObject.SetPackingLineCollection(() => new DataObjectList<PackingLine>()
					{
						new PackingLine(DefaultDataObjectWriterStrategy.TestInstance)
						{
							BillNumber = "OB323",
							ContainerNumber = "CONT1",
							BillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
							Link = 1,
							PackQty = 555
						}
					});

				var invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
				invoice.InvoiceNumber = "INVABC123";
				invoice.InvoiceAmount = 150m;
				invoice.Buyer = invoice.AddOrgAddress(writeManager, importer, AddressTypes.Importer);

				declarationDataObject.CommercialInfo = new CommercialInfo()
				{
					CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
						{
							invoice
						}
				};

				var reader = new JobDeclarationDataObjectReader(declarationDataObject, Logger, Factory);
				var declarationBO = reader.ReadIntoBusinessObject();

				AssertEquals("Importer", importer.PK, declarationBO.JE_OH_Importer);
			}
		}

		public void TestGetNewInvoiceLineAndLoadWithoutRebuildingSubCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			Factory.SaveForTesting();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingConsol, "C00029310");
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S900053111");
			shipment.DataContext = dataContext;
			shipment.SetSubShipmentCollection(() => new DataObjectList<Shipment>()
			{
				new (DefaultDataObjectWriterStrategy.TestInstance)
				{
					ShipmentType = new CodeDescriptionPair()
					{
						Code = Core.Constants.ShipmentTypes.HighVolumeLowValue
					}
				}
			});
			AssertEquals("Pre-condition", true, shipment.IsHVLV());

			var invoiceHeaderDataObject = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			var helper = new UniversalDataObjectReaderHelper(Factory, Core.Constants.CountryCodes.Canada);
			var reader = new CommercialInvoiceHeaderDataObjectReader(invoiceHeaderDataObject, Logger, helper, (JobComInvoiceGroupHeader)declaration.TopGroupInvoice, shipment);
			var readerType = reader.GetType();
			var getNewInvoiceLineMethod = readerType.GetMethod(GetNewInvoiceLineMethodName, BindingFlags.Instance | BindingFlags.NonPublic);
			AssertNotNull(getNewInvoiceLineMethod);

			CombineAssertions(delegate
			{
				AssertEquals("Initial collection should be empty.",0, invoice.InvoiceLines.Count);
				AssertEquals("Initial collection should be empty.", 0, declaration.InvoiceLines.Count);
				var invoiceLineObject = getNewInvoiceLineMethod?.Invoke(reader, new object[] { invoice });
				AssertNotNull(invoiceLineObject);
				AssertEquals("Without collection loading, it should still be empty for now.", 0, invoice.InvoiceLines.Count);
				AssertEquals("Without collection loading, it should still be empty for now.", 0, declaration.InvoiceLines.Count);

				declaration.InvoiceLines.LoadWithoutRebuildSubCollection();
				AssertEquals("After collection loaded, it show have one invoice line now.", 1, invoice.InvoiceLines.Count);
				AssertEquals("After collection loaded, it show have one invoice line now.", 1, declaration.InvoiceLines.Count);
			});
		}

		const string GetNewInvoiceLineMethodName = "GetNewInvoiceLine";
	}
}
