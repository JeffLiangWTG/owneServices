using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class ImportLicenseInfoValidation : CusSupportingInfoValidation
	{
		public ImportLicenseInfoValidation(ImportLicenseInfo parent) : base(parent)
		{
		}

		public new ImportLicenseInfo Parent => (ImportLicenseInfo)base.Parent;

		JobComInvoiceLine InvoiceLine => Parent.Parent;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			if (InvoiceLine.IsImportSiscomex && !Parent.CSI_ReferenceNumber.IsEmpty)
			{
				var targetInfo = Parent.CSI_CodeInfo;
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_CodeInfo);
			}
		}

		protected override void CheckCSI_DateOfIssue()
		{
			base.CheckCSI_DateOfIssue();

			if (InvoiceLine.IsImportSiscomex)
			{
				MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyIsEntered(Parent.CSI_DateOfIssueInfo, Parent.CSI_ReferenceNumberInfo);
			}
		}

		protected override void CheckCSI_DateOfIssueIsValidZDateTimeRange()
		{
			TypeValidation.CheckValidSmallDateTime(Parent.CSI_DateOfIssueInfo);
		}

		protected override void CheckCSI_SubType()
		{
			base.CheckCSI_SubType();

			if (InvoiceLine.IsImportSiscomex && !Parent.CSI_ReferenceNumber.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CSI_SubTypeInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();
			if (InvoiceLine.IsImportSiscomex || (InvoiceLine.IsImport && !InvoiceLine.IsAttachedToPersistentDeclaration))
			{
				if (!Parent.CSI_ReferenceNumber.IsNumbersOnlyOrEmpty)
				{
					Parent.CSI_ReferenceNumberInfo.AddError(Res.GetString("14199DE4-01C7-4064-BEF4-4694F21B1794", "Import License Number must only contain numeric characters."));
				}
				if (!Parent.CSI_ReferenceNumber.IsEmpty && Parent.CSI_ReferenceNumber.Length < ImportLicenseInfo.Schema.CSI_ReferenceNumberMaxLength)
				{
					Parent.CSI_ReferenceNumberInfo.AddError(Res.GetString("A33A9BC6-3369-4395-A95E-456E37D5D121", "Import License Number has less than 10 characters."));
				}
			}
		}
	}
}
