using System;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.DFD.Business.Import
{
	class USSupplierOrgCodeInfo : IOrgCodeInfo
	{
		public USSupplierOrgCodeInfo(OrgHeader org)
		{
			this.Org = org;
		}

		readonly OrgHeader Org;

		#region IOrgCodeInfo Members

		public string CountryCode
		{
			get { return Org.CountryCode; }
		}

		public string CountryName
		{
			get { return Org.CountryName; }
		}

		public string IataCode
		{
			get { return Org.OH_RL_NKClosestPort.Right(3); }
		}

		public bool IsCreditorForAnyCompany
		{
			get { return false; }
		}

		public bool IsDebtorForAnyCompany
		{
			get { return false; }
		}

		public string OH_Code
		{
			get { return Org.OH_Code; }
		}

		public string OH_FullName
		{
			get { return Org.OH_FullNameTruncated; }
		}

		public bool OH_IsBroker
		{
			get { return Org.OH_IsBroker; }
		}

		public bool OH_IsCompetitor
		{
			get { return Org.OH_IsCompetitor; }
		}

		public bool OH_IsConsignee
		{
			get { return Org.OH_IsConsignee; }
		}

		public bool OH_IsConsignor
		{
			get { return Org.OH_IsConsignor; }
		}

		public bool OH_IsForwarder
		{
			get { return Org.OH_IsForwarder; }
		}

		public bool OH_IsGlobalAccount
		{
			get { return Org.OH_IsGlobalAccount; }
		}

		public bool OH_IsMiscFreightServices
		{
			get { return Org.OH_IsMiscFreightServices; }
		}

		public bool OH_IsNationalAccount
		{
			get { return Org.OH_IsNationalAccount; }
		}

		public bool OH_IsSalesLead
		{
			get { return Org.OH_IsSalesLead; }
		}

		public bool OH_IsShippingProvider
		{
			get { return Org.OH_IsShippingProvider; }
		}

		public bool OH_IsTransportClient
		{
			get { return Org.OH_IsTransportClient; }
		}

		public bool OH_IsWarehouseClient
		{
			get { return Org.OH_IsWarehouseClient; }
		}

		public string OH_Language
		{
			get { return Org.OH_Language; }
		}

		public Guid PK
		{
			get { return Org.PK.ToGuid(); }
		}

		public string PortName
		{
			get { return Org.PortName; }
		}

		public string UnlocoCode
		{
			get { return Org.OH_RL_NKClosestPort; }
		}

		public string InvalidUnlocoCode
		{
			get { return string.Empty; }
		}

		#endregion
	}
}
