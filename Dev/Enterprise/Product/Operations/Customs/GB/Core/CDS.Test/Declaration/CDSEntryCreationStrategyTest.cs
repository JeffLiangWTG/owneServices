using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.CDS.Testing
{
	class CDSEntryCreationStrategyTest : TestCaseWithFactory
	{
		public void TestMergeKeyContainsCEI()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv1 = dec.Invoices.AddNew();
			var invLine = inv1.JobComInvoiceLines.AddNew();
			invLine.ZG_MethodOfPayment = "E";
			invLine.JI_CEI = cei.PK;
			var mergeStrategy = dec.CreateEntryCreationStrategy();
			Assert(mergeStrategy.GetKeyForLine(invLine).Contains(invLine.ZG_MethodOfPayment));
		}

		public void TestMergeKeyContainsSeller()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv1 = dec.Invoices.AddNew();
			var invLine = inv1.JobComInvoiceLines.AddNew();

			var seller = Factory.NewWithValidTestData<OrgAddress>();
			seller.OA_Address1 = "Seller 2";
			inv1.JZ_OA_SellerAddress = seller.PK;

			invLine.JI_CEI = cei.PK;
			var mergeStrategy = dec.CreateEntryCreationStrategy();
			Assert(mergeStrategy.GetKeyForLine(invLine).Contains(inv1.JZ_OA_SellerAddress));
		}

		public void TestMergeKeyContainsBuyer()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv1 = dec.Invoices.AddNew();
			var invLine = inv1.JobComInvoiceLines.AddNew();

			var buyer = Factory.NewWithValidTestData<OrgAddress>();
			buyer.OA_Address1 = "Buyer 2";
			inv1.JZ_OA_SellerAddress = buyer.PK;

			invLine.JI_CEI = cei.PK;
			var mergeStrategy = dec.CreateEntryCreationStrategy();
			Assert(mergeStrategy.GetKeyForLine(invLine).Contains(inv1.JZ_OA_BuyerAddress));
		}

		public void TestMergeKeyContainsGoodsOriginForExport()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			dec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
			var cei = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var inv1 = dec.Invoices.AddNew();
			var invLine = inv1.JobComInvoiceLines.AddNew();
			invLine.JI_CEI = cei.PK;
			invLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.Finland;

			var mergeStrategy = dec.CreateEntryCreationStrategy();
			var key = mergeStrategy.GetKeyForLine(invLine);
			Assert(key.Contains((ZString)Core.Constants.CountryCodes.Finland));
		}

		public void TestMergeKeyContainsAdditionalInfoCodes()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var instr01 = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			var instr02 = dec.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();

			var addInfoDec = dec.AdditionalInfos.AddNew();
			addInfoDec.CSI_Code = "IMH01";

			var inv = dec.Invoices.AddNew();

			var addInfoInv = inv.AdditionalInfos.AddNew();
			addInfoInv.CSI_Code = "IMH02";

			var invLine01 = inv.JobComInvoiceLines.AddNew();
			invLine01.JI_CEI = instr01.PK;

			var addInfo_01 = invLine01.AdditionalInfos.AddNew();
			addInfo_01.CSI_Code = "IMI01";

			var invLine02 = inv.JobComInvoiceLines.AddNew();
			invLine02.JI_CEI = instr02.PK;

			var addInfo_02 = invLine02.AdditionalInfos.AddNew();
			addInfo_02.CSI_Code = "IMI02";

			var addInfo_03 = invLine02.AdditionalInfos.AddNew();
			addInfo_03.CSI_Code = "IMI03";

			var mergeStrategy = dec.CreateEntryCreationStrategy();

			Customs.Business.MergeKey headerKeyForLine01 = mergeStrategy.GetKeyForHeader(invLine01);

			Assert(!headerKeyForLine01.Contains(addInfoDec.CSI_Code));
			Assert(!headerKeyForLine01.Contains(addInfoInv.CSI_Code));
			Assert(!headerKeyForLine01.Contains(addInfo_01.CSI_Code));
			Assert(!headerKeyForLine01.Contains(addInfo_02.CSI_Code));
			Assert(!headerKeyForLine01.Contains(addInfo_03.CSI_Code));

			Customs.Business.MergeKey headerKeyForLine02 = mergeStrategy.GetKeyForHeader(invLine02);

			Assert(!headerKeyForLine02.Contains(addInfoDec.CSI_Code));
			Assert(!headerKeyForLine02.Contains(addInfoInv.CSI_Code));
			Assert(!headerKeyForLine02.Contains(addInfo_01.CSI_Code));
			Assert(!headerKeyForLine02.Contains(addInfo_02.CSI_Code));
			Assert(!headerKeyForLine02.Contains(addInfo_03.CSI_Code));

			Customs.Business.MergeKey lineKeyForLine01 = mergeStrategy.GetKeyForLine(invLine01);

			Assert(!lineKeyForLine01.Contains(addInfoDec.CSI_Code));
			Assert(lineKeyForLine01.Contains(addInfoInv.CSI_Code));
			Assert(lineKeyForLine01.Contains(addInfo_01.CSI_Code));
			Assert(!lineKeyForLine01.Contains(addInfo_02.CSI_Code));
			Assert(!lineKeyForLine01.Contains(addInfo_03.CSI_Code));

			Customs.Business.MergeKey lineKeyForLine02 = mergeStrategy.GetKeyForLine(invLine02);

			Assert(!lineKeyForLine02.Contains(addInfoDec.CSI_Code));
			Assert(lineKeyForLine02.Contains(addInfoInv.CSI_Code));
			Assert(!lineKeyForLine02.Contains(addInfo_01.CSI_Code));
			Assert(lineKeyForLine02.Contains(addInfo_02.CSI_Code));
			Assert(lineKeyForLine02.Contains(addInfo_03.CSI_Code));
		}
	}
}
