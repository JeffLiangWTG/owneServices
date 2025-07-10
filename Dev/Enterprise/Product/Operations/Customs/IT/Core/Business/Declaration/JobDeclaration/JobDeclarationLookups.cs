using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Common.IT;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using EUBusiness = Enterprise.Customs.EU.Business;
using InlandTransportTypeListCodes = Enterprise.Customs.Business.TransportTypeList.Codes;

namespace Enterprise.Customs.IT.Business.Declaration;

public partial class JobDeclarationLookups : EUBusiness.Declaration.JobDeclarationLookups
{
	public JobDeclarationLookups(JobDeclaration parent)
		: base(parent)
	{
	}

	protected new JobDeclaration Parent => (JobDeclaration)base.Parent;

	public override GlbStaffCollection CusAgents => SubscribersProvider.GetSubscribers();

	public override CodeDescriptionPairList ProfileList
	{
		get
		{
			var customsProfileListProvider = GetCustomsProfileListProvider();
			return customsProfileListProvider.GetAccountDetails();
		}
	}

	public override ICodeDescriptionPairList DeclarantTypeList => GetDeclarantTypeListCached();

	protected override ICollection LocationsCore => (goodsLocationProvider ?? (goodsLocationProvider = new JobDeclarationGoodsLocationListProvider(Parent, Factory))).Locations;
	JobDeclarationGoodsLocationListProvider goodsLocationProvider;

	public override CodeDescriptionPairList EntryStatusList => Factory.GetCachedValue<ITEntryStatusList>();

	public RefCountryCollection SubLocationOfGoodsList => new RefCountryCollection(Factory, new ZQuery(RefCountrySchema.RN_EconomicGrouping, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN));

	protected override ZQuery FinalDestinationPortFilter()
	{
		if (Parent.IsImport)
		{
			var euCountriesIncludingSanMarino = Factory.GetEuropeanUnionForCustomsMembersAndCustomsUnionAdditionalMembers().ToList();
			euCountriesIncludingSanMarino.Add(Core.Constants.CountryCodes.SanMarino);
			return PortFilter(euCountriesIncludingSanMarino.ToArray(), negativeCountries: null);
		}

		return base.FinalDestinationPortFilter();
	}

	public override CodeDescriptionPairList TransportMeansList => GetTransportMeansList();

	public CodeDescriptionPairList LocationQualifierList
	{
		get
		{
			if (!Parent.IsImport)
			{
				return new CodeDescriptionPairList();
			}

			if (Parent.ZG_AuthorisationNumber.IsEmpty)
			{
				return Factory.GetCachedValue<GoodsLocationQualifierList>();
			}

			return Factory.GetCachedValue<ImportGoodsLocationQualifierWithAuthorisationList>();
		}
	}

	public CodeDescriptionPairList MessageVersionList => Factory.GetCachedValue<MessageVersionList>();

	public CodeDescriptionPairList DefermentApprovalNumberList => GetDefermentApprovalNumberListTruncated();

	protected override IEnumerable<ZString> GetNonSupportedMessageTypeCodes()
	{
		var baseCodes = base.GetNonSupportedMessageTypeCodes().ToList();
		baseCodes.Add(ITJobMessageTypeList.Codes.TemporaryStorage);

		return baseCodes;
	}

	public override CodeDescriptionPairList PaymentPartyList
	{
		get
		{
			var parent = Parent;
			if (parent.IsImport)
			{
				return Factory.GetCachedValue<ImportDefermentMethodList>();
			}

			if (parent.IsUCC6AndIsExport)
			{
				return Factory.GetCachedValue<Ucc6ExportDefermentMethodList>();
			}

			return base.PaymentPartyList;
		}
	}

	public override OrgHeaderCollection Buyers => new OrgHeaderCollection(Factory);

	#region Implementation

	CodeDescriptionPairList GetTransportMeansList()
	{
		var declaration = Parent;
		if (declaration.IsUCC6AndIsExport)
		{
			return GetUcc6ExportTransportMeansList(declaration.JE_TransportModeInland);
		}
		return base.TransportMeansList;
	}

	CodeDescriptionPairList GetUcc6ExportTransportMeansList(ZString transportModeInland)
	{
		return Factory.GetCachedValue($"IT.JobDeclarationLookups.TransportMeansList.UCC6Export.{transportModeInland}", () =>
		{
			var result = new CodeDescriptionPairList();
			switch (transportModeInland)
			{
				case InlandTransportTypeListCodes.FixedTransportInstallations:
				case InlandTransportTypeListCodes.OwnPropulsion:
				case InlandTransportTypeListCodes.Mail:
					return base.TransportMeansList;

				case InlandTransportTypeListCodes.InlandWaterwayTransport:
					result.AddPair(Customs.Business.TransportMeansList.Codes.EuropeanVesselIdentificationNumberEniCode, Customs.Business.TransportMeansList.Descriptions.EuropeanVesselIdentificationNumberEniCode);
					result.AddPair(Customs.Business.TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel, Customs.Business.TransportMeansList.Descriptions.NameOfTheInlandWaterwaysVessel);
					result.DefaultCode = Customs.Business.TransportMeansList.Codes.NameOfTheInlandWaterwaysVessel;
					break;

				case InlandTransportTypeListCodes.Sea:
					result.AddPair(Customs.Business.TransportMeansList.Codes.ImoShipIdentificationNumber, Customs.Business.TransportMeansList.Descriptions.ImoShipIdentificationNumber);
					result.AddPair(Customs.Business.TransportMeansList.Codes.NameOfTheSeaGoingVessel, Customs.Business.TransportMeansList.Descriptions.NameOfTheSeaGoingVessel);
					result.DefaultCode = Customs.Business.TransportMeansList.Codes.NameOfTheSeaGoingVessel;
					break;
			}

			return result;
		});
	}

	SubscriberListProvider SubscribersProvider => subscribersProvider ?? (subscribersProvider = new SubscriberListProvider(Factory, Parent));
	SubscriberListProvider subscribersProvider;

	ICustomsProfileListProvider GetCustomsProfileListProvider()
	{
		var supportingDataAdapter = new JobDeclarationCustomsProfileListLoaderSupportingDataAdapter(Parent);

		return Parent.IsUCC6
			? new AccountCustomsProfileListProvider(Factory, supportingDataAdapter)
			: new NodeCustomsProfileListProvider(Factory, supportingDataAdapter);
	}

	DefermentApprovalNumberListProvider DefermentApprovalNumberListProvider => deferementApprovalNumberListProvider ?? (deferementApprovalNumberListProvider = new DefermentApprovalNumberListProvider(Parent, Factory));
	DefermentApprovalNumberListProvider deferementApprovalNumberListProvider;

	CodeDescriptionPairList GetDeclarantTypeListCached()
	{
		return Factory.GetCachedValue(FormattableString.Invariant($"IT.JobDeclarationLookups.DeclarantTypeList_IsUCC6:{Parent.IsUCC6}"), () =>
		{
			var list = new EUBusiness.RepresentationTypeList();

			if (Parent.IsUCC6)
			{
				list.RemoveCode(EUBusiness.RepresentationTypeList.Codes._1Self);
			}

			return list;
		});
	}

	CodeDescriptionPairList GetDefermentApprovalNumberListTruncated()
	{
		var defermentApprovalNumberList = GetDefermentApprovalNumberList();
		var defermentApprovalNumberListTruncated = new CodeDescriptionPairList();
		foreach (ICodeDescription item in defermentApprovalNumberList)
		{
			defermentApprovalNumberListTruncated.AddPair(new ZString(item.Code).Left(Parent.JE_DefermentAccountNumberInfo.MaxLength));
		}
		return defermentApprovalNumberListTruncated;
	}

	CodeDescriptionPairList GetDefermentApprovalNumberList()
	{
		var parent = Parent;
		if (parent.IsImport)
		{
			return DefermentApprovalNumberListProvider.GetDefermentApprovalNumberListForImport();
		}

		if (parent.IsUCC6AndIsExport)
		{
			return DefermentApprovalNumberListProvider.GetDefermentApprovalNumberListForUcc6Export();
		}

		return DefermentApprovalNumberListProvider.GetDefaultDefermentApprovalNumberList();
	}

	#endregion
}
