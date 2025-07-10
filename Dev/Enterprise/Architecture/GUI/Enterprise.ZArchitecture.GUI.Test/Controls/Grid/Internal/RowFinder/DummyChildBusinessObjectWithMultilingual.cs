using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.GUI.Grid.Testing
{
	sealed class DummyChildBusinessObjectWithMultilingual : DummyChildBusinessObject
	{
		public DummyChildBusinessObjectWithMultilingual(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public MultilingualString MultilingualTest
		{
			get { return (NoResString)"J"; }
		}

		[RelatedBusinessObject("RelatedBusinessObject")]
		public ZGuid ZGuidValue { get; set; }
		public ZPropertyInfo ZGuidValueInfo => GetZPropertyInfo(nameof(ZGuidValue));

		public DummyBusinessObject RelatedBusinessObject => relatedBusinessObject ?? (relatedBusinessObject = Factory.NewWithValidTestData<DummyBusinessObject>());
		DummyBusinessObject relatedBusinessObject;
	}
}
