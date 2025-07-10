using System;
using System.Data;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Edifact.D99B.Messages.CUSRES;
using Enterprise.Edifact.D99B.Segments;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CMRPAYRECMessage : CMRImportDeclarationMessage
	{
		public CMRPAYRECMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_MessageType = CMRMessageTypes.PAYREC;
		}

		protected override ZString GetStatusCore()
		{
			return GetReferenceFromSendersReference(SendersReference);
		}

		public PAYRECInfoProvider PAYRECInfoProvider
		{
			get
			{
				if (fPAYRECInfoProvider == null)
				{
					fPAYRECInfoProvider = new PAYRECInfoProvider(CUSRES);
				}
				return fPAYRECInfoProvider;
			}
		}
		PAYRECInfoProvider fPAYRECInfoProvider;

		public CusEntryHeader EntryHeader
		{
			get { return (CusEntryHeader)EM_LinkedObject; }
		}

		public override ZString GetReport()
		{
			if (CUSRES != null)
			{
				StringBuilder reportBuilder = new StringBuilder();

				if (EM_LinkedObject != null)
				{
					reportBuilder.Append(GetEM_LinkedObjectDetails(false));
					reportBuilder.Append("\r\n");
				}

				reportBuilder.Append("Payment Date: " + CUSRES.DTM[0].DateTimePeriod.DateTimePeriodValue + "\r\n");
				reportBuilder.Append("\r\nEFT Run Number: " + PAYRECInfoProvider.EFTRunNumber);
				reportBuilder.Append("\r\nICS Receipt Number: " + PAYRECInfoProvider.ICSReceiptNumber);
				reportBuilder.Append("\r\nAmounts:\r\n");

				foreach (SegmentGroup5 currentGroup5 in CUSRES.Group5)
				{
					MOASegment currentMOA = currentGroup5.MOA[0];
					decimal decimalAmount = Convert.ToDecimal(currentMOA.MonetaryAmount.MonetaryAmountValue);
					string amount = decimalAmount.ToString();

					switch (currentMOA.MonetaryAmount.MonetaryAmountTypeCodeQualifier.ToString())
					{
						case "304":
							reportBuilder.Append("\tTotal Other Charges: " + amount + "\r\n");
							break;
						case "128":
							reportBuilder.Append("\tTotal Paid Amount: " + amount + "\r\n");
							break;
						case "7":
							reportBuilder.Append("\tTotal Payable Admin: " + amount + "\r\n");
							break;
						case "9":
							reportBuilder.Append("\tTotal Payable Duty: " + amount + "\r\n");
							break;
						case "369":
							reportBuilder.Append("\tTotal Payable GST: " + amount + "\r\n");
							break;
						case "371":
							reportBuilder.Append("\tTotal Payable LCT: " + amount + "\r\n");
							break;
						case "149":
							reportBuilder.Append("\tTotal Payable WET: " + amount + "\r\n");
							break;
						case "58":
							reportBuilder.Append("\tTotal WoodLevy: " + amount + "\r\n");
							break;
						case "26":
							reportBuilder.Append("\tAQIS Processing Charge: " + amount + "\r\n");
							break;
						case "23":
							reportBuilder.Append("\tDeclaration Processing Charge: " + amount + "\r\n");
							break;
						case "55":
							reportBuilder.Append("\tLine Actual Duty: " + amount + "\r\n");
							break;
						case "206":
							reportBuilder.Append("\tAQIS Services Amount: " + amount + "\r\n");
							break;
						case "292":
							reportBuilder.Append("\tTotal Security Concession Amount: " + amount + "\r\n");
							break;
						case "Z01":
							reportBuilder.Append("\tTotal Security Uncollected Amount: " + amount + "\r\n");
							break;
					}
				}

				reportBuilder.Append(GetErrorsSection(ZString.Empty));

				if (EM_LinkedObject != null)
				{
					reportBuilder.Append(StatusOfLinesReport);
				}
				return reportBuilder.ToString();
			}
			return ZString.Empty;
		}

#if DEBUG

		public CUSRESMessage CUSRESEdifactMessageForTestOnly
		{
			get { return CUSRES; }
		}

#endif
	}
}
