using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.TestDataObjects._2012_11
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class UberWorkflow : IDataObject
	{
		public UberCompany Company { get; set; }
		public UberCodeDescriptionPair ActionPurpose { get; set; }
		public UberCodeDescriptionPair EventType { get; set; }
		public UberStaff EventUser { get; set; }
		public ZDateTime? TriggerDate { get; set; }
		[MaxLength(50)]
		public ZString? TriggerDescription { get; set; }
		[MaxLength(2048)]
		public ZString? TriggerReference { get; set; }
		public TriggerType? TriggerType { get; set; }
		public ZInt? TriggerCount { get; set; }
	}
}
