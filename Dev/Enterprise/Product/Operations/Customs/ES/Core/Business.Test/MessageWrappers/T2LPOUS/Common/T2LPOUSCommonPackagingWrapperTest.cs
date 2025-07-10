using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class T2LPOUSCommonPackagingWrapperTest : WrapperHelperTest<T2LPOUSCommonPackagingWrapper>
	{
		public void TestTypeOfPackages()
		{
			AssertEquals("Expected filled TypeOfPackages", "CT", wrapper.TypeOfPackages);
		}

		public void TestNumberOfPackages()
		{
			AssertEquals("Expected filled NumberOfPackages", 20, wrapper.NumberOfPackages);
		}

		public void TestNumberOfPackagesValueSpecified()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected true NumberOfPackagesValueSpecified when not 0 and type is not bulk", true, wrapper.NumberOfPackagesValueSpecified);

				wrapper = new T2LPOUSCommonPackagingWrapper("VG", 0, Factory);
				AssertEquals("Expected false NumberOfPackagesValueSpecified when 0 and type is bulk", false, wrapper.NumberOfPackagesValueSpecified);

				wrapper = new T2LPOUSCommonPackagingWrapper("CT", 0, Factory);
				AssertEquals("Expected true NumberOfPackagesValueSpecified when 0 and type is not bulk", true, wrapper.NumberOfPackagesValueSpecified);

				wrapper = new T2LPOUSCommonPackagingWrapper("VG", 20, Factory);
				AssertEquals("Expected true NumberOfPackagesValueSpecified when not 0 and type is bulk", false, wrapper.NumberOfPackagesValueSpecified);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			Factory.SetBulkTypeHelper();

			wrapper = new T2LPOUSCommonPackagingWrapper("CT", 20, Factory);
		}
		T2LPOUSCommonPackagingWrapper wrapper;

		protected override T2LPOUSCommonPackagingWrapper GetProvider() => wrapper;
	}
}
