using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer)]
	public class AdditionalService : IDataObject
	{
		[CandidateKey]
		public CodeDescriptionPair ServiceCode { get; set; }
		public ZDateTime? Booked { get; set; }
		public ZDateTime? Completed { get; set; }
		public OrganizationAddress Contractor { get; set; }
		public ZDateTime? Duration { get; set; }
		public OrganizationAddress Location { get; set; }
		public ZDecimal? ServiceCount { get; set; }
		[MaxLength(256), AllowLineControlWhiteSpace]
		public ZString? ServiceNote { get; set; }
		[MaxLength(35)]
		public ZString? References { get; set; }
		[MaxLength(3)]
		public ZString? MeasurementBasis { get; set; }
		[MaxLength(3)]
		public ZString? ServiceRateCurrency { get; set; }
		public ZDecimal? ServiceRate { get; set; }
		[MaxLength(20), CandidateKey]
		public ZString? ServiceId { get; set; }
		[MaxLength(20)]
		public ZString? ExternalServiceId { get; set; }
		[MaxLength(35)]
		public ZString? SubLocation { get; set; }
	}
}
