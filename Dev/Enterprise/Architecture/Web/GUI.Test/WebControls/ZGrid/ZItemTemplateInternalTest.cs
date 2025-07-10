using System;
using System.Web.UI.WebControls;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.WebControls.Testing
{
	public abstract class ZItemTemplateInternalTest : TransactionedTestCase
	{
		public void TestTextTransform()
		{
			if (TestColumn != null)
			{
				TestColumn.TextTransform = TextTransformOptions.None;
				TestItemTemplate.InstantiateIn(new TableCell());
				var testControl = TestItemTemplate.Control as ITextTransformer;
				if (testControl != null)
				{
					AssertEquals(TextTransformOptions.None, testControl.TextTransform);
				}

				TestColumn.TextTransform = TextTransformOptions.UpperCase;
				TestItemTemplate.InstantiateIn(new TableCell());
				testControl = TestItemTemplate.Control as ITextTransformer;
				if (testControl != null)
				{
					AssertEquals(TextTransformOptions.UpperCase, testControl.TextTransform);
				}
			}
			Assert(true);
		}

		protected abstract Type ExpectedItemTemplateType { get; }
		protected abstract Type ExpectedColumnType { get; }

		protected ISelfBindingWebControl GetTemplateControl()
		{
			return TestItemTemplate.GetControl();
		}

		protected ZItemTemplate TestItemTemplate
		{
			get
			{
				if (fTestItemTemplate == null)
				{
					fTestItemTemplate = GetNewTemplate(TestColumn);
				}

				return fTestItemTemplate;
			}
		}
		protected ZItemTemplate fTestItemTemplate;

		protected ZTemplateColumn TestColumn
		{
			get
			{
				if (fTestColumn == null)
				{
					fTestColumn = GetNewColumn("TestHeader", "TestBindTo");
				}

				return fTestColumn;
			}
		}
		protected ZTemplateColumn fTestColumn;

		protected ZItemTemplate GetNewTemplate(ZTemplateColumn column)
		{
			ZItemTemplate result = (ZItemTemplate)Activator.CreateInstance(ExpectedItemTemplateType, new object[] { column });
			return result;
		}

		protected virtual ZTemplateColumn GetNewColumn(string header, string bindTo)
		{
			ZTemplateColumn result = (ZTemplateColumn)Activator.CreateInstance(ExpectedColumnType, new object[] { header, bindTo });
			return result;
		}
	}
}
