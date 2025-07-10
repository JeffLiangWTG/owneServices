using Enterprise.ZArchitecture.Web.Business.Utilities;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class ZCssHelperTest : TestCase
	{
		public void TestJoin()
		{
			string style1 = " DetailsTable ";
			string style2 = " Alternating ";
			string style3 = " Timeline ";

			AssertEquals("", "", ZCssHelper.Join(""));
			AssertEquals("s", "", ZCssHelper.Join(" "));
			AssertEquals("Test", "Test", ZCssHelper.Join(" ", " Test ", "", " "));
			AssertEquals("x", "DetailsTable", ZCssHelper.Join(style1));
			AssertEquals("xs", "DetailsTable", ZCssHelper.Join(style1, " "));
			AssertEquals("sx", "DetailsTable", ZCssHelper.Join(" ", style1));
			AssertEquals("sxs", "DetailsTable", ZCssHelper.Join(" ", style1, " "));

			AssertEquals("xy", "DetailsTable Alternating", ZCssHelper.Join(style1, style2));
			AssertEquals("sxy", "DetailsTable Alternating", ZCssHelper.Join(" ", style1, style2));
			AssertEquals("xys", "DetailsTable Alternating", ZCssHelper.Join(style1, style2, " "));
			AssertEquals("sxys", "DetailsTable Alternating", ZCssHelper.Join(" ", style1, style2, " "));
			AssertEquals("sxsy", "DetailsTable Alternating", ZCssHelper.Join(" ", style1, " ", style2));
			AssertEquals("xsy", "DetailsTable Alternating", ZCssHelper.Join(style1, " ", style2));
			AssertEquals("xsys", "DetailsTable Alternating", ZCssHelper.Join(style1, " ", style2, " "));
			AssertEquals("sxsys", "DetailsTable Alternating", ZCssHelper.Join(" ", style1, " ", style2, " "));

			AssertEquals("xyz", "DetailsTable Alternating Timeline", ZCssHelper.Join(style1, style2, style3));
			AssertEquals("sxyz", "DetailsTable Alternating Timeline", ZCssHelper.Join(" ", style1, style2, style3));
			AssertEquals("sxsyz", "DetailsTable Alternating Timeline", ZCssHelper.Join(style1, " ", style2, style3));
			AssertEquals("sxysz", "DetailsTable Alternating Timeline", ZCssHelper.Join(" ", style1, style2, " ", style3));
			AssertEquals("sxsysz", "DetailsTable Alternating Timeline", ZCssHelper.Join(" ", style1, " ", style2, " ", style3));
			AssertEquals("sxsyszs", "DetailsTable Alternating Timeline", ZCssHelper.Join(" ", style1, " ", style2, " ", style3, " "));
			AssertEquals("sxysz", "DetailsTable Alternating Timeline", ZCssHelper.Join(" ", style1, style2, " ", style3));
			AssertEquals("sxyszs", "DetailsTable Alternating Timeline", ZCssHelper.Join(" ", style1, style2, " ", style3, " "));
			AssertEquals("sxssyz", "DetailsTable Alternating Timeline", ZCssHelper.Join(" ", style1, style2, " ", " ", style3));
			AssertEquals("sxssyssz", "DetailsTable Alternating Timeline", ZCssHelper.Join(" ", style1, " ", " ", style2, " ", " ", style3));
			AssertEquals("ssxssysszss", "DetailsTable Alternating Timeline", ZCssHelper.Join(" ", " ", style1, " ", " ", style2, " ", " ", style3, " ", " "));
		}
	}
}
