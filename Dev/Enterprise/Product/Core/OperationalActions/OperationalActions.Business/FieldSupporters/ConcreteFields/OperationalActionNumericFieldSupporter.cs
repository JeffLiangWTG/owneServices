using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed partial class OperationalActionNumericFieldSupporter : OperationalActionFieldSupporter
	{
		public OperationalActionNumericFieldSupporter(string field, bool readOnly, decimal minValue, decimal maxValue, int precision, int scale)
			: base(field, readOnly)
		{
			this.minValue = minValue;
			this.maxValue = maxValue;
			this.precision = precision;
			this.scale = scale;
		}

		public decimal MinValue
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return minValue; }
		}

		public decimal MaxValue
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return maxValue; }
		}

		public int Precision
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return precision; }
		}

		public int Scale
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return scale; }
		}

		public bool IgnoreDecimalPrecisionCheck { get; set; }

		protected override RunnerField NewRunnerFieldCore(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
		{
			return new RunnerNumericField(factory, descriptor, this);
		}

		protected override string AsFilterStringCore(IZType value, BusinessObjectFactory factory)
		{
			if (value is ZDecimal)
			{
				return Math.Round((ZDecimal)value, Scale).ToString();
			}
			else
			{
				return value.ToString();
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly decimal minValue;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly decimal maxValue;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly int precision;
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly int scale;
	}
}

#region Test
#if DEBUG

#region Self Checking

namespace Enterprise.Services.OperationalActions.Business
{
	using CargoWise.EntityFramework;

	partial class OperationalActionNumericFieldSupporter
	{
		protected override void PerformSelfCheckForTesting(BusinessObjectFactory factoryForTesting, List<string> errors)
		{
			base.PerformSelfCheckForTesting(factoryForTesting, errors);

			if (MinValue > MaxValue)
			{
				errors.Add("MinValue cannot be greater than MaxValue");
			}

			if (Precision <= 0)
			{
				errors.Add("Precision must be atleast 1");
			}

			if (Scale < 0)
			{
				errors.Add("Scale must be positive.");
			}
			else if (Scale > Precision)
			{
				errors.Add("Scale cannot be greater than Precision");
			}
		}
	}
}

#endregion

#endif
#endregion
