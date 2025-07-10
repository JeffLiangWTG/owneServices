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
	class K84DailyReportDocumentWrapper : NonPersistentBusinessObject, IDocumentWrapper, ISourceIdentifierProvider
	{
		internal K84DailyReportDocumentWrapper(K84Message message)
			: base(message.Factory)
		{
			Argument.NotNull(message, "message");
			if (message.EM_MessageSubType != K84ReportTypes.Codes.Daily)
			{
				throw new ArgumentException("K84DailyReportDocumentWrapper is for DAI message only but was " + message.EM_MessageSubType);
			}
			this.k84Message = message;
			this.message = (CUSDECMessage)message.GetAutoEdifactMessageUsingNamedFactory(new CaEdifactMessageFactory(), new CACharSet());
			Argument.NotNull(this.message, "message", "Supported EDIFACT message type is D99B CUSDEC");
		}

		readonly K84Message k84Message;
		readonly CUSDECMessage message;

		public ZDate CurrentDate
		{
			get { return D99BMessageUtilities.GetDate(message.DTM, DateTimePeriodFunctionCodeQualifierList.DocumentMessageDateTime); }
		}

		public ZString AccountSecurityNumber
		{
			get
			{
				return message.Group1.Count == 0 ? ZString.Empty
								 : D99BMessageUtilities.GetReference(message.Group1[0].RFF, ReferenceFunctionCodeQualifierList.DeclarantsCustomsIdentityNumber);
			}
		}

		#region PreviousDaysAccountings

		public BusinessObjectCollectionWrapper<DailyAccounting> PreviousDaysAccountings
		{
			get
			{
				if (previousDaysAccountings == null)
				{
					var accountSecurityNumber = AccountSecurityNumber;
					var groups = from SegmentGroup10 group10 in message.Group10
								 where group10.DMS[0].DocumentMessageIdentification.DocumentMessageNumber == "K10"
								 select new DailyAccounting(Factory, group10, accountSecurityNumber);
					previousDaysAccountings = new BusinessObjectCollectionWrapper<DailyAccounting>(groups);
				}
				return previousDaysAccountings;
			}
		}
		BusinessObjectCollectionWrapper<DailyAccounting> previousDaysAccountings;

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class DailyAccounting : NonPersistentBusinessObject
		{
			internal DailyAccounting(BusinessObjectFactory factory, SegmentGroup10 group10, ZString accountSecurityNumber) : base(factory)
			{
				Argument.NotNull(group10, "group10");
				this.group10 = group10;
				this.accountSecurityNumber = accountSecurityNumber;
			}

			public ZDate StatementDate
			{
				get { return D99BMessageUtilities.GetDate(group10.DTM, DateTimePeriodFunctionCodeQualifierList.CurrentReportDate); }
			}

			public ZDate AccountingDate
			{
				get { return D99BMessageUtilities.GetDate(group10.DTM, DateTimePeriodFunctionCodeQualifierList.AccountingTransactionDate); }
			}

			public ZString AccountingOffice
			{
				get
				{
					return (from SegmentGroup14 group14 in group10.Group14
							from NADSegment nad in group14.NAD
							where nad.PartyFunctionCodeQualifier == PartyFunctionCodeQualifierList.FilingOffice
							select nad.PartyIdentificationDetails.PartyIdentifier).FirstOrDefault();
				}
			}

			#region Transactions

			public BusinessObjectCollectionWrapper<TransactionDetails> Transactions
			{
				get
				{
					var transactions = from SegmentGroup21 group21 in group10.Group21 select new TransactionDetails(Factory, group21, accountSecurityNumber);
					var result = new BusinessObjectCollectionWrapper<TransactionDetails>(transactions);
					var index = 0;
					foreach (TransactionDetails transaction in result)
					{
						if (transaction.IsImporterSecurityClientTotal && index > 0)
						{
							transaction.ImporterCode = result[index - 1].ImporterCode;
						}
						index++;
					}
					return result;
				}
			}

			[TestExcludeBusinessObjectsAllHaveTestCases]
			public class TransactionDetails : NonPersistentBusinessObject, ITableInterpretation
			{
				internal TransactionDetails(BusinessObjectFactory factory, SegmentGroup21 group21, ZString accountSecurityNumber) : base(factory)
				{
					this.group21 = group21;
					this.accountSecurityNumber = accountSecurityNumber;
				}

				[ColumnName(1)]
				public ZString JobNumber
				{
					get { return Declaration != null ? EmailDefBuilder.GetJobLink(Declaration, Declaration.JE_DeclarationReference) : string.Empty; }
				}

				[ColumnName(3)]
				public ZString ReleaseOffice
				{
					get { return Declaration != null ? Declaration.ReleaseOffice : ZString.Empty; }
				}

				public ZString AccountSecurityNumber
				{
					get
					{
						return accountSecurityNumber;
					}
				}

				[ColumnName(2)]
				public ZString TransactionNumber
				{
					get
					{
						return transactionNumber ?? (transactionNumber =
							(from LINSegment lin in group21.LIN select lin.ItemNumberIdentification.ItemNumber).FirstOrDefault());
					}
				}

				[ColumnName(4)]
				public ZString PaidBy
				{
					get
					{
						if (Declaration == null)
						{
							return "UNK";
						}
						else
						{
							return IsImporterSecuritySetOnLastAcceptedB3Message ? Customs.Business.PaymentPartyCodeDescriptionList.Codes.Importer : Customs.Business.PaymentPartyCodeDescriptionList.Codes.Broker;
						}
					}
				}

				public ZDecimal AmountBilled
				{
					get { return Declaration != null ? Declaration.TotalBilledAmount.Round(2) : ZDecimal.Zero; }
				}

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
								var expectedBillingAmount = Declaration.IsGSTDirectPayment && !Declaration.IsGSTDirectAutoRated ? new ZDecimal(Amounts.TotalDutiesAndTaxes - Amounts.GST) : Amounts.TotalDutiesAndTaxes;
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

				string transactionNumber;

				public bool IsImporterSecuritySetOnOrganization
				{
					get { return Declaration != null && Declaration.IsImporterOrganizationDirect; }
				}

				public bool IsGSTDirectSetOnOrganization
				{
					get { return Declaration != null && Declaration.IsGSTDirectPayment; }
				}

				public bool IsImporterSecuritySetOnLastAcceptedB3Message
				{
					get
					{
						bool result = false;
						if (Declaration != null)
						{
							result = Declaration.IsImporterDirectPayment;
							var entryHeader = Declaration.B3EntryHeader;
							if (entryHeader != null)
							{
								var lastAcceptedB3Message = B3Message.GetLastSentAcceptedB3Message(entryHeader);
								if (lastAcceptedB3Message != null)
								{
									result = ((IB3Header)new B3AsLodgedDocumentWrapper(lastAcceptedB3Message)).PaymentCode == "I";
								}
							}
						}
						return result;
					}
				}

				public bool IsImporterSecurityClientTotal
				{
					get { return TransactionNumber == "35"; }
				}

				public ZString ImporterCode
				{
					get
					{
						if (importerCode == null && !IsImporterSecurityClientTotal)
						{
							if (Declaration != null)
							{
								if (Declaration.Importer != null)
								{
									importerCode = Declaration.Importer.OH_Code;
								}
								else if (Declaration.IsLVS)
								{
									importerCode = Res.GetString("be5e40d4-d804-4e70-8ef1-db170402ebfd", "VARIOUS");
								}
							}
							else
							{
								importerCode = ZString.Empty;
							}
						}
						return importerCode;
					}
					set { importerCode = value; }
				}
				string importerCode;

				public K84AmountsWithPenaltyWrapper Amounts
				{
					get { return new K84AmountsWithPenaltyWrapper(IsImporterSecurityClientTotal ? new MOASegmentMessageSection(1) : group21.MOA); }
				}

				public K84AmountsWithPenaltyWrapper K35Amounts
				{
					get { return new K84AmountsWithPenaltyWrapper(IsImporterSecurityClientTotal ? group21.MOA : new MOASegmentMessageSection(1)); }
				}

				public JobDeclaration Declaration
				{
					get
					{
						if (declaration == null && !IsImporterSecurityClientTotal)
						{
							declaration = ImportLinkedObjectManager.LoadDeclarationWithTransactionNumber(Factory, accountSecurityNumber + TransactionNumber, ZString.Empty);
						}
						return declaration;
					}
				}
				JobDeclaration declaration;

				#region Implementation of ITableInterpretation

				string ITableInterpretation.Caption
				{
					get { return string.Empty; }
				}

				IEnumerable<string> ITableInterpretation.Titles
				{
					get { return PropertyNameProvider.GetColumnTitles<TransactionDetails>().Concat(PropertyNameProvider.GetColumnTitles<K84AmountsWithPenaltyWrapper>()).Concat(new string[] { Res.GetString("9601cd47-8960-4dbe-affb-c9d8dbb35f7f", "Billed Amount"), Res.GetString("fcf2532c-33b8-49d8-b5c3-b99ffc7a12d6", "Warning") }); }
				}

				IEnumerable<object> ITableValues.Values
				{
					get
					{
						if (IsImporterSecurityClientTotal)
						{
							var caption1 = Res.GetString("373be064-953d-4a09-812b-13e66704295c", "Importer Security Total:");
							var colspanAttribute = TableInterpretation.Attributes.GetColspanAttribute(4);
							return new[] { new CellWithFormatting(caption1, colspanAttribute, true) }.Concat(K35Amounts.ToArray());
						}
						var colorYellow = TableInterpretation.Attributes.GetColorAttribute("yellow");
						return new object[] { JobNumber, TransactionNumber, ReleaseOffice, PaidBy }.Concat(Amounts.ToArray()).Concat(new object[] { AmountBilled, Warning.IsEmpty ? Warning : new CellWithFormatting(Warning, colorYellow) });
					}
				}

				#endregion

				#region ImporterTotal

				internal class ImporterTotal : ITableValues
				{
					internal ImporterTotal(IEnumerable<TransactionDetails> transactionsByImporter)
					{
						this.transactionsByImporter = transactionsByImporter;
					}

					public IEnumerable<object> Values
					{
						get
						{
							var caption = Res.GetString("72f11e4c-8ed6-4c10-af03-d68826326a12", "Importer Total:");
							yield return new CellWithFormatting(caption, TableInterpretation.Attributes.GetColspanAttribute(4), true);
							yield return transactionsByImporter.Sum(t => t.Amounts.CustomsDuty);
							yield return transactionsByImporter.Sum(t => t.Amounts.SIMAAssessment);
							yield return transactionsByImporter.Sum(t => t.Amounts.ExciseTax);
							yield return transactionsByImporter.Sum(t => t.Amounts.GST);
							yield return transactionsByImporter.Sum(t => t.Amounts.TotalDutiesAndTaxes);
							yield return transactionsByImporter.Sum(t => t.Amounts.LateFilingPenalty);
							yield return transactionsByImporter.Sum(t => t.Amounts.TotalAmount);
						}
					}

					readonly IEnumerable<TransactionDetails> transactionsByImporter;
				}

				#endregion

				readonly SegmentGroup21 group21;
				readonly ZString accountSecurityNumber;
			}

			#endregion

			readonly SegmentGroup10 group10;
			readonly ZString accountSecurityNumber;
		}

		#endregion

		#region DailyAccountingTotals

		public BusinessObjectCollectionWrapper<DailyAccountingTotal> DailyAccountingTotals
		{
			get
			{
				var groups = from SegmentGroup10 group10 in message.Group10
							 where group10.DMS[0].DocumentMessageIdentification.DocumentMessageNumber == "K36"
										 || group10.DMS[0].DocumentMessageIdentification.DocumentMessageNumber == "K40"
							 select new DailyAccountingTotal(group10);
				return new BusinessObjectCollectionWrapper<DailyAccountingTotal>(groups);
			}
		}

		[TestExcludeBusinessObjectsAllHaveTestCases]
		public class DailyAccountingTotal : NonPersistentBusinessObject, ITableInterpretation
		{
			internal DailyAccountingTotal(SegmentGroup10 group10)
			{
				Argument.NotNull(group10, "group10");
				this.group10 = group10;
			}

			public ZString TotalsDescription
			{
				get
				{
					switch (group10.DMS[0].DocumentMessageIdentification.DocumentMessageNumber)
					{
						case "K36":
							return Res.GetString("30610003-9468-4067-9241-b3fd3e196511", "Broker Total");
						case "K40":
							return Res.GetString("29491b4f-946e-4718-bd85-11938007a010", "Report Grand Total");
						default:
							return Res.GetString("21db8744-754c-4750-9cc5-ac77ff78ad01", "Unknown Totals Group");
					}
				}
			}

			public ZDate StatementDate
			{
				get { return D99BMessageUtilities.GetDate(group10.DTM, DateTimePeriodFunctionCodeQualifierList.CurrentReportDate); }
			}

			public K84AmountsWithPenaltyWrapper Amounts
			{
				get { return (from SegmentGroup11 group11 in group10.Group11 select new K84AmountsWithPenaltyWrapper(group11.MOA)).FirstOrDefault(); }
			}

			#region Implementation of ITableInterpretation

			string ITableInterpretation.Caption
			{
				get { return string.Empty; }
			}

			IEnumerable<string> ITableInterpretation.Titles
			{
				get { return new[] { string.Empty }.Concat(PropertyNameProvider.GetColumnTitles<K84AmountsWithPenaltyWrapper>()); }
			}

			IEnumerable<object> ITableValues.Values
			{
				get { return new[] { new CellWithFormatting(TotalsDescription, true) }.Concat(Amounts.ToArray()); }
			}

			#endregion

			readonly SegmentGroup10 group10;
		}

		#endregion

		ZGuid ISourceIdentifierProvider.SourceIdentifier => this.k84Message.PK;
	}
}
