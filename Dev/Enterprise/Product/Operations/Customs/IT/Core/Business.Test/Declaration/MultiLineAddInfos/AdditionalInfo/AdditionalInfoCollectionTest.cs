using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(AdditionalInfoCollection))]
sealed class AdditionalInfoCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.AdditionalInfoCollectionTest
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		var invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
		return invoiceLine.AdditionalInfos;
	}

	protected override Type GetExpectedCollectionType() => typeof(AdditionalInfoCollection);
}
