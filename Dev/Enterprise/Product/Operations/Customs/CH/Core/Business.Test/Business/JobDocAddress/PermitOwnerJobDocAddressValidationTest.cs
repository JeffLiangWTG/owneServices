using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.CH.Business.Testing;

sealed class PermitOwnerJobDocAddressValidationTest : JobDocAddressValidationTest
{
	public void TestCheckE2_OA_Address_NS30103()
	{
		Declaration.JE_MessageType = MessageTypeCodeList.Codes.Export;

		var permitOwner = Factory.New<OrgHeader>();
		Restriction.PermitOwnerDocAddress.OrganisationPK = permitOwner.PK;

		var expectedMessageErrorNotEntered = PassarValidationMessages.MessageNotEntered(PassarValidationMessages.NS30103, restriction.PermitOwnerDocAddress.AddressCaption);
		var expectedMessageErrorNotAllowed = PassarValidationMessages.MessageNS30103_PermitOwnerNotAllowed;
		CombineAssertions(() =>
		{
			AssertHasMessageErrorContaining("Assert if Permit Owner is Not Empty and ReferenceNumber is Empty have not allowed error", Restriction.PermitOwnerDocAddress.E2_OA_AddressInfo, expectedMessageErrorNotAllowed);
			AssertNoMessageErrorContaining("Assert if Permit Owner is Empty and ReferenceNumber is Empty don't have not entered error", Restriction.PermitOwnerDocAddress.E2_OA_AddressInfo, expectedMessageErrorNotEntered);

			Restriction.CSI_ReferenceNumber = "1";
			Restriction.PermitOwnerDocAddress.OrganisationPK = ZGuid.Empty;
			Restriction.PermitOwnerDocAddress.OrganisationPK = permitOwner.PK;
			AssertNoMessageErrorContaining("Assert if Permit Owner is Not Empty and ReferenceNumber is not Empty don't have not entered error", Restriction.PermitOwnerDocAddress.E2_OA_AddressInfo, expectedMessageErrorNotEntered);
			AssertNoMessageErrorContaining("Assert if Permit Owner is Not Empty and ReferenceNumber is not Empty don't have not allowed error", Restriction.PermitOwnerDocAddress.E2_OA_AddressInfo, expectedMessageErrorNotAllowed);

			Restriction.PermitOwnerDocAddress.OrganisationPK = ZGuid.Empty;
			AssertHasMessageErrorContaining("Assert if Permit Owner is Empty and ReferenceNumber is not Empty don't have not entered error", Restriction.PermitOwnerDocAddress.E2_OA_AddressInfo, expectedMessageErrorNotEntered);
			AssertNoMessageErrorContaining("Assert if Permit Owner is Empty and ReferenceNumber is Empty don't have not allowed error", Restriction.PermitOwnerDocAddress.E2_OA_AddressInfo, expectedMessageErrorNotAllowed);

			Restriction.CSI_ReferenceNumber = ZString.Empty;
			Restriction.PermitOwnerDocAddress.OrganisationPK = permitOwner.PK;
			Restriction.PermitOwnerDocAddress.OrganisationPK = ZGuid.Empty;
			AssertNoMessageErrorContaining("Assert if Permit Owner is Empty and ReferenceNumber is Empty don't have not entered error", Restriction.PermitOwnerDocAddress.E2_OA_AddressInfo, expectedMessageErrorNotEntered);
			AssertNoMessageErrorContaining("Assert if Permit Owner is Empty and ReferenceNumber is Empty don't have not allowed error", Restriction.PermitOwnerDocAddress.E2_OA_AddressInfo, expectedMessageErrorNotAllowed);
		});
	}

	JobComInvoiceLine CreateNewInvoiceLine()
	{
		var invLine = InvoiceHeader.InvoiceLines.AddNew();
		invLine.JI_CEI = EntryInstruction.PK;
		return invLine;
	}

	JobDeclaration Declaration => declaration ??= Factory.New<JobDeclaration>();
	JobDeclaration declaration;

	CusEntryInstruction EntryInstruction => entryInstruction ??= Declaration.CustomsEntryInstructions.AddNew();
	CusEntryInstruction entryInstruction;

	JobComInvoiceHeader InvoiceHeader => invoiceHeader ??= Declaration.Invoices.AddNew();
	JobComInvoiceHeader invoiceHeader;

	JobComInvoiceLine JobComInvoiceLine => jobComInvoiceLine ??= CreateNewInvoiceLine();
	JobComInvoiceLine jobComInvoiceLine;

	Restriction Restriction => restriction ??= JobComInvoiceLine.Restrictions.AddNew();
	Restriction restriction;
}
