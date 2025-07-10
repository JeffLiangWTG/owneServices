using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.CH.Business;

public class EvvRequestSendingObjectParent : AutoEvvRequestSendingObjectParent, IMessageSendingObjectParent
{
	public EvvRequestSendingObjectParent(CusEntryHeader entryHeader) : base(entryHeader.Factory)
	{
		this.entryHeader = Argument.NotNull(entryHeader, nameof(entryHeader));
		SetDefaults();
	}
	readonly CusEntryHeader entryHeader;

	void SetDefaults()
	{
		var mrn = entryHeader.MovementReferenceNumber;
		Mrn = CusEntryNumberHelper.MovementReferenceNumberWithoutVersion(mrn);
		MrnVersion = CusEntryNumberHelper.MovementReferenceNumberVersion(mrn) ?? ZInt.Zero;
		SendCustomsDuties = true;
		SendVat = true;
	}

	[ReadOnly(true)]
	public override ZString Mrn { get => base.Mrn; set => base.Mrn = value; }

	[MaxLength(2)]
	public override ZInt MrnVersion { get => base.MrnVersion; set => base.MrnVersion = value; }

	public ZString CanSendMessage()
	{
		return EnvironmentHelper.CheckMessageSendingEnvironmentForEdec();
	}

	public void UpdateSendingObjectsBeforeSending() { }

	public IEnumerable<IMessageSendingObject> SelectedSendingObjects
	{
		get
		{
			var messages = new Collection<EvvRequestSendingObject>();

			if (SendCustomsDuties)
			{
				messages.Add(new EvvRequestSendingObject(entryHeader, Mrn, MrnVersion, MessageSubTypeCodeList.Codes.TaxationDecisionCustomsDuties));
			}
			if (SendVat)
			{
				messages.Add(new EvvRequestSendingObject(entryHeader, Mrn, MrnVersion, MessageSubTypeCodeList.Codes.TaxationDecisionVat));
			}
			if (SendReimbursementCustomsDuties)
			{
				messages.Add(new EvvRequestSendingObject(entryHeader, Mrn, MrnVersion, MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementCustomsDuties));
			}
			if (SendReimbursementVat)
			{
				messages.Add(new EvvRequestSendingObject(entryHeader, Mrn, MrnVersion, MessageSubTypeCodeList.Codes.TaxationDecisionReimbursementVat));
			}

			return messages.ToArray();
		}
	}
}
