using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class JobDeclarationDocumentSupporter : BaseJobDeclarationDocumentSupporter
	{
		public JobDeclarationDocumentSupporter(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected JobDeclaration JobDeclaration
		{
			get { return (JobDeclaration)BusinessObject; }
		}

		protected override ZString MessageForInvaildEntryPrintDataState(IStmMenuItem menuItem)
		{
			return menuItem.SU_MenuName + " cannot be printed until the Declaration is Merged. Selecting Brokerage > Answer Declaration Questions will Merge the Declaration.";
		}

		protected override string AUCountryMenuTemplateFilterValue
		{
			get { return "Yes"; }
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			List<DataContext> result = new List<DataContext>(base.GetSupportedDataContexts());
			result.Add(DataContext.CartageAdvice);
			result.Add(DataContext.ATD);
			result.Add(DataContext.EFTPaymentAdvice);
			return result.ToArray();
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;

			switch (dataContext)
			{
				case Core.Constants.DataContext.DeclarationWithCusEntryHeaders:
					if (JobDeclaration.CustomsEntryHeaders.Count > 0)
					{
						result = new DocumentWrapper[] { DocumentWrapperFactory.CreateCustomsWrapper(Core.Constants.DataContext.Declaration, JobDeclaration, GlbCompany.CurrentCompany.GC_RN_NKCountryCode) };
					}
					break;

				case Core.Constants.DataContext.CusEntryHeader:
				case Core.Constants.DataContext.ATD:
				case Core.Constants.DataContext.EFTPaymentAdvice:
					System.Collections.ArrayList list = new System.Collections.ArrayList();
					for (int i = 0; i < JobDeclaration.CustomsEntryHeaders.Count; i++)
					{
						CusEntryHeader entry = JobDeclaration.CustomsEntryHeaders[i];
						if (entry.IsActive || entry.RandomHeader != null)//IsActive or withdrawn with invoice lines still attached
						{
							DocumentWrapper[] entryHeaderWrappers = JobDeclaration.CustomsEntryHeaders[i].DocumentSupporter.GetDocumentWrappers(dataContext, commandBeingRun);
							if (entryHeaderWrappers != null)
							{
								foreach (DocumentWrapper wrapper in entryHeaderWrappers)
								{
									list.Add(wrapper);
								}
							}
						}
					}

					result = new DocumentWrapper[list.Count];
					for (int i = 0; i < list.Count; i++)
					{
						result[i] = (DocumentWrapper)list[i];
					}

					break;

				default:
					result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
					break;
			}

			return result;
		}

		protected override ZString DocDataProvidersNotFoundMessageForDeclarationWithCusEntryHeaders => "You must enter Community Protection details before viewing this document.";

		protected override DocumentSupporterDataState GetDataStateBeforeRunCore(IStmMenuItem commandAboutToBeRun)
		{
			if (commandAboutToBeRun.SU_MenuName.StartsWith("Authority To Deal"))
			{
				if (ATDPrintableEntryHeaders.Count < 1)
				{
					return new DocumentSupporterDataState(ZBool.False, CusEntryHeader.ATDNotPrintableMessageText);
				}
			}
			return base.GetDataStateBeforeRunCore(commandAboutToBeRun);
		}

		protected override List<DocumentSupporterQuestion> GenerateQuestionsToAskUsersBeforeRunningDocumentCore(IStmMenuItem commandAboutToBeRun)
		{
			var result = base.GenerateQuestionsToAskUsersBeforeRunningDocumentCore(commandAboutToBeRun);
			if (commandAboutToBeRun.SU_MenuName.Contains("Entry Print") && JobDeclaration.ActiveEntryHeaders.Count > 0)
			{
				foreach (CusEntryHeader header in JobDeclaration.ActiveEntryHeaders)
				{
					if (header.TotalDeferredDutyFromCustoms.IsEmpty && header.TotalAmountPayable != TotalPayableAmount(header))
					{
						result.Add(new DocumentSupporterQuestion("Warning: Total payable imbalance", ImbalanceErrorMessage, QuestionType.Warning));
						break;
					}
				}
			}
			return result;
		}

		internal const string ImbalanceErrorMessage = @"An imbalance has been detected on the Entry Print.
This is most likely caused by a payment receipt being processed while you have the job open.
Please close the job, re-open, and try again.
Do you wish to continue and print the Entry Print now, despite this imbalance (not recommended)?";

		internal static ZDecimal TotalPayableAmount(CusEntryHeader header)
		{
			ZDecimal result = header.DutyAmount + header.CountervailingDuty + header.DumpingDuty +
					 +header.WETAmount + header.LCTAmount + header.AQISServicePaymentAmount + header.OtherCMRCharges;
			if (!AUCustomsEntryPrint.IsGSTDeferredForEntryPrint(AUCustomsEntryPrint.GetDeferredGST(header), header))
			{
				result += header.GSTAmount;
			}
			return result;
		}

		CusEntryHeaderCollection ATDPrintableEntryHeaders
		{
			get
			{
				if (fATDPrintableEntryHeaders == null)
				{
					fATDPrintableEntryHeaders = new CusEntryHeaderCollection(JobDeclaration, JobDeclaration.Factory);

					for (int i = 0; i < JobDeclaration.CustomsEntryHeaders.Count; i++)
					{
						if (JobDeclaration.CustomsEntryHeaders[i].IsAllowedToPrintATD)
						{
							fATDPrintableEntryHeaders.Add(JobDeclaration.CustomsEntryHeaders[i]);
						}
					}
				}

				return fATDPrintableEntryHeaders;
			}
		}
		CusEntryHeaderCollection fATDPrintableEntryHeaders;

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			if (dataContextValue.DataContext == DataContext.EFTPaymentAdvice || dataContextValue.DataContext == DataContext.ATD || dataContextValue.DataContext == DataContext.CusEntryHeader)
			{
				result = Res.GetString("01D97792-F8A8-46F0-A0B8-810376AA87D8", "Entry Header cannot be found. Please selecting Brokerage > Answer Declaration Questions to Merge the Declaration.");
			}
			return result;
		}
	}
}
