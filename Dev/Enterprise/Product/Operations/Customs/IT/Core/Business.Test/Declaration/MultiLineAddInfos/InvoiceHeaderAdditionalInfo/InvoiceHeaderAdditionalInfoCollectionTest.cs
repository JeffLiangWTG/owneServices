using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(InvoiceHeaderAdditionalInfoCollection))]
sealed class InvoiceHeaderAdditionalInfoCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.AdditionalInfoCollectionTest
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var declaration = Factory.New<JobDeclaration>();
		return declaration.Invoices.AddNew().AdditionalInfos;
	}

	protected override Type GetExpectedCollectionType() => typeof(InvoiceHeaderAdditionalInfoCollection);
}
