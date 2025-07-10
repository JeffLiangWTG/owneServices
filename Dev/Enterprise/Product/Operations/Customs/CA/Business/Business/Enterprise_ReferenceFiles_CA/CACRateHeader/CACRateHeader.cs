using System;
using System.Data;
using System.Linq;
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
	public class CACRateHeader : AutoCACRateHeader
	{
		#region Schema

		public static class RateType
		{
			public const string ClassificationRate = "CLS";
			public const string ExciseDutyRate = "EXS";
		}

		#endregion

		public CACRateHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		public static CACRateHeader Load(CACClassHeader master, ZDateTime effectiveDate, string type, bool fetchOnlyFromLocalCache = false)
		{
			return LoadEffectiveRateHeaders(master, effectiveDate,type, fetchOnlyFromLocalCache).FirstOrDefault();
		}

		public static CACRateHeader[] LoadEffectiveRateHeaders(CACClassHeader master, ZDateTime effectiveDate, string type, bool fetchOnlyFromLocalCache = false)
		{
			var query = new ZQuery();

			if (!effectiveDate.IsValid)
			{
				string message = "You should input a valid effectiveDate";
				ExceptionReporter.Instance.ReportDeveloperException(message, message,
					new Exception(message + System.Environment.NewLine + (new System.Diagnostics.StackTrace().ToString())));
			}
			else
			{
				query.AddToFilter(CACRateHeaderSchema.ZB_EffectiveDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
				query.AddToFilter(CACRateHeaderSchema.ZB_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
			}

			query.AddToFilter(CACRateHeaderSchema.ZB_Inactive, SQLComparisonOperator.NotEqual, "Y");
			query.AddToFilter(CACRateHeaderSchema.ZB_ZA_ClassHeader, master.PK);
			query.AddToFilter(CACRateHeaderSchema.ZB_RateType, type);
			query.OrderBy = CACRateHeaderSchema.ZB_EffectiveDate.Name + " DESC";
			query.FetchOnlyFromLocalCache = fetchOnlyFromLocalCache;
			return master.Factory.Load<CACRateHeader>(query);
		}

		#endregion

		#region ZB_ZA_ClassHeader

		[RelatedBusinessObject("ClassHeader")]
		public override ZGuid ZB_ZA_ClassHeader
		{
			get { return base.ZB_ZA_ClassHeader; }
			set { base.ZB_ZA_ClassHeader = value; }
		}

		public CACClassHeader ClassHeader
		{
			get { return Factory.Load<CACClassHeader>(ZB_ZA_ClassHeader); }
		}

		#endregion

		#region Rates

		[ChildEditable(true)]
		public CACRateCollection Rates
		{
			get
			{
				if (rates == null)
				{
					rates = new CACRateCollection(this);
					rates.Load();
					RegisterEditableChildObject(rates);
				}
				return rates;
			}
		}

		CACRateCollection rates;

		#endregion

		#region Delete

		public override void Delete()
		{
			Rates.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion
	}
}
