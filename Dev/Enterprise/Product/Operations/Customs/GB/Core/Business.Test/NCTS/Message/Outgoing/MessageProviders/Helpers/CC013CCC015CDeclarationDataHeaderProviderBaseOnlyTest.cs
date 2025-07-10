using System;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(CC013CCC015CDeclarationDataHeaderProvider))]
	sealed class CC013CCC015CDeclarationDataHeaderProviderBaseOnlyTest : NctsHeaderProviderAbstractTest<CC013CCC015CDeclarationDataHeaderProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CC013CCC015CDeclarationDataHeaderProvider(null));
		}

		public void TestCustomsOfficesOfTransit()
		{
			AssertNotNull(Provider.CustomsOfficesOfTransit);
		}

		public void TestCustomsOfficesOfExitForTransit()
		{
			AssertNotNull(Provider.CustomsOfficesOfExitForTransit);
		}

		public void TestHolderOfTheTransitProcedure()
		{
			AssertNull(Provider.HolderOfTheTransitProcedure);
			CreatePrincipal();
			AssertNotNull(Provider.HolderOfTheTransitProcedure);
		}

		public void TestTransitOperation()
		{
			AssertNotNull(Provider.TransitOperation);
		}

		public void TestAuthorisations()
		{
			AssertNotNull(Provider.Authorisations);
		}

		public void TestRepresentative()
		{
			AssertNull(Provider.Representative);
		}

		public void TestGuarantees()
		{
			AssertNotNull(Provider.Guarantees);
		}

		public void TestConsignment()
		{
			AssertNotNull(Provider.Consignment);
		}

		protected override string MessageType => ZString.Empty;

		protected override string MovementType => NctsMovementType.Codes.Departure;
	}
}
