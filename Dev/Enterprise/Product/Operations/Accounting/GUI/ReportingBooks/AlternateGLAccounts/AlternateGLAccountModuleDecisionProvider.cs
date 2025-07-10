using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Internal;

namespace Enterprise.Accounting.GUI
{
	public class AlternateGLAccountModuleDecisionProvider : PopupModuleDecisionProvider
	{
		public AlternateGLAccountModuleDecisionProvider(IFindBox findBox) : base(findBox)
		{
		}

		public override BusinessObject[] PreHandleSelectBusinessObjects(BusinessObject[] selectedBusinessObjects)
		{
			var bOs = selectedBusinessObjects;
			if (selectedBusinessObjects.FirstOrDefault() is AlternateGLAccountCombineParentAccount)
			{
				bOs = selectedBusinessObjects.Cast<AlternateGLAccountCombineParentAccount>().Select(x => x.AlternateGLAccount).ToArray();
			}
			return bOs;
		}

		protected override void HandleDefaultActionCore(BusinessObject[] selectedBusinessObjects)
		{
			var bOs = selectedBusinessObjects;
			if (selectedBusinessObjects.FirstOrDefault() is AlternateGLAccountCombineParentAccount)
			{
				bOs = selectedBusinessObjects.Cast<AlternateGLAccountCombineParentAccount>().Select(x => x.AlternateGLAccount).ToArray();
			}
			HandleFindBoxOKButton(bOs);
		}

		public override IBusinessObjectCollection List => null;
	}
}
