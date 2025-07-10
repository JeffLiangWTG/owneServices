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
	public class JPAFRController : ZController
	{
		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public bool CreateVOCCAFR;

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.JP.AFR; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Customs.JP.AFR; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JPAFRHeader); }
		}

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			ZForm result = null;
			var consol = businessEntity as ForwardingConsol;
			if (consol != null)
			{
				SetInitialTabPageNameToSelectWhenAFormIsShown(string.Empty);
				result = new ConsolForm(consol);
				result.PlugInIDToSelectOnLoaded = ControllerIDs.Customs.JP.AFRPluggedIntoConsol;
			}
			else
			{
				result = new JPAFRForm((JPAFRHeader)businessEntity);
			}
			return result;
		}

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity)
		{
			var header = Factory.Load<JPAFRHeader>(sourceEntity.Identifier)
				?? throw new ArgumentException("SourceEntity");
			return (IBusiness)header.Consol ?? header;
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			var result = (JPAFRHeader)base.GetNewBusinessEntityInLocalFactory();
			result.JPH_IsShippingLineEntry = CreateVOCCAFR;
			return result;
		}

		#region Security

		protected override SecurityCheckpoint CheckPointForDelete
		{
			get { return Env.Security.JPAFRReporting; }
		}

		protected override SecurityCheckpoint CheckPointForEdit
		{
			get { return Env.Security.JPAFRReporting; }
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.JPAFRReporting; }
		}

		protected override SecurityCheckpoint CheckPointForView
		{
			get { return Env.Security.JPAFRReporting; }
		}

		#endregion
	}
}
