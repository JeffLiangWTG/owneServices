
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCExportAWBOtherChargesValidation : Freight.Forwarding.AWB.Business.AutoExportAWBOtherChargesValidation
	{
		public JXCExportAWBOtherChargesValidation(ExportAWBOtherCharges aWBOtherCharges)
			: base(aWBOtherCharges)
		{
		}

		#region Overrides

		protected override void CheckEO_ChargeCode()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EO_ChargeCodeInfo);
			ValidationHelper.AddJXCWarningIfInvalidCode(Parent.EO_ChargeCodeInfo, Parent.IATAChargeCodesList);
		}

		protected override void CheckEO_EntitlementCode()
		{
			if (!(Parent is ShipmentExportAWBOtherCharges))
			{
				ValidationHelper.AddJXCWarningIfNotEntered(Parent.EO_EntitlementCodeInfo);
				ValidationHelper.AddJXCWarningIfInvalidCode(Parent.EO_EntitlementCodeInfo, Parent.EntitlementCodesList);
			}
		}

		protected override void CheckEO_ChargeDescription()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.EO_ChargeDescriptionInfo);
		}

		#endregion

		protected new ExportAWBOtherCharges Parent
		{
			get { return (ExportAWBOtherCharges)base.Parent; }
		}

		ValidationHelper ValidationHelper
		{
			get
			{
				if (fValidationHelper == null)
				{
					fValidationHelper = new ValidationHelper();
				}
				return fValidationHelper;
			}
		}

		ValidationHelper fValidationHelper;
	}
}
