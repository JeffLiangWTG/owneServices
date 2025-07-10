using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal._2012_11
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName), NamespaceSpecific(UniversalXmlInfo.Namespace_2012_11)]
	public partial class Workflow : IDataObject
	{
		public CodeDescriptionPair EventType { get; set; }
		[MaxLength(1024)]
		public ZString? EventReference { get; set; }
		public Staff EventUser { get; set; }
		public Branch EventBranch { get; set; }
		public Department EventDepartment { get; set; }
		public ZDateTimeOffset? TriggerDate { get; set; }
		[MaxLength(50)]
		public ZString? TriggerDescription { get; set; }
		[MaxLength(2048)]
		public ZString? TriggerReference { get; set; }
		public TriggerType? TriggerType { get; set; }
		public ZInt? TriggerCount { get; set; }
		public Company Company { get; set; }
		public CodeDescriptionPair ActionPurpose { get; set; }
		public List<RecipientRole> RecipientRoleCollection { get; set; }

		public ZBool? CodesMappedToTarget { get; set; }
	}
}
