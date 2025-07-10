using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.AUS.Business
{
	public class ClientAUSOriginPreferenceMapping : AutoClientAUSOriginPreferenceMapping
	{
		public ClientAUSOriginPreferenceMapping(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		#region Lookups

		public OrgHeaderCollection ImporterList
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public OrgHeaderCollection SupplierList
		{
			get { return new OrgHeaderCollection(Factory); }
		}

		public RefCountryCollection OriginList
		{
			get { return Lookups.T7_ORG_List; }
		}

		public CodeDescriptionPairList PreferenceSchemeList
		{
			get { return Lookups.T7_PST_List; }
		}

		public CodeDescriptionPairList PreferenceRuleList
		{
			get { return Lookups.T7_PRT_List; }
		}

		#endregion

		#region Logging

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		#endregion
	}
}
