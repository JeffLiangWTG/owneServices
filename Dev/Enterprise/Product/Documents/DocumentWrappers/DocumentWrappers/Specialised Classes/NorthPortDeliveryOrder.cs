using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using static Enterprise.Integration.Customs.MY;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class NorthPortDeliveryOrder : NonPersistentBusinessObject, IObsoleteValidation
	{
		public NorthPortDeliveryOrder(DocForwardingShipment shipmentWrapper)
			: base(shipmentWrapper.Factory)
		{
			ShipmentWrapper = shipmentWrapper;
		}

		public ZString MarksAndNumbers
		{
			get
			{
				if (fMarksAndNumbers.IsEmpty && !ShipmentWrapper.MarksAndNumbers.IsEmpty)
				{
					ZString marksAndNumbersColumn = ShipmentWrapper.WrapTextForAColumn(ShipmentWrapper.MarksAndNumbers, ShipmentWrapper.MarksAndNumbersWidth);
					ZInt countOfNumberOfLines = marksAndNumbersColumn.Split('\n').Length;

					if (ShipmentWrapper.MarksAndNumbersHeight >= countOfNumberOfLines)
					{
						fMarksAndNumbers = marksAndNumbersColumn;
					}
					else
					{
						fMarksAndNumbers = Res.GetString("5f6870ec-dc23-44ee-b8ed-b1b58907273a", "Please see attached");
					}
				}

				return fMarksAndNumbers;
			}
		}

		public ZString GoodsDescription
		{
			get
			{
				if (fGoodsDescription.IsEmpty)
				{
					ZString result = ZString.Empty;

					if (!ShipmentWrapper.DescriptionForGoods.IsEmpty)
					{
						result += ShipmentWrapper.WrapTextForAColumn(ShipmentWrapper.DescriptionForGoods, ShipmentWrapper.GoodsDescWidth);
					}

					ZInt countOfNumberOfLines = result.Split('\n').Length;

					if (ShipmentWrapper.GoodsDescriptionHeight >= countOfNumberOfLines)
					{
						fGoodsDescription = result;
					}
					else
					{
						fGoodsDescription = Res.GetString("5f6870ec-dc23-44ee-b8ed-b1b58907273a", "Please see attached");
					}
				}

				return fGoodsDescription;
			}
		}

		public ZString Packages
		{
			get
			{
				if (fPackages.IsEmpty)
				{
					fPackages = BillOfLading.GetPackageCount();
				}

				return fPackages;
			}
		}

		public ZString Weight
		{
			get { return BillOfLading.Weight; }
		}

		public ZString Volume
		{
			get { return BillOfLading.Volume; }
		}

		public ZString ContainerNumAndSize
		{
			get
			{
				ZString result = ZString.Empty;

				if (ShipmentWrapper.Containers != null)
				{
					if (ShipmentWrapper.Containers.Count == 1)
					{
						result = ShipmentWrapper.Containers[0].ContainerNumber;
						if (ShipmentWrapper.Containers[0].Container != null)
						{
							result += "/" + ShipmentWrapper.Containers[0].Container.Code;
						}
					}
					else if (ShipmentWrapper.Containers.Count > 0)
					{
						result = Res.GetString("5f6870ec-dc23-44ee-b8ed-b1b58907273a", "Please see attached");
					}
				}

				return result;
			}
		}

		public ZDateTime DateOfArrival
		{
			get
			{
				ZDateTime result = ZDateTime.Empty;

				if (ShipmentWrapper.Consol != null)
				{
					if (!ShipmentWrapper.Consol.ATA.IsEmpty)
					{
						result = ShipmentWrapper.Consol.ATA;
					}
					else
					{
						result = ShipmentWrapper.Consol.ETA;
					}
				}

				return result;
			}
		}

		#region WarehouseNumber

		public ZString WarehouseNumber
		{
			get
			{
				ZString result = ZString.Empty;
				if (ShipmentWrapper.Consol != null)
				{
					result = GetWarehouseNumberFromShipment(ShipmentWrapper.Consol, ShipmentWrapper);
					if (result.IsEmpty)
					{
						result = GetWarehouseNumberFromConsol(ShipmentWrapper.Consol);
					}
				}
				return result;
			}
		}

		ZString GetWarehouseNumberFromShipment(DocShipmentConsol consol, DocForwardingShipment shipment)
		{
			ZString result;
			if (consol.IsImportConsol)
			{
				result = GetWarehouseNumberFromDepotAddress(shipment.ImportReleaseDepot);
			}
			else
			{
				result = GetWarehouseNumberFromDepotAddress(shipment.ExportReceivingDepot);
			}
			return result;
		}

		ZString GetWarehouseNumberFromConsol(DocShipmentConsol consol)
		{
			ZString result;
			if (consol.IsImportConsol)
			{
				result = GetWarehouseNumberFromDepotAddress(consol.UnpackDepotAddress);
			}
			else
			{
				result = GetWarehouseNumberFromDepotAddress(consol.PackDepotAddress);
			}
			return result;
		}

		ZString GetWarehouseNumberFromDepotAddress(DocDocAddress depotAddress)
		{
			ZString result = "";
			if (depotAddress != null && depotAddress.Organisation != null)
			{
				result = depotAddress.Organisation.CustomCodes.GetCustomsRegNoForCodeAndCountry(MalaysiaOrgCusCodeInfo.OrgCusCodes.CFSBondedPackUnpack, GlbCompany.CurrentCompany.Country);
			}
			return result;
		}

		#endregion

		public ZString PortOperatorAndSCN
		{
			get { return (Operator != null) ? Operator.PortOperatorAndSCN : ZString.Empty; }
		}

		public ZString CCC
		{
			get
			{
				ZString result = ZString.Empty;

				if (ShipmentWrapper.Consol != null && ShipmentWrapper.Consol.ShippingLine != null)
				{
					foreach (DocCusCode currentCusCode in ShipmentWrapper.Consol.ShippingLine.CustomCodes)
					{
						if (currentCusCode.Country != null && currentCusCode.Country.Code == "MY" && currentCusCode.CodeType == OrgCusCode.CodeTypes.CarrierCode)
						{
							result = currentCusCode.CustomsRegNo;
							break;
						}
					}
				}

				return result;
			}
		}

		public ZString FreightCharge
		{
			get
			{
				ZString result = ZString.Empty;

				if (!ShipmentWrapper.PrepaidCollect.IsEmpty)
				{
					result = Res.GetString("7b052b33-fa7c-4eb1-add1-89f51ccf2e03", "Freight {0}", ShipmentWrapper.PrepaidCollect);
				}

				return result;
			}
		}

		public ZString ExchangeRates
		{
			get
			{
				ZString result = ZString.Empty;

				if (JobInvoicingHeader != null && JobInvoicingHeader.ExchangeRates != null)
				{
					foreach (DocJobExchangeRate currentRate in JobInvoicingHeader.ExchangeRates)
					{
						result += currentRate.Currency.Code + " " + ShipmentWrapper.FormatNumber(currentRate.BuyRate) + ", ";
					}
				}

				result = result.Trim();
				return result.TrimEnd(',');
			}
		}

		#region NorthPort Container Shipping Note
		public ZString ExportTranshipmentVessel
		{
			get
			{
				if (ExportTranshipmentConsol != null)
				{
					return ExportTranshipmentConsol.VesselName;
				}
				else if (ExportTranshipmentPlanning != null)
				{
					return ExportTranshipmentPlanning.VesselName;
				}
				return ZString.Empty;
			}
		}

		public ZString ExportTranshipmentVoyage
		{
			get
			{
				if (ExportTranshipmentConsol != null)
				{
					return ExportTranshipmentConsol.VoyageNumber;
				}
				else if (ExportTranshipmentPlanning != null)
				{
					return ExportTranshipmentPlanning.VoyageFlight;
				}
				return ZString.Empty;
			}
		}

		public DocUNLOCO ExportTranshipmentPortOfLoading
		{
			get
			{
				if (ExportTranshipmentConsol != null)
				{
					return ExportTranshipmentConsol.PortOfLoading;
				}
				else if (ExportTranshipmentPlanning != null)
				{
					return ExportTranshipmentPlanning.PortOfLoading;
				}
				return null;
			}
		}

		public DocUNLOCO ExportTranshipmentPortOfDischarge
		{
			get
			{
				if (ExportTranshipmentConsol != null)
				{
					return ExportTranshipmentConsol.PortOfDischarge;
				}
				else if (ExportTranshipmentPlanning != null)
				{
					return ExportTranshipmentPlanning.PortOfDischarge;
				}
				return null;
			}
		}

		public ZString TranshipmentGoodsDetails
		{
			get
			{
				ZString result = "";
				result += Packages;
				result += AddNewLine(result);
				result += ShipmentWrapper.GoodsDescription;
				result += AddNewLine(result);
				result += "\n";
				result += ImportTranshipmentDetails;
				return result;
			}
		}

		public ZString ImportTranshipmentDetails
		{
			get
			{
				ZString result = "";
				if (ShipmentWrapper.Consol != null)
				{
					result += Res.GetString("5bc79a11-bf19-4409-b986-fb1b64e045d8", "T/S EX.") + " ";
					result += ShipmentWrapper.Consol.VesselName + " ";
					result += ShipmentWrapper.Consol.VoyageNumber.IsEmpty ? "" : "V" + ShipmentWrapper.Consol.VoyageNumber;
					result += "\n";
				}
				if (!ShipmentWrapper.HouseBill.IsEmpty)
				{
					result += Res.GetString("84c9eeca-e4ae-410e-bc6b-ea0c945848c4", "Bill Of Lading No.") + " ";
					result += ShipmentWrapper.HouseBill;
					result += "\n";
				}
				if (!ShipmentWrapper.ContainerLine.IsEmpty)
				{
					result += Res.GetString("ee608660-98d4-4b14-ab83-25a3904903da", "Container No.") + " ";
					result += ShipmentWrapper.ContainerLine;
				}
				return result;
			}
		}

		#endregion

		#region Implementation

		IPortOperator Operator
		{
			get
			{
				if (portOperator is null && ShipmentWrapper.Consol is not null)
				{
					var consol = Factory.LoadTop1<ForwardingConsol>(new ZQuery(JobConsolSchema.JK_UniqueConsignRef, ShipmentWrapper.Consol.ConsolNumber));
					portOperator = ObjectFactory.New<IPortOperator>(consol, ImportExportHelper.IsBranchCountry(consol.JK_RL_NKDischargePort));
				}
				return portOperator;
			}
		}

		IPortOperator portOperator;
		protected internal DocForwardingShipment ShipmentWrapper;
		protected ZString fMarksAndNumbers;
		protected ZString fGoodsDescription;
		protected ZString fPackages;
		protected DocBillOfLading fBillOfLading;
		protected DocTransport fExportTranshipmentPlanning;
		protected DocForwardingConsol fExportTranshipmentConsol;

		protected DocBillOfLading BillOfLading
		{
			get
			{
				if (fBillOfLading == null)
				{
					fBillOfLading = new DocBillOfLading(ShipmentWrapper);
				}

				return fBillOfLading;
			}
		}

		protected DocJobInvoicingJob JobInvoicingHeader
		{
			get
			{
				DocJobInvoicingJob jobWrapper = null;
				ZQuery filter1 = new ZQuery(JobHeaderSchema.JH_ParentTableCode, JobShipmentSchema.Constants.Prefix);
				ZQuery filter2 = new ZQuery(JobHeaderSchema.JH_ParentID, ShipmentWrapper.CommonShipment.PK);
				ZQuery filter = new ZQuery(filter1, filter2);
				Job[] jobs = (Job[])Factory.Load(typeof(Job), filter);

				if (jobs != null && jobs.Length > 0)
				{
					jobWrapper = DocJobInvoicingJob.New(jobs[0], Factory);
				}

				return jobWrapper;
			}
		}

		protected DocForwardingConsol ExportTranshipmentConsol
		{
			get
			{
				if (fExportTranshipmentConsol == null)
				{
					if (ShipmentWrapper.ShipmentConsols.Count > 1)
					{
						foreach (DocForwardingConsol consol in ShipmentWrapper.ShipmentConsols)
						{
							if (consol.PortOfLoading != null)
							{
								if (consol.PortOfLoading.CountryCode == GlbBranch.CurrentBranch.Country.Code)
								{
									fExportTranshipmentConsol = consol;
								}
							}
						}
					}
				}
				return fExportTranshipmentConsol;
			}
		}

		protected DocTransport ExportTranshipmentPlanning
		{
			get
			{
				if (fExportTranshipmentPlanning == null)
				{
					if (ShipmentWrapper.Consol != null)
					{
						if (ShipmentWrapper.Consol.PortOfDischarge != null)
						{
							foreach (DocTransport shipmentTransport in ShipmentWrapper.ShipmentTransportPlanning)
							{
								if (!shipmentTransport.PortOfLoadingCode.IsEmpty)
								{
									if (shipmentTransport.PortOfLoadingCode == ShipmentWrapper.Consol.PortOfDischarge.Code)
									{
										fExportTranshipmentPlanning = shipmentTransport;
									}
								}
							}
						}
					}
				}
				return fExportTranshipmentPlanning;
			}
		}

		protected ZString AddNewLine(ZString text)
		{
			return (!text.EndsWith("\n") && !text.IsEmpty) ? "\n" : "";
		}
		#endregion
	}
}
