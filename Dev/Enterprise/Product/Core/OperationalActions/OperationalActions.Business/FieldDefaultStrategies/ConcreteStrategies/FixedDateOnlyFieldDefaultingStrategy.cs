using System;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FixedDateOnlyFieldDefaultingStrategy : FieldDefaultingStrategy<OperationalActionDateFieldSupporter>
	{
		public const string CodeText = "FXD";

		public FixedDateOnlyFieldDefaultingStrategy(OperationalActionDateFieldSupporter fieldSupporter)
			: base(CodeText, Res.GetString("FixedDateOnlyFieldDefaultingStrategy|FixedDate", "Fixed Date"), fieldSupporter) { }

		public override FieldType DetailFieldType
		{
			get { return FieldType.Date; }
		}
		public override int DetailMaxLength
		{
			get { return 10; }
		}

		public override IZType GetDefaultValue(string detail)
		{
			try
			{
				return new ZDate(detail);
			}
			catch (FormatException)
			{
				return ZDate.Invalid;
			}
			catch (ZTypeValueException)
			{
				return ZDate.Invalid;
			}
		}
	}
}
