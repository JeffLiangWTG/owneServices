using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.DocumentVisualizer.DocDataObjects.Testing
{
	class DummyWrappedDocDataObject : DocDataObject
	{
		public DummyWrappedDocDataObject(DummyDocDataObject parent)
		{
			this.parent = parent;
		}

		readonly DummyDocDataObject parent;

		public ZString WrappedText
		{
			get => parent.Text;
			set => parent.Text = value;
		}

		public ZPropertyInfo WrappedTextInfo => GetWrappedZPropertyInfo(nameof(WrappedText), x => parent.TextInfo);
	}
}
