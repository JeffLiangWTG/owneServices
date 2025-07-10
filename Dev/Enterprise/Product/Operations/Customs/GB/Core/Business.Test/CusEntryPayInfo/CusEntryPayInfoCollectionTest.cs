using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using LineMerger = Enterprise.Customs.GB.Business.Declaration.LineMerger;

namespace Enterprise.Customs.GB.Business.Testing
{
	[TestedType(typeof(CusEntryPayInfoCollection))]
	class CusEntryPayInfoCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new CusEntryPayInfoCollection(Factory);

		public void TestModuleCusEntryPayInfoCollection()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var instruction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instruction.CEI_Style = "H1";
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = instruction.PK;
			Factory.Save();
			new LineMerger(declaration).DoMerge();
			var header = declaration.ActiveEntryHeaders[0];
			Factory.Save();
			TestCaseHelper.ClearTable(AutoCusEntryPayInfo.Schema.TableName);
			var payInfo1 = Factory.New<CusEntryPayInfo>();
			payInfo1.C9_TransactionType = "VAT";
			payInfo1.C9_PaymentAmount = new ZDecimal(10.1);
			payInfo1.C9_PaymentDate = ZDateTime.Today;
			payInfo1.C9_CH = header.PK;
			var payInfo2 = Factory.New<CusEntryPayInfo>();
			payInfo2.C9_TransactionType = "AAA";
			payInfo2.C9_PaymentDate = ZDateTime.Today;
			payInfo2.C9_PaymentAmount = new ZDecimal(10.2);
			payInfo2.C9_CH = header.PK;
			var payInfo3 = Factory.New<CusEntryPayInfo>();
			payInfo3.C9_TransactionType = "VAT";
			payInfo3.C9_PaymentAmount = new ZDecimal(10.3);
			payInfo3.C9_PaymentDate = ZDateTime.Today;
			payInfo3.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Clear;
			payInfo3.C9_CH = header.PK;
			var payInfo4 = Factory.New<CusEntryPayInfo>();
			payInfo4.C9_TransactionType = "AAA";
			payInfo4.C9_PaymentAmount = new ZDecimal(10.4);
			payInfo4.C9_PaymentDate = ZDateTime.Today;
			payInfo4.C9_PaymentStatus = CusEntryPayInfoStatusList.Codes.Pending;
			payInfo4.C9_CH = header.PK;
			Factory.Save();
			var collection = new CusEntryPayInfoCollection(Factory);
			collection.Load();
			AssertEquals(4, collection.Count);
			AssertContainsExactElementsInAnyOrder(new List<CusEntryPayInfo> { payInfo1, payInfo2, payInfo3, payInfo4 }, collection);
			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				collection = new CusEntryPayInfoCollection(Factory);
				collection.Load();
				AssertEquals(0, collection.Count);
			}
		}
	}
}
