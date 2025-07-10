using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageInterpretation;
using Enterprise.Customs.CA.Registry;
using Enterprise.DocumentEngineIntegration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;

#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ARLDailyNoticeDocumentWrapper : NonPersistentBusinessObject, IDocumentWrapper, ISourceIdentifierProvider
	{
		public ARLDailyNoticeDocumentWrapper(Enterprise.Messaging.Business.EDIMessage message)
			: this(message.GetEM_MessageTextReader().Parse<TransactionBatch>(), message.Factory)
		{
			this.message = message;
		}

		public ARLDailyNoticeDocumentWrapper(TransactionBatch transactionBatch, BusinessObjectFactory factory) : base(factory)
		{
			Argument.NotNull(transactionBatch, "transactionBatch");
			if (transactionBatch.BatchType == null)
			{
				throw new ArgumentNullException(nameof(transactionBatch), "Batch type is null in DN message.");
			}
			else if (!transactionBatch.BatchType.Code.HasValue || transactionBatch.BatchType.Code.Value != ARLMessageTypes.Codes.DailyNotice)
			{
				throw new ArgumentException("ARLDailyNoticeDocumentWrapper is for DN message only but was " + transactionBatch.BatchType.Code ?? string.Empty);
			}
			this.transactionBatch = transactionBatch;
		}

		readonly Enterprise.Messaging.Business.EDIMessage message;
		readonly TransactionBatch transactionBatch;

		#region Universal Transaction Batch

		TransactionInfo HeaderTransactionInfo
		{
			get { return fHeaderTransactionInfo ?? (fHeaderTransactionInfo = GetHeaderTransactionInfo()); }
		}
		TransactionInfo fHeaderTransactionInfo;

		TransactionInfo GetHeaderTransactionInfo()
			=> transactionBatch.GetTransactionCollectionByCategory(TransactionBatchConstants.TransactionCategoryCodes.Summary).FirstOrDefault()
				?? throw new InvalidMessageContentException("Cannot find XML node TransactionBatch/TransactionCollection/Transaction[Category='SUM']");

		List<TransactionInfo> ImporterSummaryTransactionCollection
		{
			get
			{
				return fImporterSummaryTransactionCollection ?? (fImporterSummaryTransactionCollection = GetImporterSummaryTransactionCollection());
			}
		}
		List<TransactionInfo> fImporterSummaryTransactionCollection;

		List<TransactionInfo> GetImporterSummaryTransactionCollection()
		{
			var transactions = transactionBatch.GetTransactionCollectionByCategory(TransactionBatchConstants.TransactionCategoryCodes.Summary).ToList();
			if (transactions.Count == 0)
			{
				throw new InvalidMessageContentException("Cannot find XML node TransactionBatch/TransactionCollection/Transaction[Category='SUM']");
			}
			transactions.RemoveAt(0);

			return transactions;
		}

		#endregion

		#region Properties

		public ZInt? NumberOfSupportingDocuments
		{
			get { return HeaderTransactionInfo.NumberOfSupportingDocuments; }
		}

		public ZString NumberOfSupportingDocumentsString
		{
			get { return NumberOfSupportingDocuments.HasValue ? NumberOfSupportingDocuments.Value.ToString() : string.Empty; }
		}

		public ZDate StatementDate
		{
			get { return HeaderTransactionInfo.CreateTime.GetValueOrDefault().Date; }
		}

		public ZDate AccountingDate
		{
			get { return HeaderTransactionInfo.PostDate.GetValueOrDefault().Date; }
		}

		public ZString ImporterBusinessNumber
		{
			get { return HeaderTransactionInfo.OrganizationAddress.GovRegNum.GetValueOrDefault(); }
		}

		public ZString OrganizationLegalName
		{
			get
			{
				OrgHeader singleImporter = null;
				var importers = PreviousDaysAccountings.SelectMany(x => x.Importers).Where(x => x != null).Distinct().Take(2).ToArray();
				if (importers.Length == 1)
				{
					singleImporter = importers[0];
				}
				return HeaderTransactionInfo.OrganizationAddress.CompanyName.GetValueOrDefault() + (singleImporter != null ? string.Format(CultureInfo.CurrentCulture, "\r\n({0})", singleImporter.OH_FullNameTruncated) : string.Empty);
			}
		}

		public ZString AccountSecurityCode
		{
			get { return fAccountSecurityCode ?? (fAccountSecurityCode = GetAccountSecurityCode()); }
			set { fAccountSecurityCode = value; }
		}
		string fAccountSecurityCode;

		public ZString GetAccountSecurityCode(IXmlSessionTracker logger = null)
		{
			fAccountSecurityCode = HeaderTransactionInfo.GetAccountSecurityCode(ImporterBusinessNumber, RMAccountNumber, Factory, logger);
			return fAccountSecurityCode;
		}

		public ZDecimal PaymentsReceived
		{
			get { return HeaderTransactionInfo.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.TotalPaymentReceived); }
		}

		public ZDecimal Refund
		{
			get { return HeaderTransactionInfo.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.Refund); }
		}

		public ZString StatementType
		{
			get
			{
				var result = HeaderTransactionInfo.ComplianceSubType.GetValueOrDefault();
				return new CusStatementHeaderTypes().ContainsCode(result) ? result : new ZString(CusStatementHeaderTypes.Codes.Unknown);
			}
		}

		public ZString StatementTypeDescription
		{
			get { return new CusStatementHeaderTypes().GetDescriptionFromCode(StatementType); }
		}

		public ZString RMAccountNumber
		{
			get { return HeaderTransactionInfo.Number.GetValueOrDefault(); }
		}

		public ZString MessageEN
		{
			get { return HeaderTransactionInfo.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.MessageEN); }
		}

		public ZString MessageFR
		{
			get { return HeaderTransactionInfo.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.MessageFR); }
		}

		#endregion

		#region Total Properties

		public ZDecimal TotalCustomsDuties
		{
			get { return PreviousDaysAccountings.Sum(x => x.CustomsDuties); }
		}

		public ZDecimal TotalSIMA
		{
			get { return PreviousDaysAccountings.Sum(x => x.SIMA); }
		}

		public ZDecimal TotalExciseTax
		{
			get { return PreviousDaysAccountings.Sum(x => x.ExciseTax); }
		}

		public ZDecimal TotalGST
		{
			get { return PreviousDaysAccountings.Sum(x => x.GST); }
		}

		public ZDecimal TotalOthers
		{
			get { return PreviousDaysAccountings.Sum(x => x.Others); }
		}

		public ZDecimal TotalTotal
		{
			get { return PreviousDaysAccountings.Sum(x => x.Total); }
		}

		#endregion

		#region ImporterGrandTotal

		public ITableInterpretation GetImporterGrandTotal()
		{
			return new ImportersGrandTotal(this);
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		internal class ImportersGrandTotal : NonPersistentBusinessObject, ITableInterpretation
		{
			internal ImportersGrandTotal(ARLDailyNoticeDocumentWrapper parent)
			{
				Argument.NotNull(parent, "parent");
				this.parent = parent;
			}

			readonly ARLDailyNoticeDocumentWrapper parent;

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return Res.GetString("251ff556-4033-452f-836a-a838538de790", "Report Grand Total"); }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return PropertyNameProvider.GetColumnTitles<DailyAccounting.TransactionDetails>().Skip(6).Take(6); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get
				{
					yield return InterpretationHelper.FormatAmount(parent.TotalCustomsDuties);
					yield return InterpretationHelper.FormatAmount(parent.TotalSIMA);
					yield return InterpretationHelper.FormatAmount(parent.TotalExciseTax);
					yield return InterpretationHelper.FormatAmount(parent.TotalGST);
					yield return InterpretationHelper.FormatAmount(parent.TotalOthers);
					yield return InterpretationHelper.FormatAmount(parent.TotalTotal);
				}
			}

			#endregion
		}
		#endregion

		#region PreviousDaysAccountings

		public IEnumerable<DailyAccounting> PreviousDaysAccountings
		{
			get
			{
				return previousDaysAccountings ?? (previousDaysAccountings = ImporterSummaryTransactionCollection.Select(x => new DailyAccounting(this, x)).ToArray());
			}
		}
		IEnumerable<DailyAccounting> previousDaysAccountings;

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class DailyAccounting : NonPersistentBusinessObject
		{
			internal DailyAccounting(ARLDailyNoticeDocumentWrapper parent, TransactionInfo importerDetails) : base(parent.Factory)
			{
				Argument.NotNull(parent, nameof(parent));
				Argument.NotNull(importerDetails, nameof(importerDetails));

				this.parent = parent;
				this.importerDetails = importerDetails;
			}

			readonly ARLDailyNoticeDocumentWrapper parent;
			readonly TransactionInfo importerDetails;

			OrgHeader[] FindBestMatchImporters()
			{
				var result = Transactions.Select(x => x.Declaration?.EffectiveImporter).Where(x => x != null).Distinct().ToArray();
				return result.Length == 0 ? TransactionBatchExtension.GetOrgsFromBN(ImporterBusinessNumber, ZString.Empty, parent.Factory) : result;
			}

			#region Properties

			[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
			public OrgHeader[] Importers => importers ?? (importers = FindBestMatchImporters());
			OrgHeader[] importers;

			bool HasMultipleImporters => Importers.Length > 1;

			public ZString ImporterBusinessNumber
			{
				get { return importerDetails.OrganizationAddress.GovRegNum.GetValueOrDefault(); }
			}

			public ZString ImporterCode
			{
				get { return HasMultipleImporters ? MultipleData : (string)(Importers.FirstOrDefault()?.OH_Code ?? ZString.Empty); }
			}

			string MultipleData => Res.GetString("{D379DC05-9DF8-4A50-A2C5-68E136E330F0}", "MULTIPLE");

			public ZString ImporterLegalName
			{
				get
				{
					var importer = HasMultipleImporters ? null : Importers.FirstOrDefault();
					return ImporterLegalNameFromMessage + (importer != null ? string.Format(CultureInfo.CurrentCulture, "\r\n({0})", importer.OH_FullNameTruncated) : string.Empty);
				}
			}

			internal ZString ImporterLegalNameFromMessage => importerDetails.OrganizationAddress.CompanyName.GetValueOrDefault();

			public ZDecimal PaymentsReceived
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.TotalPaymentReceived); }
			}

			public ZDecimal Refund
			{
				get { return importerDetails.GetLocalAmountByDescription(TransactionBatchConstants.PostingJournalDescriptions.Refund); }
			}

			public ZDecimal CustomsDuties
			{
				get { return importerDetails.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.Duty); }
			}

			public ZDecimal SIMA
			{
				get { return importerDetails.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.SIMA); }
			}

			public ZDecimal ExciseTax
			{
				get { return importerDetails.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.ExciseTax); }
			}

			public ZDecimal GST
			{
				get { return importerDetails.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.GST); }
			}

			public ZDecimal Others
			{
				get { return importerDetails.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.Others); }
			}

			public ZDecimal Total
			{
				get { return importerDetails.LocalTotal.GetValueOrDefault(); }
			}

			OrgImpAddInfo OrgImpAddInfo
			{
				get { return OrgImpAddInfo.Get(Importers.FirstOrDefault()); }
			}

			public ZBool IsImporterDirectPayment
			{
				get
				{
					return OrgImpAddInfo != null && OrgImpAddInfo.ZO_IsImporterDirectPayment;
				}
			}

			public ZBool IsGSTDirectPayment
			{
				get
				{
					return OrgImpAddInfo != null && OrgImpAddInfo.ZO_IsGSTDirectPayment;
				}
			}

			#endregion

			#region ImporterTotal

			public ITableValues GetImporterTotal()
			{
				return new ImporterTotal(this);
			}

			internal class ImporterTotal : ITableValues
			{
				internal ImporterTotal(DailyAccounting dailyAccounting)
				{
					this.dailyAccounting = dailyAccounting;
				}

				public IEnumerable<object> Values
				{
					get
					{
						var caption = Res.GetString("3184ecbd-d9e9-4fc0-b020-e232edc56f4f", "Importer Total");
						yield return new CellWithFormatting(caption, TableInterpretation.Attributes.GetColspanAttribute(6), true);
						yield return InterpretationHelper.FormatAmount(dailyAccounting.CustomsDuties);
						yield return InterpretationHelper.FormatAmount(dailyAccounting.SIMA);
						yield return InterpretationHelper.FormatAmount(dailyAccounting.ExciseTax);
						yield return InterpretationHelper.FormatAmount(dailyAccounting.GST);
						yield return InterpretationHelper.FormatAmount(dailyAccounting.Others);
						yield return InterpretationHelper.FormatAmount(dailyAccounting.Total);
					}
				}

				readonly DailyAccounting dailyAccounting;
			}

			#endregion

			#region Transactions

			public IEnumerable<TransactionDetails> Transactions
			{
				get { return transactionDetails ?? (transactionDetails = parent.transactionBatch.GetTransactionCollectionByCategory(TransactionBatchConstants.TransactionCategoryCodes.Normal, ImporterBusinessNumber).Select(x => new TransactionDetails(parent, x))); }
			}
			IEnumerable<TransactionDetails> transactionDetails;

			public IEnumerable<OtherTransactionDetails> OtherTransactions
			{
				get { return otherTransactionDetails ?? (otherTransactionDetails = parent.transactionBatch.GetTransactionCollectionByCategory(TransactionBatchConstants.TransactionCategoryCodes.Other, ImporterBusinessNumber).Select(x => new OtherTransactionDetails(parent, x))); }
			}
			IEnumerable<OtherTransactionDetails> otherTransactionDetails;

			public IEnumerable<OtherTransactionDetails> UnderReviewTransactions
			{
				get { return underReviewTransactionDetails ?? (underReviewTransactionDetails = parent.transactionBatch.GetTransactionCollectionByCategory(TransactionBatchConstants.TransactionCategoryCodes.UnderReview, ImporterBusinessNumber).Select(x => new OtherTransactionDetails(parent, x))); }
			}
			IEnumerable<OtherTransactionDetails> underReviewTransactionDetails;

			#region BaseTransactionDetails

			[TestExcludeBusinessObjectsAllHaveTestCases]
			public abstract class BaseTransactionDetails : TransactionDetailsWithDocumentNumber
			{
				internal BaseTransactionDetails(ARLDailyNoticeDocumentWrapper parent, TransactionInfo transaction)
					: base(parent.Factory)
				{
					Argument.NotNull(parent, "parent");
					Argument.NotNull(transaction, "transaction");
					this.parent = parent;
					this.transaction = transaction;
				}
				protected readonly TransactionInfo transaction;
				protected readonly ARLDailyNoticeDocumentWrapper parent;

				#region Properties

				public virtual ZString B3Field6Identifier
				{
					get
					{
						if (transaction.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.IsGSTDirectPayment) == "Y")
						{
							return IsGSTDirectPaymentIdentifier;
						}
						if (transaction.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.IsImporterDirectPayment) == "Y")
						{
							return IsImporterDirectPaymentIdentifier;
						}
						return ZString.Empty;
					}
				}

				public const string IsGSTDirectPaymentIdentifier = "G";
				internal const string IsImporterDirectPaymentIdentifier = "I";

				public ZString TransactionType
				{
					get { return transaction.Category.GetValueOrDefault(); }
				}

				public virtual ZDate DocumentDate
				{
					get { return transaction.TransactionDate.GetValueOrDefault().Date; }
				}

				public virtual ZString DocumentType
				{
					get { return transaction.ComplianceSubType.GetValueOrDefault(); }
				}

				public virtual ZString DocumentTypeDescription
				{
					get
					{
						ZString result = new ARLDocumentTypeList().GetDescriptionFromCode(DocumentType);
						return result.IsEmpty ? DocumentType : result;
					}
				}

				public override ZString DocumentNumber
				{
					get { return transaction.TransactionReference.GetValueOrDefault(); }
				}

				public virtual ZDecimal Total
				{
					get { return transaction.LocalTotal.GetValueOrDefault(); }
				}

				public override ZString B3RelatedDocumentNumber
				{
					get { return transaction.OriginalReference == null ? ZString.Empty : transaction.OriginalReference.OriginalTransactionNumber.GetValueOrDefault(); }
				}

				public bool IsImporterSecurityClientTotal
				{
					get { return DocumentNumber == "35"; }
				}

				#endregion

				[SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
				public abstract IEnumerable<(ZString ChargeType, ZDecimal Amount)> GetRequiredChargeAmounts();
			}

			#endregion

			#region TransactionDetails

			[TestExcludeBusinessObjectsAllHaveTestCases]
			public class TransactionDetails : BaseTransactionDetails, ITableInterpretation
			{
				internal TransactionDetails(ARLDailyNoticeDocumentWrapper parent, TransactionInfo transaction)
					: base(parent, transaction)
				{
				}

				#region Properties

				[ColumnName(0, "Direct I/G")]
				public override ZString B3Field6Identifier
				{
					get { return base.B3Field6Identifier; }
				}

				[ColumnName(1, "Doc Type")]
				public override ZString DocumentTypeDescription
				{
					get { return base.DocumentTypeDescription; }
				}

				[ColumnName(2)]
				public ZDate ReleaseDate
				{
					get
					{
						var dateStr = transaction.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.ReleaseDate);
						ZDateTime date;
						if (!dateStr.IsEmpty && ZDateTime.TryParseISO8601Date(dateStr, out date))
						{
							return date.Date;
						}

						return ZDate.Empty;
					}
				}

				[ColumnName(3)]
				public ZDate AccountingDate
				{
					get
					{
						var dateStr = transaction.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.AccountingDate);
						ZDateTime date;
						if (!dateStr.IsEmpty && ZDateTime.TryParseISO8601Date(dateStr, out date))
						{
							return date.Date;
						}

						return ZDate.Empty;
					}
				}

				[ColumnName(4)]
				public ZString Port
				{
					get { return transaction.GetAddInfo(TransactionBatchConstants.TransactionAddInfoKeys.ReleaseOffice); }
				}

				[ColumnName(5, "Document #")]
				public override ZString DocumentNumber
				{
					get { return base.DocumentNumber; }
				}

				[ColumnName(6, "Duties")]
				public ZDecimal CustomsDuties
				{
					get { return transaction.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.Duty); }
				}

				[ColumnName(7)]
				public ZDecimal SIMA
				{
					get { return transaction.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.SIMA); }
				}

				[ColumnName(8)]
				public ZDecimal ExciseTax
				{
					get { return transaction.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.ExciseTax); }
				}

				[ColumnName(9, "GST/PST/HST")]
				public ZDecimal GST
				{
					get { return transaction.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.GST); }
				}

				[ColumnName(10)]
				public ZDecimal Others
				{
					get { return transaction.GetChargeTotalAmountByChargeCode(TransactionBatchConstants.PostingJournalChargeCodes.Others); }
				}

				[ColumnName(11)]
				public override ZDecimal Total
				{
					get { return base.Total; }
				}

				[ColumnName(12, "Transaction Number")]
				public override ZString B3RelatedDocumentNumber
				{
					get { return base.B3RelatedDocumentNumber; }
				}

				public override IEnumerable<(ZString ChargeType, ZDecimal Amount)> GetRequiredChargeAmounts()
				{
					switch (DocumentType)
					{
						case ARLDocumentTypeList.Codes.LA:
							{
								yield return (EntryChargeTypeList.Codes.K84LateFilingPenalty, Others);
								break;
							}

						default:
							{
								var chargeTypeOfGST = B3Field6Identifier == IsGSTDirectPaymentIdentifier
									? EntryChargeTypeList.Codes.TotalGSTDirectAmount
									: EntryChargeTypeList.Codes.TotalGSTAmount;

								yield return (EntryChargeTypeList.Codes.TotalDutyAmount, CustomsDuties);
								yield return (EntryChargeTypeList.Codes.TotalSIMAAmount, SIMA);
								yield return (EntryChargeTypeList.Codes.TotalExciseTaxAmount, ExciseTax);
								yield return (EntryChargeTypeList.Codes.K84LateFilingPenalty, ZDecimal.Zero);
								yield return (EntryChargeTypeList.Codes.Others, Others);
								yield return (chargeTypeOfGST, GST);
								break;
							}
					}
				}

				[ColumnName(13)]
				public override ZString JobNumber
				{
					get { return base.JobNumber; }
				}

				public ZString PaidBy
				{
					get
					{
						switch (B3Field6Identifier)
						{
							case IsGSTDirectPaymentIdentifier:
								return PaymentPartyCodeDescriptionList.Codes.GST;
							case IsImporterDirectPaymentIdentifier:
								return PaymentPartyCodeDescriptionList.Codes.Importer;
							default:
								return PaymentPartyCodeDescriptionList.Codes.Broker;
						}
					}
				}

				[ColumnName(14, "Billed Amount")]
				public ZDecimal AmountBilled
				{
					get { return Declaration != null ? Declaration.TotalBilledAmount.Round(2) : ZDecimal.Zero; }
				}

				[ColumnName(15)]
				public ZString Warning
				{
					get
					{
						var result = ZString.Empty;
						if (Declaration == null)
						{
							result = "NODEC";
						}
						else
						{
							var amountBilled = AmountBilled;
							if (PaidBy == Customs.Business.PaymentPartyCodeDescriptionList.Codes.Importer)
							{
								result = amountBilled.IsEmpty ? "IMP" : "CHECK";
							}
							else
							{
								var expectedBillingAmount = Declaration.IsGSTDirectPayment && !Declaration.IsGSTDirectAutoRated ? new ZDecimal(Total - GST) : Total;
								if (amountBilled < expectedBillingAmount)
								{
									result = "BILL";
								}
								else if (amountBilled > expectedBillingAmount)
								{
									result = "CHECK";
								}
							}
						}
						return result;
					}
				}

				#endregion

				#region Implementation of ITableInterpretation

				string ITableInterpretation.Caption
				{
					get { return Res.GetString("4c566fac-37dd-4e9c-9527-cb6354429f8d", "Transactions {0}", InterpretationHelper.FormatDate(parent.AccountingDate)); }
				}

				IEnumerable<string> ITableInterpretation.Titles
				{
					get { return PropertyNameProvider.GetColumnTitles<TransactionDetails>(); }
				}

				IEnumerable<object> ITableValues.Values
				{
					get
					{
						return new object[]
						{
							B3Field6Identifier,
							DocumentTypeDescription,
							InterpretationHelper.FormatDate(ReleaseDate),
							InterpretationHelper.FormatDate(AccountingDate),
							Port,
							DocumentNumber,
							InterpretationHelper.FormatAmount(CustomsDuties, true),
							InterpretationHelper.FormatAmount(SIMA, true),
							InterpretationHelper.FormatAmount(ExciseTax, true),
							InterpretationHelper.FormatAmount(GST, true),
							InterpretationHelper.FormatAmount(Others, true),
							InterpretationHelper.FormatAmount(Total, true),
							B3RelatedDocumentNumber,
							JobNumber,
							AmountBilled,
							Warning
						};
					}
				}

				#endregion
			}

			#endregion

			#region OtherTransactionDetails

			[TestExcludeBusinessObjectsAllHaveTestCases]
			public class OtherTransactionDetails : BaseTransactionDetails, ITableInterpretation
			{
				internal OtherTransactionDetails(ARLDailyNoticeDocumentWrapper parent, TransactionInfo transaction)
					: base(parent, transaction)
				{
				}

				#region Properties

				[ColumnName(1)]
				public override ZDate DocumentDate
				{
					get { return base.DocumentDate; }
				}

				[ColumnName(2, "Document Type")]
				public override ZString DocumentTypeDescription
				{
					get { return base.DocumentTypeDescription; }
				}

				[ColumnName(3, "Document #")]
				public override ZString DocumentNumber
				{
					get { return base.DocumentNumber; }
				}

				[ColumnName(4)]
				public ZDate PaymentDueDate
				{
					get { return transaction.DueDate.GetValueOrDefault().Date; }
				}

				[ColumnName(5)]
				public override ZDecimal Total
				{
					get { return base.Total; }
				}

				[ColumnName(6, "Related B3 Document")]
				public override ZString B3RelatedDocumentNumber
				{
					get { return base.B3RelatedDocumentNumber; }
				}

				public override IEnumerable<(ZString ChargeType, ZDecimal Amount)> GetRequiredChargeAmounts()
				{
					yield return (EntryChargeTypeList.Codes.Others, Total);
				}

				[ColumnName(7, "Job Number")]
				public override ZString JobNumber
				{
					get { return base.JobNumber; }
				}

				#endregion

				#region Implementation of ITableInterpretation

				string ITableInterpretation.Caption
				{
					get { return transaction.Category.GetValueOrDefault() == TransactionBatchConstants.TransactionCategoryCodes.UnderReview ? Res.GetString("60dd7ef3-abae-492d-885c-35425ec1afae", "Under Review") : Res.GetString("bf64d5cc-8b4a-442f-bc65-6fbe279c7d6b", "Other Transactions"); }
				}

				IEnumerable<string> ITableInterpretation.Titles
				{
					get { return PropertyNameProvider.GetColumnTitles<OtherTransactionDetails>(); }
				}

				IEnumerable<object> ITableValues.Values
				{
					get
					{
						return new object[]
						{
							InterpretationHelper.FormatDate(DocumentDate),
							DocumentTypeDescription,
							DocumentNumber,
							InterpretationHelper.FormatDate(PaymentDueDate),
							InterpretationHelper.FormatAmount(Total, true),
							B3RelatedDocumentNumber,
							JobNumber
						};
					}
				}

				#endregion
			}

			#endregion

			#region OtherTransactionTotal

			public ITableValues GetOtherTransactionTotal()
			{
				return new OtherTransactionTotal(OtherTransactions);
			}

			public ITableValues GetUnderReviewTransactionTotal()
			{
				return new OtherTransactionTotal(UnderReviewTransactions);
			}

			internal class OtherTransactionTotal : ITableValues
			{
				internal OtherTransactionTotal(IEnumerable<OtherTransactionDetails> transactions)
				{
					this.transactions = transactions;
				}

				public IEnumerable<object> Values
				{
					get
					{
						var caption = Res.GetString("c7e6623d-3582-4f27-8548-d6b62f2383ad", "TOTAL");
						yield return new CellWithFormatting(caption, TableInterpretation.Attributes.GetColspanAttribute(4), true);
						yield return InterpretationHelper.FormatAmount(transactions.Sum(p => p.Total));
					}
				}

				readonly IEnumerable<OtherTransactionDetails> transactions;
			}

			#endregion

			#endregion
		}

		#endregion

		#region For Document

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class TransactionsDetailsWrapper : NonPersistentBusinessObject
		{
			public TransactionsDetailsWrapper(DailyAccounting.BaseTransactionDetails transactionDetails, DailyAccounting dailyAccounting, ZString transactionDetailsType) : base(dailyAccounting.Factory)
			{
				Argument.NotNull(dailyAccounting, "dailyAccounting");
				this.transactionDetails = transactionDetails;
				this.dailyAccounting = dailyAccounting;
				this.transactionDetailsType = transactionDetailsType;

				var details = transactionDetails as DailyAccounting.TransactionDetails;
				if (details != null)
				{
					this.B3Field6Identifier = details.B3Field6Identifier;
					this.ReleaseDate = details.ReleaseDate;
					this.AccountingDate = details.AccountingDate;
					this.Port = details.Port;
					this.CustomsDuties = details.CustomsDuties;
					this.SIMA = details.SIMA;
					this.ExciseTax = details.ExciseTax;
					this.GST = details.GST;
					this.Others = details.Others;
					this.AmountBilled = details.AmountBilled;
					this.Warning = details.Warning;
				}

				var otherDetails = transactionDetails as DailyAccounting.OtherTransactionDetails;
				if (otherDetails != null)
				{
					this.PaymentDueDate = otherDetails.PaymentDueDate;
				}
			}

			readonly DailyAccounting.BaseTransactionDetails transactionDetails;
			readonly DailyAccounting dailyAccounting;
			readonly ZString transactionDetailsType;

			#region Header Properties

			public ZString ImporterCode
			{
				get { return dailyAccounting.ImporterCode; }
			}

			public ZString ImporterLegalName
			{
				get { return dailyAccounting.ImporterLegalName; }
			}

			public ZString ImporterBusinessNumber
			{
				get { return dailyAccounting.ImporterBusinessNumber; }
			}

			public ZString IsImporterDirectPayment
			{
				get { return dailyAccounting.IsImporterDirectPayment ? "Yes" : "No"; }
			}

			public ZString IsGSTDirectPayment
			{
				get { return dailyAccounting.IsGSTDirectPayment ? "Yes" : "No"; }
			}

			public ZDecimal PaymentsReceived
			{
				get { return dailyAccounting.PaymentsReceived; }
			}

			public ZDecimal Refund
			{
				get { return dailyAccounting.Refund; }
			}

			public ZString TransactionDetailsType
			{
				get { return transactionDetailsType; }
			}

			public ZBool IsFirstItemOfCurrentTransactions { get; set; }
			public ZBool IsLastItemOfCurrentTransactions { get; set; }
			public ZBool IsFirstItemOfCurrentGroup { get; set; }

			#endregion

			#region BaseTransactionDetails

			public ZDate DocumentDate
			{
				get { return transactionDetails?.DocumentDate ?? ZDate.Empty; }
			}

			public ZString DocumentTypeDescription
			{
				get { return transactionDetails?.DocumentTypeDescription ?? ZString.Empty; }
			}

			public ZString DocumentNumber
			{
				get { return transactionDetails?.DocumentNumber ?? ZString.Empty; }
			}

			public ZDecimal Total
			{
				get { return transactionDetails?.Total ?? ZDecimal.Zero; }
			}

			public ZString B3RelatedDocumentNumber
			{
				get { return transactionDetails?.B3RelatedDocumentNumber ?? ZString.Empty; }
			}

			public ZString JobNumber
			{
				get
				{
					return Regex.Replace(transactionDetails?.JobNumber ?? ZString.Empty, @"\<.+?\>", ZString.Empty);
				}
			}

			#endregion

			#region TransactionDetails

			public ZString B3Field6Identifier { get; set; }
			public ZDate ReleaseDate { get; set; }
			public ZDate AccountingDate { get; set; }
			public ZString Port { get; set; }
			public ZDecimal CustomsDuties { get; set; }
			public ZDecimal SIMA { get; set; }
			public ZDecimal ExciseTax { get; set; }
			public ZDecimal GST { get; set; }
			public ZDecimal Others { get; set; }
			public ZDecimal AmountBilled { get; set; }
			public ZString Warning { get; set; }

			#endregion

			#region OtherTransactionDetails

			public ZDate PaymentDueDate { get; set; }

			#endregion

			#region Total Properties

			public ZDecimal TotalCustomsDuties
			{
				get { return dailyAccounting.CustomsDuties; }
			}

			public ZDecimal TotalSIMA
			{
				get { return dailyAccounting.SIMA; }
			}

			public ZDecimal TotalExciseTax
			{
				get { return dailyAccounting.ExciseTax; }
			}

			public ZDecimal TotalGST
			{
				get { return dailyAccounting.GST; }
			}

			public ZDecimal TotalOthers
			{
				get { return dailyAccounting.Others; }
			}

			public ZDecimal TotalTransactionsTotal
			{
				get { return dailyAccounting.Total; }
			}

			public ZDecimal TotalOtherTransactionsTotal { get; set; }

			public ZDecimal TotalPayableByBroker
			{
				get
				{
					CalculateTotalPayableBy();
					return totalPayableByBroker.Value;
				}
			}
			ZDecimal? totalPayableByBroker;

			public ZDecimal TotalPayableByImporter
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

					foreach (var transaction in dailyAccounting.Transactions)
					{
						if (!transaction.IsImporterSecurityClientTotal)
						{
							var chargeAmounts = transaction.GetRequiredChargeAmounts();
							var paymentParties = EntryChargeTypeList.GetPaymentParty(transaction.B3Field6Identifier);

							foreach (var chargeAmount in chargeAmounts)
							{
								paymentParties.TryGetValue(chargeAmount.ChargeType, out var paymentParty);

								if (paymentParty == PaymentPartyCodeDescriptionList.Codes.Importer)
								{
									totalPayableByImporter += chargeAmount.Amount;
								}
								else
								{
									totalPayableByBroker += chargeAmount.Amount;
								}
							}
						}
					}
				}
			}

			#endregion
		}

		public BusinessObjectCollectionWrapper<TransactionsDetailsWrapper> AllTransactionsForDocument
		{
			get
			{
				if (allTransactionsForDocument == null)
				{
					var list = new List<TransactionsDetailsWrapper>();
					var countOfTrans = 0;
					foreach (DailyAccounting dailyAccounting in PreviousDaysAccountings)
					{
						var countOfList = list.Count;
						var isSetFirstOfCurrentGorup = false;

						if (dailyAccounting.Transactions.Any())
						{
							list.AddRange(dailyAccounting.Transactions.Select(x => new TransactionsDetailsWrapper(x, dailyAccounting, Res.GetString("cf09e9db-0e11-4fd8-9516-fd815efdd85a", "Transactions"))));
							list[countOfTrans].IsFirstItemOfCurrentTransactions = true;
							list[list.Count - 1].IsLastItemOfCurrentTransactions = true;
							list[countOfTrans].IsFirstItemOfCurrentGroup = true;
							isSetFirstOfCurrentGorup = true;

							countOfTrans = list.Count;
						}
						if (dailyAccounting.OtherTransactions.Any())
						{
							list.AddRange(dailyAccounting.OtherTransactions.Select(x => new TransactionsDetailsWrapper(x, dailyAccounting, Res.GetString("5df48445-029d-460c-b7a1-7591bda21c4b", "Other Transactions"))));
							list[countOfTrans].IsFirstItemOfCurrentTransactions = true;
							list[list.Count - 1].IsLastItemOfCurrentTransactions = true;
							list[list.Count - 1].TotalOtherTransactionsTotal = dailyAccounting.OtherTransactions.Sum(x => x.Total);

							if (!isSetFirstOfCurrentGorup)
							{
								list[countOfTrans].IsFirstItemOfCurrentGroup = true;
								isSetFirstOfCurrentGorup = true;
							}

							countOfTrans = list.Count;
						}
						if (dailyAccounting.UnderReviewTransactions.Any())
						{
							list.AddRange(dailyAccounting.UnderReviewTransactions.Select(x => new TransactionsDetailsWrapper(x, dailyAccounting, Res.GetString("b95c09bc-31ff-47db-a6c9-dd304736d4c8", "Under Review"))));
							list[countOfTrans].IsFirstItemOfCurrentTransactions = true;
							list[list.Count - 1].IsLastItemOfCurrentTransactions = true;
							list[list.Count - 1].TotalOtherTransactionsTotal = dailyAccounting.UnderReviewTransactions.Sum(x => x.Total);

							if (!isSetFirstOfCurrentGorup)
							{
								list[countOfTrans].IsFirstItemOfCurrentGroup = true;
							}

							countOfTrans = list.Count;
						}
						if (countOfList == list.Count)
						{
							list.Add(new TransactionsDetailsWrapper(null, dailyAccounting, ZString.Empty));
							countOfTrans = list.Count;
						}
					}
					allTransactionsForDocument = new BusinessObjectCollectionWrapper<TransactionsDetailsWrapper>(list);
				}
				return allTransactionsForDocument;
			}
		}

		BusinessObjectCollectionWrapper<TransactionsDetailsWrapper> allTransactionsForDocument;
		ZGuid ISourceIdentifierProvider.SourceIdentifier => message.PK;
	}

	#endregion
}
