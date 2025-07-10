using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.JXC.Import.Testing
{
	public class DummyRecord : JXCRecord
	{
		public DummyRecord() : this("", "")
		{
		}

		public DummyRecord(ZString lineType, ZString lineContent) : base(lineType, lineContent)
		{
		}
	}
}
