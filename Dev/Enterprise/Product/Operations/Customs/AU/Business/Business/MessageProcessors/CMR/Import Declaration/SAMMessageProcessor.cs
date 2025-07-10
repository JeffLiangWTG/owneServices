using CargoWise.Types;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SAMMessageProcessor : BaseImportDeclarationMessageProcessor
	{
		public SAMMessageProcessor(LoggingInformation logger)
			: base(logger, CMRMessage.CMRMessageTypes.SAM, "Status Advice Message (SAM)")
		{
		}

		protected override bool IsUnsolicitedMessage
		{
			get { return true; }
		}

		protected override bool DoAdditionalProcessing()
		{
			bool result = base.DoAdditionalProcessing();
			if (!result)
			{
				declaration = incomingMessage.EM_LinkedObject as JobDeclaration;
				if (declaration == null)
				{
					Logger.LogWarning("The incoming SAM message is not responding to either an entry header or declaration.  Can't continue.");
					result = false;
				}
				else
				{
					result = true;
					SetDeclarationNumber();
					SetDeclarationStatus();
				}
			}
			return result;
		}

		void SetDeclarationNumber()
		{
			CMRCUSRESMessage message = incomingMessage;
			if (message != null)
			{
				ZString declarationNumber = message.EntryNumber;
				if (!declarationNumber.IsEmpty)
				{
					declaration.DeclarationNumber = declarationNumber;
				}
			}
		}

		void SetDeclarationStatus()
		{
			if (!statusType.IsEmpty)
			{
				DeclarationStatusProcessor declarationStatusProcessor = new DeclarationStatusProcessor(declaration);
				declarationStatusProcessor.SetDeclarationStatus(statusType);
			}
		}
	}
}
