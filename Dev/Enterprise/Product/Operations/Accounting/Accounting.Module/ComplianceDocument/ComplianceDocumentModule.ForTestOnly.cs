#if DEBUG

using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public partial class ComplianceDocumentModule
	{
		public MenuItem[] GetNewAdditionalMenuItems_ForTestOnly()
		{
			return GetNewAdditionalMenuItems();
		}

		public ZForm LastShownComplianceDocumentForm_ForTest_ForTestOnly
		{
			get { return LastShownComplianceDocumentForm_ForTest; }
			set { LastShownComplianceDocumentForm_ForTest = value; }
		}

		public BusinessObject[] SelectedBusinessObjects_ForTestOnly => SelectedBusinessObjects;
	}
}

#endif
