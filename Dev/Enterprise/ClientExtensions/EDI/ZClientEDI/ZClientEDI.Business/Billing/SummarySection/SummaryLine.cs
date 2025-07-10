using System;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class SummaryLine : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SummaryLine(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Columns

		public ZString Column1 { get; set; }
		public ZString Column2 { get; set; }
		public ZString Column3 { get; set; }
		public ZString Column4 { get; set; }
		public ZString Column5 { get; set; }
		public ZString Column6 { get; set; }
		public ZString Column7 { get; set; }
		public ZString Column8 { get; set; }
		public ZString Column9 { get; set; }
		public ZString Column10 { get; set; }

		#endregion

		#region Columns with meaningful names for usage report

		public ZString MainDescription
		{
			get { return Column1; }
			set { Column1 = value; }
		}

		public ZString AdditionalDescription
		{
			get { return Column2; }
			set { Column2 = value; }
		}

		public ZString UnitCount
		{
			get { return Column3; }
			set { Column3 = value; }
		}

		public ZString LicenceUnits
		{
			get { return Column4; }
			set { Column4 = value; }
		}

		public ZString LicenceUnitsAmount
		{
			get { return Column5; }
			set { Column5 = value; }
		}

		public ZString UnitPrice
		{
			get { return Column6; }
			set { Column6 = value; }
		}

		public ZString Amount
		{
			get { return Column7; }
			set { Column7 = value; }
		}

		public ZString PurchasedCount
		{
			get { return Column8; }
			set { Column8 = value; }
		}

		public ZString TotalUnitCount
		{
			get { return Column9; }
			set { Column9 = value; }
		}

		public ZString AmountDescription { get; set; }
		public ZString TaxCode { get; set; }
		public ZString LicenceUnitsAmountDescription { get; set; }

		public ZString Currency
		{
			get { return Column6; }
			set { Column6 = value; }
		}

		public ZString Amount1
		{
			get { return Column7; }
			set { Column7 = value; }
		}

		public ZString Amount2
		{
			get { return Column8; }
			set { Column8 = value; }
		}

		public ZString FinalAmount
		{
			get { return Column9; }
			set { Column9 = value; }
		}

		#endregion

		public SummaryLine Header { get; set; }

		#region Header properties

		public ZString TopLevelDescription { get; set; }

		public ZString TotalDescription { get; set; }
		public ZString TotalAmount { get; set; }
		public ZString TotalLicenceUnitsDescription { get; set; }
		public ZString TotalLicenceUnits { get; set; }

		public ZString AdjustmentDescription { get; set; }
		public ZString TotalAmountAdjustment { get; set; }
		public ZString TotalLicenceUnitsAdjustment { get; set; }
		public ZString TotalAmountAfterAdjustment { get; set; }
		public ZString TotalLicenceUnitsAfterAdjustment { get; set; }
		public ZString SortValue { get; set; }
		public ZString ClientCompanyDescription { get; set; }

		public ZString Code
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14}", SortValue, Column1, Column2, Column8, Column3, Column9, Column4, Column5, Column6, Column7, TopLevelDescription, TotalLicenceUnitsDescription, TotalLicenceUnits, TotalDescription, TotalAmount); }
		}

		public ZString UnsortedCode
		{
			get { return string.Format(CultureInfo.InvariantCulture, "{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", Column1, Column2, Column8, Column3, Column9, Column4, Column5, Column6, Column7, TopLevelDescription, TotalLicenceUnitsDescription, TotalLicenceUnits, TotalDescription, TotalAmount); }
		}

		public ZString CodeColumns1To10 => FormattableString.Invariant($"{Column1},{Column2},{Column3},{Column4},{Column5},{Column6},{Column7},{Column8},{Column9},{Column10}");

		/// <summary>
		/// Some header labels are optional, and not always set.
		/// When lines are combined under the one header, need to set any such labels in the result.
		/// Any header labels that are blank in this line, but not-blank in the given line are copied from the given line.,
		/// </summary>
		public void CopyMissingHeaderLabelsFrom(SummaryLine other)
		{
			if (MainDescription.IsEmpty && !other.MainDescription.IsEmpty)
			{
				MainDescription = other.MainDescription;
			}
			if (AdditionalDescription.IsEmpty && !other.AdditionalDescription.IsEmpty)
			{
				AdditionalDescription = other.AdditionalDescription;
			}
			if (PurchasedCount.IsEmpty && !other.PurchasedCount.IsEmpty)
			{
				PurchasedCount = other.PurchasedCount;
			}
			if (UnitCount.IsEmpty && !other.UnitCount.IsEmpty)
			{
				UnitCount = other.UnitCount;
			}
			if (TotalUnitCount.IsEmpty && !other.TotalUnitCount.IsEmpty)
			{
				TotalUnitCount = other.TotalUnitCount;
			}
			if (UnitPrice.IsEmpty && !other.UnitPrice.IsEmpty)
			{
				UnitPrice = other.UnitPrice;
			}
			if (Amount.IsEmpty && !other.Amount.IsEmpty)
			{
				Amount = other.Amount;
			}
		}

		#endregion
	}
}

