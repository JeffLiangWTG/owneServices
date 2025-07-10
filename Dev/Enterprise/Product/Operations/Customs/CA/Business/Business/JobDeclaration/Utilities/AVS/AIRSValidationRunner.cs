using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.CA.Services;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public sealed class AIRSValidationRunner
	{
		public AIRSValidationRunner(JobDeclaration declaration)
		{
			Argument.NotNull(declaration, nameof(declaration));
			this.declaration = declaration;
			settings = new AIRSValidationServiceSettings(declaration);
			validationService = new AIRSValidationServiceProxy(settings);
		}
		readonly JobDeclaration declaration;
		readonly AIRSValidationServiceSettings settings;
		readonly AIRSValidationServiceProxy validationService;

#if DEBUG
		public AIRSValidationRunner(JobDeclaration declaration, AIRSValidationServiceProxy validationService)
		{
			Argument.NotNull(declaration, nameof(declaration));
			Argument.NotNull(validationService, nameof(validationService));
			this.declaration = declaration;
			this.validationService = validationService;
		}
#endif

		public ZString ValidateSetting()
		{
			return settings?.CheckValidationServiceSetting() ?? ZString.Empty;
		}

		public AIRSValidationQueriedLineCollection AIRSValidationAll(bool runInAnotherThread, CancellationTokenSource cancellationTokenSource)
		{
			Argument.NotNull(cancellationTokenSource, nameof(cancellationTokenSource));
			return ValidateRequirements(GetAllLinesForAIRSValidation, cancellationTokenSource, runInAnotherThread);
		}

		public AIRSValidationQueriedLineCollection AIRSValidationNOT(bool runInAnotherThread, CancellationTokenSource cancellationTokenSource)
		{
			Argument.NotNull(cancellationTokenSource, nameof(cancellationTokenSource));
			return ValidateRequirements(GetNOTLinesForAIRSValidation, cancellationTokenSource, runInAnotherThread);
		}

		AIRSValidationQueriedLineCollection GetAllLinesForAIRSValidation(JobDeclaration declaration)
		{
			return CreateAIRSValidationQueriedLineCollection(declaration.InvoiceLines.Cast<JobComInvoiceLine>().Where(l => l.IsRegulatedByCFIA || l.IsRegulatedByIIDCFIA));
		}

		AIRSValidationQueriedLineCollection GetNOTLinesForAIRSValidation(JobDeclaration declaration)
		{
			return CreateAIRSValidationQueriedLineCollection(declaration.InvoiceLines.Cast<JobComInvoiceLine>().Where(l => notValidatedStatusList.Contains(l.CA_OGDStatus)));
		}

		readonly ZString[] notValidatedStatusList = new ZString[]
		{
			AVSStatusList.Codes.NotValidated,
			AVSStatusList.Codes.Error,
			AVSStatusList.Codes.Unknown
		};

		AIRSValidationQueriedLineCollection CreateAIRSValidationQueriedLineCollection(IEnumerable<JobComInvoiceLine> invoiceLines)
		{
			var result = new AIRSValidationQueriedLineCollection();
			bool isIID = (settings?.SchemaVersion ?? string.Empty) == AIRSValidationServiceSchemaVersion.IID;

			foreach (var invoiceLine in invoiceLines)
			{
				if (isIID)
				{
					result.AddNewOrUpdateExistingIID(invoiceLine);
				}
				else
				{
					result.AddNewOrUpdateExistingOGD(invoiceLine);
				}
			}
			return result;
		}

		AIRSValidationQueriedLineCollection ValidateRequirements(Func<JobDeclaration, AIRSValidationQueriedLineCollection> buildQueriedLines, CancellationTokenSource cancellationTokenSource, bool runInAnotherThread)
		{
			AIRSValidationQueriedLineCollection lines = buildQueriedLines(declaration);

			if (lines != null && lines.Count > 0)
			{
				if (runInAnotherThread && !Globals.IsTest)
				{
					new Thread(new ThreadStart(() =>
					{
						ValidateRequirementsCore(lines, cancellationTokenSource);
					})).Start();
				}
				else
				{
					ValidateRequirementsCore(lines, cancellationTokenSource);
				}
			}
			else
			{
				CancelTokenSource(cancellationTokenSource);
			}

			return lines;
		}

		void ValidateRequirementsCore(AIRSValidationQueriedLineCollection lines, CancellationTokenSource cancellationTokenSource)
		{
			try
			{
				validationService.ValidateRequirements(lines, cancellationTokenSource);
			}
			finally
			{
				CancelTokenSource(cancellationTokenSource);
			}
		}

		void CancelTokenSource(CancellationTokenSource cancellationTokenSource)
		{
			if (!cancellationTokenSource.IsCancellationRequested)
			{
				cancellationTokenSource.Cancel();
			}
		}

		public ZString PopulateValidateRequirementResults(JobDeclaration declaration, AIRSValidationQueriedLineCollection queriedLines, ZDateTime queryTime)
		{
			var errorMessages = new List<string>();
			foreach (IAIRSValidationQueriedLine queriedLine in queriedLines)
			{
				if (!queriedLine.ValidationFaultMessage.IsEmpty && !errorMessages.Contains(queriedLine.ValidationFaultMessage))
				{
					errorMessages.Add(queriedLine.ValidationFaultMessage);
				}

				foreach (var pk in queriedLine.InvoiceLinePKs)
				{
					var invoiceLine = (JobComInvoiceLine)declaration.InvoiceLines.FindByPK(pk);
					if (invoiceLine != null)
					{
						PopulateValidateRequirementResult(queriedLine, invoiceLine, queryTime);
					}
				}
			}
			declaration.RecalculateConsolidatedAVSStatus();

			return new ZStringBuilder(errorMessages).ToStringWithNewLineBetweenAppends();
		}

		void PopulateValidateRequirementResult(IAIRSValidationQueriedLine queriedLine, JobComInvoiceLine invoiceLine, ZDateTime queryTime)
		{
			var avsStatus = AVSStatusList.Codes.Rejected;
			var message = ZString.Empty;

			if (!queriedLine.ValidationFaultMessage.IsEmpty)
			{
				avsStatus = AVSStatusList.Codes.Error;
				message = queriedLine.ValidationFaultMessage;
			}
			else
			{
				message = queriedLine.ValidationResponse;
				if (message.IsEmpty)
				{
					avsStatus = AVSStatusList.Codes.WillBeApproved;
					message = Res.GetString("cb300a2f-8254-4c75-86c4-7f20fd439611", "No errors reported by CFIA");
				}
				else
				{
					foreach (var pair in ResponseMessageStatusPairList)
					{
						if (message.Contains(pair.Key, StringComparison.OrdinalIgnoreCase))
						{
							avsStatus = pair.Value;
							break;
						}
					}
				}
			}

			invoiceLine.CA_OGDStatus = avsStatus;
			invoiceLine.Notes.AddNew(false, PredefinedNoteTypes.Instance.AIRSValidationResults.Description, ZString.Join("|", new ZString[] { queryTime.ToISO8601String(), message }));
		}

		Dictionary<string, string> ResponseMessageStatusPairList
		{
			get
			{
				if (fResponseMessageStatusDict == null)
				{
					fResponseMessageStatusDict = new Dictionary<string, string>();
					fResponseMessageStatusDict.Add("cannot be imported", AVSStatusList.Codes.NotImport);
					fResponseMessageStatusDict.Add("rejected", AVSStatusList.Codes.Rejected);
					fResponseMessageStatusDict.Add("review", AVSStatusList.Codes.ReviewRequired);
					fResponseMessageStatusDict.Add("inspect", AVSStatusList.Codes.InspectionRequired);
					fResponseMessageStatusDict.Add("do not require", AVSStatusList.Codes.NoActionRequired);
					fResponseMessageStatusDict.Add("approved", AVSStatusList.Codes.WillBeApproved);
					fResponseMessageStatusDict.Add("not regulated", AVSStatusList.Codes.Blank);
				}
				return fResponseMessageStatusDict;
			}
		}
		Dictionary<string, string> fResponseMessageStatusDict;
	}
}
