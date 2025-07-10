using System;
using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.Common.Testing;

[TestedType(typeof(JobDeclarationEntryStatusFilterHelper))]
public class JobDeclarationEntryStatusFilterHelperTest : TestCase
{
	[ExpectNoExceptions]
	public void TestGetEntryStatusFilter()
	{
		var allCalled = false;
		var anyCalled = false;

		var helper = new JobDeclarationEntryStatusFilterHelper(
			getEntryStatusQueryForAny: delegate
			{
				anyCalled = true;
				return new ZQuery();
			},
			getEntryStatusQueryForAll: delegate
			{ allCalled = true;
				return new ZQuery();
			} );

		helper.GetEntryStatusFilter(SQLComparisonOperator.Equal, EntryStatusFilterTypeList.Codes.All, "TST");
		NUnit.Framework.Assert.That(allCalled, Is.EqualTo(true));
		NUnit.Framework.Assert.That(anyCalled, Is.EqualTo(false));

		allCalled = false;
		helper.GetEntryStatusFilter(SQLComparisonOperator.Equal, EntryStatusFilterTypeList.Codes.Any, "TST");
		NUnit.Framework.Assert.That(allCalled, Is.EqualTo(false));
		NUnit.Framework.Assert.That(anyCalled, Is.EqualTo(true));
	}

	public void TestJobDeclarationEntryStatusFilterHelper_ThrowsArgumentException()
	{
		AssertExceptionThrown<ArgumentException>(() => new JobDeclarationEntryStatusFilterHelper(delegate { return new ZQuery(); }, null));

		AssertExceptionThrown<ArgumentException>(() => new JobDeclarationEntryStatusFilterHelper(null, delegate { return new ZQuery(); }));
	}
}
