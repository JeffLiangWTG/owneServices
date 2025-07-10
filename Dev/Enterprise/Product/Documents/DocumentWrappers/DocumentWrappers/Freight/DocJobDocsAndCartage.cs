using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;

namespace Enterprise.DocumentWrappers
{
	public class DocJobDocsAndCartage : DocumentWrapper
	{
		DocJobDocsAndCartage(JobDocsAndCartage docsAndCartage, BusinessObjectFactory factoryToWrap)
			: base(docsAndCartage, factoryToWrap)
		{
		}

		public static DocJobDocsAndCartage New(JobDocsAndCartage docsAndCartage, BusinessObjectFactory factoryToWrap)
		{
			if (docsAndCartage == null || docsAndCartage.IsDeleted)
			{
				return null;
			}
			else
			{
				return new DocJobDocsAndCartage(docsAndCartage, factoryToWrap);
			}
		}

		JobDocsAndCartage DocsAndCartage
		{
			get { return (JobDocsAndCartage)WrappedObject; }
		}

		public override string ToString()
		{
			return ZString.Empty;
		}

		#region ZDateTime Fields

		public ZDateTime FumigationCompletionEventTime
		{
			get { return DocsAndCartage.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.Fumigation); }
		}

		public ZDateTime QuarantineInspectionCompletionEventTime
		{
			get { return DocsAndCartage.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.QuarantineInspection); }
		}

		public ZDateTime CustomsHoldCompletionEventTime
		{
			get { return DocsAndCartage.Services.ServiceCompletionDate(Core.Constants.FreightServiceType.Codes.CustomsHold); }
		}

		public ZDateTime DeliveryCartageAdvised
		{
			get { return DocsAndCartage.JP_DeliveryCartageAdvised; }
		}

		public ZDateTime DeliveryCartageCompleted
		{
			get { return DocsAndCartage.JP_DeliveryCartageCompleted; }
		}

		public ZDateTime EstimatedDelivery
		{
			get { return DocsAndCartage.JP_EstimatedDelivery; }
		}

		public ZDateTime EstimatedPickup
		{
			get { return DocsAndCartage.JP_EstimatedPickup; }
		}

		public ZDateTime PickupCartageAdvised
		{
			get { return DocsAndCartage.JP_PickupCartageAdvised; }
		}

		public ZDateTime PickupCartageCompleted
		{
			get { return DocsAndCartage.JP_PickupCartageCompleted; }
		}

		public ZDateTime PickupRequiredBy
		{
			get { return DocsAndCartage.JP_PickupRequiredBy; }
		}

		public ZDateTime DeliveryRequiredBy
		{
			get { return DocsAndCartage.JP_DeliveryRequiredBy; }
		}

		public ZDateTime FCLAvailable
		{
			get { return DocsAndCartage.JP_FCLAvailable; }
		}

		public ZDateTime LCLAvailable
		{
			get { return DocsAndCartage.JP_LCLAvailable; }
		}

		public ZDateTime FCLStorageCommences
		{
			get { return DocsAndCartage.JP_FCLStorageCommences; }
		}

		public ZDateTime LCLStorageCommences
		{
			get { return DocsAndCartage.JP_LCLStorageCommences; }
		}

		#endregion

		#region ZBool Fields

		public ZBool IsFumigationCompleted
		{
			get { return DocsAndCartage.Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.Fumigation); }
		}

		public ZBool IsQuarantineCompleted
		{
			get { return DocsAndCartage.Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.QuarantineInspection); }
		}

		public ZBool IsCustomsHoldCompleted
		{
			get { return DocsAndCartage.Services.IsServiceCompleted(Core.Constants.FreightServiceType.Codes.CustomsHold); }
		}

		public ZBool HasProhibitedPackaging
		{
			get { return DocsAndCartage.JP_HasProhibitedPackaging; }
		}

		public ZBool InsuranceRequired
		{
			get { return DocsAndCartage.JP_InsuranceRequired; }
		}

		#endregion

		#region ZString Fields

		public ZString OrderItemsAsString
		{
			get { return DocsAndCartage.JP_OrderItemsAsString; }
		}

		public ZString StorageTimeUnits
		{
			get { return DocsAndCartage.JP_StorageTimeUnits; }
		}

		public ZString FCLDeliveryEquipmentNeeded
		{
			get { return DocsAndCartage.JP_FCLDeliveryEquipmentNeeded; }
		}

		public ZString FCLPickupEquipmentNeeded
		{
			get { return DocsAndCartage.JP_FCLPickupEquipmentNeeded; }
		}

		#endregion

		#region ZDecimal Fields

		public ZDecimal DeliveryLabourCharge
		{
			get { return DocsAndCartage.JP_DeliveryLabourCharge; }
		}

		public ZDateTime DeliveryLabourTime
		{
			get { return DocsAndCartage.JP_DeliveryLabourTime; }
		}

		public ZDecimal DemurrageOnDeliveryCharge
		{
			get { return DocsAndCartage.JP_DeliveryTruckWaitCharge; }
		}

		public ZDecimal DemurrageOnPickupCharge
		{
			get { return DocsAndCartage.JP_PickupTruckWaitCharge; }
		}

		public ZDecimal LCLAirStorageCharge
		{
			get { return DocsAndCartage.JP_LCLAirStorageCharge; }
		}

		public ZDecimal PickupLabourCharge
		{
			get { return DocsAndCartage.JP_PickupLabourCharge; }
		}

		public ZDateTime PickupLabourTime
		{
			get { return DocsAndCartage.JP_PickupLabourTime; }
		}

		#endregion

		#region ZByte Fields

		public ZDateTime DemurrageOnDeliveryTime
		{
			get { return DocsAndCartage.JP_DeliveryTruckWaitTime; }
		}

		public ZDateTime DemurrageOnPickupTime
		{
			get { return DocsAndCartage.JP_PickupTruckWaitTime; }
		}

		public ZByte LCLAirStorageDaysOrHours
		{
			get { return DocsAndCartage.JP_LCLAirStorageDaysOrHours; }
		}

		#endregion

		#region Wrapper Fields

		#region Delivery Address

		public DocDocAddress DeliveryAddress
		{
			get
			{
				DocDocAddress result = null;
				if (DocsAndCartage.Parent.CartageImporterDocAddress.IsValidAddress)
				{
					result = DocDocAddress.New(DocsAndCartage.Parent.CartageImporterDocAddress, Factory);
				}

				return result;
			}
		}

		#endregion

		#region Pickup Address

		public DocDocAddress PickupAddress
		{
			get
			{
				DocDocAddress result = null;
				if (DocsAndCartage.Parent.CartageExporterDocAddress.IsValidAddress)
				{
					result = DocDocAddress.New(DocsAndCartage.Parent.CartageExporterDocAddress, Factory);
				}

				return result;
			}
		}

		#endregion

		public DocOrganisation DeliveryCartageCo
		{
			get { return DocOrganisation.New(DocsAndCartage.DeliveryCartageCo, Factory); }
		}

		public DocOrganisation PickupCartageCo
		{
			get { return DocOrganisation.New(DocsAndCartage.PickupCartageCo, Factory); }
		}

		public DocDocAddress ConsignorDocumentaryAddress
		{
			get { return DocDocAddress.New(DocsAndCartage.Parent.ConsignorDocumentaryAddress, Factory); }
		}

		public DocDocAddress ConsigneeDocumentaryAddress
		{
			get { return DocDocAddress.New(DocsAndCartage.Parent.ConsigneeDocumentaryAddress, Factory); }
		}

		public DocJobRequiredDocumentCollection JobDocumentsRequired
		{
			get { return new DocJobRequiredDocumentCollection(DocsAndCartage.RequiredDocuments, Factory); }
		}

		public DocJobRequiredDocument OriginalBill
		{
			get { return JobDocumentsRequired.OriginalBill; }
		}

		public DocJobRequiredDocument OriginalOceanBill
		{
			get { return JobDocumentsRequired.OriginalOceanBill; }
		}

		#endregion
	}
}
