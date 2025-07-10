using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business.MessageManagers
{
	public class TradeChainPartnerMessageManager : NonPersistentBusinessObject
	{
		public TradeChainPartnerMessageManager(OrgHeaderTCPMessageWrapper wrapper)
		{
			fOrganisation = OrgImpAddInfo.Get(wrapper.organisation);
			fTCPCollection = fOrganisation.TradeChainPartnersToBeUpdated;
			this.wrapper = wrapper;
		}

		readonly OrgHeaderTCPMessageWrapper wrapper;

		public OrgImpAddInfo Organisation
		{
			get
			{
				return fOrganisation;
			}
		}

		readonly OrgImpAddInfo fOrganisation;

		public TradeChainPartnerSendingObjectCollection TCPCollection
		{
			get
			{
				return fTCPCollection;
			}
		}

		readonly TradeChainPartnerSendingObjectCollection fTCPCollection;

		public MessageSubTypes MessageSubType
		{
			get
			{
				return fMessageSubType;
			}
		}
		MessageSubTypes fMessageSubType;

		public void SendMessage()
		{
			var count = 0;
			TCPCollection.RunPreSaveValidation();
			var testMode = !Env.Instance.IsProductionSystem;
			foreach (TradeChainPartnerSendingObject tcp in TCPCollection)
			{
				if (tcp.SendOption)
				{
					if (tcp.CA_Action == CSAActionTypeList.Codes.ReqAdd)
					{
						fMessageSubType = MessageSubTypes.AddLines;
					}
					else
					{
						fMessageSubType = MessageSubTypes.Delete;
					}
					var results = new TradeChainPartnerCuspedMessageBuilder(tcp, fMessageSubType).PopulateMessages().GetBuilderResults();
					foreach (var result in results)
					{
						var message = result.Message;
						message.EM_LinkedObject = Organisation.OrgHeader;
						message.EM_IsTestMessage = testMode;
						wrapper.Messages.Add(message);
						count++;
					}
					if (results.Any())
					{
						if (tcp.CA_Action == CSAActionTypeList.Codes.ReqAdd)
						{
							tcp.TradeChainPartner.CA_CSAStatus = CSAStatusList.Codes.AwaitingAdd;
						}
						else if (tcp.CA_Action == CSAActionTypeList.Codes.ReqDel)
						{
							tcp.TradeChainPartner.CA_CSAStatus = CSAStatusList.Codes.AwaitingDelete;
						}
					}
				}
			}
			wrapper.Factory.Save();
			var caption = Res.GetString("008A2667-FD55-43F9-A914-F4C23FBC800D", "Message queued");
			Globals.Message.ShowInformation(Res.GetString("C88E0CC8-13CF-4D4C-A9D7-51F9B0BBFB1D", "{0} message(s) queued for sending.", count), caption);
		}
	}
}
