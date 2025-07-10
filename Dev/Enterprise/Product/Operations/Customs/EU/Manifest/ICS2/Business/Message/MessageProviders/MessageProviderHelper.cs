using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.EU.MessageContracts.ICS2;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public static class MessageProviderHelper
	{
		#region Constants

		public static class RefSysConfigCodes
		{
			public const string ICS2SID = "ICS2SID";
			public const string ICS2SMS1 = "ICS2SMS1";
			public const string ICS2SMS2 = "ICS2SMS2";
			public const string ICS2SSYS = "ICS2SSYS";
			public const string ICS2INCSYS = "ICS2INCSYS";
		}

		public static class IntendedCodeTypes
		{
			public const string No = "NO";
			public const string Wtg = "WTG";
			public const string Yes = "YES";
		}

		public const string TestDomainName = "xttest-eidas-gwy.wisegrid.net";
		public const string ProdDomainName = "xt-eidas-gwy.wisegrid.net";

		public const string TestMessageTo = "EUICS2TEST";
		public const string ProdMessageTo = "EUICS2";

		public static string DomainName => Env.Instance.IsProductionSystem ? ProdDomainName : TestDomainName;
		public static string MessageTo => Env.Instance.IsProductionSystem ? ProdMessageTo : TestMessageTo;

		#endregion

		public static IReadOnlyCollection<IAdditionalInformation> GetAdditionalInformationCollection(IEnumerable<CusSupportingInfo> addInfoCollection)
		{
			return ToArray<CusSupportingInfo, IAdditionalInformation>(addInfoCollection, (addInfo) => new AdditionalInformationProvider(addInfo));
		}

		public static IReadOnlyCollection<IIdentifierTypePair> GetSupportingDocuments(IEnumerable<SupportingDocument> supportingDocuments)
		{
			return ToArray<SupportingDocument, IIdentifierTypePair>(supportingDocuments, (supportingDocument) => SupportingDocumentProvider.NewOrNull(supportingDocument));
		}

		public static IIdentifierTypePair GetFirstAdditionalFiscalReference(IEnumerable<AdditionalFiscalReference> additionalFiscalReferences)
		{
			var firstAdditionalFiscalReference = additionalFiscalReferences.FirstOrDefault();
			return AdditionalFiscalReferenceProvider.NewOrNull(firstAdditionalFiscalReference);
		}

		public static ZString GetICS2SenderEORI(BusinessObjectFactory factory)
		{
			var loader = new RefSysConfig.Loader(factory);
			return loader.GetStringValue(RefSysConfigCodes.ICS2SID);
		}

		public static ZString GetICS2SenderMemberState(BusinessObjectFactory factory, GlbCompany profileCompany)
		{
			var result = string.Empty;

			if (ICS2CustomsDataRegistry.Instance.IncludeMemberStateInSiteID.GetFallBackValueAtAllLevels(profileCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty))
			{
				var loader = new RefSysConfig.Loader(factory);

				result = new[] { RefSysConfigCodes.ICS2SMS2, RefSysConfigCodes.ICS2SMS1 }
					.Select(c => loader.GetStringValue(c, ZDateTime.UtcNow))
					.FirstOrDefault(c => !c.IsEmpty);
			}

			return result;
		}

		public static ZString GetICS2SystemID(BusinessObjectFactory factory)
		{
			var loader = new RefSysConfig.Loader(factory);
			var intendedCodeType = loader.GetStringValue(RefSysConfigCodes.ICS2INCSYS, ZDateTime.UtcNow).ToUpperInvariant();

			switch (intendedCodeType)
			{
				case IntendedCodeTypes.Wtg:
					return loader.GetStringValue(RefSysConfigCodes.ICS2SSYS, ZDateTime.UtcNow);
				case IntendedCodeTypes.No:
					return string.Empty;
				default:
					return $"{GlbCompany.CurrentCompany.LicenceEnterpriseCode}{GlbCompany.CurrentCompany.LicenceServerID}";
			}
		}

		public static string GetFromParty(BusinessObjectFactory factory, GlbCompany profileCompany)
		{
			var registryFromParty = ICS2CustomsDataRegistry.Instance.SenderPartyId.GetFallBackValueAtAllLevels(profileCompany?.PK.ToGuid() ?? Guid.Empty, Guid.Empty, Guid.Empty);

			if (string.IsNullOrEmpty(registryFromParty))
			{
				return string.Join("@", new[]
				{
					MessageProviderHelper.GetICS2SystemID(factory),
					MessageProviderHelper.GetICS2SenderEORI(factory),
					MessageProviderHelper.GetICS2SenderMemberState(factory, profileCompany),
				}.Where(c => !c.IsEmpty));
			}
			else
			{
				return registryFromParty;
			}
		}

		#region Time

		public static DateTime? ToNullableDateTime(this ZDateTime dateTime) => dateTime.IsValid ? dateTime.ToDateTime().ToUniversalTime() : null;

		public static DateTime? ToNullableDateTime(this ZDateTimeOffset dateTimeOffset) => dateTimeOffset.IsValid ? dateTimeOffset.ToUtcDateTime() : null;

		public static DateTime UtcDateTime(this ZDateTime dateTime) => dateTime.IsValid ? dateTime.ToDateTime().ToUniversalTime() : default;

		#endregion

		public static IReadOnlyCollection<N> ToArray<M, N>(this IEnumerable collection, Func<M, N> selector)
		{
			return collection.Cast<M>().Select(selector).ToArray();
		}

		public static IReadOnlyCollection<N> ToArray<M, N>(this IEnumerable<M> collection, Func<M, N> selector)
		{
			return collection.Select(selector).ToArray();
		}

		public static ZString GetCustomsMessageType(this AsycudaManifestHeader header, bool isAmending)
		{
			var indicator = header.SpecificCircumstanceIndicator;
			switch (indicator)
			{
				case EUICS2SpecificCircumstanceList.Codes.F10:
					return isAmending ? MessageTypes.Codes.A10 : MessageTypes.Codes.F10;
				case EUICS2SpecificCircumstanceList.Codes.F11:
					return isAmending ? MessageTypes.Codes.A11 : MessageTypes.Codes.F11;
				case EUICS2SpecificCircumstanceList.Codes.F12:
					return isAmending ? MessageTypes.Codes.A12 : MessageTypes.Codes.F12;
				case EUICS2SpecificCircumstanceList.Codes.F13:
					return isAmending ? MessageTypes.Codes.A13 : MessageTypes.Codes.F13;
				case EUICS2SpecificCircumstanceList.Codes.F14:
					return isAmending ? MessageTypes.Codes.A14 : MessageTypes.Codes.F14;
				case EUICS2SpecificCircumstanceList.Codes.F15:
					return isAmending ? MessageTypes.Codes.A15 : MessageTypes.Codes.F15;
				case EUICS2SpecificCircumstanceList.Codes.F16:
					return isAmending ? MessageTypes.Codes.A16 : MessageTypes.Codes.F16;
				case EUICS2SpecificCircumstanceList.Codes.F17:
					return isAmending ? MessageTypes.Codes.A17 : MessageTypes.Codes.F17;
				case EUICS2SpecificCircumstanceList.Codes.F20:
					return isAmending ? MessageTypes.Codes.A20 : MessageTypes.Codes.F20;
				case EUICS2SpecificCircumstanceList.Codes.F21:
					return isAmending ? MessageTypes.Codes.A21 : MessageTypes.Codes.F21;
				case EUICS2SpecificCircumstanceList.Codes.F22:
					return isAmending ? MessageTypes.Codes.A22 : MessageTypes.Codes.F22;
				case EUICS2SpecificCircumstanceList.Codes.F23:
					return isAmending ? MessageTypes.Codes.A23 : MessageTypes.Codes.F23;
				case EUICS2SpecificCircumstanceList.Codes.F24:
					return isAmending ? MessageTypes.Codes.A24 : MessageTypes.Codes.F24;
				case EUICS2SpecificCircumstanceList.Codes.F25:
					return MessageTypes.Codes.F25;
				case EUICS2SpecificCircumstanceList.Codes.F26:
					return isAmending ? MessageTypes.Codes.A26 : MessageTypes.Codes.F26;
				case EUICS2SpecificCircumstanceList.Codes.F27:
					return isAmending ? MessageTypes.Codes.A27 : MessageTypes.Codes.F27;
				case EUICS2SpecificCircumstanceList.Codes.F28:
					return isAmending ? MessageTypes.Codes.A28 : MessageTypes.Codes.F28;
				case EUICS2SpecificCircumstanceList.Codes.F29:
					return isAmending ? MessageTypes.Codes.A29 : MessageTypes.Codes.F29;
				case EUICS2SpecificCircumstanceList.Codes.F30:
					return isAmending ? MessageTypes.Codes.A30 : MessageTypes.Codes.F30;
				case EUICS2SpecificCircumstanceList.Codes.F31:
					return isAmending ? MessageTypes.Codes.A31 : MessageTypes.Codes.F31;
				case EUICS2SpecificCircumstanceList.Codes.F32:
					return isAmending ? MessageTypes.Codes.A32 : MessageTypes.Codes.F32;
				case EUICS2SpecificCircumstanceList.Codes.F33:
					return isAmending ? MessageTypes.Codes.A33 : MessageTypes.Codes.F33;
				case EUICS2SpecificCircumstanceList.Codes.F34:
					return isAmending ? MessageTypes.Codes.A34 : MessageTypes.Codes.F34;
				case EUICS2SpecificCircumstanceList.Codes.F40:
					return isAmending ? MessageTypes.Codes.A40 : MessageTypes.Codes.F40;
				case EUICS2SpecificCircumstanceList.Codes.F41:
					return isAmending ? MessageTypes.Codes.A41 : MessageTypes.Codes.F41;
				case EUICS2SpecificCircumstanceList.Codes.F42:
					return isAmending ? MessageTypes.Codes.A42 : MessageTypes.Codes.F42;
				case EUICS2SpecificCircumstanceList.Codes.F43:
					return isAmending ? MessageTypes.Codes.A43 : MessageTypes.Codes.F43;
				case EUICS2SpecificCircumstanceList.Codes.F44:
					return isAmending ? MessageTypes.Codes.A44 : MessageTypes.Codes.F44;
				case EUICS2SpecificCircumstanceList.Codes.F45:
					return isAmending ? MessageTypes.Codes.A45 : MessageTypes.Codes.F45;
				case EUICS2SpecificCircumstanceList.Codes.F50:
					return isAmending ? MessageTypes.Codes.A50 : MessageTypes.Codes.F50;
				case EUICS2SpecificCircumstanceList.Codes.F51:
					return isAmending ? MessageTypes.Codes.A51 : MessageTypes.Codes.F51;
				default:
					return indicator;
			}
		}
	}
}
