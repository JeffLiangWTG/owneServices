using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CLNTDUPMessageProcessor : CMRMessageResponseProcessor
	{
		public CLNTDUPMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.CLNTDUP, "Client Duplicate Response - (CLNTDUP)")
		{
		}

		protected override bool DoAdditionalProcessing()
		{
			CMRCLNTDUPMessage message = incomingMessage as CMRCLNTDUPMessage;
			if (message == null)
			{
				return false;
			}

			OrgHeader linkedOrganization = (OrgHeader)message.EM_LinkedObject;

			if (linkedOrganization != null)
			{
				var orgheaderWrapper = new OrgHeaderWrapper(linkedOrganization);

				if (!orgheaderWrapper.CLREGInfoProvider.ZA_ABN.IsEmpty)
				{
				}
				else
				{
					ZString customsClientID = cUSRES.Group6[0].DOC[0].DocumentMessageName.DocumentName;

					if (!customsClientID.IsEmpty)
					{
						linkedOrganization.SetLocalCustomsCode(OrgCusCode.CodeTypes.CustomsClientID, customsClientID);
					}
				}

				return true;
			}
			else
			{
				Logger.LogWarning("The incoming message is not responding to an organization. Can't continue.");
				return false;
			}
		}
	}
}
