using System;
using System.Collections.Generic;
using System.Linq;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal
{
	[XsdSchema(UniversalXmlInfo.CommonSchemaName)]
	public class MessageProfile : IDataObject, IMessageProfile
	{
		public SchemaFilter SchemaFilter { get; set; }

		#region IMessageProfile

		SchemaFilterType? IMessageProfile.GetFilterType()
		{
			return SchemaFilter?.Type;
		}

		Dictionary<string, IEnumerable<string>> IMessageProfile.GetFilterElements()
		{
			return SchemaFilter?
				.FilterCollection?.Where(f => !string.IsNullOrEmpty(f.ElementName))
				.GroupBy(f => f.ElementName.ToString())
				.ToDictionary(group =>
					group.Key,
					group => group.Select(f => f.DataContext?.ToString()).Where(d => !string.IsNullOrEmpty(d)) ?? Enumerable.Empty<string>(),
					StringComparer.OrdinalIgnoreCase)
				?? new Dictionary<string, IEnumerable<string>>(StringComparer.OrdinalIgnoreCase);
		}

		bool IMessageProfile.IsValid()
		{
			return SchemaFilter?.Type != null;
		}

		#endregion
	}
}
