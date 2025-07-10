using System.Collections.Generic;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(Placement.Inner)]
	public partial class SchemaFilter : IDataObject
	{
		public SchemaFilterType? Type { get; set; }

		public List<Filter> FilterCollection { get; private set; }
	}
}
