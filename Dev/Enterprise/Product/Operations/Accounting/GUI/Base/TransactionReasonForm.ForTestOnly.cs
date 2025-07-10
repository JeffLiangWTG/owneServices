#if DEBUG

namespace Enterprise.Accounting.GUI.Base
{
	public partial class TransactionReasonForm
	{
		public ZArchitecture.GUI.ZDropEdit ReversingReasonCodeDropEdit_ForTestOnly
		{
			get { return ReversingReasonCodeDropEdit; }
			set { ReversingReasonCodeDropEdit = value; }
		}

		public ZArchitecture.ZTextBox ReasonTextBox_ForTestOnly
		{
			get { return ReasonTextBox; }
			set { ReasonTextBox = value; }
		}

		public ZArchitecture.GUI.ZButton OKReasonButton_ForTestOnly
		{
			get { return OKReasonButton; }
			set { OKReasonButton = value; }
		}

		public ZArchitecture.ZTextBox SupportingDocumentNumberTextBox_ForTestOnly
		{
			get { return SupportingDocumentNumberTextBox; }
			set { SupportingDocumentNumberTextBox = value; }
		}

		public ZArchitecture.ZLabel SupportingDocumentNumLabel_ForTestOnly
		{
			get { return SupportingDocumentNumLabel; }
			set { SupportingDocumentNumLabel = value; }
		}

		public ZArchitecture.GUI.ZCheckBox IsAmendInFullCheckBox_ForTestOnly
		{
			get { return IsAmendInFullCheckBox; }
			set { IsAmendInFullCheckBox = value; }
		}

		public bool IsAmendInFull_ForTestOnly
		{
			get { return reversingHolder.IsAmendInFull; }
			set { reversingHolder.IsAmendInFull = value; }
		}
	}
}

#endif
