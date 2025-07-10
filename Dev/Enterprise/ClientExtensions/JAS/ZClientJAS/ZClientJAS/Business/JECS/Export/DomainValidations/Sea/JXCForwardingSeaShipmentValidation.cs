using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations
{
	public class JXCForwardingSeaShipmentValidation : JXCForwardingShipmentValidation
	{
		public JXCForwardingSeaShipmentValidation(JASForwardingShipment shipment)
			: base(shipment)
		{
		}

		protected override void CheckJS_HouseBill()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.JS_HouseBillInfo);
		}

		protected override void CheckJS_ShippedOnBoardDate()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.JS_ShippedOnBoardDateInfo);
		}

		protected override void CheckJS_RL_NKOrigin()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.JS_RL_NKOriginInfo);
			ValidationHelper.AddJXCWarningIfInvalidCode(Parent.JS_RL_NKOriginInfo, Parent.Lookups.Origins);
		}

		protected override void CheckJS_RL_NKDestination()
		{
			ValidationHelper.AddJXCWarningIfNotEntered(Parent.JS_RL_NKDestinationInfo);
			ValidationHelper.AddJXCWarningIfInvalidCode(Parent.JS_RL_NKDestinationInfo, Parent.Lookups.Destinations);
		}

		public override JobDocAddressValidation JobDocAddressValidation(JobDocAddress addressToValidate)
		{
			return new JASForwardingSEAShipmentDocAddressValidation(addressToValidate);
		}

		#region Implementation

		protected ValidationHelper ValidationHelper
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

		#endregion

	}
}
