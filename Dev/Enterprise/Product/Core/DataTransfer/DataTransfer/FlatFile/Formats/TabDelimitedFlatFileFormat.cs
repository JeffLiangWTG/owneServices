namespace Enterprise.DataTransfer.Business
{
	public class TabDelimitedFlatFileFormat : DelimitedFlatFileFormat
	{
		protected override char Delimiter
		{
			get { return '\t'; }
		}
	}
}
