using System;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	sealed class ZTextEditColumnItemTemplateTest : ZItemTemplateTest
	{
		public void TestLabelHtmlEncoding()
		{
			ZTextEditColumn column = new ZTextEditColumn("HeaderText", "BindTo");
			ZTextEditColumnItemTemplate template = new ZTextEditColumnItemTemplate(column);

			column.EnableHtmlEncoding = true;
			Assert("Control Html Encoding", ((ZTextLabel)template.GetControl()).EnableHtmlEncoding);

			column.EnableHtmlEncoding = false;
			Assert("Control Html Encoding", !((ZTextLabel)template.GetControl()).EnableHtmlEncoding);
		}

		[ExpectNoExceptions]
		public void TestLabelHtmlEncoding_NoExceptionForSubclass()
		{
			ZNewRowColumn column = new ZNewRowColumn("BindTo");
			ZTextEditColumnItemTemplate template = new ZTextEditColumnItemTemplate(column);

			AssertNotNull("Control should be created without an exception", template.GetControl());
		}

		protected override Type ExpectedItemTemplateType
		{
			get { return typeof(ZTextEditColumnItemTemplate); }
		}

		protected override Type ExpectedColumnType
		{
			get { return typeof(ZTextEditColumn); }
		}
	}
}
