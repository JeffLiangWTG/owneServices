using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class BuyingGroupLicenceSetting : EdiLicenceSetting
	{
		public BuyingGroupLicenceSetting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LS9_Type = BillingConstants.LicenceSetting.BuyingGroup;
		}

		public override ZString Summary
		{
			get
			{
				return  LS9_Name;
			}
		}

		[List("Lookups.BuyingGroupNames")]
		public override ZString LS9_Name
		{
			get { return base.LS9_Name; }
			set
			{
				base.LS9_Name = value;
				SummaryInfo.RefreshBinding();
			}
		}

		protected override EdiLicenceSettingValidation GetNewValidation()
		{
			return new BuyingGroupLicenceSettingValidation(this);
		}
	}

	public class BuyingGroupLicenceSettingValidation : EdiLicenceSettingValidation
	{
		public BuyingGroupLicenceSettingValidation(BuyingGroupLicenceSetting parent)
			: base(parent)
		{
		}

		protected override void CheckLS9_Name()
		{
			MandatoryValidation.CheckEntered(Parent.LS9_NameInfo);
		}
	}
}

