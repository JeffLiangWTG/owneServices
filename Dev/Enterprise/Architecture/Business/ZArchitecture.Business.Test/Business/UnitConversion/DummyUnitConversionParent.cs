using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public class DummyUnitConversionParent : DummyBusinessObject
	{
		public DummyUnitConversionParent(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[MeasureUnit("WeightUnit", MeasureUnitType.Weight)]
		public ZDecimal WeightAmount1 { get; set; }

		[MeasureUnit("WeightUnit", MeasureUnitType.Weight)]
		public ZDecimal WeightAmount2 { get; set; }

		[ReadOnly(true)]
		[MeasureUnit("WeightUnit", MeasureUnitType.Weight)]
		public ZDecimal WeightAmount3 { get; set; }

		[MeasureUnit("WeightUnit", MeasureUnitType.Weight, "IsAmount4Exposed")]
		public ZDecimal WeightAmount4 { get; set; }

		public bool IsAmount4Exposed { get; set; }

		[List("TestUnits")]
		public ZString WeightUnit { get; set; }

		[MeasureUnit("VolumeUnit", MeasureUnitType.Volume)]
		public ZDecimal VolumeAmount { get; set; }

		[List("Lookups.VolumeUnits")]
		public ZString VolumeUnit { get; set; }

		[MeasureUnit("VolumeUnit2", MeasureUnitType.Volume)]
		public ZInt VolumeAmout2 { get; set; }

		[List("Lookups.VolumeUnits")]
		public ZString VolumeUnit2 { get; set; }

		public DummyUnitConversionParent DummyParent { get; set; }

		public DummyUnitLookups Lookups
		{
			get { return lookups ?? (lookups = new DummyUnitLookups()); }
		}

		DummyUnitLookups lookups;

		public CodeDescriptionPairList TestUnits
		{
			get
			{
				if (testUnits == null)
				{
					testUnits = new CodeDescriptionPairList();
					testUnits.AddPair(Constants.Weight.Grams);
					testUnits.AddPair(Constants.Weight.Kilograms);
					testUnits.AddPair(Constants.Weight.Pounds);
				}

				return testUnits;
			}
		}

		CodeDescriptionPairList testUnits;
	}
}
