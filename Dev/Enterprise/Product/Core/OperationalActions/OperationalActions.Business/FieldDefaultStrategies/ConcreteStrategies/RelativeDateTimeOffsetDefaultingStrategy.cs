using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class RelativeDateTimeOffsetFieldDefaultingStrategy : FieldDefaultingStrategy<OperationalActionDateTimeOffsetFieldSupporter>
	{
		public const string CodeText = "DAY";

		public RelativeDateTimeOffsetFieldDefaultingStrategy(OperationalActionDateTimeOffsetFieldSupporter fieldSupporter)
			: base(CodeText, Res.GetString("06ac16e9-9fdf-40b6-a158-976cd99b62e0", "Today + Days"), fieldSupporter)
		{ }

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
				return ZDateTimeOffset.Now.AddDays(offset);
			}
			else
			{
				return ZDateTimeOffset.Invalid;
			}
		}
	}
}
