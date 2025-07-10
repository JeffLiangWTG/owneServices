using System.Xml.Serialization;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class EDISalesRelationDirectionRuleCollection : SalesRelationDirectionRuleCollection
	{
		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = () => new EDISalesRelationDirectionRuleCollection();
		}

		protected override void AddDefaultValues()
		{
			base.AddDefaultValues();
			AddNewRule(RelatableActivityTypeList.Codes.OpportunityManager, EDIRelatableActivityTypeList.Codes.Incident, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
			AddNewRule(EDIRelatableActivityTypeList.Codes.Incident, RelatableActivityTypeList.Codes.OpportunityManager, SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities);
		}
	}
}
