using System;
using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Utils.Models;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DataTransfer.Native.Business.Update.LanguageCodes
{
	class LanguageCodesInterceptor : BaseInterceptor
	{
		public LanguageCodesInterceptor(IInterceptorSetting setting, AncillaryImportServices sessionServices)
			: base(setting, sessionServices) { }

		public LanguageCodesSetting Setting
		{
			get { return (LanguageCodesSetting)InterceptorSetting; }
		}

		public override void Invoke(IEntitySet entitySet)
		{
			entitySet.Root.DepthFirstTraversal((entity, relative) => { UpdateLanguageToISOCode(entity); });

			Function(entitySet);
		}

		readonly Lazy<HashSet<string>> validLanguageCodes = new Lazy<HashSet<string>>(() =>
		{
			var validCodes = new HashSet<string>();
			foreach (var languageMap in Culture.LanguageCodeMapping)
			{
				validCodes.Add(languageMap.Value);
			}
			return validCodes;
		});

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "safe string")]
		public void UpdateLanguageToISOCode(IEntity entity)
		{
			if (entity.HasProperty("Language"))
			{
				var languageStr = entity["Language"].ToString();

				if (Culture.LanguageCodeMapping.TryGetValue(languageStr, out string newCode))
				{
					entity["Language"] = newCode;
				}
				else if (!validLanguageCodes.Value.Contains(languageStr))
				{
					if (!string.IsNullOrEmpty(languageStr))
					{
						entity["Language"] = LanguageHelper.GetCustomLanguageByFullLanguageCodeOrDescription(languageStr, Setting.Context.ObjectFactory)?.FullLanguageCode ?? Core.SharedConstants.Languages.English;
					}
					else
					{
						entity["Language"] = Core.SharedConstants.Languages.English;
					}
				}
			}
		}
	}
}
