using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineCore;

namespace Enterprise.DocumentEngine.ReflectiveFieldMap.Testing
{
	public class DummyBOForTest : BusinessObject
	{
		public DummyBOForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public DummyBOForTest(DataRow row) : base(row)
		{
		}

		public override SchemaGuidColumn PKSchemaColumn { get; }

		public ZString CustomZStringInBO1 { get; set; }
		public ZString CustomZStringInBO2 { get; set; }
		public IEnumerable<ZString> CustomZStringEnumerableInBO { get; set; }

		public IDummyIntegrationInterface RelationInterfaceAsInterface { get; }

		[ResolveTypeFromObjectFactoryForDocData]
		public IDummyIntegrationInterface RelationInterfaceAsExpectedImplementation { get; }
	}
}
