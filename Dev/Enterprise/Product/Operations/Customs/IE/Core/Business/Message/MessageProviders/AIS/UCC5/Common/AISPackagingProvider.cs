using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IE.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.IE.Business.AIS.UCC5
{
	public class AISPackagingProvider : IPackaging
	{
		public static IReadOnlyCollection<IPackaging> GetCollection<TBizObj, TPack, TItemNumber>(
			TBizObj parentBizObj,
			Func<TBizObj, IReadOnlyCollection<TPack>> getLinkedPacks,
			Func<TPack, bool> isBulk,
			Func<TPack, (string type, int quantity, string marks)> getData,
			Func<TPack, TItemNumber> getFirstLinkedItemNumber,
			Func<TBizObj, TItemNumber> getItemNumber
		)
			where TBizObj : BusinessObject
			where TPack : BusinessObject
		{
			var list = new List<IPackaging>();
			var linkedPackages = getLinkedPacks(parentBizObj);
			foreach (var linkedPackage in linkedPackages)
			{
				var data = getData(linkedPackage);
				if (isBulk(linkedPackage))
				{
					list.Add(new AISPackagingProvider(data.type, null, data.marks));
				}
				else
				{
					var firstLinkedItemNumber = linkedPackage.Factory.GetCachedValue(
						$"{linkedPackage.TablePrefix}|{linkedPackage.PK}",
						() => getFirstLinkedItemNumber(linkedPackage)
					);
					list.Add(new AISPackagingProvider(
						data.type,
						getItemNumber(parentBizObj).Equals(firstLinkedItemNumber) ? data.quantity : 0,
						data.marks
					));
				}
			}
			return list.Cast<IPackaging>().ToArray();
		}

		public static IReadOnlyCollection<IPackaging> GetCollection(CusEntryLine entryLine) => GetCollection(
			parentBizObj: entryLine,
			getLinkedPacks: entryLine => entryLine.RandomLine.PackagesPivot.Select(pivot => pivot.Package).WhereNotNull().Cast<Package>().ToHashSet(),
			isBulk: pack => pack.IsBulk,
			getData: pack => (pack.CW_PackType, pack.CW_PackQty, pack.CW_MarksAndNos),
			getFirstLinkedItemNumber: pack => pack.InvoiceLinePivotCollection.Select(pivot => pivot.InvoiceLine?.CusEntryLine).WhereNotNull().MinOrDefault(entryLine => entryLine.CL_LineNumber),
			getItemNumber: entryLine => entryLine.CL_LineNumber
		);

		public static IReadOnlyCollection<IPackaging> GetCollection(TemporaryStoragePackedItem packedItem) => GetCollection(
			parentBizObj: packedItem,
			getLinkedPacks: packedItem => packedItem.TemporaryStorageLinkPackages.Where(x => x.IsLinked).Select(link => link.Package).WhereNotNull().ToHashSet(),
			isBulk: pack => GetBulkPackageTypeList(pack.Factory).ContainsCode(pack.APA_PackUQ),
			getData: pack => (pack.APA_PackUQ, pack.APA_PackQty, pack.APA_MarksAndNumbers),
			getFirstLinkedItemNumber: pack => pack.PackedItems.Cast<AsycudaPackPackedItemPivot>().Select(pivot => pivot.PackedItem).WhereNotNull().MinOrDefault(item => item.API_LineNo),
			getItemNumber: pack => pack.API_LineNo
		);

		static CodeDescriptionPairList GetBulkPackageTypeList(BusinessObjectFactory factory)
		{
			return Universal.RefCusCodeListTypes.GetCachedListMatchSingleAttributeValues(
				factory,
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				UNPackTypeStartDate,
				false,
				Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk
			);
		}

		public AISPackagingProvider(string type, int? quantity, string marks)
		{
			PackageType = type;
			PackageQuantity = quantity;
			ShippingMarks = marks;
		}

		public string PackageType { get; }

		public int? PackageQuantity { get; }

		public string ShippingMarks { get; }
	}
}
