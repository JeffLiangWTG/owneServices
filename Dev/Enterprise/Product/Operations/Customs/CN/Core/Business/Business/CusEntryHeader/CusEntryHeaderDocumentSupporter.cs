using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CN.Business
{
	public class CusEntryHeaderDocumentSupporter : Customs.Business.CusEntryHeaderDocumentSupporter
	{
		public CusEntryHeaderDocumentSupporter(CusEntryHeader header) : base(header) { }

		internal const string CustomsDeclarationDocument = ".CustomsDeclarationDocument";

		protected new CusEntryHeader EntryHeader => (CusEntryHeader)BusinessObject;

		protected override List<DataContextValue> GetSupportedBODataSources()
		{
			var result = base.GetSupportedBODataSources();
			result.Add(new DataContextValue(CustomsDeclarationDocument));
			return result;
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return new DocumentWrapper[] { new CusDataHeaderDocumentWrapper(EntryHeader) };
		}

		protected override IBODocDataProvider[] GetBODocDataProvidersInternal(DataContextValue dataContextValue, IStmMenuItem commandBeingRun)
		{
			IBODocDataProvider[] result = null;
			if (dataContextValue.FullDataContext == CustomsDeclarationDocument)
			{
				result = new IBODocDataProvider[] { BODocDataProvider.Get(new CusDataHeaderDocumentWrapper(EntryHeader)) };
			}
			return result ?? System.Array.Empty<IBODocDataProvider>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "For logic not to shown to user.")]
		public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
		{
			DocumentSupporterDataState result;

			var baseResult = base.GetDataStateBeforeRun(commandAboutToBeRun);
			if (!baseResult.IsValid)
			{
				result = baseResult;
			}
			else
			{
				ZString errorMessage = ZString.Empty;

				var menuItemText = commandAboutToBeRun.SU_MenuName;
				if (menuItemText == "Customs Release Notice")
				{
					var instruction = EntryHeader.EntryInstruction;
					if (instruction == null || instruction.CEI_DocumentSubmissionType != EntryDocumentSubmissionTypes.Codes.PaperlessForCustoms)
					{
						errorMessage = GetEntryIsPaperedRes(menuItemText);
					}
					else if (EntryHeader.CH_EntryReleaseDate.IsEmpty)
					{
						errorMessage = GetEntryNotReleasedRes(menuItemText);
					}
				}

				if (!errorMessage.IsEmpty)
				{
					result = new DocumentSupporterDataState(false, errorMessage);
				}
				else
				{
					result = baseResult;
				}
			}

			return result;
		}

		string GetEntryIsPaperedRes(string menuItemText)
		{
			return Res.GetString("EEBC1E75-08AA-45E7-AD52-DE7623CDB869", "Cannot print {0} as the entry is not in paperless clearance mode.", menuItemText);
		}

		string GetEntryNotReleasedRes(string menuItemText)
		{
			return Res.GetString("7E4EB1D9-702E-4EB8-B7ED-872764FDBAB7", "Cannot print {0} as the entry has not been released.", menuItemText);
		}
	}
}
