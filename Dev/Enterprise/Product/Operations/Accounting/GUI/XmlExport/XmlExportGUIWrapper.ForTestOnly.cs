#if DEBUG

using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.XmlExport
{
	public partial class XmlExportGUIWrapper
	{
		public string FileName_ForTestOnly => FileName;

		public string InitialDirectory_ForTestOnly => InitialDirectory;

		public string DialogFilter_ForTestOnly => DialogFilter;

		public string FileExtention_ForTestOnly => FileExtention;

		public ProgressForm ProgressForm_ForTestOnly
		{
			get { return ProgressForm; }
			set { ProgressForm = value; }
		}

		public void ValidateDatesTogether_ForTestOnly()
		{
			ValidateDatesTogether();
		}

		public void ValidatePeriodsTogether_ForTestOnly()
		{
			ValidatePeriodsTogether();
		}

		public void ValidateTransactionNumbersTogether_ForTestOnly()
		{
			ValidateTransactionNumbersTogether();
		}

		public virtual void LoadFormAndExport_ForTestOnly()
		{
			LoadFormAndExport();
		}

		public Form ParentForm_ForTestOnly
		{
			get { return ParentForm; }
			set { ParentForm = value; }
		}
	}
}

#endif
