using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.ZArchitecture.GUI.Support.Testing
{
	sealed class TestCancellableHelper : TestCaseWithFactory
	{
		public void TestGetICancellable()
		{
			var dummyBusiness = Factory.New<DummyBusinessObject>();
			var dummyCancellable = Factory.New<DummyCancellable>();

			AssertNull(CancellableHelper.GetICancellable(dummyBusiness));
			AssertEquals(dummyCancellable, CancellableHelper.GetICancellable(dummyCancellable));
		}

		public void TestGetICancellable_TemplateRecord()
		{
			var dummyBusiness = Factory.New<DummyBusinessObject>();
			var dummyTemplateRecordProvider = Factory.New<DummyTemplateRecordProvider>();
			var dummyTemplateRecord = Factory.New<DummyTemplateRecord>();

			dummyTemplateRecordProvider.TemplateRecord = dummyTemplateRecord;

			AssertNull(CancellableHelper.GetICancellable(dummyBusiness));
			AssertEquals(
				"Should return template record, not the provider",
				dummyTemplateRecord,
				CancellableHelper.GetICancellable(dummyTemplateRecordProvider)
			);

			dummyTemplateRecordProvider.TemplateRecord = null;

			AssertEquals(
				"Should return provider because no template record",
				dummyTemplateRecordProvider,
				CancellableHelper.GetICancellable(dummyTemplateRecordProvider)
			);
		}
	}
}
