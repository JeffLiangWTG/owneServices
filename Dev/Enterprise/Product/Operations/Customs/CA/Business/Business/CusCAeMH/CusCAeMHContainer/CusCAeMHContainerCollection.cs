using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusCAeMHContainerCollection : ActiveBusinessObjectCollection<CusCAeMHContainer>
	{
		public CusCAeMHContainerCollection(CusCAeMHMaster master)
			: base(master.Factory, new ZQuery(CusCAeMHContainerSchema.BQ_BP_Master, master.PK))
		{
			this.master = master;
		}
		readonly CusCAeMHMaster master;

		protected override void SetDefaultsForNewElementCore(CusCAeMHContainer newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.BQ_BP_Master = master.PK;
		}

		protected override void SetRelationshipDefaultsForElementCore(CusCAeMHContainer newElement, bool throwIfRelationshipNotSupported)
		{
			base.SetRelationshipDefaultsForElementCore(newElement, throwIfRelationshipNotSupported);
			newElement.BQ_BP_Master = master.PK;
		}
	}
}
