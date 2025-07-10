using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business
{
	[Metadata(MetadataContext.ForwardingShipment)]
	public class ForwardingShipment : EnterpriseBusinessObject, IForwardingShipmentShareProperty
	{
		protected override NoteTypeCollection InitializeNoteTypes()
		{
			var noteTypes = base.InitializeNoteTypes();
			noteTypes.Add(PredefinedNoteTypes.Instance.AgentNotes);
			noteTypes.Add(PredefinedNoteTypes.Instance.HouseBillGoodsDetailsOverride);
			noteTypes.Add(PredefinedNoteTypes.Instance.HouseBillChargesOverride);
			noteTypes.Add(PredefinedNoteTypes.Instance.HouseBillFollowOnOverride);

			if (EnableBoleroEHBLIntegration)
			{
				noteTypes.Add(PredefinedNoteTypes.Instance.OriginalBillNotes);
			}

			noteTypes.Add(PredefinedNoteTypes.Instance.PortMessageRemarks);
			noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryDueDateCalculationLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.EmissionsCalculationLog);

			return noteTypes;
		}

		public bool EnableBoleroEHBLIntegration { get; set; }
	}
}
