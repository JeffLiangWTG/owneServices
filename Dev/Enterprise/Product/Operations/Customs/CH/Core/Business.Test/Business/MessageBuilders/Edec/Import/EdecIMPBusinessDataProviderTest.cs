using System;
using CargoWise.Customs.CH.MessageContracts.Edec.GoodsDeclarations;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business.Testing;

class EdecIMPBusinessDataProviderTest : EdecBusinessDataProviderTest
{
	protected override string MessageType => JobMessageTypeList.Codes.Import;

	protected override EdecBusinessDataProvider CreateEdecBusinessDataProvider(CusEntryHeader entryHeader) => EdecIMPBusinessDataProvider.New(entryHeader);

	protected override void SetOrganisationsVATNumber(string vATNumber) => declaration.Importer.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.VATCode, vATNumber, Core.Constants.CountryCodes.Switzerland);

	public override void TestProvider_FreeCarrierSeller()
	{
		declaration.JE_OH_Importer = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Switzerland, cadCode: "IMPCAV78", vatCode: ValidVATNumberCHE).PK;
		declaration.JE_OH_Supplier = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Germany, cavCode: "SUPCAD78").PK;

		base.TestProvider_FreeCarrierSeller();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(business.VATNumber), ValidVATNumberCHE, business.VATNumber);
			AssertEquals(nameof(business.VATSuffix), true, business.VATSuffix);
			AssertEquals(nameof(business.CustomsAccount), "IMPCAV78", business.CustomsAccount);
			AssertEquals(nameof(business.VATAccount), "-1", business.VATAccount);
		});
	}

	public override void TestProvider_DeliveredDutyPaid()
	{
		declaration.JE_OH_Importer = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Switzerland, cadCode: "IMPCAV78", vatCode: ValidVATNumberCHE).PK;
		declaration.JE_OH_Supplier = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Germany, cavCode: "SUPCAD78").PK;

		base.TestProvider_DeliveredDutyPaid();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(business.VATNumber), ValidVATNumberCHE, business.VATNumber);
			AssertEquals(nameof(business.VATSuffix), true, business.VATSuffix);
			AssertEquals(nameof(business.CustomsAccount), "-1", business.CustomsAccount);
			AssertEquals(nameof(business.VATAccount), "SUPCAD78", business.VATAccount);
		});
	}

	public void TestCustomsAccount()
	{
		CombineAssertions(() =>
		{
			AssertAccount(d => d.JE_PaymentMethodInfo, "CAD", b => b.CustomsAccount);
		});
	}

	public void TestVATAccount()
	{
		CombineAssertions(() =>
		{
			AssertAccount(d => d.JE_VATPaidByInfo, "CAV", b => b.VATAccount);
		});
	}

	void AssertAccount(Func<JobDeclaration, ZPropertyInfo> paymentPropertyInfoGetter, string codeType, Func<IEdecBusiness, string> acountGetter)
	{
		declaration.JE_OH_Supplier = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Germany, cadCode: "SUPCAD", cavCode: "SUPCAV").PK;
		declaration.JE_OH_Importer = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Switzerland, cadCode: "IMPCAD", cavCode: "IMPCAV").PK;
		var consignee = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Switzerland, cadCode: "CSECAD", cavCode: "CSECAV");
		declaration.JE_OH_Consignee = consignee.PK;
		declaration.JE_OA_ConsigneeAddress = consignee.MainAddress.PK;
		declaration.JE_OH_Forwarder = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Switzerland, cadCode: "FWDCAD", cavCode: "FWDCAV").PK;
		declaration.JE_OA_DeclarantAddress = CreateOrganisation(countryCode: Core.Constants.CountryCodes.Switzerland, cadCode: "DECCAD", cavCode: "DECCAV").MainAddress.PK;

		var paymentPropertyInfo = paymentPropertyInfoGetter(declaration);
		paymentPropertyInfo.Value = new ZString(DeclarationPayerList.Codes.Consignor);
		AssertEquals($"{paymentPropertyInfo.Name}={paymentPropertyInfo.Value}", $"SUP{codeType}", acountGetter(business));
		paymentPropertyInfo.Value = new ZString(DeclarationPayerList.Codes.Importer);
		AssertEquals($"{paymentPropertyInfo.Name}={paymentPropertyInfo.Value}", $"IMP{codeType}", acountGetter(business));
		paymentPropertyInfo.Value = new ZString(DeclarationPayerList.Codes.Consignee);
		AssertEquals($"{paymentPropertyInfo.Name}={paymentPropertyInfo.Value}", $"CSE{codeType}", acountGetter(business));
		paymentPropertyInfo.Value = new ZString(DeclarationPayerList.Codes.Forwarder);
		AssertEquals($"{paymentPropertyInfo.Name}={paymentPropertyInfo.Value}", $"FWD{codeType}", acountGetter(business));
		paymentPropertyInfo.Value = new ZString(DeclarationPayerList.Codes.Declarant);
		AssertEquals($"{paymentPropertyInfo.Name}={paymentPropertyInfo.Value}", $"DEC{codeType}", acountGetter(business));
		paymentPropertyInfo.Value = new ZString(DeclarationPayerList.Codes.Cash);
		AssertEquals($"{paymentPropertyInfo.Name}={paymentPropertyInfo.Value}", "0", acountGetter(business));
		paymentPropertyInfo.Value = ZString.Empty;
		AssertEquals($"{paymentPropertyInfo.Name}={paymentPropertyInfo.Value}", "-1", acountGetter(business));
	}
}
