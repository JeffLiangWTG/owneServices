using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.IN.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IN.Manifest.Business;

public abstract class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader, ISupportMultipleResourceStringData, IMessageAttachee, IJobNumber
{
	protected AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.India;

	#region ISupportMultipleResourceStringData

	public const string CaptionKeyAir = Core.Constants.TransportModes.Air;

	public const string CaptionKeySea = Core.Constants.TransportModes.Sea;

	public IReadOnlyList<string> MultipleKeysToUse => new string[] { AMA_TransportMode };

	#endregion

	#region IMessageAttachee

	IBusinessObjectCollection IMessageAttachee.Messages => Messages;

	string IJobNumber.JobNumber => AMA_JobReference;

	ZGuid IMessageAttachee.BranchPK => AMA_GB;

	ZString IMessageAttachee.MessageOwner => AMA_CustomsOffice;

	ZString IMessageAttachee.MessageStatus { get => AMA_MessageStatus; set => AMA_MessageStatus = value; }

	ZString IMessageAttachee.CustomsStatus { get => RegistrationStatus; set => RegistrationStatus = value; }

	void IMessageAttachee.RollbackChangesOnStatus() => RollbackChangesOnStatus();

	ZString IMessageAttachee.CalculateStatusAfterSending(ZString messageType) => CalculateStatusAfterSending(messageType);

	protected virtual ZString CalculateStatusAfterSending(ZString messageType) => ZString.Empty;

	#endregion

	protected void RollbackChangesOnStatus()
	{
		if (RegistrationEntryNumber is CusEntryNumber entryNumber)
		{
			RegistrationStatus = entryNumber.IsInDatabase ? (ZString)entryNumber.CE_EntryStatusInfo.OriginalValue : ZString.Empty;
		}
		AMA_MessageStatus = IsInDatabase ? (ZString)AMA_MessageStatusInfo.OriginalValue : ZString.Empty;
	}
}
