using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class HeaderOrLineValueMapResolverBuilderTest : TestCaseWithFactory
{
	public void TestGuardClauses()
	{
		var builder = new HeaderOrLineValueMapResolverBuilder<DummyHeader, DummyLine, ZString>();
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => builder.SetLineGetter(null));
			AssertExceptionThrown<ArgumentNullException>(() => builder.SetHeaderGetter(null));
			AssertExceptionThrown<ArgumentNullException>(() => builder.SetHeaderValueGetter(null));
			AssertExceptionThrown<ArgumentNullException>(() => builder.SetLineValueGetter(null));
		});
	}

	public void TestBuild_WhenIsNotFullyConfigured()
	{
		var builder = new HeaderOrLineValueMapResolverBuilder<DummyHeader, DummyLine, ZString>();
		var expectedExceptionMessage = "Builder configuration is incomplete. Please make sure to set values for LineValueGetter, HeaderGetter, HeaderValueGetter, and LineGetter before building.";

		CombineAssertions(() =>
		{
			builder.SetHeaderGetter(x => x.Header);
			builder.SetHeaderValueGetter(x => x.PropertyH);
			builder.SetLineValueGetter(x => x.PropertyL);
			AssertExceptionThrown<InvalidOperationException>("When LineGetter is not set", expectedExceptionMessage, () => builder.Build());

			builder.Clear();
			builder.SetLineGetter(x => x.Lines);
			builder.SetHeaderValueGetter(x => x.PropertyH);
			builder.SetLineValueGetter(x => x.PropertyL);
			AssertExceptionThrown<InvalidOperationException>("When HeaderGetter is not set", expectedExceptionMessage, () => builder.Build());

			builder.Clear();
			builder.SetLineGetter(x => x.Lines);
			builder.SetHeaderGetter(x => x.Header);
			builder.SetLineValueGetter(x => x.PropertyL);
			AssertExceptionThrown<InvalidOperationException>("When SetHeaderValueGetter is not set", expectedExceptionMessage, () => builder.Build());

			builder.Clear();
			builder.SetLineGetter(x => x.Lines);
			builder.SetHeaderGetter(x => x.Header);
			builder.SetHeaderValueGetter(x => x.PropertyH);
			AssertExceptionThrown<InvalidOperationException>("When SetLineValueGetter is not set", expectedExceptionMessage, () => builder.Build());
		});
	}

	public void TestBuildAndClear()
	{
		var isEmptyFunc = new Func<ZString, bool>(x => true);
		var equalityComparer = new Mock<IEqualityComparer<ZString>>().Object;

		var builder = HeaderOrLineValueMapResolverFluent.Configure<DummyHeader, DummyLine, ZString>()
			.SetLineGetter(x => x.Lines)
			.SetHeaderGetter(x => x.Header)
			.SetHeaderValueGetter(x => x.PropertyH)
			.SetLineValueGetter(x => x.PropertyL)
			.UseFallBack()
			.IsEmptyWhen(isEmptyFunc)
			.WithEqualityComparer(equalityComparer);

		var mapResolver = builder.Build();

		CombineAssertions(() =>
		{
			AssertEquals(nameof(mapResolver.UseFallBack), true, mapResolver.UseFallBack);
			AssertEquals(nameof(mapResolver.IsEmptyFunc), isEmptyFunc, mapResolver.IsEmptyFunc);
			AssertEquals(nameof(mapResolver.EqualityComparer), equalityComparer, mapResolver.EqualityComparer);

			AssertExceptionThrown<InvalidOperationException>(() => builder.Build());
		});
	}
}
