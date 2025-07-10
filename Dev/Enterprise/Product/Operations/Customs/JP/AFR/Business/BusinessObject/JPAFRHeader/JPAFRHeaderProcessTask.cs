using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.JP.AFR.Business
{
	public class JPAFRHeaderProcessTask : ProcessTask
	{
		public JPAFRHeaderProcessTask(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override System.Type ParentType
		{
			get { return typeof(JPAFRHeader); }
		}

		public new JPAFRHeader Parent
		{
			get { return (JPAFRHeader)base.Parent; }
		}

		public override ControllerID ParentControllerID
		{
			get { return ControllerIDs.Customs.JP.AFR; }
		}
	}
}
