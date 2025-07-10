using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(AddInfoCusEntryLine))]
	public class AddInfoCusEntryLineTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			CusEntryLine entryLine = Factory.New<CusEntryLine>();
			return new AddInfoCusEntryLine(entryLine.CL_AddInfoInfo);
		}
	}
}
