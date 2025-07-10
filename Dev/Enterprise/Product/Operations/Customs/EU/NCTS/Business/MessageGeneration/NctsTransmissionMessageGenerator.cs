using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public class NctsTransmissionMessageGenerator : IMessageGenerator<NctsHeader>
	{
		public NctsTransmissionMessageGenerator(NctsMessageFunctionSet messageFunction)
		{
			this.messageFunction = messageFunction;
		}

		#region IMessageGenerator Members

		public IBuilderResult Generate(NctsHeader nctsHeader)
		{
			this.nctsHeader = nctsHeader;
			var errorCollector = new ErrorCollector();
			var (messageText, messageType, messageSubType) = GetMessageTextAndMessageType(nctsHeader, messageFunction, errorCollector);
			var builderResult = new BuilderResult(nctsHeader, errorCollector.GetErrors(), AfterFullSuccess);
			builderResult.Message = nctsHeader.Messages.AddNew();

			builderResult.Message.EM_ApplicationCode = GetApplicationCode();
			builderResult.Message.EM_MessageOwner = GetMessageOwner(nctsHeader, errorCollector);
			builderResult.Message.MessageNumberStrategy = GetMessageNumberStrategy(builderResult);
			builderResult.Message.EM_MessageText = messageText;
			builderResult.Message.EM_SendWithMessageErrors = nctsHeader.HasErrors || nctsHeader.HasMessageErrors;
			builderResult.Message.EM_MessageType = messageType;
			builderResult.Message.EM_MessageSubType = messageSubType;
			builderResult.Message.EM_ApplicationReference = "";
			builderResult.Message.EM_LinkedObject = nctsHeader;
			builderResult.Message.EM_Status = EDIMessage.Status.Queued;
			builderResult.Message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			NctsMessageGeneratorHelper.AddPermitRecords(nctsHeader, builderResult.Message);
			return builderResult;
		}

		protected virtual IMessageNumberStrategy GetMessageNumberStrategy(BuilderResult builderResult)
		{
			return new NctsMessageNumberStrategy(builderResult.Message.Factory, "NCT");
		}

		protected virtual ZString GetApplicationCode()
		{
			return EDIMessage.ApplicationCodes.EuNcts;
		}

		protected virtual ZString GetMessageOwner(NctsHeader header, ErrorCollector errorCollector)
		{
			var eori = header.DeclarantId;
			if (eori.IsEmpty)
			{
				errorCollector.AddError(Res.GetString("94AFA15E-D132-420B-B2F9-035B931B52FE", "EORI for declarant is mandatory for native messaging"));
			}
			return eori;
		}

		protected virtual (ZString MessageText, ZString MessageType, ZString MessageSubType) GetMessageTextAndMessageType(NctsHeader header, NctsMessageFunctionSet how, ErrorCollector errorCollector)
		{
			var nctsEdifactBuilderChooser = new NctsNativeBuilderChooser(header, how);
			var messageText = nctsEdifactBuilderChooser.NativeMessage(header, how, errorCollector);
			var messageType = nctsEdifactBuilderChooser.NctsDomainCountryCode;
			var messageSubType = ((ZString)how.Code).Right(3);
			return (messageText, messageType, messageSubType);
		}

		public ZString MakePrettyForInterpretation(EDIMessage message)
		{
			var messageStatusCode = messageFunction == null ? "" : messageFunction.GetSentMessageStatusCode;
			var messageFunctionCode = messageFunction == null ? "" : messageFunction.Code;
			var messageDescription = new NctsMessageStatusList().GetDescriptionFromCode(messageStatusCode);
			var list = new ZString[] { NctsMessageStatusList.Descriptions.Ok, NctsMessageStatusList.Descriptions.Rejected, NctsMessageStatusList.Codes.Unknown };
			if (string.IsNullOrEmpty(messageDescription))
			{
				messageDescription = Res.GetString("DC3610A2-18FB-4C66-875E-F20B9EBD324C", "NCTS Message {0}", messageFunctionCode);
			}
			else if (list.Contains(messageDescription))
			{
				messageDescription = Res.GetString("914987D5-9FE7-472C-9574-2D38D99D024D", "NCTS Message {0} {1}", messageFunctionCode, messageDescription);
			}
			var table = new HtmlTableCreator(new string[] { (NoResString)"Field", (NoResString)"Value" });
			table.WriteRow((NoResString)"Message code", messageFunctionCode);
			table.WriteRow((NoResString)"Message number", message.EM_MessageNum);
			if (messageFunction != null && !messageFunction.AdditionalInformation.IsEmpty)
			{
				table.WriteRow((NoResString)"Extra Information", messageFunction.AdditionalInformation);
			}
			if (!nctsHeader.MovementReferenceNumber.IsEmpty)
			{
				table.WriteRow((NoResString)"MRN", nctsHeader.MovementReferenceNumber);
			}
			table.WriteRow((NoResString)"LRN", nctsHeader.LocalReferenceNumber);
			table.WriteRow((NoResString)"Declarant", nctsHeader.Declarant.E2_CompanyName);
			var departureCustomsOfficeCode = nctsHeader.IsPhase5 ? nctsHeader.MovementHeader.DepartureCustomsOfficeCode : nctsHeader.DepartureCustomsOfficeCode;
			if (!departureCustomsOfficeCode.IsEmpty)
			{
				table.WriteRow((NoResString)"Departing", departureCustomsOfficeCode);
			}
			var destinationCustomsOfficeCode = nctsHeader.IsPhase5 ? nctsHeader.MovementHeader.DestinationCustomsOfficeCode : nctsHeader.DestinationCustomsOfficeCode;
			table.WriteRow((NoResString)"Arriving", destinationCustomsOfficeCode);

			MakePrettyForInterpretationExtra(table);
			return string.Format((NoResString)"<h3>{0}</h3> {1}", messageDescription, table.ToHtml());
		}

		protected void MakePrettyForInterpretationExtra(HtmlTableCreator table)
		{
			if (messageFunction is NctsMessageFunctionSet.DeclarationCancellationRequestMessage declarationCancellationRequest)
			{
				table.WriteRow((NoResString)"Comment On Cancellation", declarationCancellationRequest.CommentOnCancellation);
			}
			if (messageFunction is NctsMessageFunctionSet.DeclarationAmendmentMessage declarationAmendment)
			{
				var reason = declarationAmendment.UserReasonForCancellation;
				var comment = declarationAmendment.CommentOnCancellation;
				if (reason != "")
				{
					table.WriteRow((NoResString)"Amend Reason", reason);
				}
				if (comment != "")
				{
					table.WriteRow((NoResString)"Amend Comment", comment);
				}
			}
		}

		public virtual void PutReferenceNumberIntoMessageFromPlaceholder(EDIMessage message, ZString messageText, NctsHeader nctsHeader)
		{
			message.EM_MessageText = message.EM_MessageText.Replace(ownerReferencePlaceHolder, nctsHeader.BH_JobReference);
			message.EM_MessageText = message.EM_MessageText.Replace(LocalReferenceNumberPlaceholder, nctsHeader.LocalReferenceNumber);
			message.EM_MessageText = message.EM_MessageText.Replace(XML_LRN_PLACEHOLDER, nctsHeader.LocalReferenceNumber);
			message.EM_MessageText = message.EM_MessageText.Replace(XML_MESSAGEID_PLACEHOLDER, message.EM_MessageNum);
			message.EM_MessageText = message.EM_MessageText.Replace(XML_INTERCHANGEID_PLACEHOLDER, message.EM_MessageNum); // TODO This needs to be the EI_InterchangeNum
		}

		#endregion

		public static string GetBizoPkHexadecimalOnly(EDIMessage bizO)
		{
			return bizO.PK.ToString().Replace("-", "").ToUpper();
		}

		public static string PutBizoPkIntoSysCarPlaceholder(string input, EDIMessage bizO)
		{
			var sysCar = GetBizoPkHexadecimalOnly(bizO);
			input = input.Replace(SysCarPlaceHolder, sysCar);
			return input.Replace(XML_SYSCAR_PLACEHOLDER, sysCar);
		}

		public virtual void AfterFullSuccess(IBuilderResult builderResult)
		{
			var nctsHeader = builderResult.Owner as NctsHeader;
			if (nctsHeader != null)
			{
				builderResult.Message.EM_MessageText = PutBizoPkIntoSysCarPlaceholder(builderResult.Message.EM_MessageText, builderResult.Message);
				nctsHeader.Messages.Add(builderResult.Message);
				nctsHeader.EffectiveMessageStatus = GetMessageStatusAfterSent(GetMessageCodeFromMessage(builderResult.Message));
			}
		}

		NctsMessageFunctionSetsProvider MessageFunctionSetsProvider => messageFunctionSetsProvider ?? (messageFunctionSetsProvider = GetMessageFunctionSetsProvider());
		NctsMessageFunctionSetsProvider messageFunctionSetsProvider;

		protected virtual NctsMessageFunctionSetsProvider GetMessageFunctionSetsProvider() => new NctsMessageFunctionSetsProvider();

		protected virtual ZString GetMessageCodeFromMessage(EDIMessage message) => message.EM_MessageSubType;

		protected string GetMessageStatusAfterSent(string messageCode)
		{
			return GetMessageStatusAfterSentCore(messageCode);
		}

		protected virtual string GetMessageStatusAfterSentCore(string messageCode)
		{
			return MessageFunctionSetsProvider.GetMessageFunction(messageCode)?.SentMessageStatusCode ?? NctsMessageStatusList.Codes.Ok;
		}

		public const string SysCarPlaceHolder = "<<SYSCAR>>";
		public const string LocalReferenceNumberPlaceholder = "<<LRNPLACEHOLDER>>";
		public const string ownerReferencePlaceHolder = "<<BOX7>>";
		public const string XML_INTERCHANGEID_PLACEHOLDER = "{{XML_INTERCHANGEID_PLACEHOLDER}}";
		public const string XML_LRN_PLACEHOLDER = "{{XML_LRN_PLACEHOLDER}}";
		public const string XML_MESSAGEID_PLACEHOLDER = "{{XML_MESSAGEID_PLACEHOLDER}}";
		public const string XML_SYSCAR_PLACEHOLDER = "{{XML_SYSCAR_PLACEHOLDER}}";
		public NctsMessageFunctionSet messageFunction;
		NctsHeader nctsHeader;
	}
}
