using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Enterprise.Integration.Billing;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Billing.Integration
{
	[XmlSerializerAssembly("Enterprise.Billing.Integration.XmlSerializers")]
	partial class BillingTransaction : IEquatable<BillingTransaction>, IBillingTransaction
	{
		[XmlIgnore]
		public string CompanyCode { get => ClientID.Substring(3, 3); set => ClientID = ClientID.Substring(0, 3) + value + ClientID.Substring(6, 3); }

		public bool Equals(BillingTransaction other)
		{
			return BillingTransactionComparer.Equals(this, other);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as BillingTransaction);
		}

		public override int GetHashCode()
		{
			return BillingTransactionComparer.GetHashCode(this);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is string representation of the billing transaction, used in error reports")]
		public override string ToString()
		{
			return string.Format("BillableCount: {0}, Branch: {1}, Category: {2}, ClientID: {3}, ClientNumber: {4}, ClientStaffCode: {5}, PriceItemCode: {6}, Reference1: {7}, Reference2: {8}, Reference3: {9}, Reference4: {10}, Reference5: {11}, ReportingSource: {12}, ServiceOccuredUTC: {13}, Version: {14}, MessageTrackingID: {15}, AdditionalRefs: {16}", BillableCount, Branch, Category, ClientID, ClientNumber, ClientStaffCode, PriceItemCode, Reference1, Reference2, Reference3, Reference4, Reference5, ReportingSource, ServiceOccuredUTC, Version, MessageTrackingID, AdditionalRefs);
		}

		public static IEqualityComparer<BillingTransaction> BillingTransactionComparer
		{
			get { return BillingTransactionComparerInstance; }
		}

		[Immutable]
		sealed class BillingTransactionEqualityComparer : IEqualityComparer<BillingTransaction>
		{
			public bool Equals(BillingTransaction x, BillingTransaction y)
			{
				if (ReferenceEquals(x, y))
				{
					return true;
				}

				if (ReferenceEquals(x, null))
				{
					return false;
				}

				if (ReferenceEquals(y, null))
				{
					return false;
				}

				if (x.GetType() != y.GetType())
				{
					return false;
				}

				return
					x.billableCountField == y.billableCountField &&
					string.Equals(x.branchField, y.branchField) &&
					string.Equals(x.categoryField, y.categoryField) &&
					string.Equals(x.clientIDField, y.clientIDField) &&
					string.Equals(x.clientNumberField, y.clientNumberField) &&
					string.Equals(x.clientStaffCodeField, y.clientStaffCodeField) &&
					string.Equals(x.priceItemCodeField, y.priceItemCodeField) &&
					string.Equals(x.reference1Field, y.reference1Field) &&
					string.Equals(x.reference2Field, y.reference2Field) &&
					string.Equals(x.reference3Field, y.reference3Field) &&
					string.Equals(x.reference4Field, y.reference4Field) &&
					string.Equals(x.reference5Field, y.reference5Field) &&
					string.Equals(x.reportingSourceField, y.reportingSourceField) &&
					x.serviceOccuredUTCField.Equals(y.serviceOccuredUTCField) &&
					x.versionField == y.versionField &&
					string.Equals(x.messageTrackingIDField, y.messageTrackingIDField) &&
					string.Equals(x.additionalRefsField, y.additionalRefsField);
			}

			public int GetHashCode(BillingTransaction obj)
			{
				unchecked
				{
					var hashCode = obj.billableCountField;
					hashCode = (hashCode * 397) ^ (obj.branchField != null ? obj.branchField.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.categoryField != null ? obj.categoryField.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.clientIDField != null ? obj.clientIDField.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.clientNumberField != null ? obj.clientNumberField.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.clientStaffCodeField != null ? obj.clientStaffCodeField.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.priceItemCodeField != null ? obj.priceItemCodeField.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.reference1Field != null ? obj.reference1Field.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.reference2Field != null ? obj.reference2Field.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.reference3Field != null ? obj.reference3Field.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.reference4Field != null ? obj.reference4Field.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.reference5Field != null ? obj.reference5Field.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.reportingSourceField != null ? obj.reportingSourceField.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ obj.serviceOccuredUTCField.GetHashCode();
					hashCode = (hashCode * 397) ^ obj.versionField;
					hashCode = (hashCode * 397) ^ (obj.messageTrackingIDField != null ? obj.messageTrackingIDField.GetHashCode() : 0);
					hashCode = (hashCode * 397) ^ (obj.additionalRefsField != null ? obj.additionalRefsField.GetHashCode() : 0);
					return hashCode;
				}
			}
		}

		static readonly IEqualityComparer<BillingTransaction> BillingTransactionComparerInstance = new BillingTransactionEqualityComparer();
	}
}
