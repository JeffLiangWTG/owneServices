using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentWrappers
{
	public class DocCompetitorsByType : NonPersistentBusinessObject
	{
		public ZString Type { get; set; }

		public DocCompetitorCollection Competitors => competitors ?? (competitors = new DocCompetitorCollection(Factory));
		DocCompetitorCollection competitors;

		public ZString TypeCodeAndDescription
		{
			get
			{
				if (OrganisationsDataRegistry.Instance.CompetitorType.Value.FindByCode(Type) != null)
				{
					return OrganisationsDataRegistry.Instance.CompetitorType.Value.GetCodeDescription(Type);
				}
				else
				{
					return Type;
				}
			}
		}
	}
}
