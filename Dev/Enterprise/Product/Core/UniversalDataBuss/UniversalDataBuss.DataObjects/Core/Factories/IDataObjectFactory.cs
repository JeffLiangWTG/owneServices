using System;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.UniversalDataBuss.DataObjects.Core;

public interface IDataObjectFactory
{
	IDataObject Create(Type type);
}
