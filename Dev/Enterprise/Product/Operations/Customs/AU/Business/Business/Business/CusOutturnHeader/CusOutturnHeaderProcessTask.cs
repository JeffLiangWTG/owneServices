using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusOutturnHeaderProcessTask : ProcessTask, Integration.Customs.AU.ICusOutturnHeaderProcessTask
	{
		public CusOutturnHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CusOutturnHeader);
		public new CusOutturnHeader Parent => (CusOutturnHeader)base.Parent;

		public override ControllerID ParentControllerID => ControllerIDs.Customs.AU.SeaCargoOutturnBillsController;
	}
}
