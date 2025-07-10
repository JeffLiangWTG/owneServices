using System.Collections.Generic;
using System.IO;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Billing.Integration;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer
{
	class OrganisationTaxRateFileImportFileDataImporter : FlatFileDataImporter, IOrganisationTaxRateFileImportFileDataImporter
	{
		public OrganisationTaxRateFileImportFileDataImporter()
		{
			organisationTaxRateImportDataProvider = ObjectFactory.Get<ITaxFrameworkDependencyFactory>().GetOrganisationTaxRateImportDataProvider();
		}

		IOrganisationTaxRateImportDataProvider organisationTaxRateImportDataProvider { get; }

		Dictionary<ZString, IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)>> RegNumbersToImport;
		OrganisationTaxRateFileImport OrganisationTaxRateFileImportBusinessEntity;
		IOrgTaxRateImportFileFormat CurrentFormat;

		void IOrganisationTaxRateFileImportFileDataImporter.ImportData(string filename, INotifications notifications, OrganisationTaxRateFileImport bizo)
		{
			try
			{
				OrganisationTaxRateFileImportBusinessEntity = bizo;

				var taxAuthorityCode = bizo.TaxConfigurationObject?.ETC_TaxAuthorityCode ?? ZString.Empty;

				if (taxAuthorityCode.IsEmpty)
				{
					notifications.AddError(Res.GetString("20fec1df-8e51-4aa8-aa6c-fa23dfa4767c", "Tax Authority Code as part of the Tax Configuration cannot be null."));
					return;
				}

				var accountingCountryFactory = (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(bizo.TaxConfigurationObject.ETC_RN_NKCountry) as IInstanceProvider<IOrgTaxRateImportFileFormatProvider>)?.Get();
				CurrentFormat = accountingCountryFactory?.GetFileFormat(taxAuthorityCode);

				if (CurrentFormat is null)
				{
					notifications.AddError($"Tax Rate File Format does not exist for country '{bizo.TaxConfigurationObject?.ETC_RN_NKCountry ?? ZString.Empty}', tax authority '{taxAuthorityCode}'.");
					return;
				}

				RegNumbersToImport = organisationTaxRateImportDataProvider.RegNumbersToImport(bizo.Factory, bizo.TaxConfiguration);

				var reader = GetReader(filename);
				ImportDataToFactory(reader, filename, notifications, SourceInfo.EmptySourceInfo, out var transactionActions);
			}
			catch (IOException ex)
			{
				notifications.AddError(ex.Message);
			}
			catch (System.UnauthorizedAccessException ex)
			{
				notifications.AddError(ex.Message);
			}
		}

		int BufferSize => 10 * 1024 * 1024;
		TextReader GetReader(string attachmentFileName) => new StreamReader(attachmentFileName, System.Text.Encoding.UTF8, false, BufferSize);

		protected override IFlatFileFormat FlatFileFormat => new OrganisationTaxRateFileImportFileFormat(CurrentFormat, RegNumbersToImport);

		protected override IFlatFileConverter CreateConverter(INotifications notificationSubscriber) => new OrganisationTaxRateFileImportFileConverter(OrganisationTaxRateFileImportBusinessEntity?.RateSource, notificationSubscriber, OrganisationTaxRateFileImportBusinessEntity?.Factory);

		protected override IValueObject CreateXsd() => new OrganisationTaxRateFileImportXSD();

		protected override bool ExtractToDataAdapter(IValueObject xsd, INotifications notifications)
		{
			if (OrganisationTaxRateFileImportBusinessEntity != null)
			{
				var valueObject = (OrganisationTaxRateFileImportXSD)xsd;
				OrganisationTaxRateFileImportBusinessEntity.ImportLines.RemoveAll();
				OrganisationTaxRateFileImportBusinessEntity.ImportLines.AddRange(valueObject);
				return true;
			}
			return false;
		}
	}
}
