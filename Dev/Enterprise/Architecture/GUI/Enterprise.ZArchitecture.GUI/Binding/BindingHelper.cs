using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Windows.UI;

namespace Enterprise.ZArchitecture.GUI
{
	public static class BindingHelper
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Programmatic constant")]
		internal const string InfoSuffix = "Info";

		public static PropertyDescriptor GetDescriptor(BindingContext context, object dataSource, string dataMember, bool throwOnError)
		{
			return GetTarget(context, dataSource, dataMember, throwOnError).Value;
		}

		public static object GetObject(BindingContext context, object dataSource, string dataMember, bool throwOnError)
		{
			return GetTarget(context, dataSource, dataMember, throwOnError).Key;
		}

		static KeyValuePair<object, PropertyDescriptor> GetTarget(BindingContext context, object dataSource, string dataMember, bool throwOnError)
		{
			var info = new BindingMemberInfo(dataMember);
			var bm = context.EnsureListManager(dataSource, info.BindingPath);

			var obj = bm.Position != -1 && bm.Position < bm.Count ? bm.GetCurrent() : null;
			var descriptor = bm.GetItemProperties()[info.BindingField];

			if (throwOnError && descriptor == null)
			{
				throw new InvalidOperationException("Could not find property '" + info.BindingField + "'. dataSource is '" + dataSource.GetType().FullName + "' dataMember='" + dataMember + "'");
			}
			return new KeyValuePair<object, PropertyDescriptor>(obj, descriptor);
		}

		public static string GetNestedControlDataMember(string containerControlBindTo, string containerControlDataMember, string nestedControlBindTo)
		{
			return new KBindingMemberInfo(GetDataMemberBeforeBindTo(containerControlBindTo, containerControlDataMember), nestedControlBindTo).BindingMember;
		}

		public static string GetDataMemberBeforeBindTo(string bindTo, string dataMember)
		{
			var result = dataMember;
			var bindToWithoutDot = bindTo == "." ? "" : bindTo;
			if (dataMember.EndsWith(bindToWithoutDot))
			{
				result = result.Substring(0, dataMember.Length - bindToWithoutDot.Length).TrimEnd('.');
			}
			return result;
		}
	}
}
