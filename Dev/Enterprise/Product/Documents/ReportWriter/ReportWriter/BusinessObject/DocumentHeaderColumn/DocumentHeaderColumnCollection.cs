namespace Enterprise.ReportWriter
{
	public class DocumentHeaderColumnCollection : ColumnDataCollection
	{
		public DocumentHeaderColumnCollection(DocumentHeaderRow parent)
			: base(parent)
		{
		}

		public new DocumentHeaderRow parent
		{
			get { return (DocumentHeaderRow)base.parent; }
		}

		#region Implementation

		protected override bool AllowNewCore
		{
			get { return !parent.IsCustomisedColumnRow; }
		}

		#endregion
	}
}
