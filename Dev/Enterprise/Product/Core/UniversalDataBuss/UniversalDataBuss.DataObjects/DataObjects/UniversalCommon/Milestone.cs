using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class Milestone : IDataObject
	{
		[Mandatory]
		public ZInt? Sequence { get; set; }

		[Mandatory, MaxLength(50)]
		public ZString? Description { get; set; }

		[Mandatory, MaxLength(3)]
		public ZString? EventCode { get; set; }

		public ZDateTimeOffset? EstimatedDate { get; set; }

		public ZDateTimeOffset? ActualDate { get; set; }

		[MaxLength(3)]
		public ZString? ConditionType { get; set; }

		[MaxLength(128)]
		public ZString? ConditionReference { get; set; }
	}
}
