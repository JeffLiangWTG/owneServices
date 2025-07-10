using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CADCorrectionMessageSendingAction : CusSupportingInfo
	{
		public CADCorrectionMessageSendingAction(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CADCorrectionMessageSendingActionValidation Validation => (CADCorrectionMessageSendingActionValidation)base.Validation;

		public new CADCorrectionMessageSendingActionLookups Lookups => (CADCorrectionMessageSendingActionLookups)base.Lookups;

		protected override CusSupportingInfoValidation GetNewValidation() => new CADCorrectionMessageSendingActionValidation(this);

		protected override CusSupportingInfoLookups GetNewLookups() => new CADCorrectionMessageSendingActionLookups(this);

		protected override ZString HumanReadableNameCore => Res.GetString("B729DAF1-BD13-4C8C-A7A8-EDB3C16A30C8", "CAD Correction Message Sending Action");

		public new CADCorrectionMessageSendingActionWrapper Parent => (CADCorrectionMessageSendingActionWrapper)base.Parent;

		internal CusEntryLine MatchedEntryLine;

		IEnumerable<CusEntryLine> SortedEntryLines
		{
			get
			{
				if (fSortedEntryLines == null && Parent != null)
				{
					var comparer = new B3LineNumberAssigner(Parent.CADEntryHeader).GetEntryLineComparerForLineReordering();
					fSortedEntryLines = Parent.CADEntryHeader.MergedLines.OfType<CusEntryLine>().OrderBy(x => x, comparer).ToList();
				}
				return fSortedEntryLines;
			}
		}
		IEnumerable<CusEntryLine> fSortedEntryLines;

		[ResourceStringData("NPBO:Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction|EntryLineSequence", ShortCaption = "LNO", MediumCaption = "Entry LNO", Caption = "Entry Line Number")]
		public ZShort EntryLineSequence
		{
			get
			{
				return fEntryLineSequence;
			}
			set
			{
				var isChanging = fEntryLineSequence != value;
				if (isChanging && SetNonPersistentPropertyValue(EntryLineSequenceInfo, ref fEntryLineSequence, value))
				{
					SetParentIDAndTableCode();

					if (MatchedEntryLine?.InvoiceLines.OfType<JobComInvoiceLine>().FirstOrDefault() is JobComInvoiceLine invoiceLine)
					{
						InvoiceSequence = invoiceLine.InvoiceHeaderSequence;
						InvoiceLineSequence = invoiceLine.JI_LineNo;
					}
					else
					{
						InvoiceSequence = InvoiceLineSequence = ZShort.Zero;
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateEntryLineSequence();
						Validation.ValidateInvoiceSequence();
						Validation.ValidateInvoiceLineSequence();
					}
				}
			}
		}
		ZShort fEntryLineSequence;

		public ZPropertyInfo EntryLineSequenceInfo => GetZPropertyInfo(nameof(EntryLineSequence));

		[ResourceStringData("NPBO:Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction|InvoiceSequence", ShortCaption = "Inv. Seq #", Caption = "Invoice Seq #")]
		public ZShort InvoiceSequence
		{
			get { return fInvoiceSequence; }
			set
			{
				var isChanging = fInvoiceSequence != value;
				if (isChanging && SetNonPersistentPropertyValue(InvoiceSequenceInfo, ref fInvoiceSequence, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateInvoiceSequence();
						Validation.ValidateInvoiceLineSequence();
					}
				}
			}
		}
		ZShort fInvoiceSequence;

		public ZPropertyInfo InvoiceSequenceInfo => GetZPropertyInfo(nameof(InvoiceSequence));

		[ResourceStringData("NPBO:Enterprise.Customs.CA.Business.CADCorrectionMessageSendingAction|InvoiceLineSequence", ShortCaption = "LNO", Caption = "Inv. Line #")]
		public ZShort InvoiceLineSequence
		{
			get { return fInvoiceLineSequence; }
			set
			{
				var isChanging = fInvoiceLineSequence != value;
				if (isChanging && SetNonPersistentPropertyValue(InvoiceLineSequenceInfo, ref fInvoiceLineSequence, value))
				{
					if (!IsValidationSuspended)
					{
						Validation.ValidateInvoiceLineSequence();
						Validation.ValidateInvoiceSequence();
					}
				}
			}
		}
		ZShort fInvoiceLineSequence;

		public ZPropertyInfo InvoiceLineSequenceInfo => GetZPropertyInfo(nameof(InvoiceLineSequence));

		#region Calculated Properties

		public ZString ReasonCodeDescription
		{
			get
			{
				if (reasonCodeDescriptionDescriptionCached == null)
				{
					reasonCodeDescriptionDescriptionCached = new CachedProperty<ZString>(Factory, () =>
					{
						var result = ZString.Empty;
						var code = CSI_Code;
						if (!code.IsEmpty)
						{
							var cusCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, code, Core.Constants.CountryCodes.Canada, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CARMChangeReasonCode, ZDateTime.Today);
							if (cusCode != null)
							{
								result = cusCode.ZZD_Description.ToUpper();
							}
						}
						return result;
					});
				}

				return reasonCodeDescriptionDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> reasonCodeDescriptionDescriptionCached;

		public ZString AppealsProgramCodeDescription
		{
			get
			{
				if (appealsProgramCodeDescriptionCached == null)
				{
					appealsProgramCodeDescriptionCached = new CachedProperty<ZString>(Factory, () => Lookups.CARMAppealsProgramCodeList.GetDescriptionFromCode(CSI_SubType));
				}

				return appealsProgramCodeDescriptionCached.Value;
			}
		}
		CachedProperty<ZString> appealsProgramCodeDescriptionCached;

		#endregion

		void SetParentIDAndTableCode()
		{
			if (!EntryLineSequence.IsEmpty && Parent != null)
			{
				MatchedEntryLine = EntryLineSequence > 0 && EntryLineSequence <= SortedEntryLines.Count() ? SortedEntryLines.ElementAt(EntryLineSequence - 1) : null;

				if (MatchedEntryLine != null)
				{
					CSI_ParentID = MatchedEntryLine.PK;
					CSI_ParentTableCode = CusEntryLineSchema.Constants.Prefix;
				}
				else
				{
					CSI_ParentID = FakeEntryLinePK;
				}
			}
		}

		[ResourceStringData("CA.CADCorrectionMessageSendingAction.CSI_LineNo", Caption = "Version")]
		public override ZInt CSI_LineNo { get => base.CSI_LineNo; set => base.CSI_LineNo = value; }

		public override bool IsSavedByFactory
		{
			get { return IsDeleted || CSI_ParentID != FakeEntryLinePK; }
		}

		public static ZGuid FakeEntryLinePK = new ZGuid("5EE4D87A-E01B-49C8-9A5F-E09CCD4D7852");

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(CADCorrectionMessageSendingActionLookups.CARMChangeReasonCodeList))]
		[ResourceStringData("CA.CADCorrectionMessageSendingAction.CSI_Code", Caption = "Reason Code")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set => base.CSI_Code = value;
		}

		[MaxLength(1)]
		[List(nameof(Lookups) + "." + nameof(CADCorrectionMessageSendingActionLookups.CARMAppealsProgramCodeList))]
		[ResourceStringData("CA.CADCorrectionMessageSendingAction.CSI_SubType", Caption = "Appeals Program Code")]
		public override ZString CSI_SubType
		{
			get => base.CSI_SubType;
			set => base.CSI_SubType = value;
		}

		[MaxLength(255)]
		[ResourceStringData("CA.CADCorrectionMessageSendingAction.CSI_Description", Caption = "Supporting Remarks")]
		public override ZString CSI_Description
		{
			get => base.CSI_Description;
			set => base.CSI_Description = value;
		}

		protected override bool SupportsCloneCore()
		{
			return false;
		}

		protected override void PopulateDataModelIfNeededCore()
		{
			CSI_DataModel = Core.Constants.CountryCodes.Canada;
		}
	}
}
