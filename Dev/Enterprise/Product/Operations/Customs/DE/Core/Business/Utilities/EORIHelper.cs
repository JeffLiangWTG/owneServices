using System;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Registry;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.DE.Messaging.MessageSchema;

namespace Enterprise.Customs.DE.Business
{
	public static class EORIHelper
	{
		public static bool ValidEoriLength(ZString eoriCode) => ((ZInt)eoriCode.Length).IsInRange(2, ATLASMessageSchema.EoriCodeMaxLength);

		public static OrgHeader GetOrgHeaderFromEoriCode(this BusinessObjectFactory factory, ZString eoriCode)
		{
			var (countryPrefix, eoriNumber) = eoriCode.SplitEoriDetails();
			var query = new ZQuery(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryPrefix);
			query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			query.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, eoriNumber);
			return factory.Load<OrgCusCode>(query).FirstOrDefault()?.Organisation;
		}

		public static bool IsCW1Organization(this BusinessObjectFactory factory, ZString eoriCode, ZString eoriBranch) => IsCW1OrganizationCore(factory, eoriCode, eoriBranch, false);

		public static bool IsCW1MessagingOrganization(this BusinessObjectFactory factory, ZString eoriCode, ZString eoriBranch) => IsCW1OrganizationCore(factory, eoriCode, eoriBranch, true);

		static bool IsCW1OrganizationCore(this BusinessObjectFactory factory, ZString eoriCode, ZString eoriBranch, bool requireThisOrganizationHasAPI)
		{
			var (countryPrefix, eoriNumber) = eoriCode.SplitEoriDetails();
			var mainQuery = new ZDBOnlyQuery(typeof(OrgCusCode));
			mainQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix);
			mainQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Germany);
			mainQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, eoriBranch);
			var eoriSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
			eoriSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori);
			eoriSubQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, countryPrefix);
			eoriSubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, eoriNumber);
			mainQuery.AddSubQuery(OrgCusCodeSchema.OK_OH, eoriSubQuery, JoinCondition.And);
			if (requireThisOrganizationHasAPI)
			{
				var eoriBranchSuffixOrgAddressSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OA_PremisesAddress);
				eoriBranchSuffixOrgAddressSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, GermanyOrgCusCodeInfo.OrgCusCodes.EoriBranchSuffix);
				eoriBranchSuffixOrgAddressSubQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Germany);
				eoriBranchSuffixOrgAddressSubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, eoriBranch);
				var apiSubQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				apiSubQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber);
				apiSubQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.Germany);
				apiSubQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, SQLComparisonOperator.NotEqual, ZString.Empty);
				apiSubQuery.AddSubQuery(OrgCusCodeSchema.OK_OA_PremisesAddress, eoriBranchSuffixOrgAddressSubQuery, JoinCondition.And);
				mainQuery.AddSubQuery(OrgCusCodeSchema.OK_OH, apiSubQuery, JoinCondition.And);
			}

			return factory.Exists(typeof(OrgCusCode), mainQuery);
		}

		public static (ZString countryPrefix, ZString eoriNumber) SplitEoriDetails(this ZString eoriNumber) => (eoriNumber.SubstringSafe(0, 2), eoriNumber.SubstringSafe(2, ATLASMessageSchema.EoriCodeMaxLength - 2));

		public static (IPartyID Sender, ZString Bin) GetImportMessageSenderAndBinDetails(JobDeclaration declaration)
		{
			var senderAddress = declaration.JE_DeclarantType == RepresentationTypeList.Codes._2Direct ? declaration.Representative : declaration.Declarant;
			var api = senderAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber);

			return api.IsEmpty ? GetSenderDetailsFromRegistry() : (ImportPartyIDProvider.NewOrNull(senderAddress), api);
		}

		public static (IPartyID Sender, ZString Bin) GetImportMessageSenderAndBinDetails(CusReconDeclaration declaration)
		{
			var senderAddress = declaration.CRD_DeclarantType == RepresentationTypeList.Codes._2Direct ? declaration.RepresentativeAddress : declaration.DeclarantAddress;
			var api = senderAddress.GetCustomsRegNo(GermanyOrgCusCodeInfo.OrgCusCodes.ATLASParticipantIdentificationNumber);

			return api.IsEmpty ? GetSenderDetailsFromRegistry() : (ImportPartyIDProvider.NewOrNull(senderAddress), api);
		}

		public static (IPartyID Sender, ZString Bin) GetSenderDetailsFromRegistry()
		{
			var eori = DECustomsDataRegistry.Instance.ATLASEORINumber.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var ebs = DECustomsDataRegistry.Instance.ATLASEORIBranchSuffix.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var api = DECustomsDataRegistry.Instance.ATLASParticipantIdentificationNumber.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			return (new EoriAndBranchSuffix(eori, ebs), api);
		}

		public static ZString GetMissingEoriMessage(bool eoriCodeMissing, bool eoriBranchMissing)
		{
			ZString result;
			if (eoriCodeMissing && eoriBranchMissing)
			{
				result = Res.GetString("f138f770-0783-417d-9b94-888bf4223139", "EORI number and branch.");
			}
			else
			{
				result = eoriCodeMissing ? Res.GetString("2ef7b470-5451-479c-a314-eeabd7a23951", "EORI number.") : Res.GetString("54f55d6a-b321-4773-ae43-f57efb73fd0d", "EORI branch.");
			}
			return result;
		}
	}

	sealed class EoriAndBranchSuffix : IPartyID
	{
		public EoriAndBranchSuffix(string eoriNumber, string eoriBranchSuffix)
		{
			EoriNumber = eoriNumber;
			EoriBranchSuffix = eoriBranchSuffix;
		}

		public string EoriNumber { get; private set; }

		public string EoriBranchSuffix { get; private set; }

		public string TCUNumber => null;
	}
}
