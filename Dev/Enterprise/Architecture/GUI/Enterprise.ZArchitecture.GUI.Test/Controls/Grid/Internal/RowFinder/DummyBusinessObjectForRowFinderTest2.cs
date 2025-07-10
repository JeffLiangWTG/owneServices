using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI.Grid.Testing
{
	sealed class DummyBusinessObjectForRowFinderTest2 : BaseDummyBusinessObjectForRowFinderTest
	{
		public DummyBusinessObjectForRowFinderTest2(BusinessObjectFactory factory, System.Data.DataRow row)
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
			Dummy2.Z0_VarCharMax = "a";
			Dummy2.Z0_Description = "d";
			Dummy2.RelatedBusinessObject.Z0_Code = "DUM2";
			Dummy2.ZGuidValue = Dummy2.RelatedBusinessObject.PK;

			Dummy3 = factory.NewWithValidTestData<DummyChildBusinessObjectWithMultilingual>();
			Collection.Add(Dummy3);
			Dummy3.Z0_VarCharMax = "b";
			Dummy3.Z0_Description = "Desc";
			Dummy3.RelatedBusinessObject.Z0_Code = "DUM3";
			Dummy3.ZGuidValue = Dummy3.RelatedBusinessObject.PK;

			Dummy4 = factory.NewWithValidTestData<DummyChildBusinessObjectWithMultilingual>();
			Collection.Add(Dummy4);
			Dummy4.Z0_VarCharMax = "hello";
			Dummy4.Z0_Description = "d";
			Dummy4.RelatedBusinessObject.Z0_Code = "DUM4";
			Dummy4.ZGuidValue = Dummy4.RelatedBusinessObject.PK;

			Dummy5 = factory.NewWithValidTestData<DummyChildBusinessObjectWithMultilingual>();
			Collection.Add(Dummy5);
			Dummy5.Z0_VarCharMax = "c";
			Dummy5.Z0_Description = "desc";
			Dummy5.RelatedBusinessObject.Z0_Code = "DUM5";
			Dummy5.ZGuidValue = Dummy5.RelatedBusinessObject.PK;

			Dummy6 = factory.NewWithValidTestData<DummyChildBusinessObjectWithMultilingual>();
			Collection.Add(Dummy6);
			Dummy6.Z0_VarCharMax = "d";
			Dummy6.Z0_Description = "d";
			Dummy6.RelatedBusinessObject.Z0_Code = "DUM6";
			Dummy6.ZGuidValue = Dummy6.RelatedBusinessObject.PK;

			Dummy7 = factory.NewWithValidTestData<DummyChildBusinessObjectWithMultilingual>();
			Collection.Add(Dummy7);
			Dummy7.Z0_VarCharMax = "Hello";
			Dummy7.Z0_Description = "desc";
			Dummy7.RelatedBusinessObject.Z0_Code = "DUM7";
			Dummy7.ZGuidValue = Dummy7.RelatedBusinessObject.PK;
		}

		public readonly DummyChildBusinessObjectWithMultilingual Dummy1;
		public readonly DummyChildBusinessObjectWithMultilingual Dummy2;
		public readonly DummyChildBusinessObjectWithMultilingual Dummy3;
		public readonly DummyChildBusinessObjectWithMultilingual Dummy4;
		public readonly DummyChildBusinessObjectWithMultilingual Dummy5;
		public readonly DummyChildBusinessObjectWithMultilingual Dummy6;
		public readonly DummyChildBusinessObjectWithMultilingual Dummy7;
	}
}
