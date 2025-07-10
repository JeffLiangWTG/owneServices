using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Core.Modules;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Services.OperationalActions.Business
{
	static class CollectionTypeDecider
	{
		public static Type GuessCollectionTypeFromTablePrefix(string prefix)
		{
			if (IsPrefixOnTheBlackList(prefix))
			{
				return null;
			}

			var result = GetCollectionTypeByDirectlyMappedPrefix(prefix);
			if (result == null)
			{
				var currentCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				using (var module = GetZModuleByColumnNamePrefix(prefix, currentCountryCode))
				{
					var zFilterModule = module as IZFilterModule;
					if (zFilterModule != null)
					{
						result = zFilterModule.GetNewBusinessObjectCollection().GetType();

						if (result != null && !DoesGuessedCollectionTypeMakeSense(result))
						{
							result = null;
						}
					}
				}
			}
			return result;
		}
		public static IZModule GetZModuleByColumnNamePrefix(string prefix, string countryOverride = null)
		{
			var moduleId = (new ModuleList()).GetRegisteredIdentifierByColumnNamePrefix(prefix, countryOverride);
			return moduleId != null ? ObjectFactory.Get<IModuleFactory>().Create(moduleId, countryOverride) : null;
		}

		static bool IsPrefixOnTheBlackList(string prefix)
		{
			//we exclude these prefixes because otherwise we would get the following errors by GeneralActionsTest.TestSanityCheck():
			switch (prefix)
			{
				case JobShipmentSchema.Constants.Prefix: return true; //"You can only create one ForwardingModuleShipmentCollection per factory"
				case JobConsolSchema.Constants.Prefix: return true; //"You can only create one ForwardingModuleConsolCollection per factory"
				case ZZRefCusCodeListCombinedSchema.Constants.Prefix: return true; //"It is a view and shares ZZRefCusCodeList table prefix"
				case ZZRefCusRulingCombinedSchema.Constants.Prefix: return true; //"It is a view and shares ZZRefCusRuling table prefix"
				case CusPersonSchema.Constants.Prefix: return true; //"It is a view of GlbPerson"
				case WhsAdHocServiceJobSchema.Constants.Prefix: return true; //"Table name missmatch, should be WhsAdHocServiceJob but was WorkItem" - table unused right now but will be used very soon
				case ZZRefCarrierCombinedSchema.Constants.Prefix: return true; //"It is a view and shares ZZRefCarrier table prefix"
				default: return false;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		static bool DoesGuessedCollectionTypeMakeSense(Type collectionType)
		{
			bool result;
			IBusinessObjectCollection collection = null;
			try
			{
				var providerFactory = new BusinessObjectCollectionProviderFactory();
				var factory = new BusinessObjectFactory();
				collection = providerFactory.Get(collectionType)(factory);
				result = collection != null;
			}
			catch (ArgumentNullException)
			{
				result = false;
			}
			catch (ArgumentOutOfRangeException)
			{
				result = false;
			}

			if (result)
			{
				ModuleIdentifier identifier = ZMetaData.GetModuleId(collection);
				//some modules returns specific collections not assigned to any modules (e.g. GlbCompanyCampaignItemModule returns GlbCompanyCampaignItemCampaignDependentCollection instead of GlbCompanyCampaignItemCollection)
				//we ignore such collections because we want to have a module to pick up values from the guessed collection
				result = identifier != null && identifier != ModuleIDs.NotAssigned;
			}
			return result;
		}

		static Type GetCollectionTypeByDirectlyMappedPrefix(string prefix)
		{
			switch (prefix)
			{
				case OrgHeaderSchema.Constants.Prefix: return typeof(OrganisationsFindBoxCollection);
				case OrgAddressSchema.Constants.Prefix: return typeof(OrgAddressCollection);
				case OrgContactSchema.Constants.Prefix: return typeof(OrgContactCollection);
				case OrgOpportunitySchema.Constants.Prefix: return typeof(OrgOpportunityCollection);
				case RefServiceLevelSchema.Constants.Prefix: return typeof(RefServiceLevelCollection);
				case GlbStaffSchema.Constants.Prefix: return typeof(GlbStaffCollection);

				default: return null;
			}
		}
	}
}
