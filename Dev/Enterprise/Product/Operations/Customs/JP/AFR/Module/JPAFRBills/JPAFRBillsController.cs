using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Customs.JP.AFR.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.JP.AFR.Module
{
	public class JPAFRBillsController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.JP.AFRBill; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.JP.AFRBill; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JPAFRBills); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			var consol = businessEntity as ForwardingConsol;
			return consol != null ? CreateConsolForm(consol) : CreateJPAFRForm((JPAFRHeader)businessEntity);
		}

		IZForm CreateConsolForm(ForwardingConsol consol)
		{
			var result = new ConsolForm(consol);
			result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.JP.AFRPluggedIntoConsol;
			return result;
		}

		IZForm CreateJPAFRForm(JPAFRHeader header) => new JPAFRForm(header);

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var bill = (JPAFRBills)sourceEntity;
			var header = Factory.Load<JPAFRHeader>(bill.JPB_JPH_Header);
			return header == null ? null : (IBusiness)header.Consol ?? header;
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ((IControllerIDProvider)businessEntity).ControllerID.ToString();
		}

		protected override IZForm ShowLoadedForm(IBusiness sourceEntity, FormAction action)
		{
			ZForm form = null;
			if (sourceEntity is JPAFRHeader header)
			{
				var reloadHeader = Factory.Load<JPAFRHeader>(header.PK);
				form = (ZForm)CreateJPAFRForm(reloadHeader);
				ShowForm(form);
			}
			else if (sourceEntity is ForwardingConsol consol)
			{
				var reloadConsol = Factory.Load<ForwardingConsol>(consol.PK);
				form = (ZForm)CreateConsolForm(reloadConsol);
				ShowForm(form);
			}
			else if (sourceEntity is JPAFRBills bill)
			{
				form = (ZForm)base.ShowLoadedForm(bill, action);
				SelectAndShowBill(form, bill);
			}

			form.DisableNewAction();
			return form;
		}

		void SelectAndShowBill(ZForm form, JPAFRBills bill)
		{
			var consolForm = form as ConsolForm;
			if (consolForm != null)
			{
				var plugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Customs.JP.AFRPluggedIntoConsol);
				if (plugIn.Enabled)
				{
					var userControl = plugIn.UserControl as JPAFRConsolManifestUserControl;
					if (userControl != null)
					{
						userControl.SelectAndShowBill(bill.PK);
					}
				}
			}
			else
			{
				var afrForm = form as JPAFRForm;
				if (afrForm != null)
				{
					afrForm.SelectAndShowBill(bill.PK);
				}
			}
		}

		#region Show Form

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Delete not supported from this AFR Bill.");
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			throw new ModuleGuiNotSupportedException("Template copy not supported from AFR Bill.");
		}

		protected override IZForm ShowFormForNewEntityCore(IBusiness businessEntity)
		{
			ZForm formToShow = null;
			if (businessEntity is JPAFRHeader header)
			{
				formToShow = (ZForm)CreateJPAFRForm(header);
			}
			else if (businessEntity is ForwardingConsol consol)
			{
				formToShow = (ZForm)CreateConsolForm(consol);
			}

			if (formToShow != null)
			{
				SetControllerID(formToShow, ID);
				formToShow.DisableNewAction();
				return formToShow;
			}
			else
			{
				throw new ModuleGuiNotSupportedException("New not supported from AFR Bill.");
			}
		}

		#endregion

		#region Security

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.JPAFRBillReporting; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.JPAFRBillReporting; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.JPAFRBillReporting; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.JPAFRBillReporting; }
		}

		#endregion
	}
}
