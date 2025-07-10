using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.MasterFiles.Business
{
	public class EdiCommissionAgreementConflictsFinder : CommissionAgreementConflictsFinder
	{
		protected EdiCommissionAgreementConflictsFinder()
		{
		}

		#region GetMoreGenericCommissionAgreementConflicts

		protected override IEnumerable<ICommissionAgreementConflict> GetMoreGenericCommissionAgreementConflicts(OrgCommissionAgreement commissionAgreement)
		{
			var ediCommissionAgreement = commissionAgreement as EdiCommissionAgreement;
			if (ediCommissionAgreement == null)
			{
				yield break;
			}

			foreach (var productItem in commissionAgreement.ProductItems)
			{
				foreach (var serviceItem in productItem.ChildServiceItems)
				{
					foreach (var subModuleItem in serviceItem.ChildSubModuleItems)
					{
						var itemArgs = GetItemPathCodesForConflictChecking(subModuleItem);
						if (itemArgs != null)
						{
							foreach (var companyConflict in GetMoreGenericCommissionAgreementCompanyConflicts(ediCommissionAgreement.Customization, subModuleItem, itemArgs))
							{
								yield return companyConflict;
							}

							foreach (var companyAutoAddCountryConflict in GetMoreGenericCommissionAgreementCompanyAutoAddCountryConflicts(ediCommissionAgreement.Customization, subModuleItem, itemArgs))
							{
								yield return companyAutoAddCountryConflict;
							}

							foreach (var companyAutoAddDatabaseConflict in GetMoreGenericCommissionAgreementCompanyAutoAddDatabaseConflicts(ediCommissionAgreement.Customization, subModuleItem, itemArgs))
							{
								yield return companyAutoAddDatabaseConflict;
							}

							foreach (var databaseConflict in GetMoreGenericCommissionAgreementDatabaseConflicts(ediCommissionAgreement.Customization, subModuleItem, itemArgs))
							{
								yield return databaseConflict;
							}
						}
					}
				}
			}
		}

		#region FindMoreGenericItemForCompany

		IEnumerable<CommissionAgreementItemAndCompanyConflict> GetMoreGenericCommissionAgreementCompanyConflicts(EdiCommissionAgreementCustomization customization, OrgCommissionAgreementItem subModuleItem, CommissionItemArgs itemArgs)
		{
			if (customization == null || customization.EZN_IsAllCompanies)
			{
				foreach (var genericItem in FindMoreGenericItemForAllCompanies(subModuleItem.CommissionAgreement, itemArgs))
				{
					yield return new CommissionAgreementItemAndCompanyConflict(subModuleItem, null, genericItem.CommissionAgreement);
				}
			}
			else
			{
				foreach (var companyPivot in customization.CompanyPivots)
				{
					foreach (var genericItem in FindMoreGenericItemForCompany(subModuleItem.CommissionAgreement, itemArgs, companyPivot.EPY_LCC))
					{
						yield return new CommissionAgreementItemAndCompanyConflict(subModuleItem, companyPivot.ClientCompany, genericItem.CommissionAgreement);
					}
				}
			}
		}

		IEnumerable<OrgCommissionAgreementItem> FindMoreGenericItemForAllCompanies(OrgCommissionAgreement commissionAgreement, CommissionItemArgs itemArgs)
		{
			var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, true).OfType<EdiCommissionAgreement>();
			var otherAgreementsForAllCompanies = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization == null || x.Customization.EZN_IsAllCompanies);
			var coveringItemsForAllCompanies = new List<OrgCommissionAgreementItem>();
			foreach (var agreement in otherAgreementsForAllCompanies)
			{
				coveringItemsForAllCompanies.AddRange(agreement.GetItemsThatCover(itemArgs));
			}

			if (coveringItemsForAllCompanies.Count > 0)
			{
				yield return GetMostSpecificCommissionAgreementItem(coveringItemsForAllCompanies);
			}
		}

		IEnumerable<OrgCommissionAgreementItem> FindMoreGenericItemForCompany(OrgCommissionAgreement commissionAgreement, CommissionItemArgs itemArgs, ZGuid companyPK)
		{
			var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, true).OfType<EdiCommissionAgreement>().ToArray();
			var otherAgreementsForSameCompany = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllCompanies && x.Customization.CompanyPivots.Any(y => y.EPY_LCC == companyPK));
			var coveringItemsForSameCompany = new List<OrgCommissionAgreementItem>();
			foreach (var agreement in otherAgreementsForSameCompany)
			{
				coveringItemsForSameCompany.AddRange(agreement.GetItemsThatCover(itemArgs));
			}

			if (coveringItemsForSameCompany.Count > 0)
			{
				yield return GetMostSpecificCommissionAgreementItem(coveringItemsForSameCompany);
			}
			else
			{
				// agreements with a specific company have priority over agreements for 'all' companies
				var otherAgreementsForAllCompanies = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization == null || x.Customization.EZN_IsAllCompanies);
				foreach (var agreement in otherAgreementsForAllCompanies)
				{
					foreach (var item in agreement.GetItemsThatCover(itemArgs).Union(agreement.GetItemsThatAreCoveredBy(itemArgs)))
					{
						yield return item;
					}
				}
			}
		}

		#endregion

		#region FindMoreGenericItemForCompanyAutoAddCountry

		IEnumerable<CommissionAgreementItemAndCompanyAutoAddCountryConflict> GetMoreGenericCommissionAgreementCompanyAutoAddCountryConflicts(EdiCommissionAgreementCustomization customization, OrgCommissionAgreementItem subModuleItem, CommissionItemArgs itemArgs)
		{
			if (customization != null && !customization.EZN_IsAllCompanies)
			{
				foreach (var companyAutoAddCountry in customization.CompanyAutoAddCountries)
				{
					foreach (var genericItem in FindMoreGenericItemForCompanyAutoAddCountry(subModuleItem.CommissionAgreement, itemArgs, companyAutoAddCountry.EPC_LD, companyAutoAddCountry.EPC_RN_NKCountry))
					{
						yield return new CommissionAgreementItemAndCompanyAutoAddCountryConflict(subModuleItem, companyAutoAddCountry.LicenceDatabase, companyAutoAddCountry.EPC_RN_NKCountry, genericItem.CommissionAgreement);
					}
				}
			}
		}

		IEnumerable<OrgCommissionAgreementItem> FindMoreGenericItemForCompanyAutoAddCountry(OrgCommissionAgreement commissionAgreement, CommissionItemArgs itemArgs, ZGuid databasePk, ZString countryCode)
		{
			var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, true).OfType<EdiCommissionAgreement>().ToArray();
			var otherAgreementsWithSameCompanyAutoAddCountry = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllCompanies && x.Customization.CompanyAutoAddCountries.Any(y => y.EPC_LD == databasePk && y.EPC_RN_NKCountry == countryCode));
			var coveringItemsWithSameCompanyAutoAddCountry = new List<OrgCommissionAgreementItem>();
			foreach (var agreement in otherAgreementsWithSameCompanyAutoAddCountry)
			{
				coveringItemsWithSameCompanyAutoAddCountry.AddRange(agreement.GetItemsThatCover(itemArgs));
			}

			if (coveringItemsWithSameCompanyAutoAddCountry.Count > 0)
			{
				yield return GetMostSpecificCommissionAgreementItem(coveringItemsWithSameCompanyAutoAddCountry);
			}
			else
			{
				var otherAgreementsWithSameCompanyAutoAddDatabase = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllCompanies && x.Customization.CompanyAutoAddDatabases.Any(y => y.EPD_LD == databasePk));
				var coveringItemsWithSameCompanyAutoAddDatabase = new List<OrgCommissionAgreementItem>();
				foreach (var agreement in otherAgreementsWithSameCompanyAutoAddDatabase)
				{
					coveringItemsWithSameCompanyAutoAddDatabase.AddRange(agreement.GetItemsThatCover(itemArgs));
				}

				if (coveringItemsWithSameCompanyAutoAddDatabase.Count > 0)
				{
					yield return GetMostSpecificCommissionAgreementItem(coveringItemsWithSameCompanyAutoAddDatabase);
				}
				else
				{
					// agreements with a specific company have priority over agreements for 'all' companies
					var otherAgreementsForAllCompanies = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization == null || x.Customization.EZN_IsAllCompanies);
					foreach (var agreement in otherAgreementsForAllCompanies)
					{
						foreach (var item in agreement.GetItemsThatCover(itemArgs).Union(agreement.GetItemsThatAreCoveredBy(itemArgs)))
						{
							yield return item;
						}
					}
				}
			}
		}

		#endregion

		#region FindMoreGenericItemForCompanyAutoAddDatabase

		IEnumerable<CommissionAgreementItemAndCompanyAutoAddDatabaseConflict> GetMoreGenericCommissionAgreementCompanyAutoAddDatabaseConflicts(EdiCommissionAgreementCustomization customization, OrgCommissionAgreementItem subModuleItem, CommissionItemArgs itemArgs)
		{
			if (customization != null && !customization.EZN_IsAllCompanies)
			{
				foreach (var companyAutoAddDatabase in customization.CompanyAutoAddDatabases)
				{
					foreach (var genericItem in FindMoreGenericItemForCompanyAutoAddDatabase(subModuleItem.CommissionAgreement, itemArgs, companyAutoAddDatabase.EPD_LD))
					{
						yield return new CommissionAgreementItemAndCompanyAutoAddDatabaseConflict(subModuleItem, companyAutoAddDatabase.LicenceDatabase, genericItem.CommissionAgreement);
					}
				}
			}
		}

		IEnumerable<OrgCommissionAgreementItem> FindMoreGenericItemForCompanyAutoAddDatabase(OrgCommissionAgreement commissionAgreement, CommissionItemArgs itemArgs, ZGuid databasePk)
		{
			var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, true).OfType<EdiCommissionAgreement>().ToArray();
			var otherAgreementsWithSameCompanyAutoAddDatabase = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllCompanies && x.Customization.CompanyAutoAddDatabases.Any(y => y.EPD_LD == databasePk));
			var coveringItemsWithSameCompanyAutoAddDatabase = new List<OrgCommissionAgreementItem>();
			foreach (var agreement in otherAgreementsWithSameCompanyAutoAddDatabase)
			{
				coveringItemsWithSameCompanyAutoAddDatabase.AddRange(agreement.GetItemsThatCover(itemArgs));
			}

			if (coveringItemsWithSameCompanyAutoAddDatabase.Count > 0)
			{
				yield return GetMostSpecificCommissionAgreementItem(coveringItemsWithSameCompanyAutoAddDatabase);
			}
			else
			{
				// agreements with a specific company have priority over agreements for 'all' companies
				var otherAgreementsForAllCompanies = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization == null || x.Customization.EZN_IsAllCompanies);
				foreach (var agreement in otherAgreementsForAllCompanies)
				{
					foreach (var item in agreement.GetItemsThatCover(itemArgs).Union(agreement.GetItemsThatAreCoveredBy(itemArgs)))
					{
						yield return item;
					}
				}
			}
		}

		#endregion

		#region FindMoreGenericItemForDatabase

		IEnumerable<CommissionAgreementItemAndDatabaseConflict> GetMoreGenericCommissionAgreementDatabaseConflicts(EdiCommissionAgreementCustomization customization, OrgCommissionAgreementItem subModuleItem, CommissionItemArgs itemArgs)
		{
			if (customization == null || customization.EZN_IsAllDatabases)
			{
				foreach (var genericItem in FindMoreGenericItemsForAllDatabases(subModuleItem.CommissionAgreement, itemArgs))
				{
					yield return new CommissionAgreementItemAndDatabaseConflict(subModuleItem, null, genericItem.CommissionAgreement);
				}
			}
			else
			{
				foreach (var databasePivot in customization.DatabasePivots)
				{
					foreach (var genericItem in FindMoreGenericItemsForDatabase(subModuleItem.CommissionAgreement, itemArgs, databasePivot.EZD_LD))
					{
						yield return new CommissionAgreementItemAndDatabaseConflict(subModuleItem, databasePivot.LicenceDatabase, genericItem.CommissionAgreement);
					}
				}
			}
		}

		IEnumerable<OrgCommissionAgreementItem> FindMoreGenericItemsForAllDatabases(OrgCommissionAgreement commissionAgreement, CommissionItemArgs itemArgs)
		{
			var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, true).OfType<EdiCommissionAgreement>();
			var otherAgreementsForAllDatabases = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization == null || x.Customization.EZN_IsAllDatabases);
			var coveringItemsForAllDatabases = new List<OrgCommissionAgreementItem>();
			foreach (var agreement in otherAgreementsForAllDatabases)
			{
				coveringItemsForAllDatabases.AddRange(agreement.GetItemsThatCover(itemArgs));
			}

			if (coveringItemsForAllDatabases.Count > 0)
			{
				yield return GetMostSpecificCommissionAgreementItem(coveringItemsForAllDatabases);
			}
		}

		IEnumerable<OrgCommissionAgreementItem> FindMoreGenericItemsForDatabase(OrgCommissionAgreement commissionAgreement, CommissionItemArgs itemArgs, ZGuid databasePk)
		{
			var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, true).OfType<EdiCommissionAgreement>().ToArray();
			var otherAgreementsForSameDatabase = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllDatabases && x.Customization.DatabasePivots.Any(y => y.EZD_LD == databasePk));
			var coveringItemsForSameDatabase = new List<OrgCommissionAgreementItem>();
			foreach (var agreement in otherAgreementsForSameDatabase)
			{
				coveringItemsForSameDatabase.AddRange(agreement.GetItemsThatCover(itemArgs));
			}

			if (coveringItemsForSameDatabase.Count > 0)
			{
				yield return GetMostSpecificCommissionAgreementItem(coveringItemsForSameDatabase);
			}
			else
			{
				// agreements with a specific database have priority over agreements for 'all' databases
				var otherAgreementsForAllDatabases = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization == null || x.Customization.EZN_IsAllDatabases);
				foreach (var agreement in otherAgreementsForAllDatabases)
				{
					foreach (var item in agreement.GetItemsThatCover(itemArgs).Union(agreement.GetItemsThatAreCoveredBy(itemArgs)))
					{
						yield return item;
					}
				}
			}
		}

		#endregion

		#endregion

		#region FindDuplicates

		public override ICollection<ICommissionAgreementDuplication> FindDuplicates(OrgCommissionAgreementItem item)
		{
			var duplicateItems = new List<ICommissionAgreementDuplication>();
			var commissionAgreement = (EdiCommissionAgreement)item.CommissionAgreement;
			var customization = commissionAgreement.Customization;
			var itemPath = item.GetItemPath();

			var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, false).OfType<EdiCommissionAgreement>();

			duplicateItems.AddRange(FindCompanyDuplicates(customization, otherAgreementsEffectiveAtSameTime, itemPath));
			duplicateItems.AddRange(FindDatabaseDuplicates(customization, otherAgreementsEffectiveAtSameTime, itemPath));

			return duplicateItems;
		}

		static IEnumerable<ICommissionAgreementDuplication> FindCompanyDuplicates(EdiCommissionAgreementCustomization customization, IEnumerable<EdiCommissionAgreement> otherAgreementsEffectiveAtSameTime, Stack<Tuple<ZBool, ZString>> itemPath)
		{
			var isAllCompanies = customization == null || customization.EZN_IsAllCompanies;
			if (isAllCompanies)
			{
				var otherAgreementsEffectiveAtSameTimeAndAlsoAllCompanies = otherAgreementsEffectiveAtSameTime.Where(other => other.Customization == null || other.Customization.EZN_IsAllCompanies);
				foreach (var otherAgreement in otherAgreementsEffectiveAtSameTimeAndAlsoAllCompanies)
				{
					var otherAgreementDuplicateItem = otherAgreement.GetItem(itemPath.ToArray());
					if (otherAgreementDuplicateItem != null)
					{
						yield return new CommissionAgreementItemAndCompanyDuplication(otherAgreementDuplicateItem, null);
					}
				}
			}
			else
			{
				var otherAgreementsEffectiveAsSameTimeWithCustomization = otherAgreementsEffectiveAtSameTime.Where(other => other.Customization != null);
				foreach (var otherAgreement in otherAgreementsEffectiveAsSameTimeWithCustomization)
				{
					var otherAgreementDuplicateItem = otherAgreement.GetItem(itemPath.ToArray());
					if (otherAgreementDuplicateItem != null)
					{
						foreach (var companyPivot in GetOverlappingCompanyPivots(customization, otherAgreement.Customization))
						{
							yield return new CommissionAgreementItemAndCompanyDuplication(otherAgreementDuplicateItem, companyPivot.ClientCompany);
						}

						foreach (var autoAdd in GetOverlappingCompanyAutoAddCountries(customization, otherAgreement.Customization))
						{
							yield return new CommissionAgreementItemAndCompanyAutoAddCountryDuplication(otherAgreementDuplicateItem, autoAdd.LicenceDatabase, autoAdd.EPC_RN_NKCountry);
						}

						foreach (var autoAdd in GetOverlappingCompanyAutoAddDatabases(customization, otherAgreement.Customization))
						{
							yield return new CommissionAgreementItemAndCompanyAutoAddDatabaseDuplication(otherAgreementDuplicateItem, autoAdd.LicenceDatabase);
						}
					}
				}
			}
		}

		static IEnumerable<ICommissionAgreementDuplication> FindDatabaseDuplicates(EdiCommissionAgreementCustomization customization, IEnumerable<EdiCommissionAgreement> otherAgreementsEffectiveAtSameTime, Stack<Tuple<ZBool, ZString>> itemPath)
		{
			var isAllDatabases = customization == null || customization.EZN_IsAllDatabases;
			if (isAllDatabases)
			{
				var otherAgreementsEffectiveAtSameTimeAndAlsoAllDatabases = otherAgreementsEffectiveAtSameTime.Where(other => other.Customization == null || other.Customization.EZN_IsAllDatabases);
				foreach (var otherAgreement in otherAgreementsEffectiveAtSameTimeAndAlsoAllDatabases)
				{
					var otherAgreementDuplicateItem = otherAgreement.GetItem(itemPath.ToArray());
					if (otherAgreementDuplicateItem != null)
					{
						yield return new CommissionAgreementItemAndDatabaseDuplication(otherAgreementDuplicateItem, null);
					}
				}
			}
			else
			{
				var otherAgreementsEffectiveAsSameTimeWithCustomization = otherAgreementsEffectiveAtSameTime.Where(other => other.Customization != null);
				foreach (var otherAgreement in otherAgreementsEffectiveAsSameTimeWithCustomization)
				{
					foreach (var databasePivot in GetOverlappingDatabasePivots(customization, otherAgreement.Customization))
					{
						var otherAgreementDuplicateItem = otherAgreement.GetItem(itemPath.ToArray());
						if (otherAgreementDuplicateItem != null)
						{
							yield return new CommissionAgreementItemAndDatabaseDuplication(otherAgreementDuplicateItem, databasePivot.LicenceDatabase);
						}
					}
				}
			}
		}

		static IEnumerable<EdiCommissionAgreementCompanyPivot> GetOverlappingCompanyPivots(EdiCommissionAgreementCustomization x, EdiCommissionAgreementCustomization y)
		{
			var xCompanyPksSet = new HashSet<ZGuid>(x.CompanyPivots.Select(xPivot => xPivot.EPY_LCC));
			return y.CompanyPivots.Where(yPivot => xCompanyPksSet.Contains(yPivot.EPY_LCC));
		}

		static IEnumerable<EdiCommissionAgreementCompanyAutoAddCountry> GetOverlappingCompanyAutoAddCountries(EdiCommissionAgreementCustomization x, EdiCommissionAgreementCustomization y)
		{
			var xCompanyAutoAddCountriesSet = new HashSet<Tuple<ZGuid, ZString>>(x.CompanyAutoAddCountries.Select(xAutoAdd => Tuple.Create(xAutoAdd.EPC_LD, xAutoAdd.EPC_RN_NKCountry)));
			return y.CompanyAutoAddCountries.Where(yAutoAdd => xCompanyAutoAddCountriesSet.Contains(Tuple.Create(yAutoAdd.EPC_LD, yAutoAdd.EPC_RN_NKCountry)));
		}

		static IEnumerable<EdiCommissionAgreementCompanyAutoAddDatabase> GetOverlappingCompanyAutoAddDatabases(EdiCommissionAgreementCustomization x, EdiCommissionAgreementCustomization y)
		{
			var xCompanyAutoAddDatabasesSet = new HashSet<ZGuid>(x.CompanyAutoAddDatabases.Select(xAutoAdd => xAutoAdd.EPD_LD));
			return y.CompanyAutoAddDatabases.Where(yAutoAdd => xCompanyAutoAddDatabasesSet.Contains(yAutoAdd.EPD_LD));
		}

		static IEnumerable<EdiCommissionAgreementDatabasePivot> GetOverlappingDatabasePivots(EdiCommissionAgreementCustomization x, EdiCommissionAgreementCustomization y)
		{
			var xDatabasePksSet = new HashSet<ZGuid>(x.DatabasePivots.Select(xPivot => xPivot.EZD_LD));
			return y.DatabasePivots.Where(yPivot => xDatabasePksSet.Contains(yPivot.EZD_LD));
		}

		#endregion

		#region GetMoreSpecificCommissionAgreementConflicts

		public override IEnumerable<ICommissionAgreementConflict> GetMoreSpecificCommissionAgreementConflicts(OrgCommissionAgreementItem commissionAgreementItem)
		{
			var conflicts = new List<EdiCommissionAgreementConflict>();
			var ediCommissionAgreement = commissionAgreementItem.CommissionAgreement as EdiCommissionAgreement;

			if (ediCommissionAgreement != null)
			{
				var itemArgs = GetItemPathCodesForConflictChecking(commissionAgreementItem);
				if (itemArgs != null)
				{
					conflicts.AddRange(GetMoreSpecificCommissionAgreementCompanyConflicts(ediCommissionAgreement.Customization, commissionAgreementItem, itemArgs));

					conflicts.AddRange(GetMoreSpecificCommissionAgreementCompanyAutoAddCountryConflicts(ediCommissionAgreement.Customization, commissionAgreementItem, itemArgs));

					conflicts.AddRange(GetMoreSpecificCommissionAgreementCompanyAutoAddDatabaseConflicts(ediCommissionAgreement.Customization, commissionAgreementItem, itemArgs));

					conflicts.AddRange(GetMoreSpecificCommissionAgreementDatabaseConflicts(ediCommissionAgreement.Customization, commissionAgreementItem, itemArgs));
				}
			}

			return conflicts;
		}

		#region FindMoreSpecificItemForCompany

		IEnumerable<CommissionAgreementItemAndCompanyConflict> GetMoreSpecificCommissionAgreementCompanyConflicts(EdiCommissionAgreementCustomization customization, OrgCommissionAgreementItem subModuleItem, CommissionItemArgs itemArgs)
		{
			if (customization == null || customization.EZN_IsAllCompanies)
			{
				var commissionAgreement = subModuleItem.CommissionAgreement;
				var product = itemArgs.Product;
				var service = itemArgs.Service;
				var subModule = itemArgs.SubModule;

				var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, true).OfType<EdiCommissionAgreement>();
				var otherAgreementsForAllCompanies = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization == null || x.Customization.EZN_IsAllCompanies);
				var coveringItemsForAllCompanies = new List<OrgCommissionAgreementItem>();

				foreach (var agreement in otherAgreementsForAllCompanies)
				{
					foreach (var specificItem in agreement.GetItemsThatAreCoveredBy(itemArgs))
					{
						yield return new CommissionAgreementItemAndCompanyConflict(subModuleItem, null, specificItem.CommissionAgreement, specificItem);
					}
				}

				var otherAgreementsForSpecificCompanies = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllCompanies);
				foreach (var agreement in otherAgreementsForSpecificCompanies)
				{
					foreach (var companyPivot in agreement.Customization.CompanyPivots)
					{
						foreach (var specificItem in agreement.GetItemsThatCover(itemArgs).Union(agreement.GetItemsThatAreCoveredBy(itemArgs)))
						{
							yield return new CommissionAgreementItemAndCompanyConflict(subModuleItem, companyPivot.ClientCompany, specificItem.CommissionAgreement, specificItem);
						}
					}
				}
			}
			else
			{
				foreach (var companyPivot in customization.CompanyPivots)
				{
					foreach (var specificItem in FindMoreSpecificItemForCompany(subModuleItem.CommissionAgreement, itemArgs, companyPivot.EPY_LCC))
					{
						yield return new CommissionAgreementItemAndCompanyConflict(subModuleItem, companyPivot.ClientCompany, specificItem.CommissionAgreement, specificItem);
					}
				}
			}
		}

		IEnumerable<OrgCommissionAgreementItem> FindMoreSpecificItemForCompany(OrgCommissionAgreement commissionAgreement, CommissionItemArgs itemArgs, ZGuid companyPk)
		{
			var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, true).OfType<EdiCommissionAgreement>().ToArray();
			var otherAgreementsForSameCompany = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllCompanies && x.Customization.CompanyPivots.Any(y => y.EPY_LCC == companyPk));
			var coveringItemsForSameCompany = new List<OrgCommissionAgreementItem>();

			foreach (var agreement in otherAgreementsForSameCompany)
			{
				coveringItemsForSameCompany.AddRange(agreement.GetItemsThatAreCoveredBy(itemArgs));
			}

			return coveringItemsForSameCompany;
		}

		#endregion

		#region FindMoreSpecificItemForCompanyAutoAddCountry

		IEnumerable<CommissionAgreementItemAndCompanyAutoAddCountryConflict> GetMoreSpecificCommissionAgreementCompanyAutoAddCountryConflicts(EdiCommissionAgreementCustomization customization, OrgCommissionAgreementItem subModuleItem, CommissionItemArgs itemArgs)
		{
			var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(subModuleItem.CommissionAgreement, true).OfType<EdiCommissionAgreement>().ToArray();
			var product = itemArgs.Product;
			var service = itemArgs.Service;
			var subModule = itemArgs.SubModule;

			if (customization == null || customization.EZN_IsAllCompanies)
			{
				var otherAgreementsWithCustomization = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllCompanies);
				foreach (var otherAgreement in otherAgreementsWithCustomization)
				{
					foreach (var otherCompanyAutoAddCountry in otherAgreement.Customization.CompanyAutoAddCountries)
					{
						foreach (var otherItem in otherAgreement.GetItemsThatCover(itemArgs).Union(otherAgreement.GetItemsThatAreCoveredBy(itemArgs)))
						{
							yield return new CommissionAgreementItemAndCompanyAutoAddCountryConflict(subModuleItem, otherCompanyAutoAddCountry.LicenceDatabase, otherCompanyAutoAddCountry.EPC_RN_NKCountry, otherItem.CommissionAgreement, otherItem);
						}
					}
				}
			}
			else if (customization != null && !customization.EZN_IsAllCompanies)
			{
				foreach (var autoAddDatabase in customization.CompanyAutoAddDatabases)
				{
					var otherAgreementsWithCustomization = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllCompanies);
					foreach (var otherAgreement in otherAgreementsWithCustomization)
					{
						foreach (var otherCompanyAutoAddCountry in otherAgreement.Customization.CompanyAutoAddCountries.Where(y => y.EPC_LD == autoAddDatabase.EPD_LD))
						{
							foreach (var otherItem in otherAgreement.GetItemsThatAreCoveredBy(itemArgs))
							{
								yield return new CommissionAgreementItemAndCompanyAutoAddCountryConflict(subModuleItem, otherCompanyAutoAddCountry.LicenceDatabase, otherCompanyAutoAddCountry.EPC_RN_NKCountry, otherItem.CommissionAgreement, otherItem);
							}
						}
					}
				}

				foreach (var autoAutoAddCountry in customization.CompanyAutoAddCountries)
				{
					var otherAgreementsWithCustomization = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllCompanies);
					foreach (var otherAgreement in otherAgreementsWithCustomization)
					{
						foreach (var otherCompanyAutoAddCountry in otherAgreement.Customization.CompanyAutoAddCountries.Where(y => y.EPC_LD == autoAutoAddCountry.EPC_LD && y.EPC_RN_NKCountry == autoAutoAddCountry.EPC_RN_NKCountry))
						{
							foreach (var otherItem in otherAgreement.GetItemsThatAreCoveredBy(itemArgs))
							{
								yield return new CommissionAgreementItemAndCompanyAutoAddCountryConflict(subModuleItem, otherCompanyAutoAddCountry.LicenceDatabase, otherCompanyAutoAddCountry.EPC_RN_NKCountry, otherItem.CommissionAgreement, otherItem);
							}
						}
					}
				}
			}
		}

		#endregion

		#region FindMoreSpecificItemForCompanyAutoAddDatabase

		IEnumerable<CommissionAgreementItemAndCompanyAutoAddDatabaseConflict> GetMoreSpecificCommissionAgreementCompanyAutoAddDatabaseConflicts(EdiCommissionAgreementCustomization customization, OrgCommissionAgreementItem subModuleItem, CommissionItemArgs itemArgs)
		{
			var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(subModuleItem.CommissionAgreement, true).OfType<EdiCommissionAgreement>().ToArray();
			var product = itemArgs.Product;
			var service = itemArgs.Service;
			var subModule = itemArgs.SubModule;

			if (customization == null || customization.EZN_IsAllCompanies)
			{
				var otherAgreementsWithCustomization = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllCompanies);
				foreach (var otherAgreement in otherAgreementsWithCustomization)
				{
					foreach (var otherCompanyAutoAddDatabase in otherAgreement.Customization.CompanyAutoAddDatabases)
					{
						foreach (var otherItem in otherAgreement.GetItemsThatCover(itemArgs).Union(otherAgreement.GetItemsThatAreCoveredBy(itemArgs)))
						{
							yield return new CommissionAgreementItemAndCompanyAutoAddDatabaseConflict(subModuleItem, otherCompanyAutoAddDatabase.LicenceDatabase, otherItem.CommissionAgreement, otherItem);
						}
					}
	}
}
			else if (customization != null && !customization.EZN_IsAllCompanies)
			{
				foreach (var autoAddDatabase in customization.CompanyAutoAddDatabases)
				{
					var otherAgreementsWithCustomization = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllCompanies);
					foreach (var otherAgreement in otherAgreementsWithCustomization)
					{
						foreach (var otherCompanyAutoAddDatabase in otherAgreement.Customization.CompanyAutoAddDatabases.Where(y => y.EPD_LD == autoAddDatabase.EPD_LD))
						{
							foreach (var otherItem in otherAgreement.GetItemsThatAreCoveredBy(itemArgs))
							{
								yield return new CommissionAgreementItemAndCompanyAutoAddDatabaseConflict(subModuleItem, otherCompanyAutoAddDatabase.LicenceDatabase, otherItem.CommissionAgreement, otherItem);
							}
						}
					}
				}
			}
		}

		#endregion

		#region FindMoreSpecificItemForDatabase

		IEnumerable<CommissionAgreementItemAndDatabaseConflict> GetMoreSpecificCommissionAgreementDatabaseConflicts(EdiCommissionAgreementCustomization customization, OrgCommissionAgreementItem subModuleItem, CommissionItemArgs itemArgs)
		{
			if (customization == null || customization.EZN_IsAllDatabases)
			{
				var commissionAgreement = subModuleItem.CommissionAgreement;
				var product = itemArgs.Product;
				var service = itemArgs.Service;
				var subModule = itemArgs.SubModule;

				var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, true).OfType<EdiCommissionAgreement>();
				var otherAgreementsForAllDatabases = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization == null || x.Customization.EZN_IsAllDatabases);
				var coveringItemsForAllDatabases = new List<OrgCommissionAgreementItem>();

				foreach (var agreement in otherAgreementsForAllDatabases)
				{
					foreach (var specificItem in agreement.GetItemsThatAreCoveredBy(itemArgs))
					{
						yield return new CommissionAgreementItemAndDatabaseConflict(subModuleItem, null, specificItem.CommissionAgreement, specificItem);
					}
				}

				var otherAgreementsForSpecificDatabases = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllDatabases);
				foreach (var agreement in otherAgreementsForSpecificDatabases)
				{
					foreach (var databasePivot in agreement.Customization.DatabasePivots)
					{
						foreach (var specificItem in agreement.GetItemsThatCover(itemArgs).Union(agreement.GetItemsThatAreCoveredBy(itemArgs)))
						{
							yield return new CommissionAgreementItemAndDatabaseConflict(subModuleItem, databasePivot.LicenceDatabase, specificItem.CommissionAgreement, specificItem);
						}
					}
				}
			}
			else
			{
				foreach (var databasePivot in customization.DatabasePivots)
				{
					foreach (var specificItem in FindMoreSpecificItemsForDatabase(subModuleItem.CommissionAgreement, itemArgs, databasePivot.EZD_LD))
					{
						yield return new CommissionAgreementItemAndDatabaseConflict(subModuleItem, databasePivot.LicenceDatabase, specificItem.CommissionAgreement, specificItem);
					}
				}
			}
		}

		IEnumerable<OrgCommissionAgreementItem> FindMoreSpecificItemsForDatabase(OrgCommissionAgreement commissionAgreement, CommissionItemArgs itemArgs, ZGuid databasePk)
		{
			var otherAgreementsEffectiveAtSameTime = GetOtherAgreementsEffectiveAtSameTime(commissionAgreement, true).OfType<EdiCommissionAgreement>().ToArray();
			var otherAgreementsForSameDatabase = otherAgreementsEffectiveAtSameTime.Where(x => x.Customization != null && !x.Customization.EZN_IsAllDatabases && x.Customization.DatabasePivots.Any(y => y.EZD_LD == databasePk));
			var coveringItemsForSameDatabase = new List<OrgCommissionAgreementItem>();

			foreach (var agreement in otherAgreementsForSameDatabase)
			{
				coveringItemsForSameDatabase.AddRange(agreement.GetItemsThatAreCoveredBy(itemArgs));
			}

			return coveringItemsForSameDatabase;
		}

		#endregion

		#endregion
	}
}

