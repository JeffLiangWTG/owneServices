using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.CommonShipment)]
	public class CommonShipment : EnterpriseBusinessObject, ICommonShipmentLinkedMetadata
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.CertificateOfOriginNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.ClientVisibleJobNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryInstructionsNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.PickupInstructionsNote);
			noteTypes.Add(PredefinedNoteTypes.Instance.DetailedGoodsDescription);
			noteTypes.Add(PredefinedNoteTypes.Instance.HandlingInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.MarksAndNumbers);
			noteTypes.Add(PredefinedNoteTypes.Instance.SpecialInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.BookingNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryOrderReceiptNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.CartageHistoryNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.ManifestGoodsDescription);
			noteTypes.Add(PredefinedNoteTypes.Instance.InternalWorkNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation);
			noteTypes.Add(PredefinedNoteTypes.Instance.PaymentHandlingInstructions);
			noteTypes.Add(PredefinedNoteTypes.Instance.AWBRatelineOvertypedNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.AutoRatingAuditLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.FaxEmailTransmissionLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.OutturnNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks);
			noteTypes.Add(PredefinedNoteTypes.Instance.UnmatchedOrgDetails);
			noteTypes.Add(PredefinedNoteTypes.Instance.CustomsInstructionNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.UnrecognisedAdditionalReferenceTypes);
			noteTypes.Add(PredefinedNoteTypes.Instance.CustomsManualStatus);
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRules);
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRulesInternal);
			noteTypes.Add(PredefinedNoteTypes.Instance.CountryRulesValidation);

			if (HasDeclarations)
			{
				foreach (var declaration in DeclarationsForNoteTypes)
				{
					noteTypes.Add(declaration.NoteTypes);
				}
			}

			if (MetadataExtensions != null)
			{
				foreach (var extension in MetadataExtensions)
				{
					noteTypes.Add(extension.NoteTypes);
				}
			}

			noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryDueDateCalculationLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.EmissionsCalculationLog);

			return noteTypes;
		}

		bool HasDeclarations { get { return DeclarationsForNoteTypes != null; } }

		public IMetadata[] DeclarationsForNoteTypes { get; set; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Metadata architecture requires array")]
		public IMetadata[] MetadataExtensions { get; set; }
	}
}
