using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.DocumentEngine.Imaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.UPE.Business
{
	public class DocCallout : UPEDocCusHAWB
	{
		protected DocCallout(Callout callout, BusinessObjectFactory factoryToWrap)
			: base(callout, factoryToWrap)
		{
		}

		public static DocCallout New(Callout callout, BusinessObjectFactory factoryToWrap)
		{
			return (callout == null) ? null : new DocCallout(callout, factoryToWrap);
		}

		protected Callout Callout
		{
			get { return (Callout)base.CusHAWB; }
		}

		#region World Ease Heading Overrides

		public ZString ShipmentDetailsHeaderText
		{
			get
			{
				ZString result = "Import Shipment Detail";
				if (Callout.DutyType == DutyTypeCodeDescriptionPairList.Codes.GCC)
				{
					result = "World Ease Import Shipment Detail";
				}
				return result;
			}
		}

		public ZString ShipmentNoHeaderText
		{
			get
			{
				ZString result = "Shipment No.";
				if (Callout.DutyType == DutyTypeCodeDescriptionPairList.Codes.GCC)
				{
					result = "World Ease No.";
				}
				return result;
			}
		}

		#endregion

		#region Bill To

		public ZString BillToName
		{
			get { return Callout.BillTo == null ? Callout.CS_ConsigneeName : Callout.BillTo.OH_FullNameTruncated; }
		}

		public ZString BillToAddress1
		{
			get
			{
				ZString result = ZString.Empty;
				if (UseConsigneeAddress)
				{
					result = Callout.CS_ConsigneeStreet;
				}
				else if (BillToOrgAddress != null)
				{
					result = BillToOrgAddress.OA_Address1;
				}
				return result;
			}
		}

		public ZString BillToAddress2
		{
			get
			{
				ZString result = ZString.Empty;
				if (UseConsigneeAddress)
				{
					result = Callout.CS_ConsigneeStreet2 + " " + Callout.CS_ConsigneeCity + " " + Callout.CS_ConsigneeState + " " + Callout.CS_ConsigneePostcode;
				}
				else if (BillToOrgAddress != null)
				{
					result = BillToOrgAddress.OA_Address2 + " " + BillToOrgAddress.OA_City + " " + BillToOrgAddress.OA_State + " " + BillToOrgAddress.OA_PostCode;
				}
				return result;
			}
		}

		public ZString BillToAccountNumber
		{
			get { return Callout.BillToAccountNumber; }
		}

		#region Implementation

		bool UseConsigneeAddress
		{
			get { return (BillToOrgAddress == null && !BillToAccountNumber.IsEmpty); }
		}

		OrgAddress BillToOrgAddress
		{
			get
			{
				if (fBillToOrgAddress == null)
				{
					fBillToOrgAddress = FindDefaultPostalAddressOrFallBackToMainAddressOfBillTo();
				}
				return fBillToOrgAddress;
			}
		}
		OrgAddress fBillToOrgAddress;

		OrgAddress FindDefaultPostalAddressOrFallBackToMainAddressOfBillTo()
		{
			OrgAddress result = null;
			if (Callout.BillTo != null)
			{
				result = Callout.BillTo.Addresses.DefaultAddressOfType(OrgAddressType.Postal, false);
				if (result == null)
				{
					result = Callout.BillTo.Addresses.DefaultAddressOfType(OrgAddressType.Office, false);
				}
			}
			return result;
		}

		#endregion

		#endregion

		#region BillTo HAWB Address

		public override ZString BillToHAWBAddress1
		{
			get { return Callout.BillTo != null ? Callout.BillTo.OH_FullNameTruncated : base.BillToHAWBAddress1; }
		}

		public override ZString BillToHAWBAddress2
		{
			get { return Callout.BillTo != null ? Callout.BillTo.MainAddress.OA_Address1 : base.BillToHAWBAddress2; }
		}

		public override ZString BillToHAWBAddress3
		{
			get { return Callout.BillTo != null ? Callout.BillTo.MainAddress.OA_Address2 : base.BillToHAWBAddress3; }
		}

		public override ZString BillToHAWBAddress4
		{
			get { return Callout.BillTo != null ? ((ZString)(Callout.BillTo.MainAddress.OA_City + " " + Callout.BillTo.MainAddress.OA_State).Trim()) : base.BillToHAWBAddress4; }
		}

		public override ZString BillToHAWBAddress5
		{
			get { return Callout.BillTo != null ? ((ZString)(Callout.BillTo.MainAddress.OA_RL_NKRelatedPortCode.Left(2) + " " + Callout.BillTo.MainAddress.OA_PostCode).Trim()) : base.BillToHAWBAddress4; }
		}

		#endregion

		#region ReferenceNumber1 / ReferenceNumber2

		public ZString ReferenceNumber1
		{
			get { return (Callout.Level1Record == null || Callout.Level1Record._300000 == null) ? "" : Callout.Level1Record._300000.ReferenceNumber1; }
		}

		public ZString ReferenceNumber2
		{
			get { return (Callout.Level1Record == null || Callout.Level1Record._400000 == null) ? "" : Callout.Level1Record._400000.ReferenceNumber2; }
		}

		#endregion

		#region Related Business Objects

		public DocCalloutChargeCollection Charges
		{
			get
			{
				if (fCharges == null)
				{
					fCharges = (Callout.JobHeader != null)
						? new DocCalloutChargeCollection(Callout.JobHeader.Charges, Factory)
						: new DocCalloutChargeCollection(Factory);
				}
				return fCharges;
			}
		}
		DocCalloutChargeCollection fCharges;

		#endregion

		#region Properties

		bool IsDocRTS
		{
			get
			{
				return Callout.IsRTS && Callout.CurrentQueue.P4_QueueName == CommercialQueueCodeDescriptionPairList.Codes.Rebill && Callout.CurrentQueue.P4_Status == ReasonCodeDescriptionPairList.Codes._R3_RTS;
			}
		}

		public ZString DeliverToContact
		{
			get
			{
				if (Callout.IsRedirected)
				{
					return Callout.DeliveryAddressOverride.P3_ContactName;
				}
				else if (IsDocRTS)
				{
					return Callout.CS_ConsignorContactName;
				}
				else
				{
					return Callout.CS_ConsigneeContactName;
				}
			}
		}

		public ZString DeliverToName
		{
			get
			{
				if (Callout.IsRedirected)
				{
					return Callout.DeliveryAddressOverride.P3_CompanyName;
				}
				else if (IsDocRTS)
				{
					return Callout.CS_ConsignorName;
				}
				else
				{
					return Callout.CS_ConsigneeName;
				}
			}
		}

		public ZString DeliverToStreet1
		{
			get
			{
				if (Callout.IsRedirected)
				{
					return Callout.DeliveryAddressOverride.P3_Address1;
				}
				else if (IsDocRTS)
				{
					return Callout.CS_ConsignorStreet;
				}
				else
				{
					return Callout.CS_ConsigneeStreet;
				}
			}
		}

		public ZString DeliverToStreet2
		{
			get
			{
				if (Callout.IsRedirected)
				{
					return Callout.DeliveryAddressOverride.P3_Address2;
				}
				else if (IsDocRTS)
				{
					return Callout.CS_ConsignorStreet2;
				}
				else
				{
					return Callout.CS_ConsigneeStreet2;
				}
			}
		}

		public ZString DeliverToCity
		{
			get
			{
				if (Callout.IsRedirected)
				{
					return Callout.DeliveryAddressOverride.P3_City;
				}
				else if (IsDocRTS)
				{
					return Callout.CS_ConsignorCity;
				}
				else
				{
					return Callout.CS_ConsigneeCity;
				}
			}
		}

		public ZString DeliverToPostCode
		{
			get
			{
				if (Callout.IsRedirected)
				{
					return Callout.DeliveryAddressOverride.P3_PostCode;
				}
				else if (IsDocRTS)
				{
					return Callout.CS_ConsignorPostcode;
				}
				else
				{
					return Callout.CS_ConsigneePostcode;
				}
			}
		}

		public ZString DeliverToPhone
		{
			get
			{
				if (Callout.IsRedirected)
				{
					return Callout.DeliveryAddressOverride.P3_Phone;
				}
				else if (IsDocRTS)
				{
					return Callout.CS_ConsignorPhone;
				}
				else
				{
					return Callout.CS_ConsigneePhone;
				}
			}
		}

		public ZString HFCContactName
		{
			get { return !Callout.IsHoldForCollection ? ZString.Empty : Callout.HFCContactName; }
		}

		public ZString HFCContactPhoneNumber
		{
			get { return !Callout.IsHoldForCollection ? ZString.Empty : Callout.HFCContactPhoneNumber; }
		}

		public ZString LabelType1Text
		{
			get
			{
				ZString result = "";

				if (Callout.IsHoldForCollection)
				{
					result = Callout.HoldForCollectDepot.IsEmpty ? "HFC" : string.Format("HFC at {0} Depot", Callout.HoldForCollectDepot);
				}
				else if (Callout.DoesStatusIndicateTranshipment)
				{
					result = "TRANSHIP";
				}
				else if (Callout.IsAbandoned)
				{
					result = "ABANDON";
				}
				else if (Callout.IsRTS)
				{
					result = "RTS";
				}
				else if (Callout.IsRedirected)
				{
					result = "REDIRECT";
				}

				return result;
			}
		}

		public ZString LabelType2Text
		{
			get
			{
				return Callout.IsCOD ? "COD" : "";
			}
		}

		public ZString TranshipmentNumber
		{
			get { return Callout.CS_TranshipmentEntryNum.IsEmpty ? "" : "CAN: " + Callout.CS_TranshipmentEntryNum; }
		}

		public ZString CODAmount
		{
			get
			{
				ZString result = "";
				if (Callout.IsCOD)
				{
					ZDecimal cODAmount = 0m;
					if (Callout.PartPaymentUsed)
					{
						cODAmount = Callout.PartPaymentAmountToCollect;
					}
					else
					{
						cODAmount = Callout.TotalAmountDue;
						if (Callout.BillTo != null)
						{
							if (Callout.BillTo.AccountClass == "1" || Callout.BillTo.AccountClass == "3" || Callout.BillTo.AccountClass == "4")
							{
								cODAmount = LocalCharges;
							}
						}
					}

					cODAmount = Utilities.Round(cODAmount, 2);
					result = "$" + cODAmount.ToString(2);
				}

				return result;
			}
		}

		protected virtual ZDecimal LocalCharges
		{
			get { return Callout != null ? Callout.TotalLocalCharges : ZDecimal.Zero; }
		}

		public ZString DeliveryInstructionsNote
		{
			get { return Callout.DeliveryInstructionsNote != null ? Callout.DeliveryInstructionsNote.ST_NoteText : ZString.Empty; }
		}

		public ZString WayBillShort
		{
			get { return Callout.WayBillShort; }
		}

		public override ZString InvoiceNumber
		{
			get { return Callout.InvoiceNumber; }
		}

		public ZBool ShowBPayPaymentOption
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.EnableElectronicPayments.Value;
			}
		}

		public ZString BPayBillerCode
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.ElectronicPaymentBillerCode.Value;
			}
		}

		public ZString BPayTerms
		{
			get
			{
				return AccountingConfigurationRegistry.Instance.ElectronicPaymentTerms.Value;
			}
		}

		public ZString BPayReferenceNumber
		{
			get
			{
				var invoiceNo = Callout.InvoiceNumber;

				var weightingList = new List<int>() { 7, 3, 1, 7, 3, 1, 7, 3, 1, 7, 3, 1, 7, 3, 1, 7, 3, 1, 7, 3 };
				var addDigits = invoiceNo.ToString()
										.Select(x => int.Parse(x.ToString()))
										.Reverse()
										.Zip(weightingList.Take(invoiceNo.Length), (x, y) => x * y)
										.Sum();

				var checkDigit = 10 - (addDigits % 10);
				checkDigit = checkDigit == 10 ? 0 : checkDigit;

				return invoiceNo.ToString() + checkDigit.ToString();
			}
		}

		public ZDateTime InvoiceDate
		{
			get { return Callout.BisiDownloadDate; }
		}

		public override ZString FreightPrepaidCollectDescription
		{
			get
			{
				ZString result = base.FreightPrepaidCollectDescription;
				if (CusHAWB.CS_FreightPrepaidCollect == Core.Constants.PaymentType.Collect ||
					CusHAWB.CS_FreightPrepaidCollect == CMRMethodsOfPayment.Codes.Collect)
				{
					result = "Freight Collect";
				}
				else if (CusHAWB.CS_FreightPrepaidCollect == CMRMethodsOfPayment.Codes.PrepaidOnly || CusHAWB.CS_FreightPrepaidCollect == Core.Constants.PaymentType.Prepaid)
				{
					result = "Prepaid";
				}
				return result;
			}
		}

		public ZDecimal TotalChargeDiscounts
		{
			get
			{
				ZDecimal result = 0;
				if (Callout.JobHeader != null)
				{
					foreach (CalloutCharge charge in Callout.JobHeader.Charges)
					{
						result += charge.Discount;
					}
				}
				return result;
			}
		}

		public ZString TaxInvoiceCommentsText1
		{
			get { return UPEDataRegistry.Instance.TaxInvoiceCommentsText1.Value; }
		}

		public ZString TaxInvoiceCommentsText2
		{
			get { return UPEDataRegistry.Instance.TaxInvoiceCommentsText2.Value; }
		}

		public ZString TaxInvoiceFooterText
		{
			get { return UPEDataRegistry.Instance.TaxInvoiceFooterText.Value; }
		}

		public ZBool IsCusEntryHeaderEntryPrintLinesAvailable
		{
			get { return Declaration != null && Declaration.AUCusEntryHeaderEntryPrintLines.Length > 0; }
		}

		public ZString ConsigneeAccountNum
		{
			get { return Callout.ConsigneeAccountNum; }
		}

		#region Commercial Invoice Image

		public ImageWrapperCollection CommercialInvoiceAsMultiPageImageCollection
		{
			get
			{
				var	commercialInvoiceAsMultiPageImageCollection = new ImageWrapperCollection(Factory);

					if (CommercialInvoiceImage != null)
					{
						commercialInvoiceAsMultiPageImageCollection = new ImageWrapperCollection(Factory);

						using (var imageSelector = new StandardImagePageSelector(CommercialInvoiceImage))
						{
							for (int i = 0; i < imageSelector.PageSelector.TotalPages; i++)
							{
								imageSelector.PageSelector.CurrentPageIndex = i;
								commercialInvoiceAsMultiPageImageCollection.Add(imageSelector.PageSelector.CurrentImage);
							}
						}
					}

				return commercialInvoiceAsMultiPageImageCollection;
			}
		}

		public Image CommercialInvoiceImage
		{
			get
			{
				Image result = null;
				if (Declaration == null)
				{
					result = CusHAWB.CommercialInvoiceImage;
				}
				else
				{
					result = ((UPEDocDeclaration)Declaration).CommercialInvoiceImage;
				}
				return result;
			}
		}

		#endregion

		#endregion
	}
}
