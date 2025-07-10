using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.DataTransfer.Universal
{
	public class CommercialInvoiceHeaderDataObjectReader : CommercialInvoiceHeaderDataObjectReader<JobComInvoiceGroupHeader>
	{
		public CommercialInvoiceHeaderDataObjectReader(CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, Customs.DataTransfer.Universal.UniversalDataObjectReaderHelper helper, JobComInvoiceGroupHeader groupHeader, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null, Type invoiceType = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader, invoiceType: invoiceType)
		{
		}

		void ImportOldSupportingInformationSchemaDataIfNeeded<T>(ZGuid parentPK, ZString parentTableCode, bool isParentInDatabase, T dataObject)
			where T : IAddInfoGroupCollectionParent, ICustomsSupportingInformationCollectionParent, new()
		{
			if (dataObject.CustomsSupportingInformationCollection == null && dataObject.AddInfoGroupCollection != null && helper.IsSourceAndTargetCountrySame)
			{
				var dummyData = JobDeclarationDataObjectReader.CreateDummyShipmentWithSupportingInfo<T>(dataObject.AddInfoGroupCollection, DefaultDataObjectWriterStrategy.Instance);
				if (dummyData.CustomsSupportingInformationCollection.Count > 0)
				{
					new CustomsSupportingInformationCollectionDataObjectReader(logger, helper).ReadIntoDataRows(parentPK, parentTableCode, isParentInDatabase, dummyData);
				}
			}
		}

		protected override void FillCountrySpecificDetails(CommercialInvoiceLine invoiceLineData, IColumnIndexer invoiceLineRow, Dictionary<string, ValueSetter> delaySetters, Customs.Business.BaseJobComInvoiceLine parentInvoiceLine, bool invoiceLineIsInDatabase)
		{
			base.FillCountrySpecificDetails(invoiceLineData, invoiceLineRow, delaySetters, parentInvoiceLine, invoiceLineIsInDatabase);
			var invoiceLinePK = invoiceLineRow.GetValue(JobComInvoiceLineSchema.PK);
			ImportOldSupportingInformationSchemaDataIfNeeded(invoiceLinePK, JobComInvoiceLineSchema.Constants.Prefix, invoiceLineIsInDatabase, invoiceLineData);
			ImportOldTaxSchemaDataIfNeeded(invoiceLinePK, JobComInvoiceLineSchema.Constants.Prefix, invoiceLineIsInDatabase, invoiceLineData);

			new CustomsReferenceCollectionDataObjectReader(logger, helper).ReadIntoDataRows(invoiceLinePK, JobComInvoiceLineSchema.Constants.Prefix, invoiceLineIsInDatabase, invoiceLineData);
		}

		protected override void FillCountrySpecificAddInfoDetails(Customs.Business.BaseJobComInvoiceLine invoiceLine)
		{
			base.FillCountrySpecificAddInfoDetails(invoiceLine);
			if (invoiceLine is JobComInvoiceLine line && line.Declaration is JobDeclaration declaration && declaration.Configuration.InvoiceLineConfiguration.OrganizationsSupport(declaration))
			{
				CreateNewCusSupplyChainActorReferencesDataObjectReaderForInvoiceLine().Read(line.CusSupplyChainActorReferences, CurrentInvoiceLineDataCustomsReferenceGroupByType);
			}
		}

		protected virtual CusSupplyChainActorReferencesDataObjectReader CreateNewCusSupplyChainActorReferencesDataObjectReaderForInvoiceLine() => new CusSupplyChainActorReferencesDataObjectReader(logger, factory);

		void ImportOldTaxSchemaDataIfNeeded(ZGuid invoiceLinePK, ZString tablePrefix, bool invoiceLineIsInDatabase, CommercialInvoiceLine realDataObject)
		{
			if (Registry.EUCustomsDataRegistry.Instance.AllowExportAndImportOfOldStyleCusAddInfoRowsInUniversalAsWellAsNewStyleTaxOrFee.Value && realDataObject.TaxOrFeeCollection == null && realDataObject.AddInfoGroupCollection != null)
			{
				var taxOrFeeCollection = new List<TaxOrFee>();
				foreach (var addInfoGroup in realDataObject.AddInfoGroupCollection)
				{
					if (addInfoGroup.Type.GetCodeAsUpperCase() == Constants.OldCusAddInfoTypes.Tax.TaxAddInfoTypeCodeGTX)  // "GTX"
					{
						var taxOrFee = CreateNewDummyTaxOrFeeFromAddInfo(addInfoGroup);
						if (taxOrFee != null)
						{
							taxOrFeeCollection.Add(taxOrFee);
						}
					}
				}
				if (taxOrFeeCollection.Count > 0)
				{
					new TaxOrFeeCollectionDataObjectReader(logger, helper).ReadIntoDataRows(invoiceLinePK, tablePrefix, invoiceLineIsInDatabase, new CommercialInvoiceLine() { TaxOrFeeCollection = taxOrFeeCollection });
				}
			}
		}

		TaxOrFee CreateNewDummyTaxOrFeeFromAddInfo(AddInfoGroup addInfoGroup)
		{
			TaxOrFee result = null;
			var addInfoCollection = addInfoGroup.AddInfoCollection;
			if (addInfoCollection != null)
			{
				var tty = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.Tax.Fields.Tty).GetValueOrDefault().SubstringSafe(0, JobComInvoiceLineTaxSchema.JLT_Type.MaxLength);
				if (!tty.IsEmpty) // Database constraint does not allow empty
				{
					result = new TaxOrFee();
					result.Type = new CodeDescriptionPair6Char() { Code = tty };
					result.Amount = addInfoCollection.GetZDecimalValue(Constants.OldCusAddInfoTypes.Tax.Fields.Amount).Value;
					result.BaseValue = addInfoCollection.GetZDecimalValue(Constants.OldCusAddInfoTypes.Tax.Fields.BaseAmount);
					result.BaseQuantity = addInfoCollection.GetZDecimalValue(Constants.OldCusAddInfoTypes.Tax.Fields.BaseQuantity);
					result.MethodOfPayment = new CodeDescriptionPair() { Code = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.Tax.Fields.MethodOfPayment) };
					result.RateReasonOverride = new CodeDescriptionPair() { Code = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.Tax.Fields.RateOverride) };
					var rateDuty = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.Tax.Fields.RateDuty);
					var rateSuspension = addInfoCollection.GetZStringValue(Constants.OldCusAddInfoTypes.Tax.Fields.RateSuspension);
					var moc = rateDuty.GetValueOrDefault().Left(3).PadRight(3) + rateSuspension.GetValueOrDefault().Left(1).PadRight(1);
					result.MethodOfCalculation = new CodeDescriptionPair4Char() { Code = moc };
				}
			}
			return result;
		}

		protected override void ImportCountrySpecificRelatedData(Customs.Business.BaseJobComInvoiceHeader invoiceBO, Dictionary<string, ValueSetter> delaySetters)
		{
			base.ImportCountrySpecificRelatedData(invoiceBO, delaySetters);
			ImportOldSupportingInformationSchemaDataIfNeeded(invoiceBO.PK, invoiceBO.TablePrefix, invoiceBO.IsInDatabase, dataObject);
		}

		protected override void PopulateRelatedIndicator(ICodeDataObject relatedIndicatorDataObject, IColumnIndexer invoiceRow, Dictionary<string, ValueSetter> delaySetters)
		{
			var effectiveRelatedIndicator = GetEffectiveRelatedIndicator(relatedIndicatorDataObject);
			base.PopulateRelatedIndicator(effectiveRelatedIndicator, invoiceRow, delaySetters);
		}

		ICodeDataObject GetEffectiveRelatedIndicator(ICodeDataObject relatedIndicatorDataObject)
		{
			var effectiveRelatedIndicatorCode = relatedIndicatorDataObject.GetCodeAsUpperCase();
			if (effectiveRelatedIndicatorCode != RelatedIndicatorList.Codes.Yes)
			{
				effectiveRelatedIndicatorCode = RelatedIndicatorList.Codes.No;
			}
			return new CodeDescriptionPair() { Code = effectiveRelatedIndicatorCode };
		}
	}
}
