using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class CusReconEntry : AutoKRCusReconEntry, Integration.Customs.KR.ICusReconEntry, IShortSequenceNumberLine
	{
		public CusReconEntry(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoKRCusReconEntry.Schema
		{
			public const int CRE_OriginalEntryNumber_MaxLength = 15;
			public const int Amendment5WNVersionNumberMaxLength = 2;
		}

		[BusinessObjectTestExclude]
		[MaxLength(Schema.Amendment5WNVersionNumberMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusReconEntryLookups.Amendment5WNVersionNumbers))]
		[ResourceStringData("3988A59C-DAD0-4CAB-ADA7-2C3F89634571", Caption = "5WN Amend Seq No.")]
		public ZString Amendment5WNVersionNumber
		{
			get => CRE_Amendment5WNVersionNumber.IsEmpty ? ZString.Empty : CRE_Amendment5WNVersionNumber.ToString();
			set
			{
				if (string.IsNullOrWhiteSpace(value))
				{
					CRE_Amendment5WNVersionNumber = ZShort.Zero;
				}
				if (ZShort.TryParse(value, out ZShort parsed))
				{
					CRE_Amendment5WNVersionNumber = parsed;
					if (!IsValidationSuspended)
					{
						var validation = (CusReconEntryValidation)Validation;
						validation.ValidateAmendment5WNVersionNumber();
					}
				}
				Amendment5WNVersionNumberInfo.RefreshBinding();
			}
		}
		public ZPropertyInfo Amendment5WNVersionNumberInfo => GetZPropertyInfo(nameof(Amendment5WNVersionNumber));

		public override ZShort CRE_Amendment5WNVersionNumber
		{
			get => base.CRE_Amendment5WNVersionNumber;
			set
			{
				var oldValue = base.CRE_Amendment5WNVersionNumber;
				base.CRE_Amendment5WNVersionNumber = value;
				if (oldValue != value)
				{
					var line = (CusReconEntryLine)CusReconEntryLines.FirstOrDefault();
					if (line != null)
					{
						line.DefaultAmountsToRefund();
					}
				}
			}
		}

		[ReadOnlyMember(nameof(HasImportDetails))]
		[MaxLength(Schema.CRE_CustomsBillNumberMaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusReconEntryLookups.CustomsBillsViewCollection))]
		[ResourceStringData("3CBACB85-2996-456D-836D-DCBDE82C685C", Caption = "Customs Disbursement Bill #")]
		public override ZString CRE_CustomsBillNumber { get => base.CRE_CustomsBillNumber; set => base.CRE_CustomsBillNumber = value; }

		public void SaveSnapshotByCustomsBillNumber(KREntryCustomsBillsView view)
		{
			//It will be fixed in WI00888953
			CRE_CustomsBillNumber = view.KEB_CustomsDisbursementBillNumber.Length == 19 ? view.KEB_CustomsDisbursementBillNumber.Substring(4, 15) : view.KEB_CustomsDisbursementBillNumber.Substring(0, 15);
			CRE_OriginalEntryNumber = view.KEB_ImportEntryNum.Substring(0, 15);
			CRE_EntryDate = (ZDate)view.KEB_ImportIssueDate;
			CRE_EntryType = KRJobMessageTypeList.Codes.Import;
			CRE_GB_Branch = view.KEB_BranchPK;
			using var stream = KRXmlObjectSerializer.Serialize(view.GetImportEntryOrEntryLineSerializable());
			var reader = new Enterprise.Messaging.Business.TextReaderSource(stream);
			var snapshot = FirstSnapShot ?? CusReconSnapshots.AddNew();
			snapshot.CRS_SnapshotXml = reader.GetReader().ReadToEnd();

			if (FirstSnapShot.ImportEntryOrEntryLine.RefundAmounts != null && FirstSnapShot.ImportEntryOrEntryLine.RefundAmounts.Count == 1)
			{
				CRE_Amendment5WNVersionNumber = FirstSnapShot.ImportEntryOrEntryLine.RefundAmounts[0].VersionNumber;
			}
		}

		public ZBool HasImportDetails => FirstSnapShot != null;
		[ReadOnlyMember(nameof(HasImportDetails))]
		public override ZDate CRE_EntryDate { get => base.CRE_EntryDate; set => base.CRE_EntryDate = value; }
		[ReadOnlyMember(nameof(HasImportDetails))]
		public override ZString CRE_EntryType { get => base.CRE_EntryType; set => base.CRE_EntryType = value; }
		[ReadOnlyMember(nameof(HasImportDetails))]
		public override ZGuid CRE_GB_Branch { get => base.CRE_GB_Branch; set => base.CRE_GB_Branch = value; }
		[ReadOnlyMember(nameof(HasImportDetails))]
		[MaxLength(Schema.CRE_OriginalEntryNumber_MaxLength)]
		[List(nameof(Lookups) + "." + nameof(CusReconEntryLookups.ImportEntryNumbers))]
		[ResourceStringData("36D4549E-32E2-48C3-8C6D-80BB4DD8A337", Caption = "IMP Entry Number")]
		public override ZString CRE_OriginalEntryNumber
		{
			get => base.CRE_OriginalEntryNumber;
			set
			{
				//TODO:It's temp codes to save. It should delete when DB constraint is changed.
				var oldValue = base.CRE_OriginalEntryNumber;
				base.CRE_OriginalEntryNumber = value;
				if (oldValue != value)
				{
					var line = (CusReconEntryLine)CusReconEntryLines.FirstOrDefault();
					if (line != null && value.IsEmpty)
					{
						line.CRL_OriginalEntryLineNumber = ZShort.Zero;
					}
					var entryView = Lookups.ImportEntryNumbers.FirstOrDefault(x => x.KEH_EntryNum == value);
					if (entryView != null)
					{
						CRE_CH_OriginalEntry = entryView.PK;
						CRE_OA_DeclarantAddress = ReconDeclaration.CRD_OA_DeclarantAddress;
					}
				}
			}
		}

		[ReadOnly(true)]
		[ResourceStringData("1BD0220B-8628-438A-AB69-BBA7DA1E7905", Caption = "Seq #")]
		public override ZShort CRE_SequenceNumber { get => base.CRE_SequenceNumber; set => base.CRE_SequenceNumber = value; }

		protected override void BeforeSuccessfulDelete()
		{
			if (ReconDeclaration != null)
			{
				ReconDeclaration.RecalculateEntrySequenceNumberOnDeleted(this);
			}
			base.BeforeSuccessfulDelete();
		}

		public CusReconSnapshot FirstSnapShot
		{
			get
			{
				if (firstSnapShot == null)
				{
					firstSnapShot = CusReconSnapshots.Cast<CusReconSnapshot>().FirstOrDefault();
				}
				return firstSnapShot;
			}
		}
		CusReconSnapshot firstSnapShot;

		public new CusReconDeclaration ReconDeclaration => (CusReconDeclaration)base.ReconDeclaration;
		public new CusReconEntryLookups Lookups => (CusReconEntryLookups)base.Lookups;

		ZShort ISequenceNumberLine<ZShort>.SequenceNumber
		{
			get => CRE_SequenceNumber;
			set
			{
				CRE_SequenceNumber = value;
			}
		}

		ZGuid ISequenceNumberLine.FKToHeader => ReconDeclaration.PK;

		protected override Customs.Business.CusReconBase.CusReconEntryLookups GetNewLookups() => new CusReconEntryLookups(this);
		protected override Customs.Business.CusReconBase.CusReconEntryValidation GetNewValidation() => new CusReconEntryValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CRE_EntryType = KRJobMessageTypeList.Codes.Import;
		}
	}
}
