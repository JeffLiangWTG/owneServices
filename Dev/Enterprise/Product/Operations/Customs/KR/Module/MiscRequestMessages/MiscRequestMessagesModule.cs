using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.GUI;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.KR.Module
{
	public class MiscRequestMessagesModule : ZFilterGridModule
	{
		public override ModuleIdentifier ID => ModuleIDs.Customs.KR.MiscRequestMessages;
		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.MiscRequestMessages;
		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;
		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.Customs.KR.MiscRequestMessages);
		protected override FilterBusinessObject GetNewFilterBusinessObject() => new MiscRequestMessagesFilterStripBusinessObject();
		protected override IFilterControl GetNewFilterControl() => new MiscRequestMessagesFilterControl(GridCollection, FilterBusinessObject);
		protected override IBusinessObjectCollection GetNewGridCollection() => new CusMiscRequestHeaderCollection(Factory, GlbCompany.CurrentCompany);
		public override bool AllowDelete => false;
		public override bool AllowNew => false;
		public override bool AllowEdit => false;

		protected override MenuItem[] GetNewStandardMenuItems()
		{
			var menuItems = new List<MenuItem>(base.GetNewStandardMenuItems());
			menuItems.Insert(0, GetNewMenuItem());
			return menuItems.ToArray();
		}

		MenuItem GetNewMenuItem()
		{
			var result = new ZMenuItem(Res.GetData("A2425E9C-BF96-4680-8A7D-A4210DD0A497", "New"), IconTypes.NewButtonActive, IconTypes.NewButtonRest);
			result.MenuItems.Add(Res.GetString("4007BE9C-CD68-462D-A889-A36A9644ED58", "5AC - (EXP) Application for Extended Office Hours"), New5ACMenuItem_Click);
			result.MenuItems.Add(Res.GetString("30644B27-D597-4794-8E9A-3833E2C03A45", "5GW - (IMP) Application for Extended Office Hours"), New5GWMenuItem_Click);
			result.MenuItems.Add(Res.GetString("5544BC1D-0462-4BED-A8BF-C985ECB59C58", "5SG - (IMP) Final Price Period Extension Application"), New5SGMenuItem_Click);
			return result;
		}

		void New5ACMenuItem_Click(object sender, EventArgs e)
		{
			ShowNewMiscRequestForm(ElectronicDocumentTypeList.Codes._5AC);
		}
		void New5GWMenuItem_Click(object sender, EventArgs e)
		{
			ShowNewMiscRequestForm(ElectronicDocumentTypeList.Codes._5GW);
		}
		void New5SGMenuItem_Click(object sender, EventArgs e)
		{
			ShowNewMiscRequestForm(ElectronicDocumentTypeList.Codes._5SG);
		}

		void ShowNewMiscRequestForm(ZString type)
		{
			if (type == ElectronicDocumentTypeList.Codes._5SG)
			{
				var finalPriceReportByDateExtensionHeader = new FinalPriceReportByDateExtensionHeader(Factory);
				ZFormModaliser.ShowDialogAndDispose(new FinalPriceExtensionRequestNewForm(finalPriceReportByDateExtensionHeader));
			}
			else
			{
				var extendedHoursRequestHeader = new ExtendedHoursRequestHeader(Factory, type, GlbCompany.CurrentCompany.PK);
				ZFormModaliser.ShowDialogAndDispose(new ExtendedHoursRequestNewForm(extendedHoursRequestHeader));
			}
		}
	}
}
