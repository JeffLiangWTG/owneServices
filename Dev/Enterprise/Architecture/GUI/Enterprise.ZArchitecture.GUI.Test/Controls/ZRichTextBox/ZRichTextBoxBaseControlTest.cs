using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.GUI.RichEdit.Testing
{
	class ZRichTextBoxBaseControlTest : ZControlBaseTestCase<ZRichTextBox>
	{
		protected override object GetDataSource(string propertyName)
		{
#if !WINZOR
			if (propertyName == "RtfZBlob")
#else
			if (propertyName == "HtmlZBlob")
#endif
			{
				return ZBlob.Empty;
			}
			else if (propertyName == "ReadOnly")
			{
				return true;
			}
			else
			{
				return base.GetDataSource(propertyName);
			}
		}

		protected override string[] BindablePropertyNames
		{
#if !WINZOR
			get { return new string[] { "RtfZBlob", "ReadOnly" }; }
#else
			get { return new string[] { "HtmlZBlob", "ReadOnly" }; }
#endif
		}

		protected override string InvalidBindablePropertyName
		{
			get { return "MaximumLengthOfWorld"; }
		}

		protected override void CheckNoDataBindingsExist(Control control)
		{
#if !WINZOR
			if (control is ZRichTextBoxToolBar)
			{
				return;
			}
#endif

			base.CheckNoDataBindingsExist(control);
		}

		protected override void BindControl()
		{
			base.BindControl();
			Control.SetDataBinding(Dummy, DummyBizoSchema.Z0_VarBinaryMax.Name);
		}

		protected override ZRichTextBox GetNewControl()
		{
			var result = new TestRichTextBox();
			result.CreateControl();
			return result;
		}

		class TestRichTextBox : ZRichTextBox
		{
			public new RichTextBox RichEdit
			{
				get { return base.RichEdit; }
			}
		}
	}
}
