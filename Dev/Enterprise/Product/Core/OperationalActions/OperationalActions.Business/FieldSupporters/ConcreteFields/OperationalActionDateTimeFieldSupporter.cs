using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed partial class OperationalActionDateTimeFieldSupporter : OperationalActionFieldSupporter
	{
		public OperationalActionDateTimeFieldSupporter(string field, bool readOnly, ZDateTimePickerFormat format, bool isDuration = false)
			: base(field, readOnly)
		{
			this.format = format;
			IsDuration = isDuration;
		}

		public bool IsDuration { get; }

		public ZDateTimePickerFormat Format
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return format; }
		}

		protected override void PopulateDefaultingStrategies(IList<IFieldDefaultingStrategy> strategies)
		{
			switch (format)
			{
				case ZDateTimePickerFormat.Short:
				case ZDateTimePickerFormat.Long:
					strategies.Add(new FixedDateFieldDefaultingStrategy(this));
					strategies.Add(new RelativeDateFieldDefaultingStrategy(this));
					break;
			}
		}

		protected override RunnerField NewRunnerFieldCore(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor)
		{
			return new RunnerDateTimeField(factory, descriptor, this);
		}

		protected override string AsFilterStringCore(IZType value, BusinessObjectFactory factory)
		{
			ZDateTime dateTime = (ZDateTime)value;

			switch (Format)
			{
				case ZDateTimePickerFormat.Long: return dateTime.ToLongTimeString();
				case ZDateTimePickerFormat.Short: return dateTime.ToShortDateString();
				case ZDateTimePickerFormat.Time: return dateTime.ToShortTimeString();
				default: return null;
			}
		}

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		readonly ZDateTimePickerFormat format;
	}
}

#region Test
#if DEBUG

#region Self Checking

namespace Enterprise.Services.OperationalActions.Business
{
	using System;
	using System.Collections.Generic;

	partial class OperationalActionDateTimeFieldSupporter
	{
		protected override void PerformSelfCheckForTesting(BusinessObjectFactory factoryForTesting, List<string> errors)
		{
			base.PerformSelfCheckForTesting(factoryForTesting, errors);

			if (!Enum.IsDefined(typeof(ZDateTimePickerFormat), format))
			{
				errors.Add(string.Format("Invalid Format ({0}).", format));
			}
		}
	}
}

#endregion

#endif
#endregion
