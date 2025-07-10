using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.MessageProviders.Edec.Bordereau;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business;

public sealed class BordereauListResponseMessageProcessor : BaseResponseMessageProcessor<IEdecBordereauList>
{
	public BordereauListResponseMessageProcessor(LoggingInformation logger) : base(logger)
	{
	}

	protected override string MessageFriendlyNameCore => Res.GetString("5815C829-FA1D-4FAA-9793-734E427FBE48", "Customs Bordereau List Message Response Processor");

	protected override IReadOnlyList<ZString> MessageTypesToIncludeCore => new ZString[] { MessageTypeCodeList.Codes.BOR };

	protected override IReadOnlyList<ZString> MessageSubTypesToIncludeCore => new ZString[] { MessageSubTypeCodeList.Codes.BordereauList };

	protected override BusinessObject FindLinkedObject(EDIMessage message, IEdecBordereauList xmlObject) => FindLinkedObjectByOutgoingSessionID(message);

	protected override void ProcessResponseMessage(CHEDIMessage message, IEdecBordereauList customsResponse)
	{
		if (message.EM_LinkedObject is GlbCompany company)
		{
			using (DisposableEnvironment.ForBranch(company.FirstActiveBranch.PK.ToGuid()))
			{
				var summaryLoader = new CustomsSummaryHeaderLoader(company.Factory);

				foreach (var bordereauInformation in customsResponse.BordereauInformation)
				{
					if (summaryLoader.LoadBorderau(bordereauInformation.BordereauNumber, new ZDate(bordereauInformation.CreationDate)) == null)
					{
						CreateBordereauRequest(company, bordereauInformation);
					}
				}
			}
		}
	}

	void CreateBordereauRequest(GlbCompany company, IBordereauInformation bordereauInformation)
	{
		var factory = company.Factory;

		var sendingObject = new BordereauRequestSendingObject()
		{
			ProcessingCenterNumber = bordereauInformation.ProcessingCenterNumber,
			BordereauNumber = bordereauInformation.BordereauNumber,
			CreationDate = new ZDate(bordereauInformation.CreationDate),
		};
		var messageBuilder = MessageBuilderFactory.NewMessageBuilder(sendingObject);
		var messageText = messageBuilder.GenerateXmlMessage().GetSerializedString();

		var ediMessage = factory.New<CHEDIMessage>();
		ediMessage.EM_ApplicationCode = ApplicationCode;
		ediMessage.EM_MessageType = MessageTypeCodeList.Codes.BOR;
		ediMessage.EM_MessageSubType = MessageSubTypeCodeList.Codes.BordereauResponse;
		ediMessage.EM_ReceiveTransmit = ReceiveTransmitList.Codes.Transmit;
		ediMessage.EM_Status = EDIMessage.Status.Queued;
		ediMessage.EM_MessageText = messageText;
		ediMessage.EM_LinkedObject = company;
		ediMessage.EM_GP = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company).GlbExternalPassword.PK;

		ediMessage.CreateEDIInterchange();
	}
}
