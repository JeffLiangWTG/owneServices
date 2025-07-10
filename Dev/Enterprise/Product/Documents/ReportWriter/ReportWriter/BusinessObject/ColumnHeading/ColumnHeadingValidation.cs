using CargoWise.EntityFramework;

namespace Enterprise.ReportWriter
{
	public class ColumnHeadingValidation : AutoColumnHeadingValidation
	{
		public ColumnHeadingValidation(AutoColumnHeading parent)
			: base(parent)
		{
		}

		protected override void CheckColumnNumber()
		{
			base.CheckColumnNumber();
			MandatoryValidation.CheckNotNegative(Parent.ColumnNumberInfo);
			MandatoryValidation.CheckNotZero(Parent.ColumnNumberInfo);
		}
	}
}
