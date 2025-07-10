using System.Data;
using Enterprise.ZArchitecture.Core;

namespace CargoWise.EntityFramework.Testing
{
	[CodeProperty("Code"), DescriptionProperty("Description")]
	sealed class DummyBusinessObjectWithMultilingualString : DummyBusinessObject
	{
		public DummyBusinessObjectWithMultilingualString(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public MultilingualString Code { get; set; }
		public MultilingualString Description { get; set; }
	}
}
