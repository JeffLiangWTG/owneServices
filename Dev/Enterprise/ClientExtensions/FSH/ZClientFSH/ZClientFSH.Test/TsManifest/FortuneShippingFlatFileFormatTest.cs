using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;

namespace Enterprise.Client.FSH.TsManifest
{
	public class FortuneShippingFlatFileFormatTest : FlatFileFormatTestCase
	{
		protected override FlatFileFormat GetFlatFileFormat()
		{
			return new FortuneShippingFlatFileFormat();
		}
	}
}
