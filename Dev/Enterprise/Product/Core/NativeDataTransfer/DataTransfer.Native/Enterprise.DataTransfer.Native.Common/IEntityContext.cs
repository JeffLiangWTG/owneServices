using System;
using System.Collections.Generic;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common.Interceptors;
using Enterprise.DataTransfer.Native.Common.Operations;
using Enterprise.DataTransfer.Native.Common.Stat;

namespace Enterprise.DataTransfer.Native.Common
{
	public interface IEntityContext : IUpdateOperation, IDataFactoryProvider, IDisposable
	{
		DbConnection Connection { get; }
		IList<IInterceptorSetting> InterceptorSettings { get; }
		StatisticsImpl Statistics { get; }
		bool AlwaysUseInternalPK { get; set; }
	}

	public interface IDataFactoryProvider
	{
		BusinessObjectFactory ObjectFactory { get; }
		RowFactory RowFactory { get; }

		/// <summary>
		/// Only for use in an IEntityBehaviour with changes that must be saved before the main RowFactory.
		/// </summary>
		RowFactory GetOrCreateBehaviourRowFactorySavedFirst();
	}
}