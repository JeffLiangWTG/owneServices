using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Documents.CertificateOfOrigin;
using Enterprise.Customs.EU.Business.Documents.DocDataObjects;
using IXmlATRGoodsSummary = CargoWise.Customs.IT.MessageDefinitions.CertificateOfOrigin.IGoodsSummary;

namespace Enterprise.Customs.IT.Business.Documents.CertificateOfOrigin;

public class XmlATRBoxItemBuilder : IATRBoxItems
{
	public XmlATRBoxItemBuilder(IEnumerable<IXmlATRGoodsSummary> goodsSummaryList)
	{
		this.goodsSummaryList = Argument.NotNull(goodsSummaryList, nameof(goodsSummaryList));
	}
	readonly IEnumerable<IXmlATRGoodsSummary> goodsSummaryList;

	ZString IATRBoxItems.ItemsInfoBox9 => itemsInfoBox9;
	ZString itemsInfoBox9;

	ZString IATRBoxItems.MarksNumberBox10 => marksNumberBox10;
	ZString marksNumberBox10;

	ZString IATRBoxItems.GrossWeightBox11 => grossWeightBox11;
	ZString grossWeightBox11;

	void IATRBoxItems.Build()
	{
		var box9Builder = new ZStringBuilder();
		var box10Builder = new ZStringBuilder();
		var box11Builder = new ZStringBuilder();

		foreach (IXmlATRGoodsSummary summaryLine in goodsSummaryList.OrderBy(x => x.ItemNumber))
		{
			box9Builder.Append(GetBox9Contents(summaryLine));
			box10Builder.Append(GetBox10Contents(summaryLine));
			box11Builder.Append(GetBox11Contents(summaryLine));
		}

		itemsInfoBox9 = box9Builder.ToStringWithNewLineBetweenAppends();
		marksNumberBox10 = box10Builder.ToStringWithNewLineBetweenAppends();
		grossWeightBox11 = box11Builder.ToStringWithNewLineBetweenAppends();
	}

	string GetBox9Contents(IXmlATRGoodsSummary goodsSummary)
	{
		return new ZStringBuilder()
			.AppendIfNotEmpty(goodsSummary.ItemNumber?.ToString() ?? ZString.Empty)
			.ToStringWithDelimiterBetweenAppends("; ");
	}

	string GetBox10Contents(IXmlATRGoodsSummary goodsSummary)
	{
		return new ZStringBuilder()
			.AppendIfNotEmpty(goodsSummary.PackagesNumber?.ToString() ?? ZString.Empty)
			.AppendIfNotEmpty(goodsSummary.PackageType)
			.AppendIfNotEmpty(goodsSummary.Description)
			.ToStringWithDelimiterBetweenAppends("; ");
	}

	string GetBox11Contents(IXmlATRGoodsSummary goodsSummary)
	{
		return new ZStringBuilder()
			.AppendIfNotEmpty(goodsSummary.GrossMass.HasValue ? new ZDecimal(goodsSummary.GrossMass.Value).ToStringTrimZeros() : ZString.Empty)
			.AppendIfBuilderIsNotEmpty(" KG")
			.ToString();
	}
}
