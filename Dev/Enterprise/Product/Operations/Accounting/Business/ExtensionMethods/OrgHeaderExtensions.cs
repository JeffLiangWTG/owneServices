using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business
{
	public static class OrgHeaderExtensions
	{
		public static bool IsGlobalCreditGroupParent(OrgHeader orgHeader)
		{
			var result = false;

			var sql = "SELECT Result FROM dbo.IsOrgGlobalCreditGroupParent(@OrgPK)";
			var isOrgGlobalCreditGroupParentResult = new DynamicBusinessObjectCollection(orgHeader.Factory);
			isOrgGlobalCreditGroupParentResult.Load(sql, new[]
			{
				ZSqlParameter.New("@OrgPK", orgHeader.PK, OrgHeaderSchema.PK)
			});

			if (isOrgGlobalCreditGroupParentResult.Count == 1)
			{
				result = new ZInt(isOrgGlobalCreditGroupParentResult[0]["Result"]) == 1;
			}

			return result;
		}

		public static bool IsGlobalCreditGroupChild(OrgHeader orgHeader)
		{
			var orgMiscServ = orgHeader.MiscServ;
			return !orgMiscServ?.OM_OH_ARGlobalCreditGroup.IsEmpty ?? false;
		}

		public static IJobInvoicingPlugIn[] InvoiceTargets(this OrgHeader debtor, IJobInvoicingPlugIn relatedShipment)
		{
			Argument.NotNull(debtor, nameof(debtor));
			return debtor.InvoiceTargetsEnabled() ? GatewayInvoiceTargetJobFinder.GetInvoiceTargets(relatedShipment) : System.Array.Empty<IJobInvoicingPlugIn>();
		}

		public static ZString GetInvoiceTarget(this OrgHeader debtor, ZString relatedJobNum, Job invoicingJob)
		{
			Argument.NotNull(debtor, nameof(debtor));
			return debtor.InvoiceTargetsEnabled()
				? GatewayInvoiceTargetJobFinder.GetInvoiceTarget(relatedJobNum, invoicingJob)
				: ZString.Empty;
		}

		public static bool InvoiceTargetsEnabled(this OrgHeader debtor)
		{
			Argument.NotNull(debtor, nameof(debtor));
			return debtor.IsProxyOrgOfAnyCompany(true);
		}

		public static bool HasWithholdTaxExemption(this OrgHeader creditor, ZDateTime? date = null, ZString? countryCode = null)
		{
			Argument.NotNull(creditor, nameof(creditor));
			return creditor.RequiredDocuments.Cast<JobRequiredDocument>().Any(x => x.EQ_DocCategory == ReferenceTypes.ClientSupplierRelationship
																				&& x.EQ_DocType == RefDocTypes.WithholdingTaxExemption
																				&& x.EQ_RN_NKRelatedCountry == (countryCode ?? GlbCompany.CurrentCompany.Country.Code)
																				&& x.EQ_ValidToDate >= (date ?? ZDateTime.Today));
		}
	}
}
