using System.Collections.Generic;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.GB.Business.Declaration.Testing
{
	[TestedType(typeof(AdditionalInfo))]
	public class AdditionalInfoTest : Customs.Business.Testing.CusSupportingInfoTest<AdditionalInfo>
	{
		public void TestValidationType()
		{
			CombineAssertions(() =>
			{
				AssertType<ImportAdditionalInfoValidation>("Import", GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Import).Validation);
				AssertType<AdditionalInfoValidation>("Export", GetAdditionalInfo(Factory, JobMessageTypeList.Codes.Export).Validation);
			});
		}
		public void TestSetDescriptionFromWarehouseWhenPrems()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var proc = helper.CreateRefCusProcedure("GB", ZString.Empty, "71", "71", "000", "Unit Test", "IMP", true, false, true, true, "H1");
			proc.ZZ6_ZZZ_NKDataGrouping = "CDS";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = "IMP";
			dec.JE_ApplicationCode = "CDS";

			var someWH = Factory.New<OrgHeader>();
			someWH.OH_FullName = "SOME WAREHOUSE LTD.";
			someWH.CompanyData.OB_IMUsedBondedWhs = true;
			someWH.CustomsCodes.AddNew(OrgCusCode.CodeTypes.ControlledPremisesID, "WH1EORI11111111", Core.Constants.CountryCodes.France);
			var address = someWH.Addresses.AddNew(OrgAddressType.Office, true);
			address.Address1 = "12 UNIT TEST STREET";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var cei = dec.CusEntryInstruction;
			cei.CEI_OA_Warehouse2 = address.PK;
			cei.CEI_Style = "H1";

			var inv = dec.Invoices.AddNew();
			var invLine = inv.InvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			invLine.JI_Procedure = proc.FullCodeCurrentPlusPreviousPlusConcession;

			var addInfo = dec.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "PREMS";

			// Test - Country code dropped from address
			AssertEquals("SOME WAREHOUSE LTD. 12 UNIT TEST STREET-FR", addInfo.CSI_Description);

			// Test - Long Names and addresses
			var longName = string.Empty.PadLeft(someWH.OH_FullNameInfo.MaxLength, 'X');
			someWH.OH_FullName = longName;
			var longAddress = string.Empty.PadLeft(address.Address1Info.MaxLength, 'Y');
			address.Address1 = longAddress;

			addInfo.CSI_Code = string.Empty;
			addInfo.CSI_Code = "PREMS";

			var expected = string.Format(CultureInfo.InvariantCulture, "{0}-{1}", address.AddressAsASingleLine.Substring(0, additionalInfo.CSI_DescriptionInfo.MaxLength - 3), Core.Constants.CountryCodes.France);

			AssertEquals(expected, addInfo.CSI_Description);
		}

		public void TestRefCusCode()
		{
			var refCusCodeCDS = Factory.New<ZZRefCusCodeListCombined>();
			refCusCodeCDS.ZZD_CodeType = "ADDIN";
			refCusCodeCDS.ZZD_Code = "00500";
			refCusCodeCDS.ZZD_CountryOrGrouping = "CDS";
			var attribute = refCusCodeCDS.Attributes.AddNew();
			attribute.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.Level;
			attribute.ZZE_Value = RefCusCodeListLevelType.Item;

			var refCusCodeCHF = Factory.New<ZZRefCusCodeListCombined>();
			refCusCodeCHF.ZZD_CodeType = "ADDIN";
			refCusCodeCHF.ZZD_Code = "00500";
			refCusCodeCHF.ZZD_CountryOrGrouping = "GB";
			var attribute2 = refCusCodeCHF.Attributes.AddNew();
			attribute2.ZZE_ZXE_NKName = RefCusCodeListAttributeTypes.Codes.Level;
			attribute2.ZZE_Value = RefCusCodeListLevelType.Header;

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CDS";

			var addInfo = dec.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "00500";
			AssertEquals("Should get CDS RefCusCode", refCusCodeCDS.PK, addInfo.RefCusCode.PK);
			AssertEquals("Should have LEVEL - ITEM attribute", true, addInfo.RefCusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Item));

			dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = "CHF";
			addInfo = dec.AdditionalInfos.AddNew();
			addInfo.CSI_Code = "00500";
			AssertEquals("Should get CHF RefCusCode", refCusCodeCHF.PK, addInfo.RefCusCode.PK);
			AssertEquals("Should have LEVEL - HEADER attribute", true, addInfo.RefCusCode.HasAttribute(RefCusCodeListAttributeTypes.Codes.Level, RefCusCodeListLevelType.Header));
		}

		protected override IEnumerable<AdditionalInfo> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.New<JobDeclaration>();
			yield return declaration.AdditionalInfos.AddNew();
			var invoice = declaration.Invoices.AddNew();
			yield return invoice.AdditionalInfos.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			yield return invoiceLine.AdditionalInfos.AddNew();
			var product = factory.New<EU.Business.MasterFiles.OrgSupplierPart>();
			product.OP_PartNum = "POOPY";
			var relationship = product.RelatedOrganisations.AddNew();
			relationship.OU_Relationship = "BTH";
			relationship.OU_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationType.Both;
			yield return (AdditionalInfo)pivot.AdditionalInfos.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice = declaration.Invoices.AddNew();
			return invoice.AdditionalInfos.AddNew();
		}

		static AdditionalInfo GetAdditionalInfo(BusinessObjectFactory factory, string messageType = null)
		{
			var declaration = factory.New<JobDeclaration>();
			if (!string.IsNullOrEmpty(messageType))
			{
				declaration.JE_MessageType = messageType;
			}
			var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			return invoiceLine.AdditionalInfos.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			additionalInfo = invoiceHeader.AdditionalInfos.AddNew();
		}

		AdditionalInfo additionalInfo;
		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;
	}
}
