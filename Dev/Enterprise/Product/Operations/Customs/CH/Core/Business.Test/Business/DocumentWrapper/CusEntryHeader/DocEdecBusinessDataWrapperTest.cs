using CargoWise.Types;
using Enterprise.Customs.Common.CH;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.Business.Testing;

[TestedType(typeof(DocEdecBusinessDataWrapper))]
sealed class DocEdecBusinessDataWrapperTest : DocumentWrapperTestCase
{
	public void TestVATAccount() => CombineAssertions(() =>
	{
		SetupDeclarationTraders();

		var businessDataWrapper = DocEdecBusinessDataWrapper.New(EntryHeader, Factory);

		EntryHeader.Declaration.JE_VATPaidBy = DeclarationPayerList.Codes.Importer;
		AssertEquals("VATAccount should be IMPCAV78", "IMPCAV78", businessDataWrapper.VATAccount);

		EntryHeader.Declaration.JE_VATPaidBy = DeclarationPayerList.Codes.Cash;
		AssertEquals("VATAccount should be empty - cash", ZString.Empty, businessDataWrapper.VATAccount);
	});

	public void TestCustomsAccount() => CombineAssertions(() =>
	{
		SetupDeclarationTraders();

		var businessDataWrapper = DocEdecBusinessDataWrapper.New(EntryHeader, Factory);

		EntryHeader.Declaration.JE_PaymentMethod = DeclarationPayerList.Codes.Importer;
		AssertEquals("VATAccount should be IMPCAD78", "IMPCAD78", businessDataWrapper.CustomsAccount);

		EntryHeader.Declaration.JE_PaymentMethod = DeclarationPayerList.Codes.Cash;
		AssertEquals("VATAccount should be empty - cash", ZString.Empty, businessDataWrapper.CustomsAccount);
	});

	void SetupDeclarationTraders()
	{
		var orgImporter = Factory.New<OrgHeader>();
		orgImporter.OH_Code = "Importer";
		orgImporter.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.CAD, "IMPCAD78");
		orgImporter.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.CAV, "IMPCAV78");
		EntryHeader.Declaration.JE_OH_Importer = orgImporter.PK;
		EntryHeader.Declaration.JE_OA_ImporterAddress = orgImporter.MainAddress.PK;

		var orgSupplier = Factory.New<OrgHeader>();
		orgSupplier.OH_Code = "Supplier";
		orgSupplier.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.CAD, "SUPCAD78");
		orgSupplier.CustomsCodes.AddNew(OrgCusCode.SwissCodeTypes.CAV, "SUPCAV78");
		EntryHeader.Declaration.JE_OH_Supplier = orgSupplier.PK;
		EntryHeader.Declaration.JE_OA_SupplierAddress = orgSupplier.MainAddress.PK;
	}

	CusEntryHeader EntryHeader => entryHeader ??= GetNewEntryHeader();
	CusEntryHeader entryHeader;

	CusEntryHeader GetNewEntryHeader()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = CHJobMessageTypeList.Codes.Import;
		return declaration.CustomsEntryHeaders.AddNew();
	}

	public override DocumentWrapper[] GetDocumentWrappers()
	{
		return new DocumentWrapper[] { DocEdecBusinessDataWrapper.New(EntryHeader, Factory) };
	}

	protected override string TestingCountry
	{
		get { return Core.Constants.CountryCodes.Switzerland; }
	}
}
