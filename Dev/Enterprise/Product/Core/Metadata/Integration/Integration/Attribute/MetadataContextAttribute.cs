using System;

namespace Enterprise.Metadata.Integration
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MetadataContextAttribute : Attribute
	{
		public MetadataContextAttribute(MetadataContext context, string fullQualifiedMetadataName = "")
		{
			Context = context;
			FullQualifiedMetadataName = fullQualifiedMetadataName;
		}

		public readonly MetadataContext Context;
		public readonly string FullQualifiedMetadataName;
	}
}
