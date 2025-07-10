using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.FR.Business.Declaration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module
{
	[UniversalCopyInstanceType(InstanceType = typeof(JobDeclaration))]
	public class JobDeclarationModule : EU.Module.JobDeclarationModule
	{
		protected override Customs.Module.JobDeclarationController GetControllerForStandAlone() => new JobDeclarationController();

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new JobDeclarationFilterBusinessObject();

		protected override IFilterControl GetNewFilterControl() => new JobDeclarationFilterStripControl(this, GridCollection, FilterBusinessObject);

		protected override MenuItem[] GetNewActionMenuItems()
		{
			var result = new List<MenuItem>(base.GetNewActionMenuItems());

			if (DeltaGInboundInterchangeImporter.IsMenuItemVisible())
			{
				result.Add(DeltaGInboundInterchangeImporter.GetNewMenuItem());
			}

			if (DeltaIEInboundInterchangeImporter.IsMenuItemVisible())
			{
				result.Add(DeltaIEInboundInterchangeImporter.GetNewMenuItem());
			}

			return result.ToArray();
		}

		DeltaGInboundInterchangeImporter DeltaGInboundInterchangeImporter => deltaGInboundInterchangeImporter ?? (deltaGInboundInterchangeImporter = new DeltaGInboundInterchangeImporter(Factory));
		DeltaGInboundInterchangeImporter deltaGInboundInterchangeImporter;

		DeltaIEInboundInterchangeImporter DeltaIEInboundInterchangeImporter => deltaIEInboundInterchangeImporter ?? (deltaIEInboundInterchangeImporter = new DeltaIEInboundInterchangeImporter(Factory));
		DeltaIEInboundInterchangeImporter deltaIEInboundInterchangeImporter;
	}
}
