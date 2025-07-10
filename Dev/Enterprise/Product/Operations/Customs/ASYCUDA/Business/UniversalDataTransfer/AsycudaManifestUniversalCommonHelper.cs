using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer.Universal;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer
{
	public class AsycudaManifestUniversalCommonHelper : UniversalCommonHelper
	{
		protected AsycudaManifestUniversalCommonHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public IEnumerable<GenAddOnDetail> GetAsycudaBillGenAddOnColumnList(AsycudaBill countryBO) => GetAsycudaBillGenAddOnColumnListCore(countryBO);
		protected virtual IEnumerable<GenAddOnDetail> GetAsycudaBillGenAddOnColumnListCore(AsycudaBill billBO)
		{
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(billBO.DutyAmount), AddInfoKey = AddInfoConstants.Bill.DutyAmount, GenAddOnColumnName = AsycudaBill.Schema.DutyAmount, PropertyName = AsycudaBill.Schema.DutyAmount };
			yield return new GenAddOnDetail { TypeCode = AddOnColumnDataType.GetCodeFromObject(billBO.TaxAmount), AddInfoKey = AddInfoConstants.Bill.TaxAmount, GenAddOnColumnName = AsycudaBill.Schema.TaxAmount, PropertyName = AsycudaBill.Schema.TaxAmount };
		}

		public IEnumerable<GenAddOnDetail> GetAsycudaManifestHeaderGenAddOnColumnList(AsycudaManifestHeader headerBO) => GetAsycudaManifestHeaderGenAddOnColumnListCore(headerBO);
		protected virtual IEnumerable<GenAddOnDetail> GetAsycudaManifestHeaderGenAddOnColumnListCore(AsycudaManifestHeader baseHeaderBO)
		{
			return System.Array.Empty<GenAddOnDetail>();
		}

		public IEnumerable<GenAddOnDetail> GetAsycudaPackedItemGenAddOnColumnList(AsycudaPackedItem packedItem) => GetAsycudaPackedItemGenAddOnColumnListCore(packedItem);
		protected virtual IEnumerable<GenAddOnDetail> GetAsycudaPackedItemGenAddOnColumnListCore(AsycudaPackedItem packedItem)
		{
			return System.Array.Empty<GenAddOnDetail>();
		}

		public IDictionary<ZString, ZString> GetPackedItemEntryNumberMapping() => GetPackedItemEntryNumberMappingCore();
		protected virtual IDictionary<ZString, ZString> GetPackedItemEntryNumberMappingCore()
		{
			var result = new Dictionary<ZString, ZString>();
			result.Add(AddInfoConstants.PackedItem.CustomsNumber, Constants.CustomsEntryType.ACCESSPermit);
			return result;
		}

		public IEnumerable<CusSupportingInfo> GetHeaderCustomsSupportingInfos(AsycudaManifestHeader header)
		{
			return header != null ? GetHeaderCustomsSupportingInfosCore(header) : Enumerable.Empty<CusSupportingInfo>();
		}

		protected virtual IEnumerable<CusSupportingInfo> GetHeaderCustomsSupportingInfosCore(AsycudaManifestHeader header)
		{
			return Enumerable.Empty<CusSupportingInfo>();
		}

		public IEnumerable<CusSupportingInfo> GetBillCustomsSupportingInfos(AsycudaBill bill)
		{
			return bill != null ? GetBillCustomsSupportingInfosCore(bill) : Enumerable.Empty<CusSupportingInfo>();
		}

		protected virtual IEnumerable<CusSupportingInfo> GetBillCustomsSupportingInfosCore(AsycudaBill bill)
		{
			return Enumerable.Empty<CusSupportingInfo>();
		}
	}
}
