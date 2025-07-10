using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCustomizationTreeModel : ZTreeModel<EdiCommissionAgreementTreeBizObjWrapper>
	{
		public EdiCommissionAgreementCustomizationTreeModel(EdiCommissionAgreementCustomization customization)
			: base(customization.Factory)
		{
			this.Customization = customization;

			Rebuild();
		}

		public readonly EdiCommissionAgreementCustomization Customization;
		readonly ZNodeCollection<EdiCommissionAgreementTreeBizObjWrapper> rootNodes = new ZNodeCollection<EdiCommissionAgreementTreeBizObjWrapper>();

		#region Rebuild

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public void Rebuild()
		{
			rootNodes.Clear();

			var licEnterprise = Customization.LicenceEnterprise;
			if (licEnterprise != null)
			{
				var allDatabases = licEnterprise.Databases;
				foreach (LicenceDatabase database in allDatabases)
				{
					var clientCompaniesQuery = new ZQuery(ClientCompanySchema.LCC_LD, database.PK);
					var clientCompanies = Factory.Load<ClientCompany>(clientCompaniesQuery);
					var clientCompaniesGroupedByCountry = clientCompanies.GroupBy(x => x.LCC_RN_NKCountryCode);
					var clientCompanyCountriesSet = new HashSet<ZString>();
					foreach (var countryGrouping in clientCompaniesGroupedByCountry)
					{
						Factory.AddFetchHint(RefCountrySchema.RN_Code, countryGrouping.Key);
					}

					var companyCountryWrappers = new List<EdiCommissionAgreementCountryWrapper>();
					foreach (var countryGrouping in clientCompaniesGroupedByCountry)
					{
						var companyWrappers = new List<EdiCommissionAgreementCompanyWrapper>();
						foreach (var company in countryGrouping)
						{
							companyWrappers.Add(new EdiCommissionAgreementCompanyWrapper(Customization, company));
						}

						var countryCode = countryGrouping.Key;
						companyCountryWrappers.Add(new EdiCommissionAgreementCountryWrapper(Customization, database, countryCode, companyWrappers));
						clientCompanyCountriesSet.Add(countryCode);
					}

					foreach (var companyCountry in Customization.CompanyAutoAddCountries.Where(x => x.EPC_LD == database.PK && !clientCompanyCountriesSet.Contains(x.EPC_RN_NKCountry)))
					{
						companyCountryWrappers.Add(new EdiCommissionAgreementCountryWrapper(Customization, database, companyCountry.EPC_RN_NKCountry, null));
					}

					var companyDatabaseWrapper = new EdiCommissionAgreementDatabaseWrapper(Customization, database, companyCountryWrappers);
					rootNodes.Add(new EdiCommissionAgreementCustomizationTreeNode(this, companyDatabaseWrapper));
				}
			}

			var newCompanyCountryWrappers = new List<EdiCommissionAgreementCountryWrapper>();
			foreach (var companyCountry in Customization.CompanyAutoAddCountries.Where(x => x.EPC_LD.IsEmpty))
			{
				newCompanyCountryWrappers.Add(new EdiCommissionAgreementCountryWrapper(Customization, null, companyCountry.EPC_RN_NKCountry, null));
			}
			var newCompanyDatabaseWrapper = new EdiCommissionAgreementDatabaseWrapper(Customization, null, newCompanyCountryWrappers);

			rootNodes.Add(new EdiCommissionAgreementCustomizationTreeNode(this, newCompanyDatabaseWrapper));
		}

		#endregion

		#region Nodes

		protected override ZNode<EdiCommissionAgreementTreeBizObjWrapper> CreateNewNodeCore(ZTreeModel<EdiCommissionAgreementTreeBizObjWrapper> treeModel, EdiCommissionAgreementTreeBizObjWrapper bizObj)
		{
			return new EdiCommissionAgreementCustomizationTreeNode((EdiCommissionAgreementCustomizationTreeModel)treeModel, bizObj);
		}

		protected override ZNodeCollection<EdiCommissionAgreementTreeBizObjWrapper> GetRootNodes()
		{
			return rootNodes;
		}

		#endregion
	}
}
