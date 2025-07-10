using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonConsignmentWrapperTest : WrapperHelperTest<AESCommonConsignmentWrapper>
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

		public void TestIsContainerised()
		{
			CombineAssertions(() =>
			{
				AssertEquals("Expected false IsContainerised", false, wrapper.IsContainerised);

				var containerTag = "CONTAINER";
				var container = declaration.CusContainers.AddNew();
				container.CO_ContainerNumber = containerTag;
				invoiceLine.ContainersForInvoiceLinesForBindingOnly.FindByContainerNumber(containerTag).IsForInvoiceLine = true;
				AssertEquals("Expected true IsContainerised", true, wrapper.IsContainerised);
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

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.B;
				AssertEquals("Expected empty InlandModeOfTransport when entry instruction is B", ZString.Empty, wrapper.InlandModeOfTransport);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Rail;
				AssertEquals("Expected filled InlandModeOfTransport Rail (2)", "2", wrapper.InlandModeOfTransport);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.C;
				AssertEquals("Expected empty InlandModeOfTransport when entry instruction is C and isComplementaryCWithMRN is false", ZString.Empty, wrapper.InlandModeOfTransport);

				wrapper = GetWrapper(entryHeader, isComplementaryCWithMRN: true);
				AssertEquals("Expected filled InlandModeOfTransport Rail (2) when EntryInstruction is C but isComplementaryCWithMRN is true", "2", wrapper.InlandModeOfTransport);

				entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;
				declaration.JE_TransportModeInland = Core.Constants.TransportModes.Road;
				wrapper = GetWrapper(entryHeader);
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

		protected override void SetUp()
		{
			base.SetUp();

			declaration = Factory.New<JobDeclaration>();
			declaration.FillWithValidTestData();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;

			declaration.CustomsEntryInstructions.RemoveAndDeleteAll();
			entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_SubStyle = EntrySubStyleList.Codes.A;

			var invoiceHeader = declaration.Invoices.AddNew();
			invoiceLine = invoiceHeader.InvoiceLines.AddNew();

			var mergeResult = declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertEquals("Merge done", true, mergeResult);

			entryHeader = declaration.CustomsEntryHeaders[0];

			wrapper = GetWrapper(entryHeader);
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		JobComInvoiceLine invoiceLine;
		CusEntryHeader entryHeader;
		AESCommonConsignmentWrapper wrapper;

		AESCommonConsignmentWrapper GetWrapper(CusEntryHeader entryheader, bool isComplementaryCWithMRN = false) => new AESCommonConsignmentWrapper(entryheader, isComplementaryCWithMRN);

		protected override AESCommonConsignmentWrapper GetProvider() => wrapper;
	}
}
