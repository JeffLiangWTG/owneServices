using System;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FixedDateFieldDefaultingStrategy : FieldDefaultingStrategy<OperationalActionDateTimeFieldSupporter>
	{
		public const string CodeText = "FXD";

		public FixedDateFieldDefaultingStrategy(OperationalActionDateTimeFieldSupporter fieldSupporter)
			: base(CodeText, Res.GetString("daa6b1fc-39cc-472a-8538-34a921932c70", "Fixed Date"), fieldSupporter) { }

		public override FieldType DetailFieldType
		{
			get
			{
				switch (FieldSupporter.Format)
				{
					case ZDateTimePickerFormat.Long: return FieldType.DateTime;
					case ZDateTimePickerFormat.Short: return FieldType.Date;
					default: throw new InvalidOperationException();
				}
			}
		}
		public override int DetailMaxLength
		{
			get { return 15; }
		}

		public override IZType GetDefaultValue(string detail)
		{
			try
			{
				return new ZDateTime(detail);
			}
			catch (FormatException)
			{
				return ZDateTime.Invalid;
			}
			catch (ZTypeValueException)
			{
				return ZDateTime.Invalid;
			}
		}
	}
}
