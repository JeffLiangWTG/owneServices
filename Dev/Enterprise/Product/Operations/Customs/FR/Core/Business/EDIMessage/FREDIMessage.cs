using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.FR.Business.EdiMessages
{
	public class FREDIMessage : EDIMessage, Integration.Customs.FR.IEDIMessage
	{
		public FREDIMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = ApplicationCodes.FRCustomsMessage;
		}

		public MessageDataObject MessageDataObject => messageDataObject ?? (messageDataObject = GenerateMessageDataObject());
		MessageDataObject messageDataObject;

		[BusinessObjectTestExclude]
		public override ZString EM_MessageInterpretation
		{
			get => (!MessageDataObject?.CanSetMessageInterpretation ?? true) && !EM_MessageText.IsEmpty && MessageDataObject?.Prettier != null ? MessageDataObject?.Prettier.GetMessageInterpretation() ?? ZString.Empty : base.EM_MessageInterpretation;
			set
			{
				if (!(MessageDataObject?.CanSetMessageInterpretation ?? true) && !EM_MessageText.IsEmpty)
				{
					throw new NotSupportedException("Setting EM_MessageInterpretation is not supported.");
				}

				base.EM_MessageInterpretation = value;
			}
		}

		protected virtual MessageDataObject GenerateMessageDataObject()
		{
			return null;
		}

		[ThreadSafe]
		public new static readonly FREDIMessageTypeDecider TypeDecider = new FREDIMessageTypeDecider();

		public override void OnSaving()
		{
			base.OnSaving();
			if (EM_LinkedObject is ICorrelationIDProvider correlationIdProvider)
			{
				EM_MessageText = EM_MessageText.Replace(Messaging.MessageBuilders.MessageBuilderBase<object>.CorrelationidPlaceholder, correlationIdProvider.CorrelationID);
			}
		}

		protected override void PopulateMessageNumber()
		{
			base.PopulateMessageNumber();
			if (UsesPlaceHolders)
			{
				EM_MessageText = EM_MessageText.Replace(MessageNumberPlaceHolderHtml, EM_MessageNum);
			}
		}

		protected override IEnumerable<Type> GetAdditionalRegisteredLinkedObjectTypes()
		{
			yield return typeof(CusExitDetail);
		}

		protected override EDIMessageValidation GetNewValidation() => new FREDIMessageValidation(this);

		public new FREDIMessageValidation Validation => (FREDIMessageValidation)base.Validation;
	}
}
