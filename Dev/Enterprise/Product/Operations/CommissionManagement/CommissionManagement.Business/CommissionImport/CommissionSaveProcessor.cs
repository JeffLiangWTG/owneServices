using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.DataMapping;

namespace Enterprise.CommissionManagement.Business
{
	public class CommissionSaveProcessor : DataTransferProcessor
	{
		public CommissionSaveProcessor(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public override void Import()
		{
			OnSaving(1, Res.GetString("e6d9b843-9136-4e02-acbc-d406d6981e2a", "Saving entity commissions. This may take a long time depending on the amount being imported."));
			factory.Save();
			OnSavingComplete(100, Res.GetString("51c46735-c100-412d-b6e7-6b1c4ccba3e9", "Entity commissions saved."));
		}

		public override void Rollback()
		{
		}
	}
}
