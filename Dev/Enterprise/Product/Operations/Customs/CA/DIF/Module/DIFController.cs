using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business;
using Enterprise.Customs.CA.DIF.Business;
using Enterprise.Customs.CA.DIF.GUI;
using Enterprise.Customs.Module;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.DIF.Module
{
	public sealed class DIFController : DISControllerBase
	{
		protected override SecurityCheckpoint CheckPointForNew => Env.Security.CACustomsDIFEdit;

		protected override SecurityCheckpoint CheckPointForDelete => Env.Security.None;

		protected override SecurityCheckpoint CheckPointForEdit => Env.Security.CACustomsDIFEdit;

		protected override SecurityCheckpoint CheckPointForView => Env.Security.CACustomsDIFView;

		protected override IZForm GetForm(IBusiness businessEntity)
		{
			if (businessEntity is CusCALPCO lpco &&
				lpco.Parent is IDeclarationProvider declarationProvider &&
				declarationProvider.Declaration is ICADIFHost difHostFromLPCO)
			{
				var difHostWrapper = new DIFHostWrapper(difHostFromLPCO);
				var difRefNumberOrLocationOfLPCO = lpco.CLP_DIFRefNumberOrLocation;
				var pgaOfLPCO = (lpco.Parent as IPGAHeader)?.GovAgencyIDCode ?? ZString.Empty;
				var difDocument = difHostWrapper.DISDocuments.Cast<DIFDocument>().FirstOrDefault(x => x.URN == difRefNumberOrLocationOfLPCO && x.PGA == pgaOfLPCO);
				if (difDocument == null)
				{
					difDocument = difHostWrapper.DISDocuments.AddNew();
					difDocument.SetDefaultFromLPCO(lpco);
				}
				return new DIFForm(difDocument);
			}
			else
			{
				var difHost = (ICADIFHost)businessEntity;
				return new DIFForm(new DIFHostWrapper(difHost));
			}
		}

		public override Type TypeOfTopLevelBusinessObject => typeof(DIFHostWrapper);

		protected override IBusiness GetLoadedBusinessEntityInLocalFactory(IBusiness sourceEntity) => sourceEntity;
	}
}
