using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(AWBRoundingCollection))]
	sealed class AWBRoundingCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<AWBRoundingCollection>
	{
		public void TestGetDefault()
		{
			AWBRoundingCollection defaultValue = AWBRoundingCollection.GetDefault();
			AssertEquals(4, defaultValue.Count);

			Action<string, ChargeableWeightRoundingType, string> assertAWBRounding = (awbType, expectedRoundingType, expectedScale) =>
				{
					var awbRounding = defaultValue[awbType];

					AssertNotNull(string.Format("Default values must contain {0} type", awbType), awbRounding);
					AssertEquals(expectedRoundingType.ToString(), awbRounding.RoundingMode);
					AssertEquals(expectedScale, awbRounding.RoundingScale);
				};

			assertAWBRounding(AWBRounding.Keys.AgentMaster, ChargeableWeightRoundingType.Up, ChargeableWeightRoundingScales.DefaultScale);
			assertAWBRounding(AWBRounding.Keys.DirectMaster, ChargeableWeightRoundingType.Up, ChargeableWeightRoundingScales.DefaultScale);
			assertAWBRounding(AWBRounding.Keys.House, ChargeableWeightRoundingType.Up, ChargeableWeightRoundingScales.DefaultScale);
			assertAWBRounding(AWBRounding.Keys.MasterHouse, ChargeableWeightRoundingType.Up, ChargeableWeightRoundingScales.DefaultScale);
		}

		public void TestAllowNew()
		{
			AssertEquals("Must not allow new rows", false, this.Collection.AllowNew);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override AWBRoundingCollection GetCollectionToTest()
		{
			return new AWBRoundingCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new AWBRounding();
		}

		#endregion
	}
}
