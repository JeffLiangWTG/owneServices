using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public sealed class UniversalCopyAddInfoAttribute : Attribute
	{
		public UniversalCopyAddInfoAttribute()
		{
			PropertyPrefix = string.Empty;
			AddInfoPrefix = string.Empty;
		}

		public UniversalCopyAddInfoAttribute(string propertyPrefix, string addInfoPrefix)
		{
			PropertyPrefix = Argument.NotNullOrEmpty(propertyPrefix, nameof(propertyPrefix));
			AddInfoPrefix = Argument.NotNullOrEmpty(addInfoPrefix, nameof(addInfoPrefix));
		}

		public bool HasMapping => !string.IsNullOrWhiteSpace(PropertyPrefix) && !string.IsNullOrWhiteSpace(AddInfoPrefix);

		public string PropertyPrefix { get; }

		public string AddInfoPrefix { get; }
	}
}
