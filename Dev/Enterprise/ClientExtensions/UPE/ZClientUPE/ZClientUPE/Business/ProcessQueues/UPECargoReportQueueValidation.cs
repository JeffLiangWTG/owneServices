using CargoWise.EntityFramework;

namespace Enterprise.Client.UPE.Business
{
	public class UPECargoReportQueueValidation : UPECustomsProcessQueueValidation
	{
		public UPECargoReportQueueValidation(UPECargoReportQueue parent)
			: base(parent)
		{
		}

		protected override void CheckP4_CustomAttrib4()
		{
			base.CheckP4_CustomAttrib4();
			MandatoryValidation.CheckEntered(Parent.P4_CustomAttrib4Info);
			ListValidation.ErrorIfInvalidCode(Parent.P4_CustomAttrib4Info, Parent.Lookups.ShipmentTypeList);
		}

		protected override void CheckP4_CustomAttrib6()
		{
			base.CheckP4_CustomAttrib6();
			MandatoryValidation.CheckEntered(Parent.P4_CustomAttrib6Info);
			ListValidation.ErrorIfInvalidCode(Parent.P4_CustomAttrib6Info, new DutyTypeCodeDescriptionPairList());
		}

		protected override UPECustomsQueueValidationHelper GetNewCustomsQueueValidationHelper()
		{
			return new UPECargoReportQueueValidationHelper(Parent);
		}

		protected new UPECargoReportQueue Parent
		{
			get { return (UPECargoReportQueue)base.Parent; }
		}
	}
}
