using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.Customs.FR.Business.MessagesWrappers.Common;
using Enterprise.Customs.FR.Messaging.Interfaces.Common;
using Enterprise.Customs.FR.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Moq;
using DeltaG1SendExport = CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Send.Export;
using DeltaG1SendImport = CargoWise.Customs.FR.MessageDefinitions.DeltaG1.Send.Import;
using DeltaG2SendExport = CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Export;
using DeltaG2SendImport = CargoWise.Customs.FR.MessageDefinitions.DeltaG2.Send.Import;

namespace Enterprise.Customs.FR.Messaging.Testing
{
	class MessageBuilderHelperTest : TestCaseWithFactory
	{
		public void TestTrailingZerosRemover()
		{
			var helper = new MessageBuilderHelper();
			AssertEquals(0m, helper.RemoveTrailingZeros(0.000m, 10));
			AssertEquals(0m, helper.RemoveTrailingZeros(0.00010m, 3));
			AssertEquals(0.001m, helper.RemoveTrailingZeros(0.00090m, 3));
			AssertEquals(0.145m, helper.RemoveTrailingZeros(0.145000m, 3));
			AssertEquals(100.4m, helper.RemoveTrailingZeros(100.4000m, 3));
		}

		public void TestLoadUniSpe()
		{
			var messageBuilderHelper = new MessageBuilderHelper();
			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var entryLine = entryHeader.AllEntryLines.AddNew() as CusEntryLine;
			invoiceLine.JI_CL = entryLine.PK;
			invoiceLine.JI_CustomsThirdUnitQty = "ABCD";
			invoiceLine.JI_CustomsThirdQuantity = 10m;

			var thirdUnit = new ThirdUnitWrapper(entryLine);
			var uniSpe1 = messageBuilderHelper.LoadUniSpe<DeltaG1SendExport.TUniSpe>(thirdUnit);
			AssertType<DeltaG1SendExport.TUniSpe>(uniSpe1);
			AssertEquals("ABC", uniSpe1.Unispe);
			AssertEquals(10m, uniSpe1.Nbrunispe);
			AssertEquals("D", uniSpe1.Qualifunispe);

			var uniSpe2 = messageBuilderHelper.LoadUniSpe<DeltaG1SendImport.TUniSpe>(thirdUnit);
			AssertType<DeltaG1SendImport.TUniSpe>(uniSpe2);
			AssertEquals("ABC", uniSpe2.Unispe);
			AssertEquals(10m, uniSpe2.Nbrunispe);
			AssertEquals("D", uniSpe2.Qualifunispe);

			var uniSpe3 = messageBuilderHelper.LoadUniSpe<DeltaG2SendExport.TUniSpe>(thirdUnit);
			AssertType<DeltaG2SendExport.TUniSpe>(uniSpe3);
			AssertEquals("ABC", uniSpe3.Unispe);
			AssertEquals(10m, uniSpe3.Nbrunispe);
			AssertEquals("D", uniSpe3.Qualifunispe);

			var uniSpe4 = messageBuilderHelper.LoadUniSpe<DeltaG2SendImport.TUniSpe>(thirdUnit);
			AssertType<DeltaG2SendImport.TUniSpe>(uniSpe4);
			AssertEquals("ABC", uniSpe4.Unispe);
			AssertEquals(10m, uniSpe4.Nbrunispe);
			AssertEquals("D", uniSpe4.Qualifunispe);
		}

		public void TestLoadTrader()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "TST_NJ_1";
			var customsCode = orgHeader.CustomsCodes.AddNew();
			customsCode.OK_CodeType = OrgCusCode.FranceCodeTypes.TVA;
			customsCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.France;
			customsCode.OK_CustomsRegNo = "tin";

			orgHeader.OH_FullName = "nomoperateur";
			orgHeader.MainAddress.Address1 = "rueoperateur";
			orgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;
			orgHeader.MainAddress.OA_PostCode = "123456";
			orgHeader.MainAddress.OA_City = "NanJing";
			var euImpAddInfo = EUOrgImpAddInfo.Get(orgHeader, Core.Constants.CountryCodes.France);
			euImpAddInfo.ZO_UseFr3FiscalRepresentation = true;
			Factory.Save();

			var fiscalOrgHeader = Factory.New<OrgHeader>();
			fiscalOrgHeader.OH_Code = "TST_SH_1";
			fiscalOrgHeader.OH_FullName = "ShanghaiOffice";
			fiscalOrgHeader.MainAddress.Address1 = "Pudong";
			fiscalOrgHeader.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			fiscalOrgHeader.MainAddress.OA_PostCode = "456789";
			fiscalOrgHeader.MainAddress.OA_City = "Shanghai";

			var messageBuilderHelper = new MessageBuilderHelper();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_OH_Importer = orgHeader.PK;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var entryLine = entryHeader.AllEntryLines.AddNew() as CusEntryLine;
			invoiceLine.JI_CL = entryLine.PK;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			var fiscalRef = entryInstruction.FiscalReferences.AddNew();
			fiscalRef.CFR_Code = FiscalReferenceCodeList.Codes.FR3_TaxRepresentative;
			fiscalRef.CFR_Reference = "TestOrgNO001";
			fiscalRef.CFR_OA_Owner = fiscalOrgHeader.MainAddress.PK;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var itemErrorCollector = new ErrorCollector();
			var repTaxOrganisation = RepTaxOrganisationWrapper.New(entryHeader, itemErrorCollector);
			var uniTrader1 = messageBuilderHelper.LoadTrader<DeltaG1SendExport.TTrader>(repTaxOrganisation);
			AssertType<DeltaG1SendExport.TTrader>(uniTrader1);
			AssertEquals("TestOrgNO001", uniTrader1.Tin);
			AssertEquals("ShanghaiOffice", uniTrader1.Nomoperateur);
			AssertEquals("Pudong", uniTrader1.Rueoperateur);
			AssertEquals("CN", uniTrader1.Paysoperateur);
			AssertEquals("456789", uniTrader1.Codepostaloperateur);
			AssertEquals("Shanghai", uniTrader1.Villeoperateur);

			fiscalRef.CFR_Reference = "";
			uniTrader1 = messageBuilderHelper.LoadTrader<DeltaG1SendExport.TTrader>(repTaxOrganisation, false);
			AssertEquals(string.Empty, uniTrader1.Tin);
			fiscalRef.CFR_Reference = "TestOrgNO001";

			var uniTrader2 = messageBuilderHelper.LoadTrader<DeltaG1SendImport.TTrader>(repTaxOrganisation);
			AssertType<DeltaG1SendImport.TTrader>(uniTrader2);
			AssertEquals("TestOrgNO001", uniTrader2.Tin);
			AssertEquals("ShanghaiOffice", uniTrader2.Nomoperateur);
			AssertEquals("Pudong", uniTrader2.Rueoperateur);
			AssertEquals("CN", uniTrader2.Paysoperateur);
			AssertEquals("456789", uniTrader2.Codepostaloperateur);
			AssertEquals("Shanghai", uniTrader2.Villeoperateur);

			fiscalRef.CFR_Reference = "";
			uniTrader2 = messageBuilderHelper.LoadTrader<DeltaG1SendImport.TTrader>(repTaxOrganisation, false);
			AssertEquals(string.Empty, uniTrader2.Tin);
			fiscalRef.CFR_Reference = "TestOrgNO001";

			var uniTrader3 = messageBuilderHelper.LoadTrader<DeltaG2SendExport.TTrader>(repTaxOrganisation);
			AssertType<DeltaG2SendExport.TTrader>(uniTrader3);
			AssertEquals("TestOrgNO001", uniTrader3.Tin);
			AssertEquals("ShanghaiOffice", uniTrader3.Nomoperateur);
			AssertEquals("Pudong", uniTrader3.Rueoperateur);
			AssertEquals("CN", uniTrader3.Paysoperateur);
			AssertEquals("456789", uniTrader3.Codepostaloperateur);
			AssertEquals("Shanghai", uniTrader3.Villeoperateur);

			fiscalRef.CFR_Reference = "";
			uniTrader3 = messageBuilderHelper.LoadTrader<DeltaG2SendExport.TTrader>(repTaxOrganisation, false);
			AssertEquals(string.Empty, uniTrader3.Tin);
			fiscalRef.CFR_Reference = "TestOrgNO001";

			var uniTrader4 = messageBuilderHelper.LoadTrader<DeltaG2SendImport.TTrader>(repTaxOrganisation);
			AssertType<DeltaG2SendImport.TTrader>(uniTrader4);
			AssertEquals("TestOrgNO001", uniTrader4.Tin);
			AssertEquals("ShanghaiOffice", uniTrader4.Nomoperateur);
			AssertEquals("Pudong", uniTrader4.Rueoperateur);
			AssertEquals("CN", uniTrader4.Paysoperateur);
			AssertEquals("456789", uniTrader4.Codepostaloperateur);
			AssertEquals("Shanghai", uniTrader4.Villeoperateur);

			fiscalRef.CFR_Reference = "";
			uniTrader4 = messageBuilderHelper.LoadTrader<DeltaG2SendImport.TTrader>(repTaxOrganisation, false);
			AssertEquals(string.Empty, uniTrader4.Tin);
		}

		public void TestLoadTraderTinValueDependingOnEORCode()
		{
			var messageBuilderHelper = new MessageBuilderHelper();

			var organisationMockEU = new Mock<IOrganisation> { CallBase = true };
			organisationMockEU.Setup(m => m.OrganisationNumber).Returns("FR31501335900155");
			organisationMockEU.Setup(m => m.OrganisationNumberEoriOnly).Returns("FR31501335900156");
			organisationMockEU.Setup(m => m.FullName).Returns("GE MEDICAL SYSTEMMS SCS");
			organisationMockEU.Setup(m => m.Address).Returns("RUE DE LA MINIERE");
			organisationMockEU.Setup(m => m.CountryCode).Returns("FR");
			organisationMockEU.Setup(m => m.PostCode).Returns("78533");
			organisationMockEU.Setup(m => m.City).Returns("BUC CEDEX");
			organisationMockEU.Setup(m => m.PartnerDestIDInfo).Returns("");

			var traderEU = messageBuilderHelper.LoadTrader<DeltaG2SendImport.TTrader>(organisationMockEU.Object, false);
			AssertNotNullOrEmpty(traderEU.Tin);

			var organisationMockGB = new Mock<IOrganisation> { CallBase = true };
			organisationMockGB.Setup(m => m.OrganisationNumber).Returns("GB31501335900155");
			organisationMockGB.Setup(m => m.OrganisationNumberEoriOnly).Returns("GB31501335900156");
			organisationMockGB.Setup(m => m.FullName).Returns("SYSTRON DONNER");
			organisationMockGB.Setup(m => m.Address).Returns("123 VICTORIA STREET");
			organisationMockGB.Setup(m => m.CountryCode).Returns("GB");
			organisationMockGB.Setup(m => m.PostCode).Returns("12345");
			organisationMockGB.Setup(m => m.City).Returns("LONDON");

			var traderGB = messageBuilderHelper.LoadTrader<DeltaG2SendImport.TTrader>(organisationMockGB.Object, false);
			AssertNullOrEmpty(traderGB.Tin);
		}

		public void TestLoadMenSpecTexteCodeList()
		{
			var messageBuilderHelper = new MessageBuilderHelper();
			var menspectexte = messageBuilderHelper.LoadMenSpecTexteCodeList<DeltaG1SendExport.TMenspectexte>(null);
			AssertEquals("Empty", 0, menspectexte.Count);

			var declaration = Factory.New<JobDeclaration>();
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			var entryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			var entryLine = entryHeader.AllEntryLines.AddNew() as CusEntryLine;
			invoiceLine.JI_CL = entryLine.PK;

			var additionalInfo1 = invoiceHeader.AdditionalInfos.AddNew();
			additionalInfo1.CSI_Code = "code1";
			additionalInfo1.CSI_Description = "description1";

			var additionalInfo2 = invoiceLine.AdditionalInfos.AddNew();
			additionalInfo2.CSI_Code = "code2";
			additionalInfo2.CSI_Description = "description2";

			var additionalInfoDocList = new List<TariffAdditionalCodeWrapper>();

			foreach (AdditionalInfo additionalInfoDoc in entryHeader.AdditionalInfos)
			{
				additionalInfoDocList.Add(new TariffAdditionalCodeWrapper(additionalInfoDoc));
			}

			foreach (AdditionalInfo additionalInfoDoc in entryLine.AdditionalInfos)
			{
				additionalInfoDocList.Add(new TariffAdditionalCodeWrapper(additionalInfoDoc));
			}
			additionalInfoDocList = additionalInfoDocList.OrderBy(x => x.Code).ToList();

			var menspectexte1 = messageBuilderHelper.LoadMenSpecTexteCodeList<DeltaG1SendExport.TMenspectexte>(additionalInfoDocList);
			AssertType<Collection<DeltaG1SendExport.TMenspectexte>>(menspectexte1);
			AssertEquals("code1", menspectexte1[0].Menspec);
			AssertEquals("description1", menspectexte1[0].Menspectexte);
			AssertEquals("code2", menspectexte1[1].Menspec);
			AssertEquals("description2", menspectexte1[1].Menspectexte);

			var menspectexte2 = messageBuilderHelper.LoadMenSpecTexteCodeList<DeltaG1SendImport.TMenspectexte>(additionalInfoDocList);
			AssertType<Collection<DeltaG1SendImport.TMenspectexte>>(menspectexte2);
			AssertEquals("code1", menspectexte2[0].Menspec);
			AssertEquals("description1", menspectexte2[0].Menspectexte);
			AssertEquals("code2", menspectexte2[1].Menspec);
			AssertEquals("description2", menspectexte2[1].Menspectexte);

			var menspectexte3 = messageBuilderHelper.LoadMenSpecTexteCodeList<DeltaG2SendExport.TMenspectexte>(additionalInfoDocList);
			AssertType<Collection<DeltaG2SendExport.TMenspectexte>>(menspectexte3);
			AssertEquals("code1", menspectexte3[0].Menspec);
			AssertEquals("description1", menspectexte3[0].Menspectexte);
			AssertEquals("code2", menspectexte3[1].Menspec);
			AssertEquals("description2", menspectexte3[1].Menspectexte);

			var menspectexte4 = messageBuilderHelper.LoadMenSpecTexteCodeList<DeltaG2SendImport.TMenspectexte>(additionalInfoDocList);
			AssertType<Collection<DeltaG2SendImport.TMenspectexte>>(menspectexte4);
			AssertEquals("code1", menspectexte4[0].Menspec);
			AssertEquals("description1", menspectexte4[0].Menspectexte);
			AssertEquals("code2", menspectexte4[1].Menspec);
			AssertEquals("description2", menspectexte4[1].Menspectexte);
		}

		public void TestGetYesOrNo()
		{
			AssertEquals("Y", MessageBuilderHelper.GetYesOrNo(true));
			AssertEquals("N", MessageBuilderHelper.GetYesOrNo(false));
		}

		public void TestGetOptionalPositiveInteger()
		{
			AssertEquals(null, MessageBuilderHelper.GetOptionalPositiveInteger(-12));
			AssertEquals(null, MessageBuilderHelper.GetOptionalPositiveInteger(0));
			AssertEquals("12", MessageBuilderHelper.GetOptionalPositiveInteger(12));
		}

		public void TestGetOptionalDecimal()
		{
			AssertEquals(null, MessageBuilderHelper.GetOptionalDecimal(0m, 0));
			AssertEquals("1", MessageBuilderHelper.GetOptionalDecimal(1m, 0));
			AssertEquals("1.00", MessageBuilderHelper.GetOptionalDecimal(1m, 2));
			AssertEquals("1", MessageBuilderHelper.GetOptionalDecimal(1.23m, 0));
		}
	}
}
