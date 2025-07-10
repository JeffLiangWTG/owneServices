using CargoWise.Types;
using Enterprise.ZArchitecture;

namespace Enterprise.Services.OperationalActions.Business
{
	sealed class FixedTextFieldDefaultingStrategy : FieldDefaultingStrategy<OperationalActionTextFieldSupporter>
	{
		public const string CodeText = "FXD";

		public FixedTextFieldDefaultingStrategy(OperationalActionTextFieldSupporter fieldSupporter)
			: base(CodeText, FixedTextDescription, fieldSupporter) { }

		public override FieldType DetailFieldType
		{
			get { return FieldType.Text; }
		}
		public override int DetailMaxLength
		{
			get { return FieldSupporter.MaxLength; }
		}

		public override IZType GetDefaultValue(string detail)
		{
			return new ZString(detail);
		}
	}
}
