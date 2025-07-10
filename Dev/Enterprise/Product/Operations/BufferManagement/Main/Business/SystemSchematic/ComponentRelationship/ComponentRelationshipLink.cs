using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.BufferManagement.Business
{
	public class ComponentRelationshipLink : BMComponentLink
	{
		public ComponentRelationshipLink(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region FL_FC_ComponentTo

		[List("Lookups.ComponentTos")]
		[RelatedBusinessObject("ComponentTo")]
		public override ZGuid FL_FC_ComponentTo
		{
			get { return base.FL_FC_ComponentTo; }
			set
			{
				base.FL_FC_ComponentTo = value;

				TransferHintInfo.RefreshBinding();
			}
		}

		protected override void OnSetComponentTo()
		{
		}

		#endregion

		#region FL_FC_ComponentFrom

		[BusinessObjectTestExclude]
		[List("Lookups.ComponentFroms")]
		[RelatedBusinessObject("ComponentFrom")]
		public override ZGuid FL_FC_ComponentFrom
		{
			get { return base.FL_FC_ComponentFrom; }
			set
			{
				base.FL_FC_ComponentFrom = value;

				if (ComponentFrom != null && !(ComponentFrom is ComponentRelationship))
				{
					ErrorReporter.ReportOnce("Attempted to set ComponentFrom to a type other than Component Relationship. This makes no sense. SAD!");
				}
			}
		}

		#endregion

		#region FL_IsReleaseGateRuleApplied

		[BusinessObjectTestExclude]
		public override ZBool FL_IsReleaseGateRuleApplied
		{
			get { return base.FL_IsReleaseGateRuleApplied; }
			set
			{
				if (value)
				{
					ErrorReporter.ReportOnce("Attempted to enable release gate rules on a component relationship link. This makes no sense. SAD!");
				}

				base.FL_IsReleaseGateRuleApplied = value;
			}
		}

		#endregion

		#region FL_TransferRulesEnabled

		[BusinessObjectTestExclude]
		public override ZBool FL_TransferRulesEnabled
		{
			get { return base.FL_TransferRulesEnabled; }
			set
			{
				if (value)
				{
					ErrorReporter.ReportOnce("Attempted to enable transfer rules on a component relationship link. This makes no sense. SAD!");
				}

				base.FL_TransferRulesEnabled = value;
			}
		}

		#endregion

		#endregion

		#region BusinessObject overrides

		protected override BMComponentLinkLookups GetNewLookups()
		{
			return new ComponentRelationshipLinkLookups(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			FL_TransferRulesEnabled = false;
		}

		#endregion

		#region Validation

		protected override BMComponentLinkValidation GetNewValidation()
		{
			return new ComponentRelationshipLinkValidation(this);
		}

		#endregion
	}
}
