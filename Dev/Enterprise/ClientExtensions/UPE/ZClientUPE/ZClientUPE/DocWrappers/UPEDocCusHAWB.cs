using System.Drawing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.Customs.AU;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.UPE.Business
{
	public class UPEDocCusHAWB : DocCusHAWB
	{
		protected UPEDocCusHAWB(UPECusHAWB cusHAWB, BusinessObjectFactory factoryToWrap)
			: base(cusHAWB, factoryToWrap)
		{
		}

		public static UPEDocCusHAWB New(UPECusHAWB cusHAWB, BusinessObjectFactory factoryToWrap)
		{
			return (cusHAWB == null) ? null : new UPEDocCusHAWB(cusHAWB, factoryToWrap);
		}

		protected new UPECusHAWB CusHAWB
		{
			get { return (UPECusHAWB)base.CusHAWB; }
		}

		public ZString ConsigneeContactNameOrTheWordCustomer
		{
			get { return ConsigneeContactName.IsEmpty ? "Customer" : (string)ConsigneeContactName; }
		}

		public ZString ConsignorContactNameOrTheWordShipper
		{
			get { return ConsignorContactName.IsEmpty ? "Shipper" : (string)ConsignorContactName; }
		}

		public ZDecimal TotalSplitShipmentPiecesManifested
		{
			get
			{
				ZDecimal result = 0;
				foreach (UPECusHAWB relatedCusHAWB in RelatedCusHAWBs)
				{
					result += relatedCusHAWB.CS_PiecesManifested;
				}
				return result;
			}
		}

		public ZString DeclarationEntryNumberOrPending
		{
			get
			{
				ZString result;
				if (Declaration == null)
				{
					result = ZString.Empty;
				}
				else if (Declaration.CustomsEntryNumber.IsEmpty)
				{
					result = "(Pending)";
				}
				else
				{
					result = Declaration.CustomsEntryNumber;
				}
				return result;
			}
		}

		#region UPE HAWB

		#region Consignee Address

		public ZString ConsigneeAddress1
		{
			get { return Consignee != null ? Consignee.Name : CusHAWB.CS_ConsigneeName; }
		}

		public ZString ConsigneeAddress2
		{
			get { return Consignee != null ? Consignee.MainAddress.Address1 : CusHAWB.CS_ConsigneeStreet; }
		}

		public ZString ConsigneeAddress3
		{
			get { return Consignee != null ? Consignee.MainAddress.Address2 : CusHAWB.CS_ConsigneeStreet2; }
		}

		public ZString ConsigneeAddress4
		{
			get
			{
				return Consignee != null
					? (Consignee.MainAddress.City + " " + Consignee.MainAddress.State).Trim()
					: (CusHAWB.CS_ConsigneeCity + " " + CusHAWB.CS_ConsigneeState).Trim();
			}
		}

		public ZString ConsigneeAddress5
		{
			get
			{
				return Consignee != null
					? (Consignee.MainAddress.Country + " " + Consignee.MainAddress.PostCode).Trim()
					: (CusHAWB.CS_RN_NKConsigneeCountry + " " + CusHAWB.CS_ConsigneePostcode).Trim();
			}
		}

		#endregion

		#region Consignor Address

		public ZString ConsignorAddress1
		{
			get { return Consignor != null ? Consignor.Name : CusHAWB.CS_ConsignorName; }
		}

		public ZString ConsignorAddress2
		{
			get { return Consignor != null ? Consignor.MainAddress.Address1 : CusHAWB.CS_ConsignorStreet; }
		}

		public ZString ConsignorAddress3
		{
			get { return Consignor != null ? Consignor.MainAddress.Address2 : CusHAWB.CS_ConsignorStreet2; }
		}

		public ZString ConsignorAddress4
		{
			get
			{
				return Consignor != null
					? (Consignor.MainAddress.City + " " + Consignor.MainAddress.State).Trim()
					: (CusHAWB.CS_ConsignorCity + " " + CusHAWB.CS_ConsignorState).Trim();
			}
		}

		public ZString ConsignorAddress5
		{
			get
			{
				return Consignor != null
					? (Consignor.MainAddress.Country + " " + Consignor.MainAddress.PostCode).Trim()
					: (CusHAWB.CS_RN_NKConsignorCountry + " " + CusHAWB.CS_ConsignorPostcode).Trim();
			}
		}

		#endregion

		#region BillTo Address

		public virtual ZString BillToHAWBAddress1
		{
			get { return ""; }
		}

		public virtual ZString BillToHAWBAddress2
		{
			get { return ""; }
		}

		public virtual ZString BillToHAWBAddress3
		{
			get { return ""; }
		}

		public virtual ZString BillToHAWBAddress4
		{
			get { return ""; }
		}

		public virtual ZString BillToHAWBAddress5
		{
			get { return ""; }
		}

		#endregion

		public ZString HouseBill
		{
			get { return CusHAWB.CS_HAWB; }
		}

		public ZString MasterBill
		{
			get { return CusHAWB.CS_MasterBillNum.Left(3) + "-" + CusHAWB.CS_MasterBillNum.SubstringSafe(3); }
		}

		public ZString AirlinePrefix
		{
			get { return CusHAWB.CS_MasterBillNum.Left(3); }
		}

		public ZString AWBSerialNumber
		{
			get { return CusHAWB.CS_MasterBillNum.SubstringSafe(3); }
		}

		public virtual ZString InvoiceNumber
		{
			get { return ""; }
		}

		public ZString AWBOrigin
		{
			get { return CusHAWB.CS_RL_NKOrigin.SubstringSafe(2); }
		}

		public ZString Origin
		{
			get { return CusHAWB.CS_RL_NKOrigin; }
		}

		public ZString Destination
		{
			get { return CusHAWB.CS_RL_NKDestination; }
		}

		public ZDateTime ArrivalDate
		{
			get { return CusHAWB.CS_ArrivalDate; }
		}

		public ZString CurrencyCode
		{
			get { return CusHAWB.CS_RX_NKGoodsCurrency; }
		}

		public ZString UQ
		{
			get { return CusHAWB.CS_WeightUQ.Left(1); }
		}

		public ZDecimal DimensionalWeight
		{
			get { return CusHAWB.CS_ChargableWeight; }
		}

		#region Other Charges

		public ZString OtherCharges1
		{
			get
			{
				ZString result = "";
				if (UPEDocDeclaration != null && UPEDocDeclaration.HasAlternateBroker)
				{
					result = "Documents to alternate broker: " + ZDateTime.Now.ToShortDateString();
				}
				return result;
			}
		}

		public ZString OtherCharges2
		{
			get
			{
				ZString result = "";
				if (UPEDocDeclaration != null && UPEDocDeclaration.HasAlternateBroker)
				{
					result = "Storage Date: " + UPEDocDeclaration.AlternateBrokerStorageFeeStartDate.ToShortDateString();
				}
				return result;
			}
		}

		public ZString OtherCharges3
		{
			get
			{
				ZString result = "";
				if (UPEDocDeclaration != null && UPEDocDeclaration.HasAlternateBroker)
				{
					result = "Amount Payable: " + UPEDocDeclaration.AlternateBrokerAmountPayable.ToString(2);
				}
				return result;
			}
		}

		public ZString OtherCharges4
		{
			get
			{
				ZString result = "";
				if (UPEDocDeclaration != null && UPEDocDeclaration.HasAlternateBroker)
				{
					result = "Estimated Freight Rate: " + UPEDocDeclaration.EstimatedFreight.ToString(2);
				}
				return result;
			}
		}

		#endregion

		#endregion

		#region UPE Tax Inoice

		public Image UPETaxInvoiceImage
		{
			get
			{
				if (fUPETaxInvoiceImage == null)
				{
					if (UPEDataRegistry.Instance.TaxInvoiceImage != null)
					{
						fUPETaxInvoiceImage = UPEDataRegistry.Instance.TaxInvoiceImage.Value;
					}
				}
				return fUPETaxInvoiceImage;
			}
		}
		Image fUPETaxInvoiceImage;

		public ZBool HasUPETaxInvoiceImage
		{
			get { return UPETaxInvoiceImage != null; }
		}

		#endregion

		#region Related Doc Wrappers

		public DocShipmentHeldLetterDetails ShipmentHeldLetterDetails
		{
			get
			{
				if (fShipmentHeldLetterDetails == null)
				{
					fShipmentHeldLetterDetails = DocShipmentHeldLetterDetails.New(CusHAWB.ShipmentHeldLetterDetails, Factory);
				}
				return fShipmentHeldLetterDetails;
			}
		}
		DocShipmentHeldLetterDetails fShipmentHeldLetterDetails;

		public UPEDocCusHAWB FirstSplitCusHAWB
		{
			get { return UPEDocCusHAWB.New(RelatedCusHAWBs[0], Factory); }
		}

		public UPEDocCusHAWB SubsequentSplitCusHAWB
		{
			get { return RelatedCusHAWBs.Length < 2 ? null : UPEDocCusHAWB.New(RelatedCusHAWBs[1], Factory); }
		}

		public UPEDocDeclaration UPEDocDeclaration
		{
			get { return (UPEDocDeclaration)Declaration; }
		}

		#endregion

		#region Implementation

		UPECusHAWB[] RelatedCusHAWBs
		{
			get
			{
				if (fRelatedCusHAWBs == null)
				{
					fRelatedCusHAWBs = (UPECusHAWB[])Factory.Load(typeof(UPECusHAWB), new ZQuery(CusHAWBSchema.CS_HAWB, CusHAWB.CS_HAWB));
				}
				return fRelatedCusHAWBs;
			}
		}
		UPECusHAWB[] fRelatedCusHAWBs;

		#endregion

	}
}
