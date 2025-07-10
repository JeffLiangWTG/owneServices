using System;

namespace Enterprise.DocumentEngineIntegration
{
	public interface IBusinessObjectReflector
	{
		bool IsPropertyAccessible(Type typeToReflect, string propertyIdentifier);
	}
}
