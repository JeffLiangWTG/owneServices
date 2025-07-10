using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class DiscountSuspensionPolicyLicenceSetting : EdiLicenceSetting
	{
		public DiscountSuspensionPolicyLicenceSetting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LS9_Type = BillingConstants.LicenceSetting.DiscountSuspensionPolicy;
			PolicyCode = DiscountSuspensionPolicyList.Codes.NeverSuspend;
		}

		[List("Lookups.DiscountSuspensionPolicyCodes")]
		[MaxLength(3)]
		public override ZString LS9_Name { get => base.LS9_Name; set => base.LS9_Name = value; }

		[List("Lookups.DiscountSuspensionPolicyCodes")]
		[MaxLength(3)]
		public ZString PolicyCode
		{
			get => LS9_Name;
			set => LS9_Name = value;
		}

		public ZPropertyInfo PolicyCodeInfo => GetWrappedZPropertyInfo(nameof(PolicyCode), x => LS9_NameInfo);

		public override ZString Summary => $"{PolicyCode} - {Lookups.DiscountSuspensionPolicyCodes.GetDescriptionFromCode(PolicyCode)}";

		protected override EdiLicenceSettingValidation GetNewValidation() => new DiscountSuspensionPolicyLicenceSettingValidation(this);
	}

	public class DiscountSuspensionPolicyLicenceSettingValidation : EdiLicenceSettingValidation
	{
		public DiscountSuspensionPolicyLicenceSettingValidation(DiscountSuspensionPolicyLicenceSetting parent)
			: base(parent)
		{
		}

		protected override void CheckLS9_Name()
		{
			MandatoryValidation.CheckEntered(Parent.LS9_NameInfo);
			ListValidation.ErrorIfInvalidCode(Parent.LS9_NameInfo);
		}
	}
}


