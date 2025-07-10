using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Business
{
	public class DailyNoticeStatementProcessTask : ProcessTask
	{
		public DailyNoticeStatementProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CusStatementHeader);

		public new CusStatementHeader Parent => (CusStatementHeader)base.Parent;

		public override ControllerID ParentControllerID => ControllerIDs.Customs.CA.CADailyNoticeReconciliation;
	}
}
