using System;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DataConverters.CustomsFiles
{
	public class CustomsFilesImportController : ZSingletonController
	{
		protected override IZForm GetForm(IBusiness businessEntity)
		{
			switch (GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString())
			{
				case Core.Constants.CountryCodes.NewZealand:
					{
						return new NZ.GUI.ClassificationAndPartImporterForm();
					}
				default:
					return new MainForm();
			}
		}

		public override ControllerID ID
		{
			get { return ControllerIDs.Customs.ImportCustomsFilesData; }
		}

		public override ModuleIdentifier ModuleID
		{
			get { return null; }
		}

		public override Type TypeOfTopLevelBusinessObject
		{
			get { return typeof(DummyImporter); }
		}

		protected override IBusiness GetNewBusinessEntityInLocalFactory()
		{
			return null;
		}

		protected override SecurityCheckpoint CheckPointForNew
		{
			get { return Env.Security.ImportCustomsFilesData; }
		}
	}
}
