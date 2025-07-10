using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class EXSPackageWrapper : PackageCommonWrapper, IEXSPackage
	{
		public EXSPackageWrapper(ZString packageType, ZString packageMarks, ZInt packagesQty, ZBool isPackTypeBulk) : base(packageType, packageMarks)
		{
			PackagesQty = packagesQty;
			IsPackTypeBulk = isPackTypeBulk;
		}

		public ZInt PackagesQty { get; }

		public ZBool IsPackTypeBulk { get; }

		readonly static ZString FramesMarks = "BASTIDORES";

		public static IReadOnlyCollection<EXSPackageWrapper> GetPackagesList(CusEntryLine cusEntryLine)
		{
			var packages = new List<EXSPackageWrapper>();

			var vehicleqty = cusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.Vehicles.Count);
			if (vehicleqty > 0)
			{
				packages.Add(new EXSPackageWrapper(RefCusCodeList.PackageType.Frame, FramesMarks, vehicleqty, false));
			}

			var packagingDetails = cusEntryLine.PackagingDetails.GroupBy(x => new { x.Package.CW_PackType, x.Package.CW_MarksAndNos });
			foreach (var pack in packagingDetails)
			{
				var packType = pack.Key.CW_PackType;
				var packqty = ZInt.Zero;
				foreach (var p in pack.ToArray())
				{
					if (!PackageHelper.PackTypeIsBulk(packType, cusEntryLine.Factory))
					{
						packqty += p.CHC_NumberOfPacks;
					}
				}
				packages.Add(new EXSPackageWrapper(packType, pack.Key.CW_MarksAndNos, packqty, PackageHelper.PackTypeIsBulk(packType, cusEntryLine.Factory)));
			}

			return packages.AsReadOnly();
		}
	}
}
