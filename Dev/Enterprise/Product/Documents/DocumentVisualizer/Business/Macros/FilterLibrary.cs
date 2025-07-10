using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentVisualizer.Business
{
	public class FilterLibrary : MacroLibrary
	{
		protected override IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<BusinessObject, string, bool>>(
					"HasEvent",
					(NoResString)"Return true if contains the giving event.",
					(bizO, eventCode) => HasEvent(bizO, eventCode),
					(NoResString)"HasEvent(\"SBR\")");

				yield return new Handler<Func<BusinessObject, bool>>(
					"IsCreditOnHold",
					(NoResString)"Return true if the business object is in credit on hold status.",
					bizO => IsCreditOnHold(bizO),
					"IsCreditOnHold()");

				yield return new Handler<Func<string, bool>>(
					"IsFranceOrTerritory",
					(NoResString)"Return true if the country code or unloco belongs to France or dependent territorry.",
					locationCode => IsFranceOrTerritory(locationCode),
					(NoResString)"IsFranceOrTerritory(\"FR\")");

				yield return new Handler<Func<string, bool>>(
					"IsUnderFrenchCustomsJurisdiction",
					(NoResString)"Return true if the country code is under French Customs jurisdiction.",
					locationCode => IsUnderFrenchCustomsJurisdiction(locationCode),
					(NoResString)"IsUnderFrenchCustomsJurisdiction(\"FR\")");

				yield return new Handler<Func<string, bool>>(
					"IsInEuropeanCustomsUnionOrInheritsFromEU",
					(NoResString)"Return true if the country is in EU or inherites from EU.",
					countryCode => IsInEuropeanCustomsUnionOrInheritsFromEU(Enterprise.Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode)),
					(NoResString)"IsInEuropeanCustomsUnionOrInheritsFromEU()"); // usage constant
			}
		}

		#region Implementation

		static bool HasEvent(BusinessObject bizOToProcess, string eventCode)
		{
			if (!(bizOToProcess is IStmALogParent logParent))
			{
				throw new MacroRuntimeException("The current data does not support Events.");
			}

			return logParent.Logs.HasLogWith(log => log.SL_SE_NKEvent == eventCode && !log.SL_IsEstimate && !log.SL_IsCancelled);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "no need to translation")]
		static bool IsCreditOnHold(BusinessObject bizOToProcess)
		{
			if (bizOToProcess is OrgHeader orgHeader && orgHeader.CreditChecker != null)
			{
				return orgHeader.CreditChecker.IsCreditOnHold() || orgHeader.CreditChecker.RequiredAuthorizationForCreditControlledDocumentDeliveryDueToExceedingCreditLimit() > 0;
			}

			if (bizOToProcess is ICreditControlledDocumentDelivery)
			{
				return !ObjectFactory.Get<IDocumentDeliveryCreditControlManager>().GetDocumentDeliveryStatusForCreditManagement(bizOToProcess, "document", ZGuid.Empty).IsEmpty;
			}

			throw new MacroRuntimeException("The current data does not applicable for checking credit on hold.");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		static bool IsFranceOrTerritory(string locationCode)
		{
			if (locationCode == null
				|| locationCode.Length < 2)
			{
				return false;
			}

			var countyCode = locationCode.Substring(0, 2);

			switch (countyCode)
			{
				case Enterprise.Core.Constants.CountryCodes.France:
				case Enterprise.Core.Constants.CountryCodes.FrenchGuyana:
				case Enterprise.Core.Constants.CountryCodes.FrenchPolynesia:
				case Enterprise.Core.Constants.CountryCodes.Reunion:
				case Enterprise.Core.Constants.CountryCodes.Guadeloupe:
				case Enterprise.Core.Constants.CountryCodes.Martinique:
				case Enterprise.Core.Constants.CountryCodes.Mayotte:
				case Enterprise.Core.Constants.CountryCodes.NewCaledonia:
				case Enterprise.Core.Constants.CountryCodes.SaintBarthelemy:
				case Enterprise.Core.Constants.CountryCodes.SaintMartin:
				case Enterprise.Core.Constants.CountryCodes.StPierreEtMiquelon:
				case Enterprise.Core.Constants.CountryCodes.WallisAndFutunaIslands:
					return true;

				default:
					return false;
			}
		}
		static bool IsUnderFrenchCustomsJurisdiction(string locationCode) => Enterprise.Core.Constants.CountryCodes.IsUnderFrenchCustomsJurisdiction(locationCode);

		static bool IsInEuropeanCustomsUnionOrInheritsFromEU(string countryCode)
		{
			if (string.IsNullOrEmpty(countryCode))
			{
				return false;
			}

			return ObjectFactory.Get<Enterprise.Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(countryCode);
		}

		#endregion
	}
}
