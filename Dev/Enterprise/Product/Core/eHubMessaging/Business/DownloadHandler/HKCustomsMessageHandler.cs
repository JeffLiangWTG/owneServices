using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.eHubMessaging.Business.DownloadHandler
{
	[SupportedSchemaName(EDIInterchangeTypeList.Descriptions.HKCustoms)]
	class HKCustomsMessageHandler : MessageHandler
	{
		protected override EDIInterchange CreateInterchange()
		{
			var interchange = base.CreateInterchange();
			interchange.EI_ApplicationCode = ApplicationCodeList.Codes.HKTraxon;
			interchange.EI_InterchangeType = EDIInterchangeTypeList.Codes.HKCustoms;
			var messageText = new TextReaderSource(Message.MessageStream).GetReader().ReadToEnd();
			interchange.PopulateInterchangeFromString(messageText, ApplicationCodeList.Codes.HKTraxon, true, true);
			interchange.EI_Status = EDIInterchangeStatusList.Codes.Received;
			interchange.EI_InterchangeNum = ZString.Empty;
			interchange.NumberStrategy = new GenericMessageNumberStrategy(interchange.Factory, interchange.EI_InterchangeNum);
			SaveEDIMessage(interchange);
			return interchange;
		}

		void SaveEDIMessage(EDIInterchange interchange)
		{
			var branch = GetMostMatchBranch(interchange.EI_To);
			var message = CreateEDIMessage(interchange);
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.Traxon;
			message.EM_MessageType = EDIMessageTypeList.Codes.Traxon;
			message.EM_MessageSubType = EDIMessageSubTypeList.Codes.Traxon;
			message.EM_MessageText = interchange.EI_BodyText;
			if (branch != null)
			{
				interchange.EI_GB = branch.PK;
				message.EM_GB = branch.PK;
			}
			else
			{
				var noteDescription = Res.GetString("8D30242C-7351-4018-ADA4-482AB235D7D9", "Parse message error");
				var noteText = Res.GetString("75E81D08-1BBC-45F1-B59C-1ED5EEAE287D", "There is no active branch under HK company.");
				interchange.EI_Status = EDIInterchangeStatusList.Codes.Failed;
				interchange.Notes.AddNew(true, noteDescription, noteText);
				message.EM_Status = EDIMessageStatusList.Codes.Failed;
				message.Notes.AddNew(true, noteDescription, noteText);
			}
		}

		GlbBranch GetMostMatchBranch(ZString recipientId)
		{
			var companies = GlbCompany.GetActiveCompanies(Core.Constants.CountryCodes.HongKong);
			var mostMatchCompany = (companies.Length == 1 ? companies[0] : null) ?? companies.FirstOrDefault(c => recipientId == GetSenderID(c)) ?? companies.FirstOrDefault();
			return mostMatchCompany?.FirstActiveBranch;
		}

		string GetSenderID(GlbCompany company)
		{
			return (string)ObjectFactory.Get<Integration.Customs.HK.IHKCustomsDataRegistry>().HKTraxonSenderID.GetFallBackValueAtAllLevels(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
		}
	}
}
