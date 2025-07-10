using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	[TestedType(typeof(ConfirmedCusEntryLineFeeCollection))]
	class ConfirmedCusEntryLineFeeCollectionTest : Customs.Business.Testing.ConfirmedCusEntryLineFeeCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testDec = Factory.New<JobDeclaration>();
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var entryLine = entryHeader.MergedLines.AddNew();
			return new ConfirmedCusEntryLineFeeCollection(entryLine);
		}

		protected override Type GetTypeOfElement()
		{
			return typeof(CusEntryLineFee);
		}
	}
}
