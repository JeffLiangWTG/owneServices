using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.CH.MessageContracts.Edec.ECom;
using CargoWise.Customs.CH.MessageDefinitions.Edec.ECom.Version1_0;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.CH.Business;

public class EdecComplaintRequestDataProvider : IEdecComplaintRequest
{
	public static EdecComplaintRequestDataProvider New(EComplaintMessageSendingObject sendingObject) => sendingObject == null ? null : new EdecComplaintRequestDataProvider(sendingObject);

	public EdecComplaintRequestDataProvider(EComplaintMessageSendingObject sendingObject)
	{
		this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
	}

	readonly EComplaintMessageSendingObject sendingObject;

	CusEntryHeader EntryHeader => sendingObject.EntryHeader;

	public string RequestorTraderIdentificationNumber => GlbCompany.CurrentCompany?.GC_CustomsRegistrationNo;

	public string RequestorCorrelationID => EDIMessage.MessageNumberPlaceHolder;

	public DateTime? RequestDateTime => ZDateTime.Now.ToDateTime();

	public string Item => CHGlbStaffWrapper.Get(GlbStaff.CurrentUser)?.CHDPassword?.GP_UserID;

	public string ItemElementName => nameof(ItemChoiceType.declarantNumber);

	public string CustomsDeclarationNumber
	{
		get
		{
			var declarationNumber = EntryHeader.EntryNumber;
			var dotPosition = declarationNumber.IndexOf('.');
			if (dotPosition >= 0)
			{
				declarationNumber = declarationNumber.Left(dotPosition);
			}
			return declarationNumber;
		}
	}

	public string CorrectionReason => sendingObject.CorrectionReason;

	public bool AttachedDeclaration => sendingObject.IsAttachedDeclaration;

	public string AppealText => null;

	public string PaperCorrespondence => null;

	public IEnumerable<IEdecComplaintRequestLine> Complaints => complaints ?? (complaints = EdecComplaintRequestLineDataProvider.NewCollection(sendingObject).ToArray());
	IEnumerable<IEdecComplaintRequestLine> complaints;
}
