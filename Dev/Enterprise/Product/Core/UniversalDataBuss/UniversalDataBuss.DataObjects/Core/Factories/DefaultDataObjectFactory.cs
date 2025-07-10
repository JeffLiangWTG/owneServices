using System;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core;

public class DefaultDataObjectFactory : IDataObjectFactory
{
	public IDataObject Create(Type type)
	{
		try
		{
			return (IDataObject)Activator.CreateInstance(type);
		}
		catch (Exception e)
		{
			throw new Exception($"Failed to create {type.FullName}. {e.Message}", e);
		}
	}
}
