using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.GUI.Ccsuk;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Module.StandAloneFsrEnquiry
{
	public class StandAloneFsrEnquiryModule : ZFilterGridModule
	{
		public StandAloneFsrEnquiryModule()
		{ }

		protected override IFilterControl GetNewFilterControl()
		{
			return new StandAloneFsrEnquiryFilterStripControl(GridCollection, FilterBusinessObject);
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.Customs.GB.CcsukStandAloneFsrEnquiry);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new StandAloneFsrEnquiryFilterStripBusinessObject();
		}

		public override bool AllowDelete
		{
			get { return true; }
		}

		public override bool AllowEdit
		{
			get { return false; }
		}

		public override bool AllowNew
		{
			get { return true; }
		}

		public override bool AllowView
		{
			get { return true; }
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new Ccsuk.AirCargoInventory.Messaging.StandAloneFsrEnquiryCollection(Factory);
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.Customs.EU.GB.CcsukStandAloneFsrEnquiry; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.AirCcsukBase; }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.AirCcsukEnquiry; }
		}

		protected override MenuItem[] GetUniversalDataTransferMenuItems()
		{
			var baseDataOptions = base.GetUniversalDataTransferMenuItems();
			var list = new List<MenuItem>();
			if (baseDataOptions != null)
			{
				list.AddRange(baseDataOptions);
			}
			list.Add(new ZMenuItem("Import AWBs from CSV", ImportRecordsFromCsvClick));
			return list.ToArray();
		}

		void ImportRecordsFromCsvClick(object s, EventArgs e)
		{
			new AwbMigrationDataLoadForm().Show();
		}
	}
}

