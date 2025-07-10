using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NCTS5CommonPackagingWrapper : PackageCommonWrapper, INCTSCommonPackaging
	{
		public NCTS5CommonPackagingWrapper(BusinessObjectFactory factory, ZString packageType, ZString packageMarks, ZLong packagesQty, ZShort seqNum, bool shouldNotDeclarePacksQty = false) : base(packageType, packageMarks)
		{
			SequenceNumber = seqNum.ToString();
			NumberOfPackages = (packagesQty == ZInt.Zero && PackageHelper.PackTypeIsBulk(packageType, factory)) || shouldNotDeclarePacksQty ? string.Empty : packagesQty.ToString();
		}

		readonly static ZLong FixedVehicleQty = 1;

		public ZString SequenceNumber { get; }

		public ZString NumberOfPackages { get; }

		public static IReadOnlyCollection<NCTS5CommonPackagingWrapper> GetPackagesListDeparture(NctsDepartureCargoDesc item)
		{
			var packagesDataList = new List<Tuple<ZString, ZString, ZLong, ZShort>>();

			var orderedPackages = item.Packages.Cast<NctsPackage>().OrderBy(p => p.B5_SequenceNumber);

			if (item.IsVehicles)
			{
				foreach (var vehiclePack in orderedPackages)
				{
					packagesDataList.Add(Tuple.Create((ZString)RefCusCodeList.PackageType.Frame, GetVehicleMark(vehiclePack.B5_PackageID, vehiclePack.B5_Brand, vehiclePack.B5_Model), FixedVehicleQty, vehiclePack.B5_SequenceNumber));
				}
			}
			else
			{
				foreach (var pack in orderedPackages)
				{
					var packType = pack.B5_UnitType;
					var packqty = !PackageHelper.PackTypeIsBulk(packType, item.Factory) ? pack.B5_UnitCount : ZLong.Zero;
					packagesDataList.Add(Tuple.Create(packType, pack.B5_MarksAndNumbers, packqty, pack.B5_SequenceNumber));
				}
			}

			var packagesToReturn = new List<NCTS5CommonPackagingWrapper>();

			foreach (var packageData in packagesDataList)
			{
				packagesToReturn.Add(new NCTS5CommonPackagingWrapper(item.Factory, packageData.Item1, packageData.Item2, packageData.Item3, packageData.Item4));
			}
			return packagesToReturn.AsReadOnly();
		}

		public static IReadOnlyCollection<NCTS5CommonPackagingWrapper> GetPackagesListArrival(NctsArrivalCargoDesc item)
		{
			var packagesDataList = new List<Tuple<ZString, ZString, ZLong, ZShort, bool>>();

			var orderedPackages = item.Packages.Cast<NctsPackage>().Where(p => p.B5_TypeOfDifference.IsUnloadingStateNEWorMISorDIF()).OrderBy(p => p.B5_SequenceNumber);

			foreach (var pack in orderedPackages)
			{
				var isUnloadingStateDIF = pack.B5_TypeOfDifference.IsUnloadingStateDIF();
				var packType = isUnloadingStateDIF ? pack.PackDifference.B5_UnitType : pack.B5_UnitType;
				var packqty = !PackageHelper.PackTypeIsBulk(packType, item.Factory) 
					? isUnloadingStateDIF ? pack.PackDifference.B5_UnitCount : pack.B5_UnitCount
					: ZLong.Zero;
				var marks = packType == RefCusCodeList.PackageType.Frame
									? isUnloadingStateDIF ? GetVehicleMark(pack.PackDifference.B5_PackageID, pack.PackDifference.B5_Brand, pack.PackDifference.B5_Model) : GetVehicleMark(pack.B5_PackageID, pack.B5_Brand, pack.B5_Model)
									: isUnloadingStateDIF ? pack.PackDifference.B5_MarksAndNumbers : pack.B5_MarksAndNumbers;
				packagesDataList.Add(Tuple.Create(packType, marks, packqty, pack.B5_SequenceNumber, pack.B5_TypeOfDifference.IsUnloadingStateNEWorDIF()));
			}

			var packagesToReturn = new List<NCTS5CommonPackagingWrapper>();

			foreach (var packageData in packagesDataList)
			{
				packagesToReturn.Add(packageData.Item5
										? new NCTS5CommonPackagingWrapper(item.Factory, packageData.Item1, packageData.Item2, packageData.Item3, packageData.Item4)
										: new NCTS5CommonPackagingWrapper(item.Factory, ZString.Empty, ZString.Empty, ZLong.Zero, packageData.Item4, shouldNotDeclarePacksQty: true));
			}
			return packagesToReturn.AsReadOnly();
		}

		static ZString GetVehicleMark(ZString vin, ZString brand, ZString model) => vin + ":" + brand + ":" + model;
	}
}
