using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.Testing.Business.Reflection
{
	internal class ReadonlyBO : BusinessObject
	{
		public ReadonlyBO(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ReadonlyBO(DataRow row) : base(row)
		{
		}

		public override SchemaGuidColumn PKSchemaColumn { get; }

		public ZString ZStringPropertyInReadonlyBO1 { get; set; }
		public ZString ZStringPropertyInReadonlyBO2 { get; set; }
	}
}
