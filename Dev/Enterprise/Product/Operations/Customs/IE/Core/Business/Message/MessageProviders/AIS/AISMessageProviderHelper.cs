using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IE.Business.Declaration;
using ValuationIndicatorCodeListHelper = Enterprise.Customs.EU.Business.ValuationIndicatorCodeListHelper;

namespace Enterprise.Customs.IE.Business.AIS
{
	public static class AISMessageProviderHelper
	{
		public static IReadOnlyCollection<IMTransportEquipment> GetIM415TransportEquipments(CusEntryHeader entryHeader)
		{
			var result = new List<IMTransportEquipment>();

			if (entryHeader != null)
			{
				var (containers, _) = entryHeader.GetContainerOrEquipmentToEntryLineMapping();
				if (containers.Count > 0)
				{
					result.AddRange(
						containers.Select(kvp => (ContainerNumber: kvp.Key.CO_ContainerNumber, LineNumbers: kvp.Value.Select(entryLine => entryLine.CL_LineNumber).OrderBy(lineNumber => lineNumber)))
						.OrderBy(x => x.ContainerNumber)
						.Select(item => new TransportEquipmentProvider(item.ContainerNumber, item.LineNumbers.Select(lineNumber => lineNumber.ToString()).ToArray())));
				}
			}

			return result;
		}

		public static Customs.Business.MergeKey ToMergeKey(this BusinessObject @object, string[] keys)
		{
			var result = new Customs.Business.MergeKey();
			foreach (var key in keys)
			{
				result.Add((IZType)@object[key]);
			}
			return result;
		}

		public static void AggregateDocuments<TDocumentBiz, TDocument>(this IDocumentAggregationHeader<TDocumentBiz, TDocument> headerProvider)
			where TDocumentBiz : BusinessObject
		{
			var documentKeys = headerProvider.GetDocumentKeys();
			var headerDocuments = headerProvider.GetDocumentObjects().ToDictionary(documentObj => documentObj.ToMergeKey(documentKeys), documentObj => documentObj);

			var childProviders = headerProvider.GetItemProviders().Select(itemProvider => (
				Provider: itemProvider,
				MergeKeys: itemProvider.GetDocumentObjects().Select(document => document.ToMergeKey(documentKeys)).ToHashSet(),
				Documents: itemProvider.GetDocumentObjects().ToHashSet())
			).ToArray();

			foreach (var childProvider in childProviders)
			{
				foreach (var childDocument in childProvider.Documents.ToArray())
				{
					var childDocumentMergeKey = childDocument.ToMergeKey(documentKeys);

					if (childProviders.All(child => child.MergeKeys.Contains(childDocumentMergeKey)))
					{
						childProvider.Documents.Remove(childDocument);
						if (!headerDocuments.ContainsKey(childDocumentMergeKey))
						{
							headerDocuments.Add(childDocumentMergeKey, childDocument);
						}
					}
				}
			}

			headerProvider.SetDocuments(headerDocuments.Values.Select(headerProvider.Create).ToArray());
			foreach (var childProvider in childProviders)
			{
				childProvider.Provider.SetDocuments(childProvider.Documents.Select(headerProvider.Create).ToArray());
			}
		}

		public static string GetValuationIndicator(JobComInvoiceHeader invoiceHeader, JobComInvoiceLine invoiceLine) =>
			ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(invoiceHeader.RelatedIndicator, invoiceLine.JI_RelatedIndicator)
			+ ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(invoiceHeader.RelatedIndicator2, invoiceLine.ZG_RelatedIndicator2)
			+ ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(invoiceHeader.RelatedIndicator3, invoiceLine.ZG_RelatedIndicator3)
			+ ValuationIndicatorCodeListHelper.GetFromLineIfSetElseFromHeader(invoiceHeader.RelatedIndicator4, invoiceLine.ZG_RelatedIndicator4);

		public static IReadOnlyCollection<IAdditionsAndDeductions> GetAdditionsAndDeductions(this IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			var additionsAndDeductions = new List<IAdditionsAndDeductions>();

			if (invoiceLines.FirstOrDefault() is { } defaultLine)
			{
				var isUcc5 = defaultLine.InvoiceHeader?.JobDeclaration is { } declaration && declaration.IsUCC5;

				invoiceLines.SelectMany(
					line => line.Charges.Cast<JobComInvCharge>()
					.Union(line.ApportionedCharges.Cast<JobComInvCharge>())
					.Where(IsLineApportionedChargeValid))
					.GroupBy(charge => charge.J7_ChargeType)
					.ForEach(AddToProviders);

				invoiceLines.Select(invoiceLine => invoiceLine.InvoiceHeader)
					.Distinct()
					.ForEach(invoiceHeader =>
					{
						invoiceHeader.Charges.Cast<JobComInvCharge>()
							.Union(invoiceHeader.GroupHeader?.Charges.Cast<JobComInvCharge>())
							.Where(IsValidZero)
							.GroupBy(charge => charge.J7_ChargeType)
							.ForEach(AddToProviders);

						bool IsValidZero(JobComInvCharge charge)
						{
							var chargeType = charge.J7_ChargeType.ToUpperInvariant().ToString();
							var amount = charge.MoneyInLocalCurrency.Amount;
							return chargeType switch
							{
								AISChargeCodeList.Codes._1X => amount.IsEmpty,
								AISChargeCodeList.Codes.BA => amount.IsEmpty,
								AISChargeCodeList.Codes.BC => amount.IsEmpty && isUcc5,
								_ => false
							};
						}
					});
			}

			bool IsLineApportionedChargeValid(JobComInvCharge charge) => !charge.J7_ChargeType.IsEmpty && !charge.MoneyInLocalCurrency.Amount.IsEmpty;

			void AddToProviders(IGrouping<ZString, JobComInvCharge> grouping) => additionsAndDeductions.Add(
				new AdditionsAndDeductionsProvider(grouping.Key, grouping.Sum(charge => charge.MoneyInLocalCurrency.Amount))
			);

			return additionsAndDeductions.ToArray();
		}
	}
}
