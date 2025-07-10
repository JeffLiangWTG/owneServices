using System;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Services.OperationalActions.Business
{
	public sealed class RunnerNumericField : RunnerField<OperationalActionNumericFieldSupporter, ZDecimal>
	{
		public RunnerNumericField(BusinessObjectFactory factory, OperationalActionFieldDescriptor descriptor, OperationalActionNumericFieldSupporter fieldSupporter)
			: base(factory, descriptor, fieldSupporter) { }

		protected override void ValidatePropertyCore()
		{
			base.ValidatePropertyCore();
			if (!FieldSupporter.IgnoreDecimalPrecisionCheck)
			{
				TypeValidation.CheckValidDecimal(PropertyInfo, FieldSupporter.Precision, FieldSupporter.Scale);
			}

			if (Property < FieldSupporter.MinValue || Property > FieldSupporter.MaxValue)
			{
				int scale = FieldSupporter.Scale;
				string formatString = FormatString(scale);

				PropertyInfo.AddError(Res.GetString("707b5071-0086-4027-9fd2-0a25076567c5", "Must be between {0} and {1}.",
					FieldSupporter.MinValue.ToString(formatString),
					FieldSupporter.MaxValue.ToString(formatString)));
			}
		}

		protected override IZType GetValueCore(Type expectedType)
		{
			if (expectedType == typeof(ZInt))
			{
				return (ZInt)Property.Truncate();
			}
			else if (expectedType == typeof(ZShort))
			{
				return (ZShort)(int)Property.Truncate();
			}
			else if (expectedType == typeof(ZByte))
			{
				return (ZByte)(int)Property.Truncate();
			}
			else
			{
				return Property;
			}
		}

		static string FormatString(int scale)
		{
			string formatString;

			if (scale > 0)
			{
				StringBuilder builder = new StringBuilder(scale + 2);
				builder.Append("0.");
				builder.Append('0', scale);
				formatString = builder.ToString();
			}
			else
			{
				formatString = "0";
			}

			return formatString;
		}
	}
}
