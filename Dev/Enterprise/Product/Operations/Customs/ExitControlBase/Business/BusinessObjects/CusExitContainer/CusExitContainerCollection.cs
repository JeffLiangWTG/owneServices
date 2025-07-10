using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ExitControlBase.Business;

public interface ICusExitContainerCollection<out T> : IActiveBusinessObjectCollection<T>
{
	new T this[int index] { get; }
	void MarkAsNeedingValidation();
	event CollectionCountChangedEventHandler CollectionCountChange;
	(bool HasAnEquipment, bool HasANonEquipment) Flags { get; }
}

public class CusExitContainerCollection<T> : ActiveBusinessObjectCollection<T>, ICusExitContainerCollection<T>
	where T : CusExitContainer
{
	public CusExitContainerCollection(CusExitHeader master)
		: base(master.Factory, master, new ZQuery(), CusExitContainerSchema.CXN_CXH_Header)
	{
	}

	public (bool HasAnEquipment, bool HasANonEquipment) Flags => Factory.GetValue(ref containersAndEquipmentsDataCached, () =>
	{
		var hasAnEquipment = false;
		var hasANonEquipment = false;
		if (Count > 0)
		{
			foreach (var container in this)
			{
				if (container.CXN_IsEquipment)
				{
					hasAnEquipment = true;
					if (hasANonEquipment)
					{
						break;
					}
				}
				else
				{
					hasANonEquipment = true;
					if (hasAnEquipment)
					{
						break;
					}
				}
			}
		}
		return (hasAnEquipment, hasANonEquipment);
	});
	CachedProperty<(bool HasAnEquipment, bool HasANonEquipment)> containersAndEquipmentsDataCached;

	protected override void SetDefaultsForNewElementCore(T child)
	{
		base.SetDefaultsForNewElementCore(child);
		var newElement = (CusExitContainer)child;
		newElement.CXN_Sequence = Count > ZShort.Zero ? this.Cast<CusExitContainer>().Max(x => x.CXN_Sequence) + 1 : (ZShort)1;
	}
}
