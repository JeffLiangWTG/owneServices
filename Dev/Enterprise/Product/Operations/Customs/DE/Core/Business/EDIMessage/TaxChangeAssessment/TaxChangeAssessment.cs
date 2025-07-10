using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.DE.Business
{
	[CodeProperty(Schema.ReferenceNumber)]
	[DescriptionProperty(Schema.ReferenceNumber)]
	public class TaxChangeAssessment : EDIMessage, IRelatedJob
	{
		public TaxChangeAssessment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : EDIMessage.Schema
		{
			public const string Type = nameof(TaxChangeAssessment.Type);
			public const string ReferenceNumber = nameof(TaxChangeAssessment.ReferenceNumber);
			public const string LocalReferenceNumber = nameof(TaxChangeAssessment.LocalReferenceNumber);
			public const string IssueDate = nameof(TaxChangeAssessment.IssueDate);
			public const string MaturityDate = nameof(TaxChangeAssessment.MaturityDate);
			public const string EntryStatus = nameof(TaxChangeAssessment.EntryStatus);
			public const string BranchCode = nameof(TaxChangeAssessment.BranchCode);

			public const string TaxChangeAssessmentType = "TaxChangeAssessmentType";
		}

		[ResourceStringData("7A655D8E-AEC3-4559-82A1-2A7345BD8A71", Caption = "Type")]
		public ZString Type
		{
			get
			{
				var noteText = Notes.GetNoteText(Schema.TaxChangeAssessmentType);
				return Factory.GetCached(ref assessmentTypeList, () => new ImportTaxChangeAssessmentTypeList()).GetDescriptionFromCode(noteText) ?? noteText;
			}
		}

		CachedProperty<ImportTaxChangeAssessmentTypeList> assessmentTypeList;

		public ZPropertyInfo TypeInfo => GetZPropertyInfo(Schema.Type);

		[ResourceStringData("4BEA7C6E-044E-4CAF-9759-9A3CDC145CC1", Caption = "Reference Number", ShortCaption = "Reference")]
		public ZString ReferenceNumber => MRNCusEntryNumberWrapper.EntryNumber;

		public ZPropertyInfo ReferenceNumberInfo => GetZPropertyInfo(Schema.ReferenceNumber);

		[ResourceStringData("CB47FBCE-BB4F-4182-BC58-ACCA3F88D791", Caption = "LRN")]
		public ZString LocalReferenceNumber => Notes.GetNoteText(Schema.LocalReferenceNumber);

		public ZPropertyInfo LocalReferenceNumberInfo => GetZPropertyInfo(Schema.LocalReferenceNumber);

		[ResourceStringData("4AF0C6D6-8FC3-45D2-844D-6EC39C849C3F", Caption = "Issue Date")]
		public ZDateTime IssueDate => MRNCusEntryNumberWrapper.IssueDate;

		public ZPropertyInfo IssueDateInfo => GetZPropertyInfo(Schema.IssueDate);

		[ResourceStringData("B7F2050E-7338-4C6F-B849-8E943E7CAF0D", Caption = "Maturity Date")]
		public ZDateTime MaturityDate => MRNCusEntryNumberWrapper.ExpiryDate;

		public ZPropertyInfo MaturityDateInfo => GetZPropertyInfo(Schema.MaturityDate);

		[ResourceStringData("425F00BC-3113-4354-8EBB-E9E3B9B4FB36", Caption = "Branch")]
		public ZString BranchCode => Branch.GB_Code;

		[ResourceStringData("2B2BD603-A4B6-4704-97C2-C53B7421A214", Caption = "Status")]
		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(TaxChangeAssessmentLookups.EntryStatusList))]
		public ZString EntryStatus
		{
			get => MRNCusEntryNumberWrapper.EntryStatus;
			set
			{
				var oldValue = EntryStatus;
				if (value != oldValue)
				{
					MRNCusEntryNumberWrapper.SetEntryStatus(value, EntryStatusInfo);
					if (!IsValidationSuspended)
					{
						Validation.ValidateEntryStatus();
					}
				}
			}
		}

		public ZPropertyInfo EntryStatusInfo => GetZPropertyInfo(Schema.EntryStatus);

		public new TaxChangeAssessmentLookups Lookups => (TaxChangeAssessmentLookups)base.Lookups;

		public new TaxChangeAssessmentValidation Validation => (TaxChangeAssessmentValidation)base.Validation;

		protected override EDIMessageLookups GetNewLookups() => new TaxChangeAssessmentLookups(this);

		protected override EDIMessageValidation GetNewValidation() => new TaxChangeAssessmentValidation(this);

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			EM_ApplicationCode = EDIInterchange.ApplicationCodes.DECustomsAtlasSystem;
			EM_MessageType = EDIMessageTypeList.Codes.Import;
			EM_MessageSubType = ImportMessageSubTypeList.Codes.SubsequentRaiseRefundOrAbatement;
			EM_ReceiveTransmit = EDIInterchange.Direction.Receive;
		}

		protected override string GetMessageReferenceNumber() => DEEDIMessageSharedHelpers.GetDEMessageReferenceNumber(Factory);

		#region IRelatedJob Members

		ZString IRelatedJob.JobNumber => ReferenceNumber;

		ZString IRelatedJob.JobDescription => ZString.Empty;

		ZString IRelatedJob.JobStatus => ZString.Empty;

		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.DE.TaxChangeAssessment;

		Guid IControllerIDProvider.BusinessObjectPK => PK.ToGuid();

		#endregion

		CusEntryNumberWrapper MRNCusEntryNumberWrapper => mrnCusEntryNumberWrapper ?? (mrnCusEntryNumberWrapper = new CusEntryNumberWrapper(this, CusEntryNumberTypes.Standard.MovementReferenceNumber));
		CusEntryNumberWrapper mrnCusEntryNumberWrapper;
	}
}
