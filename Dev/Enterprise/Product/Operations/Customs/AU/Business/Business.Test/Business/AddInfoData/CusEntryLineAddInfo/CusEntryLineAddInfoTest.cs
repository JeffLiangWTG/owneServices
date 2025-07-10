using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(CusEntryLineAddInfo))]
	sealed class CusEntryLineAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var dec = Factory.New<JobDeclaration>();
			var header = dec.CustomsEntryHeaders.AddNew();
			return header.MergedLines.AddNew().EntryLineAddInfo;
		}
	}
}
