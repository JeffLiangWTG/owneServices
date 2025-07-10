using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.GenericTransaction
{
	[TestedAsNonPersistentBusinessObject]
	public class GenericTransaction : AutoGenericTransaction
	{
		public GenericTransaction(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Property override

		[DecimalPlaces(nameof(Decimals))]
		public override ZDecimal VT_Amount
		{
			get { return base.VT_Amount; }
			set { base.VT_Amount = value; }
		}

		[DecimalPlaces(nameof(Decimals))]
		public override ZDecimal VT_GST
		{
			get { return base.VT_GST; }
			set { base.VT_GST = value; }
		}

		[DecimalPlaces(nameof(Decimals))]
		public override ZDecimal VT_Total
		{
			get { return base.VT_Total; }
			set { base.VT_Total = value; }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public override ZDecimal VT_OSTotal
		{
			get { return base.VT_OSTotal; }
			set { base.VT_OSTotal = value; }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public override ZDecimal VT_OSAmount
		{
			get { return base.VT_OSAmount; }
			set { base.VT_OSAmount = value; }
		}

		[DecimalPlaces(nameof(CurrencyDecimals))]
		public override ZDecimal VT_OSGST
		{
			get { return base.VT_OSGST; }
			set { base.VT_OSGST = value; }
		}

		[DecimalPlaces(nameof(ExchangeRateDecimals))]
		public override ZDecimal VT_ExchangeRate
		{
			get => base.VT_ExchangeRate;
			set => base.VT_ExchangeRate = value;
		}

		#endregion

		#region Currency Decimals

		public int CurrencyDecimals => Currency != null ? Currency.Decimals : Decimals;

		public int Decimals => GlbCompany.CurrentCompany.GetLocalDecimals();

		public int ExchangeRateDecimals => GlbCompany.CurrentCompany.ExchangeRateDecimalPlaces;

		#endregion

		#region Business Object Overrides

		public override void Delete()
		{
			ErrorReporter.ReportOnce(typeof(GenericTransaction).FullName + " cannot delete", "Cannot delete accounting object " + nameof(GenericTransaction));
		}

		#endregion

		#region Property Overrides

		[List("Lookups.Organisations")]
		public override ZGuid VT_OH
		{
			get { return base.VT_OH; }
			set { base.VT_OH = value; }
		}

		[List("Lookups.Jobs")]
		public override ZGuid VT_JH
		{
			get { return base.VT_JH; }
			set { base.VT_JH = value; }
		}

		#endregion
	}
}
