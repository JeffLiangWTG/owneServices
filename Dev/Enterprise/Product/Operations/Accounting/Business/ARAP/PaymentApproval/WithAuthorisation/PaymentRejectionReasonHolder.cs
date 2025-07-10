using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public class PaymentRejectionReasonHolder : NonPersistentBusinessObject
	{
		[MaxLength(3)]
		[List(nameof(ReasonCodesList))]
		public ZString Code
		{
			get { return code; }
			set
			{
				if (code != value)
				{
					SetNonPersistentPropertyValue(CodeInfo, ref code, value);
					Reason = Description;
				}
			}
		}
		ZString code;

		public ZPropertyInfo CodeInfo => GetZPropertyInfo(nameof(Code));

		public ZString Description => ReasonCodesList.GetDescriptionFromCode(Code);

		public ZPropertyInfo DescriptionInfo => GetZPropertyInfo(nameof(Description));

		[MaxLength(128)]
		public ZString Reason
		{
			get { return reason; }
			set { SetNonPersistentPropertyValue(ReasonInfo, ref reason, value); }
		}
		ZString reason;

		public ZPropertyInfo ReasonInfo => GetZPropertyInfo(nameof(Reason));

		public ReadOnlyCodeDescriptionPairList ReasonCodesList => AccountingMasterFilesRegistry.Instance.PaymentRejectionReasonCodesList.Value;
	}
}
