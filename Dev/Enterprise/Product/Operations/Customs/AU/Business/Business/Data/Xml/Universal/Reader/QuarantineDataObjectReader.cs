using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class QuarantineLineDataObjectReader : DataObjectReader<AddInfoGroup>
	{
		public QuarantineLineDataObjectReader(AddInfoGroup addInfoGroup, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, ZGuid parentPK)
			: base(addInfoGroup, logger, helper.Factory)
		{
			this.parentPK = parentPK;
			this.helper = helper;
			this.addInfoGroup = addInfoGroup;
		}

		public IColumnIndexer[] ReadIntoDataRow()
		{
			var rowList = new List<IColumnIndexer>();

			if (addInfoGroup.Type != null)
			{
				var code = addInfoGroup.Type.Code ?? ZString.Empty;
				if (code == QuarantineExDocHeaderSchema.Constants.Prefix)
				{
					rowList.AddRange(PopulateQH());
				}
				else if (code == QuarantineExDocLineSchema.Constants.Prefix)
				{
					rowList.AddRange(PopulateQL());
				}
			}
			return rowList.ToArray();
		}

		List<IColumnIndexer> PopulateQH()
		{
			var rowList = new List<IColumnIndexer>();
			var invoice = helper.Factory.Load<JobComInvoiceHeader>(parentPK);
			if (invoice == null || !invoice.JobDeclaration.IsQuarantine)
			{
				logger.LogBoth(LogType.Warning, "Quarantine header in XML does not belong to a quarantine (AQS) declaration.");
				return rowList;
			}

			var quarantineHeader = invoice.QuarantineExDocHeader;
			var row = GetColumnIndexer(quarantineHeader);
			var addInfoRow = GetColumnIndexer(quarantineHeader.AddInfo);
			SetValue(row, QuarantineExDocHeaderSchema.QH_JZ, invoice.PK);

			//RFP Details tab
			SetValue(row, QuarantineExDocHeaderSchema.QH_ProduceType, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ProduceType));
			SetValue(row, QuarantineExDocHeaderSchema.QH_LastAmendDateTime, addInfoGroup.AddInfoCollection.GetZDateTimeOffsetValue(Constants.InvoiceHeader.Keys.LastAmendDateTime));
			SetValue(addInfoRow, AddInfoQuarantineExDocHeaderSchema.ZH_ProductUseIndicator, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ProductUse));
			SetValue(row, QuarantineExDocHeaderSchema.QH_ObtainExportCustomsPermit, addInfoGroup.AddInfoCollection.GetZBoolValue(Constants.InvoiceHeader.Keys.ObtainExportCustomsPermit));
			SetValue(row, QuarantineExDocHeaderSchema.QH_ConsigneeAgentName, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ConsigneeAgentName));
			SetValue(row, QuarantineExDocHeaderSchema.QH_ExporterDeclaration, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ExporterDeclaration));
			SetValue(row, QuarantineExDocHeaderSchema.QH_CertificatePrintIndicator, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.CertificatePrintIndicator));
			SetValue(row, QuarantineExDocHeaderSchema.QH_AQISRegion, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ProductionRegion));
			SetValue(row, QuarantineExDocHeaderSchema.QH_SplitHealthCertByContainer, addInfoGroup.AddInfoCollection.GetZBoolValue(Constants.InvoiceHeader.Keys.SplitHealthCertByContainer));
			SetValue(row, QuarantineExDocHeaderSchema.QH_SplitHealthCertByPacker, addInfoGroup.AddInfoCollection.GetZBoolValue(Constants.InvoiceHeader.Keys.SplitHealthCertByPacker));
			SetValue(row, QuarantineExDocHeaderSchema.QH_SplitHealthCertByMarks, addInfoGroup.AddInfoCollection.GetZBoolValue(Constants.InvoiceHeader.Keys.SplitHealthCertByMarks));
			SetValue(row, QuarantineExDocHeaderSchema.QH_AMLCQuota, addInfoGroup.AddInfoCollection.GetZBoolValue(Constants.InvoiceHeader.Keys.AMLCQuota));
			SetValue(row, QuarantineExDocHeaderSchema.QH_QuotaType, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.QuotaType));
			SetValue(row, QuarantineExDocHeaderSchema.QH_ShipsStores, addInfoGroup.AddInfoCollection.GetZBoolValue(Constants.InvoiceHeader.Keys.ShipsStores));
			SetValue(row, QuarantineExDocHeaderSchema.QH_AMLCQuotaYear, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.AMLCQuotaYear));
			SetValue(row, QuarantineExDocHeaderSchema.QH_CertificateRequiredLocation, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.CertificateRequiredLocation));
			SetValue(row, QuarantineExDocHeaderSchema.QH_RN_NKOriginCountry, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ProductSource));
			SetValue(row, QuarantineExDocHeaderSchema.QH_RL_NKBorderInspectionPort, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.BorderInspectionPort));
			SetValue(row, QuarantineExDocHeaderSchema.QH_PackDate, addInfoGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceHeader.Keys.PackDate));
			SetValue(row, QuarantineExDocHeaderSchema.QH_AbsoluteTemperature, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceHeader.Keys.AbsoluteTemperature));
			SetValue(row, QuarantineExDocHeaderSchema.QH_MinimumTemperature, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceHeader.Keys.MinimumTemperature));
			SetValue(row, QuarantineExDocHeaderSchema.QH_MaximumTemperature, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceHeader.Keys.MaximumTemperature));
			SetValue(row, QuarantineExDocHeaderSchema.QH_TemperatureUM, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.TemperatureUnit));
			SetValue(row, QuarantineExDocHeaderSchema.QH_CustomsConsigneeName, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.CustomsConsigneeName));
			SetValue(row, QuarantineExDocHeaderSchema.QH_ExemptionCode, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ExemptionCode));
			SetValue(addInfoRow, AddInfoQuarantineExDocHeaderSchema.ZH_PrintLocation, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.PrintLocation));

			FillRecommendationLetters(quarantineHeader);

			//RFP Indicator Declaration tab
			SetValue(row, QuarantineExDocHeaderSchema.QH_DecOfCompliance, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.DeclarationOfCompliance));
			SetValue(row, QuarantineExDocHeaderSchema.QH_ImportedProductFlag, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ImportedProductFlag));
			SetValue(addInfoRow, AddInfoQuarantineExDocHeaderSchema.ZH_TrueAndCompleteIndicator, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.TrueAndComplete));
			SetValue(row, QuarantineExDocHeaderSchema.QH_ManufacturedTreatedPackagedLabelledInAustralia, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ManufacturedTreatedPackagedLabelledInAustralia));
			SetValue(row, QuarantineExDocHeaderSchema.QH_LegallyImportedFlag, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.LegallyImportedFlag));

			//RFP Inspection Details tab
			SetValue(row, QuarantineExDocHeaderSchema.QH_AuthorisationEstablishment, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.AuthorisationEstablishment));
			SetValue(row, QuarantineExDocHeaderSchema.QH_StorageEstablishment, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.StorageEstablishment));
			SetValue(addInfoRow, AddInfoQuarantineExDocHeaderSchema.ZH_ApprovedCertifier, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ApprovedCertifier));
			SetValue(row, QuarantineExDocHeaderSchema.QH_LotNumber, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.LotNumber));
			SetValue(row, QuarantineExDocHeaderSchema.QH_OriginCatchZone, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.CatchZone));
			SetValue(addInfoRow, AddInfoQuarantineExDocHeaderSchema.ZH_AvAnimalAge, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.AverageAnimalAge));
			SetValue(row, QuarantineExDocHeaderSchema.QH_StartHoldSeal, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.StartHoldSeal));
			SetValue(row, QuarantineExDocHeaderSchema.QH_EndHoldSeal, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.EndHoldSeal));
			SetValue(row, QuarantineExDocHeaderSchema.QH_InspectionRequestedDate, addInfoGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceHeader.Keys.InspectionRequestedDate));
			SetValue(row, QuarantineExDocHeaderSchema.QH_AuthorisedStartDate, addInfoGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceHeader.Keys.AuthorisedStartDate));
			SetValue(row, QuarantineExDocHeaderSchema.QH_AuthorisedEndDate, addInfoGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceHeader.Keys.AuthorisedEndDate));
			SetValue(row, QuarantineExDocHeaderSchema.QH_AuthorisingOfficerID, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.AuthorisingOfficerID));
			SetValue(row, QuarantineExDocHeaderSchema.QH_InspectorComments, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.InspectorComments));
			SetValue(row, QuarantineExDocHeaderSchema.QH_AuthorisationDate, addInfoGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceHeader.Keys.AuthorisationDate));
			SetValue(row, QuarantineExDocHeaderSchema.QH_AuthorisationComments, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.AuthorisationComments));
			SetValue(addInfoRow, AddInfoQuarantineExDocHeaderSchema.ZH_AuthorisationFlag, addInfoGroup.AddInfoCollection.GetZBoolValue(Constants.InvoiceHeader.Keys.AuthorisationFlag));

			FillSupportingInfos(quarantineHeader);
			FillAcknowledgements(quarantineHeader);
			FillCatchZones(quarantineHeader);

			//RFP Forward/ Transfer tab
			SetValue(row, QuarantineExDocHeaderSchema.QH_ForwardeeEDIUserIdentifier, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ForwardeeEDIUserIdentifier));
			SetValue(row, QuarantineExDocHeaderSchema.QH_ForwardStatus, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ForwardStatus));
			SetValue(row, QuarantineExDocHeaderSchema.QH_TransfereeEDIUserIdentifier, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.TransfereeEDIUserIdentifier));
			SetValue(row, QuarantineExDocHeaderSchema.QH_TransfereeExporterNumber, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.TransfereeExporterNumber));
			SetValue(row, QuarantineExDocHeaderSchema.QH_CancelTransferIndicator, addInfoGroup.AddInfoCollection.GetZBoolValue(Constants.InvoiceHeader.Keys.CancelTransferIndicator));

			//RFP Ships Compartments tab
			FillCompartments(quarantineHeader);

			//RFP EU Details
			SetValue(addInfoRow, AddInfoQuarantineExDocHeaderSchema.ZH_ApprovalNumber, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.ApprovalNumber));
			SetValue(addInfoRow, AddInfoQuarantineExDocHeaderSchema.ZH_TransitLocationType, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.TransitLocationType));
			SetValue(row, QuarantineExDocHeaderSchema.QH_EUComments, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.EUComments));
			SetValue(row, QuarantineExDocHeaderSchema.QH_EUTestResultRequired, addInfoGroup.AddInfoCollection.GetZBoolValue(Constants.InvoiceHeader.Keys.EUTestResultRequired));

			//REX Inspection Details tab
			SetValue(row, QuarantineExDocHeaderSchema.QH_LoadingDate, addInfoGroup.AddInfoCollection.GetZDateValue(Constants.InvoiceHeader.Keys.LoadingDate));
			var loadingEstablishmentID = addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.LoadingEstablishment).GetValueOrDefault();
			if (!loadingEstablishmentID.IsEmpty)
			{
				var docAddressesFound = invoice.DocAddresses.FindDocAddressesByType(DocAddressType.AQISLoadingEstablishment);
				if (docAddressesFound.Length == 1)
				{
					var docAddressRow = GetColumnIndexer(docAddressesFound[0]);
					SetValue(docAddressRow, JobDocAddressSchema.E2_GovRegNum, loadingEstablishmentID);
					SetValue(docAddressRow, JobDocAddressSchema.E2_GovRegNumType, OrgCusCode.AUQuarantineCodeTypes.EXDOCEstablishmentNumber);
				}
			}

			return rowList;
		}

		List<IColumnIndexer> PopulateQL()
		{
			var rowList = new List<IColumnIndexer>();
			var invoiceLine = helper.Factory.Load<JobComInvoiceLine>(parentPK);
			var quarantineLine = invoiceLine.QuarantineExDocLine;
			if (invoiceLine == null || !invoiceLine.IsQuarantine)
			{
				logger.LogBoth(LogType.Warning, "Quarantine line in XML does not belong to a quarantine (AQS) declaration.");
				return rowList;
			}

			var rowLine = GetColumnIndexer(quarantineLine);
			rowList.Add(rowLine);
			SetValue(rowLine, QuarantineExDocLineSchema.QL_JI, invoiceLine.PK);

			//RFP Packages tab
			SetValue(rowLine, QuarantineExDocLineSchema.QL_NetQuantity, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceLine.Keys.NetQuantity));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_NetQuantityUnit, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.NetQuantityUnit));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_ImperialNetWeight, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceLine.Keys.ImperialNetWeight));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_ImperialNetWeightUnit, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.ImperialNetWeightUnit));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_GrossMetricWeight, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceLine.Keys.GrossMetricWeight));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_GrossMetricWeightUnit, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.GrossMetricWeightUnit));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_ShippingMarks, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.ShippingMarks));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_BatchCode, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.BatchCode));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_OuterPackCount, addInfoGroup.AddInfoCollection.GetZIntValue(Constants.InvoiceLine.Keys.OuterPackCount));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_OuterPackType, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.OuterPackType));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_OuterPackAccuracy, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.OuterPackAccuracy));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_OuterPackWeight, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceLine.Keys.OuterPackWeight));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_OuterPackWeightUnit, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.OuterPackWeightUnit));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_IntermediatePackCount, addInfoGroup.AddInfoCollection.GetZIntValue(Constants.InvoiceLine.Keys.IntermediatePackCount));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_IntermediatePackType, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.IntermediatePackType));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_IntermediatePackAccuracy, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.IntermediatePackAccuracy));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_IntermediatePackWeight, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceLine.Keys.IntermediatePackWeight));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_IntermediatePackWeightUnit, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.IntermediatePackWeightUnit));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_InnerPackCount, addInfoGroup.AddInfoCollection.GetZIntValue(Constants.InvoiceLine.Keys.InnerPackCount));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_InnerPackType, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.InnerPackType));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_InnerPackAccuracy, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.InnerPackAccuracy));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_InnerPackWeight, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceLine.Keys.InnerPackWeight));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_InnerPackWeightUnit, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.InnerPackWeightUnit));

			//RFP Details tab
			SetValue(rowLine, QuarantineExDocLineSchema.QL_ProductType, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.ProductType));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_Category, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.Category));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_SupplimentaryCode, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.SupplementaryCode));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_PackType, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.PackType));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_PreservationType, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.PreservationType));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_CutCode, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.CutCode));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_ProductDescriptionLocationQualifier, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.ProductDescriptionLocationQualifier));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_ProductDescriptionQualityQualifier, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.ProductDescriptionQualityQualifier));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_NatureOfCommodity, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.NatureOfCommodity));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_TreatmentType, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.TreatmentType));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_AddtionalDeclarationComments, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.AdditionalDeclarationComments));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_ClientLineItemID, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.ClientLineItemID));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_FinalConsumer, addInfoGroup.AddInfoCollection.GetZBoolValue(Constants.InvoiceLine.Keys.FinalConsumer));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_CombinedNomenclature, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.CombinedNomenclature));

			//RFP Process tab
			SetValue(rowLine, QuarantineExDocLineSchema.QL_UseByStart, addInfoGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceLine.Keys.UseByStart));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_UseByEnd, addInfoGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceLine.Keys.UseByEnd));

			//RFP Certificates tab
			SetValue(rowLine, QuarantineExDocLineSchema.QL_MeatInspectionDescription, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.InspectionDescription));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_AddtionalProductDescription, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.AdditionalProductDescription));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_CommercialProductDescription, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.CommercialProductDescription));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_HealthCertificateDescription, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.HealthCertificateDescription));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_HCFormatRequested, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.FormatRequested));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_ExtraCertificate, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.ExtraFormatRequested));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_HCFormatAllocated, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.FormatAllocated));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_HCNumber, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.CertificateNumber));

			//RFP Analysis tab
			SetValue(rowLine, QuarantineExDocLineSchema.QL_DrainedWeight, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceLine.Keys.DrainedWeight));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_DrainedWeightUnit, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.DrainedWeightUnit));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_FishWaterIndicator, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.FishWaterIndicator));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_CatchStartDate, addInfoGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceLine.Keys.CatchStartDate));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_CatchEndDate, addInfoGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceLine.Keys.CatchEndDate));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_PercentOfMilkProtein, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceLine.Keys.PercentOfMilkProtein));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_TotalWeightOfMilkProteinInMixtures, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceLine.Keys.TotalWeightOfMilkProteinInMixtures));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_PercentOfMilkFat, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceLine.Keys.PercentOfMilkFat));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_TotalWeightOfMilkFatInMixtures, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceLine.Keys.TotalWeightOfMilkFatInMixtures));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_IMA1SerialNumber, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.IMA1SerialNumber));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_IMA1QuotaYear, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.IMA1QuotaYear));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_IMA1ProductDesciption, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.IMA1ProductDescription));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_GrowerNumber, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.GrowerNumber));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_SaltingDate, addInfoGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceLine.Keys.SaltingDate));

			//RFP Meat tab
			SetValue(rowLine, QuarantineExDocLineSchema.QL_ChemicalLeanPercentage, addInfoGroup.AddInfoCollection.GetZIntValue(Constants.InvoiceLine.Keys.ChemicalLeanPercentage));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_BeefVealWeightAmount, addInfoGroup.AddInfoCollection.GetZDecimalValue(Constants.InvoiceLine.Keys.BeefVealWeightAmount));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_LabelApprovalNumber, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.LabelApprovalNumber));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_LabelApprovalIndicator, addInfoGroup.AddInfoCollection.GetZBoolValue(Constants.InvoiceLine.Keys.LabelApprovalIndicator));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_UngradedProductIndicator, addInfoGroup.AddInfoCollection.GetZBoolValue(Constants.InvoiceLine.Keys.UngradedProductIndicator));

			//RFP Statements tab
			SetValue(rowLine, QuarantineExDocLineSchema.QL_StatementNumber1, addInfoGroup.AddInfoCollection.GetZShortValue(Constants.InvoiceLine.Keys.StatementNumber1));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_StatementNumber2, addInfoGroup.AddInfoCollection.GetZShortValue(Constants.InvoiceLine.Keys.StatementNumber2));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_StatementNumber3, addInfoGroup.AddInfoCollection.GetZShortValue(Constants.InvoiceLine.Keys.StatementNumber3));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_StatementNumber4, addInfoGroup.AddInfoCollection.GetZShortValue(Constants.InvoiceLine.Keys.StatementNumber4));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_StatementNumber5, addInfoGroup.AddInfoCollection.GetZShortValue(Constants.InvoiceLine.Keys.StatementNumber5));
			SetValue(rowLine, QuarantineExDocLineSchema.QL_StatementText, addInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.StatementText));

			var processCollection = quarantineLine.Processes;
			processCollection.RemoveAll();

			var dec = invoiceLine.Declaration;
			var organisationAddressHelper = new OrganizationAddressReaderHelper<JobDeclaration>(logger, factory, dec, DocAddressType.AQISProcessingEstablishment);

			foreach (var subGroup in addInfoGroup.AddInfoGroupCollection.Where(x => x.Type?.Code == (ZString?)Constants.InvoiceLine.Codes.NPD))
			{
				var newProcess = processCollection.AddNew();
				var rowEE = GetColumnIndexer(newProcess);
				rowList.Add(rowEE);
				SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_QL, quarantineLine.PK);
				SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_ProcessingType, subGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.ProcessingType));
				SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_AuthorisationEstablishmentID, subGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.EstablishmentID));
				SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_EstablishmentIndicator, subGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.EstablishmentIndicator));
				SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_StartDate, subGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceLine.Keys.StartDate));
				SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_EndDate, subGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceLine.Keys.EndDate));
				SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_Depuration, subGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceLine.Keys.DepurationDate));
				SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_HarvestArea, subGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.HarvestArea));
				SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_InspectionRequestedDate, subGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceLine.Keys.InspectionRequestedDate));
				SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_LeaseNumber, subGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.LeaseNumber));
				SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_TreatmentCode, subGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.TreatmentCode));
				SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_TreatmentInfo, subGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceLine.Keys.TreatmentInformation));

				var addressPK = organisationAddressHelper.GetOrCreateDocAddress(subGroup.OrganizationAddressCollection, dec.DocAddresses.Cast<JobDocAddress>(), FillAQISProcessingEstablishment, compareMobile: false);
				if (addressPK != ZGuid.Empty)
				{
					SetValue(rowEE, QuarantineExDocEstablishmentAndTimeSchema.EE_E2_Address, addressPK);
				}
			}
			return rowList;
		}

		JobDocAddress FillAQISProcessingEstablishment(BusinessObject bObject, DocAddressType docAddressType, OrganizationAddress orgAddressDataObject)
		{
			var declaration = bObject as JobDeclaration;
			var typeCode = DocAddressTypes.GetCode(factory.BOFactory, docAddressType);
			var lastDocAddress = FindLastDocAddressesByType(typeCode, declaration.PK);
			var max = lastDocAddress == null ? 0 : lastDocAddress.E2_AddressSequence + 1;

			var newAddress = factory.New<JobDocAddress>();
			newAddress.E2_ParentID = declaration.PK;
			newAddress.E2_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			newAddress.E2_AddressType = typeCode;
			newAddress.E2_AddressSequence = (ZByte)max;

			var reader = new OrganisationDataObjectReader(orgAddressDataObject, logger, factory);
			var orgAddress = reader.GetMatched();
			if (orgAddress != null && orgAddress.OA_OH == OrgHeader.UnmatchedOrganisationPK)
			{
				orgAddress = null;
				var docAddresRow = GetColumnIndexer(newAddress);
				SetValue(docAddresRow, JobDocAddressSchema.E2_OA_Address, ZGuid.Empty);
				SetValue(docAddresRow, JobDocAddressSchema.E2_AddressOverride, ZBool.True);
			}
			reader.PopulateJobDocAddress(orgAddress, newAddress);
			return newAddress;
		}

		public JobDocAddress FindLastDocAddressesByType(ZString typeCode, ZGuid declarationPK)
		{
			var query = new ZQuery(JobDocAddressSchema.E2_AddressType, typeCode);
			query.AddToFilter(JobDocAddressSchema.E2_ParentID, declarationPK);
			query.FetchOnlyFromLocalCache = true;
			var docAddress = factory.Load<JobDocAddress>(query);
			var enumerable = docAddress.Where(x => !x.IsDeleted && x.E2_AddressType == typeCode);

			return enumerable.OrderByDescending(x => x.E2_AddressSequence).FirstOrDefault();
		}

		void FillRecommendationLetters(QuarantineExDocHeader header)
		{
			header.RecommendationLetters.RemoveAndDeleteAll();
			if (addInfoGroup.AddInfoGroupCollection != null)
			{
				foreach (var subAddInfoGroup in addInfoGroup.AddInfoGroupCollection.Where(x => x.Type?.Code == (ZString?)Constants.InvoiceHeader.Codes.NRL))
				{
					var letter = header.RecommendationLetters.AddNew();
					var letterAddInfoRow = GetColumnIndexer(letter.Data);
					if (subAddInfoGroup.AddInfoCollection != null)
					{
						SetValue(letterAddInfoRow, AURecommendationLetterAddInfoSchema.ZA_LetterNumber, subAddInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.RecommendationLetterNumber));
						SetValue(letterAddInfoRow, AURecommendationLetterAddInfoSchema.ZA_LetterDate, subAddInfoGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceHeader.Keys.RecommendationLetterDate));
					}
				}
			}
		}

		void FillSupportingInfos(QuarantineExDocHeader header)
		{
			header.SupportingInfos.RemoveAndDeleteAll();
			if (addInfoGroup.CustomsReferenceCollection != null)
			{
				foreach (var customsReference in addInfoGroup.CustomsReferenceCollection.Where(x => x.Type?.Code == (ZString?)QuarantineSupportingInfoCollection.DeclarationConstant))
				{
					var supportingInfoRow = GetColumnIndexer(header.SupportingInfos.AddNew());
					SetValue(supportingInfoRow, CusSupportingInfoSchema.CSI_Description, customsReference.Reference);
					if (customsReference.Order.HasValue && customsReference.Order.Value > 0)
					{
						SetValue(supportingInfoRow, CusSupportingInfoSchema.CSI_LineNo, customsReference.Order.Value);
					}
				}
			}
		}

		void FillAcknowledgements(QuarantineExDocHeader header)
		{
			header.Acknowledgements.RemoveAndDeleteAll();
			if (addInfoGroup.CustomsReferenceCollection != null)
			{
				foreach (var customsReference in addInfoGroup.CustomsReferenceCollection.Where(x => x.Type?.Code == (ZString?)QuarantineExDocRexAcknowledgement.AcknowledgementCode))
				{
					var acknowledgementRow = GetColumnIndexer(header.Acknowledgements.AddNew());
					SetValue(acknowledgementRow, CusCodeDataSchema.CY_Data, customsReference.Reference);
				}
			}
		}

		void FillCompartments(QuarantineExDocHeader header)
		{
			header.Compartments.RemoveAndDeleteAll();
			if (addInfoGroup.AddInfoGroupCollection != null)
			{
				foreach (var subAddInfoGroup in addInfoGroup.AddInfoGroupCollection.Where(x => x.Type?.Code == (ZString?)Constants.InvoiceHeader.Codes.NSI))
				{
					var compartmentRow = GetColumnIndexer(header.Compartments.AddNew());
					if (subAddInfoGroup.AddInfoCollection != null)
					{
						SetValue(compartmentRow, QuarantineExDocShipsCompartmentSchema.QC_Compartments, subAddInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.Compartments));
						SetValue(compartmentRow, QuarantineExDocShipsCompartmentSchema.QC_RL_NKInspectionPort, subAddInfoGroup.AddInfoCollection.GetZStringValue(Constants.InvoiceHeader.Keys.InspectionPort));
						SetValue(compartmentRow, QuarantineExDocShipsCompartmentSchema.QC_InspectionDate, subAddInfoGroup.AddInfoCollection.GetZDateTimeValue(Constants.InvoiceHeader.Keys.InspectionDate));
					}
				}
			}
		}

		void FillCatchZones(QuarantineExDocHeader header)
		{
			header.NexDocCatchZones.RemoveAndDeleteAll();
			if (addInfoGroup.CustomsReferenceCollection != null)
			{
				foreach (var customsReference in addInfoGroup.CustomsReferenceCollection.Where(x => x.Type?.Code == (ZString?)CusCodeDataTypeList.Codes.NEXDOCSCatchZone))
				{
					var catchZoneRow = GetColumnIndexer(header.NexDocCatchZones.AddNew());
					SetValue(catchZoneRow, CusCodeDataSchema.CY_Data, customsReference.Reference);
				}
			}
		}

		readonly UniversalDataObjectReaderHelper helper;
		readonly ZGuid parentPK;
		readonly AddInfoGroup addInfoGroup;
	}
}
