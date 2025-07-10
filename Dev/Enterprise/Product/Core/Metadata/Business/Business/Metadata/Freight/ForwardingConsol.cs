using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.ForwardingConsol)]
	public class ForwardingConsol : CommonConsol, IForwardingConsolShareProperty
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.AgentNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.CarrierBookingRequest);

			if (EnableBoleroEBLIntegration)
			{
				noteTypes.Add(PredefinedNoteTypes.Instance.OriginalBillNotes);
			}

			noteTypes.Add(PredefinedNoteTypes.Instance.PortMessageRemarks);
			noteTypes.Add(PredefinedNoteTypes.Instance.SpotBookingTermsAndFees);
			return noteTypes;
		}

		public bool EnableBoleroEBLIntegration { get; set; }
	}
}
