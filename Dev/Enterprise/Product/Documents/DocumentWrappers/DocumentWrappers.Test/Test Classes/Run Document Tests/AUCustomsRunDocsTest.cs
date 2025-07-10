using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LandedCosting.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.Customs
{
	sealed class AUCustomsRunDocsTest : CustomsRunDocsTest
	{
		ZString StoredCountry;

		protected override void SetUp()
		{
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);
			base.SetUp();
		}

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			base.TearDown();
		}

		public override BusinessObject GetBusinessObject
		{
			get
			{
				var declaration = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
				var invoiceGroupHeader = declaration.JobComInvoiceGroupHeaders[0];
				var invoiceHeader = invoiceGroupHeader.JobComInvoiceHeaders.AddNew();
				var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				var lCHeader = Factory.New<LandedCostHeader>();
				lCHeader.LT_ParentID = declaration.PK;
				lCHeader.LT_ParentTableCode = "JE";
				declaration.CustomsEntryHeaders.AddNew();
				declaration.JE_DeclarationReference = "B00148999";
				return declaration;
			}
		}

		[ExpectNoExceptions]
		public void TestEntryPrintLandscape()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Entry Print (Landscape)");
			FilterForMenuItem.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, "AU");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestLandedCosting()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "Landed Costing");
			FilterForMenuItem.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_FilterList, SQLComparisonOperator.Contains, "AU");
			RunDocument();
		}

		[ExpectNoExceptions]
		public void TestEFTPaymentAdvice()
		{
			FilterForMenuItem = new ZQuery(StmMenuItemSchema.SU_MenuName, "EFT Payment Advice");
			FilterForMenuItem.AddToFilter(JoinCondition.And, StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.Contains, "Custom");
			RunDocument();
		}
	}
}
