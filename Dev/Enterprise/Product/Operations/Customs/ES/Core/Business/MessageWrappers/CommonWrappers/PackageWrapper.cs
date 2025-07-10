using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class PackageWrapper : PackageCommonWrapper, IPackageCommonNumbers
	{
		public PackageWrapper(ZString packageType, ZString packageMarks, ZInt packagesQty, ZInt piecesQty) : base(packageType, packageMarks)
		{
			PackagesQty = packagesQty;
			PiecesQty = piecesQty;
		}

		public ZInt PackagesQty { get; }

		public ZInt PiecesQty { get; }

		readonly static ZString FramesMarks = "BASTIDORES";

		public static IReadOnlyCollection<PackageWrapper> GetPackagesList(CusEntryLine cusEntryLine)
		{
			var packages = new List<PackageWrapper>();

			var vehicleqty = cusEntryLine.InvoiceLines.Cast<JobComInvoiceLine>().Sum(x => x.Vehicles.Count);
			if (vehicleqty > 0)
			{
				packages.Add(new PackageWrapper(RefCusCodeList.PackageType.Frame, FramesMarks, vehicleqty, 0));
			}

			var packagingDetails = cusEntryLine.PackagingDetails.GroupBy(x => new { x.Package.CW_PackType, x.Package.CW_MarksAndNos });
			foreach (var pack in packagingDetails)
			{
				var packType = pack.Key.CW_PackType;
				var packqty = ZInt.Zero;
				var piecesqty = ZInt.Zero;
				foreach (var p in pack.ToArray())
				{
					if (packType != EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked)
					{
						packqty += p.CHC_NumberOfPacks;
					}
					else
					{
						piecesqty += p.CHC_NumberOfPacks;
					}
				}
				packages.Add(new PackageWrapper(packType, pack.Key.CW_MarksAndNos, packqty, piecesqty));
			}
			return packages.AsReadOnly();
		}
	}
}
