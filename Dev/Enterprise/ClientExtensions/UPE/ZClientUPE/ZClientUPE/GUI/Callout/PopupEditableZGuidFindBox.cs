using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public class PopupEditableZGuidFindBox : ZGuidFindBox
	{
		public PopupEditableZGuidFindBox()
		{
		}

		protected override bool AllowShowEditForm
		{
			get { return !string.IsNullOrEmpty(Code); }
		}
		internal bool InternalAllowShowEditForm => AllowShowEditForm;
		internal string InternalCode
		{
			get { return Code; }
			set { Code = value; }
		}
	}
}
