using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using IAdditionalLineTariffDetailParent = Enterprise.Customs.Business.IAdditionalLineTariffDetailParent;
namespace Enterprise.Customs.BR.DataTransfer.Universal
{
	public class BRInvoiceHeaderDataObjectReader : CommercialInvoiceHeaderDataObjectReader<BaseJobComInvoiceGroupHeader>
	{
		public BRInvoiceHeaderDataObjectReader(BaseJobComInvoiceGroupHeader groupHeader, CommercialInvoiceHeader invoiceDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, Shipment topLevelObject = null, ILandedCostDataReader landedCostDataReader = null)
			: base(invoiceDataObject, logger, helper, groupHeader, topLevelObject, landedCostDataReader)
		{
		}

		protected override void FillCustomizedFields(CommercialInvoiceLine invoiceLineData, BaseJobComInvoiceLine invoiceLine)
		{
			base.FillCustomizedFields(invoiceLineData, invoiceLine);
			if (invoiceLine is JobComInvoiceLine brInvoiceLine)
			{
				FillCustomsReferenceCollection(invoiceLineData, brInvoiceLine);
				FillAddInfoGroupCollection(invoiceLineData, brInvoiceLine);
			}
		}

		void FillAddInfoGroupCollection(CommercialInvoiceLine invoiceLineData, JobComInvoiceLine brInvoiceLine)
		{
			FillSuspensionDrawback(invoiceLineData, brInvoiceLine);
		}

		void FillSuspensionDrawback(CommercialInvoiceLine invoiceLineData, JobComInvoiceLine brInvoiceLine)
		{
			invoiceLineData.AddInfoGroupCollection?.ForEach(suspensionInfoGroup =>
			{
				var suspensionDrawback = brInvoiceLine.SuspensionDrawbackCollection.AddNew();
				var suspensionDrawbackRow = GetColumnIndexer(suspensionDrawback);
				var subCollection = suspensionInfoGroup.AddInfoCollection;
				SetValue(suspensionDrawbackRow, CusSupportingInfoSchema.CSI_SubType, subCollection.GetZStringValue(Constants.AddInfoKeys.SuspensionDrawback.CATypeOfConcessionAct));
				SetValue(suspensionDrawbackRow, CusSupportingInfoSchema.CSI_ReferenceNumber, subCollection.GetZStringValue(Constants.AddInfoKeys.SuspensionDrawback.CNPJBeneficiary));
				SetValue(suspensionDrawbackRow, CusSupportingInfoSchema.CSI_ReferenceNumber2, subCollection.GetZStringValue(Constants.AddInfoKeys.SuspensionDrawback.CANumber));
				SetValue(suspensionDrawbackRow, CusSupportingInfoSchema.CSI_LineNo, subCollection.GetZIntValue(Constants.AddInfoKeys.SuspensionDrawback.CALineItemNumber));
				SetValue(suspensionDrawbackRow, CusSupportingInfoSchema.CSI_Quantity, subCollection.GetZDecimalValue(Constants.AddInfoKeys.SuspensionDrawback.QuantityUsed));
				SetValue(suspensionDrawbackRow, CusSupportingInfoSchema.CSI_Tariff, subCollection.GetZStringValue(Constants.AddInfoKeys.SuspensionDrawback.TariffOfTheCAImportItem));
				SetValue(suspensionDrawbackRow, CusSupportingInfoSchema.CSI_Value, subCollection.GetZDecimalValue(Constants.AddInfoKeys.SuspensionDrawback.ForeignExchangeHedgedVMLE));

				var suspensionDrawbackInvoiceCollection = suspensionInfoGroup?.AddInfoGroupCollection?.Where(info => info.Type.GetCodeAsUpperCase() == Constants.AddInfoKeys.AddInfoGroupTypeCodes.SuspensionDrawbackInvoice);
				suspensionDrawbackInvoiceCollection.ForEach(addInfo =>
				{
					var suspensionDrawbackInvoice = suspensionDrawback.SuspensionDrawbackInvoiceCollection.AddNew();
					var suspensionDrawbackInvoiceRow = GetColumnIndexer(suspensionDrawbackInvoice);
					var subCollectionInvoice = addInfo.AddInfoCollection;

					SetValue(suspensionDrawbackInvoiceRow, CusSupportingInfoSchema.CSI_ReferenceNumber, subCollectionInvoice.GetZStringValue(Constants.AddInfoKeys.SuspensionDrawbackInvoice.InvoiceNumber));
					SetValue(suspensionDrawbackInvoiceRow, CusSupportingInfoSchema.CSI_Quantity, subCollectionInvoice.GetZDecimalValue(Constants.AddInfoKeys.SuspensionDrawbackInvoice.Quantity));
					SetValue(suspensionDrawbackInvoiceRow, CusSupportingInfoSchema.CSI_Value, subCollectionInvoice.GetZDecimalValue(Constants.AddInfoKeys.SuspensionDrawbackInvoice.TradingCurrencyValue));
					SetValue(suspensionDrawbackInvoiceRow, CusSupportingInfoSchema.CSI_DateOfIssue, subCollectionInvoice.GetZDateTimeValue(Constants.AddInfoKeys.SuspensionDrawbackInvoice.Date));
				});

				var suspensionDrawbackImportEntryDocumentCollection = suspensionInfoGroup?.AddInfoGroupCollection?.Where(info => info.Type.GetCodeAsUpperCase() == Constants.AddInfoKeys.AddInfoGroupTypeCodes.SuspensionDrawbackImportEntryDocument);
				suspensionDrawbackImportEntryDocumentCollection.ForEach(addInfo =>
				{
					var suspensionDrawbackImportEntryDocument = suspensionDrawback.SuspensionDrawbackImportEntryDocumentCollection.AddNew();
					var suspensionDrawbackImportEntryDocumentRow = GetColumnIndexer(suspensionDrawbackImportEntryDocument);
					var subCollectionImportEntryDocument = addInfo.AddInfoCollection;

					SetValue(suspensionDrawbackImportEntryDocumentRow, CusSupportingInfoSchema.CSI_ReferenceNumber, subCollectionImportEntryDocument.GetZStringValue(Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.ImportEntry));
					SetValue(suspensionDrawbackImportEntryDocumentRow, CusSupportingInfoSchema.CSI_Quantity, subCollectionImportEntryDocument.GetZDecimalValue(Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.Quantity));
					SetValue(suspensionDrawbackImportEntryDocumentRow, CusSupportingInfoSchema.CSI_Value, subCollectionImportEntryDocument.GetZDecimalValue(Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.Value));
					SetValue(suspensionDrawbackImportEntryDocumentRow, CusSupportingInfoSchema.CSI_SubType, subCollectionImportEntryDocument.GetZStringValue(Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.Category));
					SetValue(suspensionDrawbackImportEntryDocumentRow, CusSupportingInfoSchema.CSI_LineNo, subCollectionImportEntryDocument.GetZIntValue(Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.EntryLine));
				});
			});
		}

		void FillCustomsReferenceCollection(CommercialInvoiceLine invoiceLineData, JobComInvoiceLine brInvoiceLine)
		{
			FillJobComInvLineCollection<BusinessObject>(brInvoiceLine.LPCOJobComInvLineRefsCollection, Constants.JobComInvLineRefsType.Codes.LPCO, invoiceLineData);
		}

		void FillJobComInvLineCollection<T>(IBusinessObjectCollection collection, string type, CommercialInvoiceLine invoiceLineData) where T : BusinessObject
		{
			var numbers = invoiceLineData.CustomsReferenceCollection?.Where(reference => reference.Type.GetCodeAsUpperCase() == type);
			if (numbers?.Any() ?? false)
			{
				foreach (var number in numbers)
				{
					var reference = number.Reference.GetValueOrDefault();
					if (!reference.IsEmpty)
					{
						var referenceRow = GetColumnIndexer(collection.AddNew());
						SetValue(referenceRow, JobComInvLineRefsSchema.JG_ReferenceNumber, reference);
					}
				}
			}
		}

		protected override void FillCountrySpecificDetails(CommercialInvoiceLine invoiceLineData, IColumnIndexer invoiceLineRow, Dictionary<string, ValueSetter> delaySetters, BaseJobComInvoiceLine parentInvoiceLine, bool invoiceLineIsInDatabase)
		{
			base.FillCountrySpecificDetails(invoiceLineData, invoiceLineRow, delaySetters, parentInvoiceLine, invoiceLineIsInDatabase);

			if (invoiceLineData.AddInfoCollection != null)
			{
				var complementaryAddInfo = invoiceLineData.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.InvoiceLine.ComplementaryDescriptionExport, logger);
				if (complementaryAddInfo.HasValue && !complementaryAddInfo.Value.IsEmpty)
				{
					var invoiceLinePk = invoiceLineRow.GetValue(JobComInvoiceLineSchema.PK);
					var noteRow = GetColumnIndexer(helper.LoadOrCreateStmNoteForReaderUpdate(invoiceLinePk, JobComInvoiceLineSchema.Constants.TableName, invoiceLineIsInDatabase, PredefinedNoteTypes.Instance.BRComplementaryDescription.Description));
					SetValue(noteRow, StmNoteSchema.ST_NoteText, complementaryAddInfo.Value);
				}
			}
		}

		protected override AdditionalLineTariffDetailDataObjectReader CreateAdditionalLineTariffDetailDataObjectReader(AdditionalLineTariffDetail dataObject, IXmlImportLogger logger, UniversalObjectFactory factory, UniversalDataObjectReaderHelper helper, IAdditionalLineTariffDetailParent parent)
		{
			return new BRAdditionalLineTariffDetailDataObjectReader(dataObject, logger, factory, helper, parent);
		}
	}
}
