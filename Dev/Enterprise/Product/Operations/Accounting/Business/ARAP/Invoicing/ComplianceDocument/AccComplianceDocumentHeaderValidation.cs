//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoAccComplianceDocumentHeaderValidation
//
//    This class should be used for overriding validation in AutoAccComplianceDocumentHeaderValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Accounting.Business
{
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ARAP.Invoicing;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;
	using static Enterprise.Core.Constants;

	public class AccComplianceDocumentHeaderValidation : AutoAccComplianceDocumentHeaderValidation
	{
		public AccComplianceDocumentHeaderValidation(AutoAccComplianceDocumentHeader parent) : base(parent)
		{
		}

		protected new AccComplianceDocumentHeader Parent => (AccComplianceDocumentHeader)base.Parent;

		protected AccComplianceDocumentHeaderDetailValidationHelper AccComplianceDocumentHeaderDetailValidationHelper
		{
			get
			{
				if (fAccComplianceDocumentHeaderDetailValidationHelper == null)
				{
					fAccComplianceDocumentHeaderDetailValidationHelper = new AccComplianceDocumentHeaderDetailValidationHelper(Parent.Factory, Parent);
				}

				return fAccComplianceDocumentHeaderDetailValidationHelper;
			}
		}

		AccComplianceDocumentHeaderDetailValidationHelper fAccComplianceDocumentHeaderDetailValidationHelper;

		#region Helper Validators and Calculators

		public PeriodValidationProvider PeriodValidation
		{
			get { return periodValidation ?? (periodValidation = GetPeriodValidationProvider()); }
		}
		PeriodValidationProvider periodValidation;

		protected virtual PeriodValidationProvider GetPeriodValidationProvider()
		{
			return new PeriodValidationProvider(Parent.Factory);
		}

		#endregion

		protected override void CheckADH_SupportingDocumentNumber()
		{
			base.CheckADH_SupportingDocumentNumber();

			if (!Parent.ADH_SupportingDocumentNumber.IsEmpty && Parent.ADH_SupportingDocumentType.IsEmpty)
			{
				Parent.ADH_SupportingDocumentNumberInfo.AddError(Res.GetString("D2001760-3D9E-4186-8773-61168238D8B8", "The Supporting Document Number should have a Supporting Document Type."));
			}
		}

		protected override void CheckADH_ReportingPeriod()
		{
			base.CheckADH_ReportingPeriod();
			if (Parent.IsAdded)
			{
				var error = AccComplianceDocumentHeaderDetailValidationHelper.ValidateDocumentReportingPeriod();
				if (!error.IsEmpty)
				{
					Parent.ADH_ReportingPeriodInfo.AddError(error);
				}
			}
		}

		protected override void CheckADH_DocumentDate()
		{
			base.CheckADH_DocumentDate();
			if (Parent.IsAdded)
			{
				MandatoryValidation.CheckEntered(Parent.ADH_DocumentDateInfo);

				if (!Parent.ADH_DocumentDateInfo.HasErrors() && Parent.ADH_DocumentDateInfo.HasChanges)
				{
					var error = AccComplianceDocumentHeaderDetailValidationHelper.ValidateDocumentDate();
					if (!error.IsEmpty)
					{
						Parent.ADH_DocumentDateInfo.AddError(error);
					}
				}
			}
		}

		protected override void CheckADH_ComplianceSubType()
		{
			base.CheckADH_ComplianceSubType();
			if (GlbCompany.CurrentCompany.Country.SupportComplianceSubType && Parent.IsAdded)
			{
				if (!Parent.ADH_ComplianceSubType.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(Parent.ADH_ComplianceSubTypeInfo);
				}
			}
		}

		protected override void CheckADH_XD_ComplianceBook()
		{
			base.CheckADH_XD_ComplianceBook();
			if (Parent.IsAdded)
			{
				var complianceBook = new BusinessObjectFactory().LoadTop1<AccComplianceSequence>(new ZQuery(AccComplianceSequenceSchema.PK, Parent.ADH_XD_ComplianceBook));

				if (complianceBook != null && complianceBook.XD_SequenceClass != Parent.ADH_ComplianceSubType)
				{
					Parent.ADH_XD_ComplianceBookInfo.AddError(Res.GetString("F693D8A0-FAAC-4938-9857-310F728C94FA", "The compliance book and compliance sub type do not match."));
				}

				if (complianceBook != null && (!complianceBook.XD_IsActive || Parent.IsSequenceBookExpired))
				{
					Parent.ADH_XD_ComplianceBookInfo.AddError(Res.GetString("7979BF13-AB01-45A2-B6A9-9986FE01FEFD", "The compliance book is inactive or expired."));
				}
			}
		}

		protected override void CheckADH_DocumentNumber()
		{
			base.CheckADH_DocumentNumber();

			if (!Parent.ADH_DocumentNumberInfo.HasErrors() && Parent.IsAdded)
			{
				var error = AccComplianceDocumentHeaderDetailValidationHelper.ValidateDocumentNmberMatchINVForCRD(Parent.Organisation?.PK ?? ZGuid.Empty, Parent.ADH_Ledger);
				if (!error.IsEmpty)
				{
					Parent.ADH_DocumentNumberInfo.AddError(error);
				}
			}
		}

		protected override void CheckADH_SupportingReason()
		{
			base.CheckADH_SupportingReason();
			if (!Parent.ADH_ComplianceSubType.IsEmpty)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ADH_SupportingReasonInfo);
			}
		}

		protected override void CheckADH_Description()
		{
			base.CheckADH_Description();
			MandatoryValidation.CheckEntered(Parent.ADH_DescriptionInfo);
		}

		protected override void CheckADH_OH_Organisation()
		{
			base.CheckADH_OH_Organisation();
			if (Parent.ADH_DocumentStatus != ComplianceDocumentStatus.Voided)
			{
				MandatoryValidation.CheckEntered(Parent.ADH_OH_OrganisationInfo);
			}
			ListValidation.ErrorIfInvalidPK(Parent.ADH_OH_OrganisationInfo);
		}

		protected override void CheckADH_OA_AddressOverride()
		{
			base.CheckADH_OA_AddressOverride();
			Parent.OrganisationAddressWithContact.RefreshBinding();
		}

		protected override void CheckADH_VoidingReason()
		{
			base.CheckADH_VoidingReason();
			if (Parent.IsSpecialVoiding)
			{
				MandatoryValidation.CheckEntered(Parent.ADH_VoidingReasonInfo);
			}
		}
	}
}
