using System.Windows.Forms;

using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Licencing.Module
{
	public class LicenceUsageFilterGridModule : ZFilterGridModule
	{
		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.LicenceUsage; }
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.LicenceUsage; }
		}

		protected override IZForm ShowNewForm()
		{
			return null;
		}

		protected override IZForm ShowViewForm(BusinessObject selectedBusinessObject)
		{
			return null;
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return null;
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new LicenceUsageFilterControl(GridCollection, (LicenceUsageFilterBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new LicenceUsageCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new LicenceUsageFilterBusinessObject();
		}

		public override bool AllowDelete
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return false; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			return System.Array.Empty<MenuItem>();
		}

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
	}
}
