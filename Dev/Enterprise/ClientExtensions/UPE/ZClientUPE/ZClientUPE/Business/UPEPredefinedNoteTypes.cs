using CargoWise.Definitions;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.UPE.Business
{
	public class UPEPredefinedNoteTypes : PredefinedNoteTypes
	{
		protected UPEPredefinedNoteTypes()
		{
		}

		#region Instance

		public new static UPEPredefinedNoteTypes Instance
		{
			get { return (UPEPredefinedNoteTypes)PredefinedNoteTypes.Instance; }
		}

		public static void RegisterThisSubTypeOverride()
		{
			PredefinedNoteTypes.OverrideNewDelegate(New);
		}

		static PredefinedNoteTypes New()
		{
			return new UPEPredefinedNoteTypes();
		}

		#endregion

		public UPEPredefinedNoteType Level1Record
		{
			get
			{
				if (fLevel1Record == null)
				{
					fLevel1Record = new UPEPredefinedNoteType("Level1Record", StmNoteVisibility.INT, true, true, true, 100000000);
				}
				return fLevel1Record;
			}
		}
		UPEPredefinedNoteType fLevel1Record;

		public UPEPredefinedNoteType CRNote
		{
			get
			{
				if (fCRNote == null)
				{
					fCRNote = new UPEPredefinedNoteType("CRNote", StmNoteVisibility.INT, false, true, true, 100000000);
				}
				return fCRNote;
			}
		}
		UPEPredefinedNoteType fCRNote;

		public UPEPredefinedNoteType FinanceNote
		{
			get
			{
				if (fFinanceNote == null)
				{
					fFinanceNote = new UPEPredefinedNoteType("FinanceNote", StmNoteVisibility.INT, false, true, true, 100000000);
				}
				return fFinanceNote;
			}
		}
		UPEPredefinedNoteType fFinanceNote;

		public UPEPredefinedNoteType DeclarationNote
		{
			get
			{
				if (fDeclarationNote == null)
				{
					fDeclarationNote = new UPEPredefinedNoteType("DeclarationNote", StmNoteVisibility.INT, false, true, true, 100000000);
				}
				return fDeclarationNote;
			}
		}
		UPEPredefinedNoteType fDeclarationNote;

		public UPEPredefinedNoteType PartPaymentNote
		{
			get
			{
				if (fPartPaymentNote == null)
				{
					fPartPaymentNote = new UPEPredefinedNoteType("PartPaymentNote", StmNoteVisibility.INT, false, true, true, 100000000);
				}
				return fPartPaymentNote;
			}
		}
		UPEPredefinedNoteType fPartPaymentNote;

		public UPEPredefinedNoteType RefundNote
		{
			get
			{
				if (fRefundNote == null)
				{
					fRefundNote = new UPEPredefinedNoteType("Refund", StmNoteVisibility.INT, false, true, true, 100000000);
				}
				return fRefundNote;
			}
		}
		UPEPredefinedNoteType fRefundNote;

		public UPEPredefinedNoteType ManualBillNote
		{
			get
			{
				if (fManualBillNote == null)
				{
					fManualBillNote = new UPEPredefinedNoteType("ManualBill", StmNoteVisibility.INT, false, true, true, 100000000);
				}
				return fManualBillNote;
			}
		}
		UPEPredefinedNoteType fManualBillNote;

		public UPEPredefinedNoteType MergeClearanceNote
		{
			get
			{
				if (mergeClearanceNote == null)
				{
					mergeClearanceNote = new UPEPredefinedNoteType("MergeClearance", StmNoteVisibility.INT, false, true, true, 100000000);
				}
				return mergeClearanceNote;
			}
		}
		UPEPredefinedNoteType mergeClearanceNote;

		public UPEPredefinedNoteType PreReleaseNotification
		{
			get { return (preReleaseNote) ?? (preReleaseNote = new UPEPredefinedNoteType("Pre-Release Notification", StmNoteVisibility.INT, false, true, true, 100000000)); }
		}
		UPEPredefinedNoteType preReleaseNote;
	}
}
