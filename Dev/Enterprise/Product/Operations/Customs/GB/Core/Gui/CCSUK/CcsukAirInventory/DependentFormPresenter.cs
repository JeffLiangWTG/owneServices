using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	class DependentFormPresenter : IFormPresenter
	{
		public void ShowError(string message, string caption)
		{
			Globals.Message.ShowError(message, caption);
		}

		public void ShowNew(ControllerID controllerId, BusinessObject bo)
		{
			ZControllerFactory.Create(controllerId).ShowFormForNewEntity(bo);
		}

		public void ShowEdit(ControllerID controllerId, BusinessObject bo)
		{
			ZControllerFactory.Create(controllerId).ShowEditForm(bo);
		}

		public void ShowView(ControllerID controllerId, BusinessObject bo)
		{
			ZControllerFactory.Create(controllerId).ShowViewForm(bo);
		}
	}
}
