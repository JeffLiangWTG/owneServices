using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class DrawbackImportLicenseValidation : CusSupportingInfoValidation
	{
		public DrawbackImportLicenseValidation(DrawbackImportLicense parent) : base(parent)
		{
		}

		public new DrawbackImportLicense Parent => (DrawbackImportLicense)base.Parent;

		public bool IsImportLicense => Parent.Parent?.IsImportLicense ?? false;

		protected override void CheckCSI_Code()
		{
			base.CheckCSI_Code();

			if (IsImportLicense)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.CSI_CodeInfo);
			}
		}

		protected override void CheckCSI_ReferenceNumber()
		{
			base.CheckCSI_ReferenceNumber();

			if (IsImportLicense && !Parent.CSI_ReferenceNumber_ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ReferenceNumberInfo);

				if (Parent.Parent.IsImportLicenseGeneratedFromImportSiscomexLine)
				{
					var invoiceLines = Parent.Parent.EntryInstruction.InvoiceLines;
					if (invoiceLines.Count() > 1 && invoiceLines.Any(x => x.DrawbackCANumber != Parent.CSI_ReferenceNumber))
					{
						Parent.CSI_ReferenceNumberInfo.AddError(Res.GetString("72F07004-895E-4377-A3EB-33D250CFAF9E", "CA Number cannot be different between generated invoice lines of the same Entry Line."));
					}
				}
			}
		}

		protected override void CheckCSI_ItemNumber()
		{
			base.CheckCSI_ItemNumber();

			if (IsImportLicense && !Parent.CSI_ItemNumber_ReadOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CSI_ItemNumberInfo);
			}
		}
	}
}
