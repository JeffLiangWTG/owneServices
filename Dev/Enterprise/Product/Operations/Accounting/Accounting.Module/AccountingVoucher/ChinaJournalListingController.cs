using System;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingVoucherPrint;
using Enterprise.Accounting.GUI.AccountingVoucherPrinting;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Accounting.Module
{
	public partial class ChinaJournalListingController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new ChinaJournalListingPrintForm(new ChinaJournalListingPrintWrapper());
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.ChinaJournalListing; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(ChinaJournalListingPrintWrapper); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new ChinaJournalListingPrintWrapper();
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ChinaJournalListing; }
		}
	}
}
