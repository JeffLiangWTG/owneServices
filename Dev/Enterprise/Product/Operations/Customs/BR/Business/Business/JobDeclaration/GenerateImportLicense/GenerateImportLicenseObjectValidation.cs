using System;
using System.Linq;
using CargoWise.EntityFramework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class GenerateImportLicenseObjectValidation : ZValidation
	{
		public GenerateImportLicenseObjectValidation(GenerateImportLicenseObject parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly GenerateImportLicenseObject parent;

		public override Type AutoValidationType => typeof(GenerateImportLicenseObjectValidation);

		public override void ValidateAll()
		{
			ValidateImportLicenseDeclarationPK();
		}

		public void ValidateImportLicenseDeclarationPK()
		{
			ValidateCalculatedProperty(parent.ImportLicenseDeclarationPKInfo);
		}

		protected void CheckImportLicenseDeclarationPK()
		{
			if (parent.ImportLicenseDeclarationPK.IsValid)
			{
				var invoiceNumbers = parent.EntryLine.InvoiceLines.Select(x => x.InvoiceHeader).Distinct().Select(x => x.JZ_InvoiceNumber);
				var licInvoiceNumbers = parent.ImportLicenseDeclaration?.Invoices?.Where(x => invoiceNumbers.Contains(x.JZ_InvoiceNumber) && x.InvoiceLines.Count > 0).Select(x => x.JZ_InvoiceNumber).ToArray();
				if (licInvoiceNumbers != null && licInvoiceNumbers.Length > 0)
				{
					parent.ImportLicenseDeclarationPKInfo.AddError(Res.GetString("E014EF31-3096-492D-8D83-751539ECC493", "License declaration already contains the Invoice Header: {0}", string.Join(",", licInvoiceNumbers)));
				}
			}
			else
			{
				parent.ImportLicenseDeclarationPKInfo.AddError(Res.GetString("2F623E43-FA2B-4CCA-9913-C24ED440AF28", "Please select one Import License to Generate."));
			}
		}
	}
}
