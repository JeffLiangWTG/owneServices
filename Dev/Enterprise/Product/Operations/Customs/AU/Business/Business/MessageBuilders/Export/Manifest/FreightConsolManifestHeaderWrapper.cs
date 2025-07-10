using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class FreightConsolManifestHeaderWrapper : IManifestHeaderWrapper
	{
		public FreightConsolManifestHeaderWrapper(ForwardingConsol consol)
		{
			Consol = consol;
		}

		public ZDateTime DateOfDeparture
		{
			get
			{
				return WrappedConsol.GetActualOrEstimatedDateOfDeparture();
			}
		}

		public ZString FlightNumber
		{
			get
			{
				return Consol.JK_JX_JV_VoyageFlight.KeepChars("1234567890");
			}
		}

		public bool IsAir
		{
			get
			{
				return Consol.IsAir;
			}
		}

		public bool IsSea
		{
			get
			{
				return Consol.IsSea;
			}
		}

		public ZString PortOfLoading
		{
			get
			{
				return Consol.LoadPort == null ? ZString.Empty : Consol.LoadPort.Code;
			}
		}

		public ZString CountryOfDischarge
		{
			get
			{
				return Consol.DischargePort == null ? ZString.Empty : Consol.DischargePort.RL_RN_NKCountryCode;
			}
		}

		public ZString VesselID
		{
			get
			{
				return Consol.Vessel == null ? ZString.Empty : Consol.Vessel.RV_LloydsNumber;
			}
		}

		public ZString VoyageNumber
		{
			get
			{
				return Consol.JK_JX_JV_VoyageFlight;
			}
		}

		public ZString AirlineCode
		{
			get
			{
				return Consol.JK_JX_JV_VoyageFlight.ToUpper().KeepChars("ABCDEFGHIJKLMNOPQRSTUVWXYZ");
			}
		}

		public ZString CAN
		{
			get
			{
				return WrappedConsol.CAN;
			}
		}

		public ZString CCAN
		{
			get
			{
				return WrappedConsol.ContingencyCAN;
			}
		}

		public int TotalPackageCount
		{
			get
			{
				int result = 0;
				foreach (CommonShipment shipment in Consol.Shipments)
				{
					result += shipment.JS_OuterPacks;
				}
				return result;
			}
		}

		public int TotalContainerCount
		{
			get
			{
				Dictionary<string, bool> allContainers = new Dictionary<string, bool>();
				foreach (CommonShipment shipment in Consol.Shipments)
				{
					foreach (CommonContainer container in shipment.Containers)
					{
						allContainers[container.JC_ContainerNum] = true;
					}
				}
				return allContainers.Count;
			}
		}

		public int TotalEmptyContainerCount
		{
			get
			{
				int result = 0;
				foreach (CommonShipment shipment in Consol.Shipments)
				{
					result += shipment.NumberOfEmptyContainers;
				}
				return result;
			}
		}

		public ZString AirWaybillNumber
		{
			get
			{
				return Consol.JK_MasterBillNum;
			}
		}

		public IManifestLineWrapper[] Lines => lines ?? (lines = GetLines());
		IManifestLineWrapper[] lines;

		IManifestLineWrapper[] GetLines()
		{
			var result = new List<IManifestLineWrapper>();
			var cCANWriteOff = false;

			if (CAN.IsEmpty && !CCAN.IsEmpty)
			{
				result.Add(new FreightConsolManifestLineWrapper(this));
				cCANWriteOff = true;
			}

			WrappedConsol.RemovePreliminaryLineNumbers();
			lineNumber = (cCANWriteOff ? 1 : 0);
			var sortedShipments = Consol.Shipments.Cast<CommonShipment>().OrderBy(s => s.JS_UniqueConsignRef);

			var requiresChangeMessage = !WrappedConsol.HasSameShipments;
			nextLineNumber = requiresChangeMessage ? WrappedConsol.LastLineNoUsed : ZInt.Zero;

			foreach (var shipment in sortedShipments)
			{
				if (shipment.IsHighVolumeLowValueLegacy || shipment.IsHighVolumeLowValue)
				{
					var manifestLines = ((ForwardingShipment)shipment).GetHVLVConsignmentLines();
					ProcessHVLVConsignmentLines(manifestLines, shipment, requiresChangeMessage, result);
				}
				else
				{
					ProcessShipmentLines(shipment, requiresChangeMessage, result);
				}
			}

			if (requiresChangeMessage)
			{
				ProcessRemovedLines(result);
			}

			return result.OrderBy(x => x.LineNumber).ToArray();
		}

		void ProcessRemovedLines(List<IManifestLineWrapper> result)
		{
			// include any lines that have been deleted
			var deletedLines = WrappedConsol.GetRemovedConsignmentsLineNo();
			foreach (var consignmentRefLineNo in deletedLines)
			{
				var lineWrapper = FreightShipmentManifestLineWrapper.NewWhenShipmentIsDeleted();
				lineWrapper.LineNumber = consignmentRefLineNo;
				result.Add(lineWrapper);
			}

			SetConsignmentReferencesToBeDeleted();
		}

		void ProcessShipmentLines(CommonShipment shipment, bool requiresChangeMessage, List<IManifestLineWrapper> result)
		{
			var lineWrapper = new FreightShipmentManifestLineWrapper(shipment, null, WrappedConsol);

			if (ShouldProcessShipmentLines(shipment, lineWrapper))
			{
				var shipmentWrapper = new FreightShipmentWrapper(shipment, Consol);
				if (requiresChangeMessage)  // get line number from line number used for this shipment in previous message - must be identical
				{
					lineWrapper.LineNumber = shipmentWrapper.ESMLineNumber;
					if (lineWrapper.LineNumber == 0)
					{
						nextLineNumber++;
						lineWrapper.LineNumber = nextLineNumber;
					}
				}
				else
				{
					lineNumber++;
					lineWrapper.LineNumber = lineNumber;
				}

				result.Add(lineWrapper);
				if (shipmentWrapper.ESMLineNumber == 0 && lineWrapper.LineNumber > 0)
				{
					shipmentWrapper.CreatePreliminaryManifestLineNumber(lineWrapper.LineNumber);
				}
			}
		}

		bool ShouldProcessShipmentLines(CommonShipment shipment, FreightShipmentManifestLineWrapper lineWrapper)
		{
			var hasCoLoadMasterWithCAN = shipment.CoLoadMasterShipment != null
				&& !shipment.CoLoadMasterShipment.IsHighVolumeLowValueMaster
				&& new FreightShipmentManifestLineWrapper(shipment.CoLoadMasterShipment, null, WrappedConsol).HasCANOrExemption;
			return !hasCoLoadMasterWithCAN
				&& !shipment.IsHighVolumeLowValueMaster
				&& (!shipment.IsCoLoadMaster && !shipment.IsBlindCoLoadMaster || lineWrapper.HasCANOrExemption);
		}

		void ProcessHVLVConsignmentLines(IEnumerable<IEManifestLine> consignmentLines, CommonShipment shipment, bool requiresChangeMessage, List<IManifestLineWrapper> result)
		{
			if (ShouldProcessShipmentLines(shipment, new FreightShipmentManifestLineWrapper(shipment, null, WrappedConsol)))
			{
				if (requiresChangeMessage)  // get line number from line number used for this shipment in previous message - must be identical
				{
					foreach (var consignmentLine in consignmentLines)
					{
						var shipmentWrapper = new FreightShipmentWrapper(shipment, Consol, consignmentLine);
						var hvlvLineWrapper = new FreightShipmentManifestLineWrapper(shipment, consignmentLine, WrappedConsol);
						hvlvLineWrapper.LineNumber = hvlvLineWrapper.ESMLineNumber;
						if (hvlvLineWrapper.LineNumber == 0)
						{
							nextLineNumber++;
							hvlvLineWrapper.LineNumber = nextLineNumber;
							shipmentWrapper.CreatePreliminaryManifestLineNumber(hvlvLineWrapper.LineNumber);
						}

						result.Add(hvlvLineWrapper);
					}
				}
				else
				{
					foreach (var consignmentLine in consignmentLines)
					{
						var shipmentWrapper = new FreightShipmentWrapper(shipment, Consol, consignmentLine);
						var hvlvLineWrapper = new FreightShipmentManifestLineWrapper(shipment, consignmentLine, WrappedConsol);
						hvlvLineWrapper.LineNumber = hvlvLineWrapper.ESMLineNumber;
						if (hvlvLineWrapper.LineNumber == 0)
						{
							lineNumber++;
							hvlvLineWrapper.LineNumber = lineNumber;
							shipmentWrapper.CreatePreliminaryManifestLineNumber(hvlvLineWrapper.LineNumber);
						}

						result.Add(hvlvLineWrapper);
					}
				}
			}
		}

		void SetConsignmentReferencesToBeDeleted()
		{
			foreach (KeyValuePair<ZInt, ZGuid> consignmentToDelete in WrappedConsol.RemovedConsignmentsLineNo)
			{
				var shipmentToDelete = Consol.Factory.Load<CommonShipment>(consignmentToDelete.Value);
				if (shipmentToDelete != null)
				{
					var manifestSequence = new FreightShipmentWrapper(shipmentToDelete, Consol);
					manifestSequence.UpdateManifestedLineNumberToPreliminaryDeletedLine();
				}
			}
		}

		#region Implementation

		protected readonly ForwardingConsol Consol;
		protected readonly bool ProcessingCusRes;
		protected int lineNumber;
		protected int nextLineNumber;

		public ZString DepotPremiseID
		{
			get
			{
				if (manifestStatus == null)
				{
					manifestStatus = new ESMManifestStatus(Consol);
				}
				return manifestStatus.PremisesID;
			}
		}
		ESMManifestStatus manifestStatus;

		FreightConsolWrapper WrappedConsol
		{
			get
			{
				if (wrappedConsol == null)
				{
					wrappedConsol = new FreightConsolWrapper(Consol);
				}

				return wrappedConsol;
			}
		}
		FreightConsolWrapper wrappedConsol;

		#endregion
	}
}
