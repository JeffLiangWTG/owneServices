using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class PAYRECInfoProvider : D99BCUSRESInfoProvider
	{
		public PAYRECInfoProvider(CUSRESMessage edifactMessage)
			: base(edifactMessage)
		{
		}

		#region DocumentName

		public override ZString DocumentName
		{
			get { return "PAYREC"; }
		}

		#endregion

		#region EFTRunNumber

		public ZString EFTRunNumber
		{
			get
			{
				if (fEFTRunNumber.IsEmpty)
				{
					if (CUSRES != null)
					{
						foreach (SegmentGroup3 currentGroup3 in CUSRES.Group3)
						{
							if (currentGroup3.RFF[0].Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.BanksCommonTransactionReferenceNumber)
							{
								fEFTRunNumber = currentGroup3.RFF[0].Reference.ReferenceIdentifier;
								break;
							}
						}
					}
				}
				return fEFTRunNumber;
			}
		}
		ZString fEFTRunNumber;

		#endregion

		#region ICSReceiptNumber

		public ZString ICSReceiptNumber
		{
			get
			{
				if (fICSReceiptNumber.IsEmpty)
				{
					if (CUSRES != null)
					{
						foreach (SegmentGroup3 currentGroup3 in CUSRES.Group3)
						{
							if (currentGroup3.RFF[0].Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.RemittanceAdviceNumber)
							{
								fICSReceiptNumber = currentGroup3.RFF[0].Reference.ReferenceIdentifier;
								break;
							}
						}
					}
				}
				return fICSReceiptNumber;
			}
		}
		ZString fICSReceiptNumber;

		#endregion

		#region Bank Details

		public ZString BankAccountName
		{
			get
			{
				ProcessBankDetailsIfNotDone();
				return fBankAccountName;
			}
		}
		ZString fBankAccountName;

		public ZString BankAccountNumber
		{
			get
			{
				ProcessBankDetailsIfNotDone();
				return fBankAccountNumber;
			}
		}
		ZString fBankAccountNumber;

		public ZString BSBNumber
		{
			get
			{
				ProcessBankDetailsIfNotDone();
				return fBSBNumber;
			}
		}
		ZString fBSBNumber;

		void ProcessBankDetailsIfNotDone()
		{
			if (!hasBankDetailBeenProcessed)
			{
				if (CUSRES != null)
				{
					foreach (SegmentGroup1 currentGroup1 in CUSRES.Group1)
					{
						if (currentGroup1.NAD[0].PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.AccountOf)
						{
							fBankAccountNumber = currentGroup1.NAD[0].PartyIdentificationDetails.PartyIdentifier;
							fBankAccountName = currentGroup1.NAD[0].PartyName.PartyName1 + " " + currentGroup1.NAD[0].PartyName.PartyName2;
						}
						else if (currentGroup1.NAD[0].PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.NominatedBank)
						{
							fBSBNumber = currentGroup1.NAD[0].PartyIdentificationDetails.PartyIdentifier;
						}
					}
				}
				hasBankDetailBeenProcessed = true;
			}
		}
		bool hasBankDetailBeenProcessed;

		#endregion

		#region Monetary Amounts

		public ZDecimal TotalPayableAdmin
		{
			get { return TotalPayableAdminCore; }
		}

		public ZDecimal AQISContainerCharge
		{
			get { return AQISContainerChargeCore; }
		}

		public ZDecimal AQISProcessingCharge
		{
			get { return AQISProcessingChargeCore; }
		}

		public ZDecimal DeclarationProcessingCharge
		{
			get { return DeclarationProcessingChargeCore; }
		}

		public ZDecimal TotalWoodLevy
		{
			get { return TotalWoodLevyCore; }
		}

		public ZDecimal TotalTILV
		{
			get { return TotalTILVCore; }
		}

		public ZDecimal TotalPayable
		{
			get { return TotalPayableCore; }
		}

		public ZDecimal AQISServicePayment
		{
			get { return AQISServicePaymentCore; }
		}

		public ZDecimal TotalPayableDuty
		{
			get { return TotalPayableDutyCore; }
		}

		public ZDecimal TotalPayableWET
		{
			get { return TotalPayableWETCore; }
		}

		public ZDecimal TotalPayableLCT
		{
			get { return TotalPayableLCTCore; }
		}

		public ZDecimal TotalPayableGST
		{
			get { return TotalPayableGSTCore; }
		}

		public ZDecimal TotalOtherCharges
		{
			get { return TotalOtherChargesCore; }
		}

		#endregion
	}
}
