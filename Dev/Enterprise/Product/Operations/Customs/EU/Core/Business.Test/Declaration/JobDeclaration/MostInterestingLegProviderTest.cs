using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using MostInterestingLegProvider = Enterprise.Customs.EU.Business.Declaration.MostInterestingLegProvider;

namespace Enterprise.Customs.EU.Business.Testing
{
	sealed class MostInterestingLegProviderTest : TestCaseWithFactory
	{
		public void TestOutboundLegCorrectlyCaptured_WhenMovementFromMainlandToItsTerritory()
		{
			var outboundLegCase_Export = new OutboundLegTestCase(Factory, "FRAAA -> MQAAA", Core.Constants.CountryCodes.France, MessageTypeList.Codes.Export, "FRAAA", "MQAAA", "GFAAA");
			outboundLegCase_Export.AssertTrue("Outbound Leg should be from FR to MQ when making EXP declaration.");

			var outboundLegCase_Import = new OutboundLegTestCase(Factory, "MQAAA -> GFAAA", Core.Constants.CountryCodes.FrenchGuyana, MessageTypeList.Codes.Import, "FRAAA", "MQAAA", "GFAAA");
			outboundLegCase_Import.AssertTrue("Outbound Leg should be from MQ to GF when making IMP declaration.");
		}

		public void TestInboundLegCorrectlyCaptured_WhenMovementFromMainlandToIsTerritory()
		{
			var inboundLegCase_Export = new InboundLegTestCase(Factory, "FRAAA -> MQAAA", Core.Constants.CountryCodes.France, MessageTypeList.Codes.Export, "FRAAA", "MQAAA", "GFAAA");
			inboundLegCase_Export.AssertTrue("Inbound Leg should be from FR to MQ when making EXP declaration.");

			var inboundLegCase_Import = new InboundLegTestCase(Factory, "MQAAA -> GFAAA", Core.Constants.CountryCodes.FrenchGuyana, MessageTypeList.Codes.Import, "FRAAA", "MQAAA", "GFAAA");
			inboundLegCase_Import.AssertTrue("Inbound Leg should be from MQ to GF when making IMP declaration.");
		}

		class InboundLegTestCase : MostInterestingLegProviderTestCase
		{
			public InboundLegTestCase(BusinessObjectFactory factory, ZString expectedLeg, ZString countryOfBrokerage, ZString direction, params string[] ports) : base(factory, expectedLeg, countryOfBrokerage, direction, ports)
			{
			}

			protected override IMovementLeg GetInterestingLeg(MostInterestingLegProvider provider, IEnumerable<IMovementLeg> legs)
			{
				return provider.GetInboundLeg(legs);
			}
		}

		class OutboundLegTestCase : MostInterestingLegProviderTestCase
		{
			public OutboundLegTestCase(BusinessObjectFactory factory, ZString expectedLeg, ZString countryOfBrokerage, ZString direction, params string[] ports) : base(factory, expectedLeg, countryOfBrokerage, direction, ports)
			{
			}

			protected override IMovementLeg GetInterestingLeg(MostInterestingLegProvider provider, IEnumerable<IMovementLeg> legs)
			{
				return provider.GetOutboundLeg(legs);
			}
		}

		abstract class MostInterestingLegProviderTestCase
		{
			internal MostInterestingLegProviderTestCase(BusinessObjectFactory factory, string expectedLeg, ZString countryOfBrokerage, ZString direction, params string[] routingPorts) : base()
			{
				this.movementLegs = GetMovementLegsByRouting(routingPorts);
				this.countryOfBrokerage = countryOfBrokerage;
				this.direction = direction;
				this.expectedLeg = expectedLeg;
				this.factory = factory;
			}

			readonly IEnumerable<IMovementLeg> movementLegs;
			readonly ZString countryOfBrokerage;
			readonly ZString direction;
			readonly ZString expectedLeg;
			readonly BusinessObjectFactory factory;

			protected abstract IMovementLeg GetInterestingLeg(MostInterestingLegProvider provider, IEnumerable<IMovementLeg> legs);

			static IEnumerable<IMovementLeg> GetMovementLegsByRouting(params string[] ports)
			{
				AssertGreaterThan("More than 2 parameters should be provided for GetMoveMenLegsByRouting() to create a complete movement legs.", ports.Length, 1);

				var completeRoutingMovementLegs = new List<IMovementLeg>();
				for (var i = 0; i < ports.Length - 1; i++)
				{
					var movementLeg = new Mock<IMovementLeg>();
					movementLeg.Setup(l => l.Load).Returns(ports[i]);
					movementLeg.Setup(l => l.Discharge).Returns(ports[i + 1]);
					completeRoutingMovementLegs.Add(movementLeg.Object);
				}
				return completeRoutingMovementLegs;
			}

			internal void AssertTrue(string message)
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryOfBrokerage))
				{
					var dec = factory.New<JobDeclaration>();
					dec.JE_MessageType = direction;

					var provider = new MostInterestingLegProvider(dec);
					var interestingLeg = GetInterestingLeg(provider, movementLegs);
					AssertEquals(message, expectedLeg, string.Join(" -> ", interestingLeg.Load, interestingLeg.Discharge));
				}
			}
		}
	}
}
