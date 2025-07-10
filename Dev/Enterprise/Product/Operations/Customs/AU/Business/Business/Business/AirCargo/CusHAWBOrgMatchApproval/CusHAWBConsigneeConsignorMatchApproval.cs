using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public abstract class CusHAWBConsigneeConsignorMatchApproval : OrgMatchApproval
	{
		public CusHAWBConsigneeConsignorMatchApproval(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new CusHAWB Parent
		{
			get { return (CusHAWB)base.Parent; }
		}

		protected override ZString ParentReference
		{
			get { return Parent.CS_HAWB; }
		}

		protected override ZString ParentMasterBill
		{
			get { return (Parent.MAWB == null) ? "" : (string)Parent.MAWB.CM_MAWB; }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return "Organisation Match Approval - MAWB='" + Parent.MAWB.CM_MAWB + "' HAWB='" + Parent.CS_HAWB + "'"; }
		}
	}
}
