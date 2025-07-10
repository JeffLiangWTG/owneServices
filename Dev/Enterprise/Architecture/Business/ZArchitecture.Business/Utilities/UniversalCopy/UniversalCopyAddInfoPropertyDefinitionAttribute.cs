using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
	public sealed class UniversalCopyAddInfoPropertyDefinitionAttribute : Attribute
	{
		public UniversalCopyAddInfoPropertyDefinitionAttribute(Type addInfoSchema, string countryCode)
		{
			AddInfoSchema = Argument.NotNull(addInfoSchema, nameof(addInfoSchema));
			CountryCode = countryCode;
		}

		public UniversalCopyAddInfoPropertyDefinitionAttribute(Type addInfoSchema)
			: this(addInfoSchema, null)
		{
		}

		public string CountryCode { get; }

		public Type AddInfoSchema { get; }

		public IEnumerable<string> GetPropertyNames()
		{
			return AddInfoSchema.GetFields(BindingFlags.Public | BindingFlags.Static).Select(x => x.Name);
		}
	}
}
