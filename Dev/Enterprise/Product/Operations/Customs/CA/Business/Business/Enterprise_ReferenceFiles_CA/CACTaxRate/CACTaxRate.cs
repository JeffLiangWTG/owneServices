using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	[CodeProperty(CACTaxRateSchema.Constants.ZH_TaxRefNumber), DescriptionProperty(CACTaxRateSchema.Constants.ZH_Title)]
	public class CACTaxRate : AutoCACTaxRate
	{
		public static class TaxType
		{
			public const string GST = "GST";
			public const string Excise = "EXS";
		}

		public CACTaxRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Load

		internal static CACTaxRate[] Load(BusinessObjectFactory factory, IEnumerable<ZString> refNumbers, ZString taxType, ZDateTime effectiveDate)
		{
			var query = AddEffectiveDateFilter(new ZQuery(CACTaxRateSchema.ZH_TaxRefNumber, refNumbers), taxType, effectiveDate);
			query.OrderBy = CACTaxRateSchema.ZH_EffectiveDate.Name + " DESC";
			return factory.Load<CACTaxRate>(query);
		}

		internal static CACTaxRate Load(BusinessObjectFactory factory, ZString refNumber, ZString taxType, ZDateTime effectiveDate)
		{
			var query = AddEffectiveDateFilter(new ZQuery(CACTaxRateSchema.ZH_TaxRefNumber, refNumber), taxType, effectiveDate);
			query.OrderBy = CACTaxRateSchema.ZH_EffectiveDate.Name + " DESC";
			return factory.LoadTop1<CACTaxRate>(query);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		internal static ZQuery AddEffectiveDateFilter(ZQuery query, ZString taxType, ZDateTime effectiveDate)
		{
			AddTaxTypeFilter(query, taxType);
			if (!effectiveDate.IsValid)
			{
				effectiveDate = ZDateTime.Today;
				string message = "You should input a valid effectiveDate";
				ExceptionReporter.Instance.ReportDeveloperException(message, message, new Exception(message + System.Environment.NewLine + (new System.Diagnostics.StackTrace().ToString())));
			}

			query.AddToFilter(CACTaxRateSchema.ZH_EffectiveDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
			query.AddToFilter(CACTaxRateSchema.ZH_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			query.AddToFilter(CACTaxRateSchema.ZH_Inactive, SQLComparisonOperator.NotEqual, "Y");
			return query;
		}

		internal static ZQuery AddTaxTypeFilter(ZQuery query, ZString taxType)
		{
			query.AddToFilter(CACTaxRateSchema.ZH_TaxType, taxType);
			return query;
		}

		#endregion
	}
}
