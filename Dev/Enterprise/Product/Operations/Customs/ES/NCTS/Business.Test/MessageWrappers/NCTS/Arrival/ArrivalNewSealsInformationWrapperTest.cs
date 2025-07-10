using System;
using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	public class ArrivalNewSealsInformationWrapperTest : Customs.Business.Testing.DataProviderTestCase<ArrivalNewSealsInformationWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("Throw exception if container is null", () => new ArrivalNewSealsInformationWrapper(null));
		}

		public void TestSealId()
		{
			container.BC_Seal1 = "AH";
			AssertEquals("Expected filled SealId", "AH", wrapper.SealId);
		}

		public void TestSealIdLanguage()
		{
			AssertEquals("Expected empty SealIdLanguage", ZString.Empty, wrapper.SealIdLanguage);
		}

		protected override void SetUp()
		{
			base.SetUp();

			container = Factory.New<SealContainer>();
			wrapper = new ArrivalNewSealsInformationWrapper(container);
		}
		SealContainer container;
		ArrivalNewSealsInformationWrapper wrapper;

		protected override ArrivalNewSealsInformationWrapper GetProvider() => wrapper;
	}
}
