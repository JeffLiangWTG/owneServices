using System;

namespace Enterprise.Metadata.Integration
{
	public interface IMetadataProvider
	{
		IMetadata GetMetadataFromBO(object businessObject);
		IMetadata GetMetadataFromType(Type type);
	}
}
