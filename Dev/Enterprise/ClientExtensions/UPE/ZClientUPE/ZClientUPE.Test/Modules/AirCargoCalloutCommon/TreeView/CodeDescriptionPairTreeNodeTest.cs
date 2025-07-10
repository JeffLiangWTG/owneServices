using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Module.Testing
{
	public class CodeDescriptionPairTreeNodeTest : TestCase
	{
		public void TestCodeDescPair()
		{
			AssertEquals("CodeDescPair should be populated from the constructor", "CDE", TestNode.CodeDescPair.Code);
		}

		public void TestText()
		{
			AssertEquals("Text should show the text passed into the constructor", "Text", TestNode.Text);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CodeDescriptionPair codeDescPair = new CodeDescriptionPair("CDE", "Description");
			TestNode = new CodeDescriptionPairTreeNode("Text", codeDescPair);
		}

		CodeDescriptionPairTreeNode TestNode;
	}
}
