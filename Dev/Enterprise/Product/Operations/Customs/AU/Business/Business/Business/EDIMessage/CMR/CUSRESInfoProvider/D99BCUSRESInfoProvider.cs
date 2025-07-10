using System;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class D99BCUSRESInfoProvider
	{
		public D99BCUSRESInfoProvider(CUSRESMessage edifactMessage)
		{
			if (edifactMessage == null)
			{
				throw new ArgumentNullException(nameof(edifactMessage));
			}
			CUSRES = edifactMessage;
		}

		#region DocumentName

		public abstract ZString DocumentName { get; }

		#endregion

		#region Version Number

		public ZString VersionNumber
		{
			get
			{
				if (fVersionNumber.IsEmpty)
				{
					foreach (BGMSegment bGM in CUSRES.BGM)
					{
						if (!string.IsNullOrEmpty(bGM.DocumentMessageIdentification.Version))
						{
							fVersionNumber = bGM.DocumentMessageIdentification.Version;
							break;
						}
					}
				}

				return fVersionNumber;
			}
		}
		ZString fVersionNumber;

		#endregion

		#region EntryNumber

		public ZString EntryNumber
		{
			get
			{
				if (fEntryNumber.IsEmpty)
				{
					foreach (SegmentGroup3 currentGroup3 in CUSRES.Group3)
					{
						if (currentGroup3.RFF[0].Reference.ReferenceFunctionCodeQualifier.ToString() == ReferenceFunctionCodeQualifierList.CustomsDeclarationNumber.ToString())
						{
							fEntryNumber = currentGroup3.RFF[0].Reference.ReferenceIdentifier;
							break;
						}
					}
				}
				return fEntryNumber;
			}
		}
		ZString fEntryNumber;

		#endregion

		#region Payment Finalised Date

		public ZString PaymentFinalisedDateString
		{
			get { return PaymentFinalisedDate.IsValid ? PaymentFinalisedDate.ToShortDateString() : ""; }
		}

		public ZDateTime PaymentFinalisedDate
		{
			get
			{
				return PaymentFinalisedDateCore;
			}
		}

		protected virtual ZDateTime PaymentFinalisedDateCore
		{
			get
			{
				if (fPaymentFinalisedDate.IsEmpty)
				{
					foreach (DTMSegment dTM in CUSRES.DTM)
					{
						if (dTM.DateTimePeriod.DateTimePeriodFunctionCodeQualifier == DateTimePeriodFunctionCodeQualifierList.PaymentDate)
						{
							fPaymentFinalisedDate = GetDate(dTM);
							break;
						}
					}
				}
				return fPaymentFinalisedDate;
			}
		}
		ZDateTime fPaymentFinalisedDate;

		protected ZDateTime GetDate(DTMSegment dTM)
		{
			ZDateTime result = ZDateTime.Empty;
			ZDateTime.TryParseExact(dTM.DateTimePeriod.DateTimePeriodValue, out result, "yyyyMMdd");
			return result;
		}

		#endregion

		#region Monetary Amounts

		protected ZDecimal TotalPayableAdminCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fTotalPayableAdmin;
			}
		}

		protected ZDecimal AQISContainerChargeCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fAQISContainerCharge;
			}
		}

		protected ZDecimal AQISProcessingChargeCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fAQISProcessingCharge;
			}
		}

		protected ZDecimal DeclarationProcessingChargeCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fDeclarationProcessingCharge;
			}
		}

		protected ZDecimal TotalWoodLevyCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fTotalWoodLevy;
			}
		}

		protected ZDecimal TotalTILVCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fTotalTILV;
			}
		}

		protected ZDecimal TotalPayableCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fTotalPayableOnThisSession;
			}
		}

		protected ZDecimal AQISServicePaymentCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fAQISServicePayment;
			}
		}

		protected ZDecimal TotalPayableDutyCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fTotalPayableDuty;
			}
		}

		protected ZDecimal TotalPayableWETCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fTotalPayableWET;
			}
		}

		protected ZDecimal TotalPayableLCTCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fTotalPayableLCT;
			}
		}

		protected ZDecimal TotalPayableGSTCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fTotalPayableGST;
			}
		}

		protected ZDecimal TotalDeferredGSTCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fTotalDeferredGST;
			}
		}

		protected ZDecimal TotalOtherChargesCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fTotalOtherCharges;
			}
		}

		protected ZDecimal TotalSecurityConcessionCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fTotalSecurityConcession;
			}
		}

		protected ZDecimal TotalSecurityLiabilityCore
		{
			get
			{
				ProcessMonetaryInformation();
				return fTotalSecurityLiability;
			}
		}
		#endregion

		#region Process Monetary Information

		ZDecimal fAQISContainerCharge;
		ZDecimal fAQISProcessingCharge;
		ZDecimal fDeclarationProcessingCharge;
		ZDecimal fTotalPayableAdmin;
		ZDecimal fTotalWoodLevy;
		ZDecimal fTotalTILV;
		ZDecimal fTotalPayableOnThisSession;
		ZDecimal fAQISServicePayment;
		ZDecimal fTotalPayableDuty;
		ZDecimal fTotalPayableWET;
		ZDecimal fTotalPayableLCT;
		ZDecimal fTotalPayableGST;
		ZDecimal fTotalOtherCharges;
		ZDecimal fTotalDeferredGST;
		ZDecimal fTotalSecurityConcession;
		ZDecimal fTotalSecurityLiability;

		void ProcessMonetaryInformation()
		{
			if (!isMonetaryInformationCalculated)
			{
				if (CUSRES != null)
				{
					foreach (SegmentGroup5 group5 in CUSRES.Group5)
					{
						if (group5.MOA[0].MonetaryAmount.MonetaryAmountTypeCodeQualifier != null)
						{
							switch (group5.MOA[0].MonetaryAmount.MonetaryAmountTypeCodeQualifier.ToString())
							{
								case "35":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fAQISContainerCharge);
									break;
								case "26":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fAQISProcessingCharge);
									break;
								case "23":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fDeclarationProcessingCharge);
									break;
								case "304":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fTotalOtherCharges);
									break;
								case "7":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fTotalPayableAdmin);
									break;
								case "58":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fTotalWoodLevy);
									break;
								case "68":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fTotalTILV);
									break;
								case "128":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fTotalPayableOnThisSession);
									break;
								case "206":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fAQISServicePayment);
									break;
								case "9":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fTotalPayableDuty);
									break;
								case "149":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fTotalPayableWET);
									break;
								case "369":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fTotalPayableGST);
									break;
								case "371":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fTotalPayableLCT);
									break;
								case "210":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fTotalDeferredGST);
									break;
								case "292":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fTotalSecurityConcession);
									break;
								case "Z01":
									ZDecimal.TryParse(group5.MOA[0].MonetaryAmount.MonetaryAmountValue, out fTotalSecurityLiability);
									break;
							}
						}
					}
				}
				isMonetaryInformationCalculated = true;
			}
		}
		bool isMonetaryInformationCalculated;

		#endregion

		#region Implementation

		protected readonly CUSRESMessage CUSRES;

		#endregion
	}
}
