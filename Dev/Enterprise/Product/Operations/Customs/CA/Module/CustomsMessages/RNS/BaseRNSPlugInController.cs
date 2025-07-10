using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.CA.Module
{
	abstract class BaseRNSPlugInController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			throw new ModuleGuiNotSupportedException("This controller does not have GUI.");
		}

		protected override Security.SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.None; }
		}

		protected override Security.SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.None; }
		}
	}
}
