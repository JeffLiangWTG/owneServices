using System.Collections.Generic;
using Enterprise.Customs.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public sealed class JobComInvoiceHeaderToPrintCollection : LineToPrintCollection
	{
		public JobComInvoiceHeaderToPrintCollection(JobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override Dictionary<string, ResourceStringData> GetColumnCaptionResourceStringDictionaryCore()
		{
			var result = new Dictionary<string, ResourceStringData>() { };
			result.Add(OrganisationColName, Res.GetData("A430D8CF-2FE6-4680-824E-AE15D3063AB5", "Importer"));
			result.Add(IdentifierColName, Res.GetData("86BA2EE0-0F3D-4FA3-9BB6-9E3E4B9ADB96", "LVS Identifier"));
			return result;
		}

		protected override void GetLinesToPrint(BaseJobDeclaration declaration)
		{
			foreach (JobComInvoiceHeader invoice in declaration.Invoices)
			{
				Add(new JobComInvoiceHeaderToPrint(invoice));
			}
		}
	}
}
