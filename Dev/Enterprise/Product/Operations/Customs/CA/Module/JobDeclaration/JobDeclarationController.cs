using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.GUI;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.Module
{
	public class JobDeclarationController : Customs.Module.JobDeclarationController
	{
		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(JobDeclaration); }
		}

		protected override ZArchitecture.PlugIn.ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new BrokeragePlugIn((ForwardingShipment)businessEntity);
		}

		protected override IZForm GetFormCore(IBusiness businessEntity)
		{
			var simplifiedLVS = businessEntity as SimplifiedLVS;
			if (simplifiedLVS != null)
			{
				return new SimplifiedLVSForm(simplifiedLVS);
			}
			return new JobDeclarationForm((JobDeclaration)businessEntity);
		}

		internal IZForm ShowNewLVSForm()
		{
			var declaration = (JobDeclaration)GetNewBusinessEntityInLocalFactory();
			declaration.JE_MessageType = JobMessageTypeList.Codes.LowValueShipments;
			return ShowFormForNewEntityCore(declaration);
		}

		internal IZForm ShowNewSimplifiedLVSForm()
		{
			var simplifiedLVS = new SimplifiedLVS(Factory);
			return ShowFormForNewEntity(simplifiedLVS);
		}

		internal IZForm ShowNewCopyToB2Form(JobDeclaration declaration)
		{
			return ShowFormForNewEntityCore(declaration);
		}

		internal IZForm ShowNewPrecarmAdjustmentForm(JobDeclaration declaration)
		{
			return ShowFormForNewEntityCore(declaration);
		}
	}
}
