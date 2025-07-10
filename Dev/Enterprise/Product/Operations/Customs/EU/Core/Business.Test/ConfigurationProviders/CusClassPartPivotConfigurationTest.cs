using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.Business.MasterFiles;
using NUnit.Framework;
using NUnit.Framework.TestHelper;
using WTG.NUnit;

namespace Enterprise.Customs.EU.Business.Testing
{
	[TestsSubclassesOf(typeof(CusClassPartPivotConfiguration))]
	public abstract class CusClassPartPivotConfigurationAbstractTest : TestCaseWithFactory
	{
		public abstract void TestUCCAdditionalInfosSupport();

		protected override void SetUp()
		{
			base.SetUp();
			configuration = (CusClassPartPivotConfiguration)Activator.CreateInstance(TestedTypeHelper.GetTestedType(GetType()));
		}
		protected CusClassPartPivotConfiguration configuration;
	}

	[TestedType(typeof(CusClassPartPivotConfiguration))]
	sealed class CusClassPartPivotConfigurationBaseTest : CusClassPartPivotConfigurationAbstractTest
	{
		[ExpectNoExceptions]
		public void TestGetLineSupporter()
		{
			CombineAssertions(() =>
			{
				var configuration = CusClassPartPivotConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.EuropeanUnion);
				NUnit.Framework.Assert.That(configuration.GetType().Namespace, NUnit.Framework.Is.EqualTo("Enterprise.Customs.EU.Business"), "Country Code EU returns base");
				configuration = CusClassPartPivotConfiguration.GetConfiguration(Factory, ZString.Empty);
				NUnit.Framework.Assert.That(configuration.GetType().Namespace, NUnit.Framework.Is.EqualTo("Enterprise.Customs.EU.Business"), "Empty Country Code returns base");
				configuration = CusClassPartPivotConfiguration.GetConfiguration(Factory, Core.Constants.CountryCodes.Ireland);
				NUnit.Framework.Assert.That(configuration.GetType().Namespace, NUnit.Framework.Is.EqualTo("Enterprise.Customs.IE.Business"), "Implemented Country Code returns country class");
			});
		}

		[ExpectNoExceptions]
		public override void TestUCCAdditionalInfosSupport()
		{
			NUnit.Framework.Assert.That(configuration.UCCAdditionalInfosSupport(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "BTH");
			partPivot.CI_ChildType = ClassificationType.IMP;
			NUnit.Framework.Assert.That(configuration.UCCAdditionalInfosSupport(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "IMP");
			partPivot.CI_ChildType = ClassificationType.EXP;
			NUnit.Framework.Assert.That(configuration.UCCAdditionalInfosSupport(partPivot), NUnit.Framework.Is.EqualTo(false).Using(CustomComparers.TypeComparison), "EXP");
		}

		protected override void SetUp()
		{
			base.SetUp();
			partPivot = Factory.New<CusClassPartPivot>();
		}
		CusClassPartPivot partPivot;
	}
}
