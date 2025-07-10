using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Customs.Shared.MessageContracts;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business;

public class EComplaintMessageSendingObject : AutoEComplaintMessageSendingObject, IMessageSendingObjectParent, IMessageSendingObject
{
	public EComplaintMessageSendingObject(CusEntryHeader entryHeader) : base(entryHeader.Factory)
	{
		using (GetValidationSuspender())
		using (SuspendSettingHasChanges())
		{
			EntryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		}
	}

	public CusEntryHeader EntryHeader { get; }

	[List(nameof(Lookups) + "." + nameof(EComplaintMessageSendingObjectLookups.CorrectionReasonList))]
	public override ZString CorrectionReason { get => base.CorrectionReason; set => base.CorrectionReason = value; }

	public EComplaintMessageSendingObjectLineCollection SendingObjectLines
	{
		get
		{
			if (sendingObjectLines == null)
			{
				sendingObjectLines = new EComplaintMessageSendingObjectLineCollection(EntryHeader);
				RegisterEditableChildObject(sendingObjectLines);
			}
			return sendingObjectLines;
		}
	}
	EComplaintMessageSendingObjectLineCollection sendingObjectLines;

	public EComplaintMessageSendingObjectLookups Lookups => lookups ?? (lookups = new EComplaintMessageSendingObjectLookups(this));
	EComplaintMessageSendingObjectLookups lookups;

	protected override ZString HumanReadableNameCore => Res.GetString("C20B3156-520D-4FE6-9F11-787DBDE3A3E1", "eCom Message Sending");

	#region IMessageSendingObjectParent

	public ZString CanSendMessage()
	{
		var result = EnvironmentHelper.CheckMessageSendingEnvironmentForEdec();
		if (result.IsEmpty && (CHGlbStaffWrapper.Get(GlbStaff.CurrentUser)?.CHDPassword?.GP_UserID.IsEmpty ?? true))
		{
			return Res.GetString("40AD53FA-1BAF-487C-88EF-C9FCED91CC45", "Declarant Number is not configured for the current user. Please contact your system administrator.");
		}
		return result;
	}

	public IEnumerable<IMessageSendingObject> SelectedSendingObjects => new[] { this };

	public void UpdateSendingObjectsBeforeSending() { }

	#endregion

	#region IMessageSendingObject

	public ZString ApplicationCode => EDIMessage.ApplicationCodes.CHCustomsEdec;

	public ZString GetApplicationReference() => ZString.Empty;

	public ZString MessageTypeForEDIMessage => MessageTypeCodeList.Codes.ECM;

	public ZString MessageSubTypeForEDIMessage => MessageSubTypeCodeList.Codes.Request;

	public ZString ToMessageString() => MessageBuilderFactory.NewMessageBuilder(this).GenerateXmlMessage().GetSerializedString();

	public ZGuid GetCredentialPK() => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany)?.GlbExternalPassword?.PK ?? ZGuid.Empty;

	#endregion
}
