using System.Data;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class TagLinkTemplate : TagLink
	{
		public TagLinkTemplate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[ResourceStringData("TagLinkTemplate.TGL_Magnitude", Caption = "Magnitude", FullDescription = "The Magnitude which will be used when applying tags.")]
		public override ZDecimal TGL_Magnitude
		{
			get { return base.TGL_Magnitude; }
			set { base.TGL_Magnitude = value; }
		}

		public override ZGuid TGL_TGM_Magnitude
		{
			get { return base.TGL_TGM_Magnitude; }
			set
			{
				base.TGL_TGM_Magnitude = value;
				TagRule?.Validation.ValidateTGR_ActionType();
			}
		}

		#endregion

		#region AddTag

		public ITagOperationResult AddTag(ProcessHeader processHeader, TagRule tagRule)
		{
			if (Definition.TGD_IsExclusive)
			{
				foreach (var link in Factory.Load<RuleRunnerTagLink>(GetDeleteQuery(processHeader)))
				{
					link.Delete();
				}
			}

			var addResult = processHeader.AddTag(Magnitude);
			if (addResult.WasSuccessful)
			{
				var tagLink = (TagLink)addResult.Link;
				var cloneArgs = new BusinessObjectCloneArgs(new[] { TagLinkSchema.TGL_ParentTableCode.Name, TagLinkSchema.TGL_ParentId.Name, TagLinkSchema.TGL_Sequence.Name }, typeof(TagLink));

				tagLink.CopyPersistentValuesFrom(this, cloneArgs);
				tagLink.MutatedByTagRulePK = tagRule.PK;
			}

			return addResult;
		}

		ZQuery GetDeleteQuery(ProcessHeader processHeader)
		{
			var query = new ZDBOnlyQuery(typeof(TagLink));
			query.AddToFilter(TagLinkSchema.TGL_ParentId, processHeader.PK);
			query.AddToFilter(TagLinkSchema.TGL_ParentTableCode, processHeader.TablePrefix);

			var tagMagnitudeSubQuery = new ZDBOnlySubQuery(typeof(TagMagnitude), TagLinkSchema.TGL_TGM_Magnitude);
			tagMagnitudeSubQuery.AddToFilter(TagMagnitudeSchema.TGM_TGD_Tag, TagDefinitionPk);

			query.AddSubQuery(tagMagnitudeSubQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#region TagLink Overrides

		public override bool InOperationalScope
		{
			get { return Definition == null || Definition.CanRuleUseTags; }
		}

		public override bool MutableInOperationalScope
		{
			get { return true; }
		}

		public override bool InParentScope
		{
			get { return Definition == null || Definition.ApplicableToWorkflows; }
		}

		#endregion

		#region Business Object Overrides

		protected override TagLinkLookups GetNewLookups()
		{
			return new RuleTagLinkLookups(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			TGL_ParentTableCode = ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(TagRule.Schema.TableName);
		}

		#endregion

		#region Related Business Objects

		public TagRule TagRule
		{
			get { return Factory.LoadTop1<TagRule>(new ZQuery(TagRuleSchema.PK, TGL_ParentId)); }
		}

		#endregion
	}
}
