using System.Collections.Generic;
using System.Linq;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaManifestHeaderDataObjectWriterHelper : AsycudaManifestUniversalCommonHelper
	{
		public AsycudaManifestHeaderDataObjectWriterHelper(AsycudaManifestHeader header)
			: base(header.Factory)
		{
			Header = header;
			CountryCode = Header.AMA_RN_NKCountry;
		}

		public readonly AsycudaManifestHeader Header;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IEnumerable<KeyValuePair<ZString, ZString>> GetHeaderAdditionalAddInfos(AsycudaManifestHeader header)
		{
			if (header != null)
			{
				foreach (var pair in GetHeaderAdditionalAddInfosCore(header))
				{
					yield return pair;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual IEnumerable<KeyValuePair<ZString, ZString>> GetHeaderAdditionalAddInfosCore(AsycudaManifestHeader header)
		{
			return System.Array.Empty<KeyValuePair<ZString, ZString>>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public virtual IEnumerable<Date> GetHeaderAdditionalDateAddInfos(AsycudaManifestHeader header)
		{
			return header != null ? GetHeaderAdditionalDateAddInfosCore(header) : Enumerable.Empty<Date>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual IEnumerable<Date> GetHeaderAdditionalDateAddInfosCore(AsycudaManifestHeader header)
		{
			return Enumerable.Empty<Date>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IEnumerable<KeyValuePair<ZString, ZString>> GetBillCountryAdditionalAddInfos<TCountry>(TCountry billCountry)
			where TCountry : AsycudaBill
		{
			if (billCountry != null)
			{
				foreach (var pair in GetBillCountryAdditionalAddInfosCore(billCountry))
				{
					yield return pair;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual IEnumerable<KeyValuePair<ZString, ZString>> GetBillCountryAdditionalAddInfosCore<TCountry>(TCountry bill)
		   where TCountry : AsycudaBill
		{
			return System.Array.Empty<KeyValuePair<ZString, ZString>>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IEnumerable<KeyValuePair<ZString, ZString>> GetPackedItemAdditionalAddInfos<TPackedItem>(TPackedItem packedItem)
			where TPackedItem : AsycudaPackedItem
		{
			if (packedItem != null)
			{
				foreach (var pair in GetPackedItemAdditionalAddInfosCore(packedItem))
				{
					yield return pair;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual IEnumerable<KeyValuePair<ZString, ZString>> GetPackedItemAdditionalAddInfosCore<TPackedItem>(TPackedItem packedItem)
			where TPackedItem : AsycudaPackedItem
		{
			return System.Array.Empty<KeyValuePair<ZString, ZString>>();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		public IEnumerable<KeyValuePair<ZString, ZString>> GetBillCountryEntryInstructionAdditionalAddInfos<TCountry>(TCountry billCountry)
			where TCountry : AsycudaBill
		{
			if (billCountry != null)
			{
				foreach (var pair in GetBillCountryEntryInstructionAdditionalAddInfosCore(billCountry))
				{
					yield return pair;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
		protected virtual IEnumerable<KeyValuePair<ZString, ZString>> GetBillCountryEntryInstructionAdditionalAddInfosCore<TCountry>(TCountry billCountry)
			where TCountry : AsycudaBill
		{
			return System.Array.Empty<KeyValuePair<ZString, ZString>>();
		}

		#region List

		public ICodeDescriptionPairList ForwardingTransportTypeList
		{
			get { return factory.GetCachedCodeDescriptionPairList(OLookUpEditType.TransportType); }
		}

		public CustomsChargeTypeList CustomsChargeTypeList
		{
			get
			{
				return factory.GetCachedValue("AsycudaCustomsChargeTypeList", () =>
				{
					var result = new CustomsChargeTypeList();
					result.AddPair(Constants.CustomsChargeType.CustomsChargeCode, Constants.CustomsChargeType.CustomsChargeDescription);
					result.AddPair(Constants.CustomsChargeType.GSTCode, Constants.CustomsChargeType.GSTDescription);
					result.AddPair(Constants.CustomsChargeType.CustomsDutyCode, Constants.CustomsChargeType.CustomsDutyCodeDescription);
					return result;
				});
			}
		}

		public WayBillTypeList WayBillTypeList
		{
			get { return factory.GetCachedValue<WayBillTypeList>(); }
		}

		public Freight.Common.Business.BindToLists BindToLists
		{
			get { return Freight.Common.Business.BindToLists.GetCachedLists(factory); }
		}

		#endregion

		#region EntryInstructionLink

		internal ZInt? AllocateEntryInstructionLink(ZGuid sourcePK)
		{
			ZInt? result = null;
			if (sourcePK.IsValid)
			{
				if (entryInstructionLinkMap.ContainsKey(sourcePK))
				{
					result = entryInstructionLinkMap[sourcePK];
				}
				else
				{
					result = entryInstructionLinkMap.Count + 1;
					entryInstructionLinkMap[sourcePK] = (int)result;
				}
			}
			return result;
		}
		readonly Dictionary<ZGuid, int> entryInstructionLinkMap = new Dictionary<ZGuid, int>();

		internal void ClearEntryInstructionLinkMap()
		{
			entryInstructionLinkMap.Clear();
		}

		#endregion

		public readonly ZString CountryCode;
	}
}
