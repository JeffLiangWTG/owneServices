using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.ASYCUDA.Module
{
	public class ASYCUDAManifestBillController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID => ControllerIDs.Customs.ASYCUDA.ASYCUDAManifestBill;

		public override ModuleIdentifier ModuleID => ModuleIDs.Customs.ASYCUDA.ManifestBill;

		public override Type TypeOfTopLevelBusinessObject => typeof(AsycudaBill);

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Delete not supported from Manifest Bill.");
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Template copy not supported from Manifest Bill.");
		}

		protected override IZForm GetForm(IBusiness businessEntity) => new AsycudaBillForm((AsycudaBill)businessEntity);

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity) => throw new ModuleGuiNotSupportedException("New not supported from Manifest Bill.");

		protected override SecurityCheckpoint CheckPointForDelete => null;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CustomsFiles;

		protected override SecurityCheckpoint CheckPointForNew => null;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CustomsFiles;
	}
}
