using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Bill = Enterprise.Customs.EU.Business.Declaration.Bill;
using CusEntryHeader = Enterprise.Customs.EU.Business.Declaration.CusEntryHeader;
using CusEntryHeaderCharges = Enterprise.Customs.EU.Business.Declaration.CusEntryHeaderCharges;
using CusEntryInstruction = Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction;
using JobComInvoiceLineTax = Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLineTax;
using Order = Enterprise.Freight.Forwarding.Orders.Business.Order;
using Shipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;
using UniversalCustoms = Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.EU.DataTransfer.Universal.Testing
{
	public sealed class JobDeclarationDataObjectReaderTest : JobDeclarationDataObjectReaderAbstractTest<JobDeclaration, JobDeclarationDataObjectReader>
	{
		public void TestUXMLImportSetJE_OH_SupplierAndJE_MessageTypeCorrectly()
		{
			var org1 = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			org1.OH_RL_NKClosestPort = "LVCES";
			var org2 = OrganizationAddressTestHelper.GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			org2.OH_RL_NKClosestPort = "LVCES";
			Factory.SaveForTesting();

			var shipment = CreateShipment();
			shipment.MessageType = new CodeDescriptionPair { Code = "ABC" };
			var writeManager = new DataWritingManager(new ActionInfo(null, org1));
			shipment.AddOrgAddress(writeManager, org1, AddressTypes.Supplier); // AddressTypeMatchHelper.GetSupplierAddressTypesInPreferredOrder will use this for JE_OH_Supplier
			shipment.AddOrgAddress(writeManager, org2, DocAddressType.SupplierDocumentaryAddress);
			var dec = new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("dec.JE_MessageType", "ABC", dec.JE_MessageType);
			AssertEquals("dec.JE_OH_Supplier", org2.PK, dec.JE_OH_Supplier);
			AssertEquals("dec.SupplierDocumentaryAddress.E2_OA_Address", org2.MainAddress.PK, dec.SupplierDocumentaryAddress.E2_OA_Address);
		}

		public void TestUXMLImportSetJE_OH_ImporterAndJE_MessageTypeCorrectly()
		{
			var org1 = OrganizationAddressTestHelper.GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			org1.OH_RL_NKClosestPort = "LVCES";
			var org2 = OrganizationAddressTestHelper.GetOrganizationBO_INTHEMSYD(Factory.BOFactory);
			org2.OH_RL_NKClosestPort = "LVCES";
			Factory.SaveForTesting();

			var shipment = CreateShipment();
			shipment.MessageType = new CodeDescriptionPair { Code = "ABC" };
			var writeManager = new DataWritingManager(new ActionInfo(null, org1));
			shipment.AddOrgAddress(writeManager, org1, AddressTypes.Importer); // AddressTypeMatchHelper.GetImporterAddressTypesInPreferredOrder will use this for JE_OH_Importer
			shipment.AddOrgAddress(writeManager, org2, DocAddressType.ImporterDocumentaryAddress);
			var dec = new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject();
			Factory.SaveForTesting();
			AssertEquals("dec.JE_MessageType", "ABC", dec.JE_MessageType);
			AssertEquals("dec.JE_OH_Importer", org2.PK, dec.JE_OH_Importer);
			AssertEquals("dec.ImporterDocumentaryAddress.E2_OA_Address", org2.MainAddress.PK, dec.ImporterDocumentaryAddress.E2_OA_Address);
		}

		public void TestImportAndExportOldSupportingInformationSchema()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			using (Registry.EUCustomsDataRegistry.Instance.ExportOldSuportingInfoSchemaInUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					WayBillNumber = "MB123",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					CommercialInfo = new CommercialInfo()
					{
						AddInfoGroupCollection = new List<AddInfoGroup>(new[]
						{
							CreateAdditionalInfo("DC1", "DEC ADD DESC 1", Core.Constants.CountryCodes.Afghanistan, YesNoList.Codes.Yes),
							CreatePreviousDocument("DC2", new ZDate(2017, 1, 1), "DEC MORE INFO 2", "DECP2", PreviousDocumentClassList.Codes.PreviousAdministrativeReferenceNCTS),
							CreateSupportingDocument("DC3", "DECS3", "DEC BECAUSE", 10.32m, "PARD1", "A", "U")
						}),
						CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
						{
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								AddInfoGroupCollection = new List<AddInfoGroup>(new []
								{
									CreateAdditionalInfo("IC1", "INVOICE ADD DESC 1", Core.Constants.CountryCodes.Bahamas, YesNoList.Codes.No),
									CreatePreviousDocument("IC2", new ZDate(2017, 2, 1), "INV MORE INFO 2", "INVP2", PreviousDocumentClassList.Codes.SummaryDeclaration),
									CreateSupportingDocument("IC3", "INVS3", "INV BECAUSE", 34.23m, "PARI1", "C", "H")
								}),
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
								new DataObjectList<CommercialInvoiceLine>(new []
								{
									new CommercialInvoiceLine()
									{
										AddInfoGroupCollection = new List<AddInfoGroup>(new []
										{
											CreateAdditionalInfo("LC1", "INVOICE LINE ADD DESC 1", Core.Constants.CountryCodes.Cambodia, YesNoList.Codes.Yes),
											CreatePreviousDocument("LC2", new ZDate(2017, 3, 1), "INVLINE MORE INFO 2", "INVLINE2", PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures),
											CreateSupportingDocument("LC3", "INVLINES3", "INVLINE BECAUSE", 333.34m, "PAR1", "G", "F")
										}),

										TaxOrFeeCollection = new List<TaxOrFee>()
										{
											CreateTaxOrFee(451, 452, 453, new CodeDescriptionPair4Char() { Code = "TC1" }, new CodeDescriptionPair() { Code = "TC2" }, new CodeDescriptionPair() { Code = "TC3" }, new CodeDescriptionPair6Char() { Code = "TC4" })
										}
									}
								})))
						})
					}
				};

				CombineAssertions(delegate
				{
					var message = GetQueuedUniversalShipmentMessage(shipment);
					var serviceTaskLog = new ServiceTaskLogForTesting();
					var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
					messageProcessingManager.Process(message);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);
				});

				var newFactory = new BusinessObjectFactory();
				var query = new ZQuery(JobDeclarationSchema.JE_MasterBill, "MB123");
				var declarations = newFactory.Load<JobDeclaration>(query);

				CombineAssertions(delegate
				{
					AssertEquals(1, declarations.Length);
					var declaration = declarations[0];
					AssertEquals("declaration.AdditionalInfos.Count", 1, declaration.AdditionalInfos.Count);
					AssertCusSupportingInfoContents(declaration.AdditionalInfos[0], "DC1", Core.Constants.CountryCodes.Afghanistan, ZDate.Empty, "DEC ADD DESC 1", ZDecimal.Zero, ZString.Empty, AdditionalInfoIssuerList.Codes.Customs, YesNoList.Codes.Yes);
					AssertEquals("declaration.PreviousDocuments.Count", 1, declaration.PreviousDocuments.Count);
					AssertCusSupportingInfoContents(declaration.PreviousDocuments[0], "DC2", ZString.Empty, new ZDate(2017, 1, 1), "DEC MORE INFO 2", ZDecimal.Zero, "DECP2", string.Empty, PreviousDocumentClassList.Codes.PreviousAdministrativeReferenceNCTS);
					AssertEquals("declaration.SupportingDocuments.Count", 1, declaration.SupportingDocuments.Count);
					AssertCusSupportingInfoContents(declaration.SupportingDocuments[0], "DC3", ZString.Empty, ZDate.Empty, "DEC BECAUSE", 10.32m, "DECS3", "UA", "PARD1");

					AssertEquals("declaration.Invoices.Count", 1, declaration.Invoices.Count);
					var invoice = declaration.Invoices[0];
					AssertEquals("invoice.AdditionalInfos.Count", 1, invoice.AdditionalInfos.Count);
					AssertCusSupportingInfoContents(invoice.AdditionalInfos[0], "IC1", Core.Constants.CountryCodes.Bahamas, ZDate.Empty, "INVOICE ADD DESC 1", ZDecimal.Zero, ZString.Empty, AdditionalInfoIssuerList.Codes.Customs, YesNoList.Codes.No);
					AssertEquals("invoice.PreviousDocuments.Count", 1, invoice.PreviousDocuments.Count);
					AssertCusSupportingInfoContents(invoice.PreviousDocuments[0], "IC2", ZString.Empty, new ZDate(2017, 2, 1), "INV MORE INFO 2", ZDecimal.Zero, "INVP2", string.Empty, PreviousDocumentClassList.Codes.SummaryDeclaration);
					AssertEquals("invoice.SupportingDocuments.Count", 1, invoice.SupportingDocuments.Count);
					AssertCusSupportingInfoContents(invoice.SupportingDocuments[0], "IC3", ZString.Empty, ZDate.Empty, "INV BECAUSE", 34.23m, "INVS3", "HC", "PARI1");

					AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
					var invoiceLine = declaration.InvoiceLines[0];
					AssertEquals("invoiceLine.AdditionalInfos.Count", 1, invoiceLine.AdditionalInfos.Count);
					AssertCusSupportingInfoContents(invoiceLine.AdditionalInfos[0], "LC1", Core.Constants.CountryCodes.Cambodia, ZDate.Empty, "INVOICE LINE ADD DESC 1", ZDecimal.Zero, ZString.Empty, AdditionalInfoIssuerList.Codes.Customs, YesNoList.Codes.Yes);
					AssertEquals("invoiceLine.PreviousDocuments.Count", 1, invoiceLine.PreviousDocuments.Count);
					AssertCusSupportingInfoContents(invoiceLine.PreviousDocuments[0], "LC2", ZString.Empty, new ZDate(2017, 3, 1), "INVLINE MORE INFO 2", ZDecimal.Zero, "INVLINE2", string.Empty, PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures);
					AssertEquals("invoiceLine.SupportingDocuments.Count", 1, invoiceLine.SupportingDocuments.Count);
					AssertCusSupportingInfoContents(invoiceLine.SupportingDocuments[0], "LC3", ZString.Empty, ZDate.Empty, "INVLINE BECAUSE", 333.34m, "INVLINES3", "FG", "PAR1");
					AssertEquals("invoiceLine.TaxOrFeeCollection.Count", 1, invoiceLine.Taxes.Count);
					AssertInvoiceLineTaxContents(invoiceLine.Taxes[0], 451, 452, 453, "TC1", "TC2", "TC3", "TC4");

					declaration.AdditionalInfos[0].CSI_Code = "DC4";
					declaration.PreviousDocuments[0].CSI_Code = "DC5";
					declaration.SupportingDocuments[0].CSI_Code = "DC6";
					invoice.AdditionalInfos[0].CSI_Code = "IC4";
					invoice.PreviousDocuments[0].CSI_Code = "IC5";
					invoice.SupportingDocuments[0].CSI_Code = "IC6";
					invoiceLine.AdditionalInfos[0].CSI_Code = "LC4";
					invoiceLine.PreviousDocuments[0].CSI_Code = "LC5";
					invoiceLine.SupportingDocuments[0].CSI_Code = "LC6";

					var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
					var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
					using (((IExternalFetchHintSupporter)newFactory).SetupCreator())
					{
						var exportShipment = (Shipment)writer.GetDataObject(declaration);
						AssertEquals(3, exportShipment.CommercialInfo.AddInfoGroupCollection.Count);
						AssertOldAdditionalInfo(exportShipment.CommercialInfo.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Type), CusSupportingInfoTypeList.Descriptions.AdditionalInfo, "DC4", "DEC ADD DESC 1", Core.Constants.CountryCodes.Afghanistan, YesNoList.Codes.Yes);
						AssertOldPreviousDocument(exportShipment.CommercialInfo.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Type), CusSupportingInfoTypeList.Descriptions.PreviousDocument, "DC5", new ZDate(2017, 1, 1), "DEC MORE INFO 2", "DECP2", PreviousDocumentClassList.Codes.PreviousAdministrativeReferenceNCTS);
						AssertOldSupportingDocument(exportShipment.CommercialInfo.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Type), CusSupportingInfoTypeList.Descriptions.SupportingDocument, "DC6", "DECS3", null, 10.32m, null, null, null);
						var invoiceData = exportShipment.CommercialInfo.CommercialInvoiceCollection[0];
						AssertOldAdditionalInfo(invoiceData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Type), CusSupportingInfoTypeList.Descriptions.AdditionalInfo, "IC4", "INVOICE ADD DESC 1", Core.Constants.CountryCodes.Bahamas, "");
						AssertOldPreviousDocument(invoiceData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Type), CusSupportingInfoTypeList.Descriptions.PreviousDocument, "IC5", new ZDate(2017, 2, 1), "INV MORE INFO 2", "INVP2", PreviousDocumentClassList.Codes.SummaryDeclaration);
						AssertOldSupportingDocument(invoiceData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Type), CusSupportingInfoTypeList.Descriptions.SupportingDocument, "IC6", "INVS3", null, 34.23m, null, null, null);
						var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
						AssertOldAdditionalInfo(invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Type), CusSupportingInfoTypeList.Descriptions.AdditionalInfo, "LC4", "INVOICE LINE ADD DESC 1", Core.Constants.CountryCodes.Cambodia, YesNoList.Codes.Yes);
						AssertOldPreviousDocument(invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Type), CusSupportingInfoTypeList.Descriptions.PreviousDocument, "LC5", new ZDate(2017, 3, 1), "INVLINE MORE INFO 2", "INVLINE2", PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures);
						AssertOldSupportingDocument(invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Type), CusSupportingInfoTypeList.Descriptions.SupportingDocument, "LC6", "INVLINES3", null, 333.34m, null, null, null);
					}
				});
			}
		}

		public static void AssertOldSupportingDocument(AddInfoGroup addInfoGroup, ZString typeDesc, ZString code, ZString reference, ZString? reason, ZDecimal qty, ZString? part, ZString? actions, ZString? availability)
		{
			AssertEquals("addInfoGroup.Type.Description", typeDesc, addInfoGroup.Type.Description);
			AssertEquals("addInfoGroup.AddInfoCollection.Code", code, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.TypeCode));
			AssertEquals("addInfoGroup.AddInfoCollection.Reference", reference, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Reference));
			AssertEquals("addInfoGroup.AddInfoCollection.Reason", reason, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Reason));
			AssertEquals("addInfoGroup.AddInfoCollection.Reason", qty, addInfoGroup.AddInfoCollection.GetZDecimalValue(Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Qty));
			AssertEquals("addInfoGroup.AddInfoCollection.Part", part, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Part));
			AssertEquals("addInfoGroup.AddInfoCollection.Actions", actions, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Actions));
			AssertEquals("addInfoGroup.AddInfoCollection.Availability", availability, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Availability));
		}

		public static void AssertOldPreviousDocument(AddInfoGroup addInfoGroup, ZString typeDesc, ZString code, ZDate dateOfIssue, ZString moreInfo, ZString reference, ZString classType)
		{
			AssertEquals("addInfoGroup.Type.Description", typeDesc, addInfoGroup.Type.Description);
			AssertEquals("addInfoGroup.AddInfoCollection.Code", code, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Fields.TypeCode));
			AssertEquals("addInfoGroup.AddInfoCollection.DateOfIssue", dateOfIssue, addInfoGroup.AddInfoCollection.GetZDateTimeValue(Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Fields.DateOfIssue));
			AssertEquals("addInfoGroup.AddInfoCollection.MoreInfo", moreInfo, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Fields.MoreInfo));
			AssertEquals("addInfoGroup.AddInfoCollection.Reference", reference, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Fields.Reference));
			AssertEquals("addInfoGroup.AddInfoCollection.Class", classType, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Fields.Class));
		}

		public static void AssertOldAdditionalInfo(AddInfoGroup addInfoGroup, ZString typeDesc, ZString code, ZString description, ZString nctsExportFromCountry, ZString nctsExportFromEC)
		{
			AssertEquals("addInfoGroup.Type.Description", typeDesc, addInfoGroup.Type.Description);
			AssertEquals("addInfoGroup.AddInfoCollection.Code", code, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.TypeCode));
			AssertEquals("addInfoGroup.AddInfoCollection.Description", description, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.Description));
			AssertEquals("addInfoGroup.AddInfoCollection.NctsExportFromCountry", nctsExportFromCountry, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.NctsExportFromCountry));
			AssertEquals("addInfoGroup.AddInfoCollection.NctsExportFromEC", nctsExportFromEC, addInfoGroup.AddInfoCollection.GetZStringValue(Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.NctsExportFromEC));
		}

		public static void AssertCusSupportingInfoContents(CusSupportingInfo cusSupportingInfo, ZString code, ZString country, ZDate dateOfIssue, ZString description, ZDecimal quantity, ZString referenceNumber, ZString status, ZString subType)
		{
			AssertEquals("cusSupportingInfo.CSI_Code", code, cusSupportingInfo.CSI_Code);
			AssertEquals("cusSupportingInfo.CSI_RN_NKCountryCode", country, cusSupportingInfo.CSI_RN_NKCountryCode);
			AssertEquals("cusSupportingInfo.CSI_DateOfIssue", dateOfIssue, cusSupportingInfo.CSI_DateOfIssue);
			AssertEquals("cusSupportingInfo.CSI_Description", description, cusSupportingInfo.CSI_Description);
			AssertEquals("cusSupportingInfo.CSI_Quantity", quantity, cusSupportingInfo.CSI_Quantity);
			AssertEquals("cusSupportingInfo.CSI_ReferenceNumber", referenceNumber, cusSupportingInfo.CSI_ReferenceNumber);
			AssertEquals("cusSupportingInfo.CSI_Status", status, cusSupportingInfo.CSI_Status);
			AssertEquals("cusSupportingInfo.CSI_SubType", subType, cusSupportingInfo.CSI_SubType);
		}

		public static void AssertInvoiceLineTaxContents(JobComInvoiceLineTax jobComInvoiceTaxLine, ZDecimal amount, ZDecimal baseQuantity, ZDecimal baseValue, ZString methodOfCalculation, ZString methodOfPayment, ZString rateReasonOverride, ZString type)
		{
			AssertEquals("invoiceLine.Tax.Amount", amount, jobComInvoiceTaxLine.JLT_Amount);
			AssertEquals("invoiceLine.Tax.BaseQuantity", baseQuantity, jobComInvoiceTaxLine.JLT_BaseQuantity);
			AssertEquals("invoiceLine.Tax.BaseValue", baseValue, jobComInvoiceTaxLine.JLT_BaseValue);
			AssertEquals("invoiceLine.Tax.MethodOfCalculation", methodOfCalculation, jobComInvoiceTaxLine.JLT_MethodOfCalculation);
			AssertEquals("invoiceLine.Tax.MethodOfPayment", methodOfPayment, jobComInvoiceTaxLine.JLT_MethodOfPayment);
			AssertEquals("invoiceLine.Tax.RateReasonOverride", rateReasonOverride, jobComInvoiceTaxLine.JLT_RateOverrideReasonCode);
			AssertEquals("invoiceLine.Tax.Type", type, jobComInvoiceTaxLine.JLT_Type);
		}

		public static AddInfoGroup CreateSupportingDocument(ZString code, ZString reference, ZString reason, ZDecimal qty, ZString part, ZString actions, ZString availability)
		{
			return new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Type },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection($"{Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.TypeCode}={code}*{Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Reference}={reference}*{Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Reason}={reason}*" +
					$"{Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Qty}={qty}*{Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Part}={part}*{Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Actions}={actions}*{Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Fields.Availability}={availability}")
			};
		}

		public static AddInfoGroup CreatePreviousDocument(ZString code, ZDate dateOfIssue, ZString moreInfo, ZString reference, ZString classType)
		{
			return new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Type },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection($"{Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Fields.TypeCode}={code}*{Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Fields.DateOfIssue}={dateOfIssue}*{Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Fields.MoreInfo}={moreInfo}*" +
					$"{Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Fields.Reference}={reference}*{Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Fields.Class}={classType}")
			};
		}

		public static AddInfoGroup CreateAdditionalInfo(ZString code, ZString description, ZString nctsExportFromCountry, ZString nctsExportFromEC)
		{
			return new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Type },
				AddInfoCollection = AddInfoCollectionCreator.CreateCollection($"{Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.TypeCode}={code}*{Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.Description}={description}*" +
					$"{Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.NctsExportFromCountry}={nctsExportFromCountry}*{Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Fields.NctsExportFromEC}={nctsExportFromEC}")
			};
		}

		public static TaxOrFee CreateTaxOrFee(ZDecimal amount, ZDecimal baseQuantity, ZDecimal baseValue, CodeDescriptionPair4Char methodOfCalculation, CodeDescriptionPair methodOfPayment, CodeDescriptionPair rateReasonOverride, CodeDescriptionPair6Char type)
		{
			return new TaxOrFee()
			{
				Amount = amount,
				BaseQuantity = baseQuantity,
				BaseValue = baseValue,
				MethodOfCalculation = methodOfCalculation,
				MethodOfPayment = methodOfPayment,
				RateReasonOverride = rateReasonOverride,
				Type = type
			};
		}

		public void TestImportOfficeCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
					WayBillNumber = "MYMASTER",
					WayBillType = new WayBillType { Code = WayBillTypeList.Codes.Master },
				};
				declarationDataObject.SetAddInfoGroupCollection(() => new List<AddInfoGroup>
					{
						new AddInfoGroup
						{
							Type = new CodeDescriptionPair { Code = "EUO", Description = "Office Code" },
							AddInfoCollection = new List<UniversalAddInfo>
							{
								new UniversalAddInfo { Key = "Purpose", Value = "EXP" },
								new UniversalAddInfo { Key = "OfficeCode", Value = "100" },
								new UniversalAddInfo { Key = "Time", Value = "2017-10-23 09:46:00.000" },
							}
						},
						new AddInfoGroup
						{
							Type = new CodeDescriptionPair { Code = "EUO", Description = "Office Code" },
							AddInfoCollection = new List<UniversalAddInfo>
							{
								new UniversalAddInfo { Key = "Purpose", Value = "EXT" },
								new UniversalAddInfo { Key = "OfficeCode", Value = "201" },
							}
						}
					});
				var logger = new TestErrorLogger();
				var provider = new CustomsShipmentDataObjectReaderProvider();
				BusinessObject bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				var declarationBO = (JobDeclaration)bizObj;
				var customsOffices = declarationBO.CustomsOffices;
				customsOffices.Load();
				var expOffice = customsOffices.Cast<EuOfficeCode>().Single(x => x.CY_Code == "EXP");
				AssertEquals("100", expOffice.CY_Data);
				AssertEquals(new ZDateTime(2017, 10, 23, 9, 46, 0), expOffice.CY_Date);
				var extOffice = customsOffices.Cast<EuOfficeCode>().Single(x => x.CY_Code == "EXT");
				AssertEquals("201", extOffice.CY_Data);
				AssertEquals(ZDateTime.Empty, extOffice.CY_Date);
			}
		}

		public void TestImportDeclarationTransportMeans_Road()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
				declaration.JE_Trailer1RegNo = "1";
				declaration.JE_RN_NKTrailer1Nationality = "ZA";
				declaration.JE_Trailer2RegNo = "3";
				declaration.JE_RN_NKTrailer2Nationality = "US";
				Factory.SaveForTesting();

				var shipment = CreateShipment(declaration.JE_DeclarationReference);
				shipment.SetAddInfoCollection(() => new List<UniversalAddInfo>
				{
					new UniversalAddInfo {  Key = Customs.DataTransfer.Universal.Constants.AddInfoKeys.Declaration.InlandModeOfTransport, Value = Core.Constants.TransportModes.Road }
				});
				shipment.SetTransportMeansCollection(() => new List<TransportMeans>
				{
					new TransportMeans
					{
						TransportType = TransportTypeCode.Active,
						Order = 0,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle },
						IdentificationNumber = "89756",
						Nationality = new CodeDescriptionPair2Char { Code = "UK" }
					},
					new TransportMeans
					{
						TransportType = TransportTypeCode.Inland,
						Order = 0,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle },
						IdentificationNumber = "12345",
						Nationality = new CodeDescriptionPair2Char { Code = "IE" }
					},
					new TransportMeans
					{
						TransportType = TransportTypeCode.Inland,
						Order = 3,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer },
						IdentificationNumber = "12345",
						Nationality = new CodeDescriptionPair2Char { Code = "IE" }
					},
					new TransportMeans
					{
						TransportType = TransportTypeCode.Inland,
						Order = 1,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer },
						IdentificationNumber = "24680",
						Nationality = new CodeDescriptionPair2Char { Code = "AU" }
					},
					new TransportMeans
					{
						TransportType = TransportTypeCode.Inland,
						Order = 2,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer },
						IdentificationNumber = "98765",
						Nationality = new CodeDescriptionPair2Char { Code = "FR" }
					},
				});

				CombineAssertions(() =>
				{
					AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());

					AssertEquals("declaration.JE_TransportModeInland", Core.Constants.TransportModes.Road, declaration.JE_TransportModeInland);
					AssertEquals("declaration.JE_TransportMeans", TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, declaration.JE_TransportMeans);
					AssertEquals("declaration.JE_TransportIDInland", "12345", declaration.JE_TransportIDInland);
					AssertEquals("declaration.JE_RN_NKTransportNationalityInland", "IE", declaration.JE_RN_NKTransportNationalityInland);
					AssertEquals("declaration.JE_Trailer1RegNo", "24680", declaration.JE_Trailer1RegNo);
					AssertEquals("declaration.JE_RN_NKTrailer1Nationality", "AU", declaration.JE_RN_NKTrailer1Nationality);
					AssertEquals("declaration.JE_Trailer2RegNo", "98765", declaration.JE_Trailer2RegNo);
					AssertEquals("declaration.JE_RN_NKTrailer2Nationality", "FR", declaration.JE_RN_NKTrailer2Nationality);
				});
			}
		}

		public void TestImportDeclarationTransportMeans_DoNotImportTrailerWhenNotRoad()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_Trailer1RegNo = "1";
				declaration.JE_RN_NKTrailer1Nationality = "ZA";
				declaration.JE_Trailer2RegNo = "3";
				declaration.JE_RN_NKTrailer2Nationality = "US";
				Factory.SaveForTesting();

				var shipment = CreateShipment(declaration.JE_DeclarationReference);
				shipment.SetAddInfoCollection(() => new List<UniversalAddInfo>
				{
					new UniversalAddInfo {  Key = Customs.DataTransfer.Universal.Constants.AddInfoKeys.Declaration.InlandModeOfTransport, Value = Core.Constants.TransportModes.Sea }
				});
				shipment.SetTransportMeansCollection(() => new List<TransportMeans>
				{
					new TransportMeans
					{
						TransportType = TransportTypeCode.Active,
						Order = 0,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle },
						IdentificationNumber = "89756",
						Nationality = new CodeDescriptionPair2Char { Code = "UK" }
					},
					new TransportMeans
					{
						TransportType = TransportTypeCode.Inland,
						Order = 0,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.NameOfTheSeaGoingVessel },
						IdentificationNumber = "12345",
						Nationality = new CodeDescriptionPair2Char { Code = "IE" }
					},
					new TransportMeans
					{
						TransportType = TransportTypeCode.Inland,
						Order = 1,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer },
						IdentificationNumber = "24680",
						Nationality = new CodeDescriptionPair2Char { Code = "AU" }
					},
					new TransportMeans
					{
						TransportType = TransportTypeCode.Inland,
						Order = 2,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer },
						IdentificationNumber = "98765",
						Nationality = new CodeDescriptionPair2Char { Code = "FR" }
					},
				});

				CombineAssertions(() =>
				{
					AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());

					AssertEquals("declaration.JE_TransportModeInland", Core.Constants.TransportModes.Sea, declaration.JE_TransportModeInland);
					AssertEquals("declaration.JE_TransportMeans", TransportMeansList.Codes.NameOfTheSeaGoingVessel, declaration.JE_TransportMeans);
					AssertEquals("declaration.JE_TransportIDInland", "12345", declaration.JE_TransportIDInland);
					AssertEquals("declaration.JE_RN_NKTransportNationalityInland", "IE", declaration.JE_RN_NKTransportNationalityInland);
					AssertEquals("declaration.JE_Trailer1RegNo", "1", declaration.JE_Trailer1RegNo);
					AssertEquals("declaration.JE_RN_NKTrailer1Nationality", "ZA", declaration.JE_RN_NKTrailer1Nationality);
					AssertEquals("declaration.JE_Trailer2RegNo", "3", declaration.JE_Trailer2RegNo);
					AssertEquals("declaration.JE_RN_NKTrailer2Nationality", "US", declaration.JE_RN_NKTrailer2Nationality);
				});
			}
		}

		public void TestImportDeclarationTransportMeans_Road_AddInfo_InlandModeOfTransport()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
				declaration.JE_Trailer1RegNo = "1";
				declaration.JE_RN_NKTrailer1Nationality = "ZA";
				declaration.JE_Trailer2RegNo = "3";
				declaration.JE_RN_NKTrailer2Nationality = "US";
				Factory.SaveForTesting();

				var shipment = CreateShipment(declaration.JE_DeclarationReference);
				shipment.SetTransportMeansCollection(() => new List<TransportMeans>
				{
					new TransportMeans
					{
						TransportType = TransportTypeCode.Inland,
						Order = 0,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle },
						IdentificationNumber = "12345",
						Nationality = new CodeDescriptionPair2Char { Code = "IE" }
					},
					new TransportMeans
					{
						TransportType = TransportTypeCode.Inland,
						Order = 3,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer },
						IdentificationNumber = "12345",
						Nationality = new CodeDescriptionPair2Char { Code = "IE" }
					},
					new TransportMeans
					{
						TransportType = TransportTypeCode.Inland,
						Order = 1,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer },
						IdentificationNumber = "24680",
						Nationality = new CodeDescriptionPair2Char { Code = "AU" }
					},
					new TransportMeans
					{
						TransportType = TransportTypeCode.Inland,
						Order = 2,
						TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer },
						IdentificationNumber = "98765",
						Nationality = new CodeDescriptionPair2Char { Code = "FR" }
					},
				});

				CombineAssertions(() =>
				{
					AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());

					AssertEquals("declaration.JE_TransportModeInland", Core.Constants.TransportModes.Road, declaration.JE_TransportModeInland);
					AssertEquals("declaration.JE_TransportMeans", TransportMeansList.Codes.RegistrationNumberOfTheRoadVehicle, declaration.JE_TransportMeans);
					AssertEquals("declaration.JE_TransportIDInland", "12345", declaration.JE_TransportIDInland);
					AssertEquals("declaration.JE_RN_NKTransportNationalityInland", "IE", declaration.JE_RN_NKTransportNationalityInland);
					AssertEquals("declaration.JE_Trailer1RegNo", "24680", declaration.JE_Trailer1RegNo);
					AssertEquals("declaration.JE_RN_NKTrailer1Nationality", "AU", declaration.JE_RN_NKTrailer1Nationality);
					AssertEquals("declaration.JE_Trailer2RegNo", "98765", declaration.JE_Trailer2RegNo);
					AssertEquals("declaration.JE_RN_NKTrailer2Nationality", "FR", declaration.JE_RN_NKTrailer2Nationality);
				});
			}
		}

		public void TestDeclarationLocationOfGoods()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.SaveForTesting();

			var shipment = CreateShipment(declaration.JE_DeclarationReference);
			shipment.SetLocationOfGoodsCollection(() => new List<LocationOfGoods>
			{
				new LocationOfGoods
				{
					Qualifier = new CodeDescriptionPair1Char { Code = "Z", Description = "Address" },
					LocationType = new CodeDescriptionPair1Char { Code = "B", Description = "Authorized Place" },
					AuthorizationNumber = "A123",
					AdditionalIdentifier = "Z99",
					Contact = new Contact
					{
						Name = "Bob",
						PhoneNumber = "923",
						Email = "bob@gmail.in",
					},
				}
			});

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
			{
				CombineAssertions("When Ucc6 false", () =>
				{
					AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
					var goodsLocation = declaration.GoodsLocation;

					AssertEquals("Qualifier", ZString.Empty, goodsLocation.CGL_Qualifier);
					AssertEquals("Type", ZString.Empty, goodsLocation.CGL_Type);
					AssertEquals("AuthorisationNumber", ZString.Empty, goodsLocation.Address.AuthorisationNumber);
					AssertEquals("AdditionalIdentifier", ZString.Empty, goodsLocation.CGL_AdditionalIdentifier);
					AssertEquals("Contact Name", ZString.Empty, goodsLocation.Address.E2_Contact);
					AssertEquals("Contact Phone", ZString.Empty, goodsLocation.Address.E2_Phone);
					AssertEquals("Contact Email", ZString.Empty, goodsLocation.Address.E2_Email);
				});
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				CombineAssertions("When Ucc6 true", () =>
				{
					AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
					var goodsLocation = declaration.GoodsLocation;

					AssertEquals("Qualifier", "Z", goodsLocation.CGL_Qualifier);
					AssertEquals("Type", "B", goodsLocation.CGL_Type);
					AssertEquals("AuthorisationNumber", "A123", goodsLocation.Address.AuthorisationNumber);
					AssertEquals("AdditionalIdentifier", "Z99", goodsLocation.CGL_AdditionalIdentifier);
					AssertEquals("Contact Name", "Bob", goodsLocation.Address.E2_Contact);
					AssertEquals("Contact Phone", "923", goodsLocation.Address.E2_Phone);
					AssertEquals("Contact Email", "bob@gmail.in", goodsLocation.Address.E2_Email);
				});
			}
		}

		public void TestFillGoodsLocationAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "ORG1";
			var orgAddress1 = orgHeader1.MainAddress;
			orgAddress1.AddressCode = "ADD1";
			Factory.SaveForTesting();

			var shipment = CreateShipment(declaration.JE_DeclarationReference);
			var organizationAddressInShipment = new OrganizationAddress()
			{
				AddressType = "LocationOfGoods",
				OrganizationCode = "ORG1",
				AddressShortCode = "ADD1",
			};

			var locationOfGoodsInShipment = new LocationOfGoods
			{
				Qualifier = new CodeDescriptionPair1Char { Code = "Z" },
			};

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				organizationAddressInShipment
			});

			shipment.SetLocationOfGoodsCollection(() => new List<LocationOfGoods>
			{
				locationOfGoodsInShipment
			});

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
				var goodsLocation = declaration.GoodsLocation;
				CombineAssertions("When Qualifier = Z", () =>
				{
					AssertEquals("OrganisationPK", orgHeader1.PK, goodsLocation.Address.OrganisationPK);
					AssertEquals("E2_OA_Address", orgAddress1.PK, goodsLocation.Address.E2_OA_Address);
					AssertEquals("IdentificationHolderPK", ZGuid.Empty, goodsLocation.Address.IdentificationHolderPK);
				});

				locationOfGoodsInShipment.Qualifier.Code = "Y";
				AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
				CombineAssertions("When Qualifier = Y", () =>
				{
					AssertEquals("OrganisationPK", ZGuid.Empty, goodsLocation.Address.OrganisationPK);
					AssertEquals("E2_OA_Address", ZGuid.Empty, goodsLocation.Address.E2_OA_Address);
					AssertEquals("IdentificationHolderPK", orgHeader1.PK, goodsLocation.Address.IdentificationHolderPK);
				});

				locationOfGoodsInShipment.Qualifier.Code = "K";
				AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
				CombineAssertions("When Qualifier = K", () =>
				{
					AssertEquals("OrganisationPK", ZGuid.Empty, goodsLocation.Address.OrganisationPK);
					AssertEquals("E2_OA_Address", ZGuid.Empty, goodsLocation.Address.E2_OA_Address);
					AssertEquals("IdentificationHolderPK", ZGuid.Empty, goodsLocation.Address.IdentificationHolderPK);
				});

				locationOfGoodsInShipment.Qualifier.Code = "";
				AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
				CombineAssertions("When Qualifier Empty", () =>
				{
					AssertEquals("OrganisationPK", ZGuid.Empty, goodsLocation.Address.OrganisationPK);
					AssertEquals("E2_OA_Address", ZGuid.Empty, goodsLocation.Address.E2_OA_Address);
					AssertEquals("IdentificationHolderPK", ZGuid.Empty, goodsLocation.Address.IdentificationHolderPK);
				});

				organizationAddressInShipment.AddressType = "Exporter";
				locationOfGoodsInShipment.Qualifier.Code = "Z";
				AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
				CombineAssertions("When AddressType = Exporter", () =>
				{
					AssertEquals("OrganisationPK", ZGuid.Empty, goodsLocation.Address.OrganisationPK);
					AssertEquals("E2_OA_Address", ZGuid.Empty, goodsLocation.Address.E2_OA_Address);
					AssertEquals("IdentificationHolderPK", ZGuid.Empty, goodsLocation.Address.IdentificationHolderPK);
				});
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
			{
				organizationAddressInShipment.AddressType = "LocationOfGoods";
				AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
				var goodsLocation = declaration.GoodsLocation;
				CombineAssertions("When Ucc6 false, Qualifier = Z", () =>
				{
					AssertEquals("OrganisationPK", ZGuid.Empty, goodsLocation.Address.OrganisationPK);
					AssertEquals("E2_OA_Address", ZGuid.Empty, goodsLocation.Address.E2_OA_Address);
					AssertEquals("IdentificationHolderPK", ZGuid.Empty, goodsLocation.Address.IdentificationHolderPK);
				});
			}
		}

		public void TestFillGoodsLocationAddressOverride()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.SaveForTesting();

			var shipment = CreateShipment(declaration.JE_DeclarationReference);
			var organizationAddressInShipment = new OrganizationAddress()
			{
				AddressType = "LocationOfGoods",
				CompanyName = "ABC Ltd",
				AddressOverride = false,
				Address1 = "Line1",
				Address2 = "Cross1",
				City = "Padoa",
				Postcode = "87731",
				Country = new Country { Code = "IT" },
			};

			var locationOfGoodsInShipment = new LocationOfGoods
			{
				Qualifier = new CodeDescriptionPair1Char { Code = "Z" },
			};

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				organizationAddressInShipment
			});

			shipment.SetLocationOfGoodsCollection(() => new List<LocationOfGoods>
			{
				locationOfGoodsInShipment
			});

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
				var goodsLocation = declaration.GoodsLocation;

				CombineAssertions("When address not found and Override false", () =>
				{
					AssertEquals("E2_CompanyName", "", goodsLocation.Address.E2_CompanyName);
					AssertEquals("E2_AddressOverride", false, goodsLocation.Address.E2_AddressOverride);
					AssertEquals("E2_Address1", "", goodsLocation.Address.E2_Address1);
					AssertEquals("E2_Address2", "", goodsLocation.Address.E2_Address2);
					AssertEquals("E2_City", "", goodsLocation.Address.E2_City);
					AssertEquals("E2_Postcode", "", goodsLocation.Address.E2_Postcode);
					AssertEquals("E2_RN_NKCountryCode", "", goodsLocation.Address.E2_RN_NKCountryCode);
				});

				organizationAddressInShipment.AddressOverride = true;
				AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
				CombineAssertions("When address not found and Override true", () =>
				{
					AssertEquals("E2_CompanyName", "ABC Ltd", goodsLocation.Address.E2_CompanyName);
					AssertEquals("E2_AddressOverride", true, goodsLocation.Address.E2_AddressOverride);
					AssertEquals("E2_Address1", "Line1", goodsLocation.Address.E2_Address1);
					AssertEquals("E2_Address2", "Cross1", goodsLocation.Address.E2_Address2);
					AssertEquals("E2_City", "Padoa", goodsLocation.Address.E2_City);
					AssertEquals("E2_Postcode", "87731", goodsLocation.Address.E2_Postcode);
					AssertEquals("E2_RN_NKCountryCode", "IT", goodsLocation.Address.E2_RN_NKCountryCode);
				});

				var orgHeader1 = Factory.New<OrgHeader>();
				orgHeader1.OH_Code = "ORG1";
				var orgAddress1 = orgHeader1.MainAddress;
				orgAddress1.AddressCode = "ADD1";

				organizationAddressInShipment.OrganizationCode = "ORG1";
				organizationAddressInShipment.AddressShortCode = "ADD1";
				Factory.SaveForTesting();

				AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
				CombineAssertions("When address found but Override true", () =>
				{
					AssertEquals("E2_CompanyName", "ABC Ltd", goodsLocation.Address.E2_CompanyName);
					AssertEquals("E2_AddressOverride", true, goodsLocation.Address.E2_AddressOverride);
					AssertEquals("E2_Address1", "Line1", goodsLocation.Address.E2_Address1);
					AssertEquals("E2_Address2", "Cross1", goodsLocation.Address.E2_Address2);
					AssertEquals("E2_City", "Padoa", goodsLocation.Address.E2_City);
					AssertEquals("E2_Postcode", "87731", goodsLocation.Address.E2_Postcode);
					AssertEquals("E2_RN_NKCountryCode", "IT", goodsLocation.Address.E2_RN_NKCountryCode);
				});
			}
		}

		public void TestFillLocationOfGoodsAuthorizationWithOrganization()
		{
			var declaration = Factory.New<JobDeclaration>();
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "ORG1";
			var orgAddress1 = orgHeader1.MainAddress;
			orgAddress1.AddressCode = "ADD1";
			Factory.SaveForTesting();

			var shipment = CreateShipment(declaration.JE_DeclarationReference);
			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				new OrganizationAddress()
				{
					AddressType = "LocationOfGoods",
					OrganizationCode = "ORG1",
					AddressShortCode = "ADD1",
				},
			});

			shipment.SetLocationOfGoodsCollection(() => new List<LocationOfGoods>
			{
				new LocationOfGoods
				{
					Qualifier = new CodeDescriptionPair1Char { Code = "Y" },
					AuthorizationNumber = "A1231"
				},
			});

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				CombineAssertions(() =>
				{
					AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
					var goodsLocation = declaration.GoodsLocation;
					AssertEquals("IdentificationHolderPK", orgHeader1.PK, goodsLocation.Address.IdentificationHolderPK);
					AssertEquals("AuthorisationNumber", "A1231", goodsLocation.Address.AuthorisationNumber);
				});
			}
		}

		public void TestFillLocationOfGoodsContactDetailsWithOrganization()
		{
			var declaration = Factory.New<JobDeclaration>();
			var orgHeader1 = Factory.New<OrgHeader>();
			orgHeader1.OH_Code = "ORG1";
			var orgAddress1 = orgHeader1.MainAddress;
			orgAddress1.AddressCode = "ADD1";
			orgAddress1.OA_Phone = "100";
			orgAddress1.OA_Email = "touch@gmail.com";
			Factory.SaveForTesting();

			var shipment = CreateShipment(declaration.JE_DeclarationReference);

			var organizationAddress = new OrganizationAddress()
			{
				AddressType = "LocationOfGoods",
				OrganizationCode = "ORG1",
				AddressShortCode = "ADD1",
			};

			shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
			{
				organizationAddress
			});

			shipment.SetLocationOfGoodsCollection(() => new List<LocationOfGoods>
			{
				new LocationOfGoods
				{
					Qualifier = new CodeDescriptionPair1Char { Code = "Z" },
					Contact = new Contact
					{
						Name = "Bob",
						PhoneNumber = "923",
						Email = "bob@gmail.in",
					},
				}
			});

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				CombineAssertions("When AddressOverride false", () =>
				{
					AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
					var goodsLocation = declaration.GoodsLocation;
					AssertEquals("OrganisationPK", orgHeader1.PK, goodsLocation.Address.OrganisationPK);
					AssertEquals("E2_OA_Address", orgAddress1.PK, goodsLocation.Address.E2_OA_Address);

					AssertEquals("Qualifier", "Z", goodsLocation.CGL_Qualifier);
					AssertEquals("Contact Name", "Bob", goodsLocation.Address.E2_Contact);
					AssertEquals("Contact Phone", "100", goodsLocation.Address.E2_Phone);
					AssertEquals("Contact Email", "touch@gmail.com", goodsLocation.Address.E2_Email);
				});

				CombineAssertions("When AddressOverride true", () =>
				{
					organizationAddress.AddressOverride = true;
					AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
					var goodsLocation = declaration.GoodsLocation;

					AssertEquals("Qualifier", "Z", goodsLocation.CGL_Qualifier);
					AssertEquals("Contact Name", "Bob", goodsLocation.Address.E2_Contact);
					AssertEquals("Contact Phone", "923", goodsLocation.Address.E2_Phone);
					AssertEquals("Contact Email", "bob@gmail.in", goodsLocation.Address.E2_Email);
				});
			}
		}

		public void TestReadDutyPayer()
		{
			// test for IE
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				var declaration = Factory.New<JobDeclaration>();
				var orgHeader1 = Factory.New<OrgHeader>();
				orgHeader1.OH_Code = "ORG1";
				var orgAddress1 = orgHeader1.MainAddress;
				orgAddress1.AddressCode = "ADD1";
				Factory.SaveForTesting();

				var shipment = CreateShipment(declaration.JE_DeclarationReference);
				var organizationAddressInShipment = new OrganizationAddress()
				{
					AddressType = "DutyPayer",
					OrganizationCode = "ORG1",
					AddressShortCode = "ADD1",
				};
				shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					organizationAddressInShipment
				});
				AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
				var dutyPayer = declaration.DutyPayer;
				CombineAssertions("Duty Payer", () =>
				{
					AssertEquals("OrganisationPK", orgHeader1.PK, dutyPayer.PK);
					AssertEquals("E2_OA_Address", orgAddress1.PK, dutyPayer.MainAddress.PK);
				});
			}
		}

		public void TestReadDefermentParty()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				var declaration = Factory.New<JobDeclaration>();
				var orgHeader1 = Factory.New<OrgHeader>();
				orgHeader1.OH_Code = "ORG1";
				var orgAddress1 = orgHeader1.MainAddress;
				orgAddress1.AddressCode = "ADD1";
				Factory.SaveForTesting();

				var shipment = CreateShipment(declaration.JE_DeclarationReference);
				var organizationAddressInShipment = new OrganizationAddress()
				{
					AddressType = "DefermentParty",
					OrganizationCode = "ORG1",
					AddressShortCode = "ADD1",
				};
				shipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>
				{
					organizationAddressInShipment
				});
				AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
				var defermentParty = declaration.DefermentPartyDocAddress;
				CombineAssertions("Deferment Party", () =>
				{
					AssertEquals("OrganisationPK", orgHeader1.PK, defermentParty.OrganisationPK);
					AssertEquals("E2_OA_Address", orgAddress1.PK, defermentParty.E2_OA_Address);
				});
			}
		}

		public void TestReadInlandTransportForUCC5()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				var declaration = Factory.New<JobDeclaration>();
				Factory.SaveForTesting();

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true))
				{
					declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
					declaration.JE_TransportIDInland = "12345";
					declaration.JE_TransportMeans = TransportMeansList.Codes.RegistrationNumberOfTheRoadTrailer;
					declaration.JE_RN_NKTransportNationalityInland = "AU";

					var shipment = CreateShipment(declaration.JE_DeclarationReference);
					shipment.SetAddInfoCollection(() => new List<UniversalAddInfo>
					{
						new UniversalAddInfo {  Key = Customs.DataTransfer.Universal.Constants.AddInfoKeys.Declaration.InlandModeOfTransport, Value = Core.Constants.TransportModes.Air }
					});
					shipment.SetTransportMeansCollection(() => new List<TransportMeans>
					{
						new TransportMeans
						{
							TransportType = TransportTypeCode.Inland,
							Order = 0,
							TypeOfIdentification = new CodeDescriptionPair2Char { Code = TransportMeansList.Codes.RegistrationNumberOfTheAircraft },
							IdentificationNumber = "15000",
							Nationality = new CodeDescriptionPair2Char { Code = "IE" }
						},
					});

					CombineAssertions("Inland Transport for UCC5", () =>
					{
						AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
						AssertEquals("declaration.JE_TransportModeInland", Core.Constants.TransportModes.Air, declaration.JE_TransportModeInland);
						AssertEquals("declaration.JE_TransportMeans", TransportMeansList.Codes.RegistrationNumberOfTheAircraft, declaration.JE_TransportMeans);
						AssertEquals("declaration.JE_TransportIDInland", "15000", declaration.JE_TransportIDInland);
						AssertEquals("declaration.JE_RN_NKTransportNationalityInland", "IE", declaration.JE_RN_NKTransportNationalityInland);
					});
				}
			}
			// test for IE
		}

		public void TestAgreedPlaceCode()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.SaveForTesting();

			var shipment = CreateShipment(declaration.JE_DeclarationReference);
			shipment.AgreedPlaceCode = "IT922";
			AssertSame(declaration, new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject());
			AssertEquals("AgreedPlaceCode", "IT922", declaration.EUD_AgreedPlaceCode);
		}

		Shipment CreateShipment(ZString? declarationReference = null)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, declarationReference);
			return new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
			};
		}

		[TestDate(2020, 03, 31)]
		public void TestExportAndImportIsCorrectlyMapped()
		{
			localCountryCustomsInterface?.Dispose();
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Builtin;
			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface))
			using (BusinessObjectTestDataHelper.TemporarilySetCustomisableStringValue(SetCustomisableStringValue))
			{
				var testVessel = Factory.BOFactory.NewWithValidTestData<RefVessel>();
				testVessel.RV_Code = "VESSEL";
				testVessel.RV_LloydsNumber = "LLOYDSI";
				var declaration = Factory.BOFactory.New<JobDeclarationForTesting>();
				var orgHeader = Factory.BOFactory.NewWithValidTestData<OrgHeader>();
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				declaration.ZG_AgreedPlaceCode = "1";
				var processedTypes = new LRUCache<Type, PropertyDescriptor[]>();
				var processedBizObjs = new List<BusinessObject>();
				CreateChildObject(declaration.AdditionalInfos.AddNew(), processedBizObjs, processedTypes);
				CreateChildObject(declaration.PreviousDocuments.AddNew(), processedBizObjs, processedTypes);
				CreateChildObject(declaration.SupportingDocuments.AddNew(), processedBizObjs, processedTypes);
				var container = declaration.CusContainers.AddNew();
				CreateChildObject(container, processedBizObjs, processedTypes);
				var entryInstruction = (CusEntryInstruction)declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				CreateChildObject(entryInstruction, processedBizObjs, processedTypes);
				var entryInstructionFiscalReference = entryInstruction.FiscalReferences[0];
				entryInstructionFiscalReference.CFR_Type = CusReferenceTypeList.Codes.FiscalReference;
				entryInstructionFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR1_Importer;
				var cusAuthorizationUsage = entryInstruction.CusAuthorizationUsages[0];
				cusAuthorizationUsage.AGC_Code = "ABC";
				cusAuthorizationUsage.AGC_Number = "123";
				cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
				var topGroupInvoice = declaration.TopGroupInvoice;
				CreateChildObject(topGroupInvoice, processedBizObjs, processedTypes);

				var invoice = topGroupInvoice.AllJobComInvoiceHeaders.Count > 0 ? topGroupInvoice.AllJobComInvoiceHeaders[0] : declaration.TopGroupInvoice.AllJobComInvoiceHeaders.AddNew();
				invoice.JZ_InvoiceDate = new ZDateTime(2020, 03, 31);
				CreateChildObject(invoice, processedBizObjs, processedTypes);
				invoice.JZ_InvoiceNumber = "INV3224";
				((JobComInvoiceHeader)invoice).IncoTermsAgreedPlace = ZString.Empty;
				foreach (InvoiceCharge invoiceCharge in invoice.Charges)
				{
					invoiceCharge.J7_IsNotIncludedInInvoice = true;
				}
				var invoiceLine = (JobComInvoiceLine)invoice.JobComInvoiceLines.AddNew();
				CreateChildObject(invoiceLine, processedBizObjs, processedTypes);
				invoiceLine.JI_LineNo = 1;
				foreach (InvoiceLineCharge invoiceLineCharge in invoiceLine.Charges)
				{
					invoiceLineCharge.J7_IsNotIncludedInInvoice = true;
				}
				invoiceLine.SupervisingOfficeDocAddress.E2_AddressOverride = true;
				invoiceLine.SupervisingOfficeDocAddress.E2_CompanyName = "BOB THE BUILDER";
				CreateChildObject(declaration, processedBizObjs, processedTypes);
				CreateChildObject(declaration.PrimaryMasterBill, processedBizObjs, processedTypes);
				CreateChildObject(declaration.PrimaryHouseBill, processedBizObjs, processedTypes);
				declaration.JE_MasterBill = "MB123";
				declaration.JE_TotalNoOfPacks = 110;
				declaration.JE_TotalNoOfPieces = 220;
				declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
				declaration.ZG_ShipmentType = ShipmentTypeList.Codes.BasicDirect;
				invoiceLine.PackagesPivot.AddPivotFor(declaration.PrimaryHouseBill.PackingGroups[0].Packages[0]);
				invoiceLine.JI_OrderNumber = "ORDER2";
				ClearInvalidAndNotApplicableData(declaration);
				invoiceLine.ZG_StatisticalValue = invoiceLine.ZG_StatisticalValue;
				var undgDataItem = invoiceLine.UNDGs.First();
				undgDataItem.DI_UnitOfWeight = Core.Constants.Weight.Pounds;
				undgDataItem.DI_UnitOfVolume = Core.Constants.Volume.CubicFeet;
				cusAuthorizationUsage = invoiceLine.CusAuthorizationUsages[0];
				cusAuthorizationUsage.AGC_Code = "ABC";
				cusAuthorizationUsage.AGC_Number = "123";
				cusAuthorizationUsage.AGC_OH_Owner = orgHeader.PK;
				var dv1Detail = declaration.DV1Details.Count > 0 ? declaration.DV1Details[0] : declaration.DV1Details.AddNew();
				dv1Detail.DV1_CloseApproximation = YesNoList.Codes.No;
				dv1Detail.DV1_Resale = YesNoList.Codes.No;
				dv1Detail.DV1_Relationship = YesNoList.Codes.No;
				dv1Detail.DV1_RoyaltiesLicence = YesNoList.Codes.No;
				dv1Detail.DV1_Restrictions = YesNoList.Codes.No;
				dv1Detail.DV1_Consideration = YesNoList.Codes.No;
				var invoiceLineFiscalReference = invoiceLine.FiscalReferences[0];
				invoiceLineFiscalReference.CFR_Type = CusReferenceTypeList.Codes.FiscalReference;
				invoiceLineFiscalReference.CFR_Code = FiscalReferenceCodeList.Codes.FR2_Customer;
				CreateChildObject(dv1Detail, processedBizObjs, processedTypes);
				declaration.CustomsEntryHeaders[0].CH_WarehouseTransactionStatus = ZString.Empty;
				Factory.SaveForTesting();

				IEDIMessage message = null;
				CombineAssertions(delegate
				{
					var newFactory = new BusinessObjectFactory();
					declaration = newFactory.Load<JobDeclarationForTesting>(declaration.PK);
					message = GetXmlMessage(declaration, true);
					declaration.JE_MasterBill = "MB435";
					declaration.CustomsEntryHeaders[0].CH_BGMReference = ZString.Empty;
					newFactory.Save();
					var serviceTaskLog = new ServiceTaskLogForTesting();
					var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
					messageProcessingManager.Process(message);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
				});

				CombineAssertions(delegate
				{
					var newFactory = new BusinessObjectFactory();
					var query = new ZQuery(JobDeclarationSchema.JE_MasterBill, "MB123");
					query.AddToFilter(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declaration.PK);
					var declarations = newFactory.Load<JobDeclarationForTesting>(query);
					AssertEquals(1, declarations.Length);
					var newDeclaration = declarations[0];
					ClearInvalidAndNotApplicableData(newDeclaration);
					var message2 = GetXmlMessage(newDeclaration, false);
					var expectedExportXml = message.EM_MessageText.Replace("\r\n", "\n");
					var entryHeaderCollectionStartIndex = expectedExportXml.IndexOf("<EntryHeaderCollection>");
					const string entryHeaderCollectionEnd = "</EntryHeaderCollection>";
					var entryHeaderCollectionEndIndex = expectedExportXml.IndexOf(entryHeaderCollectionEnd);
					expectedExportXml = expectedExportXml.Left(entryHeaderCollectionStartIndex).TrimEnd() + "\n" + expectedExportXml.SubstringSafe(entryHeaderCollectionEndIndex + entryHeaderCollectionEnd.Length);
					AssertMultilineEquals("XML Data", expectedExportXml, message2.EM_MessageText.Replace("\r\n", "\n"), '\n');
				});
			}
		}

		public void TestMessageWithMissingBondNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Guarantees.AddNew();
			var message = GetXmlMessage(declaration, false);

			message.EM_MessageText = message.EM_MessageText.Replace("<BondNumber></BondNumber>", "");
			AssertNotContains("Pre-requisite: BondNumber should not be present in the test message", "<BondNumber>", message.EM_MessageText);

			var serviceTaskLog = new ServiceTaskLogForTesting();
			var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
			messageProcessingManager.Process(message);

			AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Warning, message.EM_Status);
		}

		public void TestImportLocationOfGood()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Italy))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
					WayBillNumber = "MYMASTER",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					LocationAtClearance = new CodeDescriptionPair35Char() { Code = "LOCGOO", Description = "LOCGOO S" }
				};
				var logger = new TestErrorLogger();
				var provider = new CustomsShipmentDataObjectReaderProvider();
				BusinessObject bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				var declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals("LOCGOO", declarationBO.JE_LocationOfGoods);

				declarationBO.JE_LocationOfGoods = ZString.Empty;
				declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "LOCGOO", Description = "AD SDSFSD DDS" };
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals("LOCGOO", declarationBO.JE_LocationOfGoods);

				declarationBO.JE_LocationOfGoods = ZString.Empty;
				declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "LOCGOO", Description = "" };
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals("LOCGOO", declarationBO.JE_LocationOfGoods);

				declarationBO.JE_LocationOfGoods = ZString.Empty;
				declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "LOCGOO" };
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals("LOCGOO", declarationBO.JE_LocationOfGoods);

				declarationBO.JE_LocationOfGoods = ZString.Empty;
				declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "", Description = "LOCGOO" };
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals(ZString.Empty, declarationBO.JE_LocationOfGoods);

				declarationBO.JE_LocationOfGoods = ZString.Empty;
				declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Description = "LOCGOO" };
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals(ZString.Empty, declarationBO.JE_LocationOfGoods);
			}
		}

		public void TestDefermentAccountNumberFromAddInfo()
		{
			const string deferNumberAddInfo = "ADDINFODEFERNUM123";
			const string defermentNumber = "ACTUALFIELDDEFER";
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
					WayBillNumber = "MASTERDEFER",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				};
				declarationDataObject.SetAddInfoCollection(() => new List<UniversalAddInfo>(new[] { new UniversalAddInfo() { Key = Universal.Constants.OldEUAddInfo.Fields.OtherDeferNumber, Value = deferNumberAddInfo } }));
				declarationDataObject.DefermentAccountNumber = defermentNumber;

				var logger = new TestErrorLogger();
				var provider = new CustomsShipmentDataObjectReaderProvider();
				BusinessObject bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				var declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals(defermentNumber, declarationBO.JE_DefermentAccountNumber);

				declarationDataObject.DefermentAccountNumber = null;
				bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals(deferNumberAddInfo, declarationBO.JE_DefermentAccountNumber);

				declarationDataObject.DefermentAccountNumber = defermentNumber;
				declarationDataObject.SetAddInfoCollection(() => null);
				bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals(defermentNumber, declarationBO.JE_DefermentAccountNumber);
			}
		}

		public void TestIATALoadPortFromAddInfo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
					WayBillNumber = "MASTERDEFER",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				};
				declarationDataObject.SetAddInfoCollection(() => new List<UniversalAddInfo>(new[] { new UniversalAddInfo() { Key = Universal.Constants.OldEUAddInfo.Fields.OSAirTransportLoad, Value = "BNE" } }));
				declarationDataObject.CustomsValuationPort = new CodeDescriptionPair() { Code = "SYD" };

				var logger = new TestErrorLogger();
				var provider = new CustomsShipmentDataObjectReaderProvider();
				BusinessObject bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				var declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals("SYD", declarationBO.JE_IATALoadPort);

				declarationDataObject.CustomsValuationPort = null;
				bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals("BNE", declarationBO.JE_IATALoadPort);

				declarationDataObject.CustomsValuationPort = new CodeDescriptionPair() { Code = "SYD" };
				declarationDataObject.SetAddInfoCollection(() => null);
				bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (BaseJobDeclaration)bizObj;
				AssertEquals("SYD", declarationBO.JE_IATALoadPort);
			}
		}

		public void TestJobComInvoiceLineLoadFromAddInfo()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
				shipment.CommercialInfo = new CommercialInfo();
				shipment.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
				var invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
				shipment.CommercialInfo.CommercialInvoiceCollection.Add(invoice);
				invoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
				var invoiceLine = new CommercialInvoiceLine();
				invoice.CommercialInvoiceLineCollection.Add(invoiceLine);
				invoiceLine.AddInfoCollection = new List<UniversalAddInfo>();
				var addInfo = new UniversalAddInfo();
				invoiceLine.AddInfoCollection.Add(addInfo);
				addInfo.Key = "ValueAdjustmentCode";
				addInfo.Value = "A";

				addInfo = new UniversalAddInfo();
				invoiceLine.AddInfoCollection.Add(addInfo);
				addInfo.Key = "MethodOfPayment";
				addInfo.Value = "A";

				addInfo = new UniversalAddInfo();
				invoiceLine.AddInfoCollection.Add(addInfo);
				addInfo.Key = "StatisticalValueManualOverride";
				addInfo.Value = "Y";

				addInfo = new UniversalAddInfo();
				invoiceLine.AddInfoCollection.Add(addInfo);
				addInfo.Key = "StatisticalValue";
				addInfo.Value = "123";

				var reader = new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory);
				var declarationBO = reader.ReadIntoBusinessObject();
				var invoiceLineBO = declarationBO.Invoices[0].JobComInvoiceLines[0];
				CombineAssertions(() =>
				{
					AssertContains("MethodOfPayment=A", "MethodOfPayment=A", invoiceLineBO.JI_AddInfo);
					AssertContains("StatisticalValue=123", "StatisticalValue=123", invoiceLineBO.JI_AddInfo);
					AssertContains("StatisticalValueManualOverride=Y", "StatisticalValueManualOverride=Y", invoiceLineBO.JI_AddInfo);
				});
			}
		}

		public void TestImportTransportModeWhenUsingValidWCONotation()
		{
			AssertTransportModeImportProcess(inputTransportMode: new CodeDescriptionPair() { Code = "1", Description = "Sea" }, expectedOutputJE_TransportMode: "SEA");
		}

		public void TestImportTransportModeWhenUsingInvalidWCONotation()
		{
			AssertTransportModeImportProcess(inputTransportMode: new CodeDescriptionPair() { Code = "0", Description = "0 is not a valid WCO code" }, expectedOutputJE_TransportMode: "");
		}

		public void TestImportTransportModeWhenUsingEmptyTransportCode()
		{
			AssertTransportModeImportProcess(inputTransportMode: new CodeDescriptionPair() { Code = "", Description = "" }, expectedOutputJE_TransportMode: "");
		}

		public void TestImportTransportModeWhenUsingNullTransportMode()
		{
			AssertTransportModeImportProcess(inputTransportMode: null, expectedOutputJE_TransportMode: "");
		}

		public void TestImportTransportModeWhenUsingCargoWiseNotation()
		{
			AssertTransportModeImportProcess(inputTransportMode: new CodeDescriptionPair() { Code = "RAI", Description = "Rail" }, expectedOutputJE_TransportMode: "RAI");
		}

		public void TestClearTransportMode()
		{
			var declarationBOToLoad = Factory.New<BaseJobDeclaration>();
			declarationBOToLoad.JE_MasterBill = "MYMASTER";
			declarationBOToLoad.JE_SystemCreateTimeUtc = ZDateTime.Now.AddMonths(-1);
			declarationBOToLoad.JE_DeclarationReference = "B00001000";
			declarationBOToLoad.JE_MessageType = JobMessageTypeList.Codes.Import;
			declarationBOToLoad.JE_TransportMode = TransportTypeList.Codes.Mail;
			Factory.SaveForTesting();

			AssertTransportModeImportProcess(null, TransportTypeList.Codes.Mail, declarationBOToLoad);
			AssertTransportModeImportProcess(new CodeDescriptionPair() { Code = "", Description = "" }, "", declarationBOToLoad);
		}

		void AssertTransportModeImportProcess(CodeDescriptionPair inputTransportMode, ZString expectedOutputJE_TransportMode, BaseJobDeclaration declarationBOToLoad = null)
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			var universalShipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext,
				TransportMode = inputTransportMode
			};

			if (declarationBOToLoad != null)
			{
				universalShipment.DataContext.DataTargetCollection.First().Key = declarationBOToLoad.JE_DeclarationReference;
			}

			var universalMessage = GetQueuedUniversalShipmentMessage(universalShipment);
			new UniversalMessageProcessingManager(new ServiceTaskLogForTesting()).Process(universalMessage);
			AssertEquals("UXML message status", EDIMessageStatusList.Codes.ProcessedOK, universalMessage.EM_Status);

			var generatedDeclaration = new JobDeclarationDataObjectReader(universalShipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject();
			AssertEquals("UXML generated declaration JE_TransportMode", expectedOutputJE_TransportMode, generatedDeclaration.JE_TransportMode);
		}

		public void TestCreateCusReferenceOnEntryInstructionLevel()
		{
			var mainAddressPK = CustomsReferenceDataObjectReaderTest.CreateTestOrgAddress(Factory);
			Factory.SaveForTesting();
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					WayBillNumber = "MB123",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				};
				shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[] { new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance) }));
				shipment.EntryInstructionCollection[0].SetCustomsReferenceCollection(() => new List<CustomsReference>(new[]
				{
					CustomsReferenceDataObjectReaderTest.CreateCustomsReferenceBOForCusReference("FIS", "FR1", "REF3232", CustomsReferenceDataObjectReaderTest.TestOrganizationAddress)
				}));
				CombineAssertions(() =>
				{
					var message = GetQueuedUniversalShipmentMessage(shipment);
					var serviceTaskLog = new ServiceTaskLogForTesting();
					var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
					messageProcessingManager.Process(message);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					var newFactory = new BusinessObjectFactory();
					var declarationQuery = new ZQuery(JobDeclarationSchema.JE_MasterBill, "MB123");
					var declaration = newFactory.LoadTop1<JobDeclaration>(declarationQuery);
					var entryInstruction = declaration.CustomsEntryInstructions[0];
					var fiscalReferenceQuery = new ZQuery(CusReferenceSchema.CFR_ParentID, entryInstruction.PK);
					fiscalReferenceQuery.AddToFilter(CusReferenceSchema.CFR_ParentTableCode, entryInstruction.TablePrefix);
					fiscalReferenceQuery.AddToFilter(CusReferenceSchema.CFR_Type, CusSupportingInfoTypeList.Codes.FiscalReference);
					fiscalReferenceQuery.AddToFilter(CusReferenceSchema.CFR_Code, "FR1");
					fiscalReferenceQuery.AddToFilter(CusReferenceSchema.CFR_Reference, "REF3232");
					fiscalReferenceQuery.AddToFilter(CusReferenceSchema.CFR_OA_Owner, mainAddressPK);
					AssertEquals("CusReference with given values exists", true, newFactory.Exists(typeof(CusReference), fiscalReferenceQuery));
				});
			}
		}

		public void TestSupportingDocumentSetterSuspender()
		{
			var countryCode = Core.Constants.CountryCodes.UnitedKingdom;
			var eunCountryCode = Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN;
			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;

			using (eAdaptorRegistry.Instance.UseDefaultingOfDataWhenImportingUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				var shipment = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair { Code = JobMessageTypeList.Codes.Import },
					WayBillNumber = "MB123",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					CommercialInfo = new CommercialInfo()
					{
						CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
						{
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
								new DataObjectList<CommercialInvoiceLine>(new []
								{
									new CommercialInvoiceLine()
									{
										AddInfoGroupCollection = new List<AddInfoGroup>(new []
										{
											CreateSupportingDocument("Y057", "REFERENCE", "REASON", 123.45m, ZString.Empty, ZString.Empty, ZString.Empty)
										})
									}
								})))
						})
					}
				};

				var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
				var eun = helper.CreateNewOrGetExistingDataGrouping(eunCountryCode, "European Union");
				helper.CreateNewOrGetExistingDataGrouping(countryCode, "United Kingdom", eun);
				helper.CreateCusCodeListsForMultipleTypesWithAttributes(countryCode, new[] { importCodeType },
					"Y057", "Goods not requiring the presentation of a FLEGT import licence for timber",
					new Dictionary<string, string[]>
					{
						["LEVEL"] = new[] { "ITEM" },
						["StatementText"] = new[] { "Import licence not required" }
					},
					ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				Factory.SaveForTesting();

				var reader = new JobDeclarationDataObjectReader(shipment, new TestErrorLogger(), Factory);
				var declaration = reader.ReadIntoBusinessObject();
				var supportingDocument = declaration.Invoices[0].InvoiceLines[0].SupportingDocuments[0];
				AssertCusSupportingInfoContents(supportingDocument, "Y057", ZString.Empty, ZDate.Empty, "REASON", 123.45m, "REFERENCE", ZString.Empty, ZString.Empty);
			}
		}

		public void TestFillEquipments()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext
				};
				universalShipment.SetTransportEquipmentCollection(GetEquipmentsForTesting);

				var universalMessage = GetQueuedUniversalShipmentMessage(universalShipment);
				new UniversalMessageProcessingManager(new ServiceTaskLogForTesting()).Process(universalMessage);

				var generatedDeclaration = new JobDeclarationDataObjectReader(universalShipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject();

				AssertEquals("Equipments required", true, generatedDeclaration.EquipmentsRequired);
				AssertNotNull(generatedDeclaration.Equipments);
				AssertEquals("Equipments count", 1, generatedDeclaration.Equipments.Count);
				var equipment = generatedDeclaration.Equipments[0];
				Assert("Correct type", equipment is Business.Declaration.CusEquipment cusEquipemt);
				cusEquipemt = equipment;
				AssertEquals("Identification Number", "ABC123", cusEquipemt.CEQ_IdentificationNumber);
				AssertContainsExactElementsInAnyOrder("Seal Numbers", new[] { "99999", "88888" }, cusEquipemt.Seals.Select(s => s.BK_SealNumber).ToArray());
			}
		}

		public void TestFillDV1Details()
		{
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
			dataContext.CodesMappedToTarget = true;
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			var universalShipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				DataContext = dataContext
			};
			universalShipment.SetCustomsValueInformationCollection(GetCustomsValueInformationForTesting);

			var universalMessage = GetQueuedUniversalShipmentMessage(universalShipment);
			new UniversalMessageProcessingManager(new ServiceTaskLogForTesting()).Process(universalMessage);

			var generatedDeclaration = new JobDeclarationDataObjectReader(universalShipment, new TestErrorLogger(), Factory).ReadIntoBusinessObject();
			var dv1Detail = generatedDeclaration.DV1Details.Single();
			Assert("Correct type", dv1Detail is CusDV1Detail cusDv1Detail);
			cusDv1Detail = (CusDV1Detail)dv1Detail;

			CombineAssertions(() =>
			{
				AssertNotNull(cusDv1Detail.PK);
				AssertEquals("LinkID", 1, universalShipment.CustomsValueInformationCollection[0].Link);
				AssertEquals("Sequence of Customs Value Sequence", new ZShort(1), cusDv1Detail.Sequence);
				AssertEquals("Relationship Code", "Y", cusDv1Detail.DV1_Relationship);
				AssertEquals("Price Influence Code", "N", cusDv1Detail.DV1_PriceInfluence);
				AssertEquals("Relation Details", "Relation details", cusDv1Detail.DV1_RelationDetails);
				AssertEquals("Restrictions Code", "Y", cusDv1Detail.DV1_Restrictions);
				AssertEquals("Consideration Code", "N", cusDv1Detail.DV1_Consideration);
				AssertEquals("Restriction Consideration Details", "Restriction Consideration Details", cusDv1Detail.DV1_RestrictionConsiderationDetails);
				AssertEquals("Royalties Licence Code", "Y", cusDv1Detail.DV1_RoyaltiesLicence);
				AssertEquals("Royalties Licence Details", "Royalties Details", cusDv1Detail.DV1_RoyaltiesLicenceDetails);
				AssertEquals("Resale Code", "Y", cusDv1Detail.DV1_Resale);
				AssertEquals("Resale Details", "Resale details", cusDv1Detail.DV1_ResaleDetails);
				AssertEquals("Decision Number", "qwerty", cusDv1Detail.DV1_CustomsDecisionNumber);
			});
		}

		public void TestRemoveUnmatchedExistingEquipment()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var existingDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
				existingDeclaration.JE_MasterBill = "MB123";
				var existingEquipment = existingDeclaration.Equipments.AddNew();
				existingEquipment.CEQ_IdentificationNumber = "ABC123";
				var seal1 = existingEquipment.Seals.AddNew();
				seal1.BK_SealNumber = "99999";
				seal1.BK_SequenceNumber = 1;
				var seal2 = existingEquipment.Seals.AddNew();
				seal2.BK_SealNumber = "88888";
				seal2.BK_SequenceNumber = 2;
				var seal3 = existingEquipment.Seals.AddNew();
				seal3.BK_SealNumber = "55555";
				seal3.BK_SequenceNumber = 3;

				var targetEquipment = existingDeclaration.Equipments.AddNew();
				targetEquipment.CEQ_IdentificationNumber = "ZZZ999";

				Factory.SaveForTesting();

				AssertEquals("Setup of equipments Count (2)", 2, existingDeclaration.Equipments.Count);
				AssertContainsExactElementsInExactOrder("Seal Numbers", new[] { "99999", "88888", "55555" }, existingDeclaration.Equipments[0].Seals.OrderBy(s => s.BK_SequenceNumber).Select(s => s.BK_SealNumber).ToArray());

				var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					WayBillNumber = "MB123",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
				};
				shipment.SetEntryInstructionCollection(() => new List<EntryInstruction>(new[] { new EntryInstruction(DefaultDataObjectWriterStrategy.TestInstance) }));
				shipment.SetTransportEquipmentCollection(GetEquipmentsForTesting);
				CombineAssertions(() =>
				{
					var message = GetQueuedUniversalShipmentMessage(shipment);
					var serviceTaskLog = new ServiceTaskLogForTesting();
					var messageProcessingManager = new UniversalMessageProcessingManager(serviceTaskLog);
					messageProcessingManager.Process(message);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.ProcessedOK, message.EM_Status);

					var newFactory = new BusinessObjectFactory();
					var declarationQuery = new ZQuery(JobDeclarationSchema.JE_MasterBill, "MB123");
					var declaration = newFactory.LoadTop1<JobDeclaration>(declarationQuery);
					AssertEquals("Equipments count", 1, declaration.Equipments.Count);
					var equipment = declaration.Equipments[0];
					Assert("Correct type", equipment is Business.Declaration.CusEquipment cusEquipemt);
					cusEquipemt = equipment;
					AssertEquals("Identification Number", "ABC123", cusEquipemt.CEQ_IdentificationNumber);
					AssertContainsExactElementsInExactOrder("Seal Numbers", new[] { "88888", "99999" }, cusEquipemt.Seals.OrderBy(s => s.BK_SequenceNumber).Select(s => s.BK_SealNumber).ToArray());
				});
			}
		}

		public void TestEntryHeaderDataObjectReaderType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var declarationDataObjectReader = new JobDeclarationDataObjectReaderForTest(new Shipment(), new TestErrorLogger(), Factory);
			AssertType<CustomsEntryHeaderDataObjectReader>(
				"EntryHeader reader type",
				declarationDataObjectReader.CreateCustomsEntryHeaderDataObjectReaderExposed(new EntryHeader(), declaration));
		}

		List<Equipment> GetEquipmentsForTesting()
		{
			var equipments = new List<Equipment>();
			var equipment = new Equipment(DefaultDataObjectWriterStrategy.TestInstance);
			equipment.IdentificationNumber = "ABC123";

			var seals = new List<UniversalCustoms.SealNumber>();
			var equipmentSeal1 = new UniversalCustoms.SealNumber();
			equipmentSeal1.Number = "99999";
			seals.Add(equipmentSeal1);
			var equipmentSeal2 = new UniversalCustoms.SealNumber();
			equipmentSeal2.Number = "88888";
			seals.Add(equipmentSeal2);
			var equipmentSealEmpty = new UniversalCustoms.SealNumber();
			equipmentSealEmpty.Number = ZString.Empty;
			seals.Add(equipmentSealEmpty);

			equipment.SetSealNumberCollection(() => { return seals; });
			equipments.Add(equipment);

			var emptyEquipment = new Equipment(DefaultDataObjectWriterStrategy.TestInstance);
			emptyEquipment.IdentificationNumber = ZString.Empty;
			equipments.Add(emptyEquipment);

			return equipments;
		}

		List<CustomsValueInformation> GetCustomsValueInformationForTesting()
		{
			var customsValueInformationList = new List<CustomsValueInformation>();
			var customsValueInformation = new CustomsValueInformation(DefaultDataObjectWriterStrategy.TestInstance);
			customsValueInformation.Link = 1;

			var details1 = new List<CustomsValueDetail>();

			var detail1 = new CustomsValueDetail { Code = "Y", Type = "Relationship" };
			details1.Add(detail1);

			var detail2 = new CustomsValueDetail { Code = "N", Type = "PriceInfluence" };
			details1.Add(detail2);

			var detail3 = new CustomsValueDetail { Details = "Relation details", Type = "RelationDetails" };
			details1.Add(detail3);

			var detail4 = new CustomsValueDetail { Code = "Y", Type = "Restrictions" };
			details1.Add(detail4);

			var detail5 = new CustomsValueDetail { Code = "N", Type = "Consideration" };
			details1.Add(detail5);

			var detail6 = new CustomsValueDetail { Details = "Restriction Consideration Details", Type = "RestrictionConsiderationDetails" };
			details1.Add(detail6);

			var detail7 = new CustomsValueDetail { Code = "Y", Details = "Royalties Details", Type = "RoyaltiesLicence" };
			details1.Add(detail7);

			var detail8 = new CustomsValueDetail { Code = "Y", Details = "Resale details", Type = "Resale" };
			details1.Add(detail8);

			var detail9 = new CustomsValueDetail { Details = "qwerty", Type = "DecisionNumber" };
			details1.Add(detail9);

			customsValueInformation.SetCustomsValueDetailCollection(() => details1);
			customsValueInformationList.Add(customsValueInformation);

			return customsValueInformationList;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var customsInterface = new LocalCountryCustomsInterface();
			customsInterface.SubmissionType = DeclarationApplicationCodeListForRegistry.Codes.Interfaced;
			localCountryCustomsInterface = CustomsDataRegistry.Instance.LocalCountryCustomsInterface.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, customsInterface);
		}
		IDisposable localCountryCustomsInterface;

		protected override void TearDown()
		{
			base.TearDown();
			localCountryCustomsInterface?.Dispose();
		}

		#region Implementation

		IEDIMessage GetXmlMessage(JobDeclaration declaration, bool ensureCountrySpecificDataExists)
		{
			var factory = declaration.Factory;
			var processedTypes = new LRUCache<Type, PropertyDescriptor[]>();
			var processedBizObjs = new List<BusinessObject>();
			SetCollectionOrder(declaration, processedBizObjs, processedTypes);
			var manager = declaration.GetUniversalDataContextManager() as IShipmentDataContextManager;
			var writer = manager.GetShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, declaration)));
			using (((IExternalFetchHintSupporter)factory).SetupCreator())
			{
				var shipment = (Shipment)writer.GetDataObject(declaration);

				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				shipment.DataContext = dataContext;
				shipment.LocationAtClearance.Description = null;
				if (ensureCountrySpecificDataExists)
				{
					var customsSupportingInformationCollection = shipment.CustomsSupportingInformationCollection;
					AssertNotNull("AdditionalInfo", customsSupportingInformationCollection.FirstOrDefault(x => x.Category.GetCodeAsUpperCase() == Common.EU.CusSupportingInfoTypeList.Codes.AdditionalInfo));
					AssertNotNull("PreviousDocument", customsSupportingInformationCollection.FirstOrDefault(x => x.Category.GetCodeAsUpperCase() == Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument));
					AssertNotNull("SupportingDocument", customsSupportingInformationCollection.FirstOrDefault(x => x.Category.GetCodeAsUpperCase() == Common.EU.CusSupportingInfoTypeList.Codes.SupportingDocument));
					var invoiceLineOrganisations = shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].OrganizationAddressCollection;
					AssertNotNull("invoice line should have CustomsSupervisingOffice", invoiceLineOrganisations.FirstOrDefault(x => x.AddressType.Value == nameof(DocAddressType.CustomsSupervisingOffice)));
				}
				return GetQueuedUniversalShipmentMessage(shipment);
			}
		}

		static void SetCollectionOrder(BusinessObject bizObj, List<BusinessObject> processedBizObjs, LRUCache<Type, PropertyDescriptor[]> processedTypes)
		{
			if (bizObj != null && !processedBizObjs.Contains(bizObj))
			{
				processedBizObjs.Add(bizObj);
				foreach (PropertyDescriptor property in GetCollectionProperties(processedTypes, bizObj))
				{
					var data = property.GetValue(bizObj);
					var collection = data as IBusinessObjectCollection;
					if (collection != null && collection.Count > 0)
					{
						var childBizObj = (BusinessObject)collection[0];
						var properties = ((ICustomTypeDescriptor)childBizObj).GetProperties();
						properties.Sort();
						var propertToSortBy = properties.OfType<PropertyDescriptor>().FirstOrDefault(x => typeof(IComparable).IsAssignableFrom(x.PropertyType));
						if (propertToSortBy != null)
						{
							collection.ApplySort(new SortInfo(propertToSortBy.Name, ListSortDirection.Ascending));
						}
						SetCollectionOrder(childBizObj, processedBizObjs, processedTypes);
					}
				}
			}
		}

		static ZString SetCustomisableStringValue(string propertyName, int maxLength, ZString value)
		{
			if (propertyName == nameof(JobDeclaration.JE_TransportMode))
			{
				return Core.Constants.TransportModes.Sea;
			}
			if (propertyName == nameof(Freight.Business.Transport.JW_TransportMode))
			{
				return Core.Constants.TransportModes.Sea;
			}
			if (propertyName.Contains("_RX_"))
			{
				return GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;
			}
			if (propertyName.EndsWith("_Tariff"))
			{
				return "10203040";
			}
			if (propertyName == Bill.Schema.CU_Status || propertyName == BaseJobComInvHeaderCharge.Schema.J7_ExchangeRateType || propertyName == BaseJobComInvHeaderCharge.Schema.J7_ChargeDescription || propertyName.EndsWith("_MessageStatus") || propertyName == StmEntityScreeningLogSchema.Constants.PJ_SourceTableCode)
			{
				return ZString.Empty;
			}
			return value.ToUpper();
		}

		static void ClearInvalidAndNotApplicableData(JobDeclaration declaration)
		{
			declaration.JE_ApplicationCode = ZString.Empty;
			declaration.ZG_BorderTransportMeans = ZString.Empty;
			declaration.JE_ConsolidatedCargoStatus = ZString.Empty;
			declaration.JE_EntryAuthorisationDate = ZDateTime.Empty;
			declaration.JE_EntryDate = ZDate.Empty;
			declaration.JE_EntrySubmittedDate = ZDateTime.Empty;
			declaration.JE_WarehouseReleaseDate = ZDateTime.Empty;
			declaration.PrimaryHouseBill.CU_MessageStatus = ZString.Empty;
			declaration.JE_RL_NKPortOfFirstArrival = ZString.Empty;
			foreach (var entryNumberBO in declaration.Factory.Load<CusEntryNumber>(new ZQuery(CusEntryNumSchema.CE_ParentID, declaration.PK)))
			{
				if (entryNumberBO.CE_EntryType == CusEntryNumberTypes.EU.MasterUCR)
				{
					entryNumberBO.CE_EntryIsSystemGenerated = false;
				}
				else
				{
					entryNumberBO.Delete();
				}
			}
			declaration.PackingGroups.OfType<BasePackingGroup>().Where(x => x.CR_CU_HouseBill.IsEmpty).DeleteAll();
			declaration.Packages.OfType<BasePackage>().Where(x => x.CW_CR_HouseContainer.IsEmpty).DeleteAll();
			declaration.InvoiceLines.OfType<BaseJobComInvoiceLine>().SelectMany(x => x.PackagesPivot.OfType<InvoiceLinePackagePivot>().Where(y => y.CHC_CW.IsEmpty)).DeleteAll();
			declaration.InvoiceLines.OfType<BaseJobComInvoiceLine>().ForEach(c => c.JI_ClassUsageComment = ZString.Empty);
			declaration.Factory.Load<InvoiceHeaderPackagePivot>(new ZQuery()).DeleteAll();

			foreach (CusEntryHeader entryHeader in declaration.CustomsEntryHeaders)
			{
				entryHeader.CH_CustomsMessageRemarks = string.Empty;
			}
			declaration.JE_PaidBy = ZString.Empty;
		}

		static IEnumerable<PropertyDescriptor> GetCollectionProperties(LRUCache<Type, PropertyDescriptor[]> processedTypes, ICustomTypeDescriptor bizObj)
		{
			var type = bizObj.GetType();
			PropertyDescriptor[] result = processedTypes[type];
			if (result == null)
			{
				var list = new List<PropertyDescriptor>();
				foreach (PropertyDescriptor property in bizObj.GetProperties())
				{
					if ((typeof(IBusiness).IsAssignableFrom(property.PropertyType) || typeof(ICollection).IsAssignableFrom(property.PropertyType)) &&
						ChildEditableAttribute.GetValue(property))
					{
						list.Add(property);
					}
				}
				result = list.Distinct().OrderBy(x => x.Name).ToArray();
				processedTypes.Add(type, result);
			}
			return result;
		}

		static bool CreateChildObject(BusinessObject bizObj, List<BusinessObject> processedBizObjs, LRUCache<Type, PropertyDescriptor[]> processedTypes)
		{
			if (bizObj != null && !processedBizObjs.Contains(bizObj))
			{
				var bizObjType = bizObj.GetType();
				if (!NotSupportedType(bizObjType) && !(typeof(Bill).IsAssignableFrom(bizObjType) && ((Bill)bizObj).CU_BillType == BillTypeList.Codes.MasterBill))
				{
					processedBizObjs.Add(bizObj);
					if (!typeof(CusSupportingInfo).IsAssignableFrom(bizObjType) && !typeof(CusAddInfo).IsAssignableFrom(bizObjType) && !typeof(CusCodeData).IsAssignableFrom(bizObjType))
					{
						bizObj.FillWithValidTestData(TestBusinessObjectKind.PopulateDates | TestBusinessObjectKind.PopulateNumbers | TestBusinessObjectKind.PopulateStrings, Array.Empty<PropertyDescriptor>());
						if (bizObj is JobComInvCharge charge)
						{
							charge.J7_ExchangeRate = 1m;
						}
						if (bizObj is Customs.Business.CusEntryLineFee fee)
						{
							fee.CF_Source = "CW1";
						}

						if (bizObj is Customs.Business.CusVehicle vehicle)
						{
							vehicle.CVH_DriveSide = "L";
							vehicle.CVH_ModelYear = "2000";
							vehicle.CVH_Transmission = "A";
							vehicle.CVH_EngineCapacityUQ = "CC";
							vehicle.CVH_SupplyMethod = "CKD";
							vehicle.CVH_SpecificationStandard = "1";
						}
					}
					else if (bizObjType == typeof(AdditionalInfo))
					{
						((AdditionalInfo)bizObj).CSI_Code = "AI01";
					}
					else if (bizObjType == typeof(SupportingDocument))
					{
						((SupportingDocument)bizObj).CSI_Code = "N123";
					}
					else if (bizObjType == typeof(PreviousDocument))
					{
						((PreviousDocument)bizObj).CSI_Code = "380";
					}
					foreach (PropertyDescriptor property in GetCollectionProperties(processedTypes, bizObj))
					{
						var data = property.GetValue(bizObj);
						if (data is IBusinessObjectCollection collection)
						{
							if (!NotSupportedType(collection.TypeOfElements))
							{
								var childBizObj = collection.Count > 0 ? (BusinessObject)collection[0] : collection.AddNew();
								if (CreateChildObject(childBizObj, processedBizObjs, processedTypes))
								{
									try
									{
										collection.Add(childBizObj);
									}
									catch (InvalidOperationException)
									{
									}
								}
							}
						}
						else
						{
							var childBizObj = data as BusinessObject;
							if (childBizObj != null)
							{
								CreateChildObject(childBizObj, processedBizObjs, processedTypes);
							}
						}
					}
					return true;
				}
			}
			return false;
		}

		static bool NotSupportedType(Type type)
		{
			return typeof(ProcessTask).IsAssignableFrom(type)
				|| typeof(AdditionalInvoiceLineEntryLineLink).IsAssignableFrom(type)
				|| typeof(CusEntryHeaderCharges).IsAssignableFrom(type)
				|| typeof(EDIMessage).IsAssignableFrom(type)
				|| typeof(Order).IsAssignableFrom(type)
				|| typeof(JobComInvLineComponentInventory).IsAssignableFrom(type)
				|| typeof(CusPackableItem).IsAssignableFrom(type)
				|| typeof(CusContainerEntryHeaderPivot).IsAssignableFrom(type)
				|| typeof(CusContainerEntryInstructionPivot).IsAssignableFrom(type)
				|| typeof(CusEntrySnapshot).IsAssignableFrom(type)
				|| typeof(GlbCompanyCampaignItem).IsAssignableFrom(type)
				|| typeof(JobService).IsAssignableFrom(type)
				|| typeof(IProcessHeader).IsAssignableFrom(type)
				|| typeof(DeniedPartyScreening.Integration.IStmEntityScreeningLog).IsAssignableFrom(type)
				|| typeof(OwnerOfGoods).IsAssignableFrom(type)
				|| typeof(PlaceOfUseOrProcessing).IsAssignableFrom(type)
				;
		}

		#endregion

		sealed class JobDeclarationForTesting : JobDeclaration, IBusinessObjectTestDataHelperPropertiesToExclude
		{
			public JobDeclarationForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			bool IBusinessObjectTestDataHelperPropertiesToExclude.ShouldExcludeFromFillWithValidTestData(ZString name) => name == "WarehouseTransactionStatus";
		}

		class JobDeclarationDataObjectReaderForTest : JobDeclarationDataObjectReader
		{
			public JobDeclarationDataObjectReaderForTest(Shipment declarationDataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
				: base(declarationDataObject, logger, factory, forwardingShipment: null)
			{
			}

			public Customs.DataTransfer.Universal.CustomsEntryHeaderDataObjectReader CreateCustomsEntryHeaderDataObjectReaderExposed(EntryHeader entryHeaderDataObject, JobDeclaration declaration, List<ZString> matchingKeys = null)
			{
				return CreateCustomsEntryHeaderDataObjectReader(entryHeaderDataObject, declaration, matchingKeys);
			}
		}
	}
}
