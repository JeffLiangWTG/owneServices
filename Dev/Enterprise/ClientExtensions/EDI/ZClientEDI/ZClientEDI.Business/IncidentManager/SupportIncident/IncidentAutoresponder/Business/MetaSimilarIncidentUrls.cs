using System.Collections.Generic;
using CargoWise.Types;

namespace ZClientEDI.Business.IncidentManager.SupportIncident.IncidentAutoresponder
{
	public class MetaSimilarIncidentUrls
	{
		public ZGuid SimilarIncidentPk { get; }
		public decimal SimilarityScore { get; }
		public IEnumerable<string> Urls { get; set; }

		public MetaSimilarIncidentUrls(ZGuid similarIncidentPk, decimal similarityScore)
		{
			SimilarIncidentPk = similarIncidentPk;
			SimilarityScore = similarityScore;
			Urls = null;
		}

		public override bool Equals(object obj)
		{
			if (object.ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj is MetaSimilarIncidentUrls similarIncidentUrls)
			{
				return this.SimilarIncidentPk == similarIncidentUrls.SimilarIncidentPk
					&& this.SimilarityScore == similarIncidentUrls.SimilarityScore
					&& this.Urls == similarIncidentUrls.Urls;
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (this.SimilarIncidentPk, this.SimilarityScore, this.Urls).GetHashCode();
		}

		public static bool operator ==(MetaSimilarIncidentUrls obj1, MetaSimilarIncidentUrls obj2)
		{
			return obj1.Equals(obj2);
		}

		public static bool operator !=(MetaSimilarIncidentUrls obj1, MetaSimilarIncidentUrls obj2)
		{
			return !(obj1.Equals(obj2));
		}
	}
}