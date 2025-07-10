using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	class PresentationCusExitReportValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckCER_Type()
		{
			var targetInfo = cusExitReport.CER_TypeInfo;
			var message = "Message type Exit Control Presentation (IE507) cannot be submitted, when the Export entry status is not Released for Export (IE529)";

			var declaration = Factory.New<JobDeclaration>();
			cusExitHeader.Parent = declaration;
			cusExitReport.Validation.ValidateCER_Type();
			AssertHasMessageErrorContaining("Declaration do not have entries.", targetInfo, message);

			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			cusExitReport.Validation.ValidateCER_Type();
			AssertHasMessageErrorContaining("Declaration do not have messages.", targetInfo, message);

			var message1 = entry1.Messages.AddNew();
			cusExitReport.Validation.ValidateCER_Type();
			AssertHasMessageErrorContaining("Declaration do not have IE529 messages.", targetInfo, message);

			message1.EM_ApplicationCode = "IEE";
			message1.EM_MessageType = "529";
			message1.EM_ReceiveTransmit = "RCV";
			cusExitReport.Validation.ValidateCER_Type();
			AssertHasMessageErrorContaining("IE529 messages are not processed ok.", targetInfo, message);

			message1.EM_Status = "PRS";
			cusExitReport.Validation.ValidateCER_Type();
			AssertNoMessageErrorContaining("IE529 messages are processed ok.", targetInfo, message);

			cusExitHeader.CXH_ParentID = ZGuid.BrettsGuid;
			AssertNoMessageErrorContaining("Exit Header not linked to declaration", targetInfo, message);

			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			cusExitHeader.Parent = shipment;
			declaration.CustomsEntryHeaders.AddNew();
			cusExitReport.Validation.ValidateCER_Type();
			AssertHasMessageErrorContaining("Exit Header linked to shipment, but not all entries are IE529", targetInfo, message);

			declaration.JE_MessageType = "IMP";
			cusExitReport.Validation.ValidateCER_Type();
			AssertNoMessageErrorContaining("No error when Import", targetInfo, message);
		}

		public void TestCheckCER_TransportMode()
		{
			var targetInfo = cusExitReport.CER_TransportModeInfo;
			var message = "are required when one of these is present.";

			cusExitReport.Validation.ValidateCER_TransportMode();
			AssertNoMessageErrorContaining("Not Required when none of the 4 fields have value.", targetInfo, message);

			cusExitReport.CER_TransportType = EU.ExitControl.Business.CusExitReportTransportTypeList.Codes._10;
			cusExitReport.Validation.ValidateCER_TransportMode();
			AssertHasMessageErrorContaining("Required when any of the 4 fields have value.", targetInfo, message);

			cusExitReport.CER_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			AssertNoMessageErrorContaining("Required when any of the 4 fields have value(validation passes).", targetInfo, message);
		}

		public void TestCheckCER_TransportType()
		{
			var targetInfo = cusExitReport.CER_TransportTypeInfo;
			var message = "are required when one of these is present.";

			cusExitReport.Validation.ValidateCER_TransportType();
			AssertNoMessageErrorContaining("Not Required when none of the 4 fields have value.", targetInfo, message);

			cusExitReport.CER_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			cusExitReport.Validation.ValidateCER_TransportType();
			AssertHasMessageErrorContaining("Required when any of the 4 fields have value.", targetInfo, message);

			cusExitReport.CER_TransportType = EU.ExitControl.Business.CusExitReportTransportTypeList.Codes._10;
			AssertNoMessageErrorContaining("Required when any of the 4 fields have value(validation passes).", targetInfo, message);
		}

		public void TestCheckCER_TransportID()
		{
			var targetInfo = cusExitReport.CER_TransportIDInfo;
			var message = "are required when one of these is present.";

			cusExitReport.Validation.ValidateCER_TransportID();
			AssertNoMessageErrorContaining("Not Required when none of the 4 fields have value.", targetInfo, message);

			cusExitReport.CER_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			cusExitReport.Validation.ValidateCER_TransportID();
			AssertHasMessageErrorContaining("Required when any of the 4 fields have value.", targetInfo, message);

			cusExitReport.CER_TransportID = "TRANS001";
			AssertNoMessageErrorContaining("Required when any of the 4 fields have value(validation passes).", targetInfo, message);
		}

		public void TestCheckCER_RN_NKTransportNationality()
		{
			var targetInfo = cusExitReport.CER_RN_NKTransportNationalityInfo;
			var message = "are required when one of these is present.";

			cusExitReport.Validation.ValidateCER_RN_NKTransportNationality();
			AssertNoMessageErrorContaining("Not Required when none of the 4 fields have value.", targetInfo, message);

			cusExitReport.CER_TransportMode = TransportTypeList.Codes.FixedTransportInstallations;
			cusExitReport.Validation.ValidateCER_RN_NKTransportNationality();
			AssertHasMessageErrorContaining("CER_Type PRE, CER_Behavior DIS, Required when any of the 4 fields have value.", targetInfo, message);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var exitReportObj = CusExitReportTest.GetNewBusinessObject(Factory);
			cusExitHeader = exitReportObj.header;
			cusExitReport = exitReportObj.report;
			cusExitReport.CER_Type = ExitReportTypeList.Codes.Presentation;
		}

		CusExitHeader cusExitHeader;
		CusExitReport cusExitReport;
	}
}
