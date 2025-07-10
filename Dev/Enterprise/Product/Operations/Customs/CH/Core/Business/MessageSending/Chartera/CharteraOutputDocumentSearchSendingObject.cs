using System;
using System.Collections.Generic;
using CargoWise.Customs.CH.MessageContracts.Chartera.Outgoing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CH.Business;

public class CharteraOutputDocumentSearchSendingObject : AutoCharteraOutputDocumentSearchSendingObject, IMessageSendingObjectParent, IMessageSendingObject, IDocumentSearchRequest
{
	public CharteraOutputDocumentSearchSendingObject(BusinessObjectFactory factory) : base(factory)
	{
	}

	protected override ZString HumanReadableNameCore => Res.GetString("NPBO:Enterprise.Customs.CH.Business.CharteraOutputDocumentSearchSendingObject|HumanReadableName", "Chartera Output Documents Search");

	public GlbCompany Company => company ?? (company = GlbCompany.GetCurrentCompany(Factory));
	GlbCompany company;

	public ZString ProcessId { get; } = Guid.NewGuid().ToString();

	public override ZDateTime CreationTimeFrom
	{
		get => base.CreationTimeFrom;
		set
		{
			base.CreationTimeFrom = value;
			SetCreationTimeToDefault();
		}
	}

	public override ZDateTime CreationTimeTo
	{
		get => base.CreationTimeTo;
		set
		{
			base.CreationTimeTo = value;
			SetCreationTimeToDefault();
		}
	}

	void SetCreationTimeToDefault()
	{
		if (CreationTimeTo.IsEmpty && !CreationTimeFrom.IsEmpty)
		{
			CreationTimeTo = CreationTimeFrom.AddDays(1).AddSeconds(-1);
		}
	}

	public IEnumerable<IMessageSendingObject> SelectedSendingObjects => new[] { this };

	public ZString ApplicationCode => ApplicationCodeList.Codes.CHCustomsCharteraOutput;

	public ZString MessageTypeForEDIMessage => MessageTypeCodeList.Codes.REQ;

	public ZString MessageSubTypeForEDIMessage => MessageSubTypeCodeList.Codes.CharteraOutputDocumentSearchRequest;

	public ZString CanSendMessage() => EnvironmentHelper.CheckMessageSendingEnvironmentForPassarAndChartera();

	public ZString ToMessageString() => MessageBuilderFactory.NewMessageBuilder(this).GenerateXmlMessage().GetSerializedString();

	public ZString GetApplicationReference() => ProcessId;

	public void UpdateSendingObjectsBeforeSending() { }

	public ZGuid GetCredentialPK() => GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(Company)?.TokenCredentials?.PK ?? ZGuid.Empty;

	#region IDocumentSearchRequest

	string IDocumentSearchRequest.ProcessId => ProcessId;

	DateTime IDocumentSearchRequest.From => ToUtcDateTime(CreationTimeFrom);

	DateTime IDocumentSearchRequest.To => ToUtcDateTime(CreationTimeTo);

	static DateTime ToUtcDateTime(ZDateTime zDateTime)
	{
		var dateTime = zDateTime.ToDateTime();
		return dateTime.Kind == DateTimeKind.Utc ? dateTime : zDateTime.ToUniversalBranchTime().ToDateTime();
	}

	bool IDocumentSearchRequest.IsHistoricalQuery => IsHistoricalQuery;

	IReadOnlyCollection<string> IDocumentSearchRequest.DocumentTypes => null;

	#endregion
}
