#if DEBUG

using Enterprise.Metadata.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Metadata.Business.Tests
{
	public class CommonShipmentTest : EnterpriseBusinessObjectTest
	{
#region TestNoteTypes

		protected internal override NoteTypeCollection ExpectedNoteTypes()
		{
			var noteTypes = base.ExpectedNoteTypes();
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
				var declarationNoteTypes = new BaseJobDeclarationTest().ExpectedNoteTypes();
				noteTypes.Add(declarationNoteTypes);
			}

			if (IsCFSRegistered)
			{
				noteTypes.Add(PredefinedNoteTypes.Instance.GatePassNotes);
			}

			if (IsForwardingRegistered)
			{
				noteTypes.Add(PredefinedNoteTypes.Instance.AgentNotes);
				noteTypes.Add(PredefinedNoteTypes.Instance.HouseBillGoodsDetailsOverride);
				noteTypes.Add(PredefinedNoteTypes.Instance.HouseBillChargesOverride);
				noteTypes.Add(PredefinedNoteTypes.Instance.HouseBillFollowOnOverride);
				noteTypes.Add(PredefinedNoteTypes.Instance.PortMessageRemarks);
			}

			noteTypes.Add(PredefinedNoteTypes.Instance.DeliveryDueDateCalculationLog);
			noteTypes.Add(PredefinedNoteTypes.Instance.EmissionsCalculationLog);

			return noteTypes;
		}

		bool HasDeclarations { get; set; }

		bool IsCFSRegistered { get; set; }

		bool IsForwardingRegistered { get; set; }

		public void TestNoteTypes_Declarations()
		{
			HasDeclarations = true;
			var expected = ExpectedNoteTypes();

			var metadata = NewMetadata;
			((ICommonShipmentLinkedMetadata)metadata).DeclarationsForNoteTypes = new BaseJobDeclaration[] { new BaseJobDeclaration() };
			var actual = metadata.NoteTypes;
			new TestHelper().AssertIListEqualsByElements(expected, actual);
		}

		public void TestNoteTypes_GatePass()
		{
			IsCFSRegistered = true;
			var expected = ExpectedNoteTypes();

			var metadata = NewMetadata;
			((ICommonShipmentLinkedMetadata)metadata).MetadataExtensions = new IMetadata[] { new GatePassShipment() };
			var actual = metadata.NoteTypes;
			new TestHelper().AssertIListEqualsByElements(expected, actual);
		}

		public void TestNoteTypes_ForwardingShipment()
		{
			IsForwardingRegistered = true;
			var expected = ExpectedNoteTypes();

			var metadata = NewMetadata;
			((ICommonShipmentLinkedMetadata)metadata).MetadataExtensions = new IMetadata[] { new ForwardingShipment() };
			var actual = metadata.NoteTypes;
			new TestHelper().AssertIListEqualsByElements(expected, actual);
		}

#endregion

#region Implementation

		protected override IMetadata NewMetadata
		{
			get { return new CommonShipment(); }
		}

#endregion
	}
}

#endif
