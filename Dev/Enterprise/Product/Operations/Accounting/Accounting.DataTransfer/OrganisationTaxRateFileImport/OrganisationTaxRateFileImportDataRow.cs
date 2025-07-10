using System.Collections.Generic;
using System.Globalization;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;

namespace Enterprise.Accounting.DataTransfer
{
	class OrganisationTaxRateFileImportDataRow : FlatFileDataRow
	{
		public OrganisationTaxRateFileImportDataRow()
			: base(3)
		{ }

		public IReadOnlyList<(ZString OrgCode, ZString OrgName, ZGuid AccOrgTaxConfigurationPK)> OrganisationDataCollection { get; set; }

		public ZString RegistrationCode { get; set; }

		public ZDate StartDate
		{
			get { return GetFieldAsZDateTime(Schema.StartDate, "ddMMyyyy").Date; }
		}

		public ZDate EndDate
		{
			get { return GetFieldAsZDateTime(Schema.EndDate, "ddMMyyyy").Date; }
		}

		public ZDecimal Rate
		{
			get { return GetFieldAsZDecimal(Schema.Rate, new CultureInfo("es-AR")); }
		}

		internal class Schema
		{
			public const int StartDate = 0;
			public const int EndDate = 1;
			public const int Rate = 2;
		}
	}
}
