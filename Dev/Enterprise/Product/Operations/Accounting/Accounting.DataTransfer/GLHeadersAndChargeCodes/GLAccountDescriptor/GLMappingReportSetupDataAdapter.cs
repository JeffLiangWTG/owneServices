using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.GLHeadersAndChargeCodes
{
	partial class GLMappingReportSetupDataAdapter : ValueObjectDataAdapter<GLDescriptorPivot, Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup>
	{
		#region Data Adapter Overrides

		public override string RootCollectionElementName
		{
			get { return "GLHeaderMultiLanguageReportSetups"; }
		}

		public override string RootElementName
		{
			get { return "GLHeaderMultiLanguageReportSetup"; }
		}

		public override XmlSchema Schema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.GLHeaderMultiLanguageReportSetupSchema; }
		}

		public override XmlSchema CollectionSchema
		{
			get { return AccountingXmlSchemaDefinitions.Instance.GLHeaderMultiLanguageReportSetupsSchema; }
		}

		public override bool OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxChecked
		{
			get { return AccountingConfigurationRegistry.Instance.DataImportShouldSaveOnlyWhenThereAreNoErrors.Value; }
		}

		public override bool OnlySaveDataWhenNoRecordsHaveErrorsCheckBoxVisible
		{
			get { return true; }
		}

		#endregion

		#region Export

		protected override void ExportToValueObjectCore(GLDescriptorPivot descriptorPivotBizObj, Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup constructedValueObject, IValueObjectExportContext context)
		{
			throw new NotSupportedException("Exporting GL Mapping is currently not supported");
		}

		#endregion

		#region Import

		protected override void ImportFromValueObjectCore(GLDescriptorPivot newGLDescriptorPivot, Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup value, IValueObjectImportContext context)
		{
			if (value != null)
			{
				ProcessReportSetup(newGLDescriptorPivot, value, context);
			}
		}

		void ProcessReportSetup(GLDescriptorPivot newGLDescriptorPivot, Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup value, IValueObjectImportContext context)
		{
			context.SetPropertyInfoValue(newGLDescriptorPivot.ReportTypeInfo, value.ReportType, value.ReportTypeSpecified);
			context.SetPropertyInfoValue(newGLDescriptorPivot.ReportCategoryInfo, value.ReportCategory, value.ReportCategorySpecified);
			AddErrorsToNotifications(newGLDescriptorPivot, value, context);
		}

		void AddErrorsToNotifications(GLDescriptorPivot descriptorPivot, Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup value, IValueObjectImportContext context)
		{
			foreach (string errorString in descriptorPivot.Notifications.GetErrors().GetUniqueMessageList())
			{
				context.Notify(new ErrorNotification(ErrorType.Error, GetErrorMessageFormatted(value, errorString)));
			}

			foreach (string warningString in descriptorPivot.Notifications.GetWarnings().GetUniqueMessageList())
			{
				context.Notify(new ErrorNotification(ErrorType.Warning, GetErrorMessageFormatted(value, warningString)));
			}
		}

		string GetErrorMessageFormatted(Xsd.GLHeaderMultiLanguageReportSetupsGLHeaderMultiLanguageReportSetup value, string message)
		{
			return Res.GetString("6f82ee71-e7ba-4872-9ba9-4bc0ca0678ba", "GL Report setup {0} - {1}", value.LocalAccountNumber, message);
		}

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject bizObj)
		{ }

		#endregion
	}
}