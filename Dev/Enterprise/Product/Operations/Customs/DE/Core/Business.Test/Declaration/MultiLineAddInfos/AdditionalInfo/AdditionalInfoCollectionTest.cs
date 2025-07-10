using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(AdditionalInfoCollection))]
	class AdditionalInfoCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.AdditionalInfoCollectionTest
	{
		public void TestAddNewAndGetByIndex()
		{
			var dec = Factory.New<JobDeclaration>();
			var header = dec.Invoices.AddNew();
			var line = dec.InvoiceLines.AddNew();
			var headerAdd = header.AdditionalInfos.AddNew();
			var lineAdd = line.AdditionalInfos.AddNew();
			AssertEquals(typeof(AdditionalInfo), headerAdd.GetType());
			AssertEquals(typeof(AdditionalInfo), lineAdd.GetType());
			AssertEquals(typeof(AdditionalInfo), header.AdditionalInfos[0].GetType());
			AssertEquals(typeof(AdditionalInfo), line.AdditionalInfos[0].GetType());
		}

		public void TestMaxCount_NotExitDetail()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertEquals("MaxCount not set", -1, invoiceHeader.AdditionalInfos.MaxCount);
		}

		public void TestSetDefaultsForNewChild_NotExitDetail()
		{
			var invoiceHeader = Factory.New<JobComInvoiceHeader>();
			AssertEquals("CSI_SubType not set", ZString.Empty, invoiceHeader.AdditionalInfos.AddNew().CSI_SubType);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var dec = Factory.New<JobDeclaration>();
			var header = dec.Invoices.AddNew();
			var line = dec.InvoiceLines.AddNew();
			return line.AdditionalInfos;
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(AdditionalInfoCollection);
		}
	}
}
