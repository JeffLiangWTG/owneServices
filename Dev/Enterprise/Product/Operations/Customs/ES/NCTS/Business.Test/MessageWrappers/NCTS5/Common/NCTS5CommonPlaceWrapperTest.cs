using CargoWise.Types;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class NCTS5CommonPlaceWrapperTest : WrapperHelperTest<NCTS5CommonPlaceWrapper>
	{
		public void TestUNLocode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled UNLocode when code is UNLOCO", "ESMAD", wrapper.UNLocode);

				wrapper = GetWrapper("ESLOCATION");
				AssertEquals("Expected empty UNLocode when code is not UNLOCO", ZString.Empty, wrapper.UNLocode);

				wrapper = GetWrapper("ABCDE");
				AssertEquals("Expected empty UNLocode when code is not UNLOCO even if it has 5 characters", ZString.Empty, wrapper.UNLocode);
			});
		}

		public void TestCountry()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Country when code is UNLOCO", ZString.Empty, wrapper.Country);

				wrapper = GetWrapper("FRLOCATION");
				AssertEquals("Expected filled Country when code is not UNLOCO", "FR", wrapper.Country);

				wrapper = GetWrapper("ABCDE");
				AssertEquals("Expected filled Country when code is not UNLOCO even if it has 5 characters", "AB", wrapper.Country);
			});
		}

		public void TestLocation()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty Location when code is UNLOCO", ZString.Empty, wrapper.Location);

				wrapper = GetWrapper("FRLOCATION");
				AssertEquals("Expected filled Location when code is not UNLOCO and it's longer than 2", "LOCATION", wrapper.Location);

				wrapper = GetWrapper("FR");
				AssertEquals("Expected empty Location when code is not UNLOCO but it's 2 or less characters", ZString.Empty, wrapper.Location);

				wrapper = GetWrapper("ABCDE");
				AssertEquals("Expected filled Location when code is not UNLOCO even if it has 5 characters", "CDE", wrapper.Location);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var loader = new RefUNLOCO.Loader(Factory);
			loader.Load("ESMAD");

			wrapper = GetWrapper("ESMAD");
		}
		NCTS5CommonPlaceWrapper wrapper;

		NCTS5CommonPlaceWrapper GetWrapper(ZString placeCode) => new NCTS5CommonPlaceWrapper(placeCode, Factory);

		protected override NCTS5CommonPlaceWrapper GetProvider() => wrapper;
	}
}
