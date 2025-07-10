using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging;
using Enterprise.Customs.GB.GUI.Ccsuk;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module.StandAloneFsrEnquiry
{
	public class StandAloneFsrEnquiryController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public StandAloneFsrEnquiryController()
		{
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukStandAloneFsrEnquiry; }
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.GB.CcsukStandAloneFsrEnquiry; }
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness npbo)
		{
			var manager = new NewStandAloneFsrEnquiryManager((NonPersistentStandAloneFsrEnquiryForNew)npbo);
			var form = new StandAloneFsrEnquiryFormForNew(manager);
			ZFormModaliser.ShowDialogAndDispose(form);
			return form;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return new NonPersistentStandAloneFsrEnquiryForNew(Factory);
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(Ccsuk.AirCargoInventory.Messaging.StandAloneFsrEnquiry); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			return new StandAloneFsrEnquiryForm((Ccsuk.AirCargoInventory.Messaging.StandAloneFsrEnquiry)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AirCcsukEnquiryView; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AirCcsukEnquiryNew; }
		}

		protected override SecurityCheckpoint CheckPointForEdit  // not really applicable
		{
			get { return Env.Security.AirCcsukEnquiry; }
		}

		protected override SecurityCheckpoint CheckPointForDelete  // not really applicable
		{
			get { return Env.Security.AirCcsukEnquiry; }
		}
	}
}
