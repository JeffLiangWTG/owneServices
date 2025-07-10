using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusEntryHeaderDocumentSupporter : Customs.Business.CusEntryHeaderDocumentSupporter
	{
		public CusEntryHeaderDocumentSupporter(CusEntryHeader entryHeader)
			: base(entryHeader)
		{
		}

		protected CusEntryHeader CusEntryHeader
		{
			get { return (CusEntryHeader)BusinessObject; }
		}

		protected override DataContext[] GetSupportedDataContexts()
		{
			List<DataContext> result = new List<DataContext>(base.GetSupportedDataContexts());
			result.Add(DataContext.ATD);
			result.Add(DataContext.EFTPaymentAdvice);
			return result.ToArray();
		}

		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState dataState = null;
			if (commandAboutToBeRun.SU_MenuName.StartsWith("Authority To Deal"))
			{
				if (!CusEntryHeader.IsAllowedToPrintATD)
				{
					dataState = new DocumentSupporterDataState(ZBool.False, CusEntryHeader.ATDNotPrintableMessageText);
				}
			}

			if (dataState == null)
			{
				dataState = base.GetDataStateBeforeRun(commandAboutToBeRun);
			}
			if (dataState == null)
			{
				dataState = new DocumentSupporterDataState(ZBool.True, "");
			}

			return dataState;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			DocumentWrapper[] result = null;
			switch (dataContext)
			{
				case Core.Constants.DataContext.ATD:
					if (CusEntryHeader.IsAllowedToPrintATD)
					{
						result = new DocumentWrapper[] { DocumentWrapperFactory.CreateCustomsWrapper(Core.Constants.DataContext.CusEntryHeader, CusEntryHeader, Core.Constants.CountryCodes.Australia) };
					}
					else
					{
						result = System.Array.Empty<DocumentWrapper>();
					}

					break;

				case Core.Constants.DataContext.EFTPaymentAdvice:
					result = new DocumentWrapper[CusEntryHeader.CMRPAYRECandREFACCMessages.Length];
					int i = 0;
					for (; i < CusEntryHeader.CMRPAYRECandREFACCMessages.Length; i++)
					{
						if (CusEntryHeader.CMRPAYRECandREFACCMessages[i] is CMRPAYRECMessage)
						{
							result[i] = DocumentWrapperFactory.CreateCustomsWrapper(Core.Constants.DataContext.CMRPAYRECMessage, CusEntryHeader.CMRPAYRECandREFACCMessages[i], Core.Constants.CountryCodes.Australia);
						}
						else
						{
							result[i] = DocumentWrapperFactory.CreateCustomsWrapper(Core.Constants.DataContext.CMRREFACCMessage, CusEntryHeader.CMRPAYRECandREFACCMessages[i], Core.Constants.CountryCodes.Australia);
						}
					}
					break;

				default:
					result = base.GetDocumentWrappersInternal(dataContext, commandBeingRun);
					break;
			}
			return result;
		}

		public override ZString GetBODocDataProvidersNotFoundMessage(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			var result = base.GetBODocDataProvidersNotFoundMessage(dataContextValue, commandBeingRun);
			if (dataContextValue.DataContext == DataContext.EFTPaymentAdvice || dataContextValue.DataContext == DataContext.ATD)
			{
				result = Res.GetString("01D97792-F8A8-46F0-A0B8-810376AA87D8", "Entry Header cannot be found. Please selecting Brokerage > Answer Declaration Questions to Merge the Declaration.");
			}
			return result;
		}
	}
}
