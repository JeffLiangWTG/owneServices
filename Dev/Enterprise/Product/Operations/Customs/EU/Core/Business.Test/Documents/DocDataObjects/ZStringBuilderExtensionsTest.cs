using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.EU.Business.Documents.DocDataObjects.Testing;

sealed class ZStringBuilderExtensionsTest : TestCaseWithFactory
{
	public void TestAppendIfBuilderIsNotEmpty()
	{
		var sb = new ZStringBuilder();

		sb.AppendIfBuilderIsNotEmpty(ValueShouldNotBeAppended);

		CombineAssertions("AppendIfBuilderIsNotEmpty test cases", () =>
		{
			NUnit.Framework.Assert.That(sb.ToString(), NUnit.Framework.Is.EqualTo(string.Empty), "Should not append when build is empty.");

			sb.Append("Original text");
			sb.AppendIfBuilderIsNotEmpty(ValueShouldBeAppended);
			NUnit.Framework.Assert.That(sb.ToString(), NUnit.Framework.Does.Contain(ValueShouldBeAppended), "Should append when build is not empty.");

			AssertExceptionThrown<ArgumentNullException>("Exception expected when StringBuilder is null", () => (null as ZStringBuilder).AppendIfBuilderIsNotEmpty(ValueShouldBeAppended));
		});
	}

	public void TestAppendIfBuilderIsEmpty()
	{
		var sb = new ZStringBuilder();

		sb.AppendIfBuilderIsEmpty(ValueShouldBeAppended);
		NUnit.Framework.Assert.That(sb.ToString(), NUnit.Framework.Is.EqualTo(ValueShouldBeAppended), "Should append when build is empty.");

		sb.AppendIfBuilderIsEmpty(ValueShouldNotBeAppended);
		NUnit.Framework.Assert.That(sb.ToString(), NUnit.Framework.Does.Not.Contain(ValueShouldNotBeAppended), "Should not append when build is not empty.");

		AssertExceptionThrown<ArgumentNullException>("Exception expected when StringBuilder is null", () => (null as ZStringBuilder).AppendIfBuilderIsEmpty(ValueShouldBeAppended));
	}

	const string ValueShouldNotBeAppended = "this value should not be appended";
	const string ValueShouldBeAppended = "this value should be appended";
}
