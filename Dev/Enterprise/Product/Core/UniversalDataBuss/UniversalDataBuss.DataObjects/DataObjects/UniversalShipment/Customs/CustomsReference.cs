using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Customs
{
	[XsdSchema(Placement.Outer)]
	public partial class CustomsReference : IDataObject
	{
		[Mandatory]
		public CodeDescriptionPair Type { get; set; }
		public CodeDescriptionPair35Char SubType { get; set; }
		[MaxLength(100)]
		public ZString? Reference { get; set; }
		[MaxLength(255), AllowLineControlWhiteSpace]
		public ZString? ReferencedEntityDescription { get; set; }
		public ZInt? Order { get; set; }
		public ZBool? IsOverridden { get; set; }
		public List<Date> DateCollection { get; set; }
		public OrganizationAddress Owner { get; set; }
	}
}
