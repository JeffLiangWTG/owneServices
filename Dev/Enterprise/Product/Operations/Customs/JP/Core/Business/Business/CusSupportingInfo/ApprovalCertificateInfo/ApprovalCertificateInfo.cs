using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.JP;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Business
{
	public class ApprovalCertificateInfo : CusSupportingInfo
	{
		public ApprovalCertificateInfo(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public new const int CSI_CodeMaxLength = 4;
			public new const int CSI_ReferenceNumberMaxLength = 20;
		}

		[List(nameof(Lookups) + "." + nameof(ApprovalCertificateInfoLookups.ApprovalCertificateType))]
		[MaxLength(Schema.CSI_CodeMaxLength)]
		[ResourceStringData("C7E0BFF1-03CD-4A52-A8F5-DF2D0C2210B5", Caption = "ID")]
		public override ZString CSI_Code
		{
			get => base.CSI_Code;
			set
			{
				var oldValue = CSI_Code;
				base.CSI_Code = value;
				if (!IsCopying && oldValue != value)
				{
					var entryInstruction = EntryInstruction;
					if (entryInstruction != null && !entryInstruction.IsValidationSuspended)
					{
						if (entryInstruction.Validation is CusEntryInstructionValidation validation)
						{
							var certificateNumbers = Lookups.ApprovalCertificateNumberList;
							if (certificateNumbers.Count == 1 && !certificateNumbers.ContainsCode(ApprovalCertificateInfoCodes.KIJI))
							{
								CSI_ReferenceNumber = certificateNumbers[0].Code;
							}
							validation.ValidateApprovalCertificateInfos();
							validation.ValidateCEI_Style();
						}

						var approvalCertificateInfos = entryInstruction.ApprovalCertificateInfos.ToArray();
						var previousInfo = approvalCertificateInfos.ElementInFrontOf(this) as ApprovalCertificateInfo;
						if (previousInfo != null && !previousInfo.IsValidationSuspended)
						{
							previousInfo.Validation.ValidateCSI_Code();
						}
						var nextInfo = approvalCertificateInfos.ElementAfter(this) as ApprovalCertificateInfo;
						if (nextInfo != null && !nextInfo.IsValidationSuspended)
						{
							nextInfo.Validation.ValidateCSI_Code();
						}
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(ApprovalCertificateInfoLookups.ApprovalCertificateNumberList))]
		[MaxLength(Schema.CSI_ReferenceNumberMaxLength)]
		[ResourceStringData("DAF7B18A-19B1-4B27-87C0-E3BDEAEB7642", Caption = "Number")]
		public override ZString CSI_ReferenceNumber
		{
			get => base.CSI_ReferenceNumber;
			set => base.CSI_ReferenceNumber = value;
		}

		[ResourceStringData("1AB47F0D-4738-4938-9C56-A204A1E7C145", Caption = "Description")]
		public ZString ReferenceNumberDescription
		{
			get
			{
				var numberDescription = Lookups.ApprovalCertificateNumberList.GetDescriptionFromCode(CSI_ReferenceNumber);
				if (string.IsNullOrEmpty(numberDescription))
				{
					return Lookups.ApprovalCertificateType.GetDescriptionFromCode(CSI_Code);
				}
				else
				{
					return numberDescription;
				}
			}
		}

		public CusEntryInstruction EntryInstruction => (CusEntryInstruction)Parent;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.ApprovalCertificate;
		}

		protected override CusSupportingInfoValidation GetNewValidation() => new ApprovalCertificateInfoValidation(this);
		public new ApprovalCertificateInfoLookups Lookups => (ApprovalCertificateInfoLookups)base.Lookups;
		protected override CusSupportingInfoLookups GetNewLookups() => new ApprovalCertificateInfoLookups(this);
	}
}
