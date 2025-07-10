using System;

namespace Enterprise.ZArchitecture.Business.UniversalCopy
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class UniversalCopyRelatedEntityAttribute : Attribute
	{
		public string CreationMethodName { get; set; }
		public string RelatedPropertyName { get; set; }
		public string RelatedEntityTableName { get; set; }

		public string CommaSeparatedSkipPropertiesNames { get; set; }

		public bool DisableCopyMethodCopy { get; set; }
		public bool DisableCopyMethodLink { get; set; }
		public bool AllowCopyMethodLinkCopiedWhenLinkIsDisabled { get; set; }

		public string MakeRelatedEntitySavedByFactoryMethod { get; set; }
	}
}
