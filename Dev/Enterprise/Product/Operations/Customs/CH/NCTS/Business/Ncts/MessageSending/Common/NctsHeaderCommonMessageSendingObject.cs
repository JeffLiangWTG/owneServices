using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using GlbCompanyWrapper = Enterprise.Customs.CH.Business.GlbCompanyWrapper;

namespace Enterprise.Customs.CH.NCTS.Business;

public class NctsHeaderCommonMessageSendingObject : NctsHeaderMessageSendingObject, IMessageSendingObjectParent, IMessageSendingObject, INctsMessageSendingObject
{
	public new class Schema : AutoNctsHeaderMessageSendingObject.Schema
	{
		public const string ShouldSend = nameof(NctsHeaderDepartureMessageSendingObject.ShouldSend);
		public const int MrnMaxLength = 35;
		public const int MsgTypeMaxLength = 5;
	}

	public NctsHeaderCommonMessageSendingObject(NctsHeader nctsHeader) : base(nctsHeader)
	{
	}

	public new NctsHeader NctsHeader => (NctsHeader)base.NctsHeader;

	public ZString MessageIdentification { get; } = Guid.NewGuid().ToString();

	protected override bool ShouldSend_ReadOnly => ZBool.True;

	protected override void SetDefaultDataCore()
	{
		base.SetDefaultDataCore();
		ShouldSend = ZBool.True;
	}

	public new NctsHeaderCommonMessageSendingObjectLookups Lookups => (NctsHeaderCommonMessageSendingObjectLookups)base.Lookups;

	protected override NctsHeaderMessageSendingObjectLookups GetNewLookups() => new NctsHeaderCommonMessageSendingObjectLookups(this);

	protected override NctsHeaderMessageSendingObjectValidation GetNewValidation() => new NctsHeaderCommonMessageSendingObjectValidation(this);

	[List(nameof(Lookups) + "." + nameof(NctsHeaderCommonMessageSendingObjectLookups.MessageTypeList))]
	[MaxLength(Schema.MsgTypeMaxLength)]
	public override ZString MessageType { get => base.MessageType; set => base.MessageType = value; }

	#region IMessageSendingObject

	public ZString ApplicationCode => EDIMessage.ApplicationCodes.CHCustomsPassar;

	public ZString GetApplicationReference() => MessageIdentification;

	public ZString MessageTypeForEDIMessage => MessageTypeCodeList.Codes.PassarNcts;

	public ZString MessageSubTypeForEDIMessage => GetMessageSubTypeForEDIMessage();

	protected virtual ZString GetMessageSubTypeForEDIMessage() => null;

	public ZString ToMessageString() => MessageBuilderFactory.NewMessageBuilder(this).GenerateXmlMessage().GetSerializedString();

	public ZGuid GetCredentialPK() => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany)?.TokenCredentials?.PK ?? ZGuid.Empty;

	#endregion

	#region IMessageSendingObjectParent

	public ZString CanSendMessage() => ZString.Empty;

	public IEnumerable<IMessageSendingObject> SelectedSendingObjects => new[] { this };

	public void UpdateSendingObjectsBeforeSending() { }

	[ResourceStringData("NPBO:Enterprise.Customs.CH.NCTS.Business.NctsHeaderCommonMessageSendingObject|LRN", ShortCaption = "LRN", Caption = "Registration Number (LRN)")]
	public override ZString LRN => base.LRN;

	[MaxLength(Schema.MrnMaxLength)]
	public override ZString MRN => base.MRN;

	#endregion
}
