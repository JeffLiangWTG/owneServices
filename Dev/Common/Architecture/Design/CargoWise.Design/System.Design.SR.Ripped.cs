using System.ComponentModel.Design;
using System.Globalization;
using System.Reflection;
using CargoWise.Common.Testing;

namespace System.Design.Ripped
{
	internal static class SR
	{
		public static string GetString(string name)
		{ return (string)SRType.InvokeMember("GetString", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { name }, CultureInfo.CurrentCulture); }

		public static string GetString(string name, params object[] args)
		{ return (string)SRType.InvokeMember("GetString", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { name, args }, CultureInfo.CurrentCulture); }

		static Type SRType
		{ get { return srType ?? (srType = typeof(ComponentDesigner).Assembly.GetType("System.Design.SR")); } }
		[SuppressThreadStaticFieldMessage]
		static Type srType;
	}
}
