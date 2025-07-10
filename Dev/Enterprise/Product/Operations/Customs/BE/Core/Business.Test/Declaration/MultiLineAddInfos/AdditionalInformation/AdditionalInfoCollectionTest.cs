using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.BE.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.BE.Business.Testing;

[TestedType(typeof(AdditionalInfoCollection))]
class AdditionalInfoCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.AdditionalInfoCollectionTest
{
	public void TestAddNewAndGetByIndex()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceHeader = declaration.Invoices.AddNew();
		var additionalInfo = invoiceHeader.AdditionalInfos.AddNew();
		CombineAssertions(() =>
		{
			AssertEquals("AddNew", typeof(AdditionalInfo), additionalInfo.GetType());
			AssertEquals("GetByIndex", typeof(AdditionalInfo), invoiceHeader.AdditionalInfos[0].GetType());
		});
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		var header = declaration.Invoices.AddNew();
		return header.AdditionalInfos;
	}

	protected override Type GetExpectedCollectionType() => typeof(AdditionalInfoCollection);
}
