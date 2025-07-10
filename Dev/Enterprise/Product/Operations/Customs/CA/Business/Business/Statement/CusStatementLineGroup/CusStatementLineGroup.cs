using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class CusStatementLineGroup : AutoCusStatementLineGroup
	{
		public CusStatementLineGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		[RelatedBusinessObject(nameof(StatementHeader))]
		public override ZGuid B10_B2
		{
			get => base.B10_B2;
			set
			{
				var oldValue = B10_B2;
				base.B10_B2 = value;
				if (!IsCopying && oldValue != B10_B2)
				{
					RefreshStatementLines();
				}
			}
		}

		public CusStatementHeader StatementHeader => Factory.Load<CusStatementHeader>(B10_B2);

		public override ZString B10_ImporterCustomsID
		{
			get => base.B10_ImporterCustomsID;
			set
			{
				var oldValue = B10_ImporterCustomsID;
				base.B10_ImporterCustomsID = value;
				if (!IsCopying && oldValue != B10_ImporterCustomsID)
				{
					RefreshStatementLines();
				}
			}
		}

		[ResourceStringData("CACusStatementLine|PaymentMethod", Caption = "Payment Method")]
		public ZString PaymentMethod
		{
			get
			{
				if (paymentMethodCached == null)
				{
					paymentMethodCached = new CachedProperty<ZString>(Factory, () =>
					{
						var paymentMethods = (StatementHeader?.B2_IsMonthlyStatement ?? true) ? Array.Empty<ZString>() : StatementLines.Where(c => c.IsNormalLine).Select(x => x.PaymentMethod).Distinct().Take(2).ToArray();
						return paymentMethods.Length == 1 ? paymentMethods[0] : new ZString(paymentMethods.Length > 1 ? Res.GetString("{326B9DE9-FB47-426D-B05B-19EBDF19A54F}", "MULTIPLE") : string.Empty);
					});
				}
				return paymentMethodCached.Value;
			}
		}
		CachedProperty<ZString> paymentMethodCached;

		[ResourceStringData("CACusStatementLine|TotalPayableByBroker", Caption = "Total Payable By Broker", ShortCaption = "Broker Payable")]
		public ZDecimal TotalPayableByBrokerOnDailyStatement
		{
			get
			{
				CalculateTotalPayableBy();
				return totalPayableByBroker.Value;
			}
		}
		ZDecimal? totalPayableByBroker;

		[ResourceStringData("CACusStatementLine|TotalPayableByImporter", Caption = "Total Payable By Importer", ShortCaption = "Importer Payable")]
		public ZDecimal TotalPayableByImporterOnDailyStatement
		{
			get
			{
				CalculateTotalPayableBy();
				return totalPayableByImporter.Value;
			}
		}
		ZDecimal? totalPayableByImporter;

		void CalculateTotalPayableBy()
		{
			if (totalPayableByBroker == null || totalPayableByImporter == null)
			{
				totalPayableByBroker = ZDecimal.Zero;
				totalPayableByImporter = ZDecimal.Zero;

				foreach (var statementLine in StatementLines.Where(x => x.IsNormalLine))
				{
					foreach (CusStatementLineCharge charge in statementLine.Charges)
					{
						if (charge.B4_PaymentParty == PaymentPartyCodeDescriptionList.Codes.Importer)
						{
							totalPayableByImporter += charge.B4_ChargeAmount;
						}
						else
						{
							totalPayableByBroker += charge.B4_ChargeAmount;
						}
					}
				}
			}
		}
		#endregion

		#region Importer Summary Properties

		public ZDecimal B10_PreviousMonthlyStatementTotal => GetAmount(PostingJournalTypeList.Codes.PreviousMonthlyStatementTotal);

		public ZDecimal B10_PaymentReceivedSinceLastMonthlyStatement => GetAmount(PostingJournalTypeList.Codes.PaymentReceivedSinceLastMonthlyStatement);

		public ZDecimal B10_Refund => GetAmount(PostingJournalTypeList.Codes.Refund);

		public ZDecimal B10_UnpaidBalanceForward => GetAmount(PostingJournalTypeList.Codes.UnpaidBalanceForward);

		public ZDecimal B10_ArrearsInterest => GetAmount(PostingJournalTypeList.Codes.ArrearsInterest);

		public ZDecimal B10_TransactionTotal => GetAmount(PostingJournalTypeList.Codes.TransactionTotal);

		public ZDecimal B10_OtherCharges => GetAmount(PostingJournalTypeList.Codes.OtherCharges);

		public ZDecimal B10_TotalCredits => GetAmount(PostingJournalTypeList.Codes.TotalCredits);

		public ZDecimal B10_InterestAmount => GetAmount(PostingJournalTypeList.Codes.InterestAmount);

		public ZDecimal B10_GIPastTotal => GetAmount(PostingJournalTypeList.Codes.InstalmentLastAmount);
		public ZDecimal B10_GICurrentTotal => GetAmount(PostingJournalTypeList.Codes.InstalmentCurrentAmount);

		public ZDecimal B10_TotalPayableForImporterSoAStatement => GetAmount(PostingJournalTypeList.Codes.TotalPayableForImporter);

		public ZDecimal B10_TotalPayableForBrokerSoAStatement
		{
			get
			{
				var result = GetAmount(PostingJournalTypeList.Codes.TotalPayableForBroker);
				return result > 0m ? result : GetAmount(PostingJournalTypeList.Codes.TotalPayableForBrokerOld);
			}
		}

		public ZDecimal B10_TotalPaymentReceived => GetAmount(PostingJournalTypeList.Codes.TotalPaymentReceived);

		public ZDecimal TotalDuties => CalculateTotalAmount(() => totalDuties);
		ZDecimal? totalDuties;

		public ZDecimal TotalSIMA => CalculateTotalAmount(() => totalSIMA);
		ZDecimal? totalSIMA;

		public ZDecimal TotalExcise => CalculateTotalAmount(() => totalExcise);
		ZDecimal? totalExcise;

		public ZDecimal TotalExciseDuties => CalculateTotalAmount(() => totalExciseDuties);
		ZDecimal? totalExciseDuties;

		public ZDecimal TotalGSTOrGSD => CalculateTotalAmount(() => totalGSTOrGSD);
		ZDecimal? totalGSTOrGSD;

		public ZDecimal TotalOthers => CalculateTotalAmount(() => totalOthers);
		ZDecimal? totalOthers;

		public ZDecimal TotalInterests => CalculateTotalAmount(() => totalInterests);
		ZDecimal? totalInterests;

		public ZDecimal Total => TotalDuties + TotalSIMA + TotalExcise + TotalExciseDuties + TotalGSTOrGSD + TotalOthers + TotalInterests;

		ZDecimal CalculateTotalAmount(Func<ZDecimal?> fvalueGetter)
		{
			if (!fvalueGetter().HasValue)
			{
				totalDuties = 0m;
				totalSIMA = 0m;
				totalExcise = 0m;
				totalExciseDuties = 0m;
				totalGSTOrGSD = 0m;
				totalOthers = 0m;
				totalInterests = 0m;

				if (StatementHeader.IsCARMDailyNotice)
				{
					foreach (var line in StatementLines.Cast<CusStatementLine>())
					{
						totalDuties += line.B4_CARMDNChargeAmount_Duties;
						totalSIMA += line.B4_CARMDNChargeAmount_SIMA;
						totalExcise += line.B4_CARMDNChargeAmount_ExciseTax;
						totalExciseDuties += line.B4_CARMDNChargeAmount_ExciseDuties;
						totalGSTOrGSD += line.B4_CARMDNChargeAmount_GSTAndHSTAndPST;
						totalInterests += line.B4_CARMDNChargeAmount_Interests;
						totalOthers += line.B4_CARMDNChargeAmount_Others;
					}
				}
				else
				{
					foreach (var line in StatementLines.Cast<CusStatementLine>().Where(x => x.IsNormalLine))
					{
						totalDuties += line.B4_ChargeAmountDTY;
						totalSIMA += line.B4_ChargeAmountSIM;
						totalExcise += line.B4_ChargeAmountEXS;
						totalGSTOrGSD += line.B4_ChargeAmountGSTOrGSD;
						totalOthers += line.B4_ChargeAmountOTH;
					}
				}
			}

			return fvalueGetter().Value;
		}

		ZDecimal GetAmount(ZString type)
		{
			return Utilities.Round(FinancialDetailCollection.Find(type)?.B11_Amount ?? ZDecimal.Zero, 2);
		}

		public ZString B10_ImporterName
		{
			get { return Importer?.OH_FullName ?? ZString.Empty; }
		}

		public ZString B10_ImporterCode
		{
			get { return Importer?.OH_Code ?? ZString.Empty; }
		}

		#endregion

		#region Related BusinessObjects

		[ChildEditable(true)]
		public CusStatementLineGroupFinancialDetailCollection FinancialDetailCollection
		{
			get
			{
				if (financialDetailCollection == null)
				{
					financialDetailCollection = new CusStatementLineGroupFinancialDetailCollection(this);
					financialDetailCollection.Load();

					RegisterEditableChildObject(financialDetailCollection);
				}

				return financialDetailCollection;
			}
		}

		CusStatementLineGroupFinancialDetailCollection financialDetailCollection;

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
		public CusStatementLine[] StatementLines
		{
			get
			{
				if (statementLines == null)
				{
					var importerCustomsID = B10_ImporterCustomsID;
					var header = importerCustomsID.IsEmpty ? null : StatementHeader;
					statementLines = header?.StatementLines.OfType<CusStatementLine>().Where(x => x.B3_ImporterCustomsID == importerCustomsID).ToArray() ?? Array.Empty<CusStatementLine>();
				}
				return statementLines;
			}
		}
		CusStatementLine[] statementLines;

		public void RefreshStatementLines()
		{
			statementLines = null;

			totalPayableByBroker = null;
			totalPayableByImporter = null;

			totalDuties = null;
			totalExcise = null;
			totalGSTOrGSD = null;
			totalOthers = null;
			totalSIMA = null;
		}

		#endregion

		public void UpdateImporter()
		{
			var importerBusinessNumber = B10_ImporterCustomsID;

			var importers = TransactionBatchExtension.GetOrgsFromBN(importerBusinessNumber, ZString.Empty, Factory);
			if (importers.Length != 1)
			{
				importers = StatementHeader
					.StatementLines.OfType<CusStatementLine>()
					.Where(x => x.B3_ImporterCustomsID == importerBusinessNumber)
					.Select(x => x.Declaration?.EffectiveImporter)
					.Where(x => x != null)
					.Distinct()
					.Take(2)
					.ToArray();
			}

			B10_OH_Importer = importers.Length == 1 ? importers[0].PK : ZGuid.Empty;
		}

		#region Override

		public override void Delete()
		{
			if (!IsDeleted)
			{
				FinancialDetailCollection.RemoveAndDeleteAll();
			}

			base.Delete();
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var importer = Importer?.OH_Code ?? ZString.Empty;
				return importer.IsEmpty
					? Res.GetString("06CF04AE-717A-4314-96EA-D21D1EEB755A", "Statement Line Group - {0}", B10_ImporterCustomsID)
					: Res.GetString("C20BFEA3-17A8-461A-9ACB-19F0D0ADC522", "Statement Line Group - ({0}){1}", importer, B10_ImporterCustomsID);
			}
		}

		#endregion
	}
}
