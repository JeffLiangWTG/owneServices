using System;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core;

public class SetWriterStrategyDataObjectFactory : IDataObjectFactory
{
	public SetWriterStrategyDataObjectFactory(IDataObjectWriterStrategy writerStrategy)
	{
		this.writerStrategy = writerStrategy;
	}

	public IDataObject Create(Type type)
	{
		try
		{
			var instance = (IDataObject)Activator.CreateInstance(type);

			SetWriterStrategy(instance);

			return instance;
		}
		catch (Exception e)
		{
			throw new Exception($"Failed to create {type.FullName}. {e.Message}", e);
		}
	}

	void SetWriterStrategy(object instance)
	{
		if (instance is ISettableWriterStrategy settableWriterStrategy)
		{
			settableWriterStrategy.SetWriterStrategy(writerStrategy);
		}
	}

	readonly IDataObjectWriterStrategy writerStrategy;
}
