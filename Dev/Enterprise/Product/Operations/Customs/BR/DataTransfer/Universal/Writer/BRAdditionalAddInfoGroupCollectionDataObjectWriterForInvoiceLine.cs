using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.BR.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;

namespace Enterprise.Customs.BR.DataTransfer.Universal
{
	public class BRAdditionalAddInfoGroupCollectionDataObjectWriterForInvoiceLine : IAdditionalAddInfoGroupCollectionDataObjectWriter
	{
		public BRAdditionalAddInfoGroupCollectionDataObjectWriterForInvoiceLine(JobComInvoiceLine invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}
		readonly JobComInvoiceLine invoiceLine;

		#region IAdditionalAddInfoGroupCollectionDataObjectWriter Members

		public IEnumerable<AddInfoGroup> CreateCollection()
		{
			foreach (SuspensionDrawback suspensionDrawback in invoiceLine.SuspensionDrawbackCollection)
			{
				yield return new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = Constants.AddInfoKeys.AddInfoGroupTypeCodes.SuspensionDrawback, Description = CusSupportingInfoTypeList.Descriptions.SuspensionDrawback },
					AddInfoCollection = new List<AddInfo>(new[]
					{
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.CNPJBeneficiary, Value = suspensionDrawback.CSI_ReferenceNumber },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.CANumber, Value = suspensionDrawback.CSI_ReferenceNumber2 },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.CALineItemNumber, Value = suspensionDrawback.CSI_LineNo.ToString() },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.QuantityUsed, Value =  suspensionDrawback.CSI_Quantity.ToString() },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.CATypeOfConcessionAct, Value = suspensionDrawback.CSI_SubType },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.TariffOfTheCAImportItem, Value = suspensionDrawback.CSI_Tariff },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawback.ForeignExchangeHedgedVMLE, Value = suspensionDrawback.CSI_Value.ToString() },
					}),
					AddInfoGroupCollection = CreateAddInfoGroupCollectionForSuspensionDrawback(suspensionDrawback).ToList()
				};
			}
		}

		IEnumerable<AddInfoGroup> CreateAddInfoGroupCollectionForSuspensionDrawback(SuspensionDrawback suspensionDrawback)
		{
			foreach (SuspensionDrawbackInvoice drawbackInvoice in suspensionDrawback.SuspensionDrawbackInvoiceCollection)
			{
				yield return new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = Constants.AddInfoKeys.AddInfoGroupTypeCodes.SuspensionDrawbackInvoice, Description = CusSupportingInfoTypeList.Descriptions.SuspensionDrawbackInvoice },
					AddInfoCollection = new List<AddInfo>(new[]
					{
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackInvoice.InvoiceNumber, Value = drawbackInvoice.CSI_ReferenceNumber },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackInvoice.Quantity, Value = drawbackInvoice.CSI_Quantity.ToString() },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackInvoice.TradingCurrencyValue, Value = drawbackInvoice.CSI_Value.ToString() },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackInvoice.Date, Value = drawbackInvoice.CSI_DateOfIssue.ToISO8601String() }
					}),
				};
			}

			foreach (SuspensionDrawbackImportEntryDocument importEntryDocument in suspensionDrawback.SuspensionDrawbackImportEntryDocumentCollection)
			{
				yield return new AddInfoGroup()
				{
					Type = new CodeDescriptionPair() { Code = Constants.AddInfoKeys.AddInfoGroupTypeCodes.SuspensionDrawbackImportEntryDocument, Description = CusSupportingInfoTypeList.Descriptions.SuspensionDrawbackImportEntryDocument },
					AddInfoCollection = new List<AddInfo>(new[]
					{
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.ImportEntry, Value = importEntryDocument.CSI_ReferenceNumber },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.Quantity, Value = importEntryDocument.CSI_Quantity.ToString() },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.Value, Value = importEntryDocument.CSI_Value.ToString() },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.Category, Value = importEntryDocument.CSI_SubType },
						new AddInfo() { Key = Constants.AddInfoKeys.SuspensionDrawbackImportEntryDocument.EntryLine, Value = importEntryDocument.CSI_LineNo.ToString() }
					}),
				};
			}
		}

		#endregion
	}
}
