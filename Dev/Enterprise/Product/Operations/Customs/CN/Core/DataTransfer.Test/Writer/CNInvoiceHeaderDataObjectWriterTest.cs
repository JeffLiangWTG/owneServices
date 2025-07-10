using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.CN.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Customs.CN.DataTransfer.Universal.Testing
{
	class CNInvoiceHeaderDataObjectWriterTest : UniversalDataBuss.Management.Testing.TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestPopulateInvoiceLineNotesToAddInfo()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_Tariff = "1";
			invoiceLine1.CIQIngredient = "IngredientNote";
			invoiceLine1.XC_GoodsSpecModel = "4|0|A";
			invoiceLine1.XC_GoodsSpecModel2 = "4|1|B";
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_Tariff = "2";

			var writer = new CNInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory.BOFactory));
			var addInfos1 = writer.GetDataObject(invoice).CommercialInvoiceLineCollection.First(x => x.HarmonisedCode.ToString() == "1").AddInfoCollection;
			AssertEquals("IngredientNote", addInfos1.First(addInfo => addInfo.Key.Equals("CIQIngredient")).Value);
			AssertEquals("4|0|A", addInfos1.First(addInfo => addInfo.Key.Equals("XC_GoodsSpecModel")).Value);
			AssertEquals("4|1|B", addInfos1.First(addInfo => addInfo.Key.Equals("XC_GoodsSpecModel2")).Value);

			var addInfos2 = writer.GetDataObject(invoice).CommercialInvoiceLineCollection.First(x => x.HarmonisedCode.ToString() == "2").AddInfoCollection;
			AssertEquals(ZString.Empty, addInfos2.First(addInfo => addInfo.Key.Equals("CIQIngredient")).Value);
			AssertEquals(ZString.Empty, addInfos2.First(addInfo => addInfo.Key.Equals("XC_GoodsSpecModel")).Value);
			AssertEquals(ZString.Empty, addInfos2.First(addInfo => addInfo.Key.Equals("XC_GoodsSpecModel2")).Value);
		}

		public void TestPopulateCIQPQs()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			var pq = invoiceLine1.CIQProductQualifications.AddNew();
			pq.CSI_ReferenceNumber = "PQDPQD";
			pq.CSI_Code = "408";

			Factory.SaveForTesting();

			var writer = new CNInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory.BOFactory));
			var testItem = writer.GetDataObject(invoice);

			var supportInfos = testItem.CommercialInvoiceLineCollection.First().CustomsSupportingInformationCollection;
			AssertEquals(1, supportInfos.Count);
			AssertEquals("PQDPQD", supportInfos.First().ReferenceNumber);
			AssertEquals("408", supportInfos.First().Type.Code);
		}

		public void TestCIQTariffDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory.BOFactory);
			var tariffTypePK = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.China, Business.Constants.UniversalReferenceConstants.CusTariffTypes.ChinaCIQTariff).PK;
			Factory.SaveForTesting();
			helper.CreateTariff(Core.Constants.CountryCodes.China, tariffTypePK, "2009891200101", ZDateTime.Today.AddYears(-1), ZDateTime.Today.AddYears(1), "未混合芒果汁");
			Factory.SaveForTesting();

			var declaration = Factory.New<JobDeclaration>();
			declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();

			invoiceLine1.JI_CIQTariff = "2009891200101";
			var writer = new CNInvoiceHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, Factory.New<DummyBusinessObject>())), new CNDataObjectWriterHelper(Factory.BOFactory));
			var testItem = writer.GetDataObject(invoice).CommercialInvoiceLineCollection.First();

			Assert(!testItem.AddInfoCollection.Any(addinfo => addinfo.Key.StringEquals("CIQTariffDescription")));
		}

		protected override void SetUp()
		{
			setupCreator = ((IExternalFetchHintSupporter)Factory.BOFactory).SetupCreator();
			base.SetUp();
		}
		IDisposable setupCreator;

		protected override void TearDown()
		{
			base.TearDown();
			if (setupCreator != null)
			{
				setupCreator.Dispose();
				setupCreator = null;
			}
		}
	}
}

