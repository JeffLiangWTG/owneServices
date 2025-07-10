using CargoWise.Types;
using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.AUJobDeclaration)]
	public class AUJobDeclaration : BaseJobDeclaration, IAUJobDeclarationShareProperty
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();

			if (IsQuarantine)
			{
				noteTypes.Add(PredefinedNoteTypes.Instance.EXDOCLetterOfCredit);
				noteTypes.Add(PredefinedNoteTypes.Instance.EXDOCAdditionalInformation);
				noteTypes.Add(PredefinedNoteTypes.Instance.EXDOCNotifyText);
				noteTypes.Add(PredefinedNoteTypes.Instance.EXDOCAmendmentReason);
				noteTypes.Add(PredefinedNoteTypes.Instance.NEXDOCCancellationReason);
			}

			return noteTypes;
		}

		protected override void AddMarksAndNumbersNoteType(NoteTypeCollection noteTypes)
		{
			noteTypes.Add(PredefinedNoteTypes.Instance.MarksAndNumbers);
		}

		public bool IsQuarantine { get; set; }
		public bool IsImportCMR { get; set; }
		public ZBool IsExWarehouse { get; set; }
		public bool IsTransportModeOther { get; set; }
	}
}
