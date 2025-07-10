using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using UniversalAddInfo = Enterprise.UniversalDataBuss.DataObjects.Universal.AddInfo;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CommercialInvoiceHeaderDataObjectWriter : DataTransfer.Universal.CommercialInvoiceHeaderDataObjectWriter
	{
		internal CommercialInvoiceHeaderDataObjectWriter(IDataWritingManager manager, UniversalDataObjectWriterHelper helper, ILandedCostDataWriter landedCostDataWriter = null, Customs.Business.CusEntryHeader relatedEntry = null)
			: base(manager, helper, landedCostDataWriter, relatedEntry)
		{
		}

		protected override List<UniversalAddInfo> GetInvoiceLineAddInfoCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = base.GetInvoiceLineAddInfoCollection(invoiceLineBO);
			var invoiceLine = (JobComInvoiceLine)invoiceLineBO;
			var tilvAddInfo = invoiceLine.GetTILV4Warehouse();
			if (tilvAddInfo != null)
			{
				result.Add(tilvAddInfo);
			}

			UpdateAddInfoCollection(result, Constants.InvoiceLine.Keys.RelatedExportPermitNumber, invoiceLine.JI_RelatedExportPermitNumber);
			UpdateAddInfoCollection(result, Constants.InvoiceLine.Keys.RelatedExportPermitAuthority, invoiceLine.JI_RelatedExportPermitAuthority);
			UpdateAddInfoCollection(result, Constants.InvoiceLine.Keys.RelatedExportPermitDate, invoiceLine.JI_RelatedExportPermitDate);

			return result;
		}

		protected override ZBool IsPopulateBondedWarehouseDetails(BaseJobComInvoiceLine invoiceLineBO)
		{
			var declaration = invoiceLineBO.Declaration;
			return declaration != null && declaration.SupportsBondedWarehousing && (declaration.IsExWarehouse || (declaration.IsImport && invoiceLineBO.IsGoingIntoBondedWarehouse));
		}

		protected override void PopulateBondedWarehouseQuantityAndUnit(BaseJobComInvoiceLine invoiceLineBO, CommercialInvoiceLine invoiceLineData)
		{
			invoiceLineData.BondedWarehouseQuantity = invoiceLineBO.JI_InvoiceQuantity;
			invoiceLineData.BondedWarehouseQuantityUnit = ListHelper.GetWithDescription<CodeDescriptionPair>(invoiceLineBO.JI_InvoiceUQ, invoiceLineBO.Lookups.InvoiceUQList);
		}

		protected override List<AddInfoGroup> GetInvoiceLineAddInfoGroupCollection(BaseJobComInvoiceLine invoiceLineBO)
		{
			var result = base.GetInvoiceLineAddInfoGroupCollection(invoiceLineBO) ?? new List<AddInfoGroup>();

			var invoiceLine = (JobComInvoiceLine)invoiceLineBO;
			var quarantineExDocLine = invoiceLine.QuarantineExDocLine;

			if (quarantineExDocLine != null)
			{
				var addInfoGroup = new AddInfoGroup()
				{
					Type = new CodeDescriptionPair()
					{
						Code = Constants.InvoiceLine.Codes.QL,
						Description = Constants.InvoiceLine.Descriptions.QL
					}
				};
				addInfoGroup.AddInfoCollection = new List<UniversalAddInfo>();
				result.Add(addInfoGroup);

				//RFP Packages tab
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.NetQuantity, quarantineExDocLine.QL_NetQuantity);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.NetQuantityUnit, quarantineExDocLine.QL_NetQuantityUnit);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.ImperialNetWeight, quarantineExDocLine.QL_ImperialNetWeight);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.ImperialNetWeightUnit, quarantineExDocLine.QL_ImperialNetWeightUnit);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.GrossMetricWeight, quarantineExDocLine.QL_GrossMetricWeight);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.GrossMetricWeightUnit, quarantineExDocLine.QL_GrossMetricWeightUnit);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.ShippingMarks, quarantineExDocLine.QL_ShippingMarks);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.BatchCode, quarantineExDocLine.QL_BatchCode);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.OuterPackCount, quarantineExDocLine.QL_OuterPackCount);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.OuterPackType, quarantineExDocLine.QL_OuterPackType);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.OuterPackAccuracy, quarantineExDocLine.QL_OuterPackAccuracy);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.OuterPackWeight, quarantineExDocLine.QL_OuterPackWeight);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.OuterPackWeightUnit, quarantineExDocLine.QL_OuterPackWeightUnit);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.IntermediatePackCount, quarantineExDocLine.QL_IntermediatePackCount);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.IntermediatePackType, quarantineExDocLine.QL_IntermediatePackType);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.IntermediatePackAccuracy, quarantineExDocLine.QL_IntermediatePackAccuracy);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.IntermediatePackWeight, quarantineExDocLine.QL_IntermediatePackWeight);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.IntermediatePackWeightUnit, quarantineExDocLine.QL_IntermediatePackWeightUnit);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.InnerPackCount, quarantineExDocLine.QL_InnerPackCount);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.InnerPackType, quarantineExDocLine.QL_InnerPackType);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.InnerPackAccuracy, quarantineExDocLine.QL_InnerPackAccuracy);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.InnerPackWeight, quarantineExDocLine.QL_InnerPackWeight);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.InnerPackWeightUnit, quarantineExDocLine.QL_InnerPackWeightUnit);

				//RFP Details tab
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.ProductType, quarantineExDocLine.QL_ProductType);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.Category, quarantineExDocLine.QL_Category);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.SupplementaryCode, quarantineExDocLine.QL_SupplimentaryCode);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.PackType, quarantineExDocLine.QL_PackType);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.PreservationType, quarantineExDocLine.QL_PreservationType);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.CutCode, quarantineExDocLine.QL_CutCode);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.ProductDescriptionLocationQualifier, quarantineExDocLine.QL_ProductDescriptionLocationQualifier);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.ProductDescriptionQualityQualifier, quarantineExDocLine.QL_ProductDescriptionQualityQualifier);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.NatureOfCommodity, quarantineExDocLine.QL_NatureOfCommodity);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.TreatmentType, quarantineExDocLine.QL_TreatmentType);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.AdditionalDeclarationComments, quarantineExDocLine.QL_AddtionalDeclarationComments);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.ClientLineItemID, quarantineExDocLine.QL_ClientLineItemID);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.FinalConsumer, quarantineExDocLine.QL_FinalConsumer);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.CombinedNomenclature, quarantineExDocLine.QL_CombinedNomenclature);

				//RFP Process tab
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.UseByStart, quarantineExDocLine.QL_UseByStart);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.UseByEnd, quarantineExDocLine.QL_UseByEnd);
				PopulateProcesses(addInfoGroup, quarantineExDocLine.Processes);

				//RFP Certificates tab
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.InspectionDescription, quarantineExDocLine.QL_MeatInspectionDescription);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.AdditionalProductDescription, quarantineExDocLine.QL_AddtionalProductDescription);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.CommercialProductDescription, quarantineExDocLine.QL_CommercialProductDescription);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.HealthCertificateDescription, quarantineExDocLine.QL_HealthCertificateDescription);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.FormatRequested, quarantineExDocLine.QL_HCFormatRequested);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.ExtraFormatRequested, quarantineExDocLine.QL_ExtraCertificate);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.FormatAllocated, quarantineExDocLine.QL_HCFormatAllocated);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.CertificateNumber, quarantineExDocLine.QL_HCNumber);

				//RFP Analysis tab
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.DrainedWeight, quarantineExDocLine.QL_DrainedWeight);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.DrainedWeightUnit, quarantineExDocLine.QL_DrainedWeightUnit);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.FishWaterIndicator, quarantineExDocLine.QL_FishWaterIndicator);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.CatchStartDate, quarantineExDocLine.QL_CatchStartDate);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.CatchEndDate, quarantineExDocLine.QL_CatchEndDate);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.PercentOfMilkProtein, quarantineExDocLine.QL_PercentOfMilkProtein);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.TotalWeightOfMilkProteinInMixtures, quarantineExDocLine.QL_TotalWeightOfMilkProteinInMixtures);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.PercentOfMilkFat, quarantineExDocLine.QL_PercentOfMilkFat);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.TotalWeightOfMilkFatInMixtures, quarantineExDocLine.QL_TotalWeightOfMilkFatInMixtures);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.IMA1SerialNumber, quarantineExDocLine.QL_IMA1SerialNumber);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.IMA1QuotaYear, quarantineExDocLine.QL_IMA1QuotaYear);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.IMA1ProductDescription, quarantineExDocLine.QL_IMA1ProductDesciption);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.GrowerNumber, quarantineExDocLine.QL_GrowerNumber);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.SaltingDate, quarantineExDocLine.QL_SaltingDate);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.FarmCode, quarantineExDocLine.QL_FarmCode);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.FarmType, quarantineExDocLine.QL_FarmType);

				//RFP Meat tab
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.ChemicalLeanPercentage, quarantineExDocLine.QL_ChemicalLeanPercentage);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.BeefVealWeightAmount, quarantineExDocLine.QL_BeefVealWeightAmount);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.LabelApprovalNumber, quarantineExDocLine.QL_LabelApprovalNumber);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.LabelApprovalIndicator, quarantineExDocLine.QL_LabelApprovalIndicator);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.UngradedProductIndicator, quarantineExDocLine.QL_UngradedProductIndicator);

				//RFP Statements tab
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.StatementNumber1, quarantineExDocLine.QL_StatementNumber1);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.StatementNumber2, quarantineExDocLine.QL_StatementNumber2);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.StatementNumber3, quarantineExDocLine.QL_StatementNumber3);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.StatementNumber4, quarantineExDocLine.QL_StatementNumber4);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.StatementNumber5, quarantineExDocLine.QL_StatementNumber5);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.StatementText, quarantineExDocLine.QL_StatementText);

				//REX Product Attachments tab
				var invoice = invoiceLine.InvoiceHeader;
				if (invoice != null && invoice.IsNEXDOCSActive)
				{
					PopulateRFPAttachment(result, invoiceLine, new CodeDescriptionPair
					{
						Code = Constants.InvoiceLine.Codes.QLA,
						Description = Constants.InvoiceLine.Descriptions.QLA
					});
				}
			}

			return result;
		}

		void PopulateRFPAttachment(List<AddInfoGroup> result, ICusStorageDocPivotParent pivotParent, CodeDescriptionPair rfpInfoGroupType)
		{
			foreach (var cusStorageDocPivot in pivotParent.EDocPivotCollection.Cast<CusStorageDocPivot>())
			{
				var infoGroup = new AddInfoGroup
				{
					Type = rfpInfoGroupType,
					AddInfoCollection = new List<UniversalAddInfo>()
				};
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.RFPAttachment.Keys.AttachmentFileName, cusStorageDocPivot.FileName);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.RFPAttachment.Keys.AttachmentMimeType, cusStorageDocPivot.MimeType);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.RFPAttachment.Keys.AttachmentType, cusStorageDocPivot.CSD_DocType);
				UpdateAddInfoCollection(infoGroup.AddInfoCollection, Constants.RFPAttachment.Keys.AttachmentDescription, cusStorageDocPivot.CSD_Description);
				result.Add(infoGroup);
			}
		}

		void PopulateProcesses(AddInfoGroup addInfoGroup, QuarantineExDocEstablishmentAndTimeCollection processes)
		{
			addInfoGroup.AddInfoGroupCollection = new List<AddInfoGroup>();
			foreach (var process in processes.Cast<QuarantineExDocEstablishmentAndTime>())
			{
				var subAddInfoGroup = new AddInfoGroup()
				{
					Type = new CodeDescriptionPair()
					{
						Code = Constants.InvoiceLine.Codes.NPD,
						Description = Constants.InvoiceLine.Descriptions.NPD
					}
				};
				subAddInfoGroup.SetWriterStrategy(writeManager.WriterStrategy);
				addInfoGroup.AddInfoGroupCollection.Add(subAddInfoGroup);
				subAddInfoGroup.AddInfoCollection = new List<UniversalAddInfo>();

				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.ProcessingType, process.EE_ProcessingType);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.EstablishmentID, process.EE_AuthorisationEstablishmentID);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.EstablishmentIndicator, process.EE_EstablishmentIndicator);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.StartDate, process.EE_StartDate);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.EndDate, process.EE_EndDate);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.DepurationDate, process.EE_Depuration);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.HarvestArea, process.EE_HarvestArea);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.InspectionRequestedDate, process.EE_InspectionRequestedDate);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.LeaseNumber, process.EE_LeaseNumber);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.TreatmentCode, process.EE_TreatmentCode);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.TreatmentInformation, process.EE_TreatmentInfo);

				if (process.EE_EstablishmentPostedStatus == NEXDOCEstablishmentPostedStatus.Codes.DeletePending)
				{
					UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceLine.Keys.RemoveEntry, (ZString)"true");
				}

				subAddInfoGroup.AddOrgAddress(writeManager, process.Address);
			}
		}

		protected override List<AddInfoGroup> GetInvoiceAddInfoGroupCollection(BaseJobComInvoiceHeader invoiceBO)
		{
			var result = base.GetInvoiceAddInfoGroupCollection(invoiceBO) ?? new List<AddInfoGroup>();

			var invoice = (JobComInvoiceHeader)invoiceBO;
			var quarantineExDocHeader = invoice.QuarantineExDocHeader;

			if (quarantineExDocHeader != null)
			{
				var addInfoGroup = new AddInfoGroup()
				{
					Type = new CodeDescriptionPair()
					{
						Code = Constants.InvoiceHeader.Codes.QH,
						Description = Constants.InvoiceHeader.Descriptions.QH
					}
				};
				addInfoGroup.AddInfoCollection = new List<UniversalAddInfo>();
				result.Add(addInfoGroup);

				if (!quarantineExDocHeader.ManualAmendmentReasonForMessaging.IsEmpty)
				{
					UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.SubmitAmendmentRequest, ZBool.True);
					UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.RequestAmendReason, quarantineExDocHeader.ManualAmendmentReasonForMessaging);
				}

				if (!quarantineExDocHeader.ReissueCertificateNameForMessaging.IsEmpty)
				{
					UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ReissueCertificateName, quarantineExDocHeader.ReissueCertificateNameForMessaging);
					UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ReissueCertificateReason, quarantineExDocHeader.ReissueCertificateReasonForMessaging);
				}

				//RFP Details tab
				var lastAmendDateTime = quarantineExDocHeader.QH_LastAmendDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz", CultureInfo.InvariantCulture);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.LastAmendDateTime, new ZString(lastAmendDateTime));
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ProduceType, quarantineExDocHeader.QH_ProduceType);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ProductUse, quarantineExDocHeader.QH_ProductUseIndicator);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ObtainExportCustomsPermit, quarantineExDocHeader.QH_ObtainExportCustomsPermit);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ConsigneeAgentName, quarantineExDocHeader.QH_ConsigneeAgentName);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ExporterDeclaration, quarantineExDocHeader.QH_ExporterDeclaration);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.CertificatePrintIndicator, quarantineExDocHeader.QH_CertificatePrintIndicator);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ProductionRegion, quarantineExDocHeader.QH_AQISRegion);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.SplitHealthCertByContainer, quarantineExDocHeader.QH_SplitHealthCertByContainer);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.SplitHealthCertByPacker, quarantineExDocHeader.QH_SplitHealthCertByPacker);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.SplitHealthCertByMarks, quarantineExDocHeader.QH_SplitHealthCertByMarks);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.AMLCQuota, quarantineExDocHeader.QH_AMLCQuota);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.QuotaType, quarantineExDocHeader.QH_QuotaType);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ShipsStores, quarantineExDocHeader.QH_ShipsStores);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.AMLCQuotaYear, quarantineExDocHeader.QH_AMLCQuotaYear);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.CertificateRequiredLocation, quarantineExDocHeader.QH_CertificateRequiredLocation);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ProductSource, quarantineExDocHeader.QH_RN_NKOriginCountry);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.BorderInspectionPort, quarantineExDocHeader.QH_RL_NKBorderInspectionPort);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.PackDate, quarantineExDocHeader.QH_PackDate);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.AbsoluteTemperature, quarantineExDocHeader.QH_AbsoluteTemperature);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.MinimumTemperature, quarantineExDocHeader.QH_MinimumTemperature);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.MaximumTemperature, quarantineExDocHeader.QH_MaximumTemperature);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.TemperatureUnit, quarantineExDocHeader.QH_TemperatureUM);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.CustomsConsigneeName, quarantineExDocHeader.QH_CustomsConsigneeName);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ExemptionCode, quarantineExDocHeader.QH_ExemptionCode);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.PrintLocation, quarantineExDocHeader.QH_PrintLocation);

				PopulateRecommendationLetters(addInfoGroup, quarantineExDocHeader.RecommendationLetters);

				//RFP Indicator Declaration tab
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.DeclarationOfCompliance, quarantineExDocHeader.QH_DecOfCompliance);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ImportedProductFlag, quarantineExDocHeader.QH_ImportedProductFlag);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.TrueAndComplete, quarantineExDocHeader.QH_TrueAndCompleteIndicator);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ManufacturedTreatedPackagedLabelledInAustralia, quarantineExDocHeader.QH_ManufacturedTreatedPackagedLabelledInAustralia);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.LegallyImportedFlag, quarantineExDocHeader.QH_LegallyImportedFlag);

				//RFP Inspection Details tab
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.AuthorisationEstablishment, quarantineExDocHeader.QH_AuthorisationEstablishment);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.StorageEstablishment, quarantineExDocHeader.QH_StorageEstablishment);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ApprovedCertifier, quarantineExDocHeader.QH_ApprovedCertifier);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.LotNumber, quarantineExDocHeader.QH_LotNumber);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.CatchZone, quarantineExDocHeader.QH_OriginCatchZone);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.AverageAnimalAge, quarantineExDocHeader.QH_AvAnimalAge);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.StartHoldSeal, quarantineExDocHeader.QH_StartHoldSeal);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.EndHoldSeal, quarantineExDocHeader.QH_EndHoldSeal);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.InspectionRequestedDate, quarantineExDocHeader.QH_InspectionRequestedDate);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.AuthorisedStartDate, quarantineExDocHeader.QH_AuthorisedStartDate);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.AuthorisedEndDate, quarantineExDocHeader.QH_AuthorisedEndDate);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.AuthorisingOfficerID, quarantineExDocHeader.QH_AuthorisingOfficerID);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.InspectorComments, quarantineExDocHeader.QH_InspectorComments);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.AuthorisationDate, quarantineExDocHeader.QH_AuthorisationDate);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.AuthorisationComments, quarantineExDocHeader.QH_AuthorisationComments);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.AuthorisationFlag, (ZString)quarantineExDocHeader.QH_AuthorisationFlag.ToString());

				foreach (QuarantineSupportingInfo supportingInfo in quarantineExDocHeader.SupportingInfos)
				{
					AddCustomsReferenceCollection(addInfoGroup, new CodeDescriptionPair() { Code = QuarantineSupportingInfoCollection.DeclarationConstant }, supportingInfo.CSI_Description, order: supportingInfo.CSI_LineNo);
				}

				foreach (var acknowledgement in quarantineExDocHeader.Acknowledgements.Cast<QuarantineExDocRexAcknowledgement>().OrderBy(x => x.CY_Data))
				{
					AddCustomsReferenceCollection(addInfoGroup, new CodeDescriptionPair { Code = QuarantineExDocRexAcknowledgement.AcknowledgementCode }, acknowledgement.CY_Data);
				}

				foreach (var catchZone in quarantineExDocHeader.NexDocCatchZones)
				{
					AddCustomsReferenceCollection(addInfoGroup, new CodeDescriptionPair { Code = CusCodeDataTypeList.Codes.NEXDOCSCatchZone }, catchZone.CY_Data);
				}

				//RFP Forward/ Transfer tab
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ForwardeeEDIUserIdentifier, quarantineExDocHeader.QH_ForwardeeEDIUserIdentifier);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ForwardStatus, quarantineExDocHeader.QH_ForwardStatus);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.TransfereeEDIUserIdentifier, quarantineExDocHeader.QH_TransfereeEDIUserIdentifier);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.TransfereeExporterNumber, quarantineExDocHeader.QH_TransfereeExporterNumber);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.CancelTransferIndicator, quarantineExDocHeader.QH_CancelTransferIndicator);

				//RFP Ships Compartments tab
				PopulateCompartments(addInfoGroup, quarantineExDocHeader.Compartments);

				//RFP EU Details
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.ApprovalNumber, quarantineExDocHeader.QH_ApprovalNumber);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.TransitLocationType, quarantineExDocHeader.QH_TransitLocationType);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.EUComments, quarantineExDocHeader.QH_EUComments);
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.EUTestResultRequired, quarantineExDocHeader.QH_EUTestResultRequired);

				//Messages tab
				if (!quarantineExDocHeader.QH_RequestForPermitNumber.IsEmpty || !quarantineExDocHeader.QH_ExportPermitNumber.IsEmpty)
				{
					if (!quarantineExDocHeader.QH_RequestForPermitNumber.IsEmpty)
					{
						AddCustomsReferenceCollection(addInfoGroup, new CodeDescriptionPair { Code = CusEntryNumber.EntryType.RequestForPermitStatus }, quarantineExDocHeader.QH_RequestForPermitNumber, quarantineExDocHeader.RequestForPermitStatus);
					}
					if (!quarantineExDocHeader.QH_ExportPermitNumber.IsEmpty)
					{
						AddCustomsReferenceCollection(addInfoGroup, new CodeDescriptionPair { Code = CusEntryNumber.EntryType.ExdocPermitNumber }, quarantineExDocHeader.QH_ExportPermitNumber, quarantineExDocHeader.ExportPermitStatus);
					}
				}

				//REX Product Attachments tab
				if (invoice.IsNEXDOCSActive)
				{
					PopulateRFPAttachment(result, invoice, new CodeDescriptionPair
					{
						Code = Constants.InvoiceHeader.Codes.QHA,
						Description = Constants.InvoiceHeader.Descriptions.QHA
					});
				}

				//REX Inspection Details tab
				var loadingDate = quarantineExDocHeader.QH_LoadingDate.ToString("yyyy-MM-dd");
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.LoadingDate, new ZString(loadingDate));
				var loadingEstablishmentID = invoice.AQISLoadingEstablishmentLocation.E2_GovRegNum;
				UpdateAddInfoCollection(addInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.LoadingEstablishment, new ZString(loadingEstablishmentID));
			}

			return result;
		}

		void PopulateCompartments(AddInfoGroup addInfoGroup, QuarantineExDocShipsCompartmentCollection shipsCompartments)
		{
			foreach (var shipsCompartment in shipsCompartments.Cast<QuarantineExDocShipsCompartment>())
			{
				var subAddInfoGroup = new AddInfoGroup()
				{
					Type = new CodeDescriptionPair()
					{
						Code = Constants.InvoiceHeader.Codes.NSI,
						Description = Constants.InvoiceHeader.Descriptions.NSI
					}
				};
				addInfoGroup.AddInfoGroupCollection.Add(subAddInfoGroup);

				subAddInfoGroup.AddInfoCollection = new List<UniversalAddInfo>();
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.Compartments, shipsCompartment.QC_Compartments);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.InspectionPort, shipsCompartment.QC_RL_NKInspectionPort);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.InspectionDate, shipsCompartment.QC_InspectionDate);
			}
		}

		void PopulateRecommendationLetters(AddInfoGroup addInfoGroup, RecommendationLetterCollection recommendationLetters)
		{
			addInfoGroup.AddInfoGroupCollection = new List<AddInfoGroup>();
			foreach (var recommendationLetter in recommendationLetters.Cast<RecommendationLetter>())
			{
				var subAddInfoGroup = new AddInfoGroup()
				{
					Type = new CodeDescriptionPair()
					{
						Code = Constants.InvoiceHeader.Codes.NRL,
						Description = Constants.InvoiceHeader.Descriptions.NRL
					}
				};
				addInfoGroup.AddInfoGroupCollection.Add(subAddInfoGroup);

				subAddInfoGroup.AddInfoCollection = new List<UniversalAddInfo>();
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.RecommendationLetterNumber, recommendationLetter.ZA_LetterNumber);
				UpdateAddInfoCollection(subAddInfoGroup.AddInfoCollection, Constants.InvoiceHeader.Keys.RecommendationLetterDate, recommendationLetter.ZA_LetterDate);
			}
		}

		void UpdateAddInfoCollection(List<UniversalAddInfo> addInfoList, ZString key, IZType value)
		{
			if (!value.IsEmpty && !value.IsDefault)
			{
				helper.Update(addInfoList, key, value);
			}
		}

		void AddCustomsReferenceCollection(AddInfoGroup addInfoGroup, CodeDescriptionPair type, ZString reference, string referencedEntityDescription = null, int order = 0)
		{
			if (!reference.IsEmpty)
			{
				if (addInfoGroup.CustomsReferenceCollection == null)
				{
					addInfoGroup.CustomsReferenceCollection = new List<CustomsReference>();
				}
				var customsReferenceList = addInfoGroup.CustomsReferenceCollection;
				var customsReference = new CustomsReference();
				customsReference.Type = type;
				customsReference.Reference = reference;
				if (referencedEntityDescription != null)
				{
					customsReference.ReferencedEntityDescription = referencedEntityDescription;
				}
				if (order > 0)
				{
					customsReference.Order = order;
				}
				customsReferenceList.Add(customsReference);
			}
		}
	}
}
