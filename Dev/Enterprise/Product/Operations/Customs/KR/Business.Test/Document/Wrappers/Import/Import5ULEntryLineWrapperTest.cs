using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(Import5ULEntryLineWrapper))]
	sealed class Import5ULEntryLineWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var line = new Import5ULEntryLine();

			return new Import5ULEntryLineWrapper(line, ZBool.False);
		}
	}
}
