using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.GB.MessageContracts.EMCS;
using Enterprise.Customs.EU.EMCS.Business;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class TransportProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new TransportProvider(null));
		}

		public void TestUnitCode()
		{
			container.ZG_UnitCode = EMCSTransportUnitCodeList.Codes.Tractor;
			AssertEquals(EMCSTransportUnitCodeList.Codes.Tractor, dataProvider.UnitCode);
		}

		public void TestIdentityOfUnit()
		{
			container.CO_ContainerNumber = "PONU2864065";
			AssertEquals("PONU2864065", dataProvider.IdentityOfUnit);
		}

		public void TestCommercialSealIdentification()
		{
			container.CO_Seal = "4419151";
			AssertEquals("4419151", dataProvider.CommercialSealIdentification);
		}

		public void TestComplementaryInformation()
		{
			AssertEquals("Comment", "COMMENT ABOUT THE CONTAINER", dataProvider.ComplementaryInformation.Text);
		}

		public void TestSealInformation()
		{
			AssertEquals("SEAL INFORMATION", dataProvider.SealInformation.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			container = Factory.New<EMCSCusContainer>();
			container.Comment = "COMMENT ABOUT THE CONTAINER";
			container.SealDetails = "SEAL INFORMATION";
			dataProvider = new TransportProvider(container);
		}
		EMCSCusContainer container;
		IEMCSTransport dataProvider;

		protected override TransportProvider GetProvider() => (TransportProvider)dataProvider;

		protected override IEnumerable<Expression<Func<TransportProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.ComplementaryInformation;
			yield return x => x.SealInformation;
		}
	}
}
