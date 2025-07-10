using System.Collections.Generic;
using System.Xml.Serialization;
using CargoWise.Types;

namespace Enterprise.Customs.KR.Messaging
{
	public interface IImport5BAHeader : IMessageDataProvider
	{
		[XmlIgnore]
		ZString ImportDeclarationNumber { get; }
		[XmlIgnore]
		ZDate ImportDeclarationDate { get; }
		[XmlIgnore]
		IOrganization Declarant { get; }
		[DataItemID("04")]
		ZDecimal TariffRate { get; }
		[DataItemID("05")]
		ZString TariffRateClassification { get; }
		IEnumerable<IImport5BALine> EntryLines { get; }
	}
}
