using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(AddInfoCusEntryLine))]
sealed class AddInfoCusEntryLineTest : NonPersistentBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		var entryLine = Factory.New<CusEntryLine>();
		return new AddInfoCusEntryLine(entryLine.CL_AddInfoInfo);
	}
}
