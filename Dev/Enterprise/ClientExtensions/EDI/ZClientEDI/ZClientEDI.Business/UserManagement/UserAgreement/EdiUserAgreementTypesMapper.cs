using System.Collections.Generic;
using System.Collections.ObjectModel;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using WTG.StaticAnalysis.Annotation;
using Codes = Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementTypes.Codes;
using Descriptions = Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementTypes.Descriptions;

namespace Enterprise.Client.EDI.UserManagement.Business
{
	public readonly struct AgreementTypeInfo
	{
		public AgreementTypeInfo() { }

		public AgreementTypeInfo(string code, string description)
		{
			Code = code;
			Description = description;
		}

		public AgreementTypeInfo(string code, string description, HashSet<string> bindingTableCodes, bool enableVariant, bool isVariantMandatory)
		{
			Code = code;
			Description = description;
			EnableVariant = enableVariant;
			BindingTableCodes = bindingTableCodes;
			IsVariantMandatory = enableVariant && isVariantMandatory;
		}

		public readonly string Code = string.Empty;
		public readonly string Description = string.Empty;
		public readonly HashSet<string> BindingTableCodes = new HashSet<string>();
		public readonly bool EnableVariant;
		public readonly bool IsVariantMandatory;
	}

	public static class EdiUserAgreementTypesMapper
	{
		[ThreadSafe]
		static readonly ReadOnlyDictionary<string, AgreementTypeInfo> Mapper = new(new Dictionary<string, AgreementTypeInfo>
		{
			{ Codes.CargoWiseNext, new AgreementTypeInfo(Codes.CargoWiseNext, Descriptions.CargoWiseNext, new HashSet<string> { LicenceEnterpriseSchema.Constants.Prefix, LicenceDatabaseSchema.Constants.Prefix }, true, true) },
			{ Codes.CreditCheckService, new AgreementTypeInfo(Codes.CreditCheckService, Descriptions.CreditCheckService) },
			{ Codes.DeniedPartyScreening,  new AgreementTypeInfo(Codes.DeniedPartyScreening, Descriptions.DeniedPartyScreening) },
			{ Codes.MyAccountLoginUserInfo,  new AgreementTypeInfo(Codes.MyAccountLoginUserInfo, Descriptions.MyAccountLoginUserInfo) },
			{ Codes.RouteVisualiserVesselTrackingSystem, new AgreementTypeInfo(Codes.RouteVisualiserVesselTrackingSystem, Descriptions.RouteVisualiserVesselTrackingSystem) },
			{ Codes.UserAccountCollection,  new AgreementTypeInfo(Codes.UserAccountCollection, Descriptions.UserAccountCollection) },
		});

		public static HashSet<string> GetBindingTableCodes(string agreementType)
		{
			var result = new HashSet<string>();

			if (!string.IsNullOrEmpty(agreementType) && Mapper.TryGetValue(agreementType, out var info))
			{
				return info.BindingTableCodes;
			}

			return result;
		}

		public static bool IsAgreementTypeValid(string agreementType)
		{
			return Mapper.ContainsKey(agreementType);
		}

		public static string GetAgreementLevel(string agreementType)
		{
			if (!string.IsNullOrEmpty(agreementType) && Mapper.TryGetValue(agreementType, out var info))
			{
				switch (info.Code)
				{
					case Codes.CargoWiseNext:
						return EdiUserAgreementLevelList.Codes.Corporate;
					default:
						return EdiUserAgreementLevelList.Codes.User;
				}
			}

			return EdiUserAgreementLevelList.Codes.User;
		}

		public static bool IsVariantEnabled(string agreementType)
		{
			if (string.IsNullOrEmpty(agreementType))
			{
				return false;
			}

			return Mapper.TryGetValue(agreementType, out var info) && info.EnableVariant;
		}

		public static bool IsVariantMandatory(string agreementType)
		{
			if (!string.IsNullOrEmpty(agreementType) && Mapper.TryGetValue(agreementType, out var info))
			{
				return info.IsVariantMandatory;
			}

			return false;
		}

		public static CodeDescriptionPairList GetParentAvailableAgreementTypeList(string tableCode)
		{
			var result = new CodeDescriptionPairList();
			if (!string.IsNullOrEmpty(tableCode))
			{
				foreach (var item in Mapper)
				{
					if (item.Value.BindingTableCodes.Contains(tableCode))
					{
						result.AddPairIfNotExist(item.Key, item.Value.Description);
					}
				}
			}

			return result;
		}
	}
}
