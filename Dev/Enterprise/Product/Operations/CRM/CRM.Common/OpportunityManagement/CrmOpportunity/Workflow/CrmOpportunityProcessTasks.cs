using System;
using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.CRM.Common
{
	public class CrmOpportunityProcessTasks : ProcessTask
	{
		public CrmOpportunityProcessTasks(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Parent

		public override ControllerID ParentControllerID => null;

		protected override Type ParentType
		{
			get { return typeof(CrmOpportunity); }
		}

		public new CrmOpportunity Parent
		{
			get { return (CrmOpportunity)base.Parent; }
		}

		#endregion
	}
}
