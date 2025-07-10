using System.Xml.Serialization;

namespace CargoWise.UniversalCopy
{
	public class EntityFilter
	{
		[XmlAttribute(AttributeName = "FilterTypeId")]
		public string FilterTypeId { get; set; }

		[XmlAttribute(AttributeName = "OrderBy")]
		public string OrderBy { get; set; }

		public string FilterData { get; set; }
	}

	public static class EntityFilterTypeIds
	{
		public const string MandatoryExpressionFilter = "MandatoryExpressionFilter";
	}
}
