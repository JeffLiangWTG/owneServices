using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusUnderbondProcessTask : ProcessTask
	{
		public CusUnderbondProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type ParentType
		{
			get { return typeof(CusUnderbond); }
		}

		public new CusUnderbond Parent
		{
			get { return (CusUnderbond)base.Parent; }
		}
	}
}
