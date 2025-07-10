using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementCustomizationAutoAdder
	{
		public EdiCommissionAgreementCustomizationAutoAdder(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");

			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		#region Execute (LicenceDatabase)

		public void Execute(LicenceDatabase newlyCreatedLicenceDatabase)
		{
			foreach (var agreement in GetAgreementsForAutoAddNewDatabases(newlyCreatedLicenceDatabase))
			{
				foreach (var customizationToUpdate in GetCustomizationsToUpdate(agreement, true))
				{
					customizationToUpdate.DatabasePivots.AddNew(newlyCreatedLicenceDatabase);
				}
			}

			foreach (var agreement in GetAgreementsForAutoAddNewCompanyDatabases(newlyCreatedLicenceDatabase))
			{
				var customization = agreement.Customization;
				var shouldAutoAddNewDatabase = customization.CompanyAutoAddDatabases.Any(x => x.EPD_LD.IsEmpty);
				var countriesToAutoAddForNewDatabase = customization.CompanyAutoAddCountries.Where(x => x.EPC_LD.IsEmpty).Select(x => x.EPC_RN_NKCountry).ToList();
				var shouldAutoApprove = customization.CompanyPivots.Count == 0;
				foreach (var customizationToUpdate in GetCustomizationsToUpdate(agreement, shouldAutoApprove))
				{
					if (shouldAutoAddNewDatabase)
					{
						if (!customizationToUpdate.CompanyAutoAddDatabases.Any(x => x.EPD_LD == newlyCreatedLicenceDatabase.PK))
						{
							customizationToUpdate.CompanyAutoAddDatabases.AddNew(newlyCreatedLicenceDatabase);
						}
					}

					var currentAutoAddCountriesForNewDatabase = new HashSet<ZString>(customizationToUpdate.CompanyAutoAddCountries.Where(x => x.EPC_LD == newlyCreatedLicenceDatabase.PK).Select(x => x.EPC_RN_NKCountry));
					foreach (var country in countriesToAutoAddForNewDatabase)
					{
						if (!currentAutoAddCountriesForNewDatabase.Contains(country))
						{
							customizationToUpdate.CompanyAutoAddCountries.AddNew(newlyCreatedLicenceDatabase, country);
						}
					}
				}
			}
		}

		IEnumerable<EdiCommissionAgreement> GetAgreementsForAutoAddNewDatabases(LicenceDatabase newlyCreatedLicenceDatabase)
		{
			if (newlyCreatedLicenceDatabase.LD_LE.IsEmpty)
			{
				return Enumerable.Empty<EdiCommissionAgreement>();
			}

			var agreementQuery = new ZDBOnlyQuery(typeof(EdiCommissionAgreement));
			agreementQuery.AddToFilter(OrgCommissionAgreementSchema.CA0_ReversedDateUtc, null);

			AddCustomerFilter(agreementQuery, newlyCreatedLicenceDatabase.LD_LE);

			var customizationSubQuery = new ZDBOnlySubQuery(typeof(EdiCommissionAgreementCustomization), EdiCommissionAgreementCustomizationSchema.EZN_CA0);
			customizationSubQuery.AddToFilter(EdiCommissionAgreementCustomizationSchema.EZN_IsAllDatabases, false);

			var hasAutoAddNewDatabaseQuery = new ZDBOnlySubQuery(typeof(EdiCommissionAgreementDatabasePivot), EdiCommissionAgreementDatabasePivotSchema.EZD_EZN);
			hasAutoAddNewDatabaseQuery.AddToFilter(EdiCommissionAgreementDatabasePivotSchema.EZD_LD, null);

			customizationSubQuery.AddSubQuery(hasAutoAddNewDatabaseQuery, JoinCondition.And);

			agreementQuery.AddSubQuery(customizationSubQuery, JoinCondition.And);

			return factory.Load<EdiCommissionAgreement>(agreementQuery);
		}

		IEnumerable<EdiCommissionAgreement> GetAgreementsForAutoAddNewCompanyDatabases(LicenceDatabase newlyCreatedLicenceDatabase)
		{
			if (newlyCreatedLicenceDatabase.LD_LE.IsEmpty)
			{
				return Enumerable.Empty<EdiCommissionAgreement>();
			}

			var agreementQuery = new ZDBOnlyQuery(typeof(EdiCommissionAgreement));
			agreementQuery.AddToFilter(OrgCommissionAgreementSchema.CA0_ReversedDateUtc, null);

			AddCustomerFilter(agreementQuery, newlyCreatedLicenceDatabase.LD_LE);

			var customizationSubQuery = new ZDBOnlySubQuery(typeof(EdiCommissionAgreementCustomization), EdiCommissionAgreementCustomizationSchema.EZN_CA0);
			customizationSubQuery.AddToFilter(EdiCommissionAgreementCustomizationSchema.EZN_IsAllCompanies, false);

			var hasAutoAddNewDatabaseSubQuery = new ZDBOnlySubQuery(typeof(EdiCommissionAgreementCompanyAutoAddDatabase), EdiCommissionAgreementCompanyAutoAddDatabaseSchema.EPD_EZN);
			hasAutoAddNewDatabaseSubQuery.AddToFilter(EdiCommissionAgreementCompanyAutoAddDatabaseSchema.EPD_LD, null);

			var hasAutoAddCountryForNewDatabaseSubQuery = new ZDBOnlySubQuery(typeof(EdiCommissionAgreementCompanyAutoAddCountry), EdiCommissionAgreementCompanyAutoAddCountrySchema.EPC_EZN);
			hasAutoAddCountryForNewDatabaseSubQuery.AddToFilter(EdiCommissionAgreementCompanyAutoAddCountrySchema.EPC_LD, null);

			var hasAutoAddQuery = new ZDBOnlyQuery(typeof(EdiCommissionAgreementCustomization));
			hasAutoAddQuery.AddSubQuery(hasAutoAddNewDatabaseSubQuery, JoinCondition.Or);
			hasAutoAddQuery.AddSubQuery(hasAutoAddCountryForNewDatabaseSubQuery, JoinCondition.Or);

			customizationSubQuery.AddToFilter(hasAutoAddQuery);

			agreementQuery.AddSubQuery(customizationSubQuery, JoinCondition.And);

			return factory.Load<EdiCommissionAgreement>(agreementQuery);
		}

		#endregion

		#region Execute (ClientCompany)

		public void Execute(ClientCompany newlyCreatedClientCompany)
		{
			var agreements = GetAgreementsForAutoAddNewClientCompany(newlyCreatedClientCompany);

			foreach (var agreement in agreements)
			{
				foreach (var customizationToUpdate in GetCustomizationsToUpdate(agreement, true))
				{
					customizationToUpdate.CompanyPivots.AddNew(newlyCreatedClientCompany);
				}
			}
		}

		IEnumerable<EdiCommissionAgreement> GetAgreementsForAutoAddNewClientCompany(ClientCompany newlyCreatedClientCompany)
		{
			if (newlyCreatedClientCompany.Database == null)
			{
				return Enumerable.Empty<EdiCommissionAgreement>();
			}

			var agreementQuery = new ZDBOnlyQuery(typeof(EdiCommissionAgreement));
			agreementQuery.AddToFilter(OrgCommissionAgreementSchema.CA0_ReversedDateUtc, null);

			AddCustomerFilter(agreementQuery, newlyCreatedClientCompany.Database.LD_LE);

			var customizationSubQuery = new ZDBOnlySubQuery(typeof(EdiCommissionAgreementCustomization), EdiCommissionAgreementCustomizationSchema.EZN_CA0);
			customizationSubQuery.AddToFilter(EdiCommissionAgreementCustomizationSchema.EZN_IsAllCompanies, false);

			var hasAutoAddDatabaseSubQuery = new ZDBOnlySubQuery(typeof(EdiCommissionAgreementCompanyAutoAddDatabase), EdiCommissionAgreementCompanyAutoAddDatabaseSchema.EPD_EZN);
			hasAutoAddDatabaseSubQuery.AddToFilter(EdiCommissionAgreementCompanyAutoAddDatabaseSchema.EPD_LD, newlyCreatedClientCompany.LCC_LD);

			var hasAutoAddCountryForDatabaseSubQuery = new ZDBOnlySubQuery(typeof(EdiCommissionAgreementCompanyAutoAddCountry), EdiCommissionAgreementCompanyAutoAddCountrySchema.EPC_EZN);
			hasAutoAddCountryForDatabaseSubQuery.AddToFilter(EdiCommissionAgreementCompanyAutoAddCountrySchema.EPC_LD, newlyCreatedClientCompany.LCC_LD);
			hasAutoAddCountryForDatabaseSubQuery.AddToFilter(EdiCommissionAgreementCompanyAutoAddCountrySchema.EPC_RN_NKCountry, newlyCreatedClientCompany.LCC_RN_NKCountryCode);

			var hasAutoAddQuery = new ZDBOnlyQuery(typeof(EdiCommissionAgreementCustomization));
			hasAutoAddQuery.AddSubQuery(hasAutoAddDatabaseSubQuery, JoinCondition.Or);
			hasAutoAddQuery.AddSubQuery(hasAutoAddCountryForDatabaseSubQuery, JoinCondition.Or);

			customizationSubQuery.AddToFilter(hasAutoAddQuery);

			agreementQuery.AddSubQuery(customizationSubQuery, JoinCondition.And);

			return factory.Load<EdiCommissionAgreement>(agreementQuery);
		}

		#endregion

		#region Helper Methods

		static void AddCustomerFilter(ZQuery agreementQuery, ZGuid licenceEnterprisePk)
		{
			agreementQuery.AddFilterAndZSQLParameterCollection(string.Format(CultureInfo.InvariantCulture, @"
{0} IN
(
	SELECT {1}
	FROM {2}
	WHERE {3} = @LE_PK

	UNION ALL

	SELECT {4}
	FROM {5}
	WHERE {6} = @LE_PK
)",
			OrgCommissionAgreementSchema.Constants.CA0_OH_Customer, // 0

			LicenceCompanySchema.Constants.LC_OH, // 1
			LicenceCompanySchema.Constants.TableName, // 2
			LicenceCompanySchema.Constants.LC_LE, // 3

			LicenceEnterpriseSchema.Constants.LE_OH, // 4
			LicenceEnterpriseSchema.Constants.TableName, // 5
			LicenceEnterpriseSchema.Constants.PK // 6
			), new ZSqlParameterCollection(ZSqlParameter.New("@LE_PK", licenceEnterprisePk, LicenceEnterpriseSchema.PK)));
		}

		static IEnumerable<EdiCommissionAgreementCustomization> GetCustomizationsToUpdate(EdiCommissionAgreement agreement, bool autoApprove)
		{
			Argument.NotNull(agreement.Customization, "agreement.Customization");

			if (autoApprove)
			{
				yield return agreement.Customization;

				var agreementDraft = (EdiCommissionAgreement)agreement.Draft;
				if (agreementDraft != null && agreementDraft.Customization != null)
				{
					yield return agreementDraft.Customization;
				}
			}
			else
			{
				if (agreement.IsDraft)
				{
					yield return agreement.Customization;
				}
				else
				{
					if (!agreement.HasDraft)
					{
						var agreementDraft = (EdiCommissionAgreement)agreement.CreateDraft();
						var customizationDraft = agreementDraft.GetOrCreateCustomization();

						yield return customizationDraft;
					}
				}
			}
		}

		#endregion
	}
}

