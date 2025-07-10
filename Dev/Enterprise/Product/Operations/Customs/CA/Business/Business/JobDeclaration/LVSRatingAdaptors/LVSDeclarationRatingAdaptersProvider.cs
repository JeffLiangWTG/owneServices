using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.Integration.Rating;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CA.Business
{
	public class LVSDeclarationRatingAdaptersProvider : RatingAdaptersProvider<JobDeclaration>
	{
		public LVSDeclarationRatingAdaptersProvider(JobDeclaration parent) : base(parent) { }

		protected override List<IAutoRating> GetAdapters(JobDeclaration parent, IAutoRatingInteractor uiInteractor, AutoRateOptions options)
		{
			var result = base.GetAdapters(parent, uiInteractor, options);

			if (!parent.Invoices.Any())
			{
				uiInteractor.Warning(LogMessages.RatingAdaptersCannotBeCreated("No invoices found"));

				return result;
			}

			var invoiceHeadersByImporterAndIncoTerms = new Dictionary<OrganizationReferenceKey, List<JobComInvoiceHeader>>();

			foreach (JobComInvoiceHeader invoiceHeader in parent.Invoices)
			{
				var importer = parent.IsLVSTotalConsolidation ? invoiceHeader.Importer_Effective : parent.Importer;

				if (importer != null && OrgImpAddInfo.Get(importer).ZO_EffectiveLVSInvoiceDetailCode == LVSInvoiceDetailCodes.Codes.Detail)
				{
					// when details are required add seperate adaptor for each individual shipment
					result.Add(new LVSDeclarationRatingAdapter(importer, new[] { invoiceHeader }, invoiceHeader.IncoTerm, invoiceHeader.EffectiveImportClearanceProvince, parent));
				}
				else
				{
					var key = new OrganizationReferenceKey(importer, invoiceHeader.IncoTerm, invoiceHeader.EffectiveImportClearanceProvince);
					if (!invoiceHeadersByImporterAndIncoTerms.ContainsKey(key))
					{
						invoiceHeadersByImporterAndIncoTerms.Add(key, new List<JobComInvoiceHeader>());
					}
					invoiceHeadersByImporterAndIncoTerms[key].Add(invoiceHeader);
				}
			}

			result.AddRange(invoiceHeadersByImporterAndIncoTerms.Select(x => new LVSDeclarationRatingAdapter(x.Key.Organization, x.Value, x.Key.Reference, x.Key.Reference2, parent)));

			return result;
		}
	}
}
