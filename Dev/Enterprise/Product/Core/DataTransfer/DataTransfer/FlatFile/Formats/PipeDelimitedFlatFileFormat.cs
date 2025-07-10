namespace Enterprise.DataTransfer.Business
{
	public class PipeDelimitedFlatFileFormat : DelimitedFlatFileFormat
	{
		protected override char Delimiter
		{
			get { return '|'; }
		}
	}
}
