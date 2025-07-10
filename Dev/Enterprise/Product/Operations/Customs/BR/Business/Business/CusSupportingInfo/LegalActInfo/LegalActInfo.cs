using System.Collections.Generic;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class LegalActInfo : SingleCusSupportingInfo
	{
		#region Schema

		public new class Schema : AutoCusSupportingInfo.Schema
		{
			public const string CSI_YearOfIssue = "CSI_YearOfIssue";

			public const int LegalActSubjectMaxLength = 1;
			public const int LegalActTypeMaxLength = 5;
			public const int LegalActNumberMaxLength = 6;
			public const int LegalActIssuingBodyMaxLength = 10;
			public const int LegalActYearMaxLength = 4;
		}

		#endregion

		public LegalActInfo(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool SupportsNotes => false;

		[MaxLength(Schema.LegalActSubjectMaxLength)]
		public override ZString CSI_SubType { get => base.CSI_SubType; set => base.CSI_SubType = value; }

		[MaxLength(Schema.LegalActTypeMaxLength)]
		[List(nameof(Lookups) + "." + nameof(LegalActInfoLookups.ExTariffLegalActList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.LegalActInfo|CSI_Code", Caption = "Legal Act (Type)")]
		public override ZString CSI_Code { get => base.CSI_Code; set => base.CSI_Code = value; }

		[MaxLength(Schema.LegalActIssuingBodyMaxLength)]
		[List(nameof(Lookups) + "." + nameof(LegalActInfoLookups.LegalActIssuingAuthorityList))]
		[ResourceStringData("Enterprise.Customs.BR.Business.LegalActInfo|CSI_IssuerType", Caption = "Legal Act (Issuing Body)")]
		public override ZString CSI_IssuerType { get => base.CSI_IssuerType; set => base.CSI_IssuerType = value; }

		[MaxLength(Schema.LegalActNumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.BR.Business.LegalActInfo|CSI_ReferenceNumber", Caption = "Legal Act (Number)")]
		public override ZString CSI_ReferenceNumber { get => base.CSI_ReferenceNumber; set => base.CSI_ReferenceNumber = value; }

		[MaxLength(Schema.LegalActYearMaxLength)]
		[BusinessObjectTestExclude]
		[ResourceStringData("Enterprise.Customs.BR.Business.LegalActInfo|CSI_YearOfIssue", Caption = "Legal Act (Year)")]
		public ZString CSI_YearOfIssue
		{
			get => CSI_DateOfIssue.ToYearDateString();
			set
			{
				var oldValue = CSI_YearOfIssue;
				CSI_DateOfIssue = ZDateTimeHelper.ParseSmallDateTimeFromYear(value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCSI_YearOfIssue();
				}
				CSI_YearOfIssueInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo CSI_YearOfIssueInfo => GetZPropertyInfo(Schema.CSI_YearOfIssue);

		#region Override

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CSI_Type = CusSupportingInfoTypeList.Codes.LegalAct;
			CSI_ParentTableCode = JobComInvoiceLineSchema.Constants.Prefix;
		}

		public new LegalActInfoLookups Lookups => (LegalActInfoLookups)base.Lookups;

		protected override CusSupportingInfoLookups GetNewLookups() => new LegalActInfoLookups(this);

		public new LegalActInfoValidation Validation => (LegalActInfoValidation)base.Validation;

		protected override CusSupportingInfoValidation GetNewValidation() => new LegalActInfoValidation(this);

		public override IEnumerable<ZPropertyInfo> GetUsedFieldsInfos()
		{
			if (AdditionalTariff.GetLegalActSubjects(Parent as IAdditionalTariffParent).ContainsCode(CSI_SubType))
			{
				yield return CSI_SubTypeInfo;
			}
			yield return CSI_CodeInfo;
			yield return CSI_ReferenceNumberInfo;
			yield return CSI_IssuerTypeInfo;
			yield return CSI_DateOfIssueInfo;
		}

		#endregion
	}
}
