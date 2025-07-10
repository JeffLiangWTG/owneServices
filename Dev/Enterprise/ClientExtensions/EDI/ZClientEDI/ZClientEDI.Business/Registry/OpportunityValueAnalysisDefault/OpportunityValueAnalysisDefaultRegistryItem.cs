using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class OpportunityValueAnalysisDefaultRegistryItem : StronglyTypedRegistryItem<OpportunityValueAnalysisDefaultCollection>
	{
		public OpportunityValueAnalysisDefaultRegistryItem(string category)
			: base(new RegistryItemImpl(
				"OpportunityValueAnalysisDefault",
				(NoResString)category,
				(NoResString)"Opportunity Value Analysis defaults",
				(NoResString)"Opportunity Value Analysis defaults",
				new OpportunityValueAnalysisDefaultDataType(),
				RegistryStorageFlags.System,
				DataRegistry.Instance.ProductivityWiseModeEnabled ? RegistryOptions.IsHidden : RegistryOptions.Default,
				DefaultCollection))
		{
		}

		static OpportunityValueAnalysisDefaultCollection DefaultCollection
		{
			get
			{
				var defaultValue = new OpportunityValueAnalysisDefaultCollection();

				var def = defaultValue.AddNew();
				def.Code = (NoResString)"BRK";
				def.Description = (NoResString)"Customs Brokerage";

				def = defaultValue.AddNew();
				def.Code = (NoResString)"SHP";
				def.Description = (NoResString)"Forwarding";

				def = defaultValue.AddNew();
				def.Code = (NoResString)"LGY";
				def.Description = (NoResString)"Liner & Agency";

				def = defaultValue.AddNew();
				def.Code = (NoResString)"TRN";
				def.Description = (NoResString)"Transport";

				def = defaultValue.AddNew();
				def.Code = (NoResString)"WHS";
				def.Description = (NoResString)"Warehouse";

				return defaultValue;
			}
		}
	}
}

