using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EXDOCMessageDecoderNotice
	{
		public EXDOCMessageDecoderNotice()
		{
		}

		public void Process(ZString notice)
		{
			lineNumber = notice.SubstringSafe(0, 3);
			errorMessageIdentifier = notice.SubstringSafe(4, 5);
			narrativeMessage = notice.SubstringSafe(10);
		}

		public ZString lineNumber;
		public ZString errorMessageIdentifier;
		public ZString narrativeMessage;
	}
}
