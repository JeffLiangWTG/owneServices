
namespace Enterprise.BufferManagement.Business
{
	public interface ICCRHeadingCellContent
	{
		ConstraintStatus CCRStatus { get; set; }
	}

	class CCRHeadingCellContent : CellContent, ICCRHeadingCellContent
	{
		public CCRHeadingCellContent(int row, int column, BMComponentSectionConfiguration sectionConfiguration = null)
			: base(row, column, CellContentType.CCRHeading, sectionConfiguration)
		{
		}

		public ConstraintStatus CCRStatus { get; set; }
	}
}
