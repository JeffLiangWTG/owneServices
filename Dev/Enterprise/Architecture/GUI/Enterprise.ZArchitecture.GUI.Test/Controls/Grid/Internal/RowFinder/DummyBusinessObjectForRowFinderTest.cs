using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Grid.Testing
{
	sealed class DummyBusinessObjectForRowFinderTest : BaseDummyBusinessObjectForRowFinderTest
	{
		public DummyBusinessObjectForRowFinderTest(BusinessObjectFactory factory, System.Data.DataRow row)
			: base(factory, row)
		{
			Dummy1 = factory.NewWithValidTestData<DummyChildBusinessObjectWithMultilingual>();
			Collection.Add(Dummy1);
			Dummy1.Z0_VarCharMax = "hello";
			Dummy1.Z0_Description = "Desc";
			Dummy1.RelatedBusinessObject.Z0_Code = "DUM1";
			Dummy1.ZGuidValue = Dummy1.RelatedBusinessObject.PK;

			Dummy2 = factory.NewWithValidTestData<DummyChildBusinessObjectWithMultilingual>();
			Collection.Add(Dummy2);
			Dummy2.Z0_VarCharMax = "nothing";
			Dummy2.Z0_Description = "HELLo";
			Dummy2.RelatedBusinessObject.Z0_Code = "DUM2";
			Dummy2.ZGuidValue = Dummy2.RelatedBusinessObject.PK;
		}

		public readonly DummyChildBusinessObjectWithMultilingual Dummy1;
		public readonly DummyChildBusinessObjectWithMultilingual Dummy2;
	}
}
