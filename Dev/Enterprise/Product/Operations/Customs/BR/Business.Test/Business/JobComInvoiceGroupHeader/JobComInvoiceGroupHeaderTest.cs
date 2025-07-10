using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
	{
		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			ICommonInvoice commonInvoice = dec.TopGroupInvoice;
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			var customsChargeTypeList = new CustomsChargeTypeList();
			customsChargeTypeList.Sort();
			AssertEquals(customsChargeTypeList.CodesAsString, chargeTypeList1.CodesAsString);

			dec.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
			var importChargeTypeList = commonInvoice.ChargeTypeList;
			AssertContainsExactElementsInAnyOrder(new[] { "EIC", "OAC", "CIA", "CBC", "DEC", "ENG", "FIN", "IFE", "IFI", "ILO", "INS", "ISE", "ISI", "LOA", "LOE", "LOI", "MAT", "MCP", "DPA", "PAR", "ROT", "ROY", "TMM", "ONS", "FCO", "FNT", "OFC", "OFP" }, importChargeTypeList.GetAllCodes());

			dec.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			importChargeTypeList = commonInvoice.ChargeTypeList;
			AssertContainsExactElementsInAnyOrder(new[] { "EIC", "OAC", "CIA", "CBC", "DEC", "ENG", "FIN", "IFE", "IFI", "ILO", "INS", "ISE", "ISI", "LOA", "LOE", "LOI", "MAT", "MCP", "DPA", "PAR", "ROT", "ROY", "TMM", "ONS", "FCO", "FNT", "OFC", "OFP" }, importChargeTypeList.GetAllCodes());
		}

		public void TestIncoTermAndChargeFactory()
		{
			var declaration = Factory.New<JobDeclaration>();
			var groupHeader = declaration.JobComInvoiceGroupHeaders[0];

			CombineAssertions(() =>
			{
				foreach (var messageType in new BRJobMessageTypeList().GetAllCodesZString())
				{
					declaration.JE_MessageType = messageType;
					switch (messageType)
					{
						case BRJobMessageTypeList.Codes.Import:
						case BRJobMessageTypeList.Codes.ImportSiscomex:
						case BRJobMessageTypeList.Codes.WarehousedByExternalAgent:
							AssertType<ImportIncoTermAndCustomsChargeFactory>(messageType, groupHeader.IncoTermAndChargeFactory);
							break;
						case BRJobMessageTypeList.Codes.ImportLicense:
							AssertType<ImportLicenseIncoTermAndCustomsChargeFactory>(messageType, groupHeader.IncoTermAndChargeFactory);
							break;
						case BRJobMessageTypeList.Codes.Export:
							AssertType<ExportIncoTermAndCustomsChargeFactory>(messageType, groupHeader.IncoTermAndChargeFactory);
							break;
						default:
							AssertType<IncoTermAndCustomsChargeFactory>(messageType, groupHeader.IncoTermAndChargeFactory);
							break;
					}
				}
			});
		}

		public void TestImportLicenseDistributeBy()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
			var invoiceGroupHeader = declaration.AllGroupHeaders[0];
			invoiceGroupHeader.JZ_RX_NKInvoice_Currency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency;

			var charge = invoiceGroupHeader.Charges.AddNew(ImportCustomsChargeTypeList.Codes.FreightInNationalTerritory, 100m, invoiceGroupHeader.Invoice_Currency.RX_Code);
			AssertEquals("Distribute by should be NWT", ChargeDistributeByList.Codes.NetWeight, charge.J7_DistributeBy);

			charge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightCollect;
			AssertEquals("Distribute by should be NWT", ChargeDistributeByList.Codes.NetWeight, charge.J7_DistributeBy);

			charge.J7_ChargeType = ImportCustomsChargeTypeList.Codes.OverseasFreightPrepaid;
			AssertEquals("Distribute by should be NWT", ChargeDistributeByList.Codes.NetWeight, charge.J7_DistributeBy);

			charge.J7_ChargeType = Common.CustomsChargeTypeList.Codes.OverseasInsurance;
			AssertEquals("Distribute by should be FOB", ChargeDistributeByList.Codes.FOB, charge.J7_DistributeBy);
		}

		public void TestTypeDecider()
		{
			Assert("Update BaseJobComInvoiceGroupHeaderTypeDecider to include a decider for this class", Factory.New(typeof(BaseJobComInvoiceGroupHeader)).GetType() == GetExpectedBusinessObjectType());
		}

		public void TestICurrencyConverterDataProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = BRJobMessageTypeList.Codes.Import;
			var invoiceGroupHeader = declaration.AllGroupHeaders[0];

			var invGroupHeaderAsProvider = invoiceGroupHeader as ICurrencyConverterDataProvider;

			CombineAssertions(() =>
			{
				AssertEquals("JE_MessageType = IMP, Rate Type should be", ExchangeRateType.Customs, invGroupHeaderAsProvider.RateType);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportLicense;
				AssertEquals("JE_MessageType = LIC, Rate Type should be", ExchangeRateType.Customs, invGroupHeaderAsProvider.RateType);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.ImportSiscomex;
				AssertEquals("JE_MessageType = ISW, Rate Type should be", ExchangeRateType.Customs, invGroupHeaderAsProvider.RateType);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.LPCO;
				AssertEquals("JE_MessageType = LPC, Rate Type should be", ExchangeRateType.Customs, invGroupHeaderAsProvider.RateType);

				declaration.JE_MessageType = BRJobMessageTypeList.Codes.Export;
				AssertEquals("JE_MessageType = EXP, Rate Type should be", ExchangeRateType.CustomsSecondary, invGroupHeaderAsProvider.RateType);
			});
		}

		protected override Type ExpectedTypeOfCharges => typeof(Common.JobComInvChargeCollection<GroupInvoiceCharge>);

		protected override BaseJobDeclaration GetNewDeclarationForTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
			return declaration;
		}
	}
}
