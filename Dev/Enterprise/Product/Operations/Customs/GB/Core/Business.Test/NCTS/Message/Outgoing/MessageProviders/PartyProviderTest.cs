using System;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.GB.Registry;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	[TestedType(typeof(PartyProvider))]
	class PartyProviderTest : PartyProviderAbstractTest<PartyProvider>
	{
		public void TestIdentificationNumber()
		{
			AssertEquals("XI175521246821", Provider.IdentificationNumber);
		}

		public void TestName()
		{
			CombineAssertions(() =>
			{
				AssertNullOrEmpty("has identification number, should be empty", Provider.Name);
				address.Organisation.CustomsCodes.RemoveAll();
				AssertEquals("has no identification number, should be filled in", "Name", Provider.Name);
			});
		}

		public void TestAddressWithIdentificationNumber()
		{
			AssertNull(Provider.Address);
		}

		public void TestIncludeEORI()
		{
			GBCustomsDataRegistry.Instance.SuppressForeignEORI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			AssertEquals("IncludeEORI should be false", false, provider.IncludeEORI);

			GBCustomsDataRegistry.Instance.SuppressForeignEORI.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertEquals("IncludeEORI should be false", false, provider.IncludeEORI);
		}

		public void TestPartyNameMaxLength_ForPhase5TransitionPeriod()
		{
			AssertEquals("In Phase 5 Transition Period", MessageSchemaInTransitionPeriod.PartyNameMaxLength, new PartyProvider(address, true).PartyNameMaxLength);
			AssertEquals("Outside Phase 5 Transition Period", MessageSchema.PartyNameMaxLength, Provider.PartyNameMaxLength);
		}
	}
}
