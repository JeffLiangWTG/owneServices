using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.TemporaryStorage;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.DE.Business.CusTempStorage
{
	public abstract class TemporaryStorageSender
	{
		protected TemporaryStorageSender(CusTempStorageDec storageDec, ZString messageName, ITempStorageDec dataProvider)
		{
			this.storageDec = storageDec;
			messageBuilder = TemporaryStorageMessageBuilderLoader.Instance.GetMessageBuilder(messageName, dataProvider);
			this.dataProvider = dataProvider;
		}
		readonly CusTempStorageDec storageDec;
		readonly IProduceMessageXml messageBuilder;
		protected readonly ITempStorageDec dataProvider;

		public virtual (bool Can, ZString WhyCannotSend) CanSend
		{
			get
			{
				var result = true;
				var reason = ZString.Empty;
				var user = GlbStaff.CurrentUser;
				var missingDetails = GetMissingDetails();

				if (!missingDetails.IsEmpty)
				{
					result = false;
					reason = Res.GetString("D6C6E9AB-B1E8-4235-85AC-DA612C816DB2", "Staff profile is missing a {0}. Please add missing details in Module: Staff and Resources.", missingDetails);
				}
				return (result, reason);

				ZString GetMissingDetails()
				{
					var emptyProperties = new List<string>();
					if (user.GS_Title.IsEmpty)
					{
						emptyProperties.Add(Res.GetString("929F9CF8-C253-4423-8122-398FB9F4ADF0", "Job Title"));
					}
					if (user.GS_FullName.IsEmpty)
					{
						emptyProperties.Add(Res.GetString("02982C8C-D8C7-4642-BE56-CF67CEBEEA7F", "Full Name"));
					}
					if (user.GS_WorkPhone_Wrapper.FormattedForBinding.IsEmpty)
					{
						emptyProperties.Add(Res.GetString("07A6249F-300B-41EA-9B93-421D8BDC356D", "Work Phone"));
					}
					return emptyProperties.Count > 1
						? Res.GetString("4857835b-de05-4897-8946-4ee6f8ee1b6a", "{0} and {1}", string.Join(", ", emptyProperties.Take(emptyProperties.Count - 1)), emptyProperties.Last())
						: emptyProperties.SingleOrDefault();
				}
			}
		}

		public void Send()
		{
			var message = storageDec.Factory.New<AtlasEDIMessage>();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageType = EDIMessageTypeList.Codes.TemporaryStorage;
			message.EM_MessageSubType = MessageSubType;
			message.SetEM_MessageTextOrDataSource(messageBuilder.GetXMLMessage());
			message.EM_LinkedObject = storageDec;
			message.EM_ApplicationReference = messageBuilder.MessageTechnicalName;
			message.SetLogbookRegistrationNumber(RegistrationNumber);
			message.SetLogbookEORIBranchSuffix(dataProvider.Header.InterchangeSenderEoriBranch);
			message.SetLogbookLocalReferenceNumber(dataProvider.Header.LocalReferenceNumber);

			storageDec.STH_MessageStatus = Common.Shared.MessageStatusList.Codes.Sent;
			storageDec.Messages.Add(message);
		}

		protected virtual string RegistrationNumber => string.Empty;

		protected abstract string MessageSubType { get; }

		protected bool IdentificationIdicatorIsREG => dataProvider.IdentificationIndicator == TemporaryStorageIdentificationIndicatorList.Codes.REG;
	}
}
