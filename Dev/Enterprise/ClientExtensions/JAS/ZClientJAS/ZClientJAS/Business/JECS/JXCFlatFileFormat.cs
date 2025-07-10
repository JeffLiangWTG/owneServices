
using Enterprise.DataTransfer.Business;

namespace Enterprise.Client.JAS.Business.JXC
{
	public class JXCFlatFileFormat : DelimitedFlatFileFormat
	{
		protected override char Delimiter
		{
			get { return JXCConstants.Delimiter; }
		}
	}
}
