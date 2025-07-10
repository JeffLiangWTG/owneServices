using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.Integration;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.Aggregator;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Accounting.Integration;
using Enterprise.Billing.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLJournals
{
	public partial class MultiCompaniesGLJournalFlatFileDataImporter : GLJournalFlatFileDataImporter
	{
		protected override string ImportTypeForDuplicatesPrevention => "GLJournal"; // Hardcoded Identifier for the DataImportHistory

		protected override int DaysToKeepHistoryFor => 7;

		protected override bool ShouldCheckImportHistoryInCurrentCompany => false;

		protected override IValueObject CreateXsd()
		{
			return new Xsd.GLJournalCollection();
		}

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber)
		{
			Converter = new MultiCompaniesGLJournalFlatFileConverter(notificationSubscriber, FactoryProvider.Current);
			return Converter;
		}

		protected override bool ShouldSuspendValidation
		{
			get { return false; }
		}

		protected virtual string UploadGLJournalCountFeatureCode => UsageFeatures.Codes.UploadGLJournalCount;

		protected virtual string UploadGLJournalDetailsFeatureCode => UsageFeatures.Codes.UploadGLJournalDetails;

		protected override bool ImportDataToFactoryCore(TextReader dataReader, string attachmentFileName, INotifications notifications, out ITransactionParticipant[] additionalTransactionActions)
		{
			var result = base.ImportDataToFactoryCore(dataReader, attachmentFileName, notifications, out additionalTransactionActions);
			result &= !(notifications is NotificationBuffer notificationBuffer && notificationBuffer.HasErrors);

			if (result)
			{
				((StreamReader)dataReader).BaseStream.Position = 0;
				var fileInfo = (Path.GetFileName(attachmentFileName), ((StreamReader)dataReader).BaseStream.ToByteArray());
				var transactionActions = GetAdditionalTransactionParticipants(fileInfo);

				if (ImportedJournals.Any())
				{
					var identifier = ZGuid.NewZGuid();

					foreach (GLJournal gljournal in ImportedJournals)
					{
						UsageCollector.Report(LastImportedJournal.Factory, UploadGLJournalDetailsFeatureCode, (UsageProperties.JournalIdentifier, identifier),
							(UsageProperties.JournalType, gljournal.AH_TransactionType), (UsageProperties.JournalCompanyCode, gljournal.Company.GC_Code));
					}

					var countOfJournalTypes = ImportedJournals.Cast<GLJournal>().GroupBy(x => x.AH_TransactionType).Count();
					var countOfCompanies = ImportedJournals.Cast<GLJournal>().GroupBy(x => x.Company.GC_Code).Count();

					UsageCollector.Report(LastImportedJournal.Factory, UploadGLJournalCountFeatureCode, (UsageProperties.JournalIdentifier, identifier),
						(UsageProperties.Count, ImportedJournals.Count), (UsageProperties.CountOfJournalTypes, countOfJournalTypes), (UsageProperties.CountOfCompanies, countOfCompanies));
				}

				string message;
				if (Converter.IsFileHeader)
				{
					message = Res.GetString("42BBDA73-AA68-4708-BCF5-693F172DBEB9", "Journals Processed Successfully: 1 File Header, {0} Journal Lines", totalLineCount);
				}
				else
				{
					message = Res.GetString("1B557157-EC40-41E5-BB53-74E4E54A1489", "Journals Processed Successfully: {0} Journal Headers, {1} Journal Lines", totalHeaderCount, totalLineCount);
				}
				notifications.Notify(new InfoNotification(message));

				additionalTransactionActions = additionalTransactionActions.Union(transactionActions).ToArray();
			}
			else
			{
				UsageCollector.Report(UploadGLJournalCountFeatureCode);
			}

			return result;
		}

		protected override void OnAfterImportData(NotificationBuffer buffer, bool sucessfullyImported)
		{
			base.OnAfterImportData(buffer, sucessfullyImported);

			if (sucessfullyImported && ImportedJournals.Any())
			{
				var details = new StringBuilder();
				details.AppendLine();
				details.AppendLine(Res.GetString("39cc4ab5-cbc4-4b7b-ae28-29fe64fcaa47", "Imported data details below:"));

				var count = 1;
				var groupedJournals = ImportedJournals.Cast<GLJournal>().GroupBy(x => x.Company.GC_Code);
				foreach (var companyGroup in groupedJournals)
				{
					details.Append(string.Format($"{count}. {companyGroup.Key}: "));
					details.Append(string.Join(", ", companyGroup.Select(x => x.AH_TransactionNum).ToList().OrderBy(x => x)));
					details.AppendLine();
					count++;
				}

				fNotifications.Notify(new InfoNotification(details.ToString()));
			}
		}

		ITransactionParticipant[] GetAdditionalTransactionParticipants((string fileName, byte[] fileStream) fileInfo)
		{
			var transactionActions = new List<ITransactionParticipant>();

			foreach (GLJournal journal in ImportedJournals)
			{
				using (var tempUserContext = !journal.Branch.GB_Code.IsEmpty ? DisposableEnvironment.ForBranch(journal.Branch.GB_Code, false, Env.CurrentUser) : null)
				{
					var securityOverrideProvider = new NonInteractiveGLJournalSecurityOverrideProvider(true);
					var guiProvider = new NonInteractiveTransactionApprovalGUIProvider(FactoryProvider.Current, securityOverrideProvider);
					var multiCompaniesGLJournalLevelAuthorizationWithApprovalRequest = new MultiCompaniesGLJournalLevelAuthorizationWithApprovalRequest(guiProvider, journal, false);
					multiCompaniesGLJournalLevelAuthorizationWithApprovalRequest.PerformLevelAuthorization();

					var approvalRequest = multiCompaniesGLJournalLevelAuthorizationWithApprovalRequest.LastApprovalRequest;

					if (approvalRequest != null && approvalRequest.XP_ApprovalStatus == Core.Constants.GenApprovalRequestApprovalStatus.Posted)
					{
						if (!transactionActions.Contains(journal.Factory))
						{
							transactionActions.Add(journal.Factory);
						}

						var aggregator = new AggregateWrapper(journal, journal);
						transactionActions.Add(aggregator);
					}

					AttachUploadFileToeDocs(fileInfo, approvalRequest, journal);
				}
			}

			return transactionActions.ToArray();
		}

		protected override bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
		{
			fNotifications = notifications;

			foreach (Xsd.GLJournal journalXsd in (Xsd.GLJournalCollection)xsd)
			{
				totalHeaderCount++;
				totalLineCount += journalXsd.JournalLines.Count;

				var branchCode = journalXsd.GLDetail.Branch;

				using (var tempUserContext = !branchCode.IsEmpty ? DisposableEnvironment.ForBranch(branchCode, false, Env.CurrentUser) : null)
				{
					if (tempUserContext == null)
					{
						continue;
					}

					if (!Env.Security.NewGeneralLedgerJournal.IsAllowed)
					{
						NotifyErrorWithJournalDetail(Env.Security.NewGeneralLedgerJournal.ErrorMessageForNotAllowed);
					}
					else
					{
						base.ExtractToDataAdapter(journalXsd, notifications);

						LastImportedJournal.RunPreSaveValidation();
						if (LastImportedJournal.HasErrors)
						{
							var errorList = LastImportedJournal.GetErrors().ToList();
							errorList.ForEach(x =>
							{
								var errorMessage = x.Message.Replace("Error - ", string.Empty);
								NotifyErrorWithJournalDetail(errorMessage);
							});
						}
						else if (!LastImportedJournal.IsNoteJournal)
						{
							SetExchangeRateDifferenceBalanceJournal();
							if (!LastImportedJournal.IsBalanced)
							{
								ZDecimal roundedBalance = AccountingUtils.Round(LastImportedJournal.AH_LocalExTaxAmount, LastImportedJournal.Company.LocalCurrency.Code);
								NotifyErrorWithJournalDetail(Res.GetString("5DA4DAAB-9611-48B3-8F40-C5ED432F56E4", "Journal does not balance. Discrepancy: {0}", roundedBalance.ToString(LastImportedJournal.Company.LocalCurrency.Decimals)));
							}
						}

						ImportedJournals.Add(LastImportedJournal);
					}
				}
			}

			return true;
		}

		int totalHeaderCount;
		int totalLineCount;

		void AttachUploadFileToeDocs((string fileName, byte[] fileAsBytes) fileInfo, GLJournalApprovalRequest approvalRequest, GLJournal journal)
		{
			if (fileInfo.fileAsBytes != null)
			{
				IDocManagerSupport toAttachObj;
				var liveRequestStatuses = new[] { Core.Constants.GenApprovalRequestApprovalStatus.Requested, Core.Constants.GenApprovalRequestApprovalStatus.Approved };

				if (liveRequestStatuses.Contains(approvalRequest?.XP_ApprovalStatus.ToString()))
				{
					toAttachObj = approvalRequest;
				}
				else
				{
					toAttachObj = journal;
				}

				toAttachObj.DocManagerInfo.AddFileOrDocument(fileInfo.fileAsBytes, fileInfo.fileName, Core.Constants.DocManagerCodes.GLJournal);
				DocManagerInfoExtensions.ForceToUseAnotherFactory(toAttachObj.DocManagerInfo, FactoryProvider.Current);
				(toAttachObj as EnterpriseBusinessObject)?.Logs.AddNew(Events.DataImport, ZDateTimeOffset.Now);
			}
		}

		void SetExchangeRateDifferenceBalanceJournal()
		{
			var balancingAccount = AccountingMasterFilesRegistry.Instance.GLJournalExchangeRateDifferenceAccount.Value;

			if (balancingAccount == Guid.Empty)
			{
				return;
			}

			var balanceCalculator = LastImportedJournal.GLJournalLines
				.Cast<GLJournalLine>()
				.Where(jl => jl.AL_RX_NKTransactionCurrency != LastImportedJournal.Company.GC_RX_NKLocalCurrency)
				.GroupBy(jl => jl.AL_RX_NKTransactionCurrency)
				.Select(lines =>
				{
					return new
					{
						Currency = lines.Key,
						OSAmountBalance = (ZDecimal)(lines.Sum(line => line.AL_OSAmount)),
						LocalAmountBalance = (ZDecimal)(lines.Sum(line => line.AL_LineAmount)),
					};
				})
				.ToArray();

			foreach (var balancePerCurrency in balanceCalculator)
			{
				if (balancePerCurrency.OSAmountBalance == 0 && balancePerCurrency.LocalAmountBalance != 0)
				{
					var balancingLine = LastImportedJournal.GLJournalLines.AddNew();
					balancingLine.AL_AG = balancingAccount;
					balancingLine.UnsignedLocalLineAmount = Math.Abs(balancePerCurrency.LocalAmountBalance);
					balancingLine.DebitCreditSign = balancePerCurrency.LocalAmountBalance < 0 ? DebitCreditDataEntry.DR : DebitCreditDataEntry.CR;
					balancingLine.AL_Desc = Res.GetString("898A6754-F813-497A-BCFE-A81CC23DB1D9", "Foreign Currency Exchange Difference for {0}", balancePerCurrency.Currency);
				}
			}
		}

		GLJournalCollection ImportedJournals
		{
			get { return fImportedJournals ??= new GLJournalCollection(FactoryProvider.Current); }
		}

		void NotifyErrorWithJournalDetail(string message)
		{
			var errorWithJournalDetail = Res.GetString("B0434EB7-131C-49A7-8B99-F8B16D039C9A", "Journal Error '{0}' - {1}", GlbCompany.CurrentCompany.GC_Code, message);
			fNotifications.Notify(new ErrorNotification(ErrorType.Error, errorWithJournalDetail));
		}

		protected override GLJournal CreateGLJournal()
		{
			var newFactory = FactoryProvider.Current.CreateNewFactory();
			return newFactory.New<GLJournal>();
		}

		GLJournalCollection fImportedJournals;
		INotifications fNotifications;

		protected override GLJournalDataAdapter Adapter
		{
			get
			{
				var adapter = new MultiCompaniesGLJournalDataAdapter();
				var adapterSettings = new GLJournalDataAdapterSettings { factoryForNewJournal = LastImportedJournal.Factory };
				adapter.Initialize(adapterSettings);
				return adapter;
			}
		}

		protected MultiCompaniesGLJournalFlatFileConverter Converter;
	}
}
