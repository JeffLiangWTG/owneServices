using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CN.DataTransfer.Universal
{
	public class CNEntryInstructionDataObjectReader : CustomsEntryInstructionDataObjectReader
	{
		public CNEntryInstructionDataObjectReader(EntryInstruction entryInstructionDataObject, IXmlImportLogger logger, UniversalDataObjectReaderHelper helper, UniversalObjectFactory factory, BaseJobDeclaration declaration) : base(entryInstructionDataObject, logger, helper, factory, declaration)
		{
		}

		protected override void FillCountrySpecificDetails(CusEntryInstruction targetBO)
		{
			base.FillCountrySpecificDetails(targetBO);

			if (helper is CNDataObjectReaderHelper cnHelper && cnHelper.IsSourceAndTargetCountrySame && dataObject.AddInfoCollection != null)
			{
				var entryInstructionPK = targetBO.PK;

				var customsMessageRemarksAddInfo = dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.EntryInstruction.CustomsMessageRemarks, logger);
				if (customsMessageRemarksAddInfo.HasValue)
				{
					var customsMessageRemarksNoteRow = GetColumnIndexer(helper.LoadOrCreateStmNoteForReaderUpdate(entryInstructionPK, CusEntryInstructionSchema.Constants.TableName, targetBO.IsInDatabase, PredefinedNoteTypes.Instance.CustomsMessageRemarks.Description));
					SetValue(customsMessageRemarksNoteRow, StmNoteSchema.ST_NoteText, customsMessageRemarksAddInfo.Value);
				}

				var billOfLadingAddInfo = dataObject.AddInfoCollection.GetZStringValue(Constants.AddInfoKeys.EntryInstruction.BillOfLading, logger);
				var billOfLadingDateAddInfo = dataObject.AddInfoCollection.GetZDateTimeValue(Constants.AddInfoKeys.EntryInstruction.BillOfLadingDate, logger);
				if (billOfLadingAddInfo.HasValue || billOfLadingDateAddInfo.HasValue)
				{
					var entryNumRow = GetColumnIndexer(cnHelper.LoadOrCreateEntryNumForEntryInstruction(entryInstructionPK, CusEntryNumberTypes.China.BillOfLading, targetBO.IsInDatabase));
					SetValue(entryNumRow, CusEntryNumSchema.CE_EntryNum, billOfLadingAddInfo);
					SetValue(entryNumRow, CusEntryNumSchema.CE_IssueDate, billOfLadingDateAddInfo);
				}
			}
		}
	}
}
