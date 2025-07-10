using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace GlowIndexQueryService.Business
{
	using IKeyFieldCollection = ICollection<(string Key, string Value)>;
	using KeyFieldCollection = Collection<(string Key, string Value)>;

	// in future this should be CargoWise.Glow.Index.Service.EntityInfo class
	// $Glow\DotNet\Infrastructure\Index\Service\DataService\EntityInfo.cs
	public class GlowIndexQueryResult
	{
		public GlowIndexQueryResult(string pk, string entityType)
		{
			PK = pk;
			EntityType = entityType;
			KeyFields = new KeyFieldCollection();
		}

		public GlowIndexQueryResult(string pk, string entityType, IKeyFieldCollection keyFields)
		{
			PK = pk;
			EntityType = entityType;
			KeyFields = keyFields;
		}

		public string PK { get; }
		public string EntityType { get; }
		public IKeyFieldCollection KeyFields { get; }
	}
}
