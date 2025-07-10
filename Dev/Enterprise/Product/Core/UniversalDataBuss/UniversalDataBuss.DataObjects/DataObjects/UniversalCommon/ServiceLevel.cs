using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Outer), FlattenedIntoAttributes("Code")]
	public class ServiceLevel : ICodeDescriptionDataObject
	{
		[MaxLength(3), Mandatory, CodeMap(Constants.OrgPatternMatchOverrideRelationships.ServiceLevel)]
		public ZString? Code { get; set; }
		[MaxLength(35)]
		public ZString? Description { get; set; }
		[MaxLength(64)]
		public ZString? CarrierServiceCode { get; set; }
		[MaxLength(64)]
		public ZString? CarrierProductCode { get; set; }
		[MaxLength(10)]
		public ZString? CarrierChargeCode { get; set; }
		[MaxLength(10)]
		public ZString? CarrierProfileID { get; set; }
	}
}
