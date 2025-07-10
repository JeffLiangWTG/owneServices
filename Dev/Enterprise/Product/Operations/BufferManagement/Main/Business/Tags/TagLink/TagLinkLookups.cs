using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class TagLinkLookups : AutoTagLinkLookups
	{
		public TagLinkLookups(AutoTagLink parent)
			: base(parent)
		{
		}

		new TagLink Parent
		{
			get { return (TagLink)base.Parent; }
		}

		public TagMagnitudeCollection Magnitudes
		{
			get
			{
				if (!Parent.IsDeleted && Parent.Definition != null)
				{
					return new TagMagnitudeCollection(Parent.Definition, GetMagnitudeUsageScope());
				}
				else
				{
					return new TagMagnitudeCollection(Factory, GetMagnitudeUsageScope());
				}
			}
		}

		ZQuery GetMagnitudeUsageScope()
		{
			if (!Parent.IsDeleted)
			{
				var magnitudeQuery = new ZDBOnlyQuery(typeof(TagMagnitude));
				var definitionSubQuery = new ZDBOnlySubQuery(typeof(TagDefinition), TagMagnitudeSchema.TGM_TGD_Tag);
				definitionSubQuery.AddToFilter(GetDefinitionScope(false));
				magnitudeQuery.AddSubQuery(definitionSubQuery, JoinCondition.And);

				return new ZQuery(magnitudeQuery, JoinCondition.Or, Parent.TGL_TGM_Magnitude != ZGuid.Empty ? new ZQuery(TagMagnitudeSchema.PK, Parent.TGL_TGM_Magnitude) : ZQuery.NoResultQuery);
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		ZQuery GetDefinitionScope(bool includeCurrent)
		{
			var scopeQuery = new ZQuery();
			var existingQuery = ZQuery.NoResultQuery;
			if (!Parent.IsDeleted)
			{
				scopeQuery.AddToFilter(GetDefinitionOperationalScope());
				scopeQuery.AddToFilter(GetDefinitionUsageScope());
				if (includeCurrent && Parent.Magnitude != null)
				{
					existingQuery = new ZQuery(TagDefinitionSchema.PK, Parent.Magnitude.TGM_TGD_Tag);
				}
				return new ZQuery(scopeQuery, JoinCondition.Or, existingQuery);
			}
			else
			{
				return ZQuery.NoResultQuery;
			}
		}

		protected virtual ZQuery GetDefinitionUsageScope()
		{
			if (Parent.InOperationalScope && !Parent.IsTemplateTag)
			{
				return new ZQuery(TagDefinitionSchema.TGD_UsageScope, SQLComparisonOperator.NotEqual, TagUsageScopeList.Codes.Rule);
			}
			else
			{
				return new ZQuery();
			}
		}

		protected virtual ZQuery GetDefinitionOperationalScope()
		{
			if (Parent.Parent is ProcessHeader)
			{
				return new ZQuery(TagDefinitionSchema.TGD_Scope, SQLComparisonOperator.NotEqual, TagScopeList.Codes.Task);
			}
			else if (Parent.Parent is ProcessTask)
			{
				return new ZQuery(TagDefinitionSchema.TGD_Scope, SQLComparisonOperator.NotEqual, TagScopeList.Codes.Workflow);
			}
			else
			{
				return new ZQuery();
			}
		}

		public TagDefinitionCollection Definitions
		{
			get { return new TagDefinitionCollection(Factory, GetDefinitionScope(true)); }
		}

		public ICodeDescriptionPairList QueueStatusList
		{
			get { return Factory.GetCachedValue<QueueStatusList>(); }
		}
	}
}
