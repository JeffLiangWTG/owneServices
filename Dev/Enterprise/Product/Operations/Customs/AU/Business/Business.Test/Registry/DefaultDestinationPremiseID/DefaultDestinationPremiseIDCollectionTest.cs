using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(DefaultDestinationPremiseIDCollection))]
	sealed class DefaultDestinationPremiseIDCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<DefaultDestinationPremiseIDCollection>
	{
		public void TestGetPremiseIDMatchingDischargeAndDestinationPort()
		{
			var coll = new DefaultDestinationPremiseIDCollection(Factory);
			var premise1 = coll.AddNew();
			premise1.AirlineCode = "QF";
			premise1.PortOfDischarge = "AUSYD";
			premise1.UseDischargePort = true;
			premise1.PremiseID = "1234A";
			var premise2 = coll.AddNew();
			premise2.AirlineCode = "QF";
			premise2.PortOfDischarge = "AUMEL";
			premise2.PremiseID = "4321B";
			AssertEquals(ZString.Empty, coll.GetPremiseIDMatchingDischargePort("QF101", "AUMEL"));
			AssertEquals("1234A", coll.GetPremiseIDMatchingDischargePort("QF101", "AUSYD"));
			AssertEquals(ZString.Empty, coll.GetPremiseIDMatchingDestinationPort("QF101", "AUSYD"));
			AssertEquals("4321B", coll.GetPremiseIDMatchingDestinationPort("QF101", "AUMEL"));
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override DefaultDestinationPremiseIDCollection GetCollectionToTest()
		{
			return new DefaultDestinationPremiseIDCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DefaultDestinationPremiseID();
		}
	}
}
