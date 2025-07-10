using System;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCForwardingSeaShipmentValidationTest : JXCForwardingShipmentValidationTest
	{
		public void TestValidateJS_HouseBill()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.OHBLFieldBoundaries.BillOfLadingMaxLength, JobShipmentSchema.JS_HouseBill);
			AssertHasNoJXCWarnings("Pre-condition", Shipment.JS_HouseBillInfo);
			Shipment.JS_HouseBill = "123t";
			AssertHasNoJXCWarnings(Shipment.JS_HouseBillInfo);
			Shipment.JS_HouseBill = "";
			AssertHasNotEnteredJXCWarning(Shipment.JS_HouseBillInfo);
		}

		public void TestValidateJS_ShippedOnBoardDate()
		{
			AssertHasNoJXCWarnings("Pre-condition", Shipment.JS_ShippedOnBoardDateInfo);
			Shipment.JS_ShippedOnBoardDate = ZDateTime.Now;
			AssertHasNoJXCWarnings(Shipment.JS_ShippedOnBoardDateInfo);
			Shipment.JS_ShippedOnBoardDate = ZDateTime.Empty;
			AssertHasNotEnteredJXCWarning(Shipment.JS_ShippedOnBoardDateInfo);
		}

		public void TestValidateJS_RL_NKOrigin()
		{
			Shipment.JS_RL_NKOrigin = "";
			Shipment.Validation.ValidateJS_RL_NKOrigin();
			AssertHasNotEnteredJXCWarning(Shipment.JS_RL_NKOriginInfo);
			Shipment.JS_RL_NKOrigin = "TB252";
			AssertHasInvalidCodeJXCWarning(Shipment.JS_RL_NKOriginInfo);
			Shipment.JS_RL_NKOrigin = "AUSYD";
			AssertHasNoJXCWarnings(Shipment.JS_RL_NKOriginInfo);
		}

		public void TestValidateJS_RL_NKDestination()
		{
			Shipment.JS_RL_NKDestination = "";
			Shipment.Validation.ValidateJS_RL_NKDestination();
			AssertHasNotEnteredJXCWarning(Shipment.JS_RL_NKDestinationInfo);
			Shipment.JS_RL_NKDestination = "TB252";
			AssertHasInvalidCodeJXCWarning(Shipment.JS_RL_NKDestinationInfo);
			Shipment.JS_RL_NKDestination = "IDJKT";
			AssertHasNoJXCWarnings(Shipment.JS_RL_NKDestinationInfo);
		}

		protected override Type ValidationTypeToTest
		{
			get
			{
				return typeof(JXCForwardingSeaShipmentValidation);
			}
		}
	}
}
