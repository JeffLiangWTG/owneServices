using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;

namespace CargoWise.EntityFramework
{
	public static class ParameterSettingsCache
	{
#if DEBUG
		public static void RefreshCache()
		{
			fieldsToLiteralize = null;
			tVPRule = null;
		}
#endif

		[ThreadStatic]
		static IEnumerable<string> fieldsToLiteralize;
		public static IEnumerable<string> FieldsToLiteralize
		{
			get
			{
				if (fieldsToLiteralize == null)
				{
					fieldsToLiteralize = ObjectFactory.Get<IEntityFrameworkSettings>().FieldsToLiteralize
						.Replace(" ", "")
						.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
						.Distinct(StringComparer.OrdinalIgnoreCase)
						.ToArray()
						;
				}

				return fieldsToLiteralize;
			}
		}

		[ThreadStatic]
		static TVPRule tVPRule;
		public static TVPRule TVPRule
		{
			get { return (tVPRule ?? (tVPRule = ObjectFactory.Get<IEntityFrameworkSettings>().TVPRule)); }
		}
	}
}
