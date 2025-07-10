using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.BR;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.Testing
{
	public class JobComInvoiceLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckJI_CEI()
		{
			var instruction1 = declaration.CustomsEntryInstructions.AddNew();
			instruction1.CEI_Description = "ENT1";
			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertHasMessageErrorContaining(invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_CEI = instruction1.PK;
			AssertNoMessageErrorContaining(invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.MakeNonPersistent();
			invoiceLine.JI_CEI = ZGuid.Empty;
			AssertNoMessageErrorContaining(invoiceLine.JI_CEIInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public virtual void TestCheckJI_Procedure()
		{
			invoiceLine.JI_Procedure = ZString.Empty;
			AssertNoNotifications(invoiceLine.JI_ProcedureInfo);
			invoiceLine.JI_Procedure = "00000";
			AssertNoNotifications(invoiceLine.JI_ProcedureInfo);
		}

		public void TestCheckJI_NetWeightUQ()
		{
			if (declaration.IsImport)
			{
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_NetWeightUQInfo, "XX", "KG");
			}
			else
			{
				invoiceLine.JI_NetWeightUQ = ZString.Empty;
				AssertNoNotifications(invoiceLine.JI_NetWeightUQInfo);
			}
		}

		public void TestCheckJI_CustomsQuantityForInvoiceQuantity()
		{
			var packConvertion = Factory.New<CusRefPacks>();
			packConvertion.RP_CustomsCountry = "BR";
			packConvertion.RP_Type = "CIP";
			packConvertion.RP_ConversionFactor = 2.222222;
			packConvertion.RP_CustomsPack = "UN";
			packConvertion.RP_CommercialPack = "KG";
			Factory.Save();

			invoiceLine.JI_InvoiceQuantity = 1m;
			invoiceLine.JI_InvoiceUQ = "KG";
			invoiceLine.JI_CustomsUnitQty = "UN";
			invoiceLine.JI_CustomsQuantity = 0m;

			AssertEquals("Precondition: Decimal Places of JI_CustomsQuantity is 6", 6, invoiceLine.GetDecimalPlacesMetaData(JobComInvoiceLine.Schema.JI_CustomsQuantity));

			string GetMessage() => $"Customs Qty: {invoiceLine.JI_CustomsQuantity} {invoiceLine.JI_CustomsUnitQty}, Invoice Qty: {invoiceLine.JI_InvoiceQuantity} {invoiceLine.JI_InvoiceUQ}";

			if (declaration.IsImport)
			{
				AssertNoMessageError(GetMessage(), invoiceLine.JI_CustomsQuantityInfo, "Customs Qty is different from the value calculated by the system");

				invoiceLine.JI_CustomsQuantity = 2m;
				AssertHasMessageError(GetMessage(), invoiceLine.JI_CustomsQuantityInfo, "Customs Qty is different from the value calculated by the system");

				invoiceLine.JI_CustomsQuantity = 2.22222m;
				AssertNoMessageError(GetMessage(), invoiceLine.JI_CustomsQuantityInfo, "Customs Qty is different from the value calculated by the system");

				invoiceLine.JI_CustomsUnitQty = "G";
				invoiceLine.JI_CustomsQuantity = 1m;
				AssertHasMessageError(GetMessage(), invoiceLine.JI_CustomsQuantityInfo, "Customs Qty is different from the value calculated by the system");

				invoiceLine.JI_CustomsQuantity = 1000m;
				AssertNoMessageError(GetMessage(), invoiceLine.JI_CustomsQuantityInfo, "Customs Qty is different from the value calculated by the system");

				invoiceLine.JI_InvoiceUQ = "BAG";
				AssertNoMessageError(GetMessage(), invoiceLine.JI_CustomsQuantityInfo, "Customs Qty is different from the value calculated by the system");
			}
			else
			{
				invoiceLine.JI_CustomsQuantity = 2m;
				AssertNoNotifications(GetMessage(), invoiceLine.JI_CustomsQuantityInfo);
			}
		}

		public void TestCheckJI_CustomsQuantityForNetWeight()
		{
			var packConvertion = Factory.New<CusRefPacks>();
			packConvertion.RP_CustomsCountry = "BR";
			packConvertion.RP_Type = "CIP";
			packConvertion.RP_ConversionFactor = 2.222222;
			packConvertion.RP_CustomsPack = "UN";
			packConvertion.RP_CommercialPack = "KG";
			Factory.Save();

			invoiceLine.JI_NetWeight = 1m;
			invoiceLine.JI_NetWeightUQ = "KG";
			invoiceLine.JI_CustomsUnitQty = "UN";
			invoiceLine.JI_CustomsQuantity = 0m;

			AssertEquals("Precondition: Decimal Places of JI_CustomsQuantity is 6", 6, invoiceLine.GetDecimalPlacesMetaData(JobComInvoiceLine.Schema.JI_CustomsQuantity));

			string GetMessage() => $"Customs Qty: {invoiceLine.JI_CustomsQuantity} {invoiceLine.JI_CustomsUnitQty}, Net Weight: {invoiceLine.JI_NetWeight} {invoiceLine.JI_NetWeightUQ}";

			if (declaration.IsImport)
			{
				AssertNoMessageError(GetMessage(), invoiceLine.JI_CustomsQuantityInfo, "Customs Qty is different from the value calculated by the system");

				invoiceLine.JI_CustomsQuantity = 2m;
				AssertHasMessageError(GetMessage(), invoiceLine.JI_CustomsQuantityInfo, "Customs Qty is different from the value calculated by the system");

				invoiceLine.JI_CustomsQuantity = 2.22222m;
				AssertNoMessageError(GetMessage(), invoiceLine.JI_CustomsQuantityInfo, "Customs Qty is different from the value calculated by the system");

				invoiceLine.JI_CustomsUnitQty = "G";
				invoiceLine.JI_CustomsQuantity = 1m;
				AssertHasMessageError(GetMessage(), invoiceLine.JI_CustomsQuantityInfo, "Customs Qty is different from the value calculated by the system");

				invoiceLine.JI_CustomsQuantity = 1000m;
				AssertNoMessageError(GetMessage(), invoiceLine.JI_CustomsQuantityInfo, "Customs Qty is different from the value calculated by the system");

				invoiceLine.JI_CustomsUnitQty = "M3";
				AssertNoMessageError(GetMessage(), invoiceLine.JI_CustomsQuantityInfo, "Customs Qty is different from the value calculated by the system");
			}
			else
			{
				invoiceLine.JI_CustomsQuantity = 2m;
				AssertNoNotifications(GetMessage(), invoiceLine.JI_CustomsQuantityInfo);
			}
		}

		public void TestCheckJI_InvoiceUQ()
		{
			if (declaration.IsImport)
			{
				ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(invoiceLine.JI_InvoiceUQInfo, "XXX", "BAG");
			}
			else
			{
				invoiceLine.JI_InvoiceUQ = ZString.Empty;
				AssertNoNotifications(invoiceLine.JI_InvoiceUQInfo);

				invoiceLine.JI_InvoiceUQ = "KG";
				AssertNoMessageErrors(invoiceLine.JI_InvoiceUQInfo);
			}
		}

		public void TestCheckJI_InvoiceUQ_PortugueseDescription()
		{
			var refPackType = Factory.New<RefPackType>();
			refPackType.F3_Code = "XX";
			refPackType.F3_Description = "Test";

			Factory.Save();

			var refLanguageText = Factory.New<RefLanguageText>();
			refLanguageText.RLT_ParentId = refPackType.PK;
			refLanguageText.RLT_Text = "Test";
			refLanguageText.RLT_ColumnName = JobComInvoiceLine.Schema.JI_InvoiceUQ;
			refLanguageText.RLT_ParentTableCode = "RL";
			refLanguageText.RLT_Language = "PT-BR";
			refLanguageText.RLT_IsSystem = true;

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageType;

			var invoiceLine = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();

			invoiceLine.JI_InvoiceUQ = ZString.Empty;
			if (dec.JE_MessageType == BRJobMessageTypeList.Codes.ImportLicense || dec.JE_MessageType == BRJobMessageTypeList.Codes.ImportSiscomex)
			{
				invoiceLine.JI_InvoiceUQ = "XX";
				var multilingualDescription = RefPackTypeCollection.GetAsCodeDescriptionPairWithStandardUnits(Factory).GetMultilingualDescriptionFromCode(invoiceLine.JI_InvoiceUQ);
				var ptBRDescription = multilingualDescription?.ToString("PT-BR");
				AssertEquals("Test", ptBRDescription);
				AssertHasMessageError(invoiceLine.JI_InvoiceUQInfo, "Invoice Qty- UQ Portuguese description is the same as English Description");
			}
			else
			{
				AssertNoMessageErrors(invoiceLine.JI_InvoiceUQInfo);
			}
		}

		public void TestCheckFullGoodsDescription()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageType;

			var invoiceLine = dec.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.FullGoodsDescription = ZString.Empty;
			if (dec.JE_MessageType == BRJobMessageTypeList.Codes.ImportLicense || dec.JE_MessageType == BRJobMessageTypeList.Codes.ImportSiscomex)
			{
				ValidationTestHelper.AssertYouHaveNotEnteredMessageError(invoiceLine.FullGoodsDescriptionInfo, "You have not entered a Goods Description.");
			}
			else
			{
				AssertNoMessageErrors(invoiceLine.FullGoodsDescriptionInfo);

				invoiceLine.FullGoodsDescription = "Test";
				invoiceLine.Validation.ValidateFullGoodsDescription();
				AssertNoMessageErrors(invoiceLine.FullGoodsDescriptionInfo);
			}
		}

		public void TestCheckNaladiNcca()
		{
			invoiceLine.NaladiNcca = "AAA";

			if (declaration.IsImportSiscomex)
			{
				AssertHasMessageError(invoiceLine.NaladiNccaInfo, "NALADI/NCCA must consist 8 numeric characters.");

				invoiceLine.NaladiNcca = "32081020";
				AssertNoMessageError(invoiceLine.NaladiNccaInfo, "NALADI/NCCA must consist 8 numeric characters.");

				invoiceLine.NaladiNcca = ZString.Empty;
				AssertNoMessageError(invoiceLine.NaladiNccaInfo, "NALADI/NCCA must consist 8 numeric characters.");
			}
			else
			{
				AssertNoNotifications(invoiceLine.NaladiNccaInfo);
			}
		}

		public void TestCheckNaladiHs()
		{
			invoiceLine.NaladiHs = "AAA";

			if (declaration.IsImportSiscomex || declaration.IsImportLicense)
			{
				AssertHasMessageError(invoiceLine.NaladiHsInfo, "NALADI/HS must consist 8 numeric characters.");

				invoiceLine.NaladiHs = "87083900";
				AssertNoMessageError(invoiceLine.NaladiHsInfo, "NALADI/HS must consist 8 numeric characters.");

				invoiceLine.NaladiHs = ZString.Empty;
				AssertNoMessageError(invoiceLine.NaladiHsInfo, "NALADI/HS must consist 8 numeric characters.");
			}
			else
			{
				AssertNoNotifications(invoiceLine.NaladiHsInfo);
			}
		}

		public void TestValidateAllWhenDeclarationIsNull()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageType;
			invoiceLine.JI_JZ = invoice.PK;
			AssertNull(invoiceLine.Declaration);
			AssertNoExceptionThrown(invoiceLine.Validation.ValidateAll);
		}

		protected virtual string JobMessageType => BRJobMessageTypeList.Codes.LPCO;

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageType;
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		}

		protected JobDeclaration declaration;
		protected JobComInvoiceLine invoiceLine;
	}
}
