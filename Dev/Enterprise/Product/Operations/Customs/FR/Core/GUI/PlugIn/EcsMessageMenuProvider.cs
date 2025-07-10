using System.Linq;
using Enterprise.Customs.FR.Business;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.FR.GUI.PlugIn
{
	public class EcsMessageMenuProvider : EU.GUI.PlugIn.EcsMessageMenuProvider
	{
		public EcsMessageMenuProvider(EU.Business.CusExitControlHeader exitHeader) : base(exitHeader)
		{
		}

		public override bool CreateArrivalMessages()
		{
			return SendECSMessage(MessageSubTypeList.Codes.ARR);
		}

		public override bool CreateDepartureMessages()
		{
			return SendECSMessage(MessageSubTypeList.Codes.DEP);
		}

		bool SendECSMessage(string messageSubType)
		{
			var errorCollector = new EU.Business.ErrorCollector();

			var succeeded = false;

			if (!ExitHeader.CusExitDetails.Any())
			{
				Globals.Message.Show(Res.GetString("92480CB5-C7F6-4C6F-93CA-7B7B7AED0D37", "There is no movements to be sent."));
			}
			else
			{
				foreach (CusExitDetail exitDetail in ExitHeader.CusExitDetails)
				{
					new ECSMessageSender(exitDetail, messageSubType, errorCollector).Send();
				}

				succeeded = errorCollector.ErrorCount == 0;
				if (succeeded)
				{
					Globals.Message.ShowInformation(ECSMessageSender.MessageSendSuccessful);
				}
				else
				{
					Globals.Message.ShowError(errorCollector.GetErrorsAsString());
				}
			}

			return succeeded;
		}
	}
}
