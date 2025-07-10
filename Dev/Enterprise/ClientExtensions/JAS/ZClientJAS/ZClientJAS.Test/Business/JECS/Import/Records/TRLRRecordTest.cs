using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	internal class TRLRRecordTest : JXCRecordTestCase
	{
		protected override JXCRecord GetNewRecord(ZString lineType, ZString lineContent)
		{
			return new TRLRRecord(lineType, lineContent);
		}
	}
}
