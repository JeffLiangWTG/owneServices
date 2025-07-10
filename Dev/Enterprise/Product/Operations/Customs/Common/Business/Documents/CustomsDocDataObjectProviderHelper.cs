using System.Collections;
using System.Collections.Immutable;
using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Messaging.Integration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.Common.Shared
{
	public static class CustomsDocDataObjectProviderHelper
	{
		public static ICustomsDocDataObjectProvider GetProvider(string docDataObjectProviderSourceDictionaryKey, string dataContext, string businessObjectName = null)
		{
			var objectname = businessObjectName ?? string.Empty;
			var providerKey = ResolveProviderKey(docDataObjectProviderSourceDictionaryKey, dataContext, objectname);

			var providerDict = ObjectFactory.Get<Hashtable>(nameof(ICustomsDocDataObjectProvider));
			var handle = (ObjectHandle)providerDict[providerKey];
			return handle?.GetObject() as ICustomsDocDataObjectProvider;
		}

		#region Implementation

		static string ResolveProviderKey(string docDataObjectProviderSourceDictionaryKey, string dataContext, string businessObjectName)
		{
			if (docDataObjectProviderSourceDictionaryKey == NctsProviderKey)
			{
				return GetProviderKeyForNcts(docDataObjectProviderSourceDictionaryKey, dataContext, businessObjectName);
			}

			return dataContext == DocumentVisualizer.Integration.DataContext.CMRWayBill ? GetProviderKeyForJobDeclaration(docDataObjectProviderSourceDictionaryKey) : GetProviderKeyForEntryHeader(docDataObjectProviderSourceDictionaryKey);
		}

		static string GetProviderKeyForEntryHeader(string docDataObjectProviderSourceDictionaryKey)
		{
			if (!euCountriesWithEntryHeaderCustomProviderCollection.Contains(docDataObjectProviderSourceDictionaryKey)
				&& ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(docDataObjectProviderSourceDictionaryKey))
			{
				return CountryCodes.EuropeanUnion;
			}

			return docDataObjectProviderSourceDictionaryKey;
		}

		static string GetProviderKeyForJobDeclaration(string docDataObjectProviderSourceDictionaryKey)
		{
			if (!euCountriesWithJobDeclarationCustomProviderCollection.Contains(docDataObjectProviderSourceDictionaryKey)
				&& ObjectFactory.Get<Integration.Customs.Shared.IEuropeanUnionCustomsMembersProvider>().IsInEuropeanCustomsUnionOrInheritsFromEU(docDataObjectProviderSourceDictionaryKey))
			{
				return $"{CountryCodes.EuropeanUnion}.{JobDeclarationKey}";
			}

			return $"{docDataObjectProviderSourceDictionaryKey}.{JobDeclarationKey}";
		}

		static string GetProviderKeyForNcts(string docDataObjectProviderSourceDictionaryKey, string dataContext, string businessObjectName) => dataContext.StartsWith(GenericMessageDeliveryInterchangeTypeList.Codes.FRPorts) && (businessObjectName == NctsBusinessObjectName) ? "FR.NCTS" : docDataObjectProviderSourceDictionaryKey;

		const string NctsProviderKey = "NCTS";
		const string NctsBusinessObjectName = "NctsHeader";
		const string ProviderKeyForJobDeclaration = "EU.JobDeclaration";
		const string JobDeclarationKey = "JobDeclaration";

		static readonly ImmutableArray<ZString> euCountriesWithEntryHeaderCustomProviderCollection = new ZString[]
		{
			CountryCodes.France,
			CountryCodes.Italy,
			CountryCodes.Spain
		}.ToImmutableArray();

		static readonly ImmutableArray<ZString> euCountriesWithJobDeclarationCustomProviderCollection = new ZString[]
		{
			CountryCodes.Germany
		}.ToImmutableArray();

		#endregion
	}
}
