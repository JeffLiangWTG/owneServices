using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Registry.Business
{
	public interface IInvoiceDateConfiguration
	{
		ZString BrokerCode { get; set; }
		bool BrokerCode_ReadOnly { get; }
		ZPropertyInfo BrokerCodeInfo { get; }
		CodeDescriptionPairList BrokerList { get; }
		ZString DirectionCode { get; set; }
		bool DirectionCode_ReadOnly { get; }
		ZPropertyInfo DirectionCodeInfo { get; }
		CodeDescriptionPairList DirectionList { get; }
		ZString JobType { get; set; }
		ZPropertyInfo JobTypeInfo { get; }
		CodeDescriptionPairList JobTypeList { get; }
		ZString Mode { get; set; }
		bool Mode_ReadOnly { get; }
		ZPropertyInfo ModeInfo { get; }
		CodeDescriptionPairList ModeList { get; }
		BusinessObjectCollection ParentCollection { get; }
		ZString SignificantDateCode { get; set; }
		ZPropertyInfo SignificantDateCodeInfo { get; }
		CodeDescriptionPairList SignificantDateList { get; }
		InvoiceDateConfigurationLookups InvoiceDateConfigurationLookups { get; }
		ZString PriorClosedPeriod { get; set; }
		ZPropertyInfo PriorClosedPeriodInfo { get; }
		CodeDescriptionPairList PriorClosedPeriodList { get; }
		ZString PriorOpenPeriod { get; set; }
		ZPropertyInfo PriorOpenPeriodInfo { get; }
		CodeDescriptionPairList PriorOpenPeriodList { get; }
		ZString CurrentPeriod { get; set; }
		ZPropertyInfo CurrentPeriodInfo { get; }
		CodeDescriptionPairList CurrentPeriodList { get; }
		ZString FuturePeriod { get; set; }
		ZPropertyInfo FuturePeriodInfo { get; }
		CodeDescriptionPairList FuturePeriodList { get; }
		ZBool Override { get; set; }
		ZPropertyInfo OverrideInfo { get; }
		ZBool Today { get; set; }
		ZPropertyInfo TodayInfo { get; }
		ZString ReversalRule { get; set; }
		ZPropertyInfo ReversalRuleInfo { get; }
		CodeDescriptionPairList ReversalRuleList { get; }

		void ValidateBrokerCode();
		void ValidateDirectionCode();
		void ValidateJobType();
		void ValidateMode();
		void ValidateSignificantDateCode();
		void ValidatePriorClosedPeriod();
		void ValidatePriorOpenPeriod();
		void ValidateCurrentPeriod();
		void ValidateFuturePeriod();
		void ValidateOverride();
		void ValidateToday();
		void ValidateReversalRule();
	}
}
