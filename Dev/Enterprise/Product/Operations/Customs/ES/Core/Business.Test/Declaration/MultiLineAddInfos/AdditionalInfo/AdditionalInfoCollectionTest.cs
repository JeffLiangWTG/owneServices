using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	[TestedType(typeof(AdditionalInfoCollection))]
	public class AdditionalInfoCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.AdditionalInfoCollectionTest
	{
		public void TestAddNewAndGetByIndex()
		{
			var dec = Factory.New<JobDeclaration>();
			var header = dec.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			var headerAdd = header.AdditionalInfos.AddNew();
			var lineAdd = line.AdditionalInfos.AddNew();
			AssertEquals(typeof(AdditionalInfo), headerAdd.GetType());
			AssertEquals(typeof(AdditionalInfo), lineAdd.GetType());
			AssertEquals(typeof(AdditionalInfo), header.AdditionalInfos[0].GetType());
			AssertEquals(typeof(AdditionalInfo), line.AdditionalInfos[0].GetType());
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var dec = Factory.New<JobDeclaration>();
			var line = dec.Invoices.AddNew().InvoiceLines.AddNew();
			return line.AdditionalInfos;
		}

		protected override Type GetExpectedCollectionType() => typeof(AdditionalInfoCollection);
	}
}
