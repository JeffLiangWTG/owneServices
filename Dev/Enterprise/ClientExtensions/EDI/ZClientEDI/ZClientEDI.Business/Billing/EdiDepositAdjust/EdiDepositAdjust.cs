using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiDepositAdjust : AutoEdiDepositAdjust
	{
		public EdiDepositAdjust(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZDateTime DEA_SystemCreateTimeLocal
		{
			get { return DEA_SystemCreateTimeUtc.ToLocalBranchTime(); }
		}

		public ZDateTime DEA_SystemLastEditTimeLocal
		{
			get { return DEA_SystemLastEditTimeUtc.ToLocalBranchTime(); }
		}

		[ReadOnly(true)]
		public override ZString DEA_SystemCreateUser
		{
			get { return base.DEA_SystemCreateUser; }
			set { base.DEA_SystemCreateUser = value; }
		}

		[ReadOnly(true)]
		public override ZString DEA_SystemLastEditUser
		{
			get { return base.DEA_SystemLastEditUser; }
			set { base.DEA_SystemLastEditUser = value; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			DEA_Amount = 1; // zero is not allowed
		}
#endif
	}
}

