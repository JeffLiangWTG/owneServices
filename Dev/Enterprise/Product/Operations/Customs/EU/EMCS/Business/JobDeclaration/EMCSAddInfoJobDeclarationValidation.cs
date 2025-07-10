using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.EMCS.Business
{
	public class EMCSAddInfoJobDeclarationValidation : EUEMCSAddInfoValidation
	{
		public EMCSAddInfoJobDeclarationValidation(EMCSAddInfoJobDeclaration parent)
			: base(parent)
		{
		}

		protected new EMCSAddInfoJobDeclaration Parent => (EMCSAddInfoJobDeclaration)base.Parent;

		protected EMCSJobDeclaration Declaration => (EMCSJobDeclaration)Parent.Parent;

		protected override void CheckZG_DeferredSubmission()
		{
			base.CheckZG_DeferredSubmission();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_DeferredSubmissionInfo);
		}

		protected override void CheckZG_GuarantorType()
		{
			base.CheckZG_GuarantorType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_GuarantorTypeInfo);

			if (ShouldValidateGuarantorMustBe0WhenDestinationTypeIs1
				&& Parent.ZG_GuarantorType != EMCSGuarantorTypeList.Codes.GuarantorNotRequiredQualifyingSameMemberStateMovements
				&& Declaration.JE_MessageSubType == EMCSDestinationTypeList.Codes.DestinationTaxWarehouse)
			{
				Parent.ZG_GuarantorTypeInfo.AddMessageError(Res.GetString("94B07800-41F2-427F-99E0-B655BCE65C59", "Guarantor(s) must be 0 when Destination Type is 1."));
			}

			if (Parent.ZG_GuarantorType.Contains(EMCSGuarantorTypeList.Codes.Consignee, StringComparison.OrdinalIgnoreCase)
					&& Declaration.JE_MessageSubType == EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown)
			{
				Parent.ZG_GuarantorTypeInfo.AddMessageError(Res.GetString("0EA59A17-74DB-4D4C-8496-AE43407B2126", "Guarantor(s) should not contain a 4 when Destination Type is 8."));
			}
		}

		protected virtual ZBool ShouldValidateGuarantorMustBe0WhenDestinationTypeIs1 => ZBool.True;

		protected override void CheckZG_TransportArrangement()
		{
			base.CheckZG_TransportArrangement();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_TransportArrangementInfo);
		}

		protected override void CheckZG_OriginType()
		{
			base.CheckZG_OriginType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_OriginTypeInfo);
			if (Parent.ZG_OriginType == EMCSOriginTypeList.Codes.Import)
			{
				if (!Declaration.CustomsOffices.ContainsCode(EuOfficeCodesTypes.Codes.OfficeOfDispatch))
				{
					Parent.ZG_OriginTypeInfo.AddMessageError(Res.GetString("14B171F6-C0C4-453C-BED1-206FC09AE110", "A Customs Office with Purpose '{0}' is required.", EuOfficeCodesTypes.Codes.OfficeOfDispatch));
				}

				if (Declaration.ImportSADNumbers.Count == 0)
				{
					Parent.ZG_OriginTypeInfo.AddMessageError(Res.GetString("5c31a48c-25a9-4a77-a0f1-3335faa1360f", "You have not entered an Import SAD Entry Number."));
				}
			}

			if (Parent.ZG_SubmissionType == EMCSSubmissionTypeList.Codes.SubmissionForDutyPaidB2B)
			{
				if (Parent.ZG_OriginType != EMCSOriginTypeList.Codes.DutyPaid)
				{
					Parent.ZG_OriginTypeInfo.AddMessageError(Res.GetString("e87af685-320a-4df8-b99f-b053bf988afd",
						"Origin Type must be {0} when Submission Type is {1}.",
						EMCSOriginTypeList.Codes.DutyPaid,
						EMCSSubmissionTypeList.Codes.SubmissionForDutyPaidB2B));
				}
			}
			else
			{
				if (Parent.ZG_OriginType != EMCSOriginTypeList.Codes.TaxWarehouse && Parent.ZG_OriginType != EMCSOriginTypeList.Codes.Import)
				{
					Parent.ZG_OriginTypeInfo.AddMessageError(Res.GetString("f4039637-5ad9-463d-a0db-d1d6eea7dd2b",
						"Origin Type must be either {0} or {1} when Submission Type is not {2}.",
						EMCSOriginTypeList.Codes.TaxWarehouse,
						EMCSOriginTypeList.Codes.Import,
						EMCSSubmissionTypeList.Codes.SubmissionForDutyPaidB2B));
				}
			}
		}

		protected override void CheckZG_DispatchReference()
		{
			base.CheckZG_DispatchReference();
			if (ShouldValidateDispatchReferenceIsMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ZG_DispatchReferenceInfo);
			}
		}

		protected virtual ZBool ShouldValidateDispatchReferenceIsMandatory => ZBool.True;

		protected override void CheckZG_SubmissionType()
		{
			base.CheckZG_SubmissionType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ZG_SubmissionTypeInfo);
		}

		protected override void CheckZG_CCTMSA()
		{
			base.CheckZG_CCTMSA();
			if (Declaration.JE_MessageSubType == EMCSDestinationTypeList.Codes.DestinationExemptedConsignee && Parent.ZG_CCTMSA.IsEmpty)
			{
				Parent.ZG_CCTMSAInfo.AddMessageError(Res.GetString("71F26FA8-43F3-497A-8D1C-07A4CE0997E7"
					, "Member State is required when Destination Type = 5 - {0}.", EMCSDestinationTypeList.Descriptions.DestinationExemptedConsignee));
			}
			ListValidation.MessageErrorIfInvalidCode(Parent.ZG_CCTMSAInfo);
		}
	}
}
