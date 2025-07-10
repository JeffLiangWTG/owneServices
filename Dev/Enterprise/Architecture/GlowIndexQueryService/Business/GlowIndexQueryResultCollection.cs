using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Types;

namespace GlowIndexQueryService.Business
{
	public class GlowIndexQueryResultCollection
	{
		public GlowIndexQueryStatus Status { get; set; } = GlowIndexQueryStatus.Uninitialised;
		public ICollection<GlowIndexQueryResult> Results { get; set; } = new Collection<GlowIndexQueryResult>();

		public IDictionary<string, ZDateTimeOffset> TablesUpdateTime { get; } = new Dictionary<string, ZDateTimeOffset>();

		public ZDateTimeOffset LastUpdateTime => TablesUpdateTime.Count > 0 ? TablesUpdateTime.Values.Max() : ZDateTimeOffset.Invalid;

		public string ErrorMessage { get; set; }
		public string WarningMessage { get; set; }

		public int MaximumResults { get; set; }

		public void Concat(ICollection<GlowIndexQueryResult> results)
		{
			foreach (var v in results)
			{
				Results.Add(v);
			}
		}

		public bool IsOutOfDate
		{
			get
			{
				if (LastUpdateTime.IsValid)
				{
					return ZDateTimeOffset.Now - LastUpdateTime > Constants.MAXIMUM_UPDATE_TIME;
				}
				return true;
			}
		}
	}
}
