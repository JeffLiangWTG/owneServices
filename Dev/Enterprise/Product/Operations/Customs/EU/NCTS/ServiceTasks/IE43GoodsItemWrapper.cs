using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Customs.EU.NCTS.ServiceTasks;

public class IE43GoodsItemWrapper
{
	public ZShort ItemNumber { get; set; }
	public ZString CommodityCode { get; set; }
	public ZString DeclarationType { get; set; }
	public ZString DescriptionOfGoods { get; set; }
	public ZDecimal GrossWeight { get; set; }
	public ZString GrossWeightUQ { get; set; }
	public ZDecimal NetWeight { get; set; }
	public ZString NetWeightUQ { get; set; }
	public ZString CountryOfDispatch { get; set; }
	public ZString CountryOfDestination { get; set; }
	public IReadOnlyCollection<IE43ProducedDocumentsCertificateWrapper> ProducedDocumentsCertificates { get; set; }
	public IReadOnlyCollection<IE43SpecialMentionWrapper> SpecialMentions { get; set; }
	public IReadOnlyCollection<ZString> Containers { get; set; }
	public IReadOnlyCollection<IE43PackageWrapper> Packages { get; set; }
	public IReadOnlyCollection<IE43SgiCodeWrapper> SgiCodes { get; set; }
}
