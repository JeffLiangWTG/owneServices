using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DocumentWrappers.Customs.Base;
using Enterprise.DocumentWrappers.GenericWrappers;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.Customs.AU
{
	public class DocCusEntryHeader : DocBaseCusEntryHeader
	{
		DocCusEntryHeader(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
			: base(cusEntryHeader, factoryToWrap)
		{
		}

		public static DocCusEntryHeader New(CusEntryHeader cusEntryHeader, BusinessObjectFactory factoryToWrap)
		{
			if (cusEntryHeader == null)
			{
				return null;
			}
			else
			{
				return new DocCusEntryHeader(cusEntryHeader, factoryToWrap);
			}
		}

		#region ZDecimal Fields

		public ZDecimal CustomsFactor
		{
			get { return CusEntryHeader.CustomsFactor; }
		}

		public ZDecimal DutyAmount
		{
			get { return CusEntryHeader.DutyAmount; }
		}

		public ZDecimal LCTAmount
		{
			get { return CusEntryHeader.LCTAmount; }
		}

		public ZDecimal WETAmount
		{
			get { return CusEntryHeader.WETAmount; }
		}

		public ZDecimal OtherCMRCharges
		{
			get { return CusEntryHeader.OtherCMRCharges; }
		}

		public ZDecimal AQISServicePaymentAmount
		{
			get { return CusEntryHeader.AQISServicePaymentAmount; }
		}

		public ZDecimal CountervailingDuty
		{
			get { return CusEntryHeader.CountervailingDuty; }
		}

		public ZDecimal DumpingDuty
		{
			get { return CusEntryHeader.DumpingDuty; }
		}

		public ZDecimal TAndI
		{
			get { return CusEntryHeader.TAndI; }
		}

		protected override ZDecimal EntryFeeCore
		{
			get { return CusEntryHeader.EntryFee; }
		}

		protected override ZDecimal MessageFeeCore
		{
			get { return CusEntryHeader.MessageFee; }
		}

		protected override ZDecimal OtherChargesCore
		{
			get { return CusEntryHeader.OtherEntryCharge; }
		}

		protected override ZDecimal OtherEntryChargesOtherThanEntryFeeMessageFeeAndLeviesCore
		{
			get { return CusEntryHeader.OtherEntryCharge + CusEntryHeader.AllAQISCharges + CusEntryHeader.TotalPayableAdmin; }
		}

		public ZDecimal ScreenFreeCharge
		{
			get { return CusEntryHeader.ScreenFreeCharge; }
		}

		public ZDecimal TradegateGST
		{
			get { return CusEntryHeader.TradegateGST; }
		}

		public ZDecimal WoodLevy
		{
			get { return CusEntryHeader.WoodLevy; }
		}

		protected override ZDecimal EntryLeviesCore
		{
			get { return ScreenFreeCharge + TradegateGST + WoodLevy; }
		}

		public ZDecimal AllEntryFees
		{
			get { return CusEntryHeader.AllEntryFees; }
		}

		public ZDecimal TotalPayableFeesAndCharges
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				foreach (DocCusEntryHeaderCharges payableCharge in PayableCharges)
				{
					result += payableCharge.ChargeAmount;
				}

				foreach (DocCusEntryLineFee fee in PayableFees)
				{
					result += fee.ChargeAmount;
				}

				return result;
			}
		}

		public ZDecimal AllAQISCharges
		{
			get { return CusEntryHeader.AllAQISCharges; }
		}

		public ZDecimal TotalDeferredDuty => CusEntryHeader.DeferredDuty;
		public ZDecimal PayableDuty => CusEntryHeader.PayableDuty;
		public ZDecimal PayableWET => CusEntryHeader.PayableWET;
		public ZDecimal PayableLCT => CusEntryHeader.PayableLCT;
		public ZDecimal PayableOtherCMRCharges => CusEntryHeader.PayableOtherCMRCharges;

		#endregion

		#region ZBool Fields

		public ZBool IsPrimeEntry
		{
			get { return CusEntryHeader.IsPrimeEntry; }
		}

		public ZBool IsEnclosureEntry
		{
			get { return CusEntryHeader.IsEnclosureEntry; }
		}

		public ZBool IncludeAQISServicePaymentsInTotalPayable
		{
			get { return AUCustomsEntryPrint.IncludeAQISServicePaymentsInTotalPayableOnEntryPrint; }
		}

		public ZBool IsDutyDeferred => CusEntryHeader.IsDutyDeferred;

		#endregion

		#region ZString Fields

		public ZString ImportEntryAdvice
		{
			get { return CusEntryHeader.ImportEntryAdvice; }
		}

		public ZString StatusDescription
		{
			get { return CusEntryHeader.MessageStatusDescription; }
		}

		public ZString BranchID
		{
			get { return CusEntryHeader.AgencyBranchIdentifier; }
		}

		public ZString AgentReference
		{
			get { return CusEntryHeader.AgentReference; }
		}

		internal ZString[] GetEntryPrintLines(bool isPortrait)
		{
			AUCustomsEntryPrint entryPrint = new AUCustomsEntryPrint(CusEntryHeader, isPortrait);
			return entryPrint.EntryPrint.Replace("\r", "").Split('\n');
		}

		AUCustomsEntryPrint AUCustomsEntryPrint
		{
			get { return new AUCustomsEntryPrint(CusEntryHeader, false); }
		}

		#endregion

		#region Wrapper Fields

		public DocCusEntryHeader PrimeEntry
		{
			get { return DocCusEntryHeader.New(CusEntryHeader.PrimeEntry, Factory); }
		}

		public DocDeclaration Declaration
		{
			get { return DocDeclaration.New(CusEntryHeader.Declaration, Factory); }
		}

		#endregion

		#region Collections

		public DocCMRCusEntryCPDecCollection Questions
		{
			get { return fQuestions ?? (fQuestions = new DocCMRCusEntryCPDecCollection(CusEntryHeader.Questions, Factory)); }
		}
		DocCMRCusEntryCPDecCollection fQuestions;

		public DocJobComInvoiceLineCollection InvoiceLines
		{
			get
			{
				DocJobComInvoiceLineCollection result = new DocJobComInvoiceLineCollection(Factory);
				foreach (CusEntryLine mergedLine in CusEntryHeader.MergedLines)
				{
					foreach (JobComInvoiceLine invoiceLine in mergedLine.InvoiceLines)
					{
						result.Add(DocJobComInvoiceLine.New(invoiceLine, Factory));
					}
				}
				return result;
			}
		}

		public DocCusEntryLineCollection MergedLines
		{
			get
			{
				if (fMergedLines == null)
				{
					fMergedLines = new DocCusEntryLineCollection(CusEntryHeader.MergedLines, Factory);
				}

				return fMergedLines;
			}
		}

		public ZString FormattedTotalNoOfPacks
		{
			get
			{
				int totalNumberOfPacks = 0;
				if (IsCMREntry)
				{
					if (CusEntryHeader.IsSAC || CusEntryHeader.IsTransportModeOther)
					{
						totalNumberOfPacks = Declaration.TotalNoOfPacks;
					}
					else
					{
						if (!IsNature20)
						{
							totalNumberOfPacks = CusEntryHeader.TotalNumberOfPackages;
						}
					}
				}
				else
				{
					if (IsNature10)
					{
						totalNumberOfPacks = CusEntryHeader.Nature10Packages;
					}
					else if (IsNature20)
					{
						totalNumberOfPacks = CusEntryHeader.Nature20Packages;
					}
				}
				return totalNumberOfPacks.ToString(CultureInfo.CurrentCulture).PadLeft(8) + "   " + AUCustomsEntryPrint.CustomsPackagesInWords(totalNumberOfPacks);
			}
		}

		public ZString FormattedWarehouseTotalNoOfPacks
		{
			get
			{
				int totalNumberOfPacks = 0;
				if (IsCMREntry && !CusEntryHeader.IsSAC && (IsNature1020 || IsNature20))
				{
					totalNumberOfPacks = CusEntryHeader.CMRTotalNumberOfWarehousePackages;
				}
				return totalNumberOfPacks.ToString(CultureInfo.CurrentCulture).PadLeft(8) + "   " + AUCustomsEntryPrint.CustomsPackagesInWords(totalNumberOfPacks);
			}
		}

		public ZString GetVOTILine(CusEntryLine entryline)
		{
			return AUCustomsEntryPrint.GetVOTILine(IsCMREntry, entryline);
		}

		public ZString FormattedBillNumbers
		{
			get { return AUCustomsEntryPrint.GetBillNumbers(); }
		}

		public ZString TotalNoOfPacksTitle
		{
			get
			{
				return !IsCMREntry || CusEntryHeader.IsSAC || !CusEntryHeader.IsNature20 ? "TOTAL NUMBER OF PACKAGES" : string.Empty;
			}
		}
		public ZString TotalNoOfWarehousePacksTitle
		{
			get { return IsCMREntry && (IsNature1020 || IsNature20) ? "TOTAL NUMBER OF WAREHOUSE PACKAGES" : string.Empty; }
		}

		public ZBool ShouldPrintCMRPackagesBills
		{
			get { return !IsCMREntry || CusEntryHeader.IsSAC || CusEntryHeader.IsNature30 || CusEntryHeader.Declaration.JE_TransportMode == Core.Constants.TransportModes.Other; }
		}

		public ZBool IsGSTDeferred
		{
			get { return AUCustomsEntryPrint.IsGSTDeferred; }
		}

		public DocCusEntryHeaderChargesCollection PayableCharges
		{
			get
			{
				if (fPayableCharges == null)
				{
					fPayableCharges = new DocCusEntryHeaderChargesCollection(Factory);
					foreach (CusEntryHeaderCharges charge in CusEntryHeader.Charges)
					{
						if (charge.IsPayableToCustomsForHeader)
						{
							fPayableCharges.Add(DocCusEntryHeaderCharges.New(charge, CusEntryHeader.Factory));
						}
					}
				}
				return fPayableCharges;
			}
		}

		public DocCusEntryLineFeeCollection PayableFees
		{
			get
			{
				if (fPayableFees == null)
				{
					fPayableFees = new DocCusEntryLineFeeCollection(Factory);
					foreach (CusEntryLine mergedLine in CusEntryHeader.MergedLines)
					{
						foreach (CusEntryLineFee fee in mergedLine.Fees)
						{
							if (fee.IsPayableToCustomsForLine)
							{
								fPayableFees.Add(DocCusEntryLineFee.New(fee, Factory));
							}
						}
					}
				}
				return fPayableFees;
			}
		}

		public DocPackingGroupCollection HouseBillContainersForEntry
		{
			get
			{
				if (fHouseBillContainersForEntry == null)
				{
					fHouseBillContainersForEntry = new DocPackingGroupCollection(CusEntryHeader.PackingGroups, Factory);
				}
				return fHouseBillContainersForEntry;
			}
		}

		public ZString TotalNumberOfPacakges
		{
			get
			{
				ZString result = ZString.Empty;

				if (AuthorityToDealMessage != null && !AuthorityToDealMessage.TotalNumberOfPacakges.IsEmpty)
				{
					result = AuthorityToDealMessage.TotalNumberOfPacakges;
				}

				if (result.IsEmpty)
				{
					if (CusEntryHeader.IsNature30)
					{
						result = CusEntryHeader.WarehouseNumberOfPacksForMessage.ToString();
					}
					else if (!HouseBillContainersForEntryTotalNumberOfPackages.IsEmpty)
					{
						result = HouseBillContainersForEntryTotalNumberOfPackages.ToString();
					}
				}

				return result;
			}
		}

		#endregion

		#region Authority To Deal Properties
		public const string HomeConsumptionAuthorityText = "This authority to take the goods into home " +
			"consumption is given under S71C of the Customs Act 1901.";
		public const string WarehousingGoodsAuthorityText = "This authority to take the goods into warehousing " +
			"is given under S71DJ of the Customs Atc 1901.";
		public const string HomeConsumptionAndWarehouseAuthorityText = "This authority to take the goods into " +
			"home consumption is given under S71C and to take the goods into warehousing is given under S71DJ " +
			"of the Customs Act 1901.";

		public ZString Nature
		{
			get { return CusEntryHeader.Nature; }
		}

		public ZString AuthorityText
		{
			get
			{
				ZString result = ZString.Empty;
				switch (Nature)
				{
					case CusEntryHeader.NatureTypesForImportCMR.Nature10:
					case CusEntryHeader.NatureTypesForImportCMR.Nature30:
						result = HomeConsumptionAuthorityText;
						break;
					case CusEntryHeader.NatureTypesForImportCMR.Nature20:
						result = WarehousingGoodsAuthorityText;
						break;
					case CusEntryHeader.NatureTypesForImportCMR.Nature1020:
						result = HomeConsumptionAndWarehouseAuthorityText;
						break;
				}

				return result;
			}
		}

		public ZInt HouseBillContainersForEntryLineCount
		{
			get { return HouseBillContainersForEntry.Count; }
		}

		public ZInt HouseBillContainersForEntryTotalNumberOfPackages
		{
			get
			{
				ZInt result = ZInt.Zero;
				foreach (DocPackingGroup pack in HouseBillContainersForEntry)
				{
					result += pack.NumberOfPackages;
				}
				return result;
			}
		}

		public ZInt MessageLinesCount
		{
			get { return MessageLines.Count; }
		}

		public ZBool IsCMRNature10
		{
			get { return CusEntryHeader.IsCMRNature10; }
		}

		public ZBool IsCMRNature1020
		{
			get { return CusEntryHeader.IsCMRNature1020; }
		}

		public ZBool IsCMRNature20
		{
			get { return CusEntryHeader.IsCMRNature20; }
		}

		public ZBool IsCMRNature30
		{
			get { return CusEntryHeader.IsCMRNature30; }
		}

		#region Nature Type

		public ZBool IsNature10 { get { return CusEntryHeader.IsNature10; } }

		public ZBool IsNature20 { get { return CusEntryHeader.IsNature20; } }

		public ZBool IsNature1020 { get { return CusEntryHeader.IsNature1020; } }

		public ZBool IsNature30 { get { return CusEntryHeader.IsNature30; } }
		#endregion

		public ZBool CMREntryMayHaveChangedPostLodge
		{
			get { return CusEntryHeader.CMREntryMayHaveChangedPostLodge; }
		}

		public ZBool IsCMREntry
		{
			get { return IsCMRNature10 || IsCMRNature1020 || IsCMRNature20 || IsCMRNature30; }
		}

		public ZBool HasWarehouseData
		{
			get { return IsNature20 || IsNature1020 || IsNature30; }
		}

		public ZString EntryPrintHeaderText
		{
			get { return ZString.Format("ENTRY PRINT{0}", IsCMREntry && CMREntryMayHaveChangedPostLodge ? " - ESTIMATE" : ""); }
		}

		public ZString EntryHeaderStatusDescription
		{
			get { return CusEntryHeader.EntryHeaderStatusDescription; }
		}

		public ZDecimal TotalGSTForEntry
		{
			get
			{
				var totalGSTDeferred = AUCustomsEntryPrint.TotalGSTDeferred;
				var result = ZDecimal.Zero;

				if (IsCMREntry)
				{
					if (IsGSTDeferred)
					{
						result = totalGSTDeferred != ZDecimal.Zero ? totalGSTDeferred : CusEntryHeader.GSTAmount;
					}
				}
				else
				{
					result = totalGSTDeferred;
				}

				return result;
			}
		}

		public ZString LastMessage
		{
			get { return AUCustomsEntryPrint.LastMessageContent; }
		}

		public ZString OwnerDetails
		{
			get { return AUCustomsEntryPrint.GetOwnerDetails(); }
		}

		public ZString EntryNo
		{
			get { return AUCustomsEntryPrint.EntryNumber; }
		}

		public ZString DestinationPort
		{
			get { return AUCustomsEntryPrint.DestinationPort; }
		}

		public DocAuthorityToDeal AuthorityToDealMessage
		{
			get
			{
				if (fAuthorityToDealMessage == null)
				{
					fAuthorityToDealMessage = DocAuthorityToDeal.New(CusEntryHeader.AuthorityToDealMessage, Factory);
				}
				return fAuthorityToDealMessage;
			}
		}

		public ZString HouseBillsCommaSeparated
		{
			get { return CusEntryHeader.HouseBillsCommaSeparated; }
		}

		public ZString WarehouseDetails
		{
			get { return AUCustomsEntryPrint.WarehouseAddress != null ? AUCustomsEntryPrint.WarehouseAddress.Header.OH_FullName.ToUpper().Left(30) : ZString.Empty; }
		}

		public ZString WarehousePremisesID
		{
			get { return AUCustomsEntryPrint.WarehouseAddress != null ? AUCustomsEntryPrint.WarehouseAddress.LocalControlledPremisesID : ZString.Empty; }
		}

		public ZString SupplierName
		{
			get { return CusEntryHeader.JZ_SupplierName; }
		}

		public ZString SupplierCode
		{
			get { return CusEntryHeader.JZ_SupplierCode; }
		}

		public MoneyWrapper FOBMoney
		{
			get { return new MoneyWrapper(CusEntryHeader.FOB, Factory); }
		}

		public ZString FOBCurrencyIndicator
		{
			get { return GetCurrencyIndicator(CusEntryHeader.FOB.Currency); }
		}

		public ZDecimal FOBRoundedAmount
		{
			get { return GetCurrencyValue(CusEntryHeader.FOBInLocalCurrency).Amount; }
		}

		public MoneyWrapper CIFMoney
		{
			get { return new MoneyWrapper(CusEntryHeader.CIF, Factory); }
		}

		public ZString CIFCurrencyIndicator
		{
			get { return GetCurrencyIndicator(CusEntryHeader.CIF.Currency); }
		}

		public ZDecimal CIFRoundedAmount
		{
			get { return GetCurrencyValue(CusEntryHeader.CIF).Amount; }
		}

		public WeightWrapper GrossWeight
		{
			get { return new WeightWrapper(CusEntryHeader.GrossWeight.Amount, CusEntryHeader.GrossWeight.Unit, 0, new CodeDescriptionPairList(), Factory); }
		}

		public ZDecimal GrossWeightInKg
		{
			get { return Constants.Weight.ContainsCode(CusEntryHeader.GrossWeight.Unit) ? Constants.Weight.Convert(CusEntryHeader.GrossWeight.Amount, CusEntryHeader.GrossWeight.Unit, Constants.Weight.Kilograms) : Decimal.Zero; }
		}

		public ZString TIValue
		{
			get
			{
				ZDecimal tandI = CusEntryHeader.TransportAndInsuranceInLocalCurrency.Amount;
				if (tandI.IsEmpty)
				{
					tandI = GetCurrencyValue(CusEntryHeader.OverseasFreight).Amount + GetCurrencyValue(CusEntryHeader.OverseasInsurance).Amount;
				}
				return tandI.ToString(2);
			}
		}

		protected Money GetCurrencyValue(Money amount)
		{
			if (CusEntryHeader.Declaration != null)
			{
				return CusEntryHeader.CurrencyConverter.ConvertRounded(amount, GetLocalCurrency());
			}
			else
			{
				return CurrencyConverter.New(CusEntryHeader.Factory).ConvertRounded(amount, GetLocalCurrency());
			}
		}

		public ZString[] GetPortDetailsText()
		{
			ZString[] array = new ZString[5];

			if (IsCMREntry)
			{
				if (CusEntryHeader.Declaration.IsPost)
				{
					if (CusEntryHeader.Declaration.PortOfLoading != null)
					{
						array[0] = CusEntryHeader.Declaration.PortOfLoading.RL_PortName.Left(27).ToUpper();
					}
					var arrival = "";
					if (CusEntryHeader.Declaration.PortOfArrival != null)
					{
						arrival = AUCustomsEntryPrint.GetParcelPostLocation();
					}
					arrival += ("   " + CusEntryHeader.Declaration.JE_DateOfArrival.ToString("ddMMMyy", CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture));
					array[1] = arrival;
					if (CusEntryHeader.Declaration.FinalDestination != null && !CusEntryHeader.IsCMRNature30)
					{
						array[2] = CusEntryHeader.Declaration.FinalDestination.RL_PortName.Left(27).ToUpper();
					}
				}
				else
				{
					if (CusEntryHeader.Declaration.PortOfLoading != null)
					{
						array[0] = CusEntryHeader.Declaration.PortOfLoading.RL_PortName.Left(27).ToUpper();
					}
					if (CusEntryHeader.Declaration.FinalDestination != null && !CusEntryHeader.IsCMRNature30)
					{
						array[2] = CusEntryHeader.Declaration.FinalDestination.RL_PortName.Left(27).ToUpper();
					}
					var first = "";
					if (CusEntryHeader.Declaration.PortOfFirstArrival != null)
					{
						first = CusEntryHeader.Declaration.PortOfFirstArrival.RL_PortName.Left(19).ToUpper();
					}
					first += ("   " + CusEntryHeader.Declaration.JE_DateOfFirstArrival.ToString("ddMMMyy", CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture));
					array[4] = first;
					var desh = "";
					if (CusEntryHeader.Declaration.PortOfArrival != null)
					{
						desh = CusEntryHeader.Declaration.PortOfArrival.RL_PortName.Left(19).ToUpper();
					}
					desh += ("   " + CusEntryHeader.Declaration.JE_DateOfArrival.ToString("ddMMMyy", CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture));
					array[3] = desh;
				}
			}
			else
			{
				if (CusEntryHeader.Declaration.IsPost)
				{
					if (CusEntryHeader.Declaration.PortOfLoading != null)
					{
						array[0] = CusEntryHeader.Declaration.PortOfLoading.RL_PortName.Left(27).ToUpper();
					}
					var arrival = "";
					if (CusEntryHeader.Declaration.PortOfArrival != null)
					{
						arrival = AUCustomsEntryPrint.GetParcelPostLocation();
					}
					arrival += ("   " + CusEntryHeader.Declaration.JE_DateOfArrival.ToString("ddMMMyy", CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture));
					array[1] = arrival;
				}
				else
				{
					if (CusEntryHeader.Declaration.PortOfLoading != null)
					{
						array[0] = CusEntryHeader.Declaration.PortOfLoading.RL_PortName.Left(27).ToUpper();
					}
					var first = "";
					if (CusEntryHeader.Declaration.PortOfFirstArrival != null)
					{
						first = CusEntryHeader.Declaration.PortOfFirstArrival.RL_PortName.Left(19).ToUpper();
					}
					first += ("   " + CusEntryHeader.Declaration.JE_DateOfFirstArrival.ToString("ddMMMyy", CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture));
					array[4] = first;
					var dsch = "";
					if (CusEntryHeader.Declaration.PortOfArrival != null)
					{
						dsch = CusEntryHeader.Declaration.PortOfArrival.RL_PortName.Left(19).ToUpper();
					}
					dsch += ("   " + CusEntryHeader.Declaration.JE_DateOfArrival.ToString("ddMMMyy", CultureInfo.CurrentCulture).ToUpper(CultureInfo.CurrentCulture));
					array[3] = dsch;
				}
			}
			return array;
		}

		public ZString LoadPort
		{
			get { return PortDetails[0]; }
		}

		public ZString ArrivalPort
		{
			get { return PortDetails[1]; }
		}
		public ZString DestlPort
		{
			get { return PortDetails[2]; }
		}

		public ZString DschlPort
		{
			get { return PortDetails[3]; }
		}

		public ZString FirstPort
		{
			get { return PortDetails[4]; }
		}

		public MoneyWrapper InvoiceTotalMoney
		{
			get { return new MoneyWrapper(CusEntryHeader.InvoiceTotal, Factory); }
		}

		public ZString InvoiceTotalCurrencyIndicator
		{
			get { return GetCurrencyIndicator(CusEntryHeader.InvoiceTotal.Currency); }
		}

		public ZDecimal InvoiceTotalRoundedAmount
		{
			get { return GetCurrencyValue(CusEntryHeader.InvoiceTotal).Amount; }
		}

		internal static RefCurrency GetLocalCurrency()
		{
			return RefCurrency.LoadFromCurrencyCode(GlbCompany.CurrentCompany.Factory, Core.Constants.CurrencyCodes.Australia);
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
						currencyIndicatorReturned = "(" + (i + 1) + ")";
						break;
					}
				}
			}
			return currencyIndicatorReturned;
		}

		protected ArrayList WhatCurrenciesUsed()
		{
			if (CurrenciesUsed == null)
			{
				CurrenciesUsed = new ArrayList();
				if (IsCMREntry)
				{
					AddCurrencyUsed(CusEntryHeader.InvoiceTotal.Currency);
					foreach (JobComInvoiceHeader header in CusEntryHeader.InvoiceHeaders)
					{
						AddCurrencyUsed(header.InvoiceAmount.Currency);
					}
					foreach (ICurrency currency in CusEntryHeader.UsedCurrencies)
					{
						AddCurrencyUsed(currency);
					}
				}
				else
				{
					CurrenciesUsed.Add(CusEntryHeader.InvoiceTotal.Currency);
					int currenciesUsedCount = CusEntryHeader.UsedCurrencies.Length;
					if (currenciesUsedCount > 1)
					{
						CurrenciesUsed.Add(CusEntryHeader.UsedCurrencies[1]);
					}
					if (currenciesUsedCount > 2)
					{
						CurrenciesUsed.Add(CusEntryHeader.UsedCurrencies[2]);
					}
				}
			}
			return CurrenciesUsed;
		}
		ArrayList CurrenciesUsed;

		void AddCurrencyUsed(ICurrency currency)
		{
			if (currency != null)
			{
				foreach (ICurrency currencyInArray in CurrenciesUsed)
				{
					if (currencyInArray.Code == currency.Code)
					{
						return;
					}
				}
				CurrenciesUsed.Add(currency);
			}
		}

		public MoneyWrapper PackingCostsMoney
		{
			get { return new MoneyWrapper(CusEntryHeader.PackingCosts, Factory); }
		}

		public ZString PackingCostsCurrencyIndicator
		{
			get { return GetCurrencyIndicator(CusEntryHeader.PackingCosts.Currency); }
		}

		public ZDecimal PackingCostsRoundedAmount
		{
			get { return GetCurrencyValue(CusEntryHeader.PackingCosts).Amount; }
		}

		public MoneyWrapper OverseasFreightMoney
		{
			get { return new MoneyWrapper(CusEntryHeader.OverseasFreight, Factory); }
		}

		public ZString OverseasFreightCurrencyIndicator
		{
			get { return GetCurrencyIndicator(CusEntryHeader.OverseasFreight.Currency); }
		}

		public ZDecimal OverseasFreightRoundedAmount
		{
			get { return GetCurrencyValue(CusEntryHeader.OverseasFreight).Amount; }
		}

		public MoneyWrapper OverseasInsuranceMoney
		{
			get { return new MoneyWrapper(CusEntryHeader.OverseasInsurance, Factory); }
		}

		public ZString OverseasInsuranceCurrencyIndicator
		{
			get { return GetCurrencyIndicator(CusEntryHeader.OverseasInsurance.Currency); }
		}

		public ZDecimal OverseasInsuranceRoundedAmount
		{
			get { return GetCurrencyValue(CusEntryHeader.OverseasInsurance).Amount; }
		}

		public MoneyWrapper LandingChargesMoney
		{
			get { return new MoneyWrapper(CusEntryHeader.LandingCharges, Factory); }
		}

		public ZString LandingChargesCurrencyIndicator
		{
			get { return GetCurrencyIndicator(CusEntryHeader.LandingCharges.Currency); }
		}

		public ZDecimal LandingChargesRoundedAmount
		{
			get { return GetCurrencyValue(CusEntryHeader.LandingCharges).Amount; }
		}

		public MoneyWrapper ForeignInlandFreightMoney
		{
			get { return new MoneyWrapper(CusEntryHeader.ForeignInlandFreight, Factory); }
		}

		public ZString ForeignInlandFreightCurrencyIndicator
		{
			get { return GetCurrencyIndicator(CusEntryHeader.ForeignInlandFreight.Currency); }
		}

		public ZDecimal ForeignInlandFreightRoundedAmount
		{
			get { return GetCurrencyValue(CusEntryHeader.ForeignInlandFreight).Amount; }
		}

		public MoneyWrapper DiscountMoney
		{
			get { return new MoneyWrapper(CusEntryHeader.Discount, Factory); }
		}

		public ZString DiscountCurrencyIndicator
		{
			get { return GetCurrencyIndicator(CusEntryHeader.Discount.Currency); }
		}

		public ZDecimal DiscountRoundedAmount
		{
			get { return GetCurrencyValue(CusEntryHeader.Discount).Amount; }
		}

		public MoneyWrapper CommissionMoney
		{
			get { return new MoneyWrapper(CusEntryHeader.Commission, Factory); }
		}

		public ZString CommissionCurrencyIndicator
		{
			get { return GetCurrencyIndicator(CusEntryHeader.Commission.Currency); }
		}

		public ZDecimal CommissionRoundedAmount
		{
			get { return GetCurrencyValue(CusEntryHeader.Commission).Amount; }
		}

		public MoneyWrapper OtherCharges1Money
		{
			get { return new MoneyWrapper(CusEntryHeader.OtherCharges1, Factory); }
		}

		public ZString OtherCharges1CurrencyIndicator
		{
			get { return GetCurrencyIndicator(CusEntryHeader.OtherCharges1.Currency); }
		}

		public ZDecimal OtherCharges1RoundedAmount
		{
			get { return GetCurrencyValue(CusEntryHeader.OtherCharges1).Amount; }
		}

		public MoneyWrapper OtherCharges2Money
		{
			get { return new MoneyWrapper(CusEntryHeader.OtherCharges2, Factory); }
		}

		public ZString OtherCharges2CurrencyIndicator
		{
			get { return GetCurrencyIndicator(CusEntryHeader.OtherCharges2.Currency); }
		}

		public ZDecimal OtherCharges2RoundedAmount
		{
			get { return GetCurrencyValue(CusEntryHeader.OtherCharges2).Amount; }
		}

		List<ZString> PortDetails
		{
			get { return portDetails ?? (portDetails = new List<ZString>(GetPortDetailsText())); }
		}
		List<ZString> portDetails;

		public ZString ITOTIncoTerm
		{
			get { return CusEntryHeader.ITOTIncoTerm; }
		}

		public ZDateTime EffectiveValuationDate
		{
			get { return CusEntryHeader.EffectiveValuationDate; }
		}

		public ZDecimal CustomsValueInAUD
		{
			get { return CusEntryHeader.CustomsValueInAUD.Amount; }
		}
		public ZString Currencies
		{
			get { return (currencies ?? (currencies = AUCustomsEntryPrint.CurrenciesList)).ToString(); }
		}
		ZStringBuilder currencies;

		public DocAuthorityToDealLineConditionDetailsCollection MessageLines
		{
			get
			{
				if (AuthorityToDealMessage != null)
				{
					return AuthorityToDealMessage.MessageLines;
				}
				else
				{
					return new DocAuthorityToDealLineConditionDetailsCollection(Factory);
				}
			}
		}
		protected DocAuthorityToDeal fAuthorityToDealMessage;
		#endregion

		#region Implementation

		DocCusEntryLineCollection fMergedLines;
		DocCusEntryHeaderChargesCollection fPayableCharges;
		DocCusEntryLineFeeCollection fPayableFees;
		DocPackingGroupCollection fHouseBillContainersForEntry;

		CusEntryHeader CusEntryHeader
		{
			get { return (CusEntryHeader)WrappedObject; }
		}

		#endregion
	}
}
