using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.KR.Business;
using Enterprise.Customs.KR.GUI;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.Module
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
			var declaration = (JobDeclaration)businessEntity;
			if (ElectronicDocumentTypeList.IsMiscDeclaration(declaration.JE_MessageType))
			{
				return new MiscDeclarationForm(declaration);
			}
			return new JobDeclarationForm(declaration);
		}

		internal IZForm ShowMiscDeclarationForm(string messageType)
		{
			var declaration = (JobDeclaration)GetNewBusinessEntityInLocalFactory();
			declaration.JE_MessageType = messageType;
			return ShowFormForNewEntityCore(declaration);
		}
	}
}
