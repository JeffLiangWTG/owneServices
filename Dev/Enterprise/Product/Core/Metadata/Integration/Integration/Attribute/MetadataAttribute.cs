using System;

namespace Enterprise.Metadata.Integration
{
	[AttributeUsage(AttributeTargets.Class)]
	public class MetadataAttribute : Attribute
	{
		public MetadataAttribute(MetadataContext context)
		{
			Context = context;
		}

		public readonly MetadataContext Context;
	}
}
