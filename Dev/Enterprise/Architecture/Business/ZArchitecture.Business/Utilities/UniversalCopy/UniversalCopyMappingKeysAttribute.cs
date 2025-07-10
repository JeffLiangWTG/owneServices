using System;
using System.Diagnostics.CodeAnalysis;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public sealed class UniversalCopyMappingKeysAttribute : Attribute
	{
		public UniversalCopyMappingKeysAttribute(string relatedPropertyName)
		{
			this.relatedPropertyName = relatedPropertyName;
		}

		public string RelatedKeyPropertyName
		{
			get
			{
				return relatedPropertyName;
			}
		}
		readonly string relatedPropertyName;
	}
}
