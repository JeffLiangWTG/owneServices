using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.QuotedBooking)]
	[WTG.StaticAnalysis.Annotation.CodeAlive("Used for making Country Rules Validation notes default on QuotedBookings")]
	public class QuotedBooking : EnterpriseBusinessObject
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			//Client-Visible
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRules);
			//Internal
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRulesInternal);
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRulesValidation);
			noteTypes.Add(PredefinedNoteTypes.Instance.SpotBookingTermsAndFees);
			noteTypes.Add(PredefinedNoteTypes.Instance.SpotBookingPenaltiesFees);
			noteTypes.Add(PredefinedNoteTypes.Instance.SpotBookingRouting);
			noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryDueDateCalculationLog);
			return noteTypes;
		}
	}
}
