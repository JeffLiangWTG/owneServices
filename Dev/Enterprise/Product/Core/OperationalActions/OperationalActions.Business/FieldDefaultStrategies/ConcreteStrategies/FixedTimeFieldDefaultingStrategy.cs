using System;
using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FixedTimeFieldDefaultingStrategy : FieldDefaultingStrategy<OperationalActionTimeFieldSupporter>
	{
		public const string CodeText = "FXD";

		public FixedTimeFieldDefaultingStrategy(OperationalActionTimeFieldSupporter fieldSupporter)
			: base(CodeText, Res.GetString("695023BA-BB50-41FF-8568-D2EF14A6AB0B", "Fixed Time"), fieldSupporter) { }

		public override int DetailMaxLength
		{
			get { return 15; }
		}

		public override FieldType DetailFieldType => FieldType.Time;

		public override IZType GetDefaultValue(string detail)
		{
			try
			{
				return new ZTime(detail);
			}
			catch (FormatException)
			{
				return ZTime.Invalid;
			}
			catch (ZTypeValueException)
			{
				return ZTime.Invalid;
			}
		}
	}
}
