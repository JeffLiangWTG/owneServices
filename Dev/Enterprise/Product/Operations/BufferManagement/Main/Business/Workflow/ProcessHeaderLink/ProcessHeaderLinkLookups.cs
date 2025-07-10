using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class ProcessHeaderLinkLookups : AutoProcessHeaderLinkLookups
	{
		public ProcessHeaderLinkLookups(AutoProcessHeaderLink parent)
			: base(parent)
		{
		}

		public new ProcessHeaderLink Parent
		{
			get { return (ProcessHeaderLink)base.Parent; }
		}

		public CodeDescriptionPairList LinkTypeList
		{
			get { return Factory.GetCachedValue<ProcessHeaderLinkTypeList>(); }
		}

		public virtual ProcessHeaderCollection HeaderFroms
		{
			get
			{
				return Factory.GetCachedValue("ProcessHeaderLinkLookups.HeaderFroms." + Parent.PK, () =>
				{
					var headersFrom = GetTemplateLookups(RelationshipDirection.From) ?? new ProcessHeaderCollection(Factory);
					AddFilterDefaults(headersFrom);

					return headersFrom;
				});
			}
		}

		public virtual ProcessHeaderCollection HeaderTos
		{
			get
			{
				return Factory.GetCachedValue("ProcessHeaderLinkLookups.HeaderTos." + Parent.PK, () =>
				{
					var headersTo = GetTemplateLookups(RelationshipDirection.To) ?? new ProcessHeaderCollection(Factory);
					AddFilterDefaults(headersTo);

					return headersTo;
				});
			}
		}

		public ProcessTaskTemplateCollection Templates
		{
			get
			{
				return Factory.GetCachedValue("ProcessHeaderLinkLookups.Templates", () =>
				{
					var template = Parent.Template;
					var additionalQuery = template != null ? new ZQuery(ProcessTaskTemplateSchema.PK, SQLComparisonOperator.NotEqual, template.PK) : null;

					var collection = new ProcessTaskTemplateCollection(Factory, additionalQuery);
					collection.Load();

					return collection;
				});
			}
		}

		ProcessHeaderCollection GetTemplateLookups(RelationshipDirection fromOrTo)
		{
			if (Parent.Template != null)
			{
				var externalTemplatePK =
					fromOrTo == RelationshipDirection.From ? Parent.FromWorkflowExternalTemplatePK
					: fromOrTo == RelationshipDirection.To ? Parent.ToWorkflowExternalTemplatePK
					: ZGuid.Empty;
				var applicableTemplate = Factory.Load<ProcessTaskTemplate>(externalTemplatePK) ?? Parent.Template;

				return new ProcessHeaderCollection(applicableTemplate);
			}
			else
			{
				return null;
			}
		}

		void AddFilterDefaults(ProcessHeaderCollection headers)
		{
			if (!Parent.IsDeleted)
			{
				ProcessHeaderLookups.AddFilterDefaults(Parent.ProcessJobHeader, headers);
			}
		}

		internal void ClearCachedCollections()
		{
			Factory.ClearCachedValue<ProcessHeaderCollection>("ProcessHeaderLinkLookups.HeaderFroms." + Parent.PK);
			Factory.ClearCachedValue<ProcessHeaderCollection>("ProcessHeaderLinkLookups.HeaderTos." + Parent.PK);
			Factory.ClearCachedValue<ProcessHeaderCollection>("ProcessHeaderLinkLookups.Siblings." + Parent.PK);
		}
	}
}
