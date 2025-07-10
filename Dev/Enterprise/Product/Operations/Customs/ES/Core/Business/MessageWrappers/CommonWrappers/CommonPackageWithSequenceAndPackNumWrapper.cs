using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonPackageWithSequenceAndPackNumWrapper : PackageCommonWrapper, ICommonPackageWithSequenceAndPackNum
{
	public CommonPackageWithSequenceAndPackNumWrapper(ZString packageType, ZString packageMarks, ZInt packagesQty, ZShort seqNum, BusinessObjectFactory factory) : base(packageType, packageMarks)
	{
		SequenceNumber = seqNum.ToString();
		NumberOfPackages = packagesQty == ZInt.Zero && PackageHelper.PackTypeIsBulk(packageType, factory) ? string.Empty : packagesQty.ToString();
	}

	public CommonPackageWithSequenceAndPackNumWrapper(ZString packageType, ZString packageMarks, ZString packagesQty, ZShort seqNum) : base(packageType, packageMarks)
	{
		SequenceNumber = seqNum.ToString();
		NumberOfPackages = packagesQty;
	}

	public ZString SequenceNumber { get; }

	public ZString NumberOfPackages { get; }

	readonly static ZInt FixedVehicleQty = 1;
	const string VehicleMarkSeparator = ":";

	static ZString GetVehiclesMark(CusVehicle vehicle) => vehicle.CVH_VehicleIdentificationNumber
							+ (vehicle.CVH_BrandName.IsEmpty ? string.Empty : VehicleMarkSeparator + vehicle.CVH_BrandName)
							+ (vehicle.CVH_ModelName.IsEmpty ? string.Empty : VehicleMarkSeparator + vehicle.CVH_ModelName);

	public static IReadOnlyCollection<CommonPackageWithSequenceAndPackNumWrapper> GetPackagesList(CusEntryLine cusEntryLine)
	{
		var packagesDataList = new List<Tuple<ZString, ZString, ZInt>>();

		cusEntryLine.InvoiceLinesWithVehicles.ForEach(line => line.Vehicles.Cast<CusVehicle>().ForEach(vehicle => packagesDataList.Add(Tuple.Create((ZString)RefCusCodeList.PackageType.Frame, GetVehiclesMark(vehicle), FixedVehicleQty))));

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
				packagesDataList.Add(Tuple.Create(packType, pack.Key.CW_MarksAndNos, packqty));
			}
		}

		var packages = new List<CommonPackageWithSequenceAndPackNumWrapper>();
		ZShort seqNum = 1;
		foreach (var packageData in packagesDataList)
		{
			packages.Add(new CommonPackageWithSequenceAndPackNumWrapper(packageData.Item1, packageData.Item2, packageData.Item3, seqNum, cusEntryLine.Factory));
			seqNum++;
		}
		return packages.AsReadOnly();
	}
}
