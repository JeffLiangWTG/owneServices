using System;
using System.Collections;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.TemporaryStorage;

namespace Enterprise.Customs.EU.Business
{
	public class GuaranteeCountrySpecificInstruction : Customs.Business.GuaranteeCountrySpecificInstruction
	{
		public GuaranteeCountrySpecificInstruction(BusinessObjectFactory factory) : base(factory)
		{
		}

		public override CodeDescriptionPairList GetTypeList(ZString countryCode)
		{
			var result = base.GetTypeList(countryCode);
			var nctsSettings = ObjectFactory.Get<Integration.Customs.Shared.INctsSettings>();
			if (!nctsSettings.IsNctsEnabled)
			{
				result.RemoveCode(EUGuaranteeTypeList.Codes.TRA);
				result.RemoveCode(EUGuaranteeTypeList.Codes.COD);
			}
			result.Sort();
			return result;
		}

		protected override CodeDescriptionPairList GetTypeCodeDescriptionPairList() => Factory.GetCachedValue<EUGuaranteeTypeList>();

		public override CodeDescriptionPairList GetSubTypeList(ZString typeCode)
		{
			if (typeCode == EUGuaranteeTypeList.Codes.TRA || typeCode == EUGuaranteeTypeList.Codes.COD)
			{
				return Factory.GetCachedValue("9CF80646-0E95-430B-9CED-B9761E804E6F", delegate
				{
					return NCTSGuaranteeSubTypeList;
				});
			}
			else
			{
				return base.GetSubTypeList(typeCode);
			}
		}

		protected virtual CodeDescriptionPairList NCTSGuaranteeSubTypeList
		{
			get
			{
				var list = new EUNctsGuaranteeTypeList();
				list.RemoveCode(EUNctsGuaranteeTypeList.Codes.FlatRateVoucher);
				return list;
			}
		}

		protected virtual string PermitGuaranteeTypeCore => EUGuaranteeTypeList.Codes.TRA;

		public string PermitGuaranteeType => PermitGuaranteeTypeCore;

		protected override bool SupportsAdditionalCustomsReferencesCore(ZString guaranteeType) => guaranteeType == EUGuaranteeTypeList.Codes.COD;

		protected override CodeDescriptionPairList AdditionalCustomsReferenceTypesCore => Factory.GetCachedValue<AdditionalCustomsReferenceTypeList>();

		public override Customs.Business.PermitRuleCodeList GetRuleCodeList(ZString permitType, ZString permitSubType) => Factory.GetCachedValue($"ruleCodeList|{permitType}", () =>
		{
			PermitRuleCodeList result = new PermitRuleCodeList();

			if (permitType != EUGuaranteeTypeList.Codes.TRA && permitType != EUGuaranteeTypeList.Codes.COD)
			{
				result.RemoveCode(PermitRuleCodeList.Codes.PCD);
				result.RemoveCode(PermitRuleCodeList.Codes.PCP);
				result.RemoveCode(PermitRuleCodeList.Codes.PCV);
			}

			return result;
		});

		public override Customs.Business.PermitRuleCodeList GetRuleCodeListForModule() => Factory.GetCachedValue<PermitRuleCodeList>();

		public override ZString GetValueFromFieldType(ZString ruleCode)
		{
			switch (ruleCode)
			{
				case PermitRuleCodeList.Codes.ADD:
				case PermitRuleCodeList.Codes.CUS:
				case PermitRuleCodeList.Codes.TSP:
					return nameof(FieldType.TextCodeFindBox);
				case PermitRuleCodeList.Codes.INV:
				case PermitRuleCodeList.Codes.LAP:
					return nameof(FieldType.TextDropEdit);
				default:
					return base.GetValueFromFieldType(ruleCode);
			}
		}

		public override ZString GetValueToFieldType(ZString ruleCode)
		{
			switch (ruleCode)
			{
				case PermitRuleCodeList.Codes.INV:
					return nameof(FieldType.TextDropEdit);
				default:
					return base.GetValueFromFieldType(ruleCode);
			}
		}

		public override ICollection GetLookupList(SharedCusPermitHeader permitHeader, ZString ruleCode)
		{
			switch (ruleCode)
			{
				case PermitRuleCodeList.Codes.ADD:
					return Factory.GetCachedValue("EU.GuaranteeCountrySpecificInstruction.AddressList." + permitHeader?.CPH_OH_PermitHolder, () => GetLookupAddressCollection(permitHeader));
				case PermitRuleCodeList.Codes.TSP:
					return Factory.GetCachedValue("EU.GuaranteeCountrySpecificInstruction.PremisesList." + permitHeader?.CPH_OH_PermitHolder, () => (ICusTempStorageRegPremisesCollection)Activator.CreateInstance(ObjectFactory.GetType<ICusTempStorageRegPremisesCollection>(), new object[] { Factory }));
				default:
					return base.GetLookupList(permitHeader, ruleCode);
			}
		}

		OrgAddressCollection GetLookupAddressCollection(SharedCusPermitHeader permitHeader) => permitHeader switch
		{
			null => new OrgAddressCollection(Factory), // When called by Enterprise.Customs.Module.PermitRuleModuleFilter.LookupList
			{ PermitHolder: null } => new OrgAddressCollection(Factory, ZQuery.NoResultQuery),
			_ => GetLookupAddressCollectionFilteredByPermitHolder(permitHeader.CPH_OH_PermitHolder)
		};

		OrgAddressCollection GetLookupAddressCollectionFilteredByPermitHolder(ZGuid permitHolderPK)
		{
			var addresses = new OrgAddressCollection(Factory, new ZQuery(OrgAddressSchema.OA_OH, permitHolderPK));
			addresses.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault("Organisation", "Property", permitHolderPK, false));
			return addresses;
		}

		public override PermitMatchingType GetMatchingType(ZString ruleCode)
		{
			switch (ruleCode)
			{
				case PermitRuleCodeList.Codes.ADD:
					return PermitMatchingType.SingleValue;
				default:
					return base.GetMatchingType(ruleCode);
			}
		}
	}
}
