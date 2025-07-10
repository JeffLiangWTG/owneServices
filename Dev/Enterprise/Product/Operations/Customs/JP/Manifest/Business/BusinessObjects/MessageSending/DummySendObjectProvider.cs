using CargoWise.EntityFramework;
using static Enterprise.Customs.JP.Common.JPMessageActionList;

namespace Enterprise.Customs.JP.Manifest.Business;

public sealed class DummySendObjectProvider
{
	public DummySendObjectProvider(ManifestMessageSendingObjectParent sendingObjectParent)
	{
		this.sendingObjectParent = sendingObjectParent;
		this.header = sendingObjectParent.header;
		factory = new ReadOnlyBusinessObjectFactory() { RefreshEnabled = false };
	}

	readonly BusinessObjectFactory factory;
	readonly ManifestMessageSendingObjectParent sendingObjectParent;
	readonly AsycudaManifestHeader header;

	public ManifestMessageSendingObject DummySendingObjectForHCH01End
	{
		get
		{
			if (dummySendingObjectForHCH01End == null)
			{
				var dummyBill = factory.New<AsycudaBill>();
				dummyBill.ABL_AMA = header.PK;
				dummyBill.ABL_BillNumber = AsycudaBill.HCH01EndHAWB;
				dummySendingObjectForHCH01End = new ManifestMessageSendingObject(dummyBill);
			}

			return dummySendingObjectForHCH01End;
		}
	}

	ManifestMessageSendingObject dummySendingObjectForHCH01End;

	public ManifestMessageSendingObject DummySendingObjectForNVC01BondedLocationAmendment
	{
		get
		{
			if (!sendingObjectParent.IsSendingNVC01BondedLocationAmendment)
			{
				return null;
			}

			if (dummySendingObjectForNVC01BondedLocationAmendment == null)
			{
				var dummyBill = factory.New<AsycudaBill>();
				dummyBill.ABL_AMA = header.PK;
				dummySendingObjectForNVC01BondedLocationAmendment = new ManifestMessageSendingObject(dummyBill, sendingObjectParent);
				dummySendingObjectForNVC01BondedLocationAmendment.Action = NVC01MessageActionList.Codes.Five;
				dummySendingObjectForNVC01BondedLocationAmendment.ShouldSend = true;
				sendingObjectParent.SendingObjectsCollection.Add(dummySendingObjectForNVC01BondedLocationAmendment);
			}

			return dummySendingObjectForNVC01BondedLocationAmendment;
		}
	}

	ManifestMessageSendingObject dummySendingObjectForNVC01BondedLocationAmendment;
}
