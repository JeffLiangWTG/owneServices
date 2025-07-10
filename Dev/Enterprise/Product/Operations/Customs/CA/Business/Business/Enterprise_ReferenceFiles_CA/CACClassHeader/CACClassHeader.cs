using System;
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
	public class CACClassHeader : AutoCACClassHeader
	{
		public CACClassHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Loader

		public static CACClassHeader Load(BusinessObjectFactory factory, ZDateTime effectiveDate, ZString classificationNumber)
		{
			return Load(factory, effectiveDate, classificationNumber, false);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
		public static CACClassHeader Load(BusinessObjectFactory factory, ZDateTime effectiveDate, ZString classificationNumber, bool loadInactive)
		{
			return factory.GetCachedValue(string.Format("CACClassHeader_{0}_{1}_{2}", effectiveDate.ToShortDateString(), classificationNumber, loadInactive) , () =>
			{
				var query = new ZQuery(CACClassHeaderSchema.ZA_ClassificationNumber, classificationNumber);
				if (!effectiveDate.IsValid)
				{
					string message = "You should input a valid effectiveDate";
					ExceptionReporter.Instance.ReportDeveloperException(message, message,
						new Exception(message + System.Environment.NewLine + (new System.Diagnostics.StackTrace().ToString())));
				}
				else
				{
					query.AddToFilter(CACClassHeaderSchema.ZA_EffectiveDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, effectiveDate);
					query.AddToFilter(CACClassHeaderSchema.ZA_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, effectiveDate);
				}

				if (!loadInactive)
				{
					query.AddToFilter(CACClassHeaderSchema.ZA_InactiveInd, SQLComparisonOperator.NotEqual, "Y");
				}
				query.OrderBy = CACClassHeaderSchema.ZA_EffectiveDate.Name + " DESC";
				return factory.LoadTop1<CACClassHeader>(query);
			});
		}

		#endregion

		#region ClassRates

		[ChildEditable(true)]
		public CACRateHeaderCollection ClassRates
		{
			get
			{
				if (classRates == null)
				{
					classRates = new CACRateHeaderCollection(this, CACRateHeader.RateType.ClassificationRate);
					classRates.Load();
					RegisterEditableChildObject(classRates);
				}
				return classRates;
			}
		}

		CACRateHeaderCollection classRates;

		#endregion

		#region ExciseDutyRates

		[ChildEditable(true)]
		public CACRateHeaderCollection ExciseDutyRates
		{
			get
			{
				if (exciseDutyRates == null)
				{
					exciseDutyRates = new CACRateHeaderCollection(this, CACRateHeader.RateType.ExciseDutyRate);
					exciseDutyRates.Load();
					RegisterEditableChildObject(exciseDutyRates);
				}
				return exciseDutyRates;
			}
		}

		CACRateHeaderCollection exciseDutyRates;

		#endregion

		#region RefNumbers

		[ChildEditable(true)]
		public CACTaxRefNumHeaderCollection RefNumbers
		{
			get
			{
				if (refNumbers == null)
				{
					refNumbers = new CACTaxRefNumHeaderCollection(this);
					refNumbers.Load();
					RegisterEditableChildObject(refNumbers);
				}
				return refNumbers;
			}
		}

		CACTaxRefNumHeaderCollection refNumbers;

		#endregion

		#region Delete

		public override void Delete()
		{
			ClassRates.RemoveAndDeleteAll();
			ExciseDutyRates.RemoveAndDeleteAll();
			RefNumbers.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion
	}
}
