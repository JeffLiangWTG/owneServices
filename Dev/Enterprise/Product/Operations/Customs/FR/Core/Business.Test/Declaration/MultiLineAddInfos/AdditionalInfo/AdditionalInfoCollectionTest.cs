using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(AdditionalInfoCollection))]
	class AdditionalInfoCollectionTest : EU.Business.Declaration.MultiLineAddInfos.Testing.AdditionalInfoCollectionTest
	{
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
