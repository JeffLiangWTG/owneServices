using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class ExpectedArrivalCusUnderbondFactory : CusUnderbondFactory
	{
		#region Constants

		const int PremiseIDLength = 5;

		#endregion

		#region Process Incoming UBMREQR

		public CusUnderbond ProcessIncomingUBMREQR(CMRUBMREQRMessage message)
		{
			CusUnderbond result = null;
			if (IsInterestedInUBMREQR(message))
			{
				int lineCount = ((CUSRESMessage)message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new Edifact.UNOCCMRCharacterSet())).Group6.Count;
				for (int i = 0; i < lineCount; i++)
				{
					CusUnderbond underbond = CreateOrLoadUnderbond(message, i + 1)
						?? FindRelatedUnderbond(message, i + 1);

					if (underbond != null)
					{
						CMRUBMREQRMessage linkedMessage = (CMRUBMREQRMessage)message.LinkOrCloneMessage(underbond);
						AuditLogUnderbondResponse(underbond, linkedMessage);
						if (result == null)
						{
							result = underbond;
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region Logging

		protected virtual void AuditLogUnderbondResponse(CusUnderbond underbond, CMRUBMREQRMessage message)
		{
			BusinessObject linkedObject = null;
			BusinessObjectWrapper linkedWrapper = underbond.LinkedObject as BusinessObjectWrapper;
			if (linkedWrapper != null)
			{
				linkedObject = linkedWrapper.WrappedBusinessObject;
			}

			if (linkedObject == null)
			{
				linkedObject = underbond.LinkedObject as BusinessObject;
			}

			var logParent = linkedObject as IStmALogParent;

			if (logParent != null)
			{
				ZString destinationPremiseID = underbond.C4_DestinationPremiseID.PadRight(PremiseIDLength);
				ZString originPremiseID = underbond.C4_OriginPremiseID.PadRight(PremiseIDLength);
				ZString status = message.GetStatusCode();
				switch (status)
				{
					case CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived:
						logParent.Logs.AddNew(Events.UnderbondAcquitReceivalReported, destinationPremiseID + CMRUnderbondStatuses.Descriptions.ExpectedCargoArrivalAdviceReceived);
						break;

					case CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived:
						logParent.Logs.AddNew(Events.UnderbondCustomsApproval, originPremiseID + CMRUnderbondStatuses.Descriptions.UnderbondApprovalAdviceReceived);
						break;

					case CMRUnderbondStatuses.Codes.UnderbondApprovalRescindAdviceReceived:
						logParent.Logs.AddNew(Events.UnderbondCancel, originPremiseID + CMRUnderbondStatuses.Descriptions.UnderbondApprovalRescindAdviceReceived);
						CancelMostRecentUnderbondApproval(linkedObject, underbond.C4_OriginPremiseID);
						break;

					case CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived:
						logParent.Logs.AddNew(Events.UnderbondCancel, destinationPremiseID + CMRUnderbondStatuses.Descriptions.ExpectedCargoArrivalRescindAdviceReceived);
						CancelMostRecentExpectedCargoArrivalAddvice(linkedObject, underbond.C4_DestinationPremiseID);
						break;
				}
			}
		}

		#endregion

		#region Cancel Most Recent

		protected void CancelMostRecentUnderbondApproval(BusinessObject parent, ZString premiseID)
		{
			ZQuery underbondApprovalFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.UnderbondCustomsApproval.Code);
			underbondApprovalFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, premiseID);
			underbondApprovalFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, CMRUnderbondStatuses.Descriptions.UnderbondApprovalAdviceReceived);
			StmALog[] foundLogs = parent.GetLogs().Find(underbondApprovalFilter);
			if (foundLogs.Length > 0)
			{
				StmALog underbondApproval = foundLogs[0];
				underbondApproval.Cancel();
			}
		}

		protected void CancelMostRecentExpectedCargoArrivalAddvice(BusinessObject parent, ZString premiseID)
		{
			ZQuery expectedCargoArrivalFilter = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.UnderbondRequest.Code);
			expectedCargoArrivalFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.StartsWith, premiseID);
			expectedCargoArrivalFilter.AddToFilter(StmALogSchema.SL_Reference, SQLComparisonOperator.Contains, CMRUnderbondStatuses.Descriptions.ExpectedCargoArrivalAdviceReceived);
			StmALog[] foundLogs = parent.GetLogs().Find(expectedCargoArrivalFilter);
			if (foundLogs.Length > 0)
			{
				StmALog expectedCargoArrivalEvent = foundLogs[0];
				expectedCargoArrivalEvent.Cancel();
			}
		}

		#endregion

		#region Is Interested

		protected abstract bool IsInterestedInUBMREQR(CMRUBMREQRMessage message);

		#endregion

		#region Underbond Creation and Loading

		protected CusUnderbond FindRelatedUnderbond(CMRUBMREQRMessage message, int lineNumber)
		{
			CusUnderbond result = null;
			var messageParent = LoadOrCreateUnderbondParent(message, lineNumber);
			if (messageParent != null && messageParent.GetType() == typeof(CusUnderbond))
			{
				result = (CusUnderbond)messageParent;
			}

			return result;
		}

		protected internal CusUnderbond CreateOrLoadUnderbondInternal(CMRUBMREQRMessage message, int lineNumber) => CreateOrLoadUnderbond(message, lineNumber);
		protected virtual CusUnderbond CreateOrLoadUnderbond(CMRUBMREQRMessage message, int lineNumber)
		{
			ICusUnderbondDependentCollectionParent parent = LoadOrCreateUnderbondParent(message, lineNumber);
			return parent == null ? null : LoadOrCreateUnderbond(parent, message, lineNumber);
		}

		#endregion

		#region Implementation

		protected abstract ICusUnderbondDependentCollectionParent LoadOrCreateUnderbondParent(CMRUBMREQRMessage message, int lineNumber);

		protected void SetIfEmpty(ZPropertyInfo info, IZType value)
		{
			if (info.Value.IsEmpty)
			{
				info.Value = value;
			}
		}
		protected internal CusUnderbond LoadOrCreateUnderbondInternal(ICusUnderbondDependentCollectionParent parent, CMRUBMREQRMessage message, int lineNumber) => LoadOrCreateUnderbond(parent, message, lineNumber);
		protected virtual CusUnderbond LoadOrCreateUnderbond(ICusUnderbondDependentCollectionParent parent, CMRUBMREQRMessage message, int lineNumber)
		{
			return LoadOrCreateUnderbond(parent, message, lineNumber, null);
		}

		protected CusUnderbond LoadOrCreateUnderbond(ICusUnderbondDependentCollectionParent parent, CMRUBMREQRMessage message, int lineNumber, CusOutturnHeader header)
		{
			CusUnderbond underbond = null;

			if (parent.Underbonds != null)
			{
				if (message.IsAir)
				{
					underbond = (CusUnderbond)parent.Underbonds.FindUnderbond(message.OriginID, message.DestinationID);
					if (underbond == null)
					{
						underbond = (CusUnderbond)parent.Underbonds.AddNew();
					}
				}
				else
				{
					underbond = GetOrCreateUnderbond(message.Factory, message.OriginID, message.DestinationID,
						message.GetContainerNumber(lineNumber), message.GetHouseBillOfLading(lineNumber),
						message.GetOceanBillOfLading(lineNumber), message.IsExpectedArrival, header);
				}

				if (underbond != null)
				{
					if (!parent.Underbonds.Contains(underbond))
					{
						parent.Underbonds.Add(underbond);
					}
				}

				PopulateCusUnderbondInfo(underbond, message);
			}

			return underbond;
		}

		protected virtual void PopulateCusUnderbondInfo(CusUnderbond underbond, CMRUBMREQRMessage message)
		{
			underbond.C4_OriginPremiseID = message.OriginID;

			SetOrigin(underbond, message.OriginID);
			SetDestination(underbond, message.DestinationID);
			underbond.C4_MovementReason = message.RequestReason;
			underbond.C4_ModeOfMovement = message.ModeOfMovement;
			underbond.C4_UnderbondBySeaVoyage = message.UnderbondBySeaVoyage;
			ZString underbondBySeaVesselID = message.UnderbondBySeaVesselID;
			if (!underbondBySeaVesselID.IsEmpty)
			{
				var vessel = message.Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_LloydsNumber, underbondBySeaVesselID));
				if (vessel != null)
				{
					underbond.C4_UnderbondBySeaVessel = vessel.RV_Code;
				}
			}
		}

		void SetOrigin(CusUnderbond underbond, ZString originID)
		{
			OrgAddress address = new OrgAddress.Loader(underbond.Factory).LoadAddressFromLocalCode(OrgCusCode.CodeTypes.ControlledPremisesID, originID);
			if (address != null)
			{
				underbond.C4_OA_OriginAddress = address.PK;
			}
			else
			{
				underbond.C4_OriginPremiseID = originID;
			}
		}

		void SetDestination(CusUnderbond underbond, ZString destinationID)
		{
			OrgAddress address = new OrgAddress.Loader(underbond.Factory).LoadAddressFromLocalCode(OrgCusCode.CodeTypes.ControlledPremisesID, destinationID);
			if (address != null)
			{
				underbond.C4_OA_DestinationAddress = address.PK;
			}
			else
			{
				underbond.C4_DestinationPremiseID = destinationID;
			}
		}

		protected bool UnderbondForDepot(CMRUBMREQRMessage message)
		{
			bool result = false;

			var messageStatusCode = message.GetStatusCode();
			if (messageStatusCode == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived ||
				messageStatusCode == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived)
			{
				result = !message.DestinationID.IsEmpty && CompanyHasDepotWithPremiseID(GlbCompany.GetCurrentCompany(message.Factory), message.DestinationID);
			}
			else
			{
				result = !message.OriginID.IsEmpty && CompanyHasDepotWithPremiseID(GlbCompany.GetCurrentCompany(message.Factory), message.OriginID);
			}

			return result;
		}

		protected bool UnderbondRelatedToCTO(CMRUBMREQRMessage message)
		{
			bool result = false;

			var messageStatusCode = message.GetStatusCode();
			if (messageStatusCode == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived ||
				messageStatusCode == CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived)
			{
				result = CompanyHasCTOWithPremiseID(message.DestinationID);
			}
			else
			{
				result = CompanyHasCTOWithPremiseID(message.OriginID);
			}

			return result;
		}

		#endregion
	}
}
