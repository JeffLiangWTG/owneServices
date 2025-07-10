using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.JP.Common
{
	public class CusSealCollection<T> : ActiveBusinessObjectCollection<T>
		where T : CusSeal
	{
		public CusSealCollection(BusinessObject master, int maxRowCount) : base(master.Factory, master, new ZQuery(CusSealSchema.BK_ParentTableCode, master.TablePrefix), CusSealSchema.BK_ParentID)
		{
			this.maxRowCount = maxRowCount;
		}

		readonly int maxRowCount;

		protected override bool AllowNew => base.AllowNew && Count < maxRowCount;

		protected override void SetDefaultsForNewElementCore(T newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.BK_ParentTableCode = Relationship.Master.TablePrefix;
		}
	}
}
