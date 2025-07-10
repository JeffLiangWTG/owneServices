using System;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class RelativeDateFieldDefaultingStrategy : FieldDefaultingStrategy<OperationalActionDateTimeFieldSupporter>
	{
		public const string CodeText = "DAY";

		public RelativeDateFieldDefaultingStrategy(OperationalActionDateTimeFieldSupporter fieldSupporter)
			: base(CodeText, Res.GetString("06ac16e9-9fdf-40b6-a158-976cd99b62e0", "Today + Days"), fieldSupporter) { }

		public override FieldType DetailFieldType
		{
			get { return FieldType.Integer; }
		}
		public override int DetailMaxLength
		{
			get { return 4; }
		}

		public override IZType GetDefaultValue(string detail)
		{
			ZInt offset;

			if (ZInt.TryParse(detail, out offset))
			{
				switch (FieldSupporter.Format)
				{
					case ZDateTimePickerFormat.Long: return ZDateTime.Now.AddDays(offset);
					case ZDateTimePickerFormat.Short: return ZDateTime.Today.AddDays(offset);
					default: throw new InvalidOperationException();
				}
			}
			else
			{
				return ZDateTime.Invalid;
			}
		}
	}
}
