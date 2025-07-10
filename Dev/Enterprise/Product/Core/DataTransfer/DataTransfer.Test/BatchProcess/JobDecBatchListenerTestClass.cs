using System;
using CargoWise.Application;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.BatchProcessor.Testing
{
	sealed class JobDecBatchListenerTestClass : LogBatchListenerTestClass
	{
		public override string BusinessObjectTableName
		{
			get { return JobDeclarationSchema.Constants.TableName; }
		}

		public override Type BusinessObjectType
		{
			get { return ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>(); }
		}

		public override string HumanReadableName
		{
			get { return ""; }
		}
	}
}
