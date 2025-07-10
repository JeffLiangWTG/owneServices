using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.Riba
{
	public class RejectOrderReasonHolder : NonPersistentBusinessObject, IObsoleteValidation
	{
		public RejectOrderReasonHolder()
		{ }

		[MaxLength(3)]
		[List("OrderRejectReasonCodes_List")]
		public ZString Code
		{
			get { return fCode; }
			set
			{
				SetNonPersistentPropertyValue(CodeInfo, ref fCode, value);
				var readDescription = OrderRejectReasonCodes_List.GetDescriptionFromCode(Code);
				Description = readDescription != null ? ((ZString)readDescription).Left(DescriptionInfo.MaxLength) : null;
				Reason = Description;
			}
		}
		ZString fCode;

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(nameof(Code)); }
		}

		[MaxLength(50)]
		public ZString Description
		{
			get { return fDescription; }
			set { SetNonPersistentPropertyValue(DescriptionInfo, ref fDescription, value); }
		}
		ZString fDescription;

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(Description)); }
		}

		[MaxLength(200)]
		public ZString Reason
		{
			get { return fReason; }
			set { SetNonPersistentPropertyValue(ReasonInfo, ref fReason, value); }
		}
		ZString fReason;

		public ZPropertyInfo ReasonInfo
		{
			get { return GetZPropertyInfo(nameof(Reason)); }
		}

		ReadOnlyCodeDescriptionPairList fOrderRejectReasonCodes_List;
		public ReadOnlyCodeDescriptionPairList OrderRejectReasonCodes_List
		{
#if DEBUG
			set
			{
				fOrderRejectReasonCodes_List = value;
			}
#endif
			get
			{
				if (fOrderRejectReasonCodes_List == null)
				{
					fOrderRejectReasonCodes_List = AccountingConfigurationRegistry.Instance.OrderRejectReasonCodesList.Value;
				}
				return fOrderRejectReasonCodes_List;
			}
		}
	}
}

