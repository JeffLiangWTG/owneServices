using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common.BR;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class GenerateLicenseJobDeclarationModuleCollection : JobDeclarationCollection, IFilterModuleExtraNotificationProvider
	{
		public GenerateLicenseJobDeclarationModuleCollection(JobDeclaration declaration) : base(declaration.Factory, declaration.JE_GC)
		{
			importSiscomexDeclaration = Argument.NotNull(declaration, nameof(declaration));
		}

		readonly JobDeclaration importSiscomexDeclaration;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var importLicenseDeclaration = child as JobDeclaration;
			importLicenseDeclaration.JE_OH_Importer = importSiscomexDeclaration.JE_OH_Importer;
			importLicenseDeclaration.JE_TransportMode = importSiscomexDeclaration.JE_TransportMode;
			importLicenseDeclaration.JE_GoodsOrigin = importSiscomexDeclaration.JE_GoodsOrigin;
		}

		public static GenerateLicenseJobDeclarationModuleCollection GetListForGenerateImportLicense(JobDeclaration declaration)
		{
			var collection = new GenerateLicenseJobDeclarationModuleCollection(declaration);

			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.FilterConstants.Declaration.ImporterSupplier, "Property1", declaration.JE_OH_Importer, false));
			return collection;
		}

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			INotification notification = null;

			if (businessObject is JobDeclaration declaration && !AllowToGenerate(declaration))
			{
				notification = new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("46207A66-E3FE-4E0F-9C09-5CE9CB01EEF1", "A Job selected from here must be an Import License and matches the Import Declaration related."));
			}

			return notification;
		}

		bool AllowToGenerate(JobDeclaration declaration)
		{
			return (declaration.JE_MessageType == BRJobMessageTypeList.Codes.ImportLicense
				&& declaration.JE_OH_Importer == importSiscomexDeclaration.JE_OH_Importer);
		}
	}
}
