using System.Collections;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Customs.Business;
using Enterprise.Edifact;
using Enterprise.Edifact.D96B.Elements;
using Enterprise.Edifact.D96B.Messages.INVOIC;
using Enterprise.Edifact.D96B.Segments;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.CLE.MattelARInvoiceExport
{
	public abstract class INVOICMessageBuilder : NonPersistentBusinessObject, IObsoleteValidation
	{
		public INVOICMessageBuilder(JobInvoiceRecord invoiceRecord, BusinessObjectFactory factory)
			: base(factory)
		{
			Message = new INVOICMessage();
			this.InvoiceRecord = invoiceRecord;
		}

		public string InterchangeNumber
		{
			get { return fInterchangeNumber; }
		}

		#region Message Generator

		#region GenerateMessageText

		public string GenerateMessageText()
		{
			Factory.Save();

			NoOfSegment = 0;
			string messageNumber = "1" + InterchangeNumber.PadLeft(7, '0');

			UNHSegment(messageNumber);
			BGMSegment();
			DTMSegment();
			FTXSegment();
			RFFSegment();
			NADSegment();
			UNSSegment();
			Group15Segment();
			Group25Segment();
			Group48Segment();
			UNTSegment(messageNumber);

			string outputFile = GenerateOutputFile(InterchangeNumber);

			return outputFile;
		}

		#endregion

		#region MessageText

		protected string MessageText
		{
			get { return Message.ToString(new UNOACharacterSet()); }
		}

		#endregion

		#region GenerateOutputFile

		ZString GenerateOutputFile(string interchangeNumber)
		{
			ZString fileName = Path.Combine(Env.TempPath, Utilities.GetFileName(Shipment.JobNumber));
			using (StreamWriter stream = new StreamWriter(fileName))
			{
				string uNB = UNBSegment(interchangeNumber);
				string uNZ = "UNZ+1+" + interchangeNumber + "'";
				stream.Write(uNB);
				stream.Write(MessageText);
				stream.Write(uNZ);
			}

			return fileName;
		}

		#endregion

		#region UNBSegment

		string UNBSegment(string interchangeNumber)
		{
			ZDateTime now = ZDateTime.Now;
			string date = now.ToString("yyMMdd");
			string time = now.ToString("hhmm");
			string cleintMailBox = CLEDataRegistry.Instance.ClemengerMailboxNumber;
			string mattelMailBox = CLEDataRegistry.Instance.MattelMailboxNumber;

			string uNB = "UNB+UNOA:1+" + cleintMailBox + "+"
						+ mattelMailBox + "+" + date + ":"
						+ time + "+" + interchangeNumber + "++++++1'";

			return uNB;
		}

		#endregion

		#region UNHSegment

		void UNHSegment(string messageNumber)
		{
			UNHSegment uNH = Message.UNH.InstantiateAChildAndAddItToChildrenCollection();
			uNH.MessageIdentifier.MessageType = MessageTypeList.InvoiceMessage;
			uNH.MessageReferenceNumber = messageNumber;
			uNH.MessageIdentifier.MessageVersionNumber = "D";
			uNH.MessageIdentifier.MessageReleaseNumber = "96B";
			uNH.MessageIdentifier.ControllingAgency = ControllingAgencyList.UnEceTradeWp4;
			NoOfSegment++;
		}

		#endregion

		#region BGMSegment

		void BGMSegment()
		{
			BGMSegment bGM = Message.BGM.InstantiateAChildAndAddItToChildrenCollection();
			bGM.DocumentMessageName.DocumentMessageNameCoded = DocumentMessageNameCodedList.CommercialInvoice;
			bGM.DocumentMessageName.DocumentMessageName = "TAX INVOICE";
			bGM.DocumentMessageIdentification.DocumentMessageNumber = FirstDisbursementInvoiceNumber;
			bGM.MessageFunctionCoded = MessageFunctionCodedList.Original;

			NoOfSegment++;
		}

		#endregion

		#region DTMSegment

		void DTMSegment()
		{
			DTMSegment dTM = Message.DTM.InstantiateAChildAndAddItToChildrenCollection();
			dTM.DateTimePeriod.DateTimePeriodFormatQualifier = DateTimePeriodFormatQualifierList.Ccyymmdd;
			dTM.DateTimePeriod.DateTimePeriodQualifier = DateTimePeriodQualifierList.DeliveryDateTimeActual;
			dTM.DateTimePeriod.DateTimePeriod = StandardInvoices[0].AH_InvoiceDate.ToString("yyyyMMdd");

			NoOfSegment++;
		}

		#endregion

		#region FTXSegment

		protected
		string VesselCodeAndLloydsNumber
		{
			get
			{
				string lloydsNumber = new ZString("9999999");
				string result = lloydsNumber + "*";

				if (!Declaration.JE_VesselName.IsEmpty && Declaration.Vessel != null)
				{
					lloydsNumber = (!Declaration.Vessel.RV_LloydsNumber.IsEmpty) ? Declaration.Vessel.RV_LloydsNumber.ToString() : lloydsNumber;
					result = lloydsNumber + "*" + Declaration.Vessel.RV_Code.ToString();
				}

				return result;
			}
		}

		void FTXSegment()
		{
			FTXSegment fTX = Message.FTX.InstantiateAChildAndAddItToChildrenCollection();
			fTX.TextSubjectQualifier = TextSubjectQualifierList.EntireTransactionSet;
			fTX.TextLiteral.FreeText1 = VesselCodeAndLloydsNumber;
			fTX.TextLiteral.FreeText2 = GetNumberOfContainers(OneTEU);
			fTX.TextLiteral.FreeText3 = GetNumberOfContainers(TwoTEU);

			NoOfSegment++;
		}

		#region GetNumberOfContainers

		string GetNumberOfContainers(ZDecimal tEU)
		{
			int containerCount = 0;

			foreach (ForwardingContainer container in Shipment.Containers)
			{
				if (container.Container.RC_TEU == tEU)
				{
					containerCount++;
				}
			}

			return containerCount.ToString();
		}

		internal ZDecimal OneTEU = 1;
		internal ZDecimal TwoTEU = 2;

		#endregion

		#endregion

		#region RFFSegment

		void RFFSegment()
		{
			SegmentGroup1 group1 = Message.Group1.InstantiateAChildAndAddItToChildrenCollection();
			GenerateRFFSegment(group1, ReferenceQualifierList.BillOfLadingNumber, Declaration.JE_MasterBill);
			GenerateRFFSegment(group1, ReferenceQualifierList.CarriersReferenceNumber, ContainerMode);
			GenerateRFFSegment(group1, ReferenceQualifierList.BuyersOrderNumber, Shipment.JS_HouseBill);
			GenerateRFFSegment(group1, ReferenceQualifierList.InvoiceNumber, StandardInvoiceNumber);
			GenerateRFFSegment(group1, ReferenceQualifierList.JobNumber, Shipment.JobNumber);
			GenerateRFFSegment(group1, ReferenceQualifierList.OrderNumberPurchase, Shipment.DocsAndCartage.JP_OrderItemsAsString);
			GenerateRFFSegment(group1, ReferenceQualifierList.GovernmentReferenceNumber, GlbCompany.CurrentCompany.GC_BusinessRegNo.Replace(" ", ""));
		}

		void GenerateRFFSegment(SegmentGroup1 group1, ReferenceQualifierList qualifier, string reference)
		{
			RFFSegment rFF = group1.RFF.InstantiateAChildAndAddItToChildrenCollection();
			rFF.Reference.ReferenceQualifier = qualifier;
			rFF.Reference.ReferenceNumber = reference;
			NoOfSegment++;
		}

		#endregion

		#region NADSegment

		void NADSegment()
		{
			SegmentGroup2 group2 = Message.Group2.InstantiateAChildAndAddItToChildrenCollection();
			GenerateNADSegment(group2, PartyQualifierList.Vendor, Env.CurrentCompany.Name);
			GenerateNADSegment(group2, PartyQualifierList.Buyer, Declaration.Importer.OH_FullNameTruncated);

			LOCSegment lOC = group2.LOC.InstantiateAChildAndAddItToChildrenCollection();
			lOC.PlaceLocationQualifier = PlaceLocationQualifierList.PlaceOfDelivery;
			lOC.LocationIdentification.PlaceLocationIdentification = Shipment.JS_RL_NKOrigin;
			lOC.LocationIdentification.CodeListResponsibleAgencyCoded = CodeListResponsibleAgencyCodedList.UnEceUnitedNationsEconomicCommissionForEurope;
			NoOfSegment++;
		}

		void GenerateNADSegment(SegmentGroup2 group2, PartyQualifierList qualifier, string partyName)
		{
			NADSegment nAD = group2.NAD.InstantiateAChildAndAddItToChildrenCollection();
			nAD.PartyQualifier = qualifier;
			nAD.PartyIdentificationDetails.PartyIdIdentification = partyName;
			NoOfSegment++;
		}

		#endregion

		#region Group15Segment

		void Group15Segment()
		{
			VolALCSegement();
			DsbInvoiceALCSegment();
			StdInvoiceALCSegment();
		}

		#endregion

		#region Group25Segment

		void Group25Segment()
		{
			foreach (BaseJobComInvoiceHeader header in RelatedJobComInvoices)
			{
				foreach (BaseJobComInvoiceLine line in header.JobComInvoiceLines)
				{
					GenerateGroup25Segment(line, header.Invoice_Currency);
				}
			}
		}

		#endregion

		#region UNSSegment

		void UNSSegment()
		{
			UNSSegment uNS = Message.UNS.InstantiateAChildAndAddItToChildrenCollection();
			uNS.SectionIdentification = SectionIdentificationList.DetailSummarySectionSeparation;
			NoOfSegment++;
		}

		#endregion

		#region Group48Segment

		void Group48Segment()
		{
			SegmentGroup48 group48 = Message.Group48.InstantiateAChildAndAddItToChildrenCollection();

			GenerateMOASegment(group48.MOA.InstantiateAChildAndAddItToChildrenCollection(),
							   MonetaryAmountTypeQualifierList.DiscountAmount,
							   DeclarationValue.ToString(2));

			GenerateMOASegment(group48.MOA.InstantiateAChildAndAddItToChildrenCollection(),
							   MonetaryAmountTypeQualifierList.InvoiceAmount,
							   ExWorkAmount.ToString(2));

			GenerateMOASegment(group48.MOA.InstantiateAChildAndAddItToChildrenCollection(),
							   MonetaryAmountTypeQualifierList.PaymentDiscountAmount,
							   FOBAmount.ToString(2));

			GenerateMOASegment(group48.MOA.InstantiateAChildAndAddItToChildrenCollection(),
							   MonetaryAmountTypeQualifierList.AdjustmentToDebitFlow,
							   InvoicesGST(DsbInvoices));

			GenerateMOASegment(group48.MOA.InstantiateAChildAndAddItToChildrenCollection(),
							   MonetaryAmountTypeQualifierList.AdjustmentToCreditFlow,
							   InvoicesGST(StandardInvoices));

			GenerateMOASegment(group48.MOA.InstantiateAChildAndAddItToChildrenCollection(),
				   MonetaryAmountTypeQualifierList.TaxAmount,
				   DutyAmount.ToString(2));

			NoOfSegment += 6;
		}

		#endregion

		#region GenerateMOASegment

		void GenerateMOASegment(MOASegment mOA, MonetaryAmountTypeQualifierList qualifier, string amount)
		{
			mOA.MonetaryAmount.MonetaryAmountTypeQualifier = qualifier;
			mOA.MonetaryAmount.MonetaryAmount = amount;
		}

		#endregion

		#region GenerateGroup25Segment

		void GenerateGroup25Segment(BaseJobComInvoiceLine comInvLine, RefCurrency invoiceCurrency)
		{
			SegmentGroup25 group25 = Message.Group25.InstantiateAChildAndAddItToChildrenCollection();

			LINSegment lIN = group25.LIN.InstantiateAChildAndAddItToChildrenCollection();
			lIN.LineItemNumber = ActionRequestNotificationCodedList.Added;

			PIASegment pIA = group25.PIA.InstantiateAChildAndAddItToChildrenCollection();
			pIA.ProductIdFunctionQualifier = ProductIdFunctionQualifierList.AdditionalIdentification;
			pIA.ItemNumberIdentification1.ItemNumberTypeCoded = ItemNumberTypeCodedList.VendorsSellersPartNumber;
			pIA.ItemNumberIdentification1.ItemNumber = comInvLine.JI_PartNo;

			IMDSegment iMD = group25.IMD.InstantiateAChildAndAddItToChildrenCollection();
			iMD.ItemDescription.ItemDescription1 = comInvLine.JI_Description;

			SegmentGroup33 group33 = group25.Group33.InstantiateAChildAndAddItToChildrenCollection();
			TAXSegment tAX = group33.TAX.InstantiateAChildAndAddItToChildrenCollection();
			tAX.DutyTaxFeeFunctionQualifier = DutyTaxFeeFunctionQualifierList.Tax;
			tAX.DutyTaxFeeDetail.DutyTaxFeeRate = DutyRate(comInvLine);

			MOASegment mOA = group33.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeQualifier = MonetaryAmountTypeQualifierList.TaxAmount;
			mOA.MonetaryAmount.MonetaryAmount = comInvLine.JI_CustomsValue.ToString(2);

			NoOfSegment += 5;
		}

		#endregion

		#region abstract properties

		protected abstract string DutyRate(BaseJobComInvoiceLine line);
		protected abstract ZDecimal DutyAmount { get; }

		#endregion

		#region StdInvoiceALCSegment

		void StdInvoiceALCSegment()
		{
			SegmentGroup15 group15 = Message.Group15.InstantiateAChildAndAddItToChildrenCollection();
			GenerateALCMOASegment(group15, MonetaryAmountTypeQualifierList.OtherCharges, StdInvoiceTotalAmount);
		}

		#endregion

		#region VolALCSegement

		void VolALCSegement()
		{
			SegmentGroup15 group15 = Message.Group15.InstantiateAChildAndAddItToChildrenCollection();
			GenerateALCMOASegment(group15, MonetaryAmountTypeQualifierList.Insurance, Shipment.JS_ActualVolume.ToString(3));
			NoOfSegment++;
		}

		#endregion

		#region GenerateALCMOASegment

		void GenerateALCMOASegment(SegmentGroup15 group15, MonetaryAmountTypeQualifierList qualifier, string amount)
		{
			GenerateALCSegment(group15);

			SegmentGroup21 group21 = group15.Group21.InstantiateAChildAndAddItToChildrenCollection();
			MOASegment mOA = group21.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeQualifier = qualifier;
			mOA.MonetaryAmount.MonetaryAmount = amount;

			NoOfSegment++;
		}

		#endregion

		#region DsbInvoiceALCSegment

		void DsbInvoiceALCSegment()
		{
			foreach (InvoicingBase invoice in DsbInvoices)
			{
				foreach (InvoicingLineBase invoiceLine in invoice.Lines)
				{
					AccChargeCode chargeCode = Factory.Load<AccChargeCode>(invoiceLine.AL_AC);

					if (chargeCode != null && ShouldOutputThisCharge(chargeCode))
					{
						SegmentGroup15 group15 = Message.Group15.InstantiateAChildAndAddItToChildrenCollection();
						GenerateALCSegment(group15);
						GenerateMOARTESegment(group15, invoiceLine.AL_LocalExTaxAmount.ToString(2), chargeCode.AC_Code);
					}
				}
			}
		}

		#endregion

		#region GenerateMOARTESegment

		void GenerateMOARTESegment(SegmentGroup15 group15, string amount, string charge)
		{
			SegmentGroup19 group19 = group15.Group19.InstantiateAChildAndAddItToChildrenCollection();
			MOASegment mOA = group19.MOA.InstantiateAChildAndAddItToChildrenCollection();
			mOA.MonetaryAmount.MonetaryAmountTypeQualifier = MonetaryAmountTypeQualifierList.FreightCharge;
			mOA.MonetaryAmount.MonetaryAmount = amount;

			SegmentGroup20 group20 = group15.Group20.InstantiateAChildAndAddItToChildrenCollection();
			RTESegment rTE = group20.RTE.InstantiateAChildAndAddItToChildrenCollection();
			rTE.RateDetails.RateTypeQualifier = RateTypeQualifierList.ChargeRate;
			rTE.RateDetails.RatePerUnit = charge;
			NoOfSegment += 2;
		}

		#endregion

		#region GenerateALCSegment

		void GenerateALCSegment(SegmentGroup15 group15)
		{
			ALCSegment aLC = group15.ALC.InstantiateAChildAndAddItToChildrenCollection();
			aLC.AllowanceOrChargeQualifier = AllowanceOrChargeQualifierList.Charge;
			NoOfSegment++;
		}

		#endregion

		#region UNTSegment

		void UNTSegment(string messageNumber)
		{
			UNTSegment uNT = Message.UNT.InstantiateAChildAndAddItToChildrenCollection();
			uNT.NumberOfSegmentsInTheMessage = NoOfSegment.ToString();
			uNT.MessageReferenceNumber = messageNumber;
		}

		#endregion

		#endregion

		#region Values

		#region FOBAmount

		ZDecimal FOBAmount
		{
			get
			{
				ZDecimal amount = 0m;

				foreach (BaseJobComInvoiceHeader header in RelatedJobComInvoices)
				{
					amount = header.JZ_Calc_FOBAmountInLocalCurrency;
				}

				return amount;
			}
		}

		#endregion

		#region DeclarationValue

		ZDecimal DeclarationValue
		{
			get
			{
				ZDecimal amount = 0m;
				foreach (BaseJobComInvoiceHeader header in RelatedJobComInvoices)
				{
					foreach (BaseJobComInvoiceLine line in header.JobComInvoiceLines)
					{
						amount += line.JI_CustomsValue;
					}
				}

				return amount;
			}
		}

		#endregion

		#region ExWorkAmount

		ZDecimal ExWorkAmount
		{
			get
			{
				ZDecimal result = 0;

				if (Declaration.IncoTerm == Core.Constants.IncoTerms.FreeOnBoard)
				{
					result = FOBAmount;
				}
				else if (Declaration.IncoTerm == Core.Constants.IncoTerms.ExWorks)
				{
					foreach (BaseJobComInvoiceHeader header in RelatedJobComInvoices)
					{
						result += header.JZ_InvoiceAmountInLocalCurrency;
					}
				}

				return result;
			}
		}

		#endregion

		#region StdInvoiceTotalAmount

		string StdInvoiceTotalAmount
		{
			get
			{
				ZDecimal total = 0;

				foreach (InvoicingBase invoice in StandardInvoices)
				{
					total += invoice.AH_LocalExTaxAmount;
				}

				return total.ToString(2);
			}
		}

		#endregion

		#region Declaration

		protected BaseJobDeclaration Declaration
		{
			get { return InvoiceRecord.Declaration; }
		}

		#endregion

		#region Shipment'

		protected ForwardingShipment Shipment
		{
			get { return InvoiceRecord.Shipment; }
		}

		#endregion

		#region DisbursementInvoices

		InvoicingBase[] DsbInvoices
		{
			get
			{
				if (fDsbInvoices == null)
				{
					fDsbInvoices = new ArrayList();

					foreach (InvoicingBase invoice in InvoiceRecord.Invoices)
					{
						if (invoice.AH_IsDisbursementCalc)
						{
							fDsbInvoices.Add(invoice);
						}
					}
				}

				return (InvoicingBase[])fDsbInvoices.ToArray(typeof(InvoicingBase));
			}
		}

		ArrayList fDsbInvoices;

		#endregion

		#region StandardInvoices

		InvoicingBase[] StandardInvoices
		{
			get
			{
				if (fStandardInvoices == null)
				{
					fStandardInvoices = new ArrayList();
					foreach (InvoicingBase invoice in InvoiceRecord.Invoices)
					{
						if (!invoice.AH_IsDisbursementCalc)
						{
							fStandardInvoices.Add(invoice);
						}
					}
				}

				return (InvoicingBase[])fStandardInvoices.ToArray(typeof(InvoicingBase));
			}
		}

		ArrayList fStandardInvoices;

		#endregion

		#region InvoicesGST

		string InvoicesGST(InvoicingBase[] invoices)
		{
			ZDecimal result = 0;

			foreach (InvoicingBase invoice in invoices)
			{
				result += invoice.AH_LocalTaxAmount;
			}

			return result.ToString(2);
		}

		#endregion

		#region DisbursementInvoiceNumber

		string FirstDisbursementInvoiceNumber
		{
			get { return DsbInvoices[0].AH_TransactionNum.TrimStart('0'); }
		}

		#endregion

		#region StandardInvoiceNumber

		string StandardInvoiceNumber
		{
			get { return StandardInvoices[0].AH_TransactionNum.TrimStart('0'); }
		}

		#endregion

		#region RelatedJobComInvoices

		BaseJobComInvoiceHeader[] RelatedJobComInvoices
		{
			get
			{
				if (fJobComInvoices == null)
				{
					fJobComInvoices = new ArrayList();
					foreach (BaseJobComInvoiceHeader header in Declaration.Invoices)
					{
						if (IsJobComInvoiceLinkedToShipment(header))
						{
							fJobComInvoices.Add(header);
						}
					}
				}

				return (BaseJobComInvoiceHeader[])fJobComInvoices.ToArray(typeof(BaseJobComInvoiceHeader));
			}
		}

		#endregion

		ArrayList fJobComInvoices;

		string ContainerMode
		{
			get
			{
				ZString mode = (Shipment.IsAir) ? @"B/B" : Shipment.JS_PackingMode.ToString();

				return (mode == Core.Constants.ContainerModes.BuyersConsol) ? new ZString("FCX") : mode;
			}
		}

		#endregion

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			fInterchangeNumber = INVOICMessageNumberFountain.Instance.InterchangeNumber.GetNextFormatted(Factory);
		}

		protected virtual bool IsJobComInvoiceLinkedToShipment(BaseJobComInvoiceHeader invoice)
		{
			return true;
		}

		protected virtual bool ShouldOutputThisCharge(AccChargeCode chargeCode)
		{
			return true;
		}

		#region Utilities

		Utilities Utilities
		{
			get
			{
				if (fUtilities == null)
				{
					fUtilities = new Utilities();
				}

				return fUtilities;
			}
		}

		Utilities fUtilities;

		#endregion

		readonly INVOICMessage Message;
		protected JobInvoiceRecord InvoiceRecord;
		string fInterchangeNumber;
		int NoOfSegment;
	}
}

#region Implementation
#region InvoiceRecord
#endregion
#region SetupDeclaration
#endregion
#region SetupOrganisation
#endregion
#region CreateInvoiceLine
#endregion
#region Factory
#endregion
#endregion
