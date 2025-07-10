using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Edifact.D99B.Elements;
using Enterprise.Edifact.D99B.Messages.CUSDEC;
using Enterprise.Edifact.D99B.Segments;
using Enterprise.Messaging.MessageProcessors;
using CUSDECMessage = Enterprise.Edifact.CA.D99B.Messages.CUSDEC.CUSDECMessage;
using SegmentGroup10 = Enterprise.Edifact.CA.D99B.Messages.CUSDEC.SegmentGroup10;
using SegmentGroup11 = Enterprise.Edifact.CA.D99B.Messages.CUSDEC.SegmentGroup11;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	class K84MonthlyReportDocumentWrapper : NonPersistentBusinessObject, IDocumentWrapper, ISourceIdentifierProvider
	{
		internal K84MonthlyReportDocumentWrapper(K84Message message)
			: base(message.Factory)
		{
			Argument.NotNull(message, "message");
			if (message.EM_MessageSubType != K84ReportTypes.Codes.Monthly)
			{
				throw new ArgumentException("K84MonthlyReportDocumentWrapper is for MON message only but was " + message.EM_MessageSubType);
			}
			this.k84Message = message;
			this.message = (CUSDECMessage)message.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			Argument.NotNull(this.message, "message", "Supported EDIFACT message type is D99B CUSDEC");
		}

		readonly K84Message k84Message;
		readonly CUSDECMessage message;

		public ZDate StatementDate
		{
			get { return D99BMessageUtilities.GetDate(message.DTM, DateTimePeriodFunctionCodeQualifierList.CurrentReportDate); }
		}

		#region Accounting Office/Security Numbers

		public ZString AccountingOffice
		{
			get
			{
				return (from LOCSegment loc in message.LOC
						where loc.LocationFunctionCodeQualifier == LocationFunctionCodeQualifierList.CustomsOfficeOfRegistrationOfPreviousCustomsDeclaration
						select loc.LocationIdentification.LocationName).FirstOrDefault();
			}
		}

		public ZString AccountSecurityNumber
		{
			get
			{
				return message.Group1.Count == 0 ? ZString.Empty
								 : D99BMessageUtilities.GetReference(message.Group1[0].RFF, ReferenceFunctionCodeQualifierList.DeclarantsCustomsIdentityNumber);
			}
		}

		#endregion

		#region DailyAccountingTotals

		public BusinessObjectCollectionWrapper<DailyAccountingTotal> DailyAccountingTotals
		{
			get { return GetTotals(g => new DailyAccountingTotal(g), "K50"); }
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class DailyAccountingTotal : NonPersistentBusinessObject, ITableInterpretation
		{
			internal DailyAccountingTotal(SegmentGroup10 group10)
			{
				Argument.NotNull(group10, "group10");
				this.group10 = group10;
			}

			[ColumnName]
			public ZDate NoticeDate
			{
				get { return D99BMessageUtilities.GetDate(group10.DTM, DateTimePeriodFunctionCodeQualifierList.DocumentMessageDateTime); }
			}

			public K84AmountsWrapper Amounts
			{
				get { return (from SegmentGroup11 group11 in group10.Group11 select new K84AmountsWrapper(group11.MOA)).FirstOrDefault(); }
			}

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return Res.GetString("42de7ebc-d363-4011-a28d-0b03aeb08edd", "Daily Unadjusted Totals"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return PropertyNameProvider.GetColumnTitles<DailyAccountingTotal>().Concat(PropertyNameProvider.GetColumnTitles<K84AmountsWrapper>()); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get { return new object[] { NoticeDate }.Concat(Amounts.ToArray()); }
			}

			#endregion

			readonly SegmentGroup10 group10;
		}

		#endregion

		#region MonthlyUnAdjustedTotals

		public MonthlyUnAdjustedTotal MonthlyUnAdjustedTotals
		{
			get { return GetTotal(g => new MonthlyUnAdjustedTotal(g), "K51"); }
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class MonthlyUnAdjustedTotal : NonPersistentBusinessObject, ITableValues
		{
			internal MonthlyUnAdjustedTotal(SegmentGroup10 group10)
			{
				Argument.NotNull(group10, "group10");
				this.group10 = group10;
			}

			public K84AmountsWrapper Amounts
			{
				get { return (from SegmentGroup11 group11 in group10.Group11 select new K84AmountsWrapper(group11.MOA)).FirstOrDefault(); }
			}

			#region Implementation of ITableValues

			IEnumerable<object> ITableValues.Values
			{
				get { return new object[] { GetTotalCell() }.Concat(Amounts.ToArray()); }
			}

			#endregion

			readonly SegmentGroup10 group10;
		}

		#endregion

		#region IndividualTransactionCorrections

		public BusinessObjectCollectionWrapper<IndividualTransactionCorrection> TransactionCorrections
		{
			get { return GetTotals(g => new IndividualTransactionCorrection(Factory, g, AccountSecurityNumber), "K52"); }
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class IndividualTransactionCorrection : NonPersistentBusinessObject, ITableInterpretation
		{
			internal IndividualTransactionCorrection(BusinessObjectFactory factory, SegmentGroup10 group10, ZString accountSecurityNumber) : base(factory)
			{
				Argument.NotNull(group10, "group10");
				this.group10 = group10;
				this.accountSecurityNumber = accountSecurityNumber;
			}

			[ColumnName(2)]
			public ZString JobNumber
			{
				get { return GetJobLink(Factory, accountSecurityNumber, TransactionNumber); }
			}

			[ColumnName(3)]
			public ZString TransactionNumber
			{
				get { return group10.DMS[0].TotalNumberOfItems; }
			}

			[ColumnName(1)]
			public ZDate NoticeDate
			{
				get { return D99BMessageUtilities.GetDate(group10.DTM, DateTimePeriodFunctionCodeQualifierList.DocumentMessageDateTime); }
			}

			public K84AmountsWrapper Amounts
			{
				get { return (from SegmentGroup11 group11 in group10.Group11 select new K84AmountsWrapper(group11.MOA)).FirstOrDefault(); }
			}

			readonly SegmentGroup10 group10;
			readonly ZString accountSecurityNumber;

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return Res.GetString("97cb06ba-e02e-4c21-824e-3c9b8616b886", "Transaction Corrections"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return PropertyNameProvider.GetColumnTitles<IndividualTransactionCorrection>().Concat(PropertyNameProvider.GetColumnTitles<K84AmountsWrapper>()); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get { return new object[] { NoticeDate, JobNumber, TransactionNumber }.Concat(Amounts.ToArray()); }
			}

			#endregion
		}

		#endregion

		#region TotalTransactionCorrections

		public TotalTransactionCorrection TotalTransactionCorrections
		{
			get { return GetTotal(g => new TotalTransactionCorrection(g), "K53"); }
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class TotalTransactionCorrection : NonPersistentBusinessObject, ITableValues
		{
			internal TotalTransactionCorrection(SegmentGroup10 group10)
			{
				Argument.NotNull(group10, "group10");
				this.group10 = group10;
			}

			public K84AmountsWrapper Amounts
			{
				get { return (from SegmentGroup11 group11 in group10.Group11 select new K84AmountsWrapper(group11.MOA)).FirstOrDefault(); }
			}

			#region Implementation of ITableValues

			IEnumerable<object> ITableValues.Values
			{
				get { return new object[] { GetTotalCell(3) }.Concat(Amounts.ToArray()); }
			}

			#endregion

			readonly SegmentGroup10 group10;
		}

		#endregion

		#region PeriodicInterimPaymentByNotice/Transaction

		public BusinessObjectCollectionWrapper<PeriodicInterimPayment> PeriodicInterimPaymentsByNotice
		{
			get { return GetTotals(g => new PeriodicInterimPayment(Factory, g, AccountSecurityNumber), "K54"); }
		}

		public BusinessObjectCollectionWrapper<PeriodicInterimPayment> PeriodicInterimPaymentsByTransaction
		{
			get { return GetTotals(g => new PeriodicInterimPayment(Factory, g, AccountSecurityNumber), "K56"); }
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class PeriodicInterimPayment : NonPersistentBusinessObject, ITableInterpretation
		{
			internal PeriodicInterimPayment(BusinessObjectFactory factory, SegmentGroup10 group10, ZString accountSecurityNumber) : base(factory)
			{
				Argument.NotNull(group10, "group10");
				this.group10 = group10;
				this.accountSecurityNumber = accountSecurityNumber;
			}

			[ColumnName(2)]
			public ZString JobNumber
			{
				get { return GetJobLink(Factory, accountSecurityNumber, TransactionNumber); }
			}

			[ColumnName(3)]
			public ZString TransactionNumber
			{
				get { return group10.DMS[0].TotalNumberOfItems; }
			}

			[ColumnName(1)]
			public ZDate NoticeDate
			{
				get { return D99BMessageUtilities.GetDate(group10.DTM, DateTimePeriodFunctionCodeQualifierList.DocumentMessageDateTime); }
			}

			public K84AmountsWithPenaltyWrapper Amounts
			{
				get { return (from SegmentGroup11 group11 in group10.Group11 select new K84AmountsWithPenaltyWrapper(group11.MOA)).FirstOrDefault(); }
			}

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get
				{
					return IsByNotice ? Res.GetString("c849f139-64d5-4e2a-8c2d-c0400c680c90", "Periodic Interim Payments by Notice")
									 : Res.GetString("bbca18a1-bb1d-4142-9277-bfd7a427e53b", "Periodic Interim Payments by Transaction");
				}
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get
				{
					var columnTitles = PropertyNameProvider.GetColumnTitles<PeriodicInterimPayment>();
					if (IsByNotice)
					{
						columnTitles = new[] { columnTitles.First() };
					}

					return columnTitles.Concat(PropertyNameProvider.GetColumnTitles<K84AmountsWithPenaltyWrapper>());
				}
			}

			IEnumerable<object> ITableValues.Values
			{
				get { return (IsByNotice ? new object[] { NoticeDate } : new object[] { NoticeDate, JobNumber, TransactionNumber }).Concat(Amounts.ToArray()); }
			}

			bool IsByNotice
			{
				get { return group10.DMS[0].DocumentMessageIdentification.DocumentMessageNumber == "K54"; }
			}

			#endregion

			readonly SegmentGroup10 group10;
			readonly ZString accountSecurityNumber;
		}

		#endregion

		#region Late Penalties And Interest Charges

		public BusinessObjectCollectionWrapper<LatePenalty> LatePenaltiesAndInterestCharges
		{
			get
			{
				var penalties = LateFilingPenalties.Concat(LateAccountingInterestCharges).Concat(LateK84InterestCharges);
				return new BusinessObjectCollectionWrapper<LatePenalty>(penalties.Cast<LatePenalty>());
			}
		}

		public BusinessObjectCollectionWrapper<LatePenalty> LateFilingPenalties
		{
			get { return GetTotals(g => new LatePenalty(Factory, g, AccountSecurityNumber), LatePenalty.K60); }
		}

		public BusinessObjectCollectionWrapper<LatePenalty> LateAccountingInterestCharges
		{
			get { return GetTotals(g => new LatePenalty(Factory, g, AccountSecurityNumber), LatePenalty.K61); }
		}

		public BusinessObjectCollectionWrapper<LatePenalty> LateK84InterestCharges
		{
			get { return GetTotals(g => new LatePenalty(Factory, g, AccountSecurityNumber), LatePenalty.K62); }
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class LatePenalty : NonPersistentBusinessObject, ITableInterpretation
		{
			internal LatePenalty(BusinessObjectFactory factory, SegmentGroup10 group10, ZString accountSecurityNumber) : base(factory)
			{
				Argument.NotNull(group10, "group10");
				this.group10 = group10;
				Record = group10.DMS[0].DocumentMessageIdentification.DocumentMessageNumber;
				this.accountSecurityNumber = accountSecurityNumber;
			}

			[ColumnName(2)]
			public ZString JobNumber
			{
				get { return GetJobLink(Factory, accountSecurityNumber, TransactionNumber); }
			}

			[ColumnName(3)]
			public ZString TransactionNumber
			{
				get { return group10.DMS[0].TotalNumberOfItems; }
			}

			[ColumnName(1)]
			public ZDate StatementDate
			{
				get { return D99BMessageUtilities.GetDate(group10.DTM, DateTimePeriodFunctionCodeQualifierList.DocumentMessageDateTime); }
			}

			public ZDecimal Amount
			{
				get
				{
					return (from SegmentGroup11 group11 in group10.Group11
							select D99BMessageUtilities.GetAmount(group11.MOA, MonetaryAmountTypeCodeQualifierList.PenaltyAmount)).FirstOrDefault();
				}
			}

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return Res.GetString("fc672a48-03f6-4cfc-ade6-3ebb64ab7a37", "Late Penalties And Interest Charges"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return PropertyNameProvider.GetColumnTitles<LatePenalty>().Concat(PropertyNameProvider.GetColumnTitles<TotalLateFilingAndInterest>()); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get
				{
					var lateFilingPenalty = Record == K60 ? Amount.ToString() : string.Empty;
					var lateAccountInterest = Record == K61 ? Amount.ToString() : string.Empty;
					var lateK84Interest = Record == K62 ? Amount.ToString() : string.Empty;
					return new object[] { StatementDate, JobNumber, TransactionNumber, lateFilingPenalty, lateAccountInterest, lateK84Interest, string.Empty };
				}
			}

			#endregion

			internal const string K62 = "K62";
			internal const string K61 = "K61";
			internal const string K60 = "K60";

			readonly SegmentGroup10 group10;
			readonly ZString accountSecurityNumber;
			public ZString Record { get; private set; }
		}

		#endregion

		#region TotalLateFilingAndInterestDetails

		public TotalLateFilingAndInterest TotalLateFilingAndInterestDetails
		{
			get { return GetTotal(g => (from SegmentGroup11 group11 in g.Group11 select new TotalLateFilingAndInterest(group11.MOA)).FirstOrDefault(), "K65"); }
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class TotalLateFilingAndInterest : NonPersistentBusinessObject, ITableValues
		{
			internal TotalLateFilingAndInterest(MOASegmentMessageSection moaSection)
			{
				Argument.NotNull(moaSection, "moaSection");
				this.moaSection = moaSection;
			}

			[ColumnName(1)]
			public ZDecimal LateFilingPenalty
			{
				get { return D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.PenaltyAmount); }
			}

			[ColumnName(2)]
			public ZDecimal LateAccountInterest
			{
				get { return D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.InterestAmount); }
			}

			[ColumnName(3)]
			public ZDecimal LateK84Interest
			{
				get { return D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.AdditionalAmountCoveredInterest); }
			}

			[ColumnName(4)]
			public ZDecimal TotalPenaltiesAndInterest
			{
				get { return D99BMessageUtilities.GetAmount(moaSection, MonetaryAmountTypeCodeQualifierList.TotalAmount); }
			}

			#region Implementation of ITableValues

			public IEnumerable<object> Values
			{
				get { return new object[] { GetTotalCell(3), LateFilingPenalty, LateAccountInterest, LateK84Interest, TotalPenaltiesAndInterest }; }
			}

			#endregion

			protected MOASegmentMessageSection moaSection;
		}

		#endregion

		#region InterestRatesAssessedForLateAccountingAndK84Payment

		public InterestRatesAssessedForLateAccountingAndK84Payment InterestRates
		{
			get { return GetTotal(g => new InterestRatesAssessedForLateAccountingAndK84Payment(g), "K66"); }
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class InterestRatesAssessedForLateAccountingAndK84Payment : NonPersistentBusinessObject, ITableInterpretation
		{
			internal InterestRatesAssessedForLateAccountingAndK84Payment(SegmentGroup10 group10)
			{
				Argument.NotNull(group10, "group10");
				this.group10 = group10;
			}

			#region Interest Rate

			[ColumnName(2)]
			public ZDate InterestEffectiveDate
			{
				get { return D99BMessageUtilities.ParseDate(InterestEffectiveExpiryDateString.Left(8)); }
			}

			[ColumnName(3)]
			public ZDate InterestExpiryDate
			{
				get { return D99BMessageUtilities.ParseDate(InterestEffectiveExpiryDateString.Right(8)); }
			}

			[ColumnName(1)]
			public ZDecimal InterestPercentage
			{
				get { return GetRate(PercentageTypeCodeQualifierList.InterestPercentage); }
			}

			ZString InterestEffectiveExpiryDateString
			{
				get { return interestEffectiveExpiryDateString ?? (interestEffectiveExpiryDateString = GetEffectiveExpiryDateString(PaymentTermsTypeCodeQualifierList.Basic)); }
			}

			string interestEffectiveExpiryDateString;

			#endregion

			#region Penalty Rate

			[ColumnName(5)]
			public ZDate PenaltyEffectiveDate
			{
				get { return D99BMessageUtilities.ParseDate(PenaltyEffectiveExpiryDateString.Left(8)); }
			}

			[ColumnName(6)]
			public ZDate PenaltyExpiryDate
			{
				get { return D99BMessageUtilities.ParseDate(PenaltyEffectiveExpiryDateString.Right(8)); }
			}

			[ColumnName(4)]
			public ZDecimal PenaltyPercentage
			{
				get { return GetRate(PercentageTypeCodeQualifierList.PenaltyPercentage); }
			}

			ZString PenaltyEffectiveExpiryDateString
			{
				get { return penaltyEffectiveExpiryDateString ?? (penaltyEffectiveExpiryDateString = GetEffectiveExpiryDateString(PaymentTermsTypeCodeQualifierList.Extended)); }
			}

			string penaltyEffectiveExpiryDateString;

			#endregion

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return Res.GetString("b92712cb-0cc5-4bc6-b381-d886c9016d01", "Interest Rates"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return PropertyNameProvider.GetColumnTitles<InterestRatesAssessedForLateAccountingAndK84Payment>(); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get { return new object[] { InterestPercentage, InterestEffectiveDate, InterestExpiryDate, PenaltyPercentage, PenaltyEffectiveDate, PenaltyExpiryDate }; }
			}

			#endregion

			#region Implementation

			string GetEffectiveExpiryDateString(PaymentTermsTypeCodeQualifierList qualifier)
			{
				return (from SegmentGroup18 group18 in group10.Group18
						from PATSegment pat in group18.PAT
						where pat.PaymentTermsTypeCodeQualifier == qualifier
						select pat.PaymentTerms_X.PaymentTermsDescriptionIdentifier).FirstOrDefault();
			}

			ZDecimal GetRate(PercentageTypeCodeQualifierList qualifier)
			{
				return (from SegmentGroup18 group18 in group10.Group18
						from PCDSegment pcd in group18.PCD
						where pcd.PercentageDetails.PercentageTypeCodeQualifier == qualifier
						select ZDecimal.ParseSafe(pcd.PercentageDetails.Percentage, 0m)).FirstOrDefault();
			}

			readonly SegmentGroup10 group10;

			#endregion
		}

		#endregion

		#region MonthlyAdjustedTotal

		public MonthlyAdjustedTotal MonthlyAdjustedTotals
		{
			get { return GetTotal(g => new MonthlyAdjustedTotal(g), "K70"); }
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class MonthlyAdjustedTotal : NonPersistentBusinessObject, ITableInterpretation
		{
			internal MonthlyAdjustedTotal(SegmentGroup10 group10)
			{
				Argument.NotNull(group10, "group10");
				this.group10 = group10;
			}

			[ColumnName]
			public ZDate DueDate
			{
				get { return D99BMessageUtilities.GetDate(group10.DTM, DateTimePeriodFunctionCodeQualifierList.PaymentDueDate); }
			}

			public K84AmountsWithPenaltyWrapper Amounts
			{
				get { return (from SegmentGroup11 group11 in group10.Group11 select new K84AmountsWithPenaltyWrapper(group11.MOA)).FirstOrDefault(); }
			}

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return Res.GetString("ae5dab04-15a2-4618-b445-a8e38d10d004", "Monthly Adjusted Total"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return PropertyNameProvider.GetColumnTitles<MonthlyAdjustedTotal>().Concat(PropertyNameProvider.GetColumnTitles<K84AmountsWithPenaltyWrapper>()); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get { return new object[] { DueDate }.Concat(Amounts.ToArray()); }
			}

			#endregion

			readonly SegmentGroup10 group10;
		}

		#endregion

		#region Implementation

		BusinessObjectCollectionWrapper<T> GetTotals<T>(Func<SegmentGroup10, T> getTotal, string identifier) where T : NonPersistentBusinessObject
		{
			var groups = from SegmentGroup10 group10 in message.Group10
						 where group10.DMS[0].DocumentMessageIdentification.DocumentMessageNumber == identifier
						 select getTotal(group10);
			return new BusinessObjectCollectionWrapper<T>(groups);
		}

		T GetTotal<T>(Func<SegmentGroup10, T> getTotal, string identifier) where T : NonPersistentBusinessObject
		{
			return (from SegmentGroup10 group10 in message.Group10
					where group10.DMS[0].DocumentMessageIdentification.DocumentMessageNumber == identifier
					select getTotal(group10)).FirstOrDefault();
		}

		internal static CellWithFormatting GetTotalCell(int colspan = 1)
		{
			var caption = Res.GetString("c05b46d6-2256-44c8-beb5-bf13f3ec3c4e", "Total");
			return colspan == 1 ? new CellWithFormatting(caption, true)
				: new CellWithFormatting(caption, TableInterpretation.Attributes.GetColspanAttribute(colspan), true);
		}

		static ZString GetJobLink(BusinessObjectFactory factory, ZString accountSecurityNumber, ZString transactionNumber)
		{
			var declaration = ImportLinkedObjectManager.LoadDeclarationWithTransactionNumber(factory, accountSecurityNumber + transactionNumber, ZString.Empty);
			return declaration != null ? EmailDefBuilder.GetJobLink(declaration, declaration.JE_DeclarationReference) : string.Empty;
		}

		#endregion

		ZGuid ISourceIdentifierProvider.SourceIdentifier => this.k84Message.PK;
	}
}
