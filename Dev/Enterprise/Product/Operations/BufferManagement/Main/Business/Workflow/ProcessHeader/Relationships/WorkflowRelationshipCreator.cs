using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public static class WorkflowRelationshipCreator
	{
		public static RelationshipCreationResult CreateDependencyRelationship(BusinessObjectFactory factory, ProcessHeader from, ProcessHeader to, RelationshipOptions options)
		{
			var existingLink = from.PostrequisiteLinks.FirstOrDefault(l => l.HeaderTo.PK == to.PK);

			if (existingLink == null || !options.HasFlag(RelationshipOptions.CreateOnlyIfLinkDoesNotExist))
			{
				if (options.HasFlag(RelationshipOptions.ReverseExistingRelationship))
				{
					var query = new ZQuery(ProcessHeaderLinkSchema.FP_FH_HeaderFrom, to.PK);
					query.AddToFilter(ProcessHeaderLinkSchema.FP_FH_HeaderTo, from.PK);
					query.AddToFilter(ProcessHeaderLinkSchema.FP_LinkType, ProcessHeaderLinkTypeList.Codes.Dependency);

					var linkToRemove = factory.LoadTop1<ProcessHeaderLink>(query);
					if (linkToRemove != null)
					{
						linkToRemove.Delete();
					}
				}

				var link = factory.New<ProcessHeaderLink>();
				link.FP_LinkType = ProcessHeaderLinkTypeList.Codes.Dependency;
				link.FP_FH_HeaderFrom = from.PK;
				link.FP_FH_HeaderTo = to.PK;

				from.RegisterEditableChildObject(link);
				to.RegisterEditableChildObject(link);

				return new RelationshipCreationResult(link, isExistingLink: false);
			}

			return new RelationshipCreationResult(existingLink, isExistingLink: true);
		}

		public class RelationshipCreationResult
		{
			internal RelationshipCreationResult(ProcessHeaderLink link, bool isExistingLink)
			{
				Link = link;
				IsExistingLink = isExistingLink;
			}

			public ProcessHeaderLink Link { get; }
			public bool IsExistingLink { get; }
		}
	}
}
