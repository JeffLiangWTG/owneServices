using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Modules.AutoRefresh
{
	public class AutoRefreshBizO : NonPersistentBusinessObject
	{
		public AutoRefreshBizO(byte currentTimeOut)
		{
			using (SuspendSettingHasChanges())
			{
				AutoRefreshTimeOut = currentTimeOut;
			}
		}

		#region AutoRefreshTimeOut

		public ZByte AutoRefreshTimeOut
		{
			get
			{
				var minutesAsString = AutoRefreshTimeOutDescription_List.GetDescriptionFromCode(AutoRefreshTimeOutDescription);
				return ZByte.ParseSafe(minutesAsString, 0);
			}
			set { AutoRefreshTimeOutDescription = AutoRefreshManager.Instance.GetTimeoutDescription(value); }
		}

		[MaxLength(AutoRefreshDescMaxLength)]
		public ZString AutoRefreshTimeOutDescription
		{
			get { return fAutoRefreshTimeOutDescription; }
			set
			{
				if (fAutoRefreshTimeOutDescription != value)
				{
					CheckMaximumLength(AutoRefreshTimeOutDescriptionInfo, value);
					SetNonPersistentPropertyValue(AutoRefreshTimeOutDescriptionInfo, ref fAutoRefreshTimeOutDescription, value);
					Validation.ValidateAutoRefreshTimeOutDescription();
				}
			}
		}

		public const int AutoRefreshDescMaxLength = 25;

		public ZPropertyInfo AutoRefreshTimeOutDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(AutoRefreshTimeOutDescription)); }
		}

		ZString fAutoRefreshTimeOutDescription;

		#endregion

		#region AutoRefreshTimeOutDescription_List

		public CodeDescriptionPairList AutoRefreshTimeOutDescription_List
		{
			get
			{
				if (fAutoRefreshTimeOutDescription_List == null)
				{
					fAutoRefreshTimeOutDescription_List = new CodeDescriptionPairList();
					fAutoRefreshTimeOutDescription_List.AddRange
					(
						new CodeDescriptionPair[]
						{
							new CodeDescriptionPair(Res.GetString("94dcfb75-e08b-4c70-9bc4-4126c07fc5ae", "1 Minute"), "1"),
							new CodeDescriptionPair(Res.GetString("bb4a26fc-45ec-47b1-9790-a07fbbcfa88f", "5 Minutes"), "5"),
							new CodeDescriptionPair(Res.GetString("10543002-1b99-435d-8028-1651c4a34d2c", "10 Minutes"), "10"),
							new CodeDescriptionPair(Res.GetString("6a5c5792-d13e-4148-94bd-086648d204a1", "15 Minutes"), "15"),
							new CodeDescriptionPair(Res.GetString("13c1f1d3-9e14-4c1d-8dd9-d85441e85ea7", "30 Minutes"), "30"),
							new CodeDescriptionPair(Res.GetString("a5e6e377-8b21-4a27-9262-a3dbae67462b", "45 Minutes"), "45"),
							new CodeDescriptionPair(Res.GetString("c0ffbe8b-f130-40db-94c1-665671adfacb", "Hour"), "60"),
							new CodeDescriptionPair(Res.GetString("9f261229-67d0-43a8-b7b6-4557457c6ca0", "2 Hours"), "120"),
							new CodeDescriptionPair(Res.GetString("f3392a03-8641-4583-bba7-a8a5481fb081", "3 Hours"), "180"),
							new CodeDescriptionPair(Res.GetString("3baafb36-d2b7-4e50-ac4e-906354441aa9", "4 Hours"), "240")
						}
					);
				}
				return fAutoRefreshTimeOutDescription_List;
			}
		}

		CodeDescriptionPairList fAutoRefreshTimeOutDescription_List;

		#endregion

		#region Validation

		public AutoRefreshBizOValidation Validation
		{
			get { return new AutoRefreshBizOValidation(this); }
		}

		#endregion
	}

	#region class AutoRefreshBizOValidation

	public class AutoRefreshBizOValidation : ZValidation
	{
		public AutoRefreshBizOValidation(AutoRefreshBizO parent)
			: base(parent)
		{
			Parent = parent;
		}

		readonly AutoRefreshBizO Parent;

		public void ValidateAutoRefreshTimeOutDescription()
		{
			ValidateCalculatedProperty(Parent.AutoRefreshTimeOutDescriptionInfo);
		}

		protected void CheckAutoRefreshTimeOutDescription()
		{
			MandatoryValidation.CheckEntered(Parent.AutoRefreshTimeOutDescriptionInfo);
			if (!Parent.AutoRefreshTimeOutDescriptionInfo.HasErrors())
			{
				ListValidation.ErrorIfInvalidCode(Parent.AutoRefreshTimeOutDescriptionInfo, Parent.AutoRefreshTimeOutDescription_List);
			}
		}

		public override void ValidateAll()
		{
			ValidateAutoRefreshTimeOutDescription();
		}

		public override Type AutoValidationType
		{
			get { return GetType(); }
		}
	}

	#endregion
}
