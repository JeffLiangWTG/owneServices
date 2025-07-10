using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class LinePackageProvider : IPackage
	{
		public static LinePackageProvider NewOrNull(InvoiceLinePackagePivot pivot) => pivot == null ? null : new LinePackageProvider(pivot);

		LinePackageProvider(InvoiceLinePackagePivot pivot)
		{
			this.pivot = pivot;
		}
		readonly InvoiceLinePackagePivot pivot;

		public int? Quantity => pivot.CHC_NumberOfPacks;

		public bool IsSupportEmptyPackType => PackageHelper.IsSupportEmptyPackType(pivot.Package.Factory, pivot.Package.CW_PackType);

		public string Kind => pivot.Package.CW_PackType;

		public string MarksNumbers => pivot.Package.CW_MarksAndNos;

		public int PositionNumber => CachedValueHelper.GetValue(ref positionNumberCached, () =>
		{
			int result = 0;
			if (pivot.CHC_NumberOfPacks.IsEmpty)
			{
				var package = pivot.Package;
				var mainPackageInvoiceLine = package.InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().Select(x => x.InvoiceLine).Cast<JobComInvoiceLine>().FirstOrDefault(x => x.JI_IsMainPack);
				result = mainPackageInvoiceLine?.CusEntryLine?.CL_LineNumber.ToZInt() ?? 0;
			}
			return result;
		});
		CachedValue<int> positionNumberCached;
	}
}
