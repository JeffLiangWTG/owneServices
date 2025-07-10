using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers
{
	public class DVDCommonPackageWrapper : PackageCommonWrapper, IDVDCommonPackage
	{
		public DVDCommonPackageWrapper(ZString packageType, ZString packageMarks, ZInt packagesQty) : base(packageType, packageMarks)
		{
			NumberOfPackages = packagesQty;
		}

		public ZInt NumberOfPackages { get; }

		public static IReadOnlyCollection<DVDCommonPackageWrapper> GetPackagesList(CusEntryLine cusEntryLine, bool canSendFRPackages = false, bool canSendNEPackageQty = true)
		{
			var packages = new List<DVDCommonPackageWrapper>();

			var packagingDetails = cusEntryLine.PackagingDetails.GroupBy(x => new { x.Package.CW_PackType, x.Package.CW_MarksAndNos });
			foreach (var pack in packagingDetails)
			{
				var packType = pack.Key.CW_PackType;

				if (canSendFRPackages)
				{
					packages.Add(GetPackage(pack.ToArray(), packType, pack.Key.CW_MarksAndNos, cusEntryLine.Factory, canSendNEPackageQty));
				}
				else if (packType != RefCusCodeList.PackageType.Frame)
				{
					packages.Add(GetPackage(pack.ToArray(), packType, pack.Key.CW_MarksAndNos, cusEntryLine.Factory, canSendNEPackageQty));
				}
			}

			return packages.AsReadOnly();
		}

		static DVDCommonPackageWrapper GetPackage(Customs.Business.InvoiceLinePackagePivot[] packArray, ZString packType, ZString marks, BusinessObjectFactory factory, bool canSendNEPackageQty = true)
		{
			var packqty = ZInt.Zero;
			if (packType != EU.Business.UniversalReferenceConstants.RefCusCodeUnPackedPackageUnitType.Unpacked || canSendNEPackageQty)
			{
				var isBulkType = PackageHelper.PackTypeIsBulk(packType, factory);
				foreach (var p in packArray)
				{
					if (!isBulkType)
					{
						packqty += p.CHC_NumberOfPacks;
					}
				}
			}
			return new DVDCommonPackageWrapper(packType, marks, packqty);
		}
	}
}
