using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects.Genral;
using Enterprise.Customs.GB.GUI.Ccsuk;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module
{
	public class CcsukGenralMessageController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public CcsukGenralMessageController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukGenralMessage; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.CcsukGenralMessage; }
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness nonPersistentGenralEdiMessageForNew)
		{
			var manager = new NewGenralMessageManager((NonPersistentGenralEdiMessageForNew)nonPersistentGenralEdiMessageForNew);
			var form = new CcsukGenralMessageFormForNew(manager);
			ZFormModaliser.ShowDialogAndDispose(form);
			return form;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new NonPersistentGenralEdiMessageForNew(Factory);
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(GenralEdiMessage); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new CcsukGenralMessageForm((GenralEdiMessage)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AirCcsukGenralView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AirCcsukGenralNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit   // not really applicable
		{
			get { return Env.Security.AirCcsukGenral; }
		}

		protected override SecurityCheckpoint CheckPointForDelete   // not really applicable
		{
			get { return Env.Security.AirCcsukGenral; }
		}
	}
}
