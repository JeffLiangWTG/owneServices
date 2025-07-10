using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	[RootElement("Exception")]
	public class WorkflowException : IDataObject
	{
		[Mandatory, MaxLength(20)]
		public ZString? ExceptionID { get; set; }

		[Mandatory, MaxLength(100)]
		public ZString? Description { get; set; }

		[MaxLength(3)]
		public ZString? Category { get; set; }

		[MaxLength(3)]
		public ZString? Type { get; set; }

		public ZBool? Actioned { get; set; }

		public Staff Staff { get; set; }

		public Group Group { get; set; }

		public UXmlDateTime? Date { get; set; }

		[MaxLength(3)]
		public ZString? Cause { get; set; }

		[MaxLength(3)]
		public ZString? Resolution { get; set; }

		public UXmlDateTime? ActionedDate  { get; set; }

		public UXmlDateTime? EndDate { get; set; }

		public ZInt? DurationHours { get; set; }

		public UNLOCO Location { get; set; }

		[MaxLength(UniversalXmlInfo.MaxStringLength)]
		public ZString? Notes { get; set; }
	}
}
