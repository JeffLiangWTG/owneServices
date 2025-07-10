using CargoWise.Types;
using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.CAJobDeclaration)]
	public class CAJobDeclaration : BaseJobDeclaration, ICAJobDeclarationShareProperty
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();

			if (IsImport)
			{
				noteTypes.Add(PredefinedNoteTypes.Instance.CustomsMessageToPrintOnB3);
				noteTypes.Add(PredefinedNoteTypes.Instance.CustomsMessageToPrintOnCAD);
				noteTypes.Add(PredefinedNoteTypes.Instance.LeadSheetComments);
				noteTypes.Add(PredefinedNoteTypes.Instance.ManualSubmission);
				noteTypes.Add(PredefinedNoteTypes.Instance.ManualRelease);
				noteTypes.Add(PredefinedNoteTypes.Instance.ManualCancel);
			}

			return noteTypes;
		}

		public ZBool IsImport { get; set; }
	}
}
