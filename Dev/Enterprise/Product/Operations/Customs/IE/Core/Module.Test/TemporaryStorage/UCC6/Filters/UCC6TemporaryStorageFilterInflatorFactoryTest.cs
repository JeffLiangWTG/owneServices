using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Module.Testing;

[TestedType(typeof(UCC6TemporaryStorageFilterInflatorFactory))]
sealed class UCC6TemporaryStorageFilterInflatorFactoryTest : TestCaseWithFactory
{
	public void TestFactoryMethods()
	{
		var bizObj = FilterStripBizObj;
		CombineAssertions(() =>
		{
			FactoryMethodTestCases(FilterInflatorFactory)
				.ForEach(tc => AssertType(tc.ExpectedType, tc.Create(bizObj)));
		});
	}

	UCC6TemporaryStorageFilterStripBusinessObject FilterStripBizObj => new UCC6TemporaryStorageFilterStripBusinessObject();
	UCC6TemporaryStorageFilterInflatorFactory FilterInflatorFactory => new UCC6TemporaryStorageFilterInflatorFactory();

	List<(Type ExpectedType, Func<UCC6TemporaryStorageFilterStripBusinessObject, IFilterInflator> Create)> FactoryMethodTestCases(UCC6TemporaryStorageFilterInflatorFactory f) =>
	[
		// New factory methods
		(typeof(MessageVersionFilterInflator), f.CreateMessageVersionFilterInflator)
	];
}

