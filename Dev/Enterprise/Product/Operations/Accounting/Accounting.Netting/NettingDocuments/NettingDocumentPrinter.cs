using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Netting
{
	public class NettingDocumentPrinter : NonPersistentBusinessObject, IObsoleteValidation, IDocumentSupportable
	{
		public NettingDocumentPrinter(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public NettingDocumentPrinter(BusinessObjectFactory factory, ZGuid nettingPeriod)
			: base(factory)
		{
			NettingPeriod = nettingPeriod;
		}

		public static IBusiness New()
		{
			return new NettingDocumentPrinter(new BusinessObjectFactory());
		}

		[List("NettingPeriodCollection")]
		public ZGuid NettingPeriod
		{
			get
			{
				if (nettingPeriod.IsEmpty)
				{
					var firstOpenPeriod = NettingPeriodHelper.GetFirstOpenNettingPeriod(GlbCompany.CurrentCompany.PK, Factory);
					nettingPeriod = firstOpenPeriod != null ? firstOpenPeriod.PK : ZGuid.Empty;
				}

				return nettingPeriod;
			}
			set
			{
				SetNonPersistentPropertyValue(NettingPeriodInfo, ref nettingPeriod, value);
				NettingPeriodInfo.RefreshBinding();
			}
		}
		ZGuid nettingPeriod;

		public NettingSystemPeriod Period
		{
			get { return Factory.Load<NettingSystemPeriod>(NettingPeriod); }
		}

		public ZPropertyInfo NettingPeriodInfo
		{
			get { return GetZPropertyInfo(nameof(NettingPeriod)); }
		}

		public NettingSystemPeriodCollection NettingPeriodCollection
		{
			get
			{
				if (nettingPeriodCollection == null)
				{
					nettingPeriodCollection = new NettingSystemPeriodCollection(Factory);
				}

				return nettingPeriodCollection;
			}
		}
		NettingSystemPeriodCollection nettingPeriodCollection;

		public ZString PrintAllFinalParticipantStatements()
		{
			var statementType = StatementType.Final;
			var participantStatement = new ParticipantStatement(Factory, NettingPeriod, statementType);

			var period = participantStatement.Period;
			var message = ZString.Empty;

			if (!period.NSP_IsComplete)
			{
				message = Res.GetString("2f122f23-8404-486c-997b-233b529912c1", "Final Participant Statements can only be generated after the Cycle is finalized.", period.NSP_Period);
			}
			else
			{
				PrintAllParticipantStatements(participantStatement, DocumentTemplateNames.ParticipantStatementDocumentMenuName, statementType);
			}

			return message;
		}

		public void PrintParticipantStatement(ZString orgCode)
		{
			var participantStatement = new ParticipantStatement(Factory, NettingPeriod, orgCode, StatementType.Final);
			var docCommand = GetStatementDocumentCommand(participantStatement, DocumentTemplateNames.ParticipantStatementDocumentMenuName);
			using (var printTask = new DocumentPrintSet(docCommand))
			{
				using (var docPack = new DocumentPack(docCommand, participantStatement, null, null))
				{
					printTask.Add(docPack);
				}
				printTask.Run(Env.Security.NettingDocuments);
#if DEBUG
				PrintTask_ForTestOnly = printTask;
#endif
			}
		}

		public void PrintAllTrialParticipantStatements()
		{
			var statementType = StatementType.Trial;
			var participantStatement = new ParticipantStatement(Factory, NettingPeriod, statementType);

			PrintAllParticipantStatements(participantStatement, DocumentTemplateNames.TrialParticipantStatementDocumentMenuName, statementType);
		}

		public void PrintTrialParticipantStatement(ZString orgCode)
		{
			var participantStatement = new ParticipantStatement(Factory, NettingPeriod, orgCode, StatementType.Trial);
			var docCommand = GetStatementDocumentCommand(participantStatement, DocumentTemplateNames.TrialParticipantStatementDocumentMenuName);
			using (var printTask = new DocumentPrintSet(docCommand))
			{
				using (var docPack = new DocumentPack(docCommand, participantStatement, null, null))
				{
					printTask.Add(docPack);
				}
				printTask.Run(Env.Security.NettingDocuments);
#if DEBUG
				PrintTask_ForTestOnly = printTask;
#endif
			}
		}

		public void PrintAllTrialDetailedParticipantStatements()
		{
			var statementType = StatementType.Trial;
			var participantStatement = new ParticipantStatement(Factory, NettingPeriod, statementType);

			PrintAllParticipantStatements(participantStatement, DocumentTemplateNames.TrialDetailedParticipantStatementDocumentMenuName, statementType);
		}

		public ZString PrintDetailedParticipantStatements()
		{
			var statementType = StatementType.Final;
			var participantStatement = new ParticipantStatement(Factory, NettingPeriod, statementType);

			var period = participantStatement.Period;
			var message = ZString.Empty;

			if (!period.NSP_IsComplete)
			{
				message = Res.GetString("ea8e84eb-78bb-4cb3-a063-315ef77f761e", "Detailed Participant Statements can only be generated after the Cycle is finalized.", period.NSP_Period);
			}
			else
			{
				PrintAllParticipantStatements(participantStatement, DocumentTemplateNames.DetailedParticipantStatementDocumentMenuName, statementType);
			}

			return message;
		}

		public ZString PrintClearingJournals()
		{
			var statementType = StatementType.Final;
			var participantStatement = new ParticipantStatement(Factory, NettingPeriod, statementType);

			var period = participantStatement.Period;
			var message = ZString.Empty;

			if (!period.NSP_IsComplete)
			{
				message = Res.GetString("a74320d2-0ffa-41b6-bc1a-3d0d049d631d", "Netting Clearing Journals can only be generated after the Cycle is finalized.", period.NSP_Period);
			}
			else
			{
				PrintAllParticipantStatements(participantStatement, DocumentTemplateNames.ClearingJournalsDocumentName, statementType);
			}

			return message;
		}

		void PrintAllParticipantStatements(ParticipantStatement participantStatement, ZString menuName, StatementType statementType)
		{
			var docCommand = GetStatementDocumentCommand(participantStatement, menuName);
			var allParticipants = GetAllParticipants(statementType);

			var totalDocPack = allParticipants.Count();
			DocumentPrintSet printTask = null;
			if (totalDocPack > PrintTask.MaxPreviewCount)
			{
				printTask = new DocumentPrintSetWithStreaming(docCommand, totalDocPack, GetDocPackForParticipantStatement(docCommand, allParticipants, statementType));
			}
			else
			{
				printTask = new DocumentPrintSet(docCommand);
				printTask.AddRange(GetDocPackForParticipantStatement(docCommand, allParticipants, statementType));
			}

			if (printTask != null)
			{
				printTask.Run(Env.Security.NettingDocuments);
#if DEBUG
				PrintTask_ForTestOnly = printTask;
#endif
				printTask.Dispose();
			}
		}

		public DocumentPrintSet PrintTask_ForTestOnly { get; set; }

		IEnumerable<ZString> GetAllParticipants(StatementType statementType)
		{
			IEnumerable<ZString> allParticipants = new List<ZString>();
			NettingStatement statement = new NettingCentreStatement(Factory, NettingPeriod, statementType);

			var currenciesWithoutExRate = statement.GetCurrenciesWithOutExchangeRate();
			if (!currenciesWithoutExRate.Any())
			{
				allParticipants = statement.GetAllParticipants();
			}
			else
			{
				var message = ZString.Empty;
				if (statement.IsFinalOrIntermediateStatement)
				{
					message = ExecutionRateMissingMessage;
				}
				else
				{
					message = IndicativeRateMissingMessage;
				}

				throw new IncorrectDataSetupException(Res.GetString("ad7e9f63-0eba-4bd4-ba70-73e762aca815", "{0}{1}{2}", message, System.Environment.NewLine, statement.GetCurrencyStringWithOutExchangeRate(currenciesWithoutExRate)));
			}

			return allParticipants;
		}

		IEnumerable<DocumentPack> GetDocPackForParticipantStatement(DocumentCommand docCommand, IEnumerable<ZString> allParticipants, StatementType statementType)
		{
			foreach (var participant in allParticipants)
			{
				var participantStatement = new ParticipantStatement(Factory, NettingPeriod, participant, statementType);

				var docPack = new DocumentPack(docCommand, participantStatement, null, null);

				yield return docPack;
			}
		}

		public ZString PrintClearingBankPaymentDocument()
		{
			var nettingCycleStatement = new NettingCentreStatement(Factory, NettingPeriod, StatementType.Final);

			var period = nettingCycleStatement.Period;
			var message = ZString.Empty;

			if (!period.NSP_IsComplete)
			{
				message = Res.GetString("7f316075-bfd2-4a0e-8190-cae29cfd1c60", "Clearing Bank Payment Document can only be generated after the Cycle is finalized.", period.NSP_Period);
			}
			else
			{
				var currenciesWithoutExRate = nettingCycleStatement.GetCurrenciesWithOutExchangeRate().ToList();
				if (!currenciesWithoutExRate.Any())
				{
					var docCommand = GetStatementDocumentCommand(nettingCycleStatement, DocumentTemplateNames.ClearingBankPaymentDocumentName);
					using (var printTask = new DocumentPrintSet(docCommand))
					{
						using (var docPack = new DocumentPack(docCommand, nettingCycleStatement, null, null))
						{
							if (printTask != null)
							{
								printTask.Add(docPack);
								printTask.Run(Env.Security.NettingDocuments);
							}
						}
					}
				}
				else
				{
					throw new IncorrectDataSetupException(string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", ExecutionRateMissingMessage, System.Environment.NewLine, nettingCycleStatement.GetCurrencyStringWithOutExchangeRate(currenciesWithoutExRate)));
				}
			}

			return message;
		}

		static ZString IndicativeRateMissingMessage
		{
			get
			{
				return Res.GetString("4b0168ae-dded-442d-953c-5fa688b95c13", "Indicative exchange rate is missing for the following currency(s):");
			}
		}

		static ZString ExecutionRateMissingMessage
		{
			get
			{
				return Res.GetString("a1210f1d-f789-4f17-b8f9-acba982cfc4c", "Execution exchange rate is missing for the following currency(s):");
			}
		}

		DocumentCommand GetStatementDocumentCommand(NettingStatement bizO, string menuName)
		{
			var commandFilter = new ZQuery(StmMenuItemSchema.SU_MenuName, menuName);
			commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.Equal, BusinessContext.ParticipantStmnt);
			commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_IsPublished, SQLComparisonOperator.Equal, Core.Constants.BooleanTrueChar);
			commandFilter.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_MenuPath, SQLComparisonOperator.Equal, string.Empty);

			var documentCommands = new DocumentCommandCollection(bizO);
			documentCommands.Load();
			var commands = documentCommands.Find(commandFilter);
			if (commands.Length < 1)
			{
				throw new Exception();
			}
			return (DocumentCommand)commands[0];
		}

		public ZString CallFinaliseNettingCycle()
		{
			var notification = new NotificationBuffer();
			var errorMessageBuilder = new ZStringBuilder();

			var statement = new NettingCentreStatement(Factory, NettingPeriod, StatementType.Intermediate);
			var period = statement.Period;
			if (period.NSP_IsComplete)
			{
				return Res.GetString("00ab7c84-18c9-4520-a5b0-4d3358aace40", "The Cycle '{0}' is already marked as completed.", period.NSP_Period);
			}
			else
			{
				if ((Guid)AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.Value == Guid.Empty)
				{
					return Res.GetString("a10782f7-fcde-4cd7-bc2a-940c07471ece", "Please set up Netting Control Account from the following Registry Item: {0}", AccountingConfigurationRegistry.Instance.NettingMatchingControlAccount.LocationMultilingual);
				}

				var currenciesWithoutExRate = statement.GetCurrenciesWithOutExchangeRate().ToList();
				if (currenciesWithoutExRate.Any())
				{
					return string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}", ExecutionRateMissingMessage, System.Environment.NewLine, statement.GetCurrencyStringWithOutExchangeRate(currenciesWithoutExRate));
				}

				var periodManager = new NettingPeriodManager(period);
				periodManager.FinaliseNettingCycle(notification);

				if (notification.HasErrors)
				{
					foreach (var n in notification.Events)
					{
						errorMessageBuilder.AppendLine(n.Message);
					}
				}
			}

			return errorMessageBuilder.ToString();
		}

		public ZString PrintParticipantStatementsAndClearJournals()
		{
			var message = ZString.Empty;

			try
			{
				PrintAllFinalParticipantStatements();
				PrintClearingJournals();
			}
			catch (IncorrectDataSetupException ex)
			{
				message = ex.Message;
			}
			return message;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Name of document template")]
		static class DocumentTemplateNames
		{
			internal const string ParticipantStatementDocumentMenuName = "Participant Statement";
			internal const string TrialParticipantStatementDocumentMenuName = "Trial Participant Statement";
			internal const string DetailedParticipantStatementDocumentMenuName = "Detailed Participant Statement";
			internal const string TrialDetailedParticipantStatementDocumentMenuName = "Trial Detailed Participant Statement";
			internal const string ClearingBankPaymentDocumentName = "Clearing Bank Payment Document";
			internal const string ClearingJournalsDocumentName = "Clearing Journals";
		}

		public DocumentSupporter DocumentSupporter
		{
			get { return new NettingDocumentPrinterDocumentSupporter(this); }
		}
	}

	public class NettingDocumentPrinterDocumentSupporter : DocumentSupporter
	{
		public NettingDocumentPrinterDocumentSupporter(NettingDocumentPrinter nettingDocumentPrinter)
			: base(nettingDocumentPrinter)
		{ }

		NettingDocumentPrinter NettingDocumentPrinter
		{
			get { return (NettingDocumentPrinter)BusinessObject; }
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.ParticipantStmnt; }
		}

		public override ZArchitecture.Modules.ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.NettingCustomiseDocuments; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return DocumentWrapperFactory.GenerateGenericWrappers(dataContext, NettingDocumentPrinter);
		}

		protected override Core.Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Core.Constants.DataContext[] { Core.Constants.DataContext.GenericFreightJob, Core.Constants.DataContext.NettingParticipantStatement };
		}
	}
}
