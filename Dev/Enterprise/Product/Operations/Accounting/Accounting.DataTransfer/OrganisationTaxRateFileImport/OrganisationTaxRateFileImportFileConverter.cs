using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Xml;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer
{
	class OrganisationTaxRateFileImportFileConverter : FlatFileConverter
	{
		public OrganisationTaxRateFileImportFileConverter(ZString? rateSource, INotifications notification, BusinessObjectFactory factory)
			: base(notification, factory)
		{
			RateSource = rateSource ?? ZString.Empty;
		}

		ZString RateSource { get; }

		protected override void CheckArguments(IValueObject valueObject, IFlatFileFormat flatFileFormat)
		{
			base.CheckArguments(valueObject, flatFileFormat);

			if (!(valueObject is OrganisationTaxRateFileImportXSD))
			{
				throw new ArgumentException("Not supported type of ValueObject", nameof(valueObject));
			}

			if (!(flatFileFormat is OrganisationTaxRateFileImportFileFormat))
			{
				throw new ArgumentException("Not supported format type", nameof(flatFileFormat));
			}
		}

		protected override void MapImport(IValueObject valueObject, FlatFileDataRowCollection fileLines)
		{
			var collection = (OrganisationTaxRateFileImportXSD)valueObject;

			foreach (OrganisationTaxRateFileImportDataRow dataRow in fileLines)
			{
				foreach (var (organisationCode, organisationName, accOrgTaxConfigurationPK) in dataRow.OrganisationDataCollection)
				{
					var orgTaxRateFileImportLine = new OrganisationTaxRateFileImportLine(Factory);
					using (orgTaxRateFileImportLine.GetValidationSuspender())
					{
						orgTaxRateFileImportLine.OrganizationCode = organisationCode;
						orgTaxRateFileImportLine.OrganizationName = organisationName;
						orgTaxRateFileImportLine.TaxConfigurationPK = accOrgTaxConfigurationPK;

						orgTaxRateFileImportLine.RegistrationCode = dataRow.RegistrationCode;
						orgTaxRateFileImportLine.StartDate = dataRow.StartDate;
						orgTaxRateFileImportLine.EndDate = dataRow.EndDate;

						orgTaxRateFileImportLine.RateSource = RateSource;

						(ZInt numerator, ZInt denominator) = GetTaxRateAsFraction(dataRow.Rate);
						orgTaxRateFileImportLine.RateNumerator = numerator;
						orgTaxRateFileImportLine.RateDenominator = denominator;
					}

					collection.Add(orgTaxRateFileImportLine);
				}
			}
		}

		static (ZInt numerator, ZInt denominator) GetTaxRateAsFraction(ZDecimal rateInPercentage)
		{
			ZInt numerator = (ZInt)rateInPercentage.Round(0);
			ZInt denominator = 1;

			if (numerator != rateInPercentage)
			{
				var factor = (ZInt)Math.Pow(10, rateInPercentage.DecimalPlaces);
				numerator = (ZInt)(rateInPercentage * factor);
				denominator = factor;
			}

			return (numerator, denominator);
		}
	}
}
