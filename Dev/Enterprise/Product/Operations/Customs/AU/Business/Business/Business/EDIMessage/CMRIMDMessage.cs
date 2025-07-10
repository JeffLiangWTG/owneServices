using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRIMDMessage : CMRCUSDECMessage, IPaymentIncluded, IMessageFunctionCode
	{
		public CMRIMDMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region IsPreLodgeMessage

		public bool IsPreLodgeMessage
		{
			get
			{
				if (needToRefreshIsPreLodgeMessage)
				{
					if (CUSDEC != null)
					{
						foreach (GISSegment gIS in CUSDEC.GIS)
						{
							if (gIS.ProcessingIndicator_X.ProcessingIndicatorDescriptionCode.ToString() == "PRE")
							{
								fIsPreLodgeMessage = true;
								break;
							}
						}
					}
					needToRefreshIsPreLodgeMessage = false;
				}
				return fIsPreLodgeMessage;
			}
		}
		bool needToRefreshIsPreLodgeMessage = true;
		bool fIsPreLodgeMessage;

		#endregion

		#region IsOriginalMessage

		public bool IsOriginalMessage
		{
			get { return RevisionNumber == "9"; }
		}

		#endregion

		#region IsAmendmentMessage

		public bool IsAmendmentMessage
		{
			get { return RevisionNumber == "4"; }
		}

		#endregion

		#region MaxLineNumber

		public ZShort MaxLineNumber
		{
			get
			{
				if (maxLineNumber == null)
				{
					var result = ZShort.Zero;

					var lines = CUSDEC
						?.Group10
						?.Cast<Edifact.D99B.Messages.CUSDEC.SegmentGroup10>()
						?.SelectMany(c => c.Group21?.Cast<Edifact.D99B.Messages.CUSDEC.SegmentGroup21>())
						?.SelectMany(c => c.LIN?.Cast<LINSegment>())
						?? Enumerable.Empty<LINSegment>();

					foreach (var line in lines)
					{
						var actionCode = line.ActionRequestNotificationDescriptionCode?.ToString() ?? string.Empty;
						if (ZShort.TryParse(line.LineItemNumber, out var lineNumber) && (actionCode.Equals(LineAction.Amend) || actionCode.Equals(LineAction.Insert)))
						{
							result = Math.Max(result, lineNumber);
						}
					}

					maxLineNumber = result;
				}

				return maxLineNumber.Value;
			}
		}

		ZShort? maxLineNumber;

		#endregion

		#region IPaymentIncluded Members

		GISSegmentMessageSection IPaymentIncluded.GISSegments
		{
			get { return CUSDEC != null ? CUSDEC.GIS : null; }
		}

		bool IPaymentIncluded.IsPaymentIncluded
		{
			get { return IsPaymentIncludedCalculator.IsPaymentIncluded; }
		}

		IsPaymentIncludedCalculator IsPaymentIncludedCalculator
		{
			get
			{
				if (fIsPaymentIncludedCalculator == null)
				{
					fIsPaymentIncludedCalculator = new IsPaymentIncludedCalculator(this);
				}
				return fIsPaymentIncludedCalculator;
			}
		}
		IsPaymentIncludedCalculator fIsPaymentIncludedCalculator;

		#endregion

		#region Bank details included in message

		public ZString BankAccountNoInMessage
		{
			get
			{
				CalculateBankingInfo();
				return bankAccountNoInMessage;
			}
		}

		public ZString BSBInMessage
		{
			get
			{
				CalculateBankingInfo();
				return bSBInMessage;
			}
		}

		public ZString BankAccountNameInMessage
		{
			get
			{
				CalculateBankingInfo();
				return bankAccountNameInMessage;
			}
		}

		public PaymentParty PaymentPartyInMessage
		{
			get
			{
				CalculateBankingInfo();
				return paymentPartyInMessage;
			}
		}

		void CalculateBankingInfo()
		{
			if (!isBankingInfoCalculated)
			{
				if (CUSDEC != null)
				{
					if (CUSDEC.FII.Count > 0)
					{
						bankAccountNoInMessage = CUSDEC.FII[0].AccountHolderIdentification.AccountHolderNumber;
						bSBInMessage = CUSDEC.FII[0].InstitutionIdentification.InstitutionBranchNumber;
						bankAccountNameInMessage = CUSDEC.FII[0].AccountHolderIdentification.AccountHolderName1 + CUSDEC.FII[0].AccountHolderIdentification.AccountHolderName2;
					}
					bool breakNow = false;
					foreach (Edifact.D99B.Messages.CUSDEC.SegmentGroup1 group1 in CUSDEC.Group1)
					{
						foreach (RFFSegment rFF in group1.RFF)
						{
							if (rFF.Reference.ReferenceFunctionCodeQualifier == ReferenceFunctionCodeQualifierList.FinancialTransactionReferenceNumber)
							{
								switch (rFF.Reference.ReferenceIdentifier)
								{
									case "B":
										paymentPartyInMessage = PaymentParty.Broker;
										breakNow = true;
										break;
									case "I":
										paymentPartyInMessage = PaymentParty.Importer;
										breakNow = true;
										break;
								}
							}
							if (breakNow)
							{
								break;
							}
						}
						if (breakNow)
						{
							break;
						}
					}
				}
				isBankingInfoCalculated = true;
			}
		}
		bool isBankingInfoCalculated;
		ZString bankAccountNoInMessage;
		ZString bSBInMessage;
		ZString bankAccountNameInMessage;
		PaymentParty paymentPartyInMessage;

		#endregion

		#region Implementation

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.IMD;
		}

		#endregion
	}
}
