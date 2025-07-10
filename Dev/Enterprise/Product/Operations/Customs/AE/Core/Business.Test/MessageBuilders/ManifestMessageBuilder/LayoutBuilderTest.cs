using NUnit.Framework;

namespace Enterprise.Customs.AE.Business.Testing;

public class LayoutBuilderTest : TestCase
{
	public void TestBuildGeneralLayout()
	{
		RootLine root = new RootLine();
		VOYLine voy = new VOYLine();
		ENDLine end = new ENDLine();
		root.Children.Add(voy);
		root.Children.Add(end);
		BOLLine bol = new BOLLine();
		CTRLine ctr = new CTRLine();
		ctr.Children.Add(new CONLine());
		ctr.Children.Add(new CONLine());
		bol.Children.Add(ctr);
		voy.Children.Add(bol);
		AssertEquals(2, root.Children.Count);
		AssertEquals(typeof(VOYLine), root.Children[0].GetType());
		AssertEquals(1, root.Children[0].Children.Count);
		AssertEquals(typeof(BOLLine), root.Children[0].Children[0].GetType());
		AssertEquals(1, root.Children[0].Children[0].Children.Count);
		AssertEquals(typeof(CTRLine), root.Children[0].Children[0].Children[0].GetType());
		new LayoutBuilder().BuildGeneralLayout(root);
		AssertEquals(2, root.Children.Count);
		AssertEquals(typeof(VOYLine), root.Children[0].GetType());
		AssertEquals(1, root.Children[0].Children.Count);
		AssertEquals(typeof(BOLLine), root.Children[0].Children[0].GetType());
		AssertEquals(2, root.Children[0].Children[0].Children.Count);
		AssertEquals(typeof(CONLine), root.Children[0].Children[0].Children[0].GetType());
		AssertEquals(typeof(CONLine), root.Children[0].Children[0].Children[1].GetType());
	}
}
