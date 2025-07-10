using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Client.JAS.Business.Cognos
{
	public class CognosLineBizO : DynamicBusinessObject, ICognosLine, IObsoleteValidation
	{
		#region Schema

		public static class Schema
		{
			public const string AccountPK = "AccountPK";
			public const string AccountName = "AccountName";
			public const string AccountCode = "AccountCode";
			public const string CounterCompany = "CounterCompany";
			public const string Mode = "Mode";
			public const string Branch = "Branch";
			public const string Business = "Business";
			public const string Amount = "Amount";
			public const string TransactionCurrency = "TransactionCurrency";
			public const string TransactionAmount = "TransactionAmount";
			public const string Geographical = "Geographical";
		}

		#endregion

		public CognosLineBizO(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public virtual ZGuid AccountPK
		{
			get { return new ZGuid(BusinessObjectInternalsRow[Schema.AccountPK]); }
		}

		public virtual ZString AccountName
		{
			get { return new ZString(BusinessObjectInternalsRow[Schema.AccountName]); }
		}

		public virtual ZString AccountCode
		{
			get { return new ZString(BusinessObjectInternalsRow[Schema.AccountCode]); }
		}

		public virtual ZString CounterCompany
		{
			get { return new ZString(BusinessObjectInternalsRow[Schema.CounterCompany]); }
		}

		public virtual ZString Mode
		{
			get { return new ZString(BusinessObjectInternalsRow[Schema.Mode]); }
		}

		public virtual ZString Branch
		{
			get { return new ZString(BusinessObjectInternalsRow[Schema.Branch]); }
		}

		public virtual ZString Business
		{
			get { return new ZString(BusinessObjectInternalsRow[Schema.Business]); }
		}

		public virtual ZDecimal Amount
		{
			get { return new ZDecimal(BusinessObjectInternalsRow[Schema.Amount]); }
		}

		public virtual ZString TransactionCurrency
		{
			get { return new ZString(BusinessObjectInternalsRow[Schema.TransactionCurrency]); }
		}

		public virtual ZDecimal TransactionAmount
		{
			get { return new ZDecimal(BusinessObjectInternalsRow[Schema.TransactionAmount]); }
		}

		public virtual ZString Geographical
		{
			get { return new ZString(BusinessObjectInternalsRow[Schema.Geographical]); }
		}

		public override void Delete()
		{
			ErrorReporter.ReportOnce(GetType().ToString() + "Delete", "Delete is not supported");
		}

		DataRow BusinessObjectInternalsRow
		{
			get { return ((IBusinessObjectInternals)this).Row; }
		}
	}
}
