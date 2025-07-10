using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.JP.Common
{
	public static class CustomsRegNumTypeValidation
	{
		public static IReadOnlyList<ZString> JPDefaultList => new ZString[] { OrgCusCode.JapanCodeTypes.LPC, OrgCusCode.JapanCodeTypes.CIE, OrgCusCode.JapanCodeTypes.JAS };

		public static IReadOnlyList<ZString> ForeignDefaultList => new ZString[] { OrgCusCode.JapanCodeTypes.FSB };

		public static IReadOnlyList<ZString> NvcShipperList => new ZString[] { OrgCusCode.JapanCodeTypes.LPC, OrgCusCode.JapanCodeTypes.FSB };

		public static IReadOnlyList<ZString> GetListByImportAndPartyType(bool isImport, bool isShipper, bool isConsignee)
		{
			return (isImport, isShipper, isConsignee) switch
			{
				(true, true, false) => ForeignDefaultList,
				(false, false, true) => ForeignDefaultList,
				_ => JPDefaultList
			};
		}

		public static void ValidateRegNumTypeIsValid(INotificationType notificationType, ZString codeType, IEnumerable<ZString> codeTypeList, ZPropertyInfo customsRegTypeInfo)
		{
			if (!codeTypeList.Contains(codeType))
			{
				customsRegTypeInfo.AddNotification(notificationType, InvalidMessage);
			}
		}

		public static MultilingualString InvalidMessage = ResString.GetMultilingualString("149847B9-6CFE-4E7E-8680-B077246C186A", "The entered code is not in the list.");
	}
}
