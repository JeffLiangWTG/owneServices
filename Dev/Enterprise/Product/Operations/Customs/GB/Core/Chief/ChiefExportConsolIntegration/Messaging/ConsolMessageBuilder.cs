using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Edifact;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	public class ConsolMessageBuilder : IMessageBuilder
	{
		public ConsolMessageBuilder(CustomsExportConsolIntegrationWrapper consolWrapper, GbDes242MessageFunction how)
		{
			this.consolWrapper = consolWrapper;
			this.how = how;
			errorCollector = new ErrorCollector();
		}

		public IMessageBuilderResult PopulateMessages()
		{
			var result = new MessageBuilderResult();
			var builderResult = PopulateMessage();
			if (builderResult != null)
			{
				result.AddBuilderResult(builderResult);
			}
			return result;
		}

		IBuilderResult PopulateMessage()
		{
			var message = consolWrapper.ForwardingConsol.Messages.AddNew(GetEDIMessageBizOType());
			var applicationCode = GetApplicationCode();
			message.MessageNumberStrategy = new GbMessageNumberStrategy(consolWrapper.Factory, applicationCode);
			message.EM_SendWithMessageErrors = consolWrapper.MawbExportHelper.HasErrors || consolWrapper.MawbExportHelper.HasMessageErrors;
			message.EM_MessageText = GetMessageText();
			if (consolWrapper.MawbExportHelper.ME_Profile.IsEmpty || consolWrapper.MawbExportHelper.ME_ProfileInfo.HasMessageError(ListValidation.InvalidCodeMessageError.ToString()))
			{
				errorCollector.AddError("Profile", new ErrorInfo("", "Mandatory"));
			}
			else
			{
				message.EM_MessageOwner = consolWrapper.MawbExportHelper.ME_Profile.Left(EDIMessage.Schema.EM_MessageOwnerMaxLength);
			}
			message.EM_ApplicationCode = applicationCode;
			SetMessageType(message);
			SetMessageSubType(message);
			SetApplicationReference(message);
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_MessageInterpretation = GetInterpretation(message);

			var result = new BuilderResult(consolWrapper, errorCollector.GetErrors(), AfterFullSuccess)
			{
				Message = message
			};
			return result;
		}

		protected virtual Type GetEDIMessageBizOType()
		{
			return typeof(EDIMessage);
		}

		protected virtual ZString GetApplicationCode()
		{
			return ApplicationCodeList.Codes.GbCcsuk;
		}

		protected virtual void SetMessageType(EDIMessage message)
		{
			message.EM_MessageType = how.FunctionCode;
		}

		protected virtual void SetMessageSubType(EDIMessage message)
		{
			message.EM_MessageSubType = how.SubFunctionCode;
		}

		protected virtual void SetApplicationReference(EDIMessage message)
		{
		}

		protected virtual ZString GetInterpretation(EDIMessage message)
		{
			return new GbEdifactPrettier(message.EM_MessageText, CharSet, how).MakeHumanReadable();
		}

		protected UNCharacterSet CharSet => new UkCharSet();

		protected virtual ZString GetMessageText()
		{
			return new CusDec.CusDecCreatorUkcinvFromConsol(consolWrapper, how, errorCollector).Create(CharSet);
		}

		public virtual void AfterFullSuccess(IBuilderResult builderResult)
		{
			builderResult.Message.EM_MessageText = GbTransmissionMessageGenerator.PutBizoPkIntoSysCarPlaceholder(builderResult.Message.EM_MessageText, builderResult.Message);
		}

		protected readonly CustomsExportConsolIntegrationWrapper consolWrapper;
		protected readonly GbDes242MessageFunction how;
		protected readonly ErrorCollector errorCollector;
	}
}
