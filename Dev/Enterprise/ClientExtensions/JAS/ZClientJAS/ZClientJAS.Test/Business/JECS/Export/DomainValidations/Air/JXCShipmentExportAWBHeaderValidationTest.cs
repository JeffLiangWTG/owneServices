using System;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export.Validations.Testing
{
	internal class JXCShipmentExportAWBHeaderValidationTest : JXCExportAWBHeaderValidationTestCase
	{
		#region Shipper
		public void TestShipmentValidateEH_ShipperPostCode()
		{
			AWBHeader.EH_ShipperState = "NSW";
			AWBHeader.EH_ShipperPostCode = "";
			AWBHeader.Validation.ValidateEH_ShipperPostCode();
			AssertHasNoJXCWarnings(AWBHeader.EH_ShipperPostCodeInfo);
			AWBHeader.EH_ShipperState = "";
			AWBHeader.Validation.ValidateEH_ShipperPostCode();
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Both the Shipper's Post Code and State cannot be blank";
			AssertHasJXCWarning(AWBHeader.EH_ShipperPostCodeInfo, expectedWarningMessage);
		}

		public void TestShipmentValidateEH_ShipperState()
		{
			AWBHeader.EH_ShipperPostCode = "2031";
			AWBHeader.EH_ShipperState = "";
			AWBHeader.Validation.ValidateEH_ShipperState();
			AssertHasNoJXCWarnings(AWBHeader.EH_ShipperStateInfo);
			AWBHeader.EH_ShipperPostCode = "";
			AWBHeader.Validation.ValidateEH_ShipperState();
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Both the Shipper's Post Code and State cannot be blank";
			AssertHasJXCWarning(AWBHeader.EH_ShipperStateInfo, expectedWarningMessage);
		}

		public void TestValidateEH_ShipperContactDetail()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.HAWBFieldBoundaries.PhoneMaxLength, ExportAWBHeaderSchema.EH_ShipperContactDetail);
		}

		#endregion
		#region Consignee
		public void TestShipmentValidateEH_ConsigneePostCode()
		{
			AWBHeader.EH_ConsigneeState = "NSW";
			AWBHeader.EH_ConsigneePostCode = "";
			AWBHeader.Validation.ValidateEH_ConsigneePostCode();
			AssertHasNoJXCWarnings(AWBHeader.EH_ConsigneePostCodeInfo);
			AWBHeader.EH_ConsigneeState = "";
			AWBHeader.Validation.ValidateEH_ConsigneePostCode();
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Both the Consignee's Post Code and State cannot be blank";
			AssertHasJXCWarning(AWBHeader.EH_ConsigneePostCodeInfo, expectedWarningMessage);
		}

		public void TestShipmentValidateEH_ConsigneeState()
		{
			AWBHeader.EH_ConsigneePostCode = "2031";
			AWBHeader.EH_ConsigneeState = "";
			AWBHeader.Validation.ValidateEH_ConsigneeState();
			AssertHasNoJXCWarnings(AWBHeader.EH_ConsigneeStateInfo);
			AWBHeader.EH_ConsigneePostCode = "";
			AWBHeader.Validation.ValidateEH_ConsigneeState();
			string expectedWarningMessage = JXCConstants.JXCWarningPrefix + "Both the Consignee's Post Code and State cannot be blank";
			AssertHasJXCWarning(AWBHeader.EH_ConsigneeStateInfo, expectedWarningMessage);
		}

		public void TestValidateEH_ConsigneeContactDetail()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.HAWBFieldBoundaries.PhoneMaxLength, ExportAWBHeaderSchema.EH_ConsigneeContactDetail);
		}

		#endregion
		#region Notify Party
		public void TestValidateEH_AlsoNotifyName()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.HAWBFieldBoundaries.AlsoNotifyNameMaxLength, ExportAWBHeaderSchema.EH_AlsoNotifyName);
		}

		public void TestValidateEH_AlsoNotifyPlace()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.HAWBFieldBoundaries.AlsoNotifyPlaceMaxLength, ExportAWBHeaderSchema.EH_AlsoNotifyPlace);
		}

		public void TestValidateEH_AlsoNotifyContactDetail()
		{
			AssertMaxLengthLessOrEqualToForJXC(JXCConstants.HAWBFieldBoundaries.PhoneMaxLength, ExportAWBHeaderSchema.EH_AlsoNotifyContactDetail);
		}

		#endregion
		#region Freight Declarations
		public void TestValidateEH_HouseDeclaredValueCurrency()
		{
			AWBHeader.EH_HouseDeclaredValueCurrency = "89";
			AssertHasInvalidCurrencyCodeJXCWarning(AWBHeader.EH_HouseDeclaredValueCurrencyInfo);
			AWBHeader.EH_HouseDeclaredValueCurrency = Core.Constants.CurrencyCodes.Indonesia;
			AssertHasNoJXCWarnings(AWBHeader.EH_HouseDeclaredValueCurrencyInfo);
		}

		public void TestValidateEH_HouseCustomsValueCurrency()
		{
			AWBHeader.EH_HouseCustomsValueCurrency = "89";
			AssertHasInvalidCurrencyCodeJXCWarning(AWBHeader.EH_HouseCustomsValueCurrencyInfo);
			AWBHeader.EH_HouseCustomsValueCurrency = Core.Constants.CurrencyCodes.Indonesia;
			AssertHasNoJXCWarnings(AWBHeader.EH_HouseCustomsValueCurrencyInfo);
		}

		#endregion
		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<JASForwardingShipment>();
					fShipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				}

				return fShipment;
			}
		}

		protected override ExportAWBHeader GetNewAWBHeader()
		{
			return Shipment.AWBHeader;
		}

		protected override Type ExpectedJXCExportAWBHeaderValidationTypeToTest
		{
			get
			{
				return typeof(JXCShipmentExportAWBHeaderValidation);
			}
		}

		protected override void ChangeOverrideWayBillDefaultsFlag(bool shouldOverride)
		{
			Shipment.JS_OverrideWaybillDefaults = shouldOverride;
		}

		JASForwardingShipment fShipment;
	}
}
