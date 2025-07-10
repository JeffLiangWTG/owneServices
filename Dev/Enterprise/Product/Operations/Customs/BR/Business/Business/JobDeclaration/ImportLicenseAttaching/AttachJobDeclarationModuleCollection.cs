using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.BR;
using Enterprise.ZArchitecture.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.BR.Business
{
	public class AttachJobDeclarationModuleCollection : JobDeclarationCollection, IFilterModuleExtraNotificationProvider
	{
		public AttachJobDeclarationModuleCollection(JobDeclaration declaration) : base(declaration.Factory, declaration.JE_GC)
		{
			attachToDeclaration = Argument.NotNull(declaration, nameof(declaration));
		}
		protected JobDeclaration attachToDeclaration;

		public static AttachJobDeclarationModuleCollection GetListForImportLicenseAttaching(JobDeclaration declaration)
		{
			var collection = new AttachJobDeclarationModuleCollection(declaration);

			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.FilterConstants.Declaration.EntryNumber, "ComparisonOperator", (ZString)ModuleTextFilter.ComparisonConstants.IsNotBlank, false));
			collection.FilterBusinessObjectDefaults.Add(new FilterBusinessObjectDefault(Constants.FilterConstants.Declaration.ImporterSupplier, "Property1", declaration.JE_OH_Importer, false));

			return collection;
		}

		INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
		{
			INotification notification = null;

			if (businessObject is JobDeclaration declaration && !AllowToAttach(declaration))
			{
				notification = new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("1327FBF0-BDEE-4A62-BF02-57FC163D0599", "A Job selected from here must be an Import License, have at least one registered Entry available and matches the Import Declaration related."));
			}

			return notification;
		}

		bool AllowToAttach(JobDeclaration declaration)
		{
			return (declaration.JE_MessageType == BRJobMessageTypeList.Codes.ImportLicense
				&& declaration.JE_OH_Importer == attachToDeclaration.JE_OH_Importer
				&& declaration.GetPossibleEntryInstructionForAttachment().Any());
		}
	}
}
