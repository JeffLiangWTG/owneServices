using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business.MasterFiles;
using Enterprise.Customs.EU.GUI;
using Enterprise.ZArchitecture.PlugIn;

namespace Enterprise.Customs.EU.Module
{
	public class OrgSupplierPartController : MasterFiles.Module.OrgSupplierPartController
	{
		public OrgSupplierPartController()
		{ }

		public override bool MakeUrlsOnlyOpenableForCurrentCompany => true;

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return new MasterFiles.Business.OrgSupplierPartTypeDecider().GetTypeForCountryCode(Enterprise.MasterFiles.Business.GlbCompany.CurrentCompany.GC_RN_NKCountryCode); }
		}

		protected override ZPlugIn GetPlugIn(IBusiness businessEntity)
		{
			return new EUOrgSupplierPartFormCustomsPlugin((OrgSupplierPart)businessEntity);
		}
	}
}
