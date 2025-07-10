using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	sealed class DummyBizoWithoutCodeProperty : EnterpriseBusinessObject
	{
		public DummyBizoWithoutCodeProperty(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Schema

		public abstract class Schema
		{
			public const string TableName = "TestSchema";
			public const string PK = "Z0_PK";
		}

		#endregion

		public override SchemaGuidColumn PKSchemaColumn => throw new NotImplementedException();
	}
}
