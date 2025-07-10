using System.Data;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	public class DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter : DummyBusinessObject, IAddInfoChildUniqueIndexFailureHandlerSupporter
	{
		public DummyBizObjWithAddInfoChildUniqueIndexFailureHandlerSupporter(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override ZString HumanReadableNameCore => string.Join("_", "DUMMY", Z0_Code, Z0_Description);

		public DummyBizObjWithAddInfoChildSupporter Parent => Factory.Load<DummyBizObjWithAddInfoChildSupporter>(Z0_Guid);

		public string UniqueIndexName => "DUMMY_INDEX";

		IAddInfoChildSupporter IAddInfoChildUniqueIndexFailureHandlerSupporter.Parent => Parent;

		public ZString SystemLastEditUser => Z0_Code;

		public ZDateTime SystemLastEditTimeUtc => Z0_Date;
	}
}
