using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Business.MessageBuilders;
using Enterprise.Messaging.Integration;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.IL.Business.Message.MessageBuilder
{
	public abstract class ILMessageBuilderBase : IMessageBuilder
	{
		public ILMessageBuilderBase(BusinessObject parent)
		{
			this.parent = parent;
		}

		IMessageBuilderResult IMessageBuilder.PopulateMessages()
		{
			var messageBuilderResult = new MessageBuilderResult();
			if (IsAwaitingResponse)
			{
				var errorMessage = Res.GetString("91FB770F-084F-41B1-89AD-8FD9A5A07453",
				"There are messages waiting for a response. You are unable to send a message until a valid response is received. If a response has been received, exit the job, then re-open to refresh the status");

				messageBuilderResult.AddBuilderResult(new BuilderResult(parent, new string[] { errorMessage }, null) { Message = GetEDIMessage() });
				return messageBuilderResult;
			}

			foreach (var builderResult in GenerateBuilderResults())
			{
				if (builderResult != null)
				{
					messageBuilderResult.AddBuilderResult(builderResult);
				}
			}

			return messageBuilderResult;
		}

		protected abstract IBusinessObjectCollection GetMessageOwnerCollection();

		protected abstract string GetMessageText();

		protected abstract ILEDIMessage GetMessage();

		protected virtual void AfterFullSuccess(IBuilderResult builderResult)
		{
		}

		protected virtual Func<IBuilderResult, bool> AfterPartialSuccess => null;

		protected virtual bool IsAwaitingResponse => false;

		protected virtual IEnumerable<BuilderResult> GenerateBuilderResults()
		{
			var ediMessage = GetEDIMessage();
			var digitalSignatureResolver = new MessageSignatureXtInfoResolver(ediMessage, GlbStaff.CurrentUser.GS_Code);
			var (messageError, externalPassword, messageNote) = digitalSignatureResolver.GetMessageAttributes();
			if (!messageError.IsNullOrEmpty())
			{
				yield return new BuilderResult(parent, new string[] { messageError }, null) { Message = ediMessage };
			}
			else
			{
				var builderResult = CreateBuilderResult();
				ediMessage.EM_GP = externalPassword?.PK ?? ZGuid.Empty;
				ediMessage.EM_MessageText = GetMessageText();
				if (!messageNote.IsNullOrEmpty())
				{
					var note = ediMessage.Notes.AddNew();
					note.ST_NoteText = messageNote;
				}
				builderResult.Message = ediMessage;
				yield return builderResult;
			}
		}

		protected BuilderResult CreateBuilderResult() => new BuilderResult(parent, Array.Empty<string>(), AfterFullSuccess);

		protected EDIMessage GetEDIMessage()
		{
			var message = GetMessage();
			message.EM_Status = EDIMessageStatusList.Codes.Queued;
			message.EM_ReceiveTransmit = ILEDIInterchange.Direction.Transmit;

			if (parent != null)
			{
				message.EM_LinkedObject = parent;
			}

			var messageOwnerCollection = GetMessageOwnerCollection();
			messageOwnerCollection?.Add(message);

			var company = message.Factory.Load<GlbCompany>(message.EM_GC);
			message.EM_MessageOwner = GetMessageOwner(company);
			message.EM_ApplicationReference = ZString.Empty;

			return message;
		}

		ZString GetMessageOwner(GlbCompany company)
			=> company.GC_CustomsRegistrationNo.Left(EDIMessage.Schema.EM_MessageOwnerMaxLength);

		readonly BusinessObject parent;
	}

	public abstract class ILMessageBuilderBase<T> : ILMessageBuilderBase
	{
		protected ILMessageBuilderBase(BusinessObject parent) : base(parent)
		{
		}

		protected abstract IEnumerable<T> GetMessageGenerationInputs();

		protected abstract string GetMessageText(T input);

		protected override IEnumerable<BuilderResult> GenerateBuilderResults()
		{
			foreach (var input in GetMessageGenerationInputs())
			{
				var builderResult = CreateBuilderResult();
				var message = GetEDIMessage();
				message.EM_MessageText = input != null ? GetMessageText(input) : GetMessageText();
				builderResult.Message = message;
				yield return builderResult;
			}
		}

		protected override string GetMessageText() => string.Empty;
	}
}
