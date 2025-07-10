using System.Collections.Generic;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Freight;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CalculateConsolChargeable))]
	sealed class CalculateConsolChargeableTest : ValueProviderTest
	{
		protected override ValueProvider GetNewValueProvider()
		{
			return new CalculateConsolChargeable();
		}

		public void TestIsResponsibleForReplacing()
		{
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<CalculateConsolChargeable(  text   )>", Passes.FirstPass));
			AssertEquals(true, ValueProviderToTest.IsResponsibleForReplacing("<CalculateConsolChargeable(C101003)>", Passes.FirstPass));
			AssertEquals(false, ValueProviderToTest.IsResponsibleForReplacing("<CalculateConsolChargeable (C101003) >", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			var mocks = new MockRepository(MockBehavior.Default);
			var chargeableHelper = mocks.Create<IConsolChargeableCalculationHelper>(MockBehavior.Strict);
			chargeableHelper.Setup(m => m.CalculateChargeables(
				It.Is<ISet<ZString>>(uniqueRefs => uniqueRefs.SetEquals(new HashSet<ZString>() { "Consol1" })),
				Env.Registry.FreightWeightUnit, Env.Registry.FreightVolumeUnit))
				.Returns(new Dictionary<ZString, ZDecimal>() { { "Consol1", 10m } });

			chargeableHelper.Setup(m => m.CalculateChargeables(
				It.Is<ISet<ZString>>(uniqueRefs => uniqueRefs.SetEquals(new HashSet<ZString>() { "Consol2" })),
				Env.Registry.FreightWeightUnit, Env.Registry.FreightVolumeUnit))
				.Returns(new Dictionary<ZString, ZDecimal>() { { "Consol2", 20m } });

			using (ObjectFactory.Substitute(chargeableHelper.Object))
			{
				ValueProviderToTest.Reset();
				AssertEquals(10m, ValueProviderToTest.GetReplacement("<CalculateConsolChargeable(Consol1)>", Report));

				ValueProviderToTest.Reset();
				AssertEquals(20m, ValueProviderToTest.GetReplacement("<CalculateConsolChargeable(Consol2)>", Report));

				mocks.VerifyAll();
			}
		}

		protected override List<FieldInfo> FieldCollection
		{
			get
			{
				return new List<FieldInfo>()
				{
					typeof(CalculateConsolChargeable).GetField("consolChargeableCalculationHelper", BindingFlags.Instance | BindingFlags.NonPublic),
					typeof(CalculateConsolChargeable).GetField("consolChargeCalculationResult", BindingFlags.Instance | BindingFlags.NonPublic)
				};
			}
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Consol.JK_UniqueConsignRef", "TESTCONSOL"));
			var consol = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(ICommonConsol)));
			consol["JK_UniqueConsignRef"] = (ZString)"TESTCONSOL";
			consol["JK_TransportMode"] = (ZString)"AIR";
			consol["JK_RL_NKLoadPort"] = (ZString)"NZAKL";
			consol["JK_RL_NKDischargePort"] = (ZString)"AUSYD";
			consol["JK_OverrideConsolChargeable"] = (ZBool)true;
			consol["JK_ConsolChargeable"] = (ZDecimal)100m;
			Factory.Save();
		}
	}
}
