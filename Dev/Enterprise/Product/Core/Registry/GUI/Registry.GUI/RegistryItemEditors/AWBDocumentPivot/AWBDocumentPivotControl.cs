using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Registry.GUI
{
	public partial class AWBDocumentPivotControl : ZUserControl
	{
		public AWBDocumentPivotControl(bool isTitleColumnReadOnly)
		{
			InitializeComponent();

			PivotCollection = new AWBDocumentPivotCollection(new BusinessObjectFactory());
			PivotCollection.SetReadOnlyIncludingChildren(true);

			zTextBoxColumnStyleInfo2.IsReadOnly = isTitleColumnReadOnly;

			SetDataBinding(PivotCollection, "");
		}

		public bool ReadOnly
		{
			get { return fReadOnly; }
			set
			{
				fReadOnly = value;
				PivotCollection.SetReadOnlyIncludingChildren(value);
				HAWBDocumentPivotGrid.ReadOnly = value;
			}
		}
		bool fReadOnly;

		public AWBDocumentPivotCollection PivotCollection;

		public byte[] FieldValue
		{
			get
			{
				return PivotCollection.ToXmlArray();
			}
			set
			{
				PivotCollection.LoadFromXmlArray(value);
			}
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);
			HAWBDocumentPivotGrid.PerformLayout();
		}
	}
}
