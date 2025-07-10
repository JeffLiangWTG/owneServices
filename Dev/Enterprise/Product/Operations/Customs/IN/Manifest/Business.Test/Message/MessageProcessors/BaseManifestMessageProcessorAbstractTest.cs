using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.IN.Business;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

abstract class BaseManifestMessageProcessorAbstractTest : TestCaseWithFactory
{
	public void TestCanProcess()
	{
		var attachmentText = $"HREC\u001dZZ\u001dINBOM4\u001dZZ\u001dLIPLINDIA\u001dICES1_5\u001dP\u001d\u001d{MessageID}\u001d3447163\u001d20240108\u001d1106";
		var emailInfo = new EmailInfo
		{
			HasAttachments = true,
			AttachmentText = attachmentText
		};
		var processor = GetProcessor();
		Assert(processor.CanProcess(emailInfo));

		emailInfo = new EmailInfo
		{
			HasAttachments = true,
			AttachmentText = attachmentText.Replace(MessageID, "ABCDEFG")
		};
		Assert(!processor.CanProcess(emailInfo));
	}

	protected abstract IEmailMessageProcessor GetProcessor();
	protected abstract string MessageID { get; }
}

