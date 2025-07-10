using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.DocumentWrappers.Customs.EU.TemporaryStorage;

public class TemporaryStorageBillWrapper : DocBaseWrapper
{
	public static TemporaryStorageBillWrapper New(TemporaryStorageBill bill, BusinessObjectFactory factory) => new TemporaryStorageBillWrapper(bill, factory);

	protected TemporaryStorageBillWrapper(TemporaryStorageBill bill, BusinessObjectFactory factory) : base(bill, factory)
	{
	}

	new TemporaryStorageBill ParentBusinessObject => (TemporaryStorageBill)base.ParentBusinessObject;

	public ZString BillNumber => ParentBusinessObject.ABL_BillNumber;

	public ZString Consignor => new TemporaryStorageAddressFormatter(ParentBusinessObject.ShipperABLAddress).AsString();

	public ZString Consignee => new TemporaryStorageAddressFormatter(ParentBusinessObject.ConsigneeABLAddress).AsString();

	public ZString NotifyParty => new TemporaryStorageAddressFormatter(ParentBusinessObject.NotifyPartyABLAddress).AsString();

	public ZString MarksAndNumbers => LinkedPackages.Aggregate(new ZStringBuilder(), (s, f) => s.Append(f.Package.APA_MarksAndNumbers)).ToStringWithDelimiterBetweenAppends(", ");

	public ZInt PackQuantity => LinkedPackages.Aggregate(ZInt.Zero, (s, f) => s + f.PackQty);

	public ZString PackQuantityUnit => LinkedPackages.SameOrDefault(pack => pack.Package.APA_PackUQ);

	public ZDecimal GrossWeightInKilograms => ParentBusinessObject.PackedItems.Aggregate(ZDecimal.Zero, (s, f) => s + new ZWeight(f.API_GrossWeight, f.API_GrossWeightUQ).InKilogramsSafe);

	public ZString Mrn => MrnCore;

	protected virtual ZString MrnCore => ParentBusinessObject.Header.MRN;

	IReadOnlyCollection<TemporaryStorageLinkPackage> LinkedPackages => linkedPackages ??= ParentBusinessObject.PackedItems
		.Cast<TemporaryStoragePackedItem>()
		.SelectMany(p => p.TemporaryStorageLinkPackages)
		.Cast<TemporaryStorageLinkPackage>()
		.Where(pack => pack.IsLinked)
		.ToArray();

	IReadOnlyCollection<TemporaryStorageLinkPackage> linkedPackages;

	public DocBaseWrapperCollection<TemporaryStoragePackedItemWrapper> PackedItems => packedItems ??= GetPackedItemsCollection();
	DocBaseWrapperCollection<TemporaryStoragePackedItemWrapper> packedItems;

	protected virtual DocBaseWrapperCollection<TemporaryStoragePackedItemWrapper> GetPackedItemsCollection()
	{
		return new TemporaryStoragePackedItemWrapperCollection(ParentBusinessObject.PackedItems.Cast<TemporaryStoragePackedItem>(), Factory);
	}
}
