using System;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCForwardingShipmentValidationTest : JXCValidationTestCase
	{
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Factory.Validation.MainGroup.RegisterValidationType(typeof(JASForwardingShipment), ValidationTypeToTest);
		}

		protected virtual Type ValidationTypeToTest
		{
			get
			{
				return typeof(JXCForwardingShipmentValidation);
			}
		}

		protected JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<JASForwardingShipment>();
				}

				return fShipment;
			}
		}

		JASForwardingShipment fShipment;
		#endregion
	}
}
