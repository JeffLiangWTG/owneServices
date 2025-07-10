using System;
using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BusinessObjectFactoryWithSaveException : BusinessObjectFactory
	{
		public BusinessObjectFactoryWithSaveException(Exception exceptionToThrow)
		{
			this.ExceptionToThrow = exceptionToThrow;
		}

		public readonly Exception ExceptionToThrow;

		protected override IChangedTableNames SaveInTransactionCore()
		{
			throw ExceptionToThrow;
		}
	}
}
