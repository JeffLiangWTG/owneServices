using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class VoyageManifestExpectedArrivalCusUnderbondFactory : ExpectedArrivalCusUnderbondFactory
	{
		protected internal ICusUnderbondDependentCollectionParent LoadOrCreateUnderbondParentInternal(CMRUBMREQRMessage message, int lineNumber) => LoadOrCreateUnderbondParent(message, lineNumber);
		protected override ICusUnderbondDependentCollectionParent LoadOrCreateUnderbondParent(CMRUBMREQRMessage message, int lineNumber)
		{
			ICusUnderbondDependentCollectionParent result = null;

			result = FindOceanBillDetailByUBMSendersReference(message, lineNumber);
			if (result == null)
			{
				result = FindOceanBillDetail(message, lineNumber);
			}

			return result;
		}

		CusSeaManOBLDetail FindOceanBillDetail(CMRUBMREQRMessage message, int lineNumber)
		{
			ZString lloydsNumber = message.LloydsNumber;
			var vessel = message.Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, lloydsNumber));

			if (vessel != null)
			{
				ZString voyageNumber = message.VoyageNumber;
				ZString containerNumber = message.GetContainerNumber(lineNumber);
				ZString containerMode = message.GetContainerMode(lineNumber);

				ZQuery voyageFilter = new ZQuery();
				voyageFilter.AddToFilter(CusSeaManTranHeadSchema.BT_VesselName, vessel.RV_Code);
				voyageFilter.AddToFilter(CusSeaManTranHeadSchema.BT_VoyageNum, voyageNumber);

				foreach (CusSeaManTranHead tranHead in message.Factory.Load(typeof(CusSeaManTranHead), voyageFilter))
				{
					foreach (CusSeaManOBLHeader oceanBill in tranHead.OceanBills)
					{
						foreach (CusSeaManOBLDetail container in oceanBill.Details)
						{
							if (container.BD_ContainerNumber == containerNumber && container.BD_LineCargoType == containerMode)
							{
								return container;
							}
						}
					}
				}
			}

			return null;
		}

		CusSeaManOBLDetail FindOceanBillDetailByUBMSendersReference(CMRUBMREQRMessage message, int lineNumber)
		{
			if (!message.UBMSendersReference.IsEmpty)
			{
				ZQuery filter = new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, message.UBMSendersReference);
				filter.AddToFilter(CusUnderbondSchema.C4_ParentTableCode, CusSeaManOBLDetailSchema.Constants.Prefix);
				CusUnderbond[] possibleUnderbonds = (CusUnderbond[])message.Factory.Load(typeof(CusUnderbond), filter);

				foreach (CusUnderbond underbond in possibleUnderbonds)
				{
					CusSeaManOBLDetail parent = underbond.LinkedObject as CusSeaManOBLDetail;
					if (parent != null &&
						((parent.BD_ContainerNumber == message.GetContainerNumber(lineNumber))
						|| ((parent.IsBreakBulk || parent.IsBulk) && parent.Header != null && parent.Header.BO_OceanBill == message.GetOceanBillOfLading(lineNumber))))
					{
						return parent;
					}
				}
			}

			return null;
		}

		protected override CusUnderbond LoadOrCreateUnderbond(ICusUnderbondDependentCollectionParent parent, CMRUBMREQRMessage message, int lineNumber)
		{
			CusUnderbond result = null;

			ZQuery filter = new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, message.UBMSendersReference);
			CusUnderbond[] possibleUnderbonds = (CusUnderbond[])message.Factory.Load(typeof(CusUnderbond), filter);
			foreach (CusUnderbond possibleUnderbond in possibleUnderbonds)
			{
				if (!message.UBMSendersReference.IsEmpty && possibleUnderbond.C4_SendersMessageReference == message.UBMSendersReference)
				{
					result = possibleUnderbond;
				}
			}
			if (result == null)
			{
				result = (CusUnderbond)parent.Underbonds.FindUnderbond(message.OriginID, message.DestinationID);
				if (result == null)
				{
					result = (CusUnderbond)parent.Underbonds.AddNew();
					SetUnderbondDetails(message, result);
				}
			}

			return result;
		}

		void SetUnderbondDetails(CMRUBMREQRMessage message, CusUnderbond underbond)
		{
			underbond.C4_MovementReason = message.RequestReason;
			underbond.C4_ModeOfMovement = message.ModeOfMovement;
			underbond.C4_OriginPremiseID = message.OriginID;
			underbond.C4_DestinationPremiseID = message.DestinationID;
		}

		protected internal bool IsInterestedInUBMREQRInternal(CMRUBMREQRMessage message) => IsInterestedInUBMREQR(message);
		protected override bool IsInterestedInUBMREQR(CMRUBMREQRMessage message)
		{
			return message.IsSea
				&& message.GetStatusCode() != CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived
				&& message.GetStatusCode() != CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived
				&& (UnderbondRelatedToCTO(message) || SeaCTORecordExists(message));
		}

		bool SeaCTORecordExists(CMRUBMREQRMessage message)
		{
			bool result = false;

			if (!message.UBMSendersReference.IsEmpty)
			{
				ZQuery filter = new ZQuery(CusUnderbondSchema.C4_SendersMessageReference, message.UBMSendersReference);
				filter.AddToFilter(CusUnderbondSchema.C4_ParentTableCode, CusSeaManOBLDetailSchema.Constants.Prefix);
				result = message.Factory.LoadTop1<CusUnderbond>(filter) != null;
			}

			if (!result && !message.GetOceanBillOfLading(1).IsEmpty)
			{
				ZQuery filter = new ZQuery(CusSeaManOBLHeaderSchema.BO_OceanBill, message.GetOceanBillOfLading(1));
				result = message.Factory.LoadTop1<CusSeaManOBLHeader>(filter) != null;
			}

			if (!result)
			{
				if (!message.GetContainerNumber(1).IsEmpty)
				{
					ZQuery filter = new ZQuery(CusSeaManOBLDetailSchema.BD_ContainerNumber, message.GetContainerNumber(1));
					BusinessObject[] matches = message.Factory.Load(typeof(CusSeaManOBLDetail), filter);
					foreach (CusSeaManOBLDetail detail in matches)
					{
						if (detail.Header != null && detail.Header.TransportHeader != null
							&& detail.Header.TransportHeader.BT_LloydsIMO == message.LloydsNumber
							&& detail.Header.TransportHeader.BT_VoyageNum == message.VoyageNumber)
						{
							result = true;
							break;
						}
					}
				}
			}

			return result;
		}
	}
}
