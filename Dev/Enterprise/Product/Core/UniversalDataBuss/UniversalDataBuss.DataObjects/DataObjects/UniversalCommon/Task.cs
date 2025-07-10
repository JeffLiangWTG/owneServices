using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class Task : IDataObject
	{
		[Mandatory, MaxLength(20)]
		public ZString? TaskID { get; set; }

		[Mandatory]
		public ZInt? Sequence { get; set; }

		[Mandatory, MaxLength(50)]
		public ZString? Description { get; set; }

		[MaxLength(UniversalXmlInfo.MaxStringLength)]
		public ZString? TaskNotes { get; set; }

		[MaxLength(40)]
		public ZString? CardNote { get; set; }

		[Mandatory]
		public CodeDescriptionPair Type { get; set; }

		[Mandatory]
		public CodeDescriptionPair Status { get; set; }

		public Staff AssignedStaff { get; set; }

		public Capability AssignedCapability { get; set; }

		public Group AssignedGroup { get; set; }

		public TimeSpan? EstimatedDuration { get; set; }

		public ZDecimal? EstimateVariationFactor { get; set; }

		public ZDateTimeOffset? CompletedTimeUTC { get; set; }

		public TimeSpan? ActualDuration { get; set; }

		public ZDateTimeOffset? EstimatedStartTimeUTC { get; set; }

		public ZDateTimeOffset? ActualStartTimeUTC { get; set; }
	}
}
