using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Billing.Integration;

namespace Enterprise.Client.JAS.Business.JXC.Import
{
	public class JXCDataImporterBizO : NonPersistentBusinessObject
	{
		[MaxLength(250)]
		public ZString ImportFilePath
		{
			get { return fImportFilePath; }
			set
			{
				if (fImportFilePath != value)
				{
					CheckMaximumLength(ImportFilePathInfo, value);
					SetNonPersistentPropertyValue(ImportFilePathInfo, ref fImportFilePath, value);
					Validation.ValidateImportFilePath();
				}
			}
		}

		public ZPropertyInfo ImportFilePathInfo
		{
			get { return GetZPropertyInfo(nameof(ImportFilePath)); }
		}

		public ZString ImportSummary
		{
			get { return ImportSummaryBuilder.ToStringWithNewLineBetweenAppends(); }
		}

		public ZPropertyInfo ImportSummaryInfo
		{
			get { return GetZPropertyInfo(nameof(ImportSummary)); }
		}

		public bool Import(INotifications notifications)
		{
			JXCMessageImporter importer = new JXCMessageImporter();
			return importer.ImportData(ImportFilePath, notifications, new SourceInfo(BillingDataSource.InterfaceConnector, BillingInterfaceName.ClientSpecifiedImport, ZGuid.Empty, ZGuid.Empty, ZString.Empty, ImportFilePath));
		}

		public void AppendImportSummary(string summaryText)
		{
			ImportSummaryBuilder.Append(summaryText);
			ImportSummaryInfo.RefreshBinding();
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		public JXCDataImporterBizOValidation Validation
		{
			get { return new JXCDataImporterBizOValidation(this); }
		}

		ZStringBuilder ImportSummaryBuilder
		{
			get
			{
				if (fImportSummaryBuilder == null)
				{
					fImportSummaryBuilder = new ZStringBuilder();
				}
				return fImportSummaryBuilder;
			}
		}

		ZStringBuilder fImportSummaryBuilder;
		ZString fImportFilePath;
	}
}
