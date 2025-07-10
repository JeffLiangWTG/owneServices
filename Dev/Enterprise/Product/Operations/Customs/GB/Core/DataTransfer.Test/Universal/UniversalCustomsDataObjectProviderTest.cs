using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.ZArchitecture.Schema;
using UniversalTest = Enterprise.Customs.EU.DataTransfer.Universal.Testing.JobDeclarationDataObjectReaderTest;
using YesNoList = Enterprise.Customs.Universal.CodeDescriptionPairLists.YesNoList;

namespace Enterprise.Customs.GB.DataTransfer.Universal.Testing
{
	partial class UniversalCustomsDataObjectProviderTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetNewAirManifestDataObjectReaders()
		{
			AssertEquals("There should be no data readers for GB Air Manifests", Enumerable.Empty<ITopLevelDataObjectReader>(), new UniversalCustomsDataObjectProvider().GetNewAirManifestDataObjectReaders(null, null, null, null, false));
		}

		public void TestAdditionalSupplementaryInfoIsIncluded()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				invoiceLine1.JI_SupplementaryCode1 = "ABC";
				invoiceLine1.JI_SupplementaryCode2 = "DEF";
				Factory.SaveForTesting();
				var newFactory = new BusinessObjectFactory();
				declaration = newFactory.Load<JobDeclaration>(declaration.PK);

				var dataObject = (Shipment)UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

				AssertNotNull("dataObject.CommercialInfo.CommercialInvoiceCollection", dataObject.CommercialInfo.CommercialInvoiceCollection);
				AssertNotNull("dataObject.CommercialInfo.CommercialInvoiceCollection.CommercialInvoiceLineCollection", dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection);
				AssertEquals("dataObject.CommercialInfo.CommercialInvoiceCollection.CommercialInvoiceLineCollection.CustomsReferenceCollection", 2, dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsReferenceCollection.Count);
				AssertEquals("SubType.Code 1", "ABC", dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsReferenceCollection[0].SubType.Code);
				AssertEquals("SubType.Code 2", "DEF", dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsReferenceCollection[1].SubType.Code);
			}
		}

		public void TestTableSpecificCusReferenceTypeList()
		{
			var result = (ZArchitecture.Core.CodeDescriptionPairList)new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusEntryInstructionSchema.Constants.Prefix, string.Empty);
			AssertContains(CusReferenceTypeList.Codes.FiscalReference, result.CodesAsString);
			result = (ZArchitecture.Core.CodeDescriptionPairList)new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(JobComInvoiceLineSchema.Constants.Prefix, string.Empty);
			AssertContains(CusReferenceTypeList.Codes.FiscalReference, result.CodesAsString);
			result = (ZArchitecture.Core.CodeDescriptionPairList)new UniversalCustomsDataObjectProvider().TableSpecificCusReferenceTypeList(CusInBondCargoDescSchema.Constants.Prefix, string.Empty);
			AssertContains(Customs.Business.CusReferenceTypeList.Codes.SupplyChainActor, result.CodesAsString);
		}

		public void TestAdditionalProcedureInfoIsIncluded()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice.InvoiceLines.AddNew();
				var apCode1 = invoiceLine1.AdditionalProcedureCodes.AddNew();
				apCode1.CY_Code = "A";
				var apCode2 = invoiceLine1.AdditionalProcedureCodes.AddNew();
				apCode2.CY_Code = "B";
				Factory.SaveForTesting();
				var newFactory = new BusinessObjectFactory();
				declaration = newFactory.Load<JobDeclaration>(declaration.PK);

				var dataObject = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);

				AssertNotNull("dataObject.CommercialInfo.CommercialInvoiceCollection", dataObject.CommercialInfo.CommercialInvoiceCollection);
				AssertNotNull("dataObject.CommercialInfo.CommercialInvoiceCollection.CommercialInvoiceLineCollection", dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection);
				AssertEquals("dataObject.CommercialInfo.CommercialInvoiceCollection.CommercialInvoiceLineCollection.CustomsReferenceCollection", 2, dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsReferenceCollection.Count);
				AssertEquals("SubType.Code 1", "A", dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsReferenceCollection[0].SubType.Code);
				AssertEquals("SubType.Code 2", "B", dataObject.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection[0].CustomsReferenceCollection[1].SubType.Code);
			}
		}

		public void TestImportAndExportOldSupportingInformationSchema()
		{
			using (EU.Registry.EUCustomsDataRegistry.Instance.ExportOldSuportingInfoSchemaInUniversalXML.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
				var shipment = new Shipment()
				{
					DataContext = dataContext,
					WayBillNumber = "MB123",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					CommercialInfo = new CommercialInfo()
					{
						AddInfoGroupCollection = new List<AddInfoGroup>(new[]
						{
							UniversalTest.CreateAdditionalInfo("DC1", "DEC ADD DESC 1", Core.Constants.CountryCodes.Afghanistan, YesNoList.Codes.Yes),
							UniversalTest.CreatePreviousDocument("DC2", new ZDate(2017, 1, 1), "DEC MORE INFO 2", "DECP2", PreviousDocumentClassList.Codes.PreviousAdministrativeReferenceNCTS),
							UniversalTest.CreateSupportingDocument("DC3", "DECS3", "DEC BECAUSE", 10.32m, "PARD1", "A", "U")
						}),
						CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>(new[]
						{
							new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
							{
								AddInfoGroupCollection = new List<AddInfoGroup>(new []
								{
									UniversalTest.CreateAdditionalInfo("IC1", "INVOICE ADD DESC 1", Core.Constants.CountryCodes.Bahamas, YesNoList.Codes.No),
									UniversalTest.CreatePreviousDocument("IC2", new ZDate(2017, 2, 1), "INV MORE INFO 2", "INVP2", PreviousDocumentClassList.Codes.SummaryDeclaration),
									UniversalTest.CreateSupportingDocument("IC3", "INVS3", "INV BECAUSE", 34.23m, "PARI1", "C", "H")
								}),
							}.AdditionalSetup(x => x.SetCommercialInvoiceLineCollection(() =>
								new DataObjectList<CommercialInvoiceLine>(new []
								{
									new CommercialInvoiceLine()
									{
										AddInfoGroupCollection = new List<AddInfoGroup>(new []
										{
											UniversalTest.CreateAdditionalInfo("LC1", "INVOICE LINE ADD DESC 1", Core.Constants.CountryCodes.Cambodia, YesNoList.Codes.Yes),
											UniversalTest.CreatePreviousDocument("LC2", new ZDate(2017, 3, 1), "INVLINE MORE INFO 2", "INVLINEP2", PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures),
											UniversalTest.CreateSupportingDocument("LC3", "INVLINES3", "INVLINE BECAUSE", 333.34m, "PAR1","E", "F")
										})
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

				CombineAssertions(delegate
				{
					var newFactory = new BusinessObjectFactory();
					var query = new ZQuery(JobDeclarationSchema.JE_MasterBill, "MB123");
					var declarations = newFactory.Load<JobDeclaration>(query);
					AssertEquals(1, declarations.Length);
					var declaration = declarations[0];
					AssertEquals("declaration.AdditionalInfos.Count", 1, declaration.AdditionalInfos.Count);
					UniversalTest.AssertCusSupportingInfoContents(declaration.AdditionalInfos[0], "DC1", Core.Constants.CountryCodes.Afghanistan, ZDate.Empty, "DEC ADD DESC 1", ZDecimal.Zero, ZString.Empty, AdditionalInfoIssuerList.Codes.Customs, YesNoList.Codes.Yes);
					AssertEquals("declaration.PreviousDocuments.Count", 1, declaration.PreviousDocuments.Count);
					UniversalTest.AssertCusSupportingInfoContents(declaration.PreviousDocuments[0], "DC2", ZString.Empty, new ZDate(2017, 1, 1), "DEC MORE INFO 2", ZDecimal.Zero, "DECP2", ZString.Empty, PreviousDocumentClassList.Codes.PreviousAdministrativeReferenceNCTS);
					AssertEquals("declaration.SupportingDocuments.Count", 1, declaration.SupportingDocuments.Count);
					UniversalTest.AssertCusSupportingInfoContents(declaration.SupportingDocuments[0], "DC3", ZString.Empty, ZDate.Empty, "DEC BECAUSE", 10.32m, "DECS3", "UA", "PARD1");

					AssertEquals("declaration.Invoices.Count", 1, declaration.Invoices.Count);
					var invoice = declaration.Invoices[0];
					AssertEquals("invoice.AdditionalInfos.Count", 1, invoice.AdditionalInfos.Count);
					UniversalTest.AssertCusSupportingInfoContents(invoice.AdditionalInfos[0], "IC1", Core.Constants.CountryCodes.Bahamas, ZDate.Empty, "INVOICE ADD DESC 1", ZDecimal.Zero, ZString.Empty, AdditionalInfoIssuerList.Codes.Customs, YesNoList.Codes.No);
					AssertEquals("invoice.PreviousDocuments.Count", 1, invoice.PreviousDocuments.Count);
					UniversalTest.AssertCusSupportingInfoContents(invoice.PreviousDocuments[0], "IC2", ZString.Empty, new ZDate(2017, 2, 1), "INV MORE INFO 2", ZDecimal.Zero, "INVP2", ZString.Empty, PreviousDocumentClassList.Codes.SummaryDeclaration);
					AssertEquals("invoice.SupportingDocuments.Count", 1, invoice.SupportingDocuments.Count);
					UniversalTest.AssertCusSupportingInfoContents(invoice.SupportingDocuments[0], "IC3", ZString.Empty, ZDate.Empty, "INV BECAUSE", 34.23m, "INVS3", "HC", "PARI1");

					AssertEquals("declaration.InvoiceLines.Count", 1, declaration.InvoiceLines.Count);
					var invoiceLine = declaration.InvoiceLines[0];
					AssertEquals("invoiceLine.AdditionalInfos.Count", 1, invoiceLine.AdditionalInfos.Count);
					UniversalTest.AssertCusSupportingInfoContents(invoiceLine.AdditionalInfos[0], "LC1", Core.Constants.CountryCodes.Cambodia, ZDate.Empty, "INVOICE LINE ADD DESC 1", ZDecimal.Zero, ZString.Empty, AdditionalInfoIssuerList.Codes.Customs, YesNoList.Codes.Yes);
					AssertEquals("invoiceLine.PreviousDocuments.Count", 1, invoiceLine.PreviousDocuments.Count);
					UniversalTest.AssertCusSupportingInfoContents(invoiceLine.PreviousDocuments[0], "LC2", ZString.Empty, new ZDate(2017, 3, 1), "INVLINE MORE INFO 2", ZDecimal.Zero, "INVLINEP2", ZString.Empty, PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures);
					AssertEquals("invoiceLine.SupportingDocuments.Count", 1, invoiceLine.SupportingDocuments.Count);
					UniversalTest.AssertCusSupportingInfoContents(invoiceLine.SupportingDocuments[0], "LC3", ZString.Empty, ZDate.Empty, "INVLINE BECAUSE", 333.34m, "INVLINES3", "FE", "PAR1");

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
						UniversalTest.AssertOldAdditionalInfo(exportShipment.CommercialInfo.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Type), CusSupportingInfoTypeList.Descriptions.AdditionalInfo, "DC4", "DEC ADD DESC 1", Core.Constants.CountryCodes.Afghanistan, YesNoList.Codes.Yes);
						UniversalTest.AssertOldPreviousDocument(exportShipment.CommercialInfo.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Type), CusSupportingInfoTypeList.Descriptions.PreviousDocument, "DC5", new ZDate(2017, 1, 1), "DEC MORE INFO 2", "DECP2", PreviousDocumentClassList.Codes.PreviousAdministrativeReferenceNCTS);
						UniversalTest.AssertOldSupportingDocument(exportShipment.CommercialInfo.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Type), CusSupportingInfoTypeList.Descriptions.SupportingDocument, "DC6", "DECS3", "DEC BECAUSE", 10.32m, "PARD1", "A", "U");
						var invoiceData = exportShipment.CommercialInfo.CommercialInvoiceCollection[0];
						UniversalTest.AssertOldAdditionalInfo(invoiceData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Type), CusSupportingInfoTypeList.Descriptions.AdditionalInfo, "IC4", "INVOICE ADD DESC 1", Core.Constants.CountryCodes.Bahamas, "");
						UniversalTest.AssertOldPreviousDocument(invoiceData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Type), CusSupportingInfoTypeList.Descriptions.PreviousDocument, "IC5", new ZDate(2017, 2, 1), "INV MORE INFO 2", "INVP2", PreviousDocumentClassList.Codes.SummaryDeclaration);
						UniversalTest.AssertOldSupportingDocument(invoiceData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Type), CusSupportingInfoTypeList.Descriptions.SupportingDocument, "IC6", "INVS3", "INV BECAUSE", 34.23m, "PARI1", "C", "H");
						var invoiceLineData = invoiceData.CommercialInvoiceLineCollection[0];
						UniversalTest.AssertOldAdditionalInfo(invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.AdditionalInfo.Type), CusSupportingInfoTypeList.Descriptions.AdditionalInfo, "LC4", "INVOICE LINE ADD DESC 1", Core.Constants.CountryCodes.Cambodia, YesNoList.Codes.Yes);
						UniversalTest.AssertOldPreviousDocument(invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.PreviousDocument.Type), CusSupportingInfoTypeList.Descriptions.PreviousDocument, "LC5", new ZDate(2017, 3, 1), "INVLINE MORE INFO 2", "INVLINEP2", PreviousDocumentClassList.Codes.InitialDeclarationOfGoodsUnderSimplifiedProcedures);
						UniversalTest.AssertOldSupportingDocument(invoiceLineData.AddInfoGroupCollection.FirstOrDefault(x => x.Type.GetCodeAsUpperCase() == EU.DataTransfer.Universal.Constants.OldCusAddInfoTypes.SupportingDocument.Type), CusSupportingInfoTypeList.Descriptions.SupportingDocument, "LC6", "INVLINES3", "INVLINE BECAUSE", 333.34m, "PAR1", "E", "F");
					}
				});
			}
		}

		public void TestTableSpecificCusCodeDataTypeList()
		{
			var provider = new UniversalCustomsDataObjectProvider();

			var cusCodeDataTypeList = provider.TableSpecificCusCodeDataTypeList(JobDeclarationSchema.Constants.Prefix, string.Empty);
			AssertEquals(1, cusCodeDataTypeList.Count);
			AssertEquals(true, cusCodeDataTypeList.ContainsCode(EU.Business.CusCodeDataTypeList.Codes.OfficeCode));

			cusCodeDataTypeList = provider.TableSpecificCusCodeDataTypeList(JobComInvoiceLineSchema.Constants.Prefix, string.Empty);
			AssertEquals(2, cusCodeDataTypeList.Count);
			AssertEquals(true, cusCodeDataTypeList.ContainsCode(EU.Business.CusCodeDataTypeList.Codes.SupplementaryCode));
			AssertEquals(true, cusCodeDataTypeList.ContainsCode(EU.Business.CusCodeDataTypeList.Codes.AdditionalProcedureCode));

			cusCodeDataTypeList = provider.TableSpecificCusCodeDataTypeList(AsycudaManifestHeaderSchema.Constants.Prefix, string.Empty);
			AssertEquals(1, cusCodeDataTypeList.Count);
			AssertEquals(true, cusCodeDataTypeList.ContainsCode(EU.Business.CusCodeDataTypeList.Codes.OfficeCode));
		}

		public void TestTableSpecificCusAddInfoTypeListEndToEnd()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_GBRouteOfEntry = "123";
			var guarantee = Factory.New<EU.Business.Declaration.GuaranteeForDeclaration>();
			guarantee.PW_ParentID = declaration.PK;
			guarantee.PW_ParentTableCode = declaration.TablePrefix;
			guarantee.PW_BondType = Customs.EU.Business.CodeDescriptionPairLists.EUNctsGuaranteeTypeList.Codes.CashDepositGuarantee;
			Factory.SaveForTesting();
			var newFactory = new BusinessObjectFactory();
			declaration = newFactory.Load<JobDeclaration>(declaration.PK);
			var dataObject = (Shipment)Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWriter.GetDataObject(RecipientRoleType.ORP, declaration);
			AssertEquals("dataObject.AddInfoGroupCollection.Count", 1, dataObject.AddInfoGroupCollection.Count);
			var addInfoGroup = dataObject.AddInfoGroupCollection[0];
			AssertEquals("addInfoGroup.Type.Code", Enterprise.Customs.Business.MultiLineAddInfos.CusAddInfoTypeAttribute.Codes.GBAllSimpleProperties, addInfoGroup.Type.GetCodeAsUpperCase());
			AssertEquals("addInfoGroup.Type.Description", "All Simple Properties", addInfoGroup.Type.Description.GetValueOrDefault());
		}

		public void TestImportGoodsLocationForCDS()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				var dataContext = DataContextFactory.New();
				dataContext.SetCompanyAndDataProviderDetails(Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
				dataContext.CodesMappedToTarget = true;
				dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);

				var declarationDataObject = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
				{
					DataContext = dataContext,
					MessageType = new CodeDescriptionPair() { Code = JobMessageTypeList.Codes.Import },
					WayBillNumber = "MYMASTER",
					WayBillType = new WayBillType() { Code = WayBillTypeList.Codes.Master },
					MessagingApplicationCode = new CodeDescriptionPair() { Code = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services },
					LocationAtClearance = new CodeDescriptionPair35Char() { Code = "GBBUCWTESTPLACE0" }
				};
				var logger = new TestErrorLogger();
				var provider = new Customs.DataTransfer.Universal.CustomsShipmentDataObjectReaderProvider();
				BusinessObject bizObj = null;
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				var declarationBO = (JobDeclaration)bizObj;
				AssertEquals("GB", declarationBO.JE_Calc_LocationOtherInformationCountry);
				AssertEquals("BU", declarationBO.JE_Calc_LocationOtherInformationType);
				AssertEquals("CW", declarationBO.JE_LocationQualifier);
				AssertEquals("TESTPLACE0", declarationBO.JE_GoodsLocation);

				declarationBO.JE_GoodsLocation = ZString.Empty;
				declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "  BUCWTESTPLACE0" };
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (JobDeclaration)bizObj;
				AssertEquals("", declarationBO.JE_Calc_LocationOtherInformationCountry);
				AssertEquals("BU", declarationBO.JE_Calc_LocationOtherInformationType);
				AssertEquals("CW", declarationBO.JE_LocationQualifier);
				AssertEquals("TESTPLACE0", declarationBO.JE_GoodsLocation);

				declarationBO.JE_GoodsLocation = ZString.Empty;
				declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "    CWTESTPLACE0" };
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (JobDeclaration)bizObj;
				AssertEquals("", declarationBO.JE_Calc_LocationOtherInformationCountry);
				AssertEquals("", declarationBO.JE_Calc_LocationOtherInformationType);
				AssertEquals("CW", declarationBO.JE_LocationQualifier);
				AssertEquals("TESTPLACE0", declarationBO.JE_GoodsLocation);

				declarationBO.JE_GoodsLocation = ZString.Empty;
				declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "      TESTPLACE0" };
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (JobDeclaration)bizObj;
				AssertEquals(ZString.Empty, declarationBO.JE_Calc_LocationOtherInformationCountry);
				AssertEquals(ZString.Empty, declarationBO.JE_Calc_LocationOtherInformationType);
				AssertEquals(ZString.Empty, declarationBO.JE_LocationQualifier);
				AssertEquals("TESTPLACE0", declarationBO.JE_GoodsLocation);

				declarationBO.JE_GoodsLocation = ZString.Empty;
				declarationDataObject.LocationAtClearance = new CodeDescriptionPair35Char() { Code = "GBBUCW" };
				provider.GetReader(declarationDataObject, logger, Factory, null).ReadIntoBusinessObject(ref bizObj);
				declarationBO = (JobDeclaration)bizObj;
				AssertEquals("GB", declarationBO.JE_Calc_LocationOtherInformationCountry);
				AssertEquals("BU", declarationBO.JE_Calc_LocationOtherInformationType);
				AssertEquals("CW", declarationBO.JE_LocationQualifier);
				AssertEquals(ZString.Empty, declarationBO.JE_GoodsLocation);
			}
		}
	}
}
