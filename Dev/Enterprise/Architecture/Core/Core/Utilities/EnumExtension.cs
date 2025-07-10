using System;
using System.Reflection;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.ZArchitecture.Core
{
	public static class EnumExtensions
	{
		public static string GetCaption(this Enum value)
		{
			FieldInfo field = value.GetType().GetField(value.ToString());
			var attribute = Attribute.GetCustomAttribute(field, typeof(ResourceStringDataAttribute)) as ResourceStringDataAttribute
				?? throw new ArgumentNullException(nameof(value), "Language translations should be supported. Use ResourceStringDataAttribute to support language translations.");

			return SourceGenerated.ResString._GetMultilingualString(ResourceStringAssemblyIdAttribute.GetAsmid(value.GetType()), attribute.Key, attribute.Caption);
		}
	}
}
