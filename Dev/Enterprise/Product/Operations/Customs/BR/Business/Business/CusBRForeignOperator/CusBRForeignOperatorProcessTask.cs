using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business
{
	public class CusBRForeignOperatorProcessTask : ProcessTask
	{
		public CusBRForeignOperatorProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		protected override Type ParentType => typeof(CusBRForeignOperator);
	}
}
