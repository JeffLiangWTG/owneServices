using System.Collections.Generic;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(VINData))]
	class VINDataTest : Customs.Business.MultiLineAddInfos.Testing.CusAddInfoTest<VINData>
	{
		public void TestSupportsNotes()
		{
			AssertEquals("SupportsNotes should be false", false, ((VINData)BusinessObject).SupportsNotes);
		}

		public void TestNFields()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			testItems.JobDeclaration.JE_MessageType = "IMP";
			var vinData = testItems.InvoiceLine.VINDataCollection.AddNew();
			vinData.XC_QGP = "不低于责任规定";
			Factory.Save();
			AssertEquals("QGP=不低于责任规定", vinData.B7_NAddInfoData);
		}

		public void TestXC_ProductNameCNDefaulting()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			testItems.JobDeclaration.JE_MessageType = "IMP";
			testItems.InvoiceLine.JI_NameOfGoods2 = "品名2";
			var vinData1 = testItems.InvoiceLine.VINDataCollection.AddNew();
			AssertEquals("Product Name CN should be defaulted to NameOfGoods2", "品名2", vinData1.XC_ProductNameCN);
			testItems.InvoiceLine.JI_NameOfGoods = "品名1";
			var vinData2 = testItems.InvoiceLine.VINDataCollection.AddNew();
			AssertEquals("Product Name CN should be defaulted to NameOfGoods", "品名1", vinData2.XC_ProductNameCN);
			var vinData3 = testItems.InvoiceLine.VINDataCollection.AddNew();
			vinData3.XC_ProductNameCN = "Override";
			AssertEquals("Product Name CN should keep its overriden value", "Override", vinData3.XC_ProductNameCN);
		}

		public void TestXC_ProductNameENDefaulting()
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(Factory, () => { });
			testItems.JobDeclaration.JE_MessageType = "IMP";
			testItems.InvoiceLine.JI_Description = "Goods";
			var vinData1 = testItems.InvoiceLine.VINDataCollection.AddNew();
			AssertEquals("Product Name EN should be defaulted to the invoice line description", testItems.InvoiceLine.JI_Description, vinData1.XC_ProductNameEN);
			var vinData2 = testItems.InvoiceLine.VINDataCollection.AddNew();
			vinData2.XC_ProductNameEN = "Override";
			AssertEquals("Product Name EN should keep its overriden value", "Override", vinData2.XC_ProductNameEN);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(factory, () => { });
			testItems.JobDeclaration.JE_MessageType = "IMP";
			return testItems.InvoiceLine.VINDataCollection.AddNew();
		}

		protected override IEnumerable<VINData> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var testItems = CNCusEntryHeaderHelper.SetupCusEntryHeader(factory, () => { });
			testItems.JobDeclaration.JE_MessageType = "IMP";
			yield return testItems.InvoiceLine.VINDataCollection.AddNew();
		}
	}
}
