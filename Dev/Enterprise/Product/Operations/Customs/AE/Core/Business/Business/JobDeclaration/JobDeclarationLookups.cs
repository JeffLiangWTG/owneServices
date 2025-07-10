using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AE;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AE.Business;

public sealed class JobDeclarationLookups : Customs.Business.JobDeclarationLookups
{
	public JobDeclarationLookups(JobDeclaration parent)
		: base(parent)
	{
	}

	public override CodeDescriptionPairList MessageSubTypeList => new();

	public override CodeDescriptionPairList PaymentPartyList => Factory.GetCachedValue<PaymentByList>();

	public override CodeDescriptionPairList EntryStatusList => Factory.GetCachedValue<AEEntryStatusList>();

	public override CodeDescriptionPairList OperationalStatusList => Factory.GetCachedValue<OperationalStatusList>();

	public override CodeDescriptionPairList ApplicationCodeList
		=> Factory.GetCachedValue<CodeDescriptionPairList>("AE|JobDeclarationLookups|ApplicationCodeList",
			() =>
			{
				var result = new AEDeclarationApplicationCodeList();
				result.AddPair(DeclarationApplicationCodeList.Codes.Interfaced, DeclarationApplicationCodeList.Descriptions.Interfaced);
				return result;
			});

	protected override ZQuery FinalDestinationPortFilter()
	{
		var result = new ZQuery();
		if (Parent.IsTransit || Parent.IsTranshipment)
		{
			result.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, Core.Constants.CountryCodes.UnitedArabEmirates);
		}
		else
		{
			result.AddToFilter(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.StartsWith, Core.Constants.CountryCodes.UnitedArabEmirates);
		}
		return result;
	}

	public override CodeDescriptionPairList TransportTypeList
	{
		get
		{
			var isApplicationCodeDubai = Parent.IsApplicationCodeDubai;
			return Factory.GetCachedValue("AE|JobDeclarationLookups|TransportTypeList|" + (isApplicationCodeDubai ? "AEDeclarationApplicationCodeDubai" : "Other"), () =>
			{
				var result = new TransportTypeList();
				if (isApplicationCodeDubai)
				{
					result.AddPair(AETransportTypeList.Codes.Courier, AETransportTypeList.Descriptions.Courier);
					result.AddPair(AETransportTypeList.Codes.Coastal, AETransportTypeList.Descriptions.Coastal);
					result.AddPair(AETransportTypeList.Codes.CourierLand, AETransportTypeList.Descriptions.CourierLand);
					result.AddPair(AETransportTypeList.Codes.CourierAir, AETransportTypeList.Descriptions.CourierAir);
					result.AddPair(AETransportTypeList.Codes.Passenger, AETransportTypeList.Descriptions.Passenger);
					result.Sort();
				}
				return result;
			});
		}
	}

	protected override IEnumerable<ZString> GetNonSupportedMessageTypeCodes()
	{
		if (Parent.IsApplicationCodeDubai)
		{
			return new ZString[] { AEJobMessageTypeList.Codes.Drawback, AEJobMessageTypeList.Codes.ExWarehouse, AEJobMessageTypeList.Codes.Refund, AEJobMessageTypeList.Codes.MiscellaneousCustoms };
		}
		else
		{
			return new ZString[] { AEJobMessageTypeList.Codes.Transit, AEJobMessageTypeList.Codes.TemporaryAdmission, AEJobMessageTypeList.Codes.Transfer, AEJobMessageTypeList.Codes.CargoTransfer };
		}
	}

	public CodeDescriptionPairList PlaceOfDischargeList => GetCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UAEPlaceofDischarge);

	public CodeDescriptionPairList ExitPointList => GetCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UAEExitPoint);

	public CodeDescriptionPairList TypeOfGoodsList
	{
		get { return Factory.GetCachedValue<TypeOfGoodsList>(); }
	}

	public CodeDescriptionPairList ClearanceLocationList => GetCusCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UAEClearanceLocation);

	CodeDescriptionPairList GetCusCodeList(ZString typeCode)
	{
		return Factory.GetCachedValue($"UAERefCusCodeList_{typeCode}_{ZDateTime.Today.ToISO8601ShortDateString()}", () =>   // Cache Key
		{
			var cusCodeList = ZZRefCusCodeListCombined.Loader
				.Load(Factory, Core.Constants.CountryCodes.UnitedArabEmirates, typeCode, ZDateTime.Today)
				.OrderBy(x => x.ZZD_Description)
				.AsEnumerable();

			var result = new CodeDescriptionPairList();
			result.AddPairsIfNotExist(cusCodeList);
			return result;
		});
	}

	new JobDeclaration Parent => (JobDeclaration)base.Parent;
}
