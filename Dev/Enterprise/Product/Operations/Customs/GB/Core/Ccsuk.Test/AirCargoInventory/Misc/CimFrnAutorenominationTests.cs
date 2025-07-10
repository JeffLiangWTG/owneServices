using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Testing
{
	class CimFrnAutorenominationTests : TestCaseWithFactory
	{
		public void TestTrustDefault()
		{
			AssertEquals("Everyone is trusted", true, CIMFRN.IsAutoRenominationAllowed("ABC", "000ABC", GlbBranch.CurrentBranch));
			AssertEquals("Requestor must be the current agent", false, CIMFRN.IsAutoRenominationAllowed("ABC", "000XXX", GlbBranch.CurrentBranch));
			AssertEquals("Current agent must be set", false, CIMFRN.IsAutoRenominationAllowed("", "000ABC", GlbBranch.CurrentBranch));
		}

		public void TestTrustOnlyAllowSomeMembers()
		{
			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateTrustedAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "ABC");
			AssertEquals("Only ABC is trusted", true, CIMFRN.IsAutoRenominationAllowed("ABC", "000ABC", GlbBranch.CurrentBranch));
			AssertEquals("DEF is not trusted", false, CIMFRN.IsAutoRenominationAllowed("DEF", "000DEF", GlbBranch.CurrentBranch));

			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateTrustedAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "ABC,DEF");
			AssertEquals("ABC is trusted", true, CIMFRN.IsAutoRenominationAllowed("ABC", "000ABC", GlbBranch.CurrentBranch));
			AssertEquals("DEF is trusted", true, CIMFRN.IsAutoRenominationAllowed("DEF", "000DEF", GlbBranch.CurrentBranch));
			AssertEquals("But XYZ is not", false, CIMFRN.IsAutoRenominationAllowed("XYZ", "000XYZ", GlbBranch.CurrentBranch));
		}

		public void TestTrustForbid()
		{
			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateForbiddenAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "ABC");
			AssertEquals("ABC is explicitly not trusted", false, CIMFRN.IsAutoRenominationAllowed("ABC", "000ABC", GlbBranch.CurrentBranch));
			AssertEquals("DEF is not forbidden, so is trusted", true, CIMFRN.IsAutoRenominationAllowed("DEF", "000DEF", GlbBranch.CurrentBranch));

			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateForbiddenAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "ABC,DEF");
			AssertEquals("ABC is explicitly not trusted", false, CIMFRN.IsAutoRenominationAllowed("ABC", "000ABC", GlbBranch.CurrentBranch));
			AssertEquals("DEF is explicitly not trusted", false, CIMFRN.IsAutoRenominationAllowed("DEF", "000DEF", GlbBranch.CurrentBranch));
			AssertEquals("But XYZ is not explicitly forbidden", true, CIMFRN.IsAutoRenominationAllowed("XYZ", "000XYZ", GlbBranch.CurrentBranch));

			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateForbiddenAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "*");
			AssertEquals("Forbid all", false, CIMFRN.IsAutoRenominationAllowed("ABC", "000ABC", GlbBranch.CurrentBranch));
			AssertEquals("Forbid all", false, CIMFRN.IsAutoRenominationAllowed("DEF", "000DEF", GlbBranch.CurrentBranch));
			AssertEquals("Forbid all", false, CIMFRN.IsAutoRenominationAllowed("XYZ", "000XYZ", GlbBranch.CurrentBranch));
		}

		public void TestTrustMixedSettings()
		{
			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateTrustedAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "ABC");
			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateForbiddenAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "ABC");
			AssertEquals("ABC is explicitly trusted and explicitly forbidden, and forbid wins", false, CIMFRN.IsAutoRenominationAllowed("ABC", "000ABC", GlbBranch.CurrentBranch));
			AssertEquals("DEF not explicitly trusted and not explicitly forbidden, default is untrusted", false, CIMFRN.IsAutoRenominationAllowed("DEF", "000DEF", GlbBranch.CurrentBranch));

			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateTrustedAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "ABC");
			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateForbiddenAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "*");
			AssertEquals("ABC is explicitly trusted and implicitly forbidden, and forbid wins", false, CIMFRN.IsAutoRenominationAllowed("ABC", "000ABC", GlbBranch.CurrentBranch));

			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateTrustedAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "*");
			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateForbiddenAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "*");
			AssertEquals("ABC is implicitly trusted and implicitly forbidden, and forbid wins", false, CIMFRN.IsAutoRenominationAllowed("ABC", "000ABC", GlbBranch.CurrentBranch));

			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateTrustedAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "ABC,DEF");
			GBCustomsDataRegistry.Instance.CcsukShedAutoRenominateForbiddenAgents.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, "UVW,XYZ");
			AssertEquals("MNO is not mentioned, default is untrusted", false, CIMFRN.IsAutoRenominationAllowed("MNO", "000MNO", GlbBranch.CurrentBranch));
			AssertEquals("ABC is explictly trusted", true, CIMFRN.IsAutoRenominationAllowed("ABC", "000ABC", GlbBranch.CurrentBranch));
			AssertEquals("XYZ is explictly forbidden", false, CIMFRN.IsAutoRenominationAllowed("XYZ", "000XYZ", GlbBranch.CurrentBranch));
		}
	}
}
