#if DEBUG
using NUnit.Framework;

namespace CargoWise.ComponentModel.Testing
{
	public class KBindingMemberInfoTests : TestCase
	{
		public void TestEmpty()
		{
			KBindingMemberInfo info = new KBindingMemberInfo("");
			AssertEquals("", info.BindingMember);
			AssertEquals("", info.BindingPath);
			AssertEquals("", info.BindingField);

			info = new KBindingMemberInfo("", "");
			AssertEquals("", info.BindingMember);
			AssertEquals("", info.BindingPath);
			AssertEquals("", info.BindingField);

			info = new KBindingMemberInfo(" .", "");
			AssertEquals("", info.BindingMember);
			AssertEquals("", info.BindingPath);
			AssertEquals("", info.BindingField);
		}

		public void TestDotOnly()
		{
			KBindingMemberInfo info = new KBindingMemberInfo(" .");
			AssertEquals("", info.BindingMember);
			AssertEquals("", info.BindingPath);
			AssertEquals("", info.BindingField);

			info = new KBindingMemberInfo("Path", " .");
			AssertEquals("Path", info.BindingMember);
			AssertEquals("Path", info.BindingPath);
			AssertEquals("", info.BindingField);

			info = new KBindingMemberInfo(" .", "Path");
			AssertEquals("Path", info.BindingMember);
			AssertEquals("", info.BindingPath);
			AssertEquals("Path", info.BindingField);
		}

		public void TestFieldOnly()
		{
			KBindingMemberInfo info = new KBindingMemberInfo("", "field");
			AssertEquals("field", info.BindingMember);
			AssertEquals("", info.BindingPath);
			AssertEquals("field", info.BindingField);

			info = new KBindingMemberInfo(" .", "field");
			AssertEquals("field", info.BindingMember);
			AssertEquals("", info.BindingPath);
			AssertEquals("field", info.BindingField);
		}

		public void TestBoth()
		{
			KBindingMemberInfo info = new KBindingMemberInfo("path.path2", "field");
			AssertEquals("path.path2.field", info.BindingMember);
			AssertEquals("path.path2", info.BindingPath);
			AssertEquals("field", info.BindingField);
		}
	}
}
#endif
