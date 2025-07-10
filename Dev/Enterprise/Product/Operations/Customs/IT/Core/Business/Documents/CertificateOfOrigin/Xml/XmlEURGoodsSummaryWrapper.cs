using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;
using IXmlEURGoodsSummary = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IEURGoodsSummary;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class XmlEURGoodsSummaryWrapper : IEURGoodsSummary
{
	public XmlEURGoodsSummaryWrapper(IEnumerable<IXmlEURGoodsSummary> goodsSummaryList)
	{
		this.goodsSummaryList = Argument.NotNull(goodsSummaryList, nameof(goodsSummaryList));
	}

	readonly IEnumerable<IXmlEURGoodsSummary> goodsSummaryList;

	ZString IEURGoodsSummary.InvoiceNumbers => Builder.InvoiceNumbers;

	ZString IGoodsSummary.Description => Builder.Description;

	ZString IGoodsSummary.WeightAndVolume => Builder.WeightAndVolume;

	XmlEURGoodsSummaryBuilder Builder => builder ?? (builder = InitializeBuilder());
	XmlEURGoodsSummaryBuilder builder;

	XmlEURGoodsSummaryBuilder InitializeBuilder()
	{
		var builder = new XmlEURGoodsSummaryBuilder(goodsSummaryList);
		builder.Build();
		return builder;
	}

	#region XmlEURGoodsSummaryBuilder

	public class XmlEURGoodsSummaryBuilder
	{
		public XmlEURGoodsSummaryBuilder(IEnumerable<IXmlEURGoodsSummary> goodsSummaryList)
		{
			this.goodsSummaryList = goodsSummaryList;
		}

		readonly IEnumerable<IXmlEURGoodsSummary> goodsSummaryList;

		public ZString Description { get; private set; }
		public ZString WeightAndVolume { get; private set; }
		public ZString InvoiceNumbers { get; private set; }

		public void Build()
		{
			var descriptionBuilder = new ZStringBuilder();
			var weightAndVolumeBuilder = new ZStringBuilder();
			var invoiceNumbersBuilder = new ZStringBuilder();
			foreach (var goodsSummary in goodsSummaryList.OrderBy(x => x.ItemNumber))
			{
				descriptionBuilder.Append(GetGoodsDescription(goodsSummary));
				weightAndVolumeBuilder.Append(GetGoodsWeightAndVolume(goodsSummary));
				invoiceNumbersBuilder.Append(GetGoodsInvoiceNumbers(goodsSummary));
			}
			descriptionBuilder.AppendIfBuilderIsNotEmpty(EndOfSection);
			Description = descriptionBuilder.ToStringWithNewLineBetweenAppends();
			WeightAndVolume = weightAndVolumeBuilder.ToStringWithNewLineBetweenAppends();
			InvoiceNumbers = invoiceNumbersBuilder.ToStringWithNewLineBetweenAppends();
		}

		string GetGoodsDescription(IXmlEURGoodsSummary goodsSummary)
		{
			return new ZStringBuilder()
				.AppendIfNotEmpty(goodsSummary.ItemNumber?.ToString() ?? ZString.Empty)
				.AppendIfNotEmpty(goodsSummary.PackagesNumber?.ToString() ?? ZString.Empty)
				.AppendIfNotEmpty(goodsSummary.PackageType)
				.AppendIfNotEmpty(goodsSummary.Description)
				.ToStringWithDelimiterBetweenAppends("; ");
		}

		string GetGoodsWeightAndVolume(IXmlEURGoodsSummary goodsSummary)
		{
			return new ZStringBuilder()
				.AppendIfNotEmpty(goodsSummary.GrossMass.HasValue ? new ZDecimal(goodsSummary.GrossMass.Value).ToStringTrimZeros() : ZString.Empty)
				.AppendIfBuilderIsNotEmpty(" KG")
				.ToString();
		}

		string GetGoodsInvoiceNumbers(IXmlEURGoodsSummary goodsSummary)
		{
			var stringBuilder = new ZStringBuilder();
			foreach (var invoiceNumber in goodsSummary.InvoiceNumbers)
			{
				stringBuilder.AppendIfNotEmpty(invoiceNumber);
			}
			return stringBuilder.ToStringWithNewLineBetweenAppends();
		}

		const string EndOfSection = "-------------------------------------------------------------------------------------------------------";
	}

	#endregion
}
