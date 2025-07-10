using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.Accounting.Business.CashBook.DirectDebitBatch;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class DirectDebitFileModule : FilterGridModuleWithMultipleReversing
	{
		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.DirectDebitFile; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.DDRFile; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.Core; }
		}

		protected override bool CanBeCopied()
		{
			return false;
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return new DirectDebitFileController();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DirectDebitBatchHeaderCollection(Factory);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new DirectDebitFileFilterControl(GridCollection, FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DirectDebitFileFilterBusinessObject();
		}

		#region Menu Items

		protected override ResourceStringData GetDeleteMenuItemText()
		{
			return Res.GetData("D77E0C23-EFAE-4B36-A462-C6187FC4EEEB", "Cancel Batch", "Cancels the selected batch after viewing its details read-only (shortcut Del)");
		}

		#endregion
	}
}