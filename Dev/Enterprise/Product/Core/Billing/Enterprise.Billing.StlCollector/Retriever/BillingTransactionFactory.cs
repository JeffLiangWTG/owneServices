using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using Enterprise.Billing.Integration;
using Enterprise.Integration.Billing;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Billing.StlCollector.Retriever
{
	public class BillingTransactionFactory : IStlTransactionFactory
	{
		public IBillingTransaction CreateTransaction(string priceItemCode, int billableCount, DateTime serviceOccuredUtc, string reference1, string companyCode = null, string reference2 = null, string reference3 = null, string reference4 = null, string reference5 = null, string branch = null, string clientStaffCode = null, string additionalRefs = null)
		{
			if (billableCount <= 0)
			{
				return null;
			}

			return new BillingTransaction
			{
				PriceItemCode = priceItemCode,
				BillableCount = billableCount,
				ServiceOccuredUTC = serviceOccuredUtc,
				Reference1 = reference1,
				Reference2 = reference2,
				Reference3 = reference3,
				Reference4 = reference4,
				Reference5 = reference5,
				Branch = branch,
				ClientStaffCode = clientStaffCode,
				AdditionalRefs = additionalRefs,
				ClientID = GetClientId(companyCode),
				ClientNumber = GetClientNumber(companyCode),
				ReportingSource = BillingManager.ReportingSource,
				Category = "STL"
			};
		}

		public BillingTransactionFactory() : this(null)
		{
		}

		public BillingTransactionFactory(string systemId)
		{
			systemIdOverride = systemId;
		}

		readonly Lazy<string> DefaultSystemId = new Lazy<string>(() => ObjectFactory.Get<IProductRegistration>().Key.SystemId);
		readonly List<Tuple<string, Regex>> propertyRegExs = new List<Tuple<string, Regex>>();
		readonly string systemIdOverride;

		string SystemId => systemIdOverride ?? DefaultSystemId.Value;

		readonly Lazy<GlbCompany> CurrentCompany = new Lazy<GlbCompany>(() =>
		{
			var currentCompany = GlbCompany.CurrentCompany;
			if (currentCompany == null)
			{
				throw new BillingException("No current company context available.");
			}
			else if (string.IsNullOrWhiteSpace(currentCompany.LicenceKeyIdentifier))
			{
				throw new BillingException(string.Format(CultureInfo.CurrentCulture, "Current company [{0} - {1}] does not have a valid licence key.", currentCompany.GC_Code, currentCompany.GC_Name));
			}
			return currentCompany;
		});

		string GetClientId(string transactionCompanyCode = null)
		{
			var transactionCompany = (transactionCompanyCode != null) ? CurrentCompany.Value.Factory.LoadFromNaturalKey<GlbCompany>(GlbCompanySchema.GC_Code, transactionCompanyCode) : null;
			var actualCompany = (transactionCompany == null || transactionCompany.IsDemoCompany || string.IsNullOrWhiteSpace(transactionCompany.LicenceKeyIdentifier)) ? CurrentCompany.Value : transactionCompany;
			return actualCompany.LicenceKeyIdentifier;
		}

		string GetClientNumber(string transactionCompanyCode = null)
		{
			return SystemId + (string.IsNullOrWhiteSpace(transactionCompanyCode) ? "" : "." + transactionCompanyCode);
		}

		public IStlTransaction CreateTransaction(IStlScript script, DataRow dataRow)
		{ 
			var branchCode = dataRow["BranchCode"].ToString();
			var ref1 = ReplaceUnsupportedControlCharsWithSpaces("Reference1", dataRow["TransactionReference01"].ToString());
			var ref2Obj = ReplaceUnsupportedControlCharsWithSpaces("Reference2", dataRow["TransactionReference02"].ToString());
			var ref3Obj = ReplaceUnsupportedControlCharsWithSpaces("Reference3", dataRow["TransactionReference03"].ToString());
			var ref4Obj = ReplaceUnsupportedControlCharsWithSpaces("Reference4", dataRow["TransactionReference04"].ToString());
			var encoding = Encoding.GetEncoding(script.AdditionalRefsEncoding);
			var additionalRefsObj = encoding.GetString((byte[])dataRow["AdditionalRefs"]);

			return CreateTransaction(
				script.Code,
				(int)dataRow["ItemCount"],
				(script.DateType == StlDateType.DateTimeOffset) ? ((DateTimeOffset)dataRow["TransactionDateUtc"]).UtcDateTime : (DateTime)dataRow["TransactionDateUtc"],
				string.IsNullOrWhiteSpace(ref1) ? "[BLANK]" : ref1,
				dataRow["CompanyCode"].ToString(),
				string.IsNullOrWhiteSpace(ref2Obj) ? null : ref2Obj,
				string.IsNullOrWhiteSpace(ref3Obj) ? null : ref3Obj,
				string.IsNullOrWhiteSpace(ref4Obj) ? null : ref4Obj,
				ReplaceUnsupportedControlCharsWithSpaces("Reference5", dataRow["TransactionGuidReference"].ToString()),
				string.IsNullOrWhiteSpace(branchCode) ? null : branchCode,
				dataRow["UserCode"].ToString(),
				string.IsNullOrWhiteSpace(additionalRefsObj) ? null : additionalRefsObj);
		}

		string ReplaceUnsupportedControlCharsWithSpaces(string propertyName, string propertyValue)
		{
			var propertyValueAsChars = propertyValue != null ? propertyValue.ToCharArray() : [];
			if (!propertyValueAsChars.Any(char.IsControl))
			{
				return propertyValue;
			}

			var propertyPair = propertyRegExs.SingleOrDefault(vp => vp.Item1 == propertyName);
			if (propertyPair == null)
			{
				var regularExpressionAttribute = typeof(CargoWise.Billing.API.BillingTransaction).GetProperty(propertyName).GetCustomAttributes(true).Select(a => a as RegularExpressionAttribute).Single((a) => a != null);
				propertyPair = new Tuple<string, Regex>(propertyName, new Regex(regularExpressionAttribute.Pattern));
				propertyRegExs.Add(propertyPair);
			}

			return new string(propertyValueAsChars.Select(c => propertyPair.Item2.IsMatch(c.ToString()) ? c : ' ').ToArray());
		}
	}
}
