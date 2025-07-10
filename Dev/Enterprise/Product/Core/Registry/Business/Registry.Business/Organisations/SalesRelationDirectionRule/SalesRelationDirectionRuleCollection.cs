using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class SalesRelationDirectionRuleCollection : RegistryBusinessObjectCollectionTemplate<SalesRelationDirectionRule>
	{
		public SalesRelationDirectionRuleCollection()
			: base(null, null)
		{
		}

		public static SalesRelationDirectionRuleCollection GetDefaultValues()
		{
			var collection = OverridableNewDelegate.Value?.Invoke() ?? new SalesRelationDirectionRuleCollection();
			collection.SuspendValidation();
			collection.AddDefaultValues();
			collection.ResumeValidation();

			return collection;
		}

		public SalesRelationDirectionRule AddNewRule(params string[] nodeSequence)
		{
			var rule = AddNew();
			using (rule.GetValidationSuspender())
			{
				foreach (var node in nodeSequence)
				{
					rule.Nodes.AddNew().Type = node;
				}
			}

			return rule;
		}

		public IEnumerable<SalesRelationRuleNode> FindAllSucceedingNodes(ZString[] nodeSequence)
		{
			var result = new List<SalesRelationRuleNode>();
			foreach (SalesRelationDirectionRule rule in this)
			{
				var succeedingNode = rule.Nodes.FindSucceedingNode(nodeSequence);
				if (succeedingNode != null)
				{
					result.Add(succeedingNode);
				}
			}

			return result;
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public static IEnumerable<string> AffectedActivityTypes
		{
			get
			{
				return
					from type in SalesRelationTypeList.New().GetCodes()
					where type != RelatableActivityTypeList.Codes.Communication
					select type;
			}
		}

		#region Implementation

		protected delegate SalesRelationDirectionRuleCollection NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

		protected virtual void AddDefaultValues()
		{
			AddNewRule(RelatableActivityTypeList.Codes.InquiryManager, RelatableActivityTypeList.Codes.OpportunityManager, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.InquiryManager, RelatableActivityTypeList.Codes.Quotations, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.InquiryManager, RelatableActivityTypeList.Codes.OneOffQuotes, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.InquiryManager, RelatableActivityTypeList.Codes.CampaignManagement, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.InquiryManager, RelatableActivityTypeList.Codes.Projects, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);

			AddNewRule(RelatableActivityTypeList.Codes.OpportunityManager, RelatableActivityTypeList.Codes.CampaignManagement, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.OpportunityManager, RelatableActivityTypeList.Codes.Quotations, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.OpportunityManager, RelatableActivityTypeList.Codes.OneOffQuotes, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.OpportunityManager, RelatableActivityTypeList.Codes.OpportunityManager, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.OpportunityManager, RelatableActivityTypeList.Codes.Projects, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);

			AddNewRule(RelatableActivityTypeList.Codes.CampaignManagement, RelatableActivityTypeList.Codes.OpportunityManager, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.CampaignManagement, RelatableActivityTypeList.Codes.InquiryManager, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.CampaignManagement, RelatableActivityTypeList.Codes.Quotations, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.CampaignManagement, RelatableActivityTypeList.Codes.OneOffQuotes, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.CampaignManagement, RelatableActivityTypeList.Codes.CampaignManagement, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.CampaignManagement, RelatableActivityTypeList.Codes.Projects, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);

			AddNewRule(RelatableActivityTypeList.Codes.Quotations, RelatableActivityTypeList.Codes.Quotations, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.Quotations, RelatableActivityTypeList.Codes.OneOffQuotes, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.Quotations, RelatableActivityTypeList.Codes.InquiryManager, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.Quotations, RelatableActivityTypeList.Codes.OpportunityManager, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.Quotations, RelatableActivityTypeList.Codes.Projects, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);

			AddNewRule(RelatableActivityTypeList.Codes.OneOffQuotes, RelatableActivityTypeList.Codes.OneOffQuotes, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.OneOffQuotes, RelatableActivityTypeList.Codes.Quotations, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.OneOffQuotes, RelatableActivityTypeList.Codes.InquiryManager, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.OneOffQuotes, RelatableActivityTypeList.Codes.OpportunityManager, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.OneOffQuotes, RelatableActivityTypeList.Codes.Projects, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);

			AddNewRule(RelatableActivityTypeList.Codes.Projects, RelatableActivityTypeList.Codes.CampaignManagement, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.Projects, RelatableActivityTypeList.Codes.OpportunityManager, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(RelatableActivityTypeList.Codes.Projects, RelatableActivityTypeList.Codes.InquiryManager, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new SalesRelationDirectionRuleCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SalesRelationDirectionRule();
		}

		#endregion
	}
}
