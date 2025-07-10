using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public sealed class UniversalCopyAddInfoPropertyMappingAttribute : Attribute
	{
		public UniversalCopyAddInfoPropertyMappingAttribute(string addInfoPropertyInfoName)
		{
			AddInfoPropertyInfoName = Argument.NotNull(addInfoPropertyInfoName, nameof(addInfoPropertyInfoName));
		}

		public string AddInfoPropertyInfoName { get; }
	}
}
