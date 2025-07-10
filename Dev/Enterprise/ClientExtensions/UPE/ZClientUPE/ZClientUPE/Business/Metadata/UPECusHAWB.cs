using Enterprise.Client.UPE.Business;
using Enterprise.Metadata.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Metadata
{
	public class UPECusHAWB : AUCusHAWB
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.DeliveryInstructionsNote);
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.Level1Record);
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.CRNote);

			// Finance Level Notes; it needs to be here coz of backward ref. 
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.FinanceNote);
			noteTypes.Add(UPEPredefinedNoteTypes.Instance.PartPaymentNote);

			noteTypes.Add(UPEPredefinedNoteTypes.Instance.MergeClearanceNote);
			return noteTypes;
		}
	}
}
