using System;
using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Integration
{
	public interface IUniversalObjectFactory
	{
	}

	public interface IUniversalBusinessObjectFactory
	{
		RowFactory RowFactory { get; }

		IDisposable EnableSave();
	}
}
