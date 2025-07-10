using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles
{
	public class EdiCommissionAgreementLookups : OrgCommissionAgreementLookups
	{
		public EdiCommissionAgreementLookups(EdiCommissionAgreement parent)
			: base(parent)
		{ }

		new EdiCommissionAgreement Parent
		{
			get { return (EdiCommissionAgreement)base.Parent; }
		}

		public CodeDescriptionPairList CustomerPairList
		{
			get
			{
				var opportunity = Parent.Opportunity;
				var opportunityOrgPk = opportunity.Header?.PK ?? ZGuid.Empty;

				return Factory.GetCachedValue("EdiCommissionAgreementLookups.CustomerPairList" + opportunityOrgPk, () =>
				{
					var result = new CodeDescriptionPairList();
					var invoiceToOrgs = Factory.Load<OrgHeader>(GetInvoiceToOrgQuery(opportunityOrgPk));
					foreach (var orgGroup in Customers.Cast<OrgHeader>()
							.GroupBy(x => GetOrgCategory(x.OH_Code, invoiceToOrgs, opportunity.Header))
							.OrderBy(x => x.Key))
					{
						result.AddPair("");
						result.Add(new CategoryCodeDescriptionPair(orgGroup.Key, ""));
						foreach (var org in orgGroup.OrderBy(x => x.OH_Code))
						{
							result.AddPair(org.OH_Code, org.OH_FullName);
						}
					}
					return result;
				});
			}
		}

		ZQuery GetInvoiceToOrgQuery(ZGuid opportunityOrgPk)
		{
			var invoiceOrgQuery = new ZDBOnlyQuery(typeof(OrgHeader));
			var invoiceDeliverySubQuery = new ZDBOnlySubQuery(typeof(ClientInvoiceDelivery), ClientInvoiceDeliverySchema.L9_OH_InvoiceTo);
			invoiceDeliverySubQuery.AddToFilter(ClientInvoiceDeliverySchema.L9_IsBilled, ZBool.True);
			var licenceCompanySubQuery = new ZDBOnlySubQuery(typeof(LicenceCompany), ClientInvoiceDeliverySchema.L9_LC);
			licenceCompanySubQuery.AddToFilter(LicenceCompanySchema.LC_OH, opportunityOrgPk);
			invoiceDeliverySubQuery.AddSubQuery(licenceCompanySubQuery, JoinCondition.And);
			invoiceOrgQuery.AddSubQuery(invoiceDeliverySubQuery, JoinCondition.And);

			return invoiceOrgQuery;
		}

		ZString GetOrgCategory(ZString orgCode, OrgHeader[] invoiceToOrgs, OrgHeader opportunityOrg)
		{
			var result = ZString.Empty;

			if (invoiceToOrgs.Any(x => orgCode == x.OH_Code))
			{
				result = "Invoicing Org.";
			}
			else if (opportunityOrg != null && orgCode == opportunityOrg.OH_Code)
			{
				result = "Opportunity Org.";
			}
			else
			{
				result = "Parent Org.";
			}

			return result;
		}

		protected override ZQuery GetCustomersFilter(ZGuid opportunityOrgPk)
		{
			if (opportunityOrgPk.IsEmpty)
			{
				return ZQuery.NoResultQuery;
			}

			var result = new ZDBOnlyQuery(typeof(OrgHeader));
			result.AddFilterAndZSQLParameterCollection(@"
OH_PK IN
(
	SELECT OrgPk
	FROM dbo.vw_OrgManagementGroupingNode TreeNode
	WHERE
		TreeNode.RootOrgPk IN (SELECT RootOrgPk FROM dbo.vw_OrgManagementGroupingNode WHERE OrgPk = @MasterPk)

	UNION ALL

	SELECT @MasterPk

	UNION ALL

	SELECT L9_OH_InvoiceTo 
	FROM dbo.ClientInvoiceDelivery 
	WHERE 
		L9_OH_InvoiceTo IS NOT NULL 
		AND L9_IsBilled = 'Y' 
		AND L9_LC IN (SELECT LC_PK FROM dbo.LicenceCompany WHERE LC_OH = @MasterPk)
)", new ZSqlParameterCollection(ZSqlParameter.New("@MasterPk", opportunityOrgPk, OrgHeaderSchema.PK)));

			return result;
		}
	}
}

