using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class HeaderOrLineValueMapResolverTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		CombineAssertions(() =>
		{
			AssertExceptionThrown<ArgumentNullException>(() => GetNewMapResolver(
				null,
				x => x.Header,
				x => x.PropertyH,
				x => x.PropertyL));

			AssertExceptionThrown<ArgumentNullException>(() => GetNewMapResolver(
				x => x.Lines,
				null,
				x => x.PropertyH,
				x => x.PropertyL));

			AssertExceptionThrown<ArgumentNullException>(() => GetNewMapResolver(
				x => x.Lines,
				x => x.Header,
				null,
				x => x.PropertyL));

			AssertExceptionThrown<ArgumentNullException>(() => GetNewMapResolver(
				x => x.Lines,
				x => x.Header,
				x => x.PropertyH,
				null));
		});
	}

	public void TestGuardClauses()
	{
		var mapResolver = GetDefaultMapResolver();

		AssertExceptionThrown<ArgumentNullException>(nameof(mapResolver.GetValueForHeader),
			() => mapResolver.GetValueForHeader(null));

		AssertExceptionThrown<ArgumentNullException>(nameof(mapResolver.GetValueForLine),
			() => mapResolver.GetValueForLine(null));
	}

	public void TestGetValueForHeader()
	{
		var mapResolver = GetDefaultMapResolver();

		header.PropertyH = "H";
		line1.PropertyL = "L1";
		line2.PropertyL = "L2";

		AssertEquals("When Lines has different value, GetValueHeader",
			"",
			mapResolver.GetValueForHeader(header));

		line1.PropertyL = "L";
		line2.PropertyL = "L";
		AssertEquals("When Lines has same value, GetValueHeader",
			"L",
			mapResolver.GetValueForHeader(header));

		header = new DummyHeader();
		header.PropertyH = "H";
		AssertEquals("When there is no line, GetValueHeader",
			"H",
			mapResolver.GetValueForHeader(header));
	}

	public void TestGetValueForLine()
	{
		var mapResolver = GetDefaultMapResolver();

		header.PropertyH = "H";
		line1.PropertyL = "L1";
		line2.PropertyL = "L2";

		AssertEquals("When Lines has different value, GetValueHeader",
			"L1",
			mapResolver.GetValueForLine(line1));

		line1.PropertyL = "L";
		line2.PropertyL = "L";
		AssertEquals("When Lines has same value, GetValueHeader",
			"",
			mapResolver.GetValueForLine(line1));
	}

	public void TestGetValueForLine_WithFallback()
	{
		var mapResolver = GetDefaultMapResolver();

		header.PropertyH = "H";
		line1.PropertyL = "";
		line2.PropertyL = "L";

		mapResolver.UseFallBack = true;
		line1.PropertyL = "";
		AssertEquals("When a Line Is Empty, GetValueHeader",
			"H",
			mapResolver.GetValueForLine(line1));
	}

	public void TestGetValueForLine_WithIsEmptyFuncNull()
	{
		var mapResolver = GetDefaultMapResolver();

		mapResolver.IsEmptyFunc = null;
		header.PropertyH = "H";
		line1.PropertyL = "";
		line2.PropertyL = "L";

		mapResolver.UseFallBack = true;
		line1.PropertyL = "";
		AssertEquals("When a Line Is Empty but EmptyFunc is null, GetValueHeader",
			"",
			mapResolver.GetValueForLine(line1));
	}

	public void TestEqualityComparer()
	{
		var mapResolver = GetDefaultMapResolver();
		var equalityComparerMock = new Mock<IEqualityComparer<ZString>>();

		equalityComparerMock
			.Setup(x => x.Equals(It.IsAny<ZString>(), It.IsAny<ZString>()))
			.Returns<ZString, ZString>((x, y) => ReverseString(x) == y);

		mapResolver.EqualityComparer = equalityComparerMock.Object;

		header.PropertyH = "H";
		line1.PropertyL = "AB";
		line2.PropertyL = "BA";

		AssertEquals("When EqualityComparer is set, GetValueHeader",
			"AB",
			mapResolver.GetValueForHeader(header));

		ZString ReverseString(ZString value)
		{
			var charArray = value.ToString().ToCharArray();
			Array.Reverse(charArray);
			return new ZString(new string(charArray));
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		header = new DummyHeader();
		line1 = new DummyLine();
		line2 = new DummyLine();
		header.AddLines(line1, line2);
	}

	DummyHeader header;
	DummyLine line1;
	DummyLine line2;

	IHeaderOrLineValueMapResolver<DummyHeader, DummyLine, ZString> GetDefaultMapResolver()
	{
		var mapResolver = GetNewMapResolver(
			x => x.Lines,
			x => x.Header,
			x => x.PropertyH,
			x => x.PropertyL);

		mapResolver.IsEmptyFunc = x => x.IsEmpty;
		return mapResolver;
	}

	IHeaderOrLineValueMapResolver<DummyHeader, DummyLine, ZString> GetNewMapResolver(
		Func<DummyHeader, IEnumerable<DummyLine>> linesGetter,
		Func<DummyLine, DummyHeader> headerGetter,
		Func<DummyHeader, ZString> headerValueGetter,
		Func<DummyLine, ZString> lineValueGetter)
	{
		return new HeaderOrLineValueMapResolver<DummyHeader, DummyLine, ZString>(
			linesGetter,
			headerGetter,
			headerValueGetter,
			lineValueGetter);
	}
}

sealed class DummyHeader
{
	public ZString PropertyH { get; set; }
	public List<DummyLine> Lines { get; } = new List<DummyLine>();

	public void AddLines(params DummyLine[] lines)
	{
		foreach (var line in lines)
		{
			line.Header = this;
			Lines.Add(line);
		}
	}
}

sealed class DummyLine
{
	public ZString PropertyL { get; set; }
	public DummyHeader Header { get; set; }
}
