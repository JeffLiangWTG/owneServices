using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class CommitmentLicenceSetting : EdiLicenceSetting
	{
		public CommitmentLicenceSetting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			LS9_Type = BillingConstants.LicenceSetting.Commitment;
		}

		#region Licence Units (Alias for LS9_price)

		public ZDecimal LicenceUnits
		{
			get { return LS9_Price; }
			set { LS9_Price = value; }
		}

		public ZPropertyInfo LicenceUnitsInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(LicenceUnits), x => LS9_PriceInfo); }
		}

		#endregion

		[List("Lookups.CommitmentGroupNames")]
		public override ZString LS9_Name
		{
			get { return base.LS9_Name; }
			set
			{
				base.LS9_Name = value;
				SummaryInfo.RefreshBinding();
			}
		}

		public override ZString Summary
		{
			get
			{
				return "Licence Units " + LicenceUnits.ToString("#,##0", CultureInfo.InvariantCulture);
			}
		}

		protected override EdiLicenceSettingValidation GetNewValidation()
		{
			return new CommitmentLicenceSettingValidation(this);
		}
	}

	public class CommitmentLicenceSettingValidation : EdiLicenceSettingValidation
	{
		public CommitmentLicenceSettingValidation(CommitmentLicenceSetting parent)
			: base(parent)
		{
			this.parent = parent;
		}

		readonly CommitmentLicenceSetting parent;

		protected override void CheckLS9_Price()
		{
			MandatoryValidation.CheckNotNegative(parent.LS9_PriceInfo);
			MandatoryValidation.CheckNotZero(parent.LS9_PriceInfo);
		}
	}
}

