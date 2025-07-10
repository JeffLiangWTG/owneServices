using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public abstract class PurgeSettingsConfig
	{
		public abstract IEnumerable<ApplicationCodeObj> GetPurgeSettings();

		protected InterchangeObj NewInterchangeConfigObj(ZShort purgeTime, ZGuid purgeTimeUnit)
		{
			return new InterchangeObj
			{
				PurgeTime = purgeTime,
				PurgeTimeUnit = purgeTimeUnit,
				Selected = true
			};
		}

		protected ApplicationCodeObj AddApplicationCodePurgeType(string code, InterchangeObjCollection interchanges, ZShort purgeTime, ZGuid purgeTimeUnit)
		{
			var appCode = new ApplicationCodeObj();
			appCode.PurgeType = PurgeTypeList.ApplicationCode;
			appCode.ApplicationCode = code;
			appCode.Interchanges.AddRange(interchanges);
			appCode.PurgeTime = purgeTime;
			appCode.PurgeTimeUnit = purgeTimeUnit;
			return appCode;
		}

		protected ApplicationCodeObj AddApplicationCodeMessageTypePurgeType(string code, InterchangeObjCollection interchanges, MessageTypePurgeTypeObjCollection messageTypes)
		{
			var appCode = new ApplicationCodeObj();
			appCode.PurgeType = PurgeTypeList.MessageType;
			appCode.ApplicationCode = code;
			appCode.Interchanges.AddRange(interchanges);
			appCode.SetMessageTypes(messageTypes);
			return appCode;
		}

		protected ApplicationCodeObj AddApplicationCodeMessageSubTypePurgeType(string code, InterchangeObjCollection interchanges, MessageSubTypePurgeTypeObjCollection messageTypes)
		{
			var appCode = new ApplicationCodeObj();
			appCode.PurgeType = PurgeTypeList.MessageSubType;
			appCode.ApplicationCode = code;
			appCode.Interchanges.AddRange(interchanges);
			appCode.SetMessageTypes(messageTypes);
			return appCode;
		}

		protected ApplicationCodeObj AddApplicationCodeMessageTypeAndSubTypePurgeType(string code, InterchangeObjCollection interchanges, MessageTypeAndSubTypePurgeTypeObjCollection messageTypes)
		{
			var appCode = new ApplicationCodeObj();
			appCode.PurgeType = PurgeTypeList.MessageTypeAndSubType;
			appCode.ApplicationCode = code;
			appCode.Interchanges.AddRange(interchanges);
			appCode.SetMessageTypes(messageTypes);
			return appCode;
		}

		/// <summary>
		/// This method should only be used if an Application code cannot be purged at all. Please do not use this method for application codes that can be purged but need to persist for a certain period of time as setting and enforcing a minimum purge time is a better option for this scenario. 
		/// </summary>
		protected ApplicationCodeObj AddUnpurgableApplicationCode(string code)
		{
			var appCode = new ApplicationCodeObj();
			appCode.ApplicationCode = code;
			appCode.PurgeType = PurgeTypeList.ApplicationCode;
			appCode.IsUnpurgable = true;
			appCode.Selected = false;
			return appCode;
		}
	}
}
