using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	/// <summary>
	///  Used only for synching
	/// </summary>
	public class NctsDepartureHeaderContainersActiveCollection : ActiveBusinessObjectCollection<NctsDepartureHeaderContainer>, Customs.Business.ICusInBondContainerCollection
	{
		public NctsDepartureHeaderContainersActiveCollection(NctsHeader master)
			: base(master.Factory, master, new ZQuery(), CusInBondContainerSchema.BC_ParentID)
		{
			this.master = master;
		}
		readonly NctsHeader master;

		public NctsDepartureHeaderContainer this[ZString containerNum] => this.FirstOrDefault(container => !container.IsDeleted && container.BC_ContainerNum == containerNum);

		protected override void EndNew(int index)
		{
			NctsDepartureHeaderContainer newElement = null;
			if (index >= 0 && index < Count)
			{
				newElement = this[index];
				if (!IsNonCommittedElement(newElement))
				{
					newElement = null;
				}
			}
			base.EndNew(index);
			if (newElement != null)
			{
				master.DepartureHeaderContainers.Add(newElement);
			}
		}
		protected override void OnAdded(NctsDepartureHeaderContainer businessObject)
		{
			if (!IsNonCommittedElement(businessObject))
			{
				master.DepartureHeaderContainers.Add(businessObject);
			}
		}

		BusinessObject Customs.Business.ICusInBondContainerCollection.this[ZString containerNum] => this[containerNum];

		BusinessObject Customs.Business.ICusInBondContainerCollection.this[int index] => this[index];
	}
}
