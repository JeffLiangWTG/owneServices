using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class CusLPCOHeaderProcessTask : ProcessTask, Integration.Customs.BR.ICusLPCOHeaderProcessTask
	{
		public CusLPCOHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType => typeof(CusLPCOHeader);
	}
}
