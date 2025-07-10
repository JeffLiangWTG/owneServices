using System.Xml.Linq;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageBuilders
{
	public class AmendmentDetails
	{
		public AmendmentDetails(XElement diffGram, ZString comparisonXml, ZString newXml)
		{
			DiffGram = diffGram;
			ComparisonXml = comparisonXml;
			NewXml = newXml;
		}

		public XElement DiffGram { get; set; }

		public ZString ComparisonXml { get; set; }

		public ZString NewXml { get; set; }

		public bool HasDifferences => DiffGram.HasElements;
		public AmendmentObjectWrapper Amendments { get; set; }
	}
}
