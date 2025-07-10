using System;
using Enterprise.Integration.Billing;

namespace Enterprise.Billing.Integration
{
	public partial class UsageTransaction : IEquatable<UsageTransaction>, IStlTransaction
	{
		[System.Xml.Serialization.XmlIgnore]
		public string Branch { get => BranchCode; set => BranchCode = value; }
		[System.Xml.Serialization.XmlIgnore]
		public string PriceItemCode { get => UsageCode; set => UsageCode = value; }
		[System.Xml.Serialization.XmlIgnore]
		public int BillableCount { get => UsageCount; set => UsageCount = value; }
		[System.Xml.Serialization.XmlIgnore]
		public string ClientStaffCode { get; set; }
		[System.Xml.Serialization.XmlIgnore]
		public string Reference1 { get; set; }
		[System.Xml.Serialization.XmlIgnore]
		public string Reference2 { get; set; }
		[System.Xml.Serialization.XmlIgnore]
		public string Reference3 { get; set; }
		[System.Xml.Serialization.XmlIgnore]
		public string Reference4 { get; set; }
		[System.Xml.Serialization.XmlIgnore]
		public string Reference5 { get; set; }

		public bool Equals(UsageTransaction other)
		{
			if (other is null)
			{
				return false;
			}

			return	UsageCount == other.UsageCount
					&& string.Equals(AdditionalRefs, other.AdditionalRefs)
					&& ServiceOccuredUTC == other.ServiceOccuredUTC
					&& string.Equals(AdditionalRefs, other.AdditionalRefs)
					&& string.Equals(EnterpriseCode, other.EnterpriseCode)
					&& string.Equals(ServerCode, other.ServerCode)
					&& string.Equals(Environment, other.Environment)
					&& string.Equals(CompanyCode, other.CompanyCode)
					&& string.Equals(CompanyName, other.CompanyName)
					&& string.Equals(BranchCode, other.BranchCode)
					&& string.Equals(UsageCode, other.UsageCode);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as UsageTransaction);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = UsageCount;
				hashCode = (hashCode * 397) ^ (AdditionalRefs != null ? AdditionalRefs.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ ServiceOccuredUTC.GetHashCode();
				hashCode = (hashCode * 397) ^ (EnterpriseCode != null ? EnterpriseCode.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (ServerCode != null ? ServerCode.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Environment != null ? Environment.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (CompanyCode != null ? CompanyCode.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (CompanyName != null ? CompanyName.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (BranchCode != null ? BranchCode.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (UsageCode != null ? UsageCode.GetHashCode() : 0);
				return hashCode;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is string representation of the billing transaction, used in error reports")]
		public override string ToString()
		{
			return string.Format("BillableCount: {0}, ClientStaffCode: {1}, Reference1: {2}, Reference2: {3}, Reference3: {4}, Reference4: {5}, Reference5: {6}, ServiceOccuredUTC: {7}, AdditionalRefs: {8}, EnterpriseCode: {9}, ServerCode: {10}, Environment: {11}, CompanyCode: {12}, CompanyName: {13}, BranchCode: {14}, UsageCode: {15}",
				BillableCount, ClientStaffCode, Reference1, Reference2, Reference3, Reference4, Reference5, ServiceOccuredUTC, AdditionalRefs, EnterpriseCode, ServerCode, Environment, CompanyCode, CompanyName, BranchCode, UsageCode);
		}
	}
}
