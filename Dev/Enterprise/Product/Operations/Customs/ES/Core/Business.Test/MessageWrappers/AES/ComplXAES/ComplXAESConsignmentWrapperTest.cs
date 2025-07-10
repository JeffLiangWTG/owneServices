using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class ComplXAESConsignmentWrapperTest : WrapperHelperTest<ComplXAESConsignmentWrapper>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown("Constructor Throws Exception if entryHeader is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","entryHeader"), () => GetWrapper(null));

				var entryHeader = Factory.New<CusEntryHeader>();
				AssertExceptionThrown("Constructor Throws Exception if jobDeclaration is null", typeof(ArgumentNullException),
					ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","Declaration"), () => GetWrapper(entryHeader));
			});
		}

		public void TestInlandModeOfTransport()
		{
			declaration.CustomsOffices.RemoveAndDeleteAll();
			CombineAssertions(() =>
			{
				AssertEquals("Expected empty InlandModeOfTransport when CustomsOfficeOfExport and CustomsOffice of Exit are the same (empty)", ZString.Empty, wrapper.InlandModeOfTransport);

				declaration.JE_CustomsOffice = "ES009999";

				var customsOfficeExit = declaration.CustomsOffices.AddNew();
				customsOfficeExit.CY_Code = EU.Business.EuOfficeCodesTypes.Codes.OfficeOfExit;
				customsOfficeExit.CY_Data = "FR008889";

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Sea;
				AssertEquals("Expected filled InlandModeOfTransport Sea (1)", "1", wrapper.InlandModeOfTransport);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Rail;
				AssertEquals("Expected filled InlandModeOfTransport Rail (2)", "2", wrapper.InlandModeOfTransport);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
				AssertEquals("Expected filled InlandModeOfTransport Road (3)", "3", wrapper.InlandModeOfTransport);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Air;
				AssertEquals("Expected filled InlandModeOfTransport Air (4)", "4", wrapper.InlandModeOfTransport);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Mail;
				AssertEquals("Expected filled InlandModeOfTransport Mail (5)", "5", wrapper.InlandModeOfTransport);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.FixedTransportInstallations;
				AssertEquals("Expected filled InlandModeOfTransport FixedTransportInstallations (7)", "7", wrapper.InlandModeOfTransport);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.InlandWaterwayTransport;
				AssertEquals("Expected filled InlandModeOfTransport InlandWaterwayTransport (8)", "8", wrapper.InlandModeOfTransport);

				declaration.JE_TransportModeInland = Core.Constants.TransportModes.OwnPropulsion;
				AssertEquals("Expected filled InlandModeOfTransport OwnPropulsion (9)", "9", wrapper.InlandModeOfTransport);

				customsOfficeExit.CY_Data = ZString.Empty;
				AssertEquals("Expected empty InlandModeOfTransport when CustomsOfficeOfExport and CustomsOffice of Exit are the same", ZString.Empty, wrapper.InlandModeOfTransport);
			});
		}

		public void TestModeOfTransportAtBorder()
		{
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled ModeOfTransportAtBorder Sea (1)", "1", wrapper.ModeOfTransportAtBorder);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Rail;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled ModeOfTransportAtBorder Rail (2)", "2", wrapper.ModeOfTransportAtBorder);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled ModeOfTransportAtBorder Road (3)", "3", wrapper.ModeOfTransportAtBorder);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Air;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled ModeOfTransportAtBorder Air (4)", "4", wrapper.ModeOfTransportAtBorder);

				declaration.JE_TransportMode = Core.Constants.TransportModes.Mail;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled ModeOfTransportAtBorder Mail (5)", "5", wrapper.ModeOfTransportAtBorder);

				declaration.JE_TransportMode = Core.Constants.TransportModes.FixedTransportInstallations;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled ModeOfTransportAtBorder FixedTransportInstallations (7)", "7", wrapper.ModeOfTransportAtBorder);

				declaration.JE_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled ModeOfTransportAtBorder InlandWaterwayTransport (8)", "8", wrapper.ModeOfTransportAtBorder);

				declaration.JE_TransportMode = Core.Constants.TransportModes.OwnPropulsion;
				wrapper = GetWrapper(entryHeader);
				AssertEquals("Expected filled ModeOfTransportAtBorder OwnPropulsion (9)", "9", wrapper.ModeOfTransportAtBorder);
			});
		}

		public void TestActiveBorderTransportMeans()
		{
			CombineAssertions(() =>
			{
				declaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
				wrapper = GetWrapper(entryHeader);

				AssertNull("Expected null ActiveBorderTransportMeans when no data declared (even if TransportMode is declared)", wrapper.ActiveBorderTransportMeans);

				declaration.ZG_BorderTransportMeans = "00";
				wrapper = GetWrapper(entryHeader);

				var activeBorderTransportMeans = wrapper.ActiveBorderTransportMeans;
				AssertNotNull("Expected filled ActiveBorderTransportMeans when TransportMode is not empty", activeBorderTransportMeans);
				AssertSame("Cached ActiveBorderTransportMeans", wrapper.ActiveBorderTransportMeans, activeBorderTransportMeans);

				declaration.JE_TransportMode = ZString.Empty;
				wrapper = GetWrapper(entryHeader);

				AssertNull("Expected null ActiveBorderTransportMeans when TransportMode is empty", wrapper.ActiveBorderTransportMeans);
			});
		}

		public void TestTransportChargesMoP()
		{
			invoiceHeader.ZG_TransportChargesMethodOfPayment = "A";
			AssertEquals("Expected filled TransportChargesMoP", "A", wrapper.TransportChargesMoP);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;

			invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "11";

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceHeader invoiceHeader;
		CusEntryHeader entryHeader;
		ComplXAESConsignmentWrapper wrapper;

		ComplXAESConsignmentWrapper GetWrapper(CusEntryHeader entryheader) => new ComplXAESConsignmentWrapper(entryheader);

		protected override ComplXAESConsignmentWrapper GetProvider() => wrapper;
	}
}
