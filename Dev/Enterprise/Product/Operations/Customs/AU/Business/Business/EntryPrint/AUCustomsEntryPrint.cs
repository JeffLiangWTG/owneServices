using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.AU;
using Enterprise.Environment;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	/// <summary>
	/// AUCustomsEntryPrint generates a text representation of an entry.
	/// </summary>
	public class AUCustomsEntryPrint
	{
		public AUCustomsEntryPrint(CusEntryHeader entryHeader, bool isPortrait)
		{
			this.entryHeader = entryHeader;
			this.isPortrait = isPortrait;
			fEntryPrint = new TextLayout();
			firstPageFooter = new TextLayout();
			includeAQISServicePaymentsInTotalPayableOnEntryPrint = AUCustomsDataRegistry.Instance.IncludeAQISServicePaymentsInTotalPayableOnEntryPrint.Value;
		}

		public void Generate()
		{
			isEntryNormalised = entryHeader.NormalisationConditionChecker.ShouldEntryBeNormalised;
			hasFirstPageFooterBeenPrinted = false;
			fEntryPrint.Clear();
			firstPageFooter.Clear();
			firstPageFooter.Print(GetFirstPageFooter());

			fEntryPrint.Print(Heading);
			fEntryPrint.Print(InvoiceHeaderAndCustomsValue);
			if (isPortrait)
			{
				fEntryPrint.CRLF();
				fEntryPrint.CRLF();
			}

			PrintLinesBody();
			if (isPortrait)
			{
				fEntryPrint.CRLF();
				fEntryPrint.CRLF();
			}

			PrintPackagesBillsContainers();
			PrintAmberReasonAndStatement();
			PrintPaidUnderProtestStatement();
			if (!hasFirstPageFooterBeenPrinted)
			{
				PrintFirstPageFooter(LinesPerPage - fEntryPrint.Collection.Count);
			}

			fEntryPrint.Print(EndOfEntry);
			fEntryPrint.Print(PrintToPageBreak);
			fGenerated = true;
		}

		public bool Generated
		{
			get { return fGenerated; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1054:DoNotHardcodePaths", Justification = "Not sure what this is meant to be using")]
		public void PrintToTestFile()
		{
			using (System.IO.FileStream fileStream = new System.IO.FileStream("c:\\ENTRY.TXT", System.IO.FileMode.Create, System.IO.FileAccess.ReadWrite))// Not sure what this is meant to be using
			{
				fileStream.Write(System.Text.Encoding.ASCII.GetBytes(fEntryPrint.Text), 0, fEntryPrint.Text.Length);
			}
		}

		public ZString EntryPrint
		{
			get
			{
				if (!fGenerated)
				{
					Generate();
				}
				return fEntryPrint.Text;
			}
		}

		#region Implementation

		protected CusEntryHeader entryHeader;
		protected bool isPortrait;
		protected TextLayout fEntryPrint;
		protected TextLayout firstPageFooter;
		protected int pageNumber;
		protected int headerLines = 16;
		protected int linesToPrint;
		bool fGenerated;
		bool hasFirstPageFooterBeenPrinted;
		readonly bool includeAQISServicePaymentsInTotalPayableOnEntryPrint;
		protected bool isEntryNormalised;

		public ZBool IncludeAQISServicePaymentsInTotalPayableOnEntryPrint
		{
			get { return includeAQISServicePaymentsInTotalPayableOnEntryPrint; }
		}

		protected int LinesPerPage
		{
			get { return isPortrait ? 98 : 68; }
		}

		protected int FooterLines
		{
			get { return firstPageFooter.VerticalPosition; }
		}

		protected int EndOfEntryLines
		{
			get { return EndOfEntry.VerticalPosition; }
		}

		protected bool IsCMREntry
		{
			get { return (entryHeader.IsCMRNature1020 || entryHeader.IsCMRNature10 || entryHeader.IsCMRNature20 || entryHeader.IsCMRNature30); }
		}

		protected bool HasWarehouseData
		{
			get { return (entryHeader.IsNature20 || entryHeader.IsNature1020 || entryHeader.IsNature30); }
		}

		protected TextLayout NoDeclaration
		{
			get
			{
				TextLayout noDec = new TextLayout();
				noDec.PrintAt(1, 1, "Entry cannot be printed: The declaration has not yet been made/returned");
				return noDec;
			}
		}

		int headerPrintAt;

		public ZString EntryNumber
		{
			get
			{
				var entryNo = entryHeader.EntryNumber;

				if (!IsCMREntry && entryNo.Length > 6)
				{
					entryNo = entryNo.Substring(0, 2) + "." + entryNo.Substring(2, 4) + "." + entryNo.Substring(6);
				}

				return entryNo;
			}
		}

		public ZString DestinationPort
		{
			get
			{
				var result = ZString.Empty;
				if (!IsCMREntry || entryHeader.IsCMRNature30)
				{
					if (entryHeader.Declaration != null && entryHeader.Declaration.FinalDestination != null)
					{
						result = entryHeader.Declaration.FinalDestination.RL_PortName.ToUpper();
					}
					else
					{
						result = "INVALID";
					}
				}
				return result;
			}
		}

		protected TextLayout Heading
		{
			get
			{
				TextLayout header = new TextLayout();
				header.PrintAt(1, 1, "ENTRY PRINT");
				if (IsCMREntry)
				{
					PrintCMRHeader(header);
				}

				PrintNatureSpecificHeadings(header);
				header.PrintAt(6, 57, "PREPARED " + CustomsTime(CurrentDateTime));
				header.PrintAt(7, 106, "COPYNUM: ");

				pageNumber += 1;
				header.PrintAt(1, 95, "PAGE " + pageNumber.ToString());
				header.PrintAt(3, 95, "ENTRY NO. " + EntryNumber + "  PRINT ");
				if (!DestinationPort.IsEmpty)
				{
					header.PrintAt(5, 95, "DESTINATION PORT: " + DestinationPort);
				}

				int nextVertical = isPortrait ? 10 : 8;
				header.PrintAt(nextVertical, 1, OwnerDetails);
				header.PrintAt(nextVertical, 42, AgencyDetails);
				header.PrintAt(nextVertical, 95, ShippingDetails);

				nextVertical += 6;
				if (isPortrait)
				{
					nextVertical += 2;
				}

				if (pageNumber == 1)
				{
					PrintFirstPageHeadings(header, nextVertical);
				}

				return header;
			}
		}

		void PrintCMRHeader(TextLayout header)
		{
			if (entryHeader.CMREntryMayHaveChangedPostLodge)
			{
				header.PrintAt(1, 13, "- ESTIMATE");
			}

			int headerMaxLength = (entryHeader.IsNature1020) ? 30 : 40;
			headerPrintAt = 1;
			ZStringBuilder entryStatusTextBuilder = new ZStringBuilder();
			var declaration = entryHeader.Declaration;
			if (declaration != null)
			{
				entryStatusTextBuilder.Append(entryHeader.EntryHeaderStatusDescription);
				if (declaration.HasOutstandingAmendment)
				{
					entryStatusTextBuilder.Append("(Amendment detected, NOT Lodged.) ");
				}
				else if (declaration.HasOutstandingAmendmentNotQueued)
				{
					entryStatusTextBuilder.Append("(Saved without sending amendment, NOT Lodged.) ");
				}
				else if (declaration.HasOutstandingManualAmendments)
				{
					entryStatusTextBuilder.Append(declaration.OutstandingManualAmendmentDescription);
				}
				else if (declaration.HasOutstandingFailedAmendments)
				{
					entryStatusTextBuilder.Append(declaration.AmendmentFailedDescription);
				}
			}

			PrintLongStringsInMultipleLinesWrapAtSpace(entryStatusTextBuilder.ToString(), header, headerMaxLength);
			PrintLongStringsInMultipleLinesWrapAtSpace(LastMessage, header, headerMaxLength);
		}

		public ZString LastMessageContent
		{
			get
			{
				ZString msgStatus = GetMsgStatus().Text;
				ZStringBuilder msgStatusAndDate = new ZStringBuilder();
				msgStatusAndDate.Append(msgStatus);
				ZString lastMsgDateStr = ZString.Empty;
				if (entryHeader.Messages.LastMessage != null)
				{
					lastMsgDateStr = entryHeader.Messages.LastMessage.EM_SystemCreateTimeUtc.ToString("ddMMMyy HH:mm", CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture);
					msgStatusAndDate.Append(string.Format(CultureInfo.CurrentCulture, "({0}UTC)", lastMsgDateStr));
				}
				return msgStatusAndDate.ToString();
			}
		}

		public ZString LastMessage
		{
			get
			{
				return "Last Msg: " + LastMessageContent;
			}
		}

		void PrintNatureSpecificHeadings(TextLayout header)
		{
			if (entryHeader.IsNature10)
			{
				header.PrintAt(1, 42, "X  XXXXXX    ******************************");
				header.PrintAt(2, 42, "X  X    X    *     AUSTRALIAN CUSTOMS     *");
				header.PrintAt(3, 42, "X  X    X    * ENTRY FOR HOME CONSUMPTION *");
				header.PrintAt(4, 42, "X  X    X    ******************************");
				header.PrintAt(5, 42, "X  XXXXXX");
			}
			else if (entryHeader.IsNature20)
			{
				header.PrintAt(1, 42, "XXXXX  XXXXXX    *************************");
				header.PrintAt(2, 42, "    X  X    X    *   AUSTRALIAN CUSTOMS  *");
				header.PrintAt(3, 42, "XXXXX  X    X    * ENTRY FOR WAREHOUSING *");
				header.PrintAt(4, 42, "X      X    X    *************************");
				header.PrintAt(5, 42, "XXXXX  XXXXXX");
			}
			else if (entryHeader.IsNature30)
			{
				header.PrintAt(1, 42, "XXXXX  XXXXXX    ************************");
				header.PrintAt(2, 42, "    X  X    X    *  AUSTRALIAN CUSTOMS  *");
				header.PrintAt(3, 42, "XXXXX  X    X    * EX-WAREHOUSING ENTRY *");
				header.PrintAt(4, 42, "    X  X    X    ************************");
				header.PrintAt(5, 42, "XXXXX  XXXXXX");
			}
			else if (entryHeader.IsNature1020)
			{
				header.PrintAt(1, 32, "X  XXXXXX    XXXXX  XXXXXX ******************************");
				header.PrintAt(2, 32, "X  X    X        X  X    X *     AUSTRALIAN CUSTOMS     *");
				header.PrintAt(3, 32, "X  X    X  / XXXXX  X    X * ENTRY FOR HOME CONSUMPTION/*");
				header.PrintAt(4, 32, "X  X    X    X      X    X *    ENTRY FOR WAREHOUSING   *");
				header.PrintAt(5, 32, "X  XXXXXX    XXXXX  XXXXXX ******************************");
			}
		}

		void PrintFirstPageHeadings(TextLayout header, int nextVertical)
		{
			if (IsCMREntry)
			{
				if (HasWarehouseData)
				{
					header.PrintAt(nextVertical, 1, WarehouseDetails);
				}
			}
			else
			{
				header.PrintAt(nextVertical, 1, SupplierDetails);
			}

			if (entryHeader.Declaration != null && entryHeader.Declaration.IsSOFADeclaration)
			{
				header.PrintAt(nextVertical, 1, SOFAIndicator);
			}

			header.PrintAt(nextVertical, 42, AmountDetails);
			header.PrintAt(nextVertical, 95, PortDetails);
		}

		#region Get Entry Status

		internal TextLayout GetMsgStatus()
		{
			TextLayout layout = new TextLayout();
			ZStringBuilder status = new ZStringBuilder();

			if (entryHeader.CH_Status == CustomsEntryStatus.AwaitingSAC.Code)
			{
				status.Append("SAC PENDING");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.AwaitingFormalLodge.Code)
			{
				status.Append(CMREntryPaymentStatusList.IsPaymentMessageSentOrCleared(entryHeader.AddInfo.ZA_PaymentStatus_Hidden) ? "LODGE WITH PAY PENDING" : "LODGE WITHOUT PAY PENDING");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.AwaitingPreLodge.Code)
			{
				status.Append("PRE-LODGE PENDING");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.AwaitingPayment.Code)
			{
				status.Append(GetLastMessageType());
				status.Append(" PAYMENT PENDING");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.AwaitingAmendment.Code)
			{
				status.Append("AMENDMENT PENDING");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.AwaitingWithdrawal.Code)
			{
				status.Append("WITHDRAWAL PENDING");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.FailSAC.Code)
			{
				status.Append("SAC FAILED");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.FailFormalLodge.Code)
			{
				status.Append("LODGE FAILED");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.FailPreLodge.Code)
			{
				status.Append("PRE-LODGE FAILED");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.FailPayment.Code)
			{
				status.Append(GetLastMessageType());
				status.Append(" PAYMENT FAILED");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.FailAmendment.Code)
			{
				status.Append("AMENDMENT FAILED");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.FailWithdrawal.Code)
			{
				status.Append("WITHDRAWAL FAILED");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.ClearSAC.Code)
			{
				status.Append("SAC");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.ClearFormalLodge.Code)
			{
				status.Append("LODGE");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.ClearPreLodge.Code)
			{
				status.Append("PRE-LODGE");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.ClearPayment.Code)
			{
				status.Append(GetLastMessageType());
				status.Append(" PAYMENT");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.ClearAmendment.Code)
			{
				status.Append("AMENDMENT");
			}
			else if (entryHeader.CH_Status == CustomsEntryStatus.ClearWithdrawal.Code)
			{
				status.Append("WITHDRAWAL");
			}
			else
			{
				status.Append("NONE");
			}
			layout.Print(status.ToString());
			return layout;
		}

		string GetLastMessageType()
		{
			string result = "";
			if (entryHeader.Declaration.IsSAC)
			{
				CMRSACMessage sACMessage = entryHeader.Messages.GetLastMessage(ZString.Empty, CMRMessage.CMRMessageTypes.SAC, "TRX") as CMRSACMessage;
				result = (sACMessage == null || sACMessage.IsOriginalMessage) ? "SAC" : "AMENDMENT";
			}
			else
			{
				CMRIMDMessage tRXMessage = entryHeader.Messages.GetLastMessage(ZString.Empty, CMRMessage.CMRMessageTypes.IMD, "TRX") as CMRIMDMessage;
				result = (tRXMessage == null || tRXMessage.IsOriginalMessage) ? "LODGE" : "AMENDMENT";
			}
			return result;
		}

		#endregion

		protected virtual ZDateTime CurrentDateTime
		{
			get { return ZDateTime.Now; }
		}

		#region OwnerDetails

		public ZString GetOwnerDetails()
		{
			var cRN = ZString.Empty;
			if (entryHeader.Declaration != null && entryHeader.Declaration.Importer != null)
			{
				var splitter = new ABNCACSplitter(entryHeader.Declaration.Importer.LocalBusinessRegNo);
				if (!splitter.ABN.IsEmpty)
				{
					cRN = splitter.ABN;
				}
				if (!splitter.CAC.IsEmpty)
				{
					cRN += "/" + splitter.CAC;
				}
			}
			return cRN;
		}

		protected TextLayout OwnerDetails
		{
			get
			{
				TextLayout layout = new TextLayout();
				layout.PrintAt(1, 1, "OWNER   :");
				var cRN = ZString.Empty;
				if (entryHeader.Declaration != null)
				{
					if (entryHeader.Declaration.Importer != null)
					{
						layout.PrintAt(1, 11, entryHeader.Declaration.Importer.OH_FullName.Left(30));
						cRN = GetOwnerDetails();
					}
					layout.PrintAt(2, 1, "(" + entryHeader.Declaration.OwnerCode.Replace(" ", "") + ") (" + cRN + ")");
					layout.PrintAt(5, 1, "O/REF   : " + entryHeader.Declaration.JE_OwnerRef.Left(30));
				}
				return layout;
			}
		}

		#endregion

		#region AgencyDetails
		protected TextLayout AgencyDetails
		{
			get
			{
				TextLayout layout = new TextLayout();
				layout.PrintAt(1, 1, "AGENCY  : " + GlbCompany.CurrentCompany.GC_Name.Left(30));
				if (entryHeader.Declaration != null)
				{
					layout.PrintAt(2, 11, "(" + Env.Registry.BrokerageID + ")");
					layout.PrintAt(3, 34, "(" + entryHeader.AgencyBranchIdentifier + ")");
					layout.PrintAt(4, 1, "BOX NO  : " + entryHeader.Declaration.BranchBoxNo);
					layout.PrintAt(5, 1, "A/REF   : " + entryHeader.AgentReference);
				}
				return layout;
			}
		}
		#endregion

		#region ShippingDetails
		protected TextLayout ShippingDetails
		{
			get
			{
				TextLayout result = null;
				if (entryHeader.Declaration != null)
				{
					if (entryHeader.Declaration.IsSea)
					{
						result = SeaShippingDetails;
					}
					else if (entryHeader.Declaration.IsAir)
					{
						result = AirShippingDetails;
					}
					else if (entryHeader.Declaration.IsPost)
					{
						result = PostShippingDetails;
					}
				}
				if (result == null)
				{
					result = new TextLayout();
				}
				return result;
			}
		}

		protected TextLayout SeaShippingDetails
		{
			get
			{
				string vessel = "";
				string lloydsNo = entryHeader.Declaration.VesselNumber;
				string voyageNo = "";
				if (entryHeader.Declaration.JE_VesselName.IsValid)
				{
					vessel = entryHeader.Declaration.JE_VesselName;
				}

				if (entryHeader.Declaration != null && entryHeader.Declaration.IsImportCMR)
				{
					voyageNo = entryHeader.Declaration.JE_VoyageFlightNo;
				}
				else
				{
					if (entryHeader.Declaration.CleanVoyageNumber.IsValid)
					{
						voyageNo = entryHeader.Declaration.CleanVoyageNumber;
					}
					else if (entryHeader.Declaration.JE_VoyageFlightNo.IsValid)
					{
						voyageNo = entryHeader.Declaration.JE_VoyageFlightNo;
					}
				}
				TextLayout layout = new TextLayout();
				layout.PrintAt(1, 1, "MODE     : SEA");
				layout.PrintAt(3, 1, "SHIP     : " + vessel);
				layout.PrintAt(4, 12, "(" + lloydsNo + ")");
				layout.PrintAt(5, 1, "SHIP VOY : " + voyageNo);
				return layout;
			}
		}

		protected TextLayout AirShippingDetails
		{
			get
			{
				string flightNo = "";
				string folio = "";
				if (entryHeader.Declaration.JE_VoyageFlightNo.IsValid)
				{
					flightNo = entryHeader.Declaration.JE_VoyageFlightNo;
				}
				if (entryHeader.Declaration.JE_Folio.IsValid)
				{
					folio = entryHeader.Declaration.JE_Folio;
				}
				TextLayout layout = new TextLayout();
				layout.PrintAt(1, 1, "MODE     : AIR");
				layout.PrintAt(3, 1, "AIRCR    : " + flightNo);
				layout.PrintAt(5, 1, "FOLIO    : " + folio);
				return layout;
			}
		}

		protected TextLayout PostShippingDetails
		{
			get
			{
				TextLayout layout = new TextLayout();
				layout.PrintAt(1, 1, "MODE     : POST");
				layout.PrintAt(3, 1, "PPC'S    : " + entryHeader.HouseBillsCommaSeparated);
				return layout;
			}
		}

		#endregion

		#region SOFA Details

		protected TextLayout SOFAIndicator
		{
			get
			{
				var layout = new TextLayout();
				layout.PrintAt(1, 1, "SOFA Declaration");
				return layout;
			}
		}

		#endregion

		#region Supplier Details
		protected TextLayout SupplierDetails
		{
			get
			{
				TextLayout layout = new TextLayout();
				if (entryHeader.Declaration != null && entryHeader.Declaration.Supplier != null)
				{
					layout.PrintAt(1, 1, "SUPPLIER: " + entryHeader.JZ_SupplierName.Left(30));
					layout.PrintAt(2, 11, "(" + entryHeader.JZ_SupplierCode + ")");
				}
				return layout;
			}
		}

		protected void PrintCMRSupplierDetails(CusEntryLine line, TextLayout layout)
		{
			var supplier = line.Supplier;
			var customsClientID = line.SupplierCode;

			if (!customsClientID.IsEmpty)
			{
				var lineNo = layout.VerticalPosition;
				lineNo++;
				var supplierInfo = ZString.Format("SUPPLIER: {0} ({1})", supplier.OH_FullNameTruncated, customsClientID);
				layout.PrintAt(lineNo, 5, supplierInfo.Left(129));
			}
		}

		#endregion

		public ZString TIValue
		{
			get
			{
				ZDecimal tandI = entryHeader.TransportAndInsuranceInLocalCurrency.Amount;
				if (tandI.IsEmpty)
				{
					tandI = GetCurrencyValue(entryHeader.OverseasFreight).Amount + GetCurrencyValue(entryHeader.OverseasInsurance).Amount;
				}
				return tandI.ToString(2);
			}
		}

		#region Amount Details
		protected TextLayout AmountDetails
		{
			get
			{
				TextLayout layout = new TextLayout();
				Money fOB = entryHeader.FOB;
				Money fOBInLocalCurrency = entryHeader.FOBInLocalCurrency;
				if (!entryHeader.IsCMRNature30 && fOB.IsValid)
				{
					string currencyIndicator = GetCurrencyIndicator(fOB.Currency);
					layout.PrintAt(1, 1, "FOB  " + currencyIndicator + " :");

					if (entryHeader.Declaration != null)
					{
						layout.PrintAt(1, 12, fOB.Amount.ToString(2).PadLeft(14) + " = $A" + fOBInLocalCurrency.Amount.ToString().PadLeft(14));
					}
				}

				Money cIF = entryHeader.CIF;
				if (!entryHeader.IsCMRNature30 && cIF.IsValid)
				{
					string currencyIndicator = GetCurrencyIndicator(cIF.Currency);
					layout.PrintAt(2, 1, "CIF  " + currencyIndicator + " :");
					if (entryHeader.Declaration != null)
					{
						layout.PrintAt(2, 12, cIF.Amount.ToString(2).PadLeft(14) + " = $A" + entryHeader.CurrencyConverter.ConvertRounded(cIF, JobDeclaration.GetLocalCurrency()).Amount.ToString().PadLeft(14));
					}
				}

				if (!entryHeader.IsCMRNature30)
				{
					string grossWeight = "GRWT";
					switch (entryHeader.GrossWeight.Unit.Length)
					{
						case 1:
							grossWeight += " (" + entryHeader.GrossWeight.Unit + ") :";
							break;
						case 2:
							grossWeight += " (" + entryHeader.GrossWeight.Unit + "):";
							break;
						case 3:
							grossWeight += "(" + entryHeader.GrossWeight.Unit + "):";
							break;
						default:
							grossWeight += "    :";
							break;
					}
					layout.PrintAt(3, 1, grossWeight);

					var weightInKgs = Core.Constants.Weight.ContainsCode(entryHeader.GrossWeight.Unit) ? Core.Constants.Weight.Convert(entryHeader.GrossWeight.Amount, entryHeader.GrossWeight.Unit, Core.Constants.Weight.Kilograms) : decimal.Zero;
					layout.PrintAt(3, 12, entryHeader.GrossWeight.Amount.ToString(2).PadLeft(14) + " = KG" + new ZDecimal(weightInKgs).ToString(2).PadLeft(14));
				}

				layout.PrintAt(4, 1, "T & I    :");
				ZDecimal tandI = entryHeader.TransportAndInsuranceInLocalCurrency.Amount;
				if (tandI.IsEmpty)
				{
					tandI = GetCurrencyValue(entryHeader.OverseasFreight).Amount + GetCurrencyValue(entryHeader.OverseasInsurance).Amount;
				}
				layout.PrintAt(4, 12, tandI.ToString(2).PadLeft(14) + " = $A" + tandI.ToString(2).PadLeft(14));

				return layout;
			}
		}
		#endregion

		#region Port Details
		protected TextLayout PortDetails
		{
			get
			{
				TextLayout layout = new TextLayout();
				if (entryHeader.Declaration != null)
				{
					layout = (IsCMREntry) ? GetCMRPortDetails() : GetPortDetails();
				}
				return layout;
			}
		}

		TextLayout GetPortDetails()
		{
			TextLayout layout = new TextLayout();
			if (entryHeader.Declaration.IsPost)
			{
				if (entryHeader.Declaration.PortOfLoading != null)
				{
					layout.PrintAt(3, 1, "LOAD  PT : " + entryHeader.Declaration.PortOfLoading.RL_PortName.Left(27).ToUpper());
				}
				if (entryHeader.Declaration.PortOfArrival != null)
				{
					layout.PrintAt(5, 1, "ARRIVAL  : " + GetParcelPostLocation());
				}
				layout.PrintAt(5, 31, CustomsDate(entryHeader.Declaration.JE_DateOfArrival));
			}
			else
			{
				if (entryHeader.Declaration.PortOfLoading != null)
				{
					layout.PrintAt(1, 1, "LOAD  PT : " + entryHeader.Declaration.PortOfLoading.RL_PortName.Left(27).ToUpper());
				}
				if (entryHeader.Declaration.PortOfFirstArrival != null)
				{
					layout.PrintAt(3, 1, "FIRST PT : " + entryHeader.Declaration.PortOfFirstArrival.RL_PortName.Left(19).ToUpper());
				}
				layout.PrintAt(3, 31, CustomsDate(entryHeader.Declaration.JE_DateOfFirstArrival));
				if (entryHeader.Declaration.PortOfArrival != null)
				{
					layout.PrintAt(5, 1, "DSCH  PT : " + entryHeader.Declaration.PortOfArrival.RL_PortName.Left(19).ToUpper());
				}
				layout.PrintAt(5, 31, CustomsDate(entryHeader.Declaration.JE_DateOfArrival));
			}
			return layout;
		}

		TextLayout GetCMRPortDetails()
		{
			TextLayout layout = new TextLayout();
			if (entryHeader.Declaration.IsPost)
			{
				if (entryHeader.Declaration.PortOfLoading != null)
				{
					layout.PrintAt(1, 1, "LOAD  PT : " + entryHeader.Declaration.PortOfLoading.RL_PortName.Left(27).ToUpper());
				}
				if (entryHeader.Declaration.PortOfArrival != null)
				{
					layout.PrintAt(2, 1, "ARRIVAL  : " + GetParcelPostLocation());
				}
				layout.PrintAt(2, 31, CustomsDate(entryHeader.Declaration.JE_DateOfArrival));
				if (entryHeader.Declaration.FinalDestination != null && !entryHeader.IsCMRNature30)
				{
					layout.PrintAt(3, 1, "DEST  PT : " + entryHeader.Declaration.FinalDestination.RL_PortName.Left(27).ToUpper());
				}
			}
			else
			{
				if (entryHeader.Declaration.PortOfLoading != null)
				{
					layout.PrintAt(1, 1, "LOAD  PT : " + entryHeader.Declaration.PortOfLoading.RL_PortName.Left(27).ToUpper());
				}
				if (entryHeader.Declaration.PortOfFirstArrival != null)
				{
					layout.PrintAt(2, 1, "FIRST PT : " + entryHeader.Declaration.PortOfFirstArrival.RL_PortName.Left(19).ToUpper());
				}
				layout.PrintAt(2, 31, CustomsDate(entryHeader.Declaration.JE_DateOfFirstArrival));
				if (entryHeader.Declaration.PortOfArrival != null)
				{
					layout.PrintAt(3, 1, "DSCH  PT : " + entryHeader.Declaration.PortOfArrival.RL_PortName.Left(19).ToUpper());
				}
				layout.PrintAt(3, 31, CustomsDate(entryHeader.Declaration.JE_DateOfArrival));
				if (entryHeader.Declaration.FinalDestination != null && !entryHeader.IsCMRNature30)
				{
					layout.PrintAt(4, 1, "DEST  PT : " + entryHeader.Declaration.FinalDestination.RL_PortName.Left(27).ToUpper());
				}
			}
			return layout;
		}

		public string GetParcelPostLocation()
		{
			string pPLocation;
			switch (entryHeader.Declaration.PortOfArrival.Code)
			{
				case "AUADL":
					pPLocation = "SA PARCELS POST";
					break;
				case "AUBNE":
					pPLocation = "QLD PARCELS POST";
					break;
				case "AUHBA":
					pPLocation = "TAS PARCELS POST";
					break;
				case "AUMEL":
					pPLocation = "VIC PARCELS POST";
					break;
				case "AUPER":
					pPLocation = "WA PARCELS POST";
					break;
				case "AUSYD":
					pPLocation = "NSW PARCELS POST";
					break;
				default:
					pPLocation = entryHeader.Declaration.PortOfArrival.RL_PortName.Left(19).ToUpper();
					break;
			}
			return pPLocation;
		}

		#endregion

		#region Invoice Header
		protected TextLayout InvoiceHeader
		{
			get
			{
				TextLayout layout = new TextLayout();
				if (!entryHeader.ITOTIncoTerm.IsEmpty && !entryHeader.IsCMRNature30 && !entryHeader.IsNature30)
				{
					layout.PrintAt(1, 1, "ITERMS  : " + entryHeader.ITOTIncoTerm);
				}
				if (entryHeader.Declaration != null)
				{
					layout.PrintAt(3, 1, "VALUATION DATE : " + CustomsDate(entryHeader.EffectiveValuationDate));
				}
				layout.PrintAt(5, 1, Currencies);
				layout.PrintAt(3, IsCMREntry ? 42 : 30, HeaderAmounts);
				return layout;
			}
		}

		#region Currencies

		protected TextLayout Currencies
		{
			get { return (IsCMREntry) ? GetCMRCurrencies() : GetCurrencies(); }
		}

		public ZStringBuilder CurrenciesList
		{
			get { return GetCurrenciesList(); }
		}

		ZStringBuilder GetCurrenciesList()
		{
			ZStringBuilder builder = new ZStringBuilder();
			Money invoiceTotal = entryHeader.InvoiceTotal;
			if (entryHeader.Declaration != null)
			{
				if (IsCMREntry)
				{
					int lineNo = 0;
					foreach (ICurrency currency in WhatCurrenciesUsed())
					{
						lineNo++;
						RefCurrency currency1 = (RefCurrency)entryHeader.Factory.Load(typeof(RefCurrency), currency.PK);
						if (currency1.PK == entryHeader.LocalCurrency.PK)
						{
							var str = string.Format(CultureInfo.CurrentCulture, "{0} {1} @ 1.0000", lineNo.ToString(CultureInfo.CurrentCulture), currency1.RX_Code);
							builder.Append(str.PadRight(50));
						}
						else
						{
							ZDateTime foundRateDate = ZDateTime.Empty;
							ZDecimal exchangeRate = currency1.GetRateForDate(ZArchitecture.Core.ExchangeRateType.Customs, entryHeader.EffectiveValuationDate, MaximumDaysToFallback, out foundRateDate);
							var str = string.Format(CultureInfo.CurrentCulture, "{0} {1} @ {2} ({3})", lineNo.ToString(CultureInfo.CurrentCulture), currency1.RX_Code, exchangeRate.ToString("0.####", CultureInfo.CurrentCulture), CustomsDate(foundRateDate));
							builder.Append(str.PadRight(50));
						}
					}
				}
				else
				{
					if (invoiceTotal.IsValid)
					{
						var str = "1 " + invoiceTotal.Currency.Code + " @ " + entryHeader.CurrencyConverter.GetExchangeRate(invoiceTotal.Currency).ToString("0.####", CultureInfo.CurrentCulture);
						builder.Append(str.PadRight(50));
					}
					int currenciesUsed = entryHeader.UsedCurrencies.Length;
					if (currenciesUsed > 1)
					{
						RefCurrency currency2 = (RefCurrency)entryHeader.Factory.Load(typeof(RefCurrency), entryHeader.UsedCurrencies[1].PK);
						var str = "2 " + entryHeader.UsedCurrencies[1].Code + " @ " + currency2.GetCustomsRate(entryHeader.Declaration.DateOfValuation).ToString("0.####", CultureInfo.CurrentCulture);
						builder.Append(str.PadRight(50));
					}
					if (currenciesUsed > 2)
					{
						RefCurrency currency3 = (RefCurrency)entryHeader.Factory.Load(typeof(RefCurrency), entryHeader.UsedCurrencies[2].PK);
						var str = "3 " + entryHeader.UsedCurrencies[2].Code + " @ " + currency3.GetCustomsRate(entryHeader.Declaration.DateOfValuation).ToString("0.####", CultureInfo.CurrentCulture);
						builder.Append(str.PadRight(50));
					}
				}
			}
			return builder;
		}

		TextLayout GetCurrencies()
		{
			TextLayout result = new TextLayout();
			result.PrintAt(1, 1, "CRNCYS");
			Money invoiceTotal = entryHeader.InvoiceTotal;
			if (entryHeader.Declaration != null)
			{
				if (invoiceTotal.IsValid)
				{
					result.PrintAt(1, 8, "1 " + invoiceTotal.Currency.Code + " @ " + entryHeader.CurrencyConverter.GetExchangeRate(invoiceTotal.Currency).ToString("0.####", CultureInfo.CurrentCulture));
				}
				int currenciesUsed = entryHeader.UsedCurrencies.Length;
				if (currenciesUsed > 1)
				{
					RefCurrency currency2 = (RefCurrency)entryHeader.Factory.Load(typeof(RefCurrency), entryHeader.UsedCurrencies[1].PK);
					result.PrintAt(2, 8, "2 " + entryHeader.UsedCurrencies[1].Code + " @ " + currency2.GetCustomsRate(entryHeader.Declaration.DateOfValuation).ToString("0.####", CultureInfo.CurrentCulture));
				}
				if (currenciesUsed > 2)
				{
					RefCurrency currency3 = (RefCurrency)entryHeader.Factory.Load(typeof(RefCurrency), entryHeader.UsedCurrencies[2].PK);
					result.PrintAt(3, 8, "3 " + entryHeader.UsedCurrencies[2].Code + " @ " + currency3.GetCustomsRate(entryHeader.Declaration.DateOfValuation).ToString("0.####", CultureInfo.CurrentCulture));
				}
			}
			return result;
		}

		int MaximumDaysToFallback
		{
			get { return entryHeader.RandomHeader != null ? ((ICurrencyConverterDataProvider)entryHeader.RandomHeader).MaximumDaysToFallback : 0; }
		}

		TextLayout GetCMRCurrencies()
		{
			TextLayout result = new TextLayout();
			result.PrintAt(1, 1, "CRNCYS");
			Money invoiceTotal = entryHeader.InvoiceTotal;
			if (entryHeader.Declaration != null)
			{
				int lineNo = 0;
				foreach (ICurrency currency in WhatCurrenciesUsed())
				{
					lineNo++;
					RefCurrency currency1 = (RefCurrency)entryHeader.Factory.Load(typeof(RefCurrency), currency.PK);
					if (currency1.PK == entryHeader.LocalCurrency.PK)
					{
						result.PrintAt(lineNo, 8, string.Format("{0} {1} @ 1.0000", lineNo.ToString(CultureInfo.CurrentCulture), currency1.RX_Code));
					}
					else
					{
						ZDateTime foundRateDate = ZDateTime.Empty;
						ZDecimal exchangeRate = currency1.GetRateForDate(ZArchitecture.Core.ExchangeRateType.Customs, entryHeader.EffectiveValuationDate, MaximumDaysToFallback, out foundRateDate);
						result.PrintAt(lineNo, 8, string.Format("{0} {1} @ {2} ({3})", lineNo.ToString(CultureInfo.CurrentCulture), currency1.RX_Code, exchangeRate.ToString("0.####", CultureInfo.CurrentCulture), CustomsDate(foundRateDate)));
					}
				}
			}
			return result;
		}

		#endregion

		protected TextLayout HeaderAmounts
		{
			get
			{
				TextLayout layout = new TextLayout();
				layout.PrintAt(1, 1, GetCurrencyDetail("ITOT ", entryHeader.InvoiceTotal));
				if (!entryHeader.PackingCosts.IsEmpty)
				{
					layout.Print(GetCurrencyDetail("PC   ", entryHeader.PackingCosts));
				}

				if (!entryHeader.OverseasFreight.IsEmpty)
				{
					layout.Print(GetCurrencyDetail("OSEA ", entryHeader.OverseasFreight));
				}

				if (!entryHeader.OverseasInsurance.IsEmpty)
				{
					layout.Print(GetCurrencyDetail("ONS  ", entryHeader.OverseasInsurance));
				}

				if (!entryHeader.LandingCharges.IsEmpty)
				{
					layout.Print(GetCurrencyDetail("LCH  ", entryHeader.LandingCharges));
				}

				if (!entryHeader.ForeignInlandFreight.IsEmpty)
				{
					layout.Print(GetCurrencyDetail("FIFT ", entryHeader.ForeignInlandFreight));
				}

				if (!entryHeader.Discount.IsEmpty)
				{
					layout.Print(GetCurrencyDetail("DS1  ", entryHeader.Discount));
				}

				if (!entryHeader.Commission.IsEmpty)
				{
					layout.Print(GetCurrencyDetail("COM  ", entryHeader.Commission));
				}

				if (!entryHeader.OtherCharges1.IsEmpty)
				{
					layout.Print(GetCurrencyDetail("ADD  ", entryHeader.OtherCharges1));
				}

				if (!entryHeader.OtherCharges2.IsEmpty)
				{
					layout.Print(GetCurrencyDetail("DED  ", entryHeader.OtherCharges2));
				}

				return layout;
			}
		}

		public ZString DEDDetails
		{
			get { return GetCurrencyDetail("DED  ", entryHeader.OtherCharges2); }
		}

		protected string GetCurrencyDetail(string caption, Money amount)
		{
			return GetCurrencyDetail(caption, amount, true, 14);
		}

		protected string GetCurrencyDetail(string caption, Money amount, bool shouldPrintAUDAmount, int paddingForAmount)
		{
			var result = caption + GetCurrencyCaptionDetail(amount) + GetCurrencyValueDetail(amount, shouldPrintAUDAmount).PadLeft(paddingForAmount);

			if (shouldPrintAUDAmount)
			{
				result += GetCurrencyAUDAmountDetail(amount).PadLeft(paddingForAmount);
			}
			return result;
		}

		protected string GetCurrencyCaptionDetail(Money amount)
		{
			string currencyIndicator = GetCurrencyIndicator(amount.Currency);
			ZStringBuilder result = new ZStringBuilder();
			result.Append(currencyIndicator);
			result.Append(" : ");
			return result.ToString();
		}

		protected string GetCurrencyValueDetail(Money amount, bool shouldPrintAUDAmount)
		{
			int decimalPlaces = amount.Currency != null ? amount.Currency.Decimals : 2;

			ZStringBuilder result = new ZStringBuilder();
			result.Append(amount.Amount.ToString(decimalPlaces));
			if (shouldPrintAUDAmount)
			{
				result.Append(" = $A");
			}
			return result.ToString();
		}

		protected string GetCurrencyAUDAmountDetail(Money amount)
		{
			string aUDollarAmount = "";
			if (entryHeader.Declaration != null)
			{
				aUDollarAmount = entryHeader.CurrencyConverter.ConvertRounded(amount, JobDeclaration.GetLocalCurrency()).Amount.ToString(2);
			}
			return aUDollarAmount;
		}

		protected Money GetCurrencyValue(Money amount)
		{
			if (entryHeader.Declaration != null)
			{
				return entryHeader.CurrencyConverter.ConvertRounded(amount, JobDeclaration.GetLocalCurrency());
			}
			else
			{
				return CurrencyConverter.New(entryHeader.Factory).ConvertRounded(amount, JobDeclaration.GetLocalCurrency());
			}
		}

		protected TextLayout InvoiceHeaderAndCustomsValue
		{
			get
			{
				TextLayout layout = new TextLayout();
				int nextVertical = isPortrait ? 3 : 1;
				layout.PrintAt(nextVertical, 1, InvoiceHeader);
				layout.PrintAt(2, 95, CustomsValueAndWarehouseDetails);
				return layout;
			}
		}

		protected TextLayout CustomsValueAndWarehouseDetails
		{
			get
			{
				TextLayout result = new TextLayout();
				result.PrintAt(1, 1, "TOTAL CUSTOMS VALUE : $A " + entryHeader.CustomsValueInAUD.Amount.ToString(2));

				int nextVertical = isPortrait ? 5 : 3;
				result.PrintAt(nextVertical, 1, "FACTOR              : " + entryHeader.CustomsFactor.ToString(8));
				nextVertical += 2;

				if (entryHeader.Declaration != null)
				{
					result.PrintAt(nextVertical, 1, "CALCULATION DATE    : " + CustomsDate(entryHeader.Declaration.JE_EntrySubmittedDate));
				}
				nextVertical += 3;

				if (!IsCMREntry && entryHeader.IsNature20 && WarehouseAddress != null)
				{
					result.PrintAt(nextVertical, 1, WarehouseDetails);
				}
				return result;
			}
		}

		protected TextLayout WarehouseDetails
		{
			get
			{
				TextLayout result = new TextLayout();
				if (HasWarehouseData && WarehouseAddress != null)
				{
					result.PrintAt(1, 1, "WAREHOUSE: " + WarehouseAddress.Header.OH_FullName.ToUpper().Left(30));
					result.PrintAt(2, 1, "           (" + WarehouseAddress.LocalControlledPremisesID + ")");
				}
				return result;
			}
		}

		public OrgAddress WarehouseAddress
		{
			get
			{
				if (fWarehouseAddress == null && entryHeader.MergedLines.Count > 0)
				{
					OrgAddress warehouseFromLine = GetWarehouseAddressFromLineIfAvailable();
					fWarehouseAddress = warehouseFromLine ?? entryHeader.Declaration.WarehouseAddress;
				}
				return fWarehouseAddress;
			}
		}
		OrgAddress fWarehouseAddress;

		OrgAddress GetWarehouseAddressFromLineIfAvailable()
		{
			foreach (CusEntryLine entryLine in entryHeader.MergedLines)
			{
				if (entryLine.RandomLine != null && entryLine.RandomLine.AddInfo.WarehouseAddress != null)
				{
					return entryLine.RandomLine.AddInfo.WarehouseAddress;
				}
			}
			return null;
		}

		#region LineHeader

		protected TextLayout LineHeader
		{
			get { return (IsCMREntry) ? GetCMRLineHeader() : GetLineHeader(); }
		}

		protected TextLayout GetLineHeader()
		{
			TextLayout layout = new TextLayout();
			layout.LF();
			layout.Print("LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT");
			layout.Print("NO.                          PREF         QUANTITY            PRICE           BASE       DUTY RATE                     NO.");
			layout.Print("");
			return layout;
		}

		protected TextLayout GetCMRLineHeader()
		{
			TextLayout layout = new TextLayout();
			layout.LF();
			layout.Print("LN  TARIFF      STAT TREAT ORIGIN/    INVOICE/TARIFF        INVOICE  CUSTOMS VALUE/           DUTY/          GST    INSTRUMENT");
			string line2 = "NO. PREF (ORIGIN, SCHEME, RULE)           QUANTITY            PRICE           BASE       DUTY RATE                  ";
			line2 += (HasWarehouseData) ? "WAREHOUSE UV" : "   NO.";
			layout.Print(line2);
			layout.Print("");
			return layout;
		}

		#endregion

		protected void PrintLinesBody()
		{
			TextLayout header = LineHeader;
			bool addHeader = true;
			entryHeader.MergedLines.Sort(CusEntryLine.Schema.CL_LineNumber, System.ComponentModel.ListSortDirection.Ascending);
			foreach (CusEntryLine line in entryHeader.MergedLines)
			{
				TextLayout lineBody = OneLineBody(line);
				if (CreateNewPageIfNotEnoughSpaceToPrintLines(lineBody.VerticalPosition + (addHeader ? header.VerticalPosition : 0)))
				{
					addHeader = true;
				}

				if (addHeader)
				{
					fEntryPrint.Print(header);
					addHeader = false;
				}
				fEntryPrint.Print(lineBody);
			}
		}

		protected bool CreateNewPageIfNotEnoughSpaceToPrintLines(int noOfLinesNeeded)
		{
			bool result = false;
			if (!IsEnoughSpaceToPrintLines(noOfLinesNeeded))
			{
				if (!hasFirstPageFooterBeenPrinted)
				{
					PrintFirstPageFooter(LinesPerPage - fEntryPrint.Collection.Count);
				}
				fEntryPrint.Print(PrintToPageBreak);
				fEntryPrint.Print(Heading);
				result = true;
			}
			return result;
		}

		protected bool IsEnoughSpaceToPrintLines(int noOfLinesNeeded)
		{
			int availableLines = LinesPerPage - LinesPrintedThisPage;
			if (!hasFirstPageFooterBeenPrinted)
			{
				availableLines -= FooterLines + EndOfEntryLines;
			}

			return availableLines >= noOfLinesNeeded;
		}

		#region OneLineBody

		protected TextLayout OneLineBody(CusEntryLine line)
		{
			return (IsCMREntry) ? GetCMROneLineBody(line) : GetEdificeOneLineBody(line);
		}

		TextLayout GetEdificeOneLineBody(CusEntryLine line)
		{
			TextLayout layout = new TextLayout();

			if (line.IsTrailer)
			{
				layout.PrintAt(1, 1, "TRL");
				layout.PrintAt(1, 5, line.TariffNumber);
				layout.PrintAt(1, 18, line.StatCode);
				layout.PrintAt(1, 23, line.TreatmentCode);
				layout.PrintAt(1, 28, line.ORG);
				layout.PrintAt(1, 33, line.PRF);
				layout.PrintAt(2, 5, line.Description);
			}
			else
			{
				layout.Print(line.CL_LineNumber.ToString().PadLeft(3, '0'));
				int lineNo = layout.VerticalPosition;
				layout.PrintAt(lineNo, 5, line.TariffNumber);
				layout.PrintAt(lineNo, 18, line.StatCode);
				layout.PrintAt(lineNo, 23, line.TreatmentCode);
				layout.PrintAt(lineNo, 28, line.ORG);
				layout.PrintAt(lineNo, 33, line.PRF);
				layout.PrintAt(lineNo, 39, line.Quantity.ToString(2).PadLeft(10) + " " + line.UnitOfQuantity);
				layout.PrintAt(lineNo, 54, line.Price.Amount.ToString(2).PadLeft(14));
				layout.PrintAt(lineNo, 69, line.CL_CustomsValue.ToString(2).PadLeft(14));
				layout.PrintAt(lineNo, 85, line.DutyAmount.ToString(2).PadLeft(14));
				ZDecimal gSTAmountPayableOrDeferred = line.GSTVATAmount.IsEmpty ? line.GSTVATDeferred : line.GSTVATAmount;
				layout.PrintAt(lineNo, 99, gSTAmountPayableOrDeferred.ToString(2).PadLeft(14));
				layout.PrintAt(lineNo, 117, line.InstrumentType + " " + line.InstrumentCode);

				lineNo++;

				string valuationBasis = line.RandomLine.AddInfo.AggregatedZA_ValuationBasis_Hidden;
				if (!line.PriceAdjustment.IsEmpty)
				{
					valuationBasis += "+ADJ";
				}
				layout.PrintAt(lineNo, 81, valuationBasis);
				layout.PrintAt(lineNo, 94, line.DutyAmount == 0m ? " FREE" : line.DutyRateDescription.ToString());

				lineNo++;
				layout.PrintAt(lineNo, 5, "ADD INFO: " + line.AddInfoLine);

				lineNo++;
				if (line.Description.Length < 113)
				{
					layout.PrintAt(lineNo, 5, line.Description);
				}
				else
				{
					layout.PrintAt(lineNo, 5, (line.Description.SubstringSafe(0, 110) + "..."));
				}

				lineNo++;
				layout.PrintAt(lineNo, 5, GetVOTILine(false, line));
			}
			layout.Print("");
			return layout;
		}

		public ZString GetVOTILine(ZBool isCMREntry, CusEntryLine entryline)
		{
			if (isCMREntry)
			{
				ZStringBuilder vOTILine = new ZStringBuilder();
				vOTILine.Append("VOTI=" + entryline.VOTI.ToString(2).PadLeft(12) + "      T&I=" + entryline.CustomsCalculatedTransportAndInsuranceInLocalCurrency.Amount.ToString(2).PadLeft(12));

				if (entryline.IsNature20)
				{
					vOTILine.Append("      T&I UV=" + entryline.TransportAndInsuranceUnitValueInLocalCurrency.ToString(2).PadLeft(12));
				}
				if (entryline.WETAmountIncludingWHEstimate != 0)
				{
					vOTILine.Append("      WET=" + entryline.WETAmountIncludingWHEstimate.ToString(2).PadLeft(12));
				}
				if (entryline.LCTAmountIncludingWHEstimate != 0)
				{
					vOTILine.Append("      LCT=" + entryline.LCTAmountIncludingWHEstimate.ToString(2).PadLeft(12));
				}
				if (entryline.CountervailingDuty != 0)
				{
					vOTILine.Append("      CVD=" + entryline.CountervailingDuty.ToString(2).PadLeft(12));
				}
				if (entryline.DumpingDuty != 0)
				{
					vOTILine.Append("      DMP=" + entryline.DumpingDuty.ToString(2).PadLeft(12));
				}
				if (entryline.SecurityConcessionAmount != 0)
				{
					vOTILine.Append("      Security=" + entryline.SecurityConcessionAmount.ToString(2).PadLeft(12));
				}
				if (entryline.SecurityLiabilityAmount != 0)
				{
					vOTILine.Append("      Security Uncollected=" + entryline.SecurityLiabilityAmount.ToString(2).PadLeft(12));
				}
				return vOTILine.ToString();
			}
			else
			{
				string vOTILine = "VOTI=" + entryline.VOTI.ToString(2).PadLeft(12) + "      T&I=" + entryline.TransportAndInsuranceInLocalCurrency.Amount.ToString(2).PadLeft(12) + "      WET=" + entryline.WETAmount.ToString(2).PadLeft(12);
				if (entryline.LCTAmount > 0)
				{
					vOTILine += "      LCT=" + entryline.LCTAmount.ToString(2).PadLeft(12);
				}
				return vOTILine;
			}
		}

		TextLayout GetCMROneLineBody(CusEntryLine line)
		{
			TextLayout layout = new TextLayout();

			if (line.IsTrailer)
			{
				layout.PrintAt(1, 1, "TRL");
				layout.PrintAt(1, 5, line.TariffNumber);
				layout.PrintAt(1, 18, line.StatCode);
				layout.PrintAt(1, 23, line.TreatmentCode);
				layout.PrintAt(1, 28, line.ORG);
				if (!line.POC.IsEmpty || !line.PST.IsEmpty || !line.PRT.IsEmpty)
				{
					layout.PrintAt(2, 5, PreferenceDetails(line));
					layout.LF();
				}
				PrintLongStringsInMultipleLines(line.Description, layout, 5);
			}
			else
			{
				layout.Print(line.CL_LineNumber.ToString().PadLeft(3, '0'));
				int lineNo = layout.VerticalPosition;
				layout.PrintAt(lineNo, 5, line.TariffNumber);
				layout.PrintAt(lineNo, 18, line.StatCode);
				layout.PrintAt(lineNo, 23, line.TreatmentCode);
				layout.PrintAt(lineNo, 28, line.ORG);
				layout.PrintAt(lineNo, 39, line.Quantity.ToString("0.#####").PadLeft(10).Substring(0, 10) + " " + line.UnitOfQuantity);
				layout.PrintAt(lineNo, 54, line.Price.Amount.ToString(2).PadLeft(14));
				layout.PrintAt(lineNo, 69, line.CL_CustomsValue.ToString(2).PadLeft(14));
				if (line.IsNature20 && !line.IsDutyAndTaxEstimatedForWH)
				{
					layout.PrintAt(lineNo, 85, ZDecimal.Zero.ToString(2).PadLeft(14));
					layout.PrintAt(lineNo, 99, ZDecimal.Zero.ToString(2).PadLeft(14));
				}
				else
				{
					layout.PrintAt(lineNo, 85, line.DutyAmountIncludingWHEstimate.ToString(2).PadLeft(14));
					ZDecimal gSTAmountPayableOrDeferred = line.GSTVATAmountIncludingWHEstimate.IsEmpty ? line.GSTVATDeferred : line.GSTVATAmountIncludingWHEstimate;
					layout.PrintAt(lineNo, 99, gSTAmountPayableOrDeferred.ToString(2).PadLeft(14));
				}

				var instrument = line.InstrumentType + " " + line.InstrumentCode;
				if (line.IsDutyAndTaxEstimatedForWH)
				{
					layout.PrintAt(lineNo, 117, "*ESTIMATE*");
					if (!string.IsNullOrEmpty(instrument.Trim()))
					{
						lineNo++;
						layout.PrintAt(lineNo, 117, instrument);
					}
				}
				else
				{
					layout.PrintAt(lineNo, 117, instrument);
				}

				lineNo++;

				if (!line.POC.IsEmpty || !line.PST.IsEmpty || !line.PRT.IsEmpty)
				{
					layout.PrintAt(lineNo, 5, PreferenceDetails(line));
				}

				string valuationBasis = (line != null && line.REL == "Y") ? "RT/" : "UT/";
				valuationBasis += line.ValuationBasisForCMR;
				if (!line.PriceAdjustment.IsEmpty)
				{
					valuationBasis += "+ADJ";
				}
				layout.PrintAt(lineNo, 78, valuationBasis);

				ZStringBuilder dutyRates = new ZStringBuilder();
				dutyRates.Append(line.CL_DutyPercent.ToString(2));
				dutyRates.Append("%");
				if (line.CL_FlatAmount != 0m && !line.CL_FlatAmountUQ.IsEmpty)
				{
					dutyRates.Append("+");
					dutyRates.Append(line.CL_FlatAmount.ToString(5));
					dutyRates.Append("/");
					dutyRates.Append(line.CL_FlatAmountUQ);
				}
				layout.PrintAt(lineNo, 94, line.DutyAmountIncludingWHEstimate == 0m ? " FREE" : dutyRates.ToString());

				if (line.IsNature20 || line.IsNature30)
				{
					layout.PrintAt(lineNo, 117, line.WarehouseUnitValue.Amount.ToString(4).PadLeft(12));
				}

				var permitNumbers = line.OrderedAQISPermitIds.ReBuildAQISElements();
				if (!permitNumbers.IsEmpty)
				{
					permitNumbers = string.Format("Quarantine Permits: {0}", permitNumbers);
					if (permitNumbers.Length > 50)
					{
						lineNo++;
						PrintLongStringsInMultipleLines(permitNumbers, layout, 5);
					}
					else
					{
						layout.PrintAt(lineNo, 20, permitNumbers);
					}
				}

				if (!entryHeader.IsCMRNature30)
				{
					PrintCMRSupplierDetails(line, layout);
				}
				PrintAddInfo(line.AddInfoLine, layout);
				PrintLongStringsInMultipleLines(line.Description, layout, 5);

				lineNo = layout.VerticalPosition;
				PrintLongStringsInMultipleLinesWrapAtSpaceAnywhere(GetVOTILine(true, line), layout, MaxLength - 5, ref lineNo, 5);
			}
			layout.Print("");
			return layout;
		}

		string PreferenceDetails(CusEntryLine line)
		{
			return line.PST == AUAddInfo.GeneralPreferenceRate ? string.Format("({0})", line.PST) :
				string.Format("({0},{1},{2})", line.POC, line.PST, line.PRT);
		}

		#endregion

		void PrintAddInfo(ZString addInfo, TextLayout layout)
		{
			if (!addInfo.IsEmpty)
			{
				PrintLongStringsInMultipleLines("ADD INFO: " + addInfo.Trim(), layout, 5);
			}
		}

		void PrintLongStringsInMultipleLines(ZString longString, TextLayout layout, int offSet)
		{
			int lineNo = layout.VerticalPosition;
			longString = longString.Replace("\r\n", " ").Replace("  ", " ").Trim();
			if (!longString.IsEmpty)
			{
				while (!longString.IsEmpty)
				{
					lineNo++;
					layout.PrintAt(lineNo, offSet, longString.Left(MaxLength - offSet));
					longString = longString.SubstringSafe(MaxLength - offSet).TrimStart();
				}
			}
		}

		void PrintLongStringsInMultipleLinesWrapAtSpace(ZString longString, TextLayout layout, int maxLength)
		{
			longString = longString.Replace("\r\n", " ").Replace("  ", " ").Trim();
			PrintLongStringsInMultipleLinesWrapAtSpaceAnywhere(longString, layout, maxLength, ref headerPrintAt, 1);
		}

		void PrintLongStringsInMultipleLinesWrapAtSpace(ZString longString, TextLayout layout, int maxLength, int printAtLine)
		{
			longString = longString.Replace("\r\n", " ").Replace("  ", " ").Trim();
			PrintLongStringsInMultipleLinesWrapAtSpaceAnywhere(longString, layout, maxLength, ref printAtLine, 1);
		}

		void PrintLongStringsInMultipleLinesWrapAtSpaceAnywhere(ZString longString, TextLayout layout, int maxLength, ref int printAtLine, int offset)
		{
			int lastSpace;
			if (!longString.IsEmpty)
			{
				while (!longString.IsEmpty)
				{
					printAtLine++;
					lastSpace = longString.Length;
					if (longString.Length > maxLength)
					{
						lastSpace = longString.SubstringSafe(0, maxLength).LastIndexOf(" ");
					}
					if (lastSpace < 1)
					{
						lastSpace = maxLength;
					}

					layout.PrintAt(printAtLine, offset, longString.Left(lastSpace));
					longString = longString.SubstringSafe(lastSpace).TrimStart();
				}
			}
		}

		const int MaxLength = 134;

		protected TextLayout GetFirstPageFooter()
		{
			TextLayout layout = new TextLayout();
			layout.PrintAt(1, 1, TotalDeferredGSTLayout);
			layout.PrintAt(layout.VerticalPosition, 1, AuthorisationStatement);
			layout.PrintAt(1, 87, EFTBlock);
			return layout;
		}

		protected void PrintFirstPageFooter(int remainingAvailableLines)
		{
			if ((remainingAvailableLines - 1) > firstPageFooter.Collection.Count)
			{
				fEntryPrint.PrintAt(LinesPerPage - ((remainingAvailableLines - 1) - firstPageFooter.Collection.Count) / 2 - firstPageFooter.Collection.Count, 1, firstPageFooter);
			}
			else
			{
				fEntryPrint.Print(firstPageFooter);
			}
			hasFirstPageFooterBeenPrinted = true;
		}

		#region TotalDeferredGST

		protected TextLayout TotalDeferredGSTLayout
		{
			get { return (IsCMREntry) ? GetCMRTotalDeferredGST() : GetTotalDeferredGST(); }
		}

		TextLayout GetTotalDeferredGST()
		{
			TextLayout layout = new TextLayout();
			layout.Print("");
			layout.Print("");
			layout.Print("");
			layout.Print("");
			layout.Print("");
			layout.Print("");
			layout.Print("");
			layout.Print("TOTAL DEFERRED DUTY FOR ENTRY  =   $" + entryHeader.DeferredDuty.ToString(2));
			layout.Print("TOTAL DEFERRED GST  FOR ENTRY  =   $" + TotalGSTDeferred.ToString(2));
			return layout;
		}

		TextLayout GetCMRTotalDeferredGST()
		{
			TextLayout layout = new TextLayout();
			layout.Print("");
			layout.Print("");

			if ((entryHeader.DutyAmount) != 0)
			{
				layout.Print("");
			}

			if (entryHeader.CountervailingDuty != 0)
			{
				layout.Print("");
			}

			if (entryHeader.DumpingDuty != 0)
			{
				layout.Print("");
			}
			if (entryHeader.IsDutyDeferred)
			{
				layout.Print("TOTAL DEFERRED DUTY FOR ENTRY  =   $" + entryHeader.DeferredDuty.ToString(2));
			}
			else
			{
				layout.Print("");
			}
			if (IsGSTDeferred)
			{
				layout.Print("TOTAL DEFERRED GST  FOR ENTRY  =   $" + (TotalGSTDeferred != ZDecimal.Zero ? TotalGSTDeferred : entryHeader.GSTAmount).ToString(2));
			}
			else
			{
				layout.Print("");
			}

			if (entryHeader.WETAmount != 0)
			{
				layout.Print("");
			}

			if (entryHeader.LCTAmount != 0)
			{
				layout.Print("");
			}

			if (entryHeader.AQISServicePaymentAmount != 0 && IncludeAQISServicePaymentsInTotalPayableOnEntryPrint)
			{
				layout.Print("");
			}

			layout.Print("");
			return layout;
		}

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise", "EDI012:UnmaintainableProductName_CSharp", Justification = "Baseline issue")]
		protected TextLayout AuthorisationStatement
		{
			get
			{
				TextLayout layout = new TextLayout();
				layout.Print("");
				if (entryHeader.Declaration != null)
				{
					var deliveryAddress = entryHeader.Declaration.ImporterDeliveryAddress;
					var formattedAddress = deliveryAddress.AddressSummary.Replace("\r\n", "\n").Split('\n');
					layout.Print("Delivery: " + deliveryAddress.E2_CompanyName);
					layout.Print("          " + (formattedAddress.Length > 0 ? formattedAddress[0] : ZString.Empty));
					layout.Print("          " + (formattedAddress.Length > 1 ? formattedAddress[1] : ZString.Empty));
					layout.Print("          " + (formattedAddress.Length > 2 ? formattedAddress[2] : ZString.Empty));
				}
				else
				{
					layout.Print("");
					layout.Print("");
					layout.Print("");
					layout.Print("");
				}
				layout.Print("");
				layout.Print("I ......................................................");
				layout.Print("BEING AN AUTHORISED AGENT/OWNER, MAKE THIS ENTRY AND AUTHORISE LODGEMENT.");
				layout.Print("(SIGNED)    /  /    ........................ AGENT/OWNER");
				layout.Print("");
				layout.Print("ENTRY PRINT - ESTIMATED ENTRY ONLY - Printed by CargoWise One   www.cargowise.com");
				layout.Print("The Entry print will reflect the message details, once a successful message");
				layout.Print("response has been received from Government Customs.");
				return layout;
			}
		}

		#region EFTBlock

		protected TextLayout EFTBlock
		{
			get { return (IsCMREntry) ? GetCMREFTBlock() : GetEFTBlock(); }
		}

		ZBool IsDutyDeferred => entryHeader.IsDutyDeferred;

		string FormatDeferrableCharge(ZDecimal chargeValue) => FormatDecimalCharge(IsDutyDeferred ? ZDecimal.Zero : chargeValue);
		string FormatDecimalCharge(ZDecimal chargeValue) => chargeValue.ToString(2).PadLeft(10);

		TextLayout GetEFTBlock()
		{
			ZDecimal deferrableOtherCharges = IsDutyDeferred ? 0m : Convert.ToDecimal(
				  entryHeader.EntryFee
				+ entryHeader.WoodLevy
				+ entryHeader.AQISProcessingCharge);

			ZDecimal allOtherCharges = deferrableOtherCharges + Convert.ToDecimal(
				  entryHeader.MessageFee
				+ entryHeader.TradegateGST
				+ entryHeader.ScreenFreeCharge
				+ entryHeader.OtherEntryCharge
				+ entryHeader.AQISServicePaymentAmount
				+ entryHeader.TotalPayableAdmin
				+ entryHeader.AQISContainerCharges);

			TextLayout layout = new TextLayout();
			int lineNo = 0;
			layout.PrintAt(++lineNo, 1, "**************  E F T    O N L Y  **************");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "* TOTAL DUTY                      " + FormatDeferrableCharge(entryHeader.DutyAmount) + "   *");
			layout.PrintAt(++lineNo, 1, "* TOTAL GST                       " + FormatDeferrableCharge(entryHeader.GSTAmount) + "   *");
			layout.PrintAt(++lineNo, 1, "* TOTAL WET                       " + FormatDeferrableCharge(entryHeader.WETAmount) + "   *");
			layout.PrintAt(++lineNo, 1, "* TOTAL LCT                       " + FormatDeferrableCharge(entryHeader.LCTAmount) + "   *");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "* OTHER CHARGES                   " + FormatDecimalCharge(allOtherCharges) + "   *");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "* TOTAL AMOUNT PAYABLE ***        " + FormatDeferrableCharge(entryHeader.TotalAmountPayable) + " ***");
			layout.PrintAt(++lineNo, 1, "************************************************");
			layout.PrintAt(++lineNo, 1, "* OFFICIAL USE ONLY                            *");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "* ................................     /  /    *");
			layout.PrintAt(++lineNo, 1, "* SIGNATURE OF AUTHORISING OFFICER     DATE    *");
			layout.PrintAt(++lineNo, 1, "************************************************");
			layout.PrintAt(++lineNo, 1, "* WARRANTED AND RECEIPTED:                     *");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "************************************************");
			return layout;
		}

		TextLayout GetCMREFTBlock()
		{
			TextLayout layout = new TextLayout();
			int lineNo = 0;
			layout.PrintAt(++lineNo, 1, "**************  E F T    O N L Y  **************");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "*                                              *");

			if (entryHeader.PayableDuty != 0)
			{
				layout.PrintAt(++lineNo, 1, "* DUTY                            " + FormatDecimalCharge(entryHeader.PayableDuty) + "   *");
			}

			if (entryHeader.CountervailingDuty != 0)
			{
				layout.PrintAt(++lineNo, 1, "* COUNTERVAILING DUTY             " + FormatDecimalCharge(entryHeader.CountervailingDuty) + "   *");
			}

			if (entryHeader.DumpingDuty != 0)
			{
				layout.PrintAt(++lineNo, 1, "* DUMPING DUTY                    " + FormatDecimalCharge(entryHeader.DumpingDuty) + "   *");
			}

			layout.PrintAt(++lineNo, 1, "* GST                             " + FormatDecimalCharge(IsGSTDeferred ? ZDecimal.Zero : entryHeader.GSTAmount) + "   *");

			if (entryHeader.PayableWET != 0)
			{
				layout.PrintAt(++lineNo, 1, "* WET                             " + FormatDecimalCharge(entryHeader.PayableWET) + "   *");
			}

			if (entryHeader.PayableLCT != 0)
			{
				layout.PrintAt(++lineNo, 1, "* LCT                             " + FormatDecimalCharge(entryHeader.PayableLCT) + "   *");
			}

			ZDecimal totalAmountPayable = entryHeader.TotalAmountPayable;
			if (IncludeAQISServicePaymentsInTotalPayableOnEntryPrint)
			{
				if (entryHeader.AQISServicePaymentAmount != 0)
				{
					layout.PrintAt(++lineNo, 1, "* QUARANTINE SERVICES FEE         " + FormatDecimalCharge(entryHeader.AQISServicePaymentAmount) + "   *");
				}
			}
			else
			{
				totalAmountPayable -= entryHeader.AQISServicePaymentAmount;
			}
			layout.PrintAt(++lineNo, 1, "* OTHER CHARGES                   " + FormatDecimalCharge(entryHeader.PayableOtherCMRCharges) + "   *");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "* TOTAL AMOUNT PAYABLE ***        " + FormatDecimalCharge(totalAmountPayable) + " ***");
			if (!IncludeAQISServicePaymentsInTotalPayableOnEntryPrint)
			{
				if (entryHeader.AQISServicePaymentAmount != 0)
				{
					layout.PrintAt(++lineNo, 1, "* QUARANTINE SERVICES FEE         " + FormatDecimalCharge(entryHeader.AQISServicePaymentAmount) + "   *");
				}
			}
			layout.PrintAt(++lineNo, 1, "************************************************");
			layout.PrintAt(++lineNo, 1, "* OFFICIAL USE ONLY                            *");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "* ................................     /  /    *");
			layout.PrintAt(++lineNo, 1, "* SIGNATURE OF AUTHORISING OFFICER     DATE    *");
			layout.PrintAt(++lineNo, 1, "************************************************");
			layout.PrintAt(++lineNo, 1, "* WARRANTED AND RECEIPTED:                     *");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "*                                              *");
			layout.PrintAt(++lineNo, 1, "************************************************");
			return layout;
		}

		#endregion

		protected TextLayout EndOfEntry
		{
			get
			{
				TextLayout layout = new TextLayout();
				layout.Print("                                                ***  END OF ENTRY  ***");
				return layout;
			}
		}

		protected TextLayout PrintToPageBreak
		{
			get
			{
				TextLayout layout = new TextLayout();
				int linesPrintedThisPage = this.LinesPrintedThisPage;
				while (linesPrintedThisPage < LinesPerPage)
				{
					layout.Print("");
					linesPrintedThisPage += 1;
				}
				return layout;
			}
		}

		protected int LinesPrintedThisPage
		{
			get
			{
				int result = fEntryPrint.VerticalPosition;
				while (result > LinesPerPage)
				{
					result -= LinesPerPage;
				}
				return result;
			}
		}

		#endregion

		#region PackagesBillsContainers

		protected void PrintPackagesBillsContainers()
		{
			PrintTotalNumberOfPackages();
			if (IsCMREntry)
			{
				PrintCMRPackagesBillsContainers();
			}
			else
			{
				PrintPreCMRPackagesBillsContainers();
			}
		}

		void PrintTotalNumberOfPackages()
		{
			int totalNumberOfPacks = 0;

			if (IsCMREntry)
			{
				CreateNewPageIfNotEnoughSpaceToPrintLines(entryHeader.IsNature1020 ? 3 : 2);
				if (entryHeader.IsSAC)
				{
					totalNumberOfPacks = entryHeader.Declaration.JE_TotalNoOfPacks;
					string packagesInWords = CustomsPackagesInWords(totalNumberOfPacks);
					fEntryPrint.Print("TOTAL NUMBER OF PACKAGES:  " + totalNumberOfPacks.ToString().PadLeft(8) + "   " + packagesInWords);
				}
				else
				{
					if (!entryHeader.IsNature20)
					{
						totalNumberOfPacks = entryHeader.TotalNumberOfPackages;
						string packagesInWords = CustomsPackagesInWords(totalNumberOfPacks);
						fEntryPrint.Print("TOTAL NUMBER OF PACKAGES:  " + totalNumberOfPacks.ToString().PadLeft(8) + "   " + packagesInWords);
					}
					if (entryHeader.IsNature1020 || entryHeader.IsNature20)
					{
						totalNumberOfPacks = entryHeader.CMRTotalNumberOfWarehousePackages;
						string packagesInWords = CustomsPackagesInWords(totalNumberOfPacks);
						fEntryPrint.Print("TOTAL NUMBER OF WAREHOUSE PACKAGES:  " + totalNumberOfPacks.ToString().PadLeft(8) + "   " + packagesInWords);
					}
				}
			}
			else
			{
				if (entryHeader.IsNature10)
				{
					totalNumberOfPacks = entryHeader.Nature10Packages;
				}
				else if (entryHeader.IsNature20)
				{
					totalNumberOfPacks = entryHeader.Nature20Packages;
				}
				string packagesInWords = CustomsPackagesInWords(totalNumberOfPacks);
				CreateNewPageIfNotEnoughSpaceToPrintLines(2);
				fEntryPrint.Print("TOTAL NUMBER OF PACKAGES:  " + totalNumberOfPacks.ToString().PadLeft(8) + "   " + packagesInWords);
			}
			fEntryPrint.Print("");
		}

		protected void PrintPreCMRPackagesBillsContainers()
		{
			if (entryHeader.Declaration != null)
			{
				CreateNewPageIfNotEnoughSpaceToPrintLines(2);
				fEntryPrint.Print(GetBillNumbers());
				fEntryPrint.Print("");
				if (entryHeader.Declaration.IsContainerised)
				{
					if (entryHeader.Declaration.CusContainers.Count > 0)
					{
						ZString containersInEntry = "CONTAINER NOS:   ";
						int noOfContainers = 0;
						foreach (CusContainer container in entryHeader.Declaration.CusContainers)
						{
							ZString containerNo = container.CO_ContainerNumber;
							ZString containerType = container.CO_FCL_LCL_AIR;
							containersInEntry = containersInEntry + "(" + containerType + ")" + containerNo + "  ";
							noOfContainers++;
							int remainder = 0;
							Math.DivRem(noOfContainers, 6, out remainder);
							if (remainder == 0)
							{
								if (!IsEnoughSpaceToPrintLines(3))
								{
									fEntryPrint.Print(containersInEntry.TrimEnd());
									if (noOfContainers < entryHeader.Declaration.CusContainers.Count)
									{
										CreateNewPageIfNotEnoughSpaceToPrintLines(2);
										fEntryPrint.CRLF();
									}
									containersInEntry = "CONTAINER NOS:   ";
								}
								else
								{
									fEntryPrint.Print(containersInEntry.TrimEnd());
									containersInEntry = "                 ";
								}
							}
						}

						if (!containersInEntry.IsEmpty && containersInEntry.TrimEnd() != "CONTAINER NOS:")
						{
							fEntryPrint.Print(containersInEntry.TrimEnd());
						}
						fEntryPrint.Print("");
					}
				}
				PrintMarksAndNumbers();
			}
		}

		public string GetBillNumbers()
		{
			string billNos = string.Empty;

			ZString masterBills = entryHeader.DirectMasterOrLinkedMasterBillsCommaSeparated;
			if (masterBills != "")
			{
				billNos += masterBills + "/M   ";
			}

			ZString houseBills = entryHeader.HouseBillsCommaSeparated;
			if (houseBills != "")
			{
				billNos += houseBills + "/H";
			}
			return billNos;
		}

		void PrintMarksAndNumbers()
		{
			if (entryHeader.Declaration != null)
			{
				ZString marksAndNumbers = entryHeader.Declaration.JE_MarksAndNumbers.Replace("\r\n", " ").Replace("  ", " ");
				if (!marksAndNumbers.IsEmpty)
				{
					TextLayout marksAndNumbersLayout = new TextLayout();
					marksAndNumbers = "MARKS AND NOS: " + marksAndNumbers.Trim();
					marksAndNumbersLayout.Print(marksAndNumbers.Left(MaxLength));
					marksAndNumbers = marksAndNumbers.SubstringSafe(MaxLength);
					if (!marksAndNumbers.IsEmpty)
					{
						PrintLongStringsInMultipleLines(marksAndNumbers, marksAndNumbersLayout, 16);
					}
					CreateNewPageIfNotEnoughSpaceToPrintLines(marksAndNumbersLayout.VerticalPosition);
					fEntryPrint.Print(marksAndNumbersLayout);
				}
			}
		}

		protected void PrintCMRPackagesBillsContainers()
		{
			if (entryHeader.Declaration != null)
			{
				if (entryHeader.IsSAC || entryHeader.IsNature30 || entryHeader.Declaration.JE_TransportMode == Core.Constants.TransportModes.Other)
				{
					PrintPreCMRPackagesBillsContainers();
				}
				else
				{
					PrintPackingDetails();
				}
			}
		}

		void PrintPackingDetails()
		{
			if (entryHeader.PackingGroups.Count > 0)
			{
				TextLayout header = PackingDetailsHeader;
				bool addHeader = true;
				foreach (PackingGroup package in entryHeader.PackingGroups)
				{
					TextLayout packingLine = GetPackingLine(package);
					if (CreateNewPageIfNotEnoughSpaceToPrintLines(packingLine.VerticalPosition + (addHeader ? header.VerticalPosition : 0)))
					{
						addHeader = true;
					}

					if (addHeader)
					{
						fEntryPrint.Print(header);
						addHeader = false;
					}
					fEntryPrint.Print(packingLine);
				}
				fEntryPrint.CRLF();
			}
		}

		TextLayout PackingDetailsHeader
		{
			get
			{
				TextLayout layout = new TextLayout();
				layout.LF();
				switch (entryHeader.Declaration.JE_TransportMode)
				{
					case Core.Constants.TransportModes.Mail:
						layout.Print("NO. OF PACKAGES / PARCEL POST NUMBER");
						break;
					case Core.Constants.TransportModes.Air:

						bool consignRefNumberEntered = entryHeader.Declaration.Bills.Cast<Bill>().Any(x => !x.CU_fPartShipConsignmentReference.IsEmpty);
						if (consignRefNumberEntered)
						{
							layout.Print("NO. OF PACKAGES / MASTER BILL / HOUSE BILL / CONSIGN. REF. NO.");
						}
						else
						{
							layout.Print("NO. OF PACKAGES / MASTER BILL / HOUSE BILL");
						}
						break;
					default:
						layout.Print("CONTAINER NO. / NO. OF PACKAGES / MASTER BILL / HOUSE BILL");
						break;
				}
				layout.Print("");
				return layout;
			}
		}

		internal TextLayout GetPackingLine(PackingGroup package)
		{
			TextLayout layout = new TextLayout();
			ZStringBuilder packingLine = new ZStringBuilder();
			packingLine.Append(GetPackingDetails(package) + " / ");

			if (package.Bill != null)
			{
				if (entryHeader.Declaration.JE_TransportMode == Core.Constants.TransportModes.Mail)
				{
					if (!package.Bill.CU_BillNum.IsEmpty)
					{
						packingLine.Append(package.Bill.CU_BillNum);
					}
				}
				else
				{
					if (!entryHeader.Declaration.IsAir)
					{
						ZString containerDetails = ZString.Empty;

						if (package.Container != null)
						{
							string containerNo = package.Container.CO_ContainerNumber;
							string containerType = package.Container.CO_FCL_LCL_AIR;
							containerDetails = "(" + containerType + ")" + containerNo + " / ";
						}
						else if (entryHeader.Declaration.JE_ContainerMode == Core.Constants.ContainerModes.BreakBulk ||
							entryHeader.Declaration.JE_ContainerMode == Core.Constants.ContainerModes.Bulk ||
							entryHeader.Declaration.JE_ContainerMode == Core.Constants.ContainerModes.Liquid)
						{
							ZString containerType = entryHeader.Declaration.JE_ContainerMode;
							containerDetails = "(" + containerType + ") / ";
						}

						packingLine.Prepend(containerDetails);
					}

					ZString masterBill = package.Bill.CU_MasterBill;
					if (!masterBill.IsEmpty)
					{
						packingLine.Append(masterBill);
					}

					packingLine.Append(" / ");

					ZString houseBill = package.Bill.CU_HouseBill;
					if (!houseBill.IsEmpty)
					{
						packingLine.Append(houseBill);
					}

					bool consignRefNumberEntered = entryHeader.Declaration.Bills.Cast<Bill>().Any(x => !x.CU_fPartShipConsignmentReference.IsEmpty);

					if (consignRefNumberEntered)
					{
						packingLine.Append(" / ");
					}

					if (entryHeader.Declaration.IsAir && package.Bill != null && !package.Bill.CU_fPartShipConsignmentReference.IsEmpty)
					{
						packingLine.Append(package.Bill.CU_fPartShipConsignmentReference);
					}
				}
			}
			layout.Print(packingLine.ToString());
			PrintPackageMarksAndNumbers(package.MarksAndNumbers, layout);

			return layout;
		}

		string GetPackingDetails(PackingGroup package)
		{
			int totalNumberOfPackages = package.TotalNumberOfPackages;
			return totalNumberOfPackages.ToString() + " " + entryHeader.Declaration.JE_TotalNoOfPacksPackType;
		}

		void PrintPackageMarksAndNumbers(ZString marksAndNumbers, TextLayout layout)
		{
			if (!marksAndNumbers.IsEmpty)
			{
				PrintLongStringsInMultipleLines("PACKAGE MARKS: " + marksAndNumbers.Trim(), layout, 5);
			}
		}

		#endregion

		#region Amber Reason And Statement

		void PrintAmberReasonAndStatement()
		{
			if (entryHeader.Declaration != null && !entryHeader.Declaration.JE_AmberStatement.IsEmpty)
			{
				TextLayout layout = new TextLayout();
				layout.PrintAt(1, 1, "AMBER REASON AND STATEMENT:");
				layout.PrintAt(3, 1, "REASON: " + entryHeader.Declaration.AddInfo.ZA_HART_Hidden);
				PrintLongStringsInMultipleLinesWrapAtSpace("STATEMENT: " + entryHeader.Declaration.JE_AmberStatement, layout, MaxLength, 3);
				if (CreateNewPageIfNotEnoughSpaceToPrintLines(layout.Collection.Count))
				{
					fEntryPrint.CRLF();
				}

				layout.Print(string.Empty);
				fEntryPrint.Print(layout);
			}
		}

		#endregion

		#region Print Paid Under Protest Statement

		void PrintPaidUnderProtestStatement()
		{
			if (entryHeader.Declaration != null && !entryHeader.Declaration.JE_PaidUnderProtestStatement.IsEmpty)
			{
				TextLayout layout = new TextLayout();
				layout.PrintAt(1, 1, "PAID UNDER PROTEST STATEMENT:");
				PrintLongStringsInMultipleLinesWrapAtSpace(entryHeader.Declaration.JE_PaidUnderProtestStatement, layout, MaxLength, 2);
				layout.Print(string.Empty);
				if (CreateNewPageIfNotEnoughSpaceToPrintLines(layout.Collection.Count))
				{
					fEntryPrint.CRLF();
				}

				fEntryPrint.Print(layout);
			}
		}

		#endregion

		protected ArrayList WhatCurrenciesUsed()
		{
			if (currenciesUsed == null)
			{
				currenciesUsed = new ArrayList();
				if (IsCMREntry)
				{
					AddCurrencyUsed(entryHeader.InvoiceTotal.Currency);
					foreach (JobComInvoiceHeader header in entryHeader.InvoiceHeaders)
					{
						AddCurrencyUsed(header.InvoiceAmount.Currency);
					}
					foreach (ICurrency currency in entryHeader.UsedCurrencies)
					{
						AddCurrencyUsed(currency);
					}
				}
				else
				{
					currenciesUsed.Add(entryHeader.InvoiceTotal.Currency);
					int currenciesUsedCount = entryHeader.UsedCurrencies.Length;
					if (currenciesUsedCount > 1)
					{
						currenciesUsed.Add(entryHeader.UsedCurrencies[1]);
					}
					if (currenciesUsedCount > 2)
					{
						currenciesUsed.Add(entryHeader.UsedCurrencies[2]);
					}
				}
			}
			return currenciesUsed;
		}
		ArrayList currenciesUsed;

		void AddCurrencyUsed(ICurrency currency)
		{
			if (currency != null)
			{
				foreach (ICurrency currencyInArray in currenciesUsed)
				{
					if (currencyInArray.Code == currency.Code)
					{
						return;
					}
				}
				currenciesUsed.Add(currency);
			}
		}

		protected string GetCurrencyIndicator(ICurrency currency)
		{
			string currencyIndicatorReturned = "   ";
			if (currency != null)
			{
				ArrayList currencies = WhatCurrenciesUsed();
				for (int i = 0; i < currencies.Count; i++)
				{
					if (((ICurrency)currencies[i]).Code == currency.Code)
					{
						currencyIndicatorReturned = "(" + (i + 1).ToString() + ")";
						break;
					}
				}
			}
			return currencyIndicatorReturned;
		}

		internal ZString CustomsDate(ZDateTime customsDate) => customsDate.ToString("ddMMMyy").ToUpper();

		internal string CustomsTime(ZDateTime customsDateTime) => customsDateTime.ToString("ddMMMyy HH:mm").ToUpper() + " HRS";

		public string CustomsPackagesInWords(int totalPackages)
		{
			string numberInWords = "(";
			if (totalPackages >= 0)
			{
				string[] primitiveNumbers = { "ZERO", "ONE", "TWO", "THREE", "FOUR", "FIVE", "SIX", "SEVEN", "EIGHT", "NINE" };
				string digits = totalPackages.ToString();
				foreach (char digit in digits)
				{
					string num = digit.ToString();
					int digitValue = Convert.ToInt16(num);
					numberInWords = numberInWords + primitiveNumbers[digitValue] + " ";
				}
			}
			else
			{
				numberInWords += "NEGATIVE";
			}
			numberInWords = numberInWords.Trim() + ")";
			return numberInWords;
		}

		public ZDecimal TotalGSTDeferred
		{
			get { return totalGSTDeferred ?? (totalGSTDeferred = (GetDeferredGST(entryHeader))).Value; }
		}
		ZDecimal? totalGSTDeferred;

		internal static ZDecimal GetDeferredGST(CusEntryHeader header)
		{
			ZDecimal result = 0;
			foreach (CusEntryLine line in header.MergedLines)
			{
				result += line.GSTVATDeferred;
			}
			return result;
		}

		public bool IsGSTDeferred
		{
			get { return isGSTDeferred ?? (isGSTDeferred = (IsGSTDeferredForEntryPrint(TotalGSTDeferred, entryHeader))).Value; }
		}
		bool? isGSTDeferred;

		internal static bool IsGSTDeferredForEntryPrint(ZDecimal totalGSTDeferred, CusEntryHeader header)
		{
			return (totalGSTDeferred != ZDecimal.Zero || (header != null &&
									!header.IsStatusPostLodge && header.Declaration != null &&
									header.Declaration.Consignee != null &&
									header.Declaration.Consignee.MiscServ.OM_IMIsGSTDeferred == ZBool.True));
		}

		#endregion
	}
}
