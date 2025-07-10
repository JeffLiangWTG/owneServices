using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[CodeProperty("Code"), DescriptionProperty("Description")]
	sealed class DummyBizo : EnterpriseBusinessObject
	{
		public DummyBizo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
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

		public string Code { get; set; }
		public string Description { get; set; }
	}
}
