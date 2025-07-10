using System;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.IN;

namespace Enterprise.Customs.IN.Business;

public class EDIMessage : Messaging.Business.EDIMessage, IINMessage
{
	public EDIMessage(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{ }

	public new EDIMessageLookups Lookups => (EDIMessageLookups)base.Lookups;

	protected override Messaging.Business.EDIMessageLookups GetNewLookups() => new EDIMessageLookups(this);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		EM_ApplicationCode = ApplicationCodeList.Codes.INCustoms;
		EM_IsTestMessage = !EnvProxy.Instance.IsProductionSystem;
	}

	protected override string GetMessageReferenceNumber() => new MessageReferenceNumberGenerator(Factory).Generate(EM_MessageType, EM_MessageSubType, EM_MessageOwner, Company?.PK ?? ZGuid.Empty, ZDate.Today);

	protected override string MessageNumberPlaceHolderOverride => Constants.Messaging.INMessageNumPlaceHolder;

	protected override CodeDescriptionPairList MessageSubTypeList => Factory.GetCachedValue<EDIMessageSubTypeList>();

	protected override ZBool ShouldUseNTextOrEvenBetterUseMessageDataAsItsCompressedOverride() => ZBool.True;

	protected override IStreamFormatter MessageStreamFormatter => null;

	[BusinessObjectTestExclude]
	public override ZString EM_MessageInterpretation => fEM_MessageInterpretation ??= MessageInterpreterFactory.GetINMessageInterpreter(this)?.GetMessageInterpretation();
	string fEM_MessageInterpretation;

	public override void OnSaving()
	{
		base.OnSaving();

		if (IsTransmitMessage && EM_StatusInfo.HasChanges && EM_LinkedObject is IMessageAttachee messageAttachee)
		{
			ZString status = EM_Status.ToString() switch
			{
				EDIMessageStatusList.Codes.Failed => MessageStatusList.Codes.MessageDeliveryFailed,
				EDIMessageStatusList.Codes.ProcessedOK => MessageStatusList.Codes.MessageSent,
				_ => string.Empty
			};

			if (!status.IsEmpty)
			{
				messageAttachee.MessageStatus = status;
			}
		}
	}

	#region Loader

	public new class Loader : BusinessObject.Loader
	{
		public Loader(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public bool MessageHasBeenSentOn(BusinessObject businessObject, string messageType, string[] messageSubTypes)
		{
			Argument.NotNull(businessObject, nameof(businessObject));

			var messageQuery = new ZQuery(EDIMessageSchema.EM_LinkUniqueID, businessObject.PK)
				.AddToFilter(EDIMessageSchema.EM_LinkTable, businessObject.TableName)
				.AddToFilter(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.INCustoms)
				.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);

			if (messageType != null)
			{
				messageQuery.AddToFilter(EDIMessageSchema.EM_MessageType, messageType);
			}
			if (messageSubTypes != null && messageSubTypes.Length > 0)
			{
				messageQuery.AddToFilter(EDIMessageSchema.EM_MessageSubType, messageSubTypes);
			}

			return Factory.ExistsInDatabase(EDIMessage.Schema.TableName, messageQuery);
		}

		protected override Type GetTypeOfBusinessObjectToLoad()
		{
			return typeof(EDIMessage);
		}
	}

	#endregion
}
