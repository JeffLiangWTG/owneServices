using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ComplianceRisk.GUI.Test
{
	public class ComplianceRiskSecurityRightsTest : ComplianceRiskHelperTest
	{
		#region Shipment

		public void TestShipmentAllowDeclineComplianceAssessment_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceDeclineComplianceAssessment.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedDeclineComplianceAssessmentWithShowError((IBusiness)CreateNewShipment, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentAllowDeclineComplianceAssessment_WhenSecurityIOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceDeclineComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedDeclineComplianceAssessmentWithShowError((IBusiness)CreateNewShipment, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.ShipmentsComplianceDeclineComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentAllowEditComplianceAssessment_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceEditComplianceAssessment.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedEditComplianceAssessmentWithShowError((IBusiness)CreateNewShipment, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentAllowEditComplianceAssessment_WhenSecurityIOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceEditComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedEditComplianceAssessmentWithShowError((IBusiness)CreateNewShipment, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.ShipmentsComplianceEditComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentAllowEditHarmonizedCode_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceEditHarmonizedCode.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedEditHarmonizedCodeWithShowError((IBusiness)CreateNewShipment, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentAllowEditHarmonizedCode_WhenSecurityIOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceEditHarmonizedCode.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedEditHarmonizedCodeWithShowError((IBusiness)CreateNewShipment, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.ShipmentsComplianceEditHarmonizedCode.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentAllowOverrideComplianceRisk_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowOverrideOverallRiskStatus.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedOverrideOverallRiskStatusWithShowError((IBusiness)CreateNewShipment, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentAllowOverrideComplianceRiskWithShowError_WhenSecurityIsOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowOverrideOverallRiskStatus.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedOverrideOverallRiskStatusWithShowError((IBusiness)CreateNewShipment, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.ShipmentsComplianceAllowOverrideOverallRiskStatus.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentAllowOverrideFreightMovementRestrictions_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowOverrideFreightMovementRestrictions.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedOverrideFreightMovementRestrictionsWithShowError((IBusiness)CreateNewShipment, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentAllowOverrideFreightMovementRestrictions_WhenSecurityIsOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowOverrideFreightMovementRestrictions.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedOverrideFreightMovementRestrictionsWithShowError((IBusiness)CreateNewShipment, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.ShipmentsComplianceAllowOverrideFreightMovementRestrictions.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentsAllowResynchronizeComplianceRiskStatus_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowResynchronizeRiskStatus.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedResynchronizeComplianceRiskStatusWithShowError((IBusiness)CreateNewShipment, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShipmentsAllowResynchronizeComplianceRiskStatus_WhenSecurityIsOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ShipmentsComplianceAllowResynchronizeRiskStatus.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedResynchronizeComplianceRiskStatusWithShowError((IBusiness)CreateNewShipment, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.ShipmentsComplianceAllowResynchronizeRiskStatus.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Consolidations

		public void TestConsolidationAllowOverrideComplianceRisk_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ConsolidationsComplianceAllowOverrideOverallRiskStatus.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedOverrideOverallRiskStatusWithShowError((IBusiness)CreateNewConsolidation, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolidationAllowOverrideComplianceRiskWithShowError_WhenSecurityIsOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ConsolidationsComplianceAllowOverrideOverallRiskStatus.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedOverrideOverallRiskStatusWithShowError((IBusiness)CreateNewConsolidation, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.ConsolidationsComplianceAllowOverrideOverallRiskStatus.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolidationAllowOverrideFreightMovementRestrictions_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ConsolidationsComplianceAllowOverrideFreightMovementRestrictions.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedOverrideFreightMovementRestrictionsWithShowError((IBusiness)CreateNewConsolidation, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolidationAllowOverrideFreightMovementRestrictions_WhenSecurityIsOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ConsolidationsComplianceAllowOverrideFreightMovementRestrictions.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedOverrideFreightMovementRestrictionsWithShowError((IBusiness)CreateNewConsolidation, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.ConsolidationsComplianceAllowOverrideFreightMovementRestrictions.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolidationsAllowResynchronizeComplianceRiskStatus_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceAllowResynchronizeRiskStatus.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedResynchronizeComplianceRiskStatusWithShowError((IBusiness)CreateNewConsolidation, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolidationsAllowResynchronizeComplianceRiskStatus_WhenSecurityIsOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.ConsolidationsComplianceAllowResynchronizeRiskStatus.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedResynchronizeComplianceRiskStatusWithShowError((IBusiness)CreateNewConsolidation, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.ConsolidationsComplianceAllowResynchronizeRiskStatus.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestConsolidationsAllowEditHarmonizedCode()
		{
			Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedEditHarmonizedCodeWithShowError((IBusiness)CreateNewConsolidation, showErrorWhenNotAllowed: true));
			AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region Quoted Booking

		public void TestQuotedBookingAllowDeclineComplianceAssessment_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceDeclineComplianceAssessment.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedDeclineComplianceAssessmentWithShowError((IBusiness)CreateNewBookingQuick, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuotedBookingAllowDeclineComplianceAssessment_WhenSecurityIOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceDeclineComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedDeclineComplianceAssessmentWithShowError((IBusiness)CreateNewBookingQuick, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.BookingsComplianceDeclineComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuotedBookingAllowEditComplianceAssessment_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceEditComplianceAssessment.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedEditComplianceAssessmentWithShowError((IBusiness)CreateNewBookingQuick, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuotedBookingAllowEditComplianceAssessment_WhenSecurityIOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceEditComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedEditComplianceAssessmentWithShowError((IBusiness)CreateNewBookingQuick, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.BookingsComplianceEditComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuotedBookingAllowEditHarmonizedCode_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceEditHarmonizedCode.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedEditHarmonizedCodeWithShowError((IBusiness)CreateNewBookingQuick, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuotedBookingAllowEditHarmonizedCode_WhenSecurityIOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceEditHarmonizedCode.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedEditHarmonizedCodeWithShowError((IBusiness)CreateNewBookingQuick, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.BookingsComplianceEditHarmonizedCode.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuotedBookingAllowOverrideComplianceRisk_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceAllowOverrideOverallRiskStatus.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedOverrideOverallRiskStatusWithShowError((IBusiness)CreateNewBookingQuick, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuotedBookingAllowOverrideComplianceRiskWithShowError_WhenSecurityIsOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceAllowOverrideOverallRiskStatus.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedOverrideOverallRiskStatusWithShowError((IBusiness)CreateNewBookingQuick, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.BookingsComplianceAllowOverrideOverallRiskStatus.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuotedBookingAllowOverrideFreightMovementRestrictions_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceAllowOverrideFreightMovementRestrictions.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedOverrideFreightMovementRestrictionsWithShowError((IBusiness)CreateNewBookingQuick, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuotedBookingAllowOverrideFreightMovementRestrictions_WhenSecurityIsOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceAllowOverrideFreightMovementRestrictions.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedOverrideFreightMovementRestrictionsWithShowError((IBusiness)CreateNewBookingQuick, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.BookingsComplianceAllowOverrideFreightMovementRestrictions.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuotedBookingAllowResynchronizeComplianceRiskStatus_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceAllowResynchronizeRiskStatus.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedResynchronizeComplianceRiskStatusWithShowError((IBusiness)CreateNewBookingQuick, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestQuotedBookingAllowResynchronizeComplianceRiskStatus_WhenSecurityIsOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BookingsComplianceAllowResynchronizeRiskStatus.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedResynchronizeComplianceRiskStatusWithShowError((IBusiness)CreateNewBookingQuick, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.BookingsComplianceAllowResynchronizeRiskStatus.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Agency Booking

		public void TestAgencyBookingAllowDeclineComplianceAssessment_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.AgencyBookingsComplianceDeclineComplianceAssessment.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedDeclineComplianceAssessmentWithShowError((IBusiness)CreateNewAgencyBooking, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAgencyBookingAllowDeclineComplianceAssessment_WhenSecurityIOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.AgencyBookingsComplianceDeclineComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedDeclineComplianceAssessmentWithShowError((IBusiness)CreateNewAgencyBooking, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.AgencyBookingsComplianceDeclineComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAgencyBookingAllowEditComplianceAssessment_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.AgencyBookingsComplianceEditComplianceAssessment.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedEditComplianceAssessmentWithShowError((IBusiness)CreateNewAgencyBooking, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAgencyBookingAllowEditComplianceAssessment_WhenSecurityIOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.AgencyBookingsComplianceEditComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedEditComplianceAssessmentWithShowError((IBusiness)CreateNewAgencyBooking, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.AgencyBookingsComplianceEditComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAgencyBookingAllowEditHarmonizedCode_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.AgencyBookingsComplianceEditHarmonizedCode.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedEditHarmonizedCodeWithShowError((IBusiness)CreateNewAgencyBooking, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAgencyBookingAllowEditHarmonizedCode_WhenSecurityIOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.AgencyBookingsComplianceEditHarmonizedCode.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedEditHarmonizedCodeWithShowError((IBusiness)CreateNewAgencyBooking, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.AgencyBookingsComplianceEditHarmonizedCode.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAgencyBookingAllowOverrideComplianceRisk_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.AgencyBookingsComplianceAllowOverrideOverallRiskStatus.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedOverrideOverallRiskStatusWithShowError((IBusiness)CreateNewAgencyBooking, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAgencyBookingAllowOverrideComplianceRiskWithShowError_WhenSecurityIsOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.AgencyBookingsComplianceAllowOverrideOverallRiskStatus.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedOverrideOverallRiskStatusWithShowError((IBusiness)CreateNewAgencyBooking, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.AgencyBookingsComplianceAllowOverrideOverallRiskStatus.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Bill of Lading

		public void TestBillsOfLadingAllowDeclineComplianceAssessment_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BillsOfLadingComplianceDeclineComplianceAssessment.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedDeclineComplianceAssessmentWithShowError((IBusiness)CreateNewBillOfLading, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBillsOfLadingAllowDeclineComplianceAssessment_WhenSecurityIOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BillsOfLadingComplianceDeclineComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedDeclineComplianceAssessmentWithShowError((IBusiness)CreateNewBillOfLading, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.BillsOfLadingComplianceDeclineComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBillsOfLadingAllowEditComplianceAssessment_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BillsOfLadingComplianceEditComplianceAssessment.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedEditComplianceAssessmentWithShowError((IBusiness)CreateNewBillOfLading, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBillsOfLadingAllowEditComplianceAssessment_WhenSecurityIOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BillsOfLadingComplianceEditComplianceAssessment.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedEditComplianceAssessmentWithShowError((IBusiness)CreateNewBillOfLading, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.BillsOfLadingComplianceEditComplianceAssessment.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBillsOfLadingAllowEditHarmonizedCode_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BillsOfLadingComplianceEditHarmonizedCode.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedEditHarmonizedCodeWithShowError((IBusiness)CreateNewBillOfLading, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBillsOfLadingAllowEditHarmonizedCode_WhenSecurityIOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BillsOfLadingComplianceEditHarmonizedCode.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedEditHarmonizedCodeWithShowError((IBusiness)CreateNewBillOfLading, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.BillsOfLadingComplianceEditHarmonizedCode.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBillsOfLadingAllowOverrideComplianceRisk_WhenSecurityIsON()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BillsOfLadingComplianceAllowOverrideOverallRiskStatus.IsAllowed = true;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns true", ComplianceRiskSecurityRights.IsAllowedOverrideOverallRiskStatusWithShowError((IBusiness)CreateNewBillOfLading, showErrorWhenNotAllowed: false));
				AssertNullOrEmptyOrWhitespace(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestBillsOfLadingAllowOverrideComplianceRiskWithShowError_WhenSecurityIsOFF()
		{
			var securityCore = CreateNewSecurityCore;
			securityCore.BillsOfLadingComplianceAllowOverrideOverallRiskStatus.IsAllowed = false;

			using (Env.SetTemporarySecurityInstanceForTest(securityCore))
			{
				Assert("Security checkpoint returns false", !ComplianceRiskSecurityRights.IsAllowedOverrideOverallRiskStatusWithShowError((IBusiness)CreateNewBillOfLading, showErrorWhenNotAllowed: true));
				AssertEquals(securityCore.BillsOfLadingComplianceAllowOverrideOverallRiskStatus.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			UnitTestUserNotification.Instance.ClearMessages();
		}

		#endregion
	}
}
