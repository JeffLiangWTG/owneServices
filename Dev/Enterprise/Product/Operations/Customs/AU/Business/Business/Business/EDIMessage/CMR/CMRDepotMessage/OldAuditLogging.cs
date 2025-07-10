using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	// This will be reconsidered and refactored in a future workitem.
	// All this code has simply been pulled out of deprecated classes.
	public class CMRSeaDepotOldAuditLogging
	{
		public CMRSeaDepotOldAuditLogging()
		{
		}

		#region Constants

		const int PremiseIDLength = 5;

		#endregion

		public void AuditLogUnderbondResponse(CusUnderbond underbond, CMRUBMREQRMessage message)
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

			if (linkedObject != null)
			{
				ZString destinationPremiseID = underbond.C4_DestinationPremiseID.PadRight(PremiseIDLength);
				ZString originPremiseID = underbond.C4_OriginPremiseID.PadRight(PremiseIDLength);
				ZString status = message.GetStatusCode();
				switch (status)
				{
					case CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived:
						StmALog requestLog = linkedObject.GetLogs().AddNew(Events.UnderbondRequest, destinationPremiseID + CMRUnderbondStatuses.Descriptions.ExpectedCargoArrivalAdviceReceived);
						break;

					case CMRUnderbondStatuses.Codes.UnderbondApprovalAdviceReceived:
						StmALog approvalLog = linkedObject.GetLogs().AddNew(Events.UnderbondCustomsApproval, originPremiseID + CMRUnderbondStatuses.Descriptions.UnderbondApprovalAdviceReceived);
						break;

					case CMRUnderbondStatuses.Codes.UnderbondApprovalRescindAdviceReceived:
						StmALog requestRescindLog = linkedObject.GetLogs().AddNew(Events.UnderbondCancel, originPremiseID + CMRUnderbondStatuses.Descriptions.UnderbondApprovalRescindAdviceReceived);
						CancelMostRecentUnderbondApproval(linkedObject, underbond.C4_OriginPremiseID);
						break;

					case CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived:
						StmALog approvalRescindLog = linkedObject.GetLogs().AddNew(Events.UnderbondCancel, destinationPremiseID + CMRUnderbondStatuses.Descriptions.ExpectedCargoArrivalRescindAdviceReceived);
						CancelMostRecentExpectedCargoArrivalAdvice(linkedObject, underbond.C4_DestinationPremiseID);
						break;
				}
			}

			if (linkedWrapper != null)
			{
				linkedObject = linkedWrapper.WrappedBusinessObject;
			}

			if (linkedObject != null)
			{
				ZString status = message.GetStatusCode();
				switch (status)
				{
					case CMRUnderbondStatuses.Codes.ExpectedCargoArrivalAdviceReceived:
						StmALog requestLog = linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.ImpendingCargo + " " + underbond.C4_DestinationPremiseID);
						break;

					case CMRUnderbondStatuses.Codes.ExpectedCargoArrivalRescindAdviceReceived:
						StmALog impendingCancellationEvent = linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.ImpendingCargoCancelled + " " + underbond.C4_DestinationPremiseID);
						break;
				}
			}
		}

		public void AuditLogCargoStatusResponse(BusinessObject parent, CMRCARSTMessage message)
		{
			BusinessObject linkedObject = parent;
			BusinessObjectWrapper linkedWrapper = parent as BusinessObjectWrapper;
			if (linkedWrapper != null)
			{
				linkedObject = linkedWrapper.WrappedBusinessObject;
			}

			if (linkedObject != null)
			{
				linkedObject.GetLogs().AddNew(Events.StatusUpdated, message.GetStatusDescription().SubstringSafe(0, StmALogSchema.SL_Reference.MaxLength));
			}

			if (linkedWrapper != null)
			{
				linkedObject = linkedWrapper.WrappedBusinessObject;
			}

			if (linkedObject != null)
			{
				ZString statusCode = ReleaseStatus(message);
				AddConditionalServicesAndDocuments(parent, message);

				switch (statusCode)
				{
					case CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased:
						var scdLog = linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceClear + " " + message.PremiseID);
						if (linkedObject is CusSCAPivot scaPivot)
						{
							scaPivot.CV_ClearanceDate = scdLog.SL_EventTime;
						}
						break;
					case CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation:
						linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceConditionalClear + " " + message.PremiseID);
						break;
					case CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms:
						linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceACSSEIZED + " " + message.PremiseID);
						break;
					case CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine:
						linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceAQISSEIZED + " " + message.PremiseID);
						break;
					case CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement:
						linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceClearHRM + " " + message.PremiseID);
						break;
					case CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl:
						linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceHELD + " " + message.PremiseID);
						break;
					case CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed:
						linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceSUBUBMOV + " " + message.PremiseID);
						break;
					case CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus:
						linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceTRANSHIP + " " + message.PremiseID);
						break;
					case CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement:
						linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceTRANSHPHRM + " " + message.PremiseID);
						break;
					case CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction:
						linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceTRANSHPHRM + " " + message.PremiseID);
						break;
					default:
						linkedObject.GetLogs().AddNew(Events.SeaCargoDepotEvent, DepotEvents.CargoStatusAdviceDetailed + " " + message.PremiseID);
						break;
				}
			}
		}

		#region Cancel Most Recent

		void CancelMostRecentUnderbondApproval(BusinessObject parent, ZString premiseID)
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

		void CancelMostRecentExpectedCargoArrivalAdvice(BusinessObject parent, ZString premiseID)
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

		#region Release Status

		ZString ReleaseStatus(CMRCARSTMessage message)
		{
			ZString result = ZString.Empty;
			CUSRESMessage cUSRES = message.GetAutoEdifactMessageUsingNamedFactory(AuEdifactMessageFactory.AUCMessageFactory, new Edifact.UNOCCMRCharacterSet()) as CUSRESMessage;
			if (cUSRES != null)
			{
				foreach (FTXSegment fTX in cUSRES.FTX)
				{
					if (fTX.TextSubjectCodeQualifier == TextSubjectCodeQualifierList.StatusDetails)
					{
						result = GetCargoStatusCodeFromDescription(fTX.TextLiteral.FreeTextValue2, message.Factory.GetCachedValue<CMRConsolidatedCargoStatuses>());
						break;
					}
				}
			}
			return result;
		}

		ZString GetCargoStatusCodeFromDescription(ZString statusDescription, CMRConsolidatedCargoStatuses list)
		{
			foreach (CodeDescriptionPair pair in list)
			{
				if (pair.Description.StartsWith(statusDescription))
				{
					return pair.Code;
				}
			}
			return ZString.Empty;
		}

		#endregion

		#region AddConditionalServicesAndDocuments

		void PropogateServiceToHouseBill(CFSContainer container, ZString serviceCode, CMRCARSTMessage message)
		{
			if (message.HouseBillNumber.IsEmpty)
			{
				if (container.PackLines.Count != 0)
				{
					foreach (CFSPackLine packLine in container.PackLines)
					{
						if (packLine.Shipment != null)
						{
							packLine.Shipment.DocsAndCartage.Services.AddIfNotExists(serviceCode);
						}
					}
				}
				else
				{
					if (container.Consol.Containers.Count == 1 || container.Consol.Shipments.Count == 1)
					{
						foreach (CFSShipment shipment in container.Consol.Shipments)
						{
							shipment.DocsAndCartage.Services.AddIfNotExists(serviceCode);
						}
					}
				}
			}
			else
			{
				foreach (CFSShipment shipmentFromHouseBill in container.Consol.Shipments.Find(new ZQuery(JobShipmentSchema.JS_HouseBill, message.HouseBillNumber)))
				{
					shipmentFromHouseBill.DocsAndCartage.Services.AddIfNotExists(serviceCode);
				}
			}
		}

		void PropogateDocumentToHouseBill(CFSContainer container, ZString documentCode, CMRCARSTMessage message)
		{
			if (message.HouseBillNumber.IsEmpty)
			{
				if (container.PackLines.Count != 0)
				{
					foreach (CFSPackLine packLine in container.PackLines)
					{
						if (packLine.Shipment != null)
						{
							packLine.Shipment.DocsAndCartage.RequiredDocuments.AddIfNotExists(documentCode, JobRequiredDocument.DocUsage.Both);
						}
					}
				}
				else
				{
					if (container.Consol.Containers.Count == 1 || container.Consol.Shipments.Count == 1)
					{
						foreach (CFSShipment shipment in container.Consol.Shipments)
						{
							shipment.DocsAndCartage.RequiredDocuments.AddIfNotExists(documentCode, JobRequiredDocument.DocUsage.Both);
						}
					}
				}
			}
			else
			{
				foreach (CFSShipment shipmentFromHouseBill in container.Consol.Shipments.Find(new ZQuery(JobShipmentSchema.JS_HouseBill, message.HouseBillNumber)))
				{
					shipmentFromHouseBill.DocsAndCartage.RequiredDocuments.AddIfNotExists(documentCode, JobRequiredDocument.DocUsage.Both);
				}
			}
		}

		void AddConditionalServicesAndDocuments(BusinessObject bizO, CMRCARSTMessage message)
		{
			CFSContainerWrapper container = bizO as CFSContainerWrapper;
			if (container != null)
			{
				foreach (ZString impedimentDetail in message.ACSAQISImpedimentDetails)
				{
					ZString serviceCode = GetServiceCodeFromImpedimentDescription(impedimentDetail);
					if (!serviceCode.IsEmpty)
					{
						container.Container.Services.AddIfNotExists(serviceCode);
						PropogateServiceToHouseBill(container.Container, serviceCode, message);
					}
					ZString documentCode = GetDocumentCodeFromImpedimentDescription(impedimentDetail);
					if (!documentCode.IsEmpty)
					{
						PropogateDocumentToHouseBill(container.Container, documentCode, message);
					}
				}
			}

			CFSShipmentWrapper shipment = bizO as CFSShipmentWrapper;
			if (shipment != null)
			{
				foreach (ZString impedimentDetail in message.ACSAQISImpedimentDetails)
				{
					ZString serviceCode = GetServiceCodeFromImpedimentDescription(impedimentDetail);
					if (!serviceCode.IsEmpty)
					{
						shipment.Shipment.DocsAndCartage.Services.AddIfNotExists(serviceCode);
					}
					ZString documentCode = GetDocumentCodeFromImpedimentDescription(impedimentDetail);
					if (!documentCode.IsEmpty)
					{
						shipment.Shipment.DocsAndCartage.RequiredDocuments.AddIfNotExists(documentCode, JobRequiredDocument.DocUsage.Both);
					}
				}
			}
		}

		ZString GetServiceCodeFromImpedimentDescription(ZString impedimentDetail)
		{
			if (impedimentDetail.ToUpper().Contains("FUMIGATION"))
			{
				return Core.Constants.FreightServiceType.Codes.Fumigation;
			}

			if (impedimentDetail.ToUpper().Contains("PENDING AQIS ACTION (QUARANTINE)"))
			{
				return Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			}

			if (impedimentDetail.ToUpper().Contains("INSPECTION - FRESH PRODUCE INSPECT"))
			{
				return Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			}

			if (impedimentDetail.ToUpper().Contains("PENDING AQIS ACTION (FOOD)"))
			{
				return Core.Constants.FreightServiceType.Codes.QuarantineInspection;
			}

			if (impedimentDetail.ToUpper().Contains("INSPECTION - TAILGATE"))
			{
				return Core.Constants.FreightServiceType.Codes.Tailgate;
			}

			if (impedimentDetail.ToUpper().Contains("INSPECTION - INSPECT (UNPACK)"))
			{
				return Core.Constants.FreightServiceType.Codes.QuarantineUnpack;
			}

			if (impedimentDetail.ToUpper().Contains("OTHER TREATMENTS - CLEANING AS DIRECTED"))
			{
				return Core.Constants.FreightServiceType.Codes.Cleaning;
			}

			if (impedimentDetail.ToUpper().Contains("OTHER TREATMENTS - STEAM CLEANING"))
			{
				return Core.Constants.FreightServiceType.Codes.SteamCleaning;
			}

			if (impedimentDetail.ToUpper().Contains("INSPECTION - RE-INSPECTION OF GOODS"))
			{
				return Core.Constants.FreightServiceType.Codes.ExtraInspection;
			}

			return ZString.Empty;
		}

		ZString GetDocumentCodeFromImpedimentDescription(ZString impedimentDetail)
		{
			if (impedimentDetail.ToUpper().Contains("PENDING AQIS ACTION (FOOD)"))
			{
				return Enterprise.Core.Constants.RefDocTypes.FoodControlCertificate;
			}

			if (impedimentDetail.ToUpper().Contains("INSPECTION - FRESH PRODUCE INSPECT"))
			{
				return Enterprise.Core.Constants.RefDocTypes.FoodControlCertificate;
			}

			if (impedimentDetail.ToUpper().Contains("FUMIGATION"))
			{
				return Enterprise.Core.Constants.RefDocTypes.FumigationCertificate;
			}

			if (impedimentDetail.ToUpper().Contains("INSPECTION - INSPECT (UNPACK)"))
			{
				return Enterprise.Core.Constants.RefDocTypes.QuarantinePackingDeclaration;
			}

			return ZString.Empty;
		}

		#endregion
	}
}
