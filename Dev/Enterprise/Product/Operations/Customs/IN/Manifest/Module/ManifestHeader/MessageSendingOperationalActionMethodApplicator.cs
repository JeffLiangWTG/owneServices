using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IN.Business;
using Enterprise.Customs.IN.GUI;
using Enterprise.Customs.IN.Manifest.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.IN.Manifest.Module;

sealed class MessageSendingOperationalActionMethodApplicator : OperationalActionMethodApplicator
{
	public MessageSendingOperationalActionMethodApplicator() : base(Res.GetString("6A4008EF-AA36-43BA-B6FB-227FA9EE6BE4", "Sending message"))
	{
	}

	public ZBool AllowSendWithMessageError { get; set; }

	protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
	{
		var cgmHeaders = targets.OfType<CGMAsycudaManifestHeader>().ToArray();
		if (CheckIsOKToSend(cgmHeaders, log) && SendMessageHelper.SetTokenPinIfNeeded())
		{
			new CGMManifestMessageSendingOperationalActionRunner().SendMessage(cgmHeaders, log, AllowSendWithMessageError);
		}
	}

	bool CheckIsOKToSend(IEnumerable<CGMAsycudaManifestHeader> cgmHeaders, IOperationalActionSectionLog log)
	{
		ZString errorMessage;
		if (!cgmHeaders.Any())
		{
			errorMessage = Res.GetString("4A86EFF9-3131-4C78-B9B3-5E1256CDC531", "Please select at least one India Forwarder Manifest.");
		}
		else
		{
			errorMessage = new PreMessageSendingValidation().ValidateMessageSending();
		}

		var result = errorMessage.IsEmpty;
		if (!result)
		{
			log.Notify(OperationalActionLogErrorLevel.Error, errorMessage);
			Globals.Message.ShowError(errorMessage);
		}

		return result;
	}
}
