using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineLineDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestQuarantineFields()
		{
			var yesterday = new ZDateTime(2020, 1, 1);
			var yesterdayString = yesterday.ToISO8601String();
			var today = new ZDateTime(2020, 1, 2);
			var todayString = today.ToISO8601String();
			var tomorrow = new ZDateTime(2020, 1, 3);
			var tomorrowString = tomorrow.ToISO8601String();

			var shipment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance)
			{
				MessageType = new CodeDescriptionPair() { Code = Common.AU.AUJobMessageTypeList.Codes.Quarantine, Description = Common.AU.AUJobMessageTypeList.Descriptions.Quarantine }
			};
			var dataContext = DataContextFactory.New();
			dataContext.SetCompanyAndDataProviderDetails(GlbCompany.CurrentCompany);
			dataContext.AddDataTarget(DataContextType.CustomsDeclaration, null);
			shipment.DataContext = dataContext;
			shipment.CommercialInfo = new CommercialInfo
			{
				CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>()
			};
			var invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance)
			{
				AddInfoGroupCollection = new List<AddInfoGroup>(),
				OrganizationAddressCollection = new List<OrganizationAddress>()
				{
					new OrganizationAddress()
					{
						AddressType = "AQISLoadingEstablishment",
						AddressOverride = true,
						AdditionalAddressInformation = "ESN"
					}
				}
			};

			var addInfoGroupQH = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Constants.InvoiceHeader.Codes.QH, Description = Constants.InvoiceHeader.Descriptions.QH },
				AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ProduceType, Value = "DAI" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.LastAmendDateTime, Value = today.ToLocalBranchTimeOffset().SqlFormat },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ProductUse, Value = "H" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ObtainExportCustomsPermit, Value = "Y" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ConsigneeAgentName, Value = "CONSIGNEE REPRESENTATIVE 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ExporterDeclaration, Value = "EXPORTER DECLARATION 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.CertificatePrintIndicator, Value = "A" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ProductionRegion, Value = "CAB" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.SplitHealthCertByContainer, Value = "Y" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.SplitHealthCertByPacker, Value = "Y" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.SplitHealthCertByMarks, Value = "Y" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.AMLCQuota, Value = "Y" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.QuotaType, Value = "ABC" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ShipsStores, Value = "Y" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.AMLCQuotaYear, Value = "2020" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.CertificateRequiredLocation, Value = "PRINT LOCATION 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ProductSource, Value = "AU" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.BorderInspectionPort, Value = "ADALV" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.PackDate, Value = yesterdayString },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.AbsoluteTemperature, Value = "20" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.MinimumTemperature, Value = "10" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.MaximumTemperature, Value = "30" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.TemperatureUnit, Value = "CEL" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.CustomsConsigneeName, Value = "CUSTOMS CONSIGNEE 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ExemptionCode, Value = "EXEMPTION CODE 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.PrintLocation, Value = "CODE" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.DeclarationOfCompliance, Value = "YES" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ImportedProductFlag, Value = "YES" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.TrueAndComplete, Value = "YES" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ManufacturedTreatedPackagedLabelledInAustralia, Value = "YES" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.LegallyImportedFlag, Value = "YES" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.AuthorisationEstablishment, Value = "ABC" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.AuthorisationFlag, Value = "Y" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.StorageEstablishment, Value = "DEF" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ApprovedCertifier, Value = "H0001" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.LotNumber, Value = "LOT NUMBER 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.CatchZone, Value = "CATCH ZONE 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.AverageAnimalAge, Value = "both age ranges" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.StartHoldSeal, Value = "START 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.EndHoldSeal, Value = "END 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.InspectionRequestedDate, Value = todayString },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.AuthorisedStartDate, Value = todayString },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.AuthorisedEndDate, Value = todayString },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.AuthorisingOfficerID, Value = "123456" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.InspectorComments, Value = "INSPECTOR COMMENTS 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.AuthorisationDate, Value = tomorrowString },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.AuthorisationComments, Value = "COMMENTS 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ForwardeeEDIUserIdentifier, Value = "FORWARD 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ForwardStatus, Value = "COMP" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.TransfereeEDIUserIdentifier, Value = "EDI 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.TransfereeExporterNumber, Value = "EXPORTER 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.CancelTransferIndicator, Value = "Y" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.ApprovalNumber, Value = "APPROVAL 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.TransitLocationType, Value = "C" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.EUComments, Value = "COMMENT 1" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.EUTestResultRequired, Value = "Y" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.LoadingEstablishment, Value = "92" },
					new AddInfo() { Key = Constants.InvoiceHeader.Keys.LoadingDate, Value = "2024-08-21" },
				},
				AddInfoGroupCollection = new List<AddInfoGroup>
				{
					new AddInfoGroup
					{
						Type = new CodeDescriptionPair { Code = Constants.InvoiceHeader.Codes.NRL, Description = Constants.InvoiceHeader.Descriptions.NRL },
						AddInfoCollection = new List<AddInfo>
						{
							new AddInfo() { Key = Constants.InvoiceHeader.Keys.RecommendationLetterNumber, Value = "1A" },
							new AddInfo() { Key = Constants.InvoiceHeader.Keys.RecommendationLetterDate, Value = todayString }
						}
					},
					new AddInfoGroup
					{
						Type = new CodeDescriptionPair { Code = Constants.InvoiceHeader.Codes.NRL, Description = Constants.InvoiceHeader.Descriptions.NRL },
						AddInfoCollection = new List<AddInfo>
						{
							new AddInfo() { Key = Constants.InvoiceHeader.Keys.RecommendationLetterNumber, Value = "2B" },
							new AddInfo() { Key = Constants.InvoiceHeader.Keys.RecommendationLetterDate, Value = tomorrowString }
						}
					},
					new AddInfoGroup
					{
						Type = new CodeDescriptionPair { Code = Constants.InvoiceHeader.Codes.NSI, Description = Constants.InvoiceHeader.Descriptions.NSI },
						AddInfoCollection = new List<AddInfo>
						{
							new AddInfo() { Key = Constants.InvoiceHeader.Keys.Compartments, Value = "COMPARTMENT 1" },
							new AddInfo() { Key = Constants.InvoiceHeader.Keys.InspectionPort, Value = "INSP" },
							new AddInfo() { Key = Constants.InvoiceHeader.Keys.InspectionDate, Value = yesterdayString }
						}
					}
				},
				CustomsReferenceCollection = new List<CustomsReference>
				{
					new CustomsReference
					{
						Type = new CodeDescriptionPair { Code = QuarantineSupportingInfoCollection.DeclarationConstant },
						Reference = "007",
						Order = 7
					},
					new CustomsReference
					{
						Type = new CodeDescriptionPair { Code = QuarantineSupportingInfoCollection.DeclarationConstant },
						Reference = "008",
						Order = 8
					},
					new CustomsReference
					{
						Type = new CodeDescriptionPair { Code = QuarantineExDocRexAcknowledgement.AcknowledgementCode },
						Reference = "ACK1"
					},
					new CustomsReference
					{
						Type = new CodeDescriptionPair { Code = CusCodeDataTypeList.Codes.NEXDOCSCatchZone },
						Reference = "catch zone 1"
					},
					new CustomsReference
					{
						Type = new CodeDescriptionPair { Code = CusCodeDataTypeList.Codes.NEXDOCSCatchZone },
						Reference = "catch zone 2"
					}
				}
			};
			invoice.AddInfoGroupCollection.Add(addInfoGroupQH);

			shipment.CommercialInfo.CommercialInvoiceCollection.Add(invoice);

			invoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLine = new CommercialInvoiceLine();
			invoice.CommercialInvoiceLineCollection.Add(invoiceLine);

			invoiceLine.AddInfoCollection = new List<AddInfo>()
			{
				new AddInfo() { Key = "AQISAdditionalProducts_Hidden", Value = "EMU,GOAT,CAMEL" }
			};

			invoiceLine.AddInfoGroupCollection = new List<AddInfoGroup>();

			var addInfoGroupQL = new AddInfoGroup()
			{
				Type = new CodeDescriptionPair() { Code = Constants.InvoiceLine.Codes.QL, Description = Constants.InvoiceLine.Descriptions.QL },
				AddInfoCollection = new List<AddInfo>()
				{
					new AddInfo() { Key = Constants.InvoiceLine.Keys.NetQuantity, Value = "59" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.NetQuantityUnit, Value = "BIL" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.ImperialNetWeight, Value = "89.5" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.ImperialNetWeightUnit, Value = "CWI" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.GrossMetricWeight, Value = "98.5" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.GrossMetricWeightUnit, Value = "KGM" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.ShippingMarks, Value = "MARKS 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.BatchCode, Value = "BATCH 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.OuterPackCount, Value = "34" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.OuterPackType, Value = "BE" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.OuterPackAccuracy, Value = "3" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.OuterPackWeight, Value = "123" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.OuterPackWeightUnit, Value = "CGM" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.IntermediatePackCount, Value = "459" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.IntermediatePackType, Value = "BE" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.IntermediatePackAccuracy, Value = "4" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.IntermediatePackWeight, Value = "4588" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.IntermediatePackWeightUnit, Value = "CEN" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.InnerPackCount, Value = "500" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.InnerPackType, Value = "FE" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.InnerPackAccuracy, Value = "4" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.InnerPackWeight, Value = "3.9" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.InnerPackWeightUnit, Value = "BIL" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.ProductType, Value = "ADA" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.Category, Value = "D9999" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.SupplementaryCode, Value = "11" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.PackType, Value = "22" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.PreservationType, Value = "33" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.CutCode, Value = "ABC" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.ProductDescriptionLocationQualifier, Value = "Australian" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.ProductDescriptionQualityQualifier, Value = "PRODUCT QUALITY 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.NatureOfCommodity, Value = "TT" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.TreatmentType, Value = "BI" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.AdditionalDeclarationComments, Value = "ADDITIONAL DECLARATION" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.AdditionalProductDescription, Value = "ADDITIONAL PROD DESC" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.ClientLineItemID, Value = "CLIENT LINE ITEM ID 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.UseByStart, Value = yesterdayString },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.UseByEnd, Value = tomorrowString },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.InspectionDescription, Value = "LINE ITEM 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.CommercialProductDescription, Value = "COMMERCIAL 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.HealthCertificateDescription, Value = "HEALTH CERT 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.FormatRequested, Value = "FORMAT 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.ExtraFormatRequested, Value = "EXTRA FORMAT 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.FormatAllocated, Value = "ALLOCATED 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.CertificateNumber, Value = "CERT 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.FishWaterIndicator, Value = "F" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.DrainedWeight, Value = "666" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.DrainedWeightUnit, Value = "BIL" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.CatchStartDate, Value = yesterdayString },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.CatchEndDate, Value = tomorrowString },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.PercentOfMilkProtein, Value = "40" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.TotalWeightOfMilkProteinInMixtures, Value = "50" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.PercentOfMilkFat, Value = "60" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.TotalWeightOfMilkFatInMixtures, Value = "70" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.IMA1SerialNumber, Value = "IMA1 SERIAL NUMBER 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.IMA1QuotaYear, Value = "2010" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.IMA1ProductDescription, Value = "IMA1 DESP " },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.GrowerNumber, Value = "GROWER NO 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.SaltingDate, Value = todayString },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.ChemicalLeanPercentage, Value = "46" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.BeefVealWeightAmount, Value = "64" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.LabelApprovalNumber, Value = "NO 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.LabelApprovalIndicator, Value = "Y" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.UngradedProductIndicator, Value = "Y" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.StatementNumber1, Value = "11" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.StatementNumber2, Value = "22" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.StatementNumber3, Value = "33" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.StatementNumber4, Value = "44" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.StatementNumber5, Value = "55" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.StatementText, Value = "STATEMENT TEXT 1" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.FinalConsumer, Value = "Y" },
					new AddInfo() { Key = Constants.InvoiceLine.Keys.CombinedNomenclature, Value = "90132654" }
				},
				AddInfoGroupCollection = new List<AddInfoGroup>()
			};

			invoiceLine.AddInfoGroupCollection.Add(addInfoGroupQL);

			var reader = new JobDeclarationDataObjectReader(shipment, Logger, Factory);
			var declarationBO = reader.ReadIntoBusinessObject();

			var invoiceBO = declarationBO.Invoices.Cast<JobComInvoiceHeader>().Single();
			var quarantineHeader = invoiceBO.QuarantineExDocHeader;
			var invoiceLineBO = invoiceBO.InvoiceLines.Cast<JobComInvoiceLine>().Single();
			var quarantineLine = invoiceLineBO.QuarantineExDocLine;
			var recommendationLetters = quarantineHeader.RecommendationLetters;
			var supportingInfos = quarantineHeader.SupportingInfos;
			var acknowledgements = quarantineHeader.Acknowledgements;
			var catchZones = quarantineHeader.NexDocCatchZones;
			var compartments = quarantineHeader.Compartments;

			CombineAssertions(() =>
			{
				AssertEquals("quarantineHeader.QH_ProduceType", "DAI", quarantineHeader.QH_ProduceType);
				AssertEquals("quarantineHeader.QH_ImportedProductFlag", "YES", quarantineHeader.QH_ImportedProductFlag);
				AssertEquals("quarantineHeader.QH_LastAmendDateTime", today.ToLocalBranchTimeOffset(), quarantineHeader.QH_LastAmendDateTime);
				AssertEquals("quarantineHeader.QH_ProductUseIndicator", "H", quarantineHeader.QH_ProductUseIndicator);
				AssertEquals("quarantineHeader.QH_ObtainExportCustomsPermit", true, quarantineHeader.QH_ObtainExportCustomsPermit);
				AssertEquals("quarantineHeader.QH_ConsigneeAgentName", "CONSIGNEE REPRESENTATIVE 1", quarantineHeader.QH_ConsigneeAgentName);
				AssertEquals("quarantineHeader.QH_ExporterDeclaration", "EXPORTER DECLARATION 1", quarantineHeader.QH_ExporterDeclaration);
				AssertEquals("quarantineHeader.QH_CertificatePrintIndicator", "A", quarantineHeader.QH_CertificatePrintIndicator);
				AssertEquals("quarantineHeader.QH_AQISRegion", "CAB", quarantineHeader.QH_AQISRegion);
				AssertEquals("quarantineHeader.QH_SplitHealthCertByContainer", true, quarantineHeader.QH_SplitHealthCertByContainer);
				AssertEquals("quarantineHeader.QH_SplitHealthCertByPacker", true, quarantineHeader.QH_SplitHealthCertByPacker);
				AssertEquals("QH_SplitHealthCertByMarks", true, quarantineHeader.QH_SplitHealthCertByMarks);
				AssertEquals("quarantineHeader.QH_AMLCQuota", true, quarantineHeader.QH_AMLCQuota);
				AssertEquals("quarantineHeader.QH_QuotaType", "ABC", quarantineHeader.QH_QuotaType);
				AssertEquals("quarantineHeader.QH_ShipsStores", true, quarantineHeader.QH_ShipsStores);
				AssertEquals("quarantineHeader.QH_AMLCQuotaYear", "2020", quarantineHeader.QH_AMLCQuotaYear);
				AssertEquals("quarantineHeader.QH_CertificateRequiredLocation", "PRINT LOCATION 1", quarantineHeader.QH_CertificateRequiredLocation);
				AssertEquals("quarantineHeader.QH_RN_NKOriginCountry", "AU", quarantineHeader.QH_RN_NKOriginCountry);
				AssertEquals("quarantineHeader.QH_RL_NKBorderInspectionPort", "ADALV", quarantineHeader.QH_RL_NKBorderInspectionPort);
				AssertEquals("quarantineHeader.QH_PackDate", yesterday, quarantineHeader.QH_PackDate);
				AssertEquals("quarantineHeader.QH_AbsoluteTemperature", 20m, quarantineHeader.QH_AbsoluteTemperature);
				AssertEquals("quarantineHeader.QH_MinimumTemperature", 10m, quarantineHeader.QH_MinimumTemperature);
				AssertEquals("quarantineHeader.QH_MaximumTemperature", 30m, quarantineHeader.QH_MaximumTemperature);
				AssertEquals("quarantineHeader.QH_TemperatureUM", "CEL", quarantineHeader.QH_TemperatureUM);
				AssertEquals("quarantineHeader.QH_CustomsConsigneeName", "CUSTOMS CONSIGNEE 1", quarantineHeader.QH_CustomsConsigneeName);
				AssertEquals("quarantineHeader.QH_ExemptionCode", "EXEMPTION CODE 1", quarantineHeader.QH_ExemptionCode);
				AssertEquals("quarantineHeader.QH_PrintLocation", "CODE", quarantineHeader.QH_PrintLocation);
				AssertEquals("quarantineHeader.QH_DecOfCompliance", "YES", quarantineHeader.QH_DecOfCompliance);
				AssertEquals("quarantineHeader.QH_ImportedProductFlag", "YES", quarantineHeader.QH_ImportedProductFlag);
				AssertEquals("quarantineHeader.QH_TrueAndCompleteIndicator", "YES", quarantineHeader.QH_TrueAndCompleteIndicator);
				AssertEquals("quarantineHeader.QH_ManufacturedTreatedPackagedLabelledInAustralia", "YES", quarantineHeader.QH_ManufacturedTreatedPackagedLabelledInAustralia);
				AssertEquals("quarantineHeader.QH_LegallyImportedFlag", "YES", quarantineHeader.QH_LegallyImportedFlag);
				AssertEquals("quarantineHeader.QH_AuthorisationEstablishment", "ABC", quarantineHeader.QH_AuthorisationEstablishment);
				AssertEquals("quarantineHeader.QH_StorageEstablishment", "DEF", quarantineHeader.QH_StorageEstablishment);
				AssertEquals("quarantineHeader.QH_ApprovedCertifier", "H0001", quarantineHeader.QH_ApprovedCertifier);
				AssertEquals("quarantineHeader.QH_LotNumber", "LOT NUMBER 1", quarantineHeader.QH_LotNumber);
				AssertEquals("quarantineHeader.QH_OriginCatchZone", "CATCH ZONE 1", quarantineHeader.QH_OriginCatchZone);
				AssertEquals("quarantineHeader.QH_AvAnimalAge", "both age ranges", quarantineHeader.QH_AvAnimalAge);
				AssertEquals("quarantineHeader.QH_StartHoldSeal", "START 1", quarantineHeader.QH_StartHoldSeal);
				AssertEquals("quarantineHeader.QH_EndHoldSeal", "END 1", quarantineHeader.QH_EndHoldSeal);
				AssertEquals("quarantineHeader.QH_InspectionRequestedDate", today, quarantineHeader.QH_InspectionRequestedDate);
				AssertEquals("quarantineHeader.QH_AuthorisedStartDate", today, quarantineHeader.QH_AuthorisedStartDate);
				AssertEquals("quarantineHeader.QH_AuthorisedEndDate", today, quarantineHeader.QH_AuthorisedEndDate);
				AssertEquals("quarantineHeader.QH_AuthorisingOfficerID", "123456", quarantineHeader.QH_AuthorisingOfficerID);
				AssertEquals("quarantineHeader.QH_InspectorComments", "INSPECTOR COMMENTS 1", quarantineHeader.QH_InspectorComments);
				AssertEquals("quarantineHeader.QH_AuthorisationDate", tomorrow, quarantineHeader.QH_AuthorisationDate);
				AssertEquals("quarantineHeader.QH_AuthorisationComments", "COMMENTS 1", quarantineHeader.QH_AuthorisationComments);
				AssertEquals("quarantineHeader.QH_ForwardeeEDIUserIdentifier", "FORWARD 1", quarantineHeader.QH_ForwardeeEDIUserIdentifier);
				AssertEquals("quarantineHeader.QH_ForwardStatus", "COMP", quarantineHeader.QH_ForwardStatus);
				AssertEquals("quarantineHeader.QH_TransfereeEDIUserIdentifier", "EDI 1", quarantineHeader.QH_TransfereeEDIUserIdentifier);
				AssertEquals("quarantineHeader.QH_TransfereeExporterNumber", "EXPORTER 1", quarantineHeader.QH_TransfereeExporterNumber);
				AssertEquals("quarantineHeader.QH_CancelTransferIndicator", true, quarantineHeader.QH_CancelTransferIndicator);
				AssertEquals("quarantineHeader.QH_ApprovalNumber", "APPROVAL 1", quarantineHeader.QH_ApprovalNumber);
				AssertEquals("quarantineHeader.QH_TransitLocationType", "C", quarantineHeader.QH_TransitLocationType);
				AssertEquals("quarantineHeader.QH_EUComments", "COMMENT 1", quarantineHeader.QH_EUComments);
				AssertEquals("quarantineHeader.QH_EUTestResultRequired", true, quarantineHeader.QH_EUTestResultRequired);
				AssertEquals("quarantineHeader.QH_AuthorisationFlag", true, quarantineHeader.QH_AuthorisationFlag);
				AssertEquals("quarantineHeader.QH_AuthorisationFlag", new ZDate(2024, 08, 21), quarantineHeader.QH_LoadingDate);
				AssertEquals("AQISLoadingEstablishmentLocation.EXDOCEstablishmentNumber", "92", invoiceBO.AQISLoadingEstablishmentLocation.EXDOCEstablishmentNumber);

				AssertEquals("recommendationLetters.Count", 2, recommendationLetters.Count);
				AssertEquals("recommendationLetters[0].ZA_LetterNumber", "1A", recommendationLetters[0].ZA_LetterNumber);
				AssertEquals("recommendationLetters[0].ZA_LetterDate", today, recommendationLetters[0].ZA_LetterDate);
				AssertEquals("recommendationLetters[1].ZA_LetterNumber", "2B", recommendationLetters[1].ZA_LetterNumber);
				AssertEquals("recommendationLetters[1].ZA_LetterDate", tomorrow, recommendationLetters[1].ZA_LetterDate);

				AssertEquals("supportingInfos.Count", 2, supportingInfos.Count);
				AssertEquals("supportingInfos[0].CSI_Description", "007", supportingInfos[0].CSI_Description);
				AssertEquals("supportingInfos[0].CSI_LineN", 7, supportingInfos[0].CSI_LineNo);
				AssertEquals("supportingInfos[1].CSI_Description", "008", supportingInfos[1].CSI_Description);
				AssertEquals("supportingInfos[1].CSI_LineNo", 8, supportingInfos[1].CSI_LineNo);

				AssertEquals("acknowledgements.Count", 1, acknowledgements.Count);
				AssertEquals("acknowledgements[0].CY_Data", "ACK1", acknowledgements[0].CY_Data);

				AssertEquals("catchZones.Count", 2, catchZones.Count);
				AssertEquals("catchZones[0].CY_Data", "catch zone 1", catchZones[0].CY_Data);
				AssertEquals("catchZones[1].CY_Data", "catch zone 2", catchZones[1].CY_Data);

				AssertEquals("compartments.Count", 1, compartments.Count);
				AssertEquals("compartments[0].QC_Compartments", "COMPARTMENT 1", compartments[0].QC_Compartments);
				AssertEquals("compartments[0].QC_RL_NKInspectionPort", "INSP", compartments[0].QC_RL_NKInspectionPort);
				AssertEquals("compartments[0].QC_InspectionDate", yesterday, compartments[0].QC_InspectionDate);

				AssertEquals("quarantineLine.QL_NetQuantity", 59m, quarantineLine.QL_NetQuantity);
				AssertEquals("quarantineLine.QL_NetQuantityUnit", "BIL", quarantineLine.QL_NetQuantityUnit);
				AssertEquals(" quarantineLine.QL_ImperialNetWeight", 89.5m, quarantineLine.QL_ImperialNetWeight);
				AssertEquals("quarantineLine.QL_ImperialNetWeightUnit", "CWI", quarantineLine.QL_ImperialNetWeightUnit);
				AssertEquals("quarantineLine.QL_GrossMetricWeight", 98.5m, quarantineLine.QL_GrossMetricWeight);
				AssertEquals("quarantineLine.QL_GrossMetricWeightUnit", "KGM", quarantineLine.QL_GrossMetricWeightUnit);
				AssertEquals("quarantineLine.QL_ShippingMarks", "MARKS 1", quarantineLine.QL_ShippingMarks);
				AssertEquals("quarantineLine.QL_BatchCode", "BATCH 1", quarantineLine.QL_BatchCode);
				AssertEquals("quarantineLine.QL_OuterPackCount", 34, quarantineLine.QL_OuterPackCount);
				AssertEquals("quarantineLine.QL_OuterPackType", "BE", quarantineLine.QL_OuterPackType);
				AssertEquals("quarantineLine.QL_OuterPackAccuracy", "3", quarantineLine.QL_OuterPackAccuracy);
				AssertEquals("quarantineLine.QL_OuterPackWeight", 123m, quarantineLine.QL_OuterPackWeight);
				AssertEquals("quarantineLine.QL_OuterPackWeightUnit", "CGM", quarantineLine.QL_OuterPackWeightUnit);
				AssertEquals("quarantineLine.QL_IntermediatePackCount", 459, quarantineLine.QL_IntermediatePackCount);
				AssertEquals("quarantineLine.QL_IntermediatePackType", "BE", quarantineLine.QL_IntermediatePackType);
				AssertEquals("quarantineLine.QL_IntermediatePackAccuracy", "4", quarantineLine.QL_IntermediatePackAccuracy);
				AssertEquals("quarantineLine.QL_IntermediatePackWeight", 4588m, quarantineLine.QL_IntermediatePackWeight);
				AssertEquals("quarantineLine.QL_IntermediatePackWeightUnit", "CEN", quarantineLine.QL_IntermediatePackWeightUnit);
				AssertEquals("quarantineLine.QL_InnerPackCount", 500, quarantineLine.QL_InnerPackCount);
				AssertEquals("quarantineLine.QL_InnerPackType", "FE", quarantineLine.QL_InnerPackType);
				AssertEquals("quarantineLine.QL_InnerPackAccuracy", "4", quarantineLine.QL_InnerPackAccuracy);
				AssertEquals("quarantineLine.QL_InnerPackWeight", 3.9m, quarantineLine.QL_InnerPackWeight);
				AssertEquals("quarantineLine.QL_InnerPackWeightUnit", "BIL", quarantineLine.QL_InnerPackWeightUnit);
				AssertEquals("quarantineLine.QL_ProductType", "ADA", quarantineLine.QL_ProductType);
				AssertEquals("quarantineLine.QL_Category", "D9999", quarantineLine.QL_Category);
				AssertEquals("quarantineLine.QL_SupplimentaryCode", "11", quarantineLine.QL_SupplimentaryCode);
				AssertEquals("quarantineLine.QL_PackType", "22", quarantineLine.QL_PackType);
				AssertEquals("quarantineLine.QL_PreservationType", "33", quarantineLine.QL_PreservationType);
				AssertEquals("quarantineLine.QL_CutCode", "ABC", quarantineLine.QL_CutCode);
				AssertEquals("quarantineLine.QL_ProductDescriptionLocationQualifier", "Australian", quarantineLine.QL_ProductDescriptionLocationQualifier);
				AssertEquals("quarantineLine.QL_ProductDescriptionQualityQualifier", "PRODUCT QUALITY 1", quarantineLine.QL_ProductDescriptionQualityQualifier);
				AssertEquals("quarantineLine.QL_NatureOfCommodity", "TT", quarantineLine.QL_NatureOfCommodity);
				AssertEquals("quarantineLine.QL_TreatmentType", "BI", quarantineLine.QL_TreatmentType);
				AssertEquals("quarantineLine.QL_AddtionalDeclarationComments", "ADDITIONAL DECLARATION", quarantineLine.QL_AddtionalDeclarationComments);
				AssertEquals("quarantineLine.QL_AddtionalProductDescription", "ADDITIONAL PROD DESC", quarantineLine.QL_AddtionalProductDescription);
				AssertEquals("quarantineLine.QL_AdditionalProducts", "EMU,GOAT,CAMEL", quarantineLine.QL_AdditionalProducts);
				AssertEquals("quarantineLine.QL_ClientLineItemID", "CLIENT LINE ITEM ID 1", quarantineLine.QL_ClientLineItemID);
				AssertEquals("quarantineLine.QL_UseByStart", yesterday, quarantineLine.QL_UseByStart);
				AssertEquals("quarantineLine.QL_UseByEnd", tomorrow, quarantineLine.QL_UseByEnd);
				AssertEquals("quarantineLine.QL_MeatInspectionDescription", "LINE ITEM 1", quarantineLine.QL_MeatInspectionDescription);
				AssertEquals("quarantineLine.QL_CommercialProductDescription", "COMMERCIAL 1", quarantineLine.QL_CommercialProductDescription);
				AssertEquals("quarantineLine.QL_HealthCertificateDescription", "HEALTH CERT 1", quarantineLine.QL_HealthCertificateDescription);
				AssertEquals("quarantineLine.QL_HCFormatRequested", "FORMAT 1", quarantineLine.QL_HCFormatRequested);
				AssertEquals("quarantineLine.QL_ExtraCertificate", "EXTRA FORMAT 1", quarantineLine.QL_ExtraCertificate);
				AssertEquals("quarantineLine.QL_HCFormatAllocated", "ALLOCATED 1", quarantineLine.QL_HCFormatAllocated);
				AssertEquals("quarantineLine.QL_HCNumber", "CERT 1", quarantineLine.QL_HCNumber);
				AssertEquals("quarantineLine.QL_FishWaterIndicator", "F", quarantineLine.QL_FishWaterIndicator);
				AssertEquals("quarantineLine.QL_DrainedWeight", 666m, quarantineLine.QL_DrainedWeight);
				AssertEquals("quarantineLine.QL_DrainedWeightUnit", "BIL", quarantineLine.QL_DrainedWeightUnit);
				AssertEquals("quarantineLine.QL_CatchStartDate", yesterday, quarantineLine.QL_CatchStartDate);
				AssertEquals("quarantineLine.QL_CatchEndDate", tomorrow, quarantineLine.QL_CatchEndDate);
				AssertEquals("quarantineLine.QL_PercentOfMilkProtein", 40m, quarantineLine.QL_PercentOfMilkProtein);
				AssertEquals("quarantineLine.QL_TotalWeightOfMilkProteinInMixtures", 50m, quarantineLine.QL_TotalWeightOfMilkProteinInMixtures);
				AssertEquals("quarantineLine.QL_PercentOfMilkFat", 60m, quarantineLine.QL_PercentOfMilkFat);
				AssertEquals("quarantineLine.QL_TotalWeightOfMilkFatInMixtures", 70m, quarantineLine.QL_TotalWeightOfMilkFatInMixtures);
				AssertEquals("quarantineLine.QL_IMA1SerialNumber", "IMA1 SERIAL NUMBER 1", quarantineLine.QL_IMA1SerialNumber);
				AssertEquals("quarantineLine.QL_IMA1QuotaYear", "2010", quarantineLine.QL_IMA1QuotaYear);
				AssertEquals("quarantineLine.QL_IMA1ProductDesciption", "IMA1 DESP ", quarantineLine.QL_IMA1ProductDesciption);
				AssertEquals("quarantineLine.QL_GrowerNumber", "GROWER NO 1", quarantineLine.QL_GrowerNumber);
				AssertEquals("quarantineLine.QL_SaltingDate", today, quarantineLine.QL_SaltingDate);
				AssertEquals("quarantineLine.QL_ChemicalLeanPercentage", 46, quarantineLine.QL_ChemicalLeanPercentage);
				AssertEquals("quarantineLine.QL_BeefVealWeightAmount", 64m, quarantineLine.QL_BeefVealWeightAmount);
				AssertEquals("quarantineLine.QL_LabelApprovalNumber", "NO 1", quarantineLine.QL_LabelApprovalNumber);
				AssertEquals("quarantineLine.QL_LabelApprovalIndicator", true, quarantineLine.QL_LabelApprovalIndicator);
				AssertEquals("quarantineLine.QL_UngradedProductIndicator", true, quarantineLine.QL_UngradedProductIndicator);
				AssertEquals("quarantineLine.QL_StatementNumber1", (short)11, quarantineLine.QL_StatementNumber1);
				AssertEquals("quarantineLine.QL_StatementNumber2", (short)22, quarantineLine.QL_StatementNumber2);
				AssertEquals("quarantineLine.QL_StatementNumber3", (short)33, quarantineLine.QL_StatementNumber3);
				AssertEquals("quarantineLine.QL_StatementNumber4", (short)44, quarantineLine.QL_StatementNumber4);
				AssertEquals("quarantineLine.QL_StatementNumber5", (short)55, quarantineLine.QL_StatementNumber5);
				AssertEquals("quarantineLine.QL_StatementText", "STATEMENT TEXT 1", quarantineLine.QL_StatementText);
				AssertEquals("quarantineLine.QL_FinalConsumer", true, quarantineLine.QL_FinalConsumer);
				AssertEquals("quarantineLine.QL_CombinedNomenclature", "90132654", quarantineLine.QL_CombinedNomenclature);
			});
		}
	}
}
