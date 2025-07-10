using System;
using CargoWise.Common;

namespace Enterprise.ZArchitecture.Business
{
	[AttributeUsage(AttributeTargets.Property)]
	public sealed class MeasureUnitAttribute : Attribute
	{
		public MeasureUnitAttribute(string unitProperty, MeasureUnitType unitType, string exposureMember = null)
		{
			this.unitProperty = Argument.NotNullOrEmpty(unitProperty, "unitProperty");
			this.unitType = unitType;
			this.exposureMember = exposureMember;
		}

		readonly string unitProperty;
		readonly MeasureUnitType unitType;
		readonly string exposureMember;

		public string UnitProperty
		{
			get { return unitProperty; }
		}

		public MeasureUnitType UnitType
		{
			get { return unitType; }
		}

		public string ExposureMember
		{
			get { return exposureMember; }
		}
	}
}
