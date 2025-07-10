using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocCompetitorsByTypeCollection : NonPersistentBusinessObjectCollection<DocCompetitorsByType>
	{
		public DocCompetitorsByTypeCollection()
			: base()
		{
		}

		public DocCompetitorsByTypeCollection(OrgHeader orgHeader)
		{
			OrgHeader = orgHeader;
			var groups = OrgHeader.Competitors.GroupBy(c => c.OCP_Type);

			foreach (var group in groups)
			{
				var docCompetitorsByType = new DocCompetitorsByType()
				{
					Type = group.Key,
				};

				var competitors = group.ToList().GroupBy(c => c.Competitor.OH_Code).Select(g => DocCompetitor.New(g.First(), OrgHeader.Factory));
				docCompetitorsByType.Competitors.AddRange(competitors);

				Add(docCompetitorsByType);
			}
		}

		public OrgHeader OrgHeader { get; }

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DocCompetitorsByType();
		}
	}
}
