using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class T2LPOUSRequestAndReceptionPackagingWrapper : T2LPOUSCommonPackagingWrapper, IT2LPOUSRequestAndReceptionPackaging
{
	public T2LPOUSRequestAndReceptionPackagingWrapper(ZString marks, ZString packageType, ZInt packageNum, BusinessObjectFactory factory) : base(packageType, packageNum, factory)
	{
		Marks = marks;
	}

	public ZString Marks { get; }

	readonly static ZInt FixedVehicleQty = 1;

	const string VehicleMarkSeparator = ":";

	static ZString GetVehiclesMark(CusVehicle vehicle) => vehicle.CVH_VehicleIdentificationNumber
										+ (vehicle.CVH_BrandName.IsEmpty ? string.Empty : $"{VehicleMarkSeparator}{vehicle.CVH_BrandName}")
										+ (vehicle.CVH_ModelName.IsEmpty ? string.Empty : $"{VehicleMarkSeparator}{vehicle.CVH_ModelName}");

	public static List<T2LPOUSRequestAndReceptionPackagingWrapper> GetPackagesList(CusEntryLine cusEntryLine)
	{
		var packages = new List<T2LPOUSRequestAndReceptionPackagingWrapper>();

		cusEntryLine.InvoiceLinesWithVehicles.ForEach(line => line.Vehicles.Cast<CusVehicle>().ForEach(vehicle => packages.Add(new T2LPOUSRequestAndReceptionPackagingWrapper(GetVehiclesMark(vehicle), (ZString)RefCusCodeList.PackageType.Frame, FixedVehicleQty, cusEntryLine.Factory))));

		var packagingDetails = cusEntryLine.PackagingDetails.GroupBy(x => new { x.Package.CW_PackType, x.Package.CW_MarksAndNos });
		foreach (var pack in packagingDetails)
		{
			var packType = pack.Key.CW_PackType;
			if (!cusEntryLine.InvoiceLinesWithVehicles.Any() || packType != RefCusCodeList.PackageType.Frame)
			{
				var isBulkType = PackageHelper.PackTypeIsBulk(packType, cusEntryLine.Factory);
				var packqty = ZInt.Zero;
				foreach (var p in pack.ToArray())
				{
					if (!isBulkType)
					{
						packqty += p.CHC_NumberOfPacks;
					}
				}
				packages.Add(new T2LPOUSRequestAndReceptionPackagingWrapper(pack.Key.CW_MarksAndNos, packType, packqty, cusEntryLine.Factory));
			}
		}
		return packages;
	}
}
