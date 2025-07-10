using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.JP.AFR.Business;
using Enterprise.Customs.JP.AFR.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.Freight.Forwarding.Module;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.JP.AFR.Module
{
	/// <summary>
	/// This controller is used when a AFR is attached to consol to show ConsolForm from AFR module
	/// </summary>
	public class JPAFRConsolController : JobConsolController
	{
		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.JP.AFRPluggedIntoConsol; }
		}

		public override IZForm ShowDeleteForm(BusinessObject sourceEntity)
		{
			var header = sourceEntity as JPAFRHeader;
			var consol = header == null ? null : header.Consol;
			return base.ShowDeleteForm(consol ?? sourceEntity);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.JP.AFR; }
		}

		public override IZForm ShowNewForm()
		{
			throw new ControllerShowNewFormNotSupportedException("Show New Form is not supported as it should be handle by the consol module");
		}

		public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
		{
			throw new ModuleTemplateCopyNotSupportedException("Show Template Copy Form is not supported as it should be handle by the consol module");
		}

		protected override string GetIDForFormCache(IBusiness businessEntity)
		{
			return ((IControllerIDProvider)businessEntity).ControllerID.ToString();
		}

		protected override ConsolForm GetFormCore(ForwardingConsol businessEntity)
		{
			var result = new ConsolForm(businessEntity);
			result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.JP.AFRPluggedIntoConsol;
			return result;
		}

		protected override IBusiness LoadBusinessEntity(BusinessObjectFactory factory, ZGuid sourceEntityPK)
		{
			IBusiness result = null;
			var header = factory.Load<JPAFRHeader>(sourceEntityPK);
			if (header != null)
			{
				result = header.Consol;
			}
			else
			{
				var consol = factory.Load<ForwardingConsol>(sourceEntityPK);
				if (consol != null)
				{
					result = consol;
				}
			}
			return result;
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new JPAFRPlugIn((ForwardingConsol)businessEntity);
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.AdvanceFilingRules; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.AdvanceFilingRules; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.AdvanceFilingRules; }
		}

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.AdvanceFilingRules; }
		}

		public override ZGuid LastSavedPK
		{
			get
			{
				var form = LastShownForm as ZForm;
				var businessObject = form == null ? null : form.DataSource as IIdentified;
				if (businessObject != null)
				{
					var consol = businessObject as ForwardingConsol;
					if (consol != null)
					{
						var header = consol.GetAFRHeader();
						if (header != null)
						{
							return header.PK;
						}
					}
				}

				return base.LastSavedPK;
			}
		}

		public override ResourceStringData PluginTabPageCaption
		{
			get { return Enterprise.Customs.JP.AFR.Module.Res.GetData("AFRPlugInTabPage|JPAFR", "AFR", "AFR", "Advance Filing Rules"); }
		}
	}
}
