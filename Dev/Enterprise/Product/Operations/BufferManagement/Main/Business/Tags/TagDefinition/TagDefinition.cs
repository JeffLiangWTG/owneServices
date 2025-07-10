using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[CodeProperty(Schema.TGD_Code), DescriptionProperty(Schema.TGD_Description)]
	public class TagDefinition : AutoTagDefinition, ITagDefinition, IAuditParent
	{
		public TagDefinition(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Related Business Objects

		[ChildEditable]
		public TagMagnitudeCollection Magnitudes
		{
			get
			{
				if (magnitudes == null)
				{
					magnitudes = new TagMagnitudeCollection(this);
					RegisterEditableChildObject(magnitudes);
				}

				return magnitudes;
			}
		}
		TagMagnitudeCollection magnitudes;

		#endregion

		#region Properties

		[ReadOnly(true)]
		public override ZBool TGD_IsSystem
		{
			get { return base.TGD_IsSystem; }
			set { base.TGD_IsSystem = value; }
		}

		[ReadOnlyMember(TagDefinitionSchema.Constants.TGD_IsSystem)]
		[List("Lookups.UsageScopeList")]
		public override ZString TGD_UsageScope
		{
			get { return base.TGD_UsageScope; }
			set { base.TGD_UsageScope = value; }
		}

		[ReadOnlyMember(TagDefinitionSchema.Constants.TGD_IsSystem)]
		[List("Lookups.ScopeList")]
		public override ZString TGD_Scope
		{
			get { return base.TGD_Scope; }
			set { base.TGD_Scope = value; }
		}

		[ReadOnlyMember(TagDefinitionSchema.Constants.TGD_IsSystem)]
		public override ZString TGD_Code
		{
			get { return base.TGD_Code; }
			set { base.TGD_Code = value; }
		}

		[ReadOnlyMember(TagDefinitionSchema.Constants.TGD_IsSystem)]
		[TranslatableDataField(Schema.TableName, Schema.TGD_Description, @"Database\Odyssey\Data\Public\TagRule\TagRule.xml", MaxLength = Schema.TGD_DescriptionMaxLength, Type = typeof(TagDefinition), Asmid = ResString.AssemblyId)]
		public override ZString TGD_Description
		{
			get { return base.TGD_Description; }
			set { base.TGD_Description = value; }
		}

		[ResourceStringData("4cd57b94-8327-4104-9549-a95b80e6ea28", Caption = "Description")]
		public MultilingualString TGD_DescriptionMultilingual => GetMultilingual(TGD_DescriptionInfo);

		[ReadOnlyMember(TagDefinitionSchema.Constants.TGD_IsSystem)]
		public override ZBool TGD_IsExclusive
		{
			get { return base.TGD_IsExclusive; }
			set { base.TGD_IsExclusive = value; }
		}

		#endregion

		#region New Properties

		public string DisplayText
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0} - {1}", TGD_Code, TGD_DescriptionMultilingual); }
		}

		public bool CanUserUseTags
		{
			get { return TGD_UsageScope != TagUsageScopeList.Codes.Rule; }
		}

		public bool CanRuleUseTags
		{
			get { return TGD_UsageScope != TagUsageScopeList.Codes.User; }
		}

		public bool CanUserAndRuleUseTags
		{
			get { return TGD_UsageScope == TagUsageScopeList.Codes.All; }
		}

		public bool ApplicableToWorkflows
		{
			get { return TGD_Scope != TagScopeList.Codes.Task; }
		}

		public bool ApplicableToTasks
		{
			get { return TGD_Scope != TagScopeList.Codes.Workflow; }
		}

		public bool HasTagRules
		{
			get { return Factory.Exists(typeof(TagRule), GetTagRuleQuery()); }
		}

		ZQuery GetTagRuleQuery()
		{
			var query = new ZDBOnlyQuery(typeof(TagRule));
			var linkSubQuery = new ZDBOnlySubQuery(typeof(TagLink), TagLinkSchema.TGL_ParentId);
			linkSubQuery.AddToFilter(TagLinkSchema.TGL_ParentTableCode, TagRuleSchema.Constants.Prefix);

			var magSubQuery = new ZDBOnlySubQuery(typeof(TagMagnitude), TagLinkSchema.TGL_TGM_Magnitude);
			magSubQuery.AddToFilter(TagMagnitudeSchema.TGM_TGD_Tag, PK);

			linkSubQuery.AddSubQuery(magSubQuery, JoinCondition.And);
			query.AddSubQuery(TagRuleSchema.PK, linkSubQuery, JoinCondition.And);
			return query;
		}

		#endregion

		#region Comparison

		public bool ValidForScope(Type tagableType)
		{
			return (typeof(ProcessHeader).IsAssignableFrom(tagableType) && ApplicableToWorkflows)
				|| (typeof(ProcessTask).IsAssignableFrom(tagableType) && ApplicableToTasks);
		}

		#endregion

		#region BusinessObject Overrides

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("f734d684-4456-4e56-9d90-55720486d050", "Tag Group - {0}", TGD_Code);
			}
		}

		public override void Delete()
		{
			if (TGD_IsSystem)
			{
				throw new CannotDeleteException("Cannot delete system tag groups");
			}

			Magnitudes.DeleteAll();
			base.Delete();
		}

		#endregion

		#region IAuditParent Mambers

		public IEnumerable<AuditChildInfo> RelatedAuditChildren => Enumerable.Empty<AuditChildInfo>();

		#endregion
	}
}
