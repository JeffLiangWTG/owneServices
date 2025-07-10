using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.OrgCusCode;

namespace Enterprise.Customs.JP.Common
{
	public static class CustomsRegistrationNumberValidation
	{
		static Dictionary<string, (Regex Rex, string Msg)> RegexAndMessageDictionary => regexAndMessageDictionary ??= new()
		{
			{ OrgCusCode.JapanCodeTypes.JAS, (new("^[A-Z0-9]{12}$"), JASMessage) },
			{ OrgCusCode.JapanCodeTypes.LPC, (new("^[0-9]{13}$|^[0-9]{17}$"), LPCMessage) },
			{ OrgCusCode.JapanCodeTypes.CIE, (new("^1([0-9]{7}|[0-9]{11})$|^C0000([0-9]{8}|[0-9]{12})$"), CIEMessage) },
			{ OrgCusCode.JapanCodeTypes.FSB, (new("^F[A-Z0-9]{11}$"), FSBMessage) },
			{ OrgCusCode.JapanCodeTypes.NUC, (new("^[A-Z0-9]{5}$"), NUCMessage) },
			{ OrgCusCode.JapanCodeTypes.AAL, (new("^[A-Z0-9]{1,3}$"), AALMessage) },
		};

		[ThreadStatic]
		static Dictionary<string, (Regex Rex, string Msg)> regexAndMessageDictionary;

		public static void ValidateCustomsCode(INotificationType notificationType, ZString codeType, ZString customsRegNo, ZPropertyInfo customsRegNoInfo)
		{
			if (codeType == OrgCusCode.CodeTypes.ControlledPremisesID)
			{
				ListValidation.IfInvalidCode(notificationType, customsRegNoInfo, CCPMessage);
			}
			else if (codeType == CodeTypes.CarrierCode)
			{
				var newfactory = new BusinessObjectFactory();
				var carrierLoader = new ZZRefCarrierCombined.Loader(newfactory);
				if (carrierLoader.LoadFromCode(Core.Constants.CountryCodes.Japan, customsRegNo) == null)
				{
					customsRegNoInfo.AddNotification(notificationType, CCCMessage);
				}
			}
			else if (RegexAndMessageDictionary.TryGetValue(codeType, out var regexAndMessage) && !regexAndMessage.Rex.IsMatch(customsRegNo))
			{
				customsRegNoInfo.AddNotification(notificationType, regexAndMessage.Msg);
			}
		}

		public static MultilingualString JASMessage = ResString.GetMultilingualString("D53EB87B-796B-4CA3-BE40-2EEC1B37A3F8", "Please enter exactly 12 characters consisting solely of digits and capital letters [A-Z].");
		public static MultilingualString LPCMessage = ResString.GetMultilingualString("DF9D3C31-044A-479A-A341-31D5E55908FF", "Please enter 13 digits, or 17 digits.");
		public static MultilingualString CIEMessage = ResString.GetMultilingualString("C37573F3-93EE-4A62-87E8-1D8EBD90C997", "Please enter C0000 followed by 8 or 12 digits, or 1 followed by 7 or 11 digits.");
		public static MultilingualString FSBMessage = ResString.GetMultilingualString("D03AA1A3-A677-4FC1-BC56-981B13BC604F", "Please enter exactly 12 characters consisting solely of digits and capital letters [A-Z]. The first character must be F.");
		public static MultilingualString NUCMessage = ResString.GetMultilingualString("8049ADB8-D429-43F4-89B5-6332D37C4812", "Please enter exactly 5 characters consisting solely of digits and capital letters [A-Z].");
		public static MultilingualString CCPMessage = ResString.GetMultilingualString("11612FA8-8991-4B5B-8736-18865F08B0C4", "The entered Bonded Location Code does not exist. To view the complete list, go to Maintain > Customs > Global Codes, then set the Country/Region or Grouping to JP and List Type to JPBLC.");
		public static MultilingualString CCCMessage = ResString.GetMultilingualString("AC243D4C-EA89-4341-B561-1C0C88347294", "The entered value is not a valid Japan Global Carrier Code. To add one, go to Maintain > Customs > Global Carriers.");
		public static MultilingualString AALMessage = ResString.GetMultilingualString("CE77CF62-F24A-48FD-BA1D-C33E9434FD17", "Please enter no longer than 3 characters consisting solely of digits and capital letters [A-Z].");
	}
}
