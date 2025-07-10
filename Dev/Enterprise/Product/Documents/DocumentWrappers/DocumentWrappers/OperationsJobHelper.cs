using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportCommon.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs.US.ISF;

namespace Enterprise.DocumentWrappers
{
	public class OperationsJobHelper
	{
		public OperationsJobHelper(DocJobHeader jobHeader, DocShipment shipment, DocJobInvoicingJobCharge charge, BusinessObjectFactory factory)
		{
			this.factory = factory;
			this.jobHeader = jobHeader;
			this.shipment = shipment;
			this.charge = charge;
		}

		readonly BusinessObjectFactory factory;
		readonly DocJobHeader jobHeader;
		readonly DocJobInvoicingJobCharge charge;
		readonly DocShipment shipment;

		public FreightWrapper OperationsJob
		{
			get
			{
				var jobHeaderParentID = jobHeader != null ? jobHeader.ParentID : ZGuid.Empty;
				if (IsLoadListJob)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<CFSLoadListConsol>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsCFSShipmentJob)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<CFSShipment>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsAgencyShipment)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<AgencyShipment>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsForwardingJob)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<ForwardingShipment>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsLocalCartage)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<CommonCartage>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsCustomJob)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<BaseJobDeclaration>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsISFJob)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New((BusinessObject)factory.Load<ICusISFHeader>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsConsignmentJob)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<DtbBookingConsignment>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsLandConsignmentJob)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<DtbConsignment>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsSundryCharges)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<SundryCharges>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsVoyageAccounting)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<VoyageAccount>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsContainerDetention)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<ContainerDetention>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsWarehouseJob) // Leave at bottom, more expensive than loading other types of jobs
				{
					if (warehouseJob != null)
					{
						return factory.GetCachedValue(warehouseJob.PK.ToStringKey(), delegate
						{
							return FreightWrapper.New(warehouseJob, factory)[0];
						});
					}
					else if (warehouseInvoice != null)
					{
						return factory.GetCachedValue(warehouseInvoice.PK.ToStringKey(), delegate
						{
							return FreightWrapper.New(warehouseInvoice, factory)[0];
						});
					}
					else
					{
						Argument.NotNull(warehouseJob, "If IsWarehouseJob is true, warehouse job should not be null.");
						return null;
					}
				}
				else if (IsContainerRegistrationJob)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<CFSContainer>(jobHeaderParentID), factory)[0];
					});
				}
				else if (IsNCTSJob)
				{
					return factory.GetCachedValue(jobHeaderParentID.ToStringKey(), delegate
					{
						return FreightWrapper.New(factory.Load<NctsHeader>(jobHeaderParentID), factory)[0];
					});
				}
				else
				{
					return null;
				}
			}
		}

		public ZBool IsCFSShipment
		{
			get
			{
				if (!isCFSShipment.HasValue)
				{
					isCFSShipment = shipment != null && shipment.IsCFSRegistered && !shipment.IsForwardRegistered;
				}

				return isCFSShipment.Value;
			}
		}
		ZBool? isCFSShipment;

		public ZBool IsAgencyShipment
		{
			get
			{
				if (!isAgencyShipment.HasValue)
				{
					isAgencyShipment = shipment != null && shipment.IsAgencyShipping;
				}

				return isAgencyShipment.Value;
			}
		}
		ZBool? isAgencyShipment;

		public ZBool IsConsignmentJob
		{
			get { return jobHeader != null && jobHeader.ParentTableCode == DtbBookingSchema.Constants.Prefix && factory.Load<IDtbTransport>(jobHeader.ParentID) is DtbBookingConsignment; }
		}

		public ZBool IsLandConsignmentJob
		{
			get { return jobHeader != null && jobHeader.ParentTableCode == DtbConsignmentSchema.Constants.Prefix && factory.Load<DtbConsignment>(jobHeader.ParentID) is DtbConsignment; }
		}

		public ZBool IsISFJob
		{
			get
			{
				return (ZBool)(jobHeader != null && jobHeader.ParentTableCode == CusISFHeaderSchema.Constants.Prefix);
			}
		}

		public ZBool IsForwardingJob
		{
			get
			{
				return (ZBool)(jobHeader != null && jobHeader.ParentTableCode == JobShipmentSchema.Constants.Prefix) && !IsCFSShipment && !IsAgencyShipment;
			}
		}

		public ZBool IsCustomJob
		{
			get
			{
				return (ZBool)(jobHeader != null && jobHeader.ParentTableCode == JobDeclarationSchema.Constants.Prefix);
			}
		}

		public ZBool IsNCTSJob
		{
			get
			{
				var cusInBondHeader = jobHeader != null ? factory.Load<BaseCusInBondHeader>(jobHeader.ParentID) : null;
				return new ZString[] { Enterprise.Customs.Common.CusInBondApplicationCodeList.Codes.NCTS4, Enterprise.Customs.Common.CusInBondApplicationCodeList.Codes.NCTS5 }.Contains(cusInBondHeader?.BH_ApplicationCode ?? ZString.Empty);
			}
		}

		public ZBool IsLocalCartage
		{
			get
			{
				return (ZBool)(jobHeader != null && jobHeader.ParentTableCode == JobCartageSchema.Constants.Prefix);
			}
		}

		public ZBool IsCFSShipmentJob
		{
			get
			{
				return (ZBool)(jobHeader != null && jobHeader.ParentTableCode == JobShipmentSchema.Constants.Prefix) && IsCFSShipment;
			}
		}

		public ZBool IsLoadListJob
		{
			get
			{
				return (ZBool)(jobHeader != null && jobHeader.ParentTableCode == JobConsolSchema.Constants.Prefix);
			}
		}

		public ZBool IsSundryCharges
		{
			get
			{
				return (ZBool)(jobHeader != null && jobHeader.ParentTableCode == JobSundryChargesSchema.Constants.Prefix);
			}
		}

		public ZBool IsVoyageAccounting
		{
			get
			{
				return (ZBool)(jobHeader != null && jobHeader.ParentTableCode == JobVoyAccountSchema.Constants.Prefix);
			}
		}

		public ZBool IsContainerDetention
		{
			get
			{
				return (ZBool)(jobHeader != null && jobHeader.ParentTableCode == JobContainerDetentionSchema.Constants.Prefix);
			}
		}

		public ZBool IsContainerRegistrationJob
		{
			get
			{
				return (ZBool)(jobHeader != null && jobHeader.ParentTableCode == JobContainerSchema.Constants.Prefix);
			}
		}

		#region IsWarehouseJob

		public ZBool IsWarehouseJob
		{
			get
			{
				if (!isWarehouseJob.HasValue)
				{
					warehouseJob = GetWarehouseJob();
					warehouseInvoice = GetJobStorage();
					isWarehouseJob = (warehouseJob != null || warehouseInvoice != null);
				}

				return isWarehouseJob.Value;
			}
		}

		WhsDocket GetWarehouseJob()
		{
			var docket = charge != null ? WhsChargeHelper.GetDocket((JobCharge)charge.WrappedObject) : null;

			if (docket == null && jobHeader != null && jobHeader.ParentTableCode == WhsDocketSchema.Constants.Prefix)
			{
				docket = factory.Load<WhsDocket>(jobHeader.ParentID);
			}

			return docket;
		}

		WhsInvoice GetJobStorage()
		{
			WhsInvoice storage = null;

			if (jobHeader != null && jobHeader.ParentTableCode == JobStorageSchema.Constants.Prefix)
			{
				storage = factory.Load<WhsInvoice>(jobHeader.ParentID);
			}

			return storage;
		}

		ZBool? isWarehouseJob;
		WhsDocket warehouseJob;
		WhsInvoice warehouseInvoice;

		#endregion
	}
}
