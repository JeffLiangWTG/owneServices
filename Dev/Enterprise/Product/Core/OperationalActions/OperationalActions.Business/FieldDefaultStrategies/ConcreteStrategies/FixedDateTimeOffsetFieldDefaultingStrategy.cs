using System;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FixedDateTimeOffsetFieldDefaultingStrategy : FieldDefaultingStrategy<OperationalActionDateTimeOffsetFieldSupporter>
	{
		public const string CodeText = "FXD";

		public FixedDateTimeOffsetFieldDefaultingStrategy(OperationalActionDateTimeOffsetFieldSupporter fieldSupporter)
			: base(CodeText, Res.GetString("daa6b1fc-39cc-472a-8538-34a921932c70", "Fixed Date"), fieldSupporter)
		{ }

		public override FieldType DetailFieldType
		{
			get
			{
				return FieldType.DateTimeOffset;
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
				return new ZDateTimeOffset(detail);
			}
			catch (FormatException)
			{
				return ZDateTimeOffset.Invalid;
			}
			catch (ZTypeValueException)
			{
				return ZDateTimeOffset.Invalid;
			}
		}
	}
}
