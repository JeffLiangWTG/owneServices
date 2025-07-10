using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.Organizations.CodeGeneration;
using CargoWise.Types;
using Enterprise.DataTransfer.Native.Utils;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgCodeGenerations
{
	class OrgCodeDataProvider : IOrgCodeInfo
	{
		public OrgCodeDataProvider(Guid internalPK, DataRow row)
		{
			this.internalPK = internalPK;
			this.row = row;
		}
		readonly DataRow row;
		readonly Guid internalPK;

		void initializeMembersFromDb()
		{
			InitializeOrgHeaderMembersFromDb();
			InitializeRefUnlocoMembersFromDb();
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No easy access to factory here.")]
		void InitializeOrgHeaderMembersFromDb()
		{
			OH_PK = new ZGuid(row[OrgHeaderSchema.Constants.PK]).ToGuid();
			if (internalPK.IsEmpty())
			{
				string sql = string.Format(CultureInfo.InvariantCulture, @"SELECT {0} FROM {1} WHERE {2} = @code", OrgHeaderSchema.Constants.PK, OrgHeaderSchema.Constants.TableName, OrgHeaderSchema.Constants.OH_Code);
				var command = Db.Connection.Command(sql);
				command.AddParameterBasedOnDbColumn("code", row[OrgHeaderSchema.Constants.OH_Code], OrgHeaderSchema.OH_Code);
				var reader = command.ExecuteReader(CommandBehavior.SingleRow);
				try
				{
					if (reader.Read())
					{
						OH_PK = new ZGuid(reader[OrgHeaderSchema.Constants.PK]).ToGuid();
					}
				}
				finally
				{
					reader.Close();
					fullyInitialized = true;
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "No easy access to factory here.")]
		void InitializeRefUnlocoMembersFromDb()
		{
			string sql = @"SELECT
			ISNULL(RL_IATA, '') AS RL_IATA, ISNULL(RL_PortName, '') AS RL_PortName, ISNULL(RN_Desc, '') AS RN_Desc
			FROM dbo.RefUnloco
			LEFT JOIN dbo.RefCountry ON RL_RN_NKCountryCode = RN_Code
			WHERE RL_Code = @code";
			var command = Db.Connection.Command(sql);
			command.AddParameterBasedOnDbColumn("code", row[OrgHeaderSchema.Constants.OH_RL_NKClosestPort], RefUNLOCOSchema.RL_Code);
			var reader = command.ExecuteReader(CommandBehavior.SingleRow);
			try
			{
				if (reader.Read())
				{
					RL_IATA = (string)reader[RefUNLOCOSchema.Constants.RL_IATA];
					RL_PortName = (string)reader[RefUNLOCOSchema.Constants.RL_PortName];
					RN_Desc = (string)reader[RefCountrySchema.Constants.RN_Desc];
				}
			}
			finally
			{
				reader.Close();
				fullyInitialized = true;
			}
		}
		bool fullyInitialized;

		#region IOrgCodeInfo Members

		public string CountryCode
		{
			get { return string.IsNullOrWhiteSpace(UnlocoCode) ? "" : UnlocoCode.Substring(0, 2); } //first two characters of an UnlocoCode is its CountryCode. Saves a query.
		}

		public string IataCode
		{
			get
			{
				if (!fullyInitialized)
				{
					initializeMembersFromDb();
				}
				return RL_IATA;
			}
		}
		string RL_IATA = "";

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
			get { return new ZString(row[OrgHeaderSchema.Constants.OH_Code]).Trim(); }
		}

		public bool OH_IsBroker
		{
			get { return new ZBool(row[OrgHeaderSchema.Constants.OH_IsBroker]); }
		}

		public bool OH_IsCompetitor
		{
			get { return new ZBool(row[OrgHeaderSchema.Constants.OH_IsCompetitor]); }
		}

		public bool OH_IsConsignee
		{
			get { return new ZBool(row[OrgHeaderSchema.Constants.OH_IsConsignee]); }
		}

		public bool OH_IsConsignor
		{
			get { return new ZBool(row[OrgHeaderSchema.Constants.OH_IsConsignor]); }
		}

		public bool OH_IsForwarder
		{
			get { return new ZBool(row[OrgHeaderSchema.Constants.OH_IsForwarder]); }
		}

		public bool OH_IsGlobalAccount
		{
			get { return new ZBool(row[OrgHeaderSchema.Constants.OH_IsGlobalAccount]); }
		}

		public bool OH_IsMiscFreightServices
		{
			get { return new ZBool(row[OrgHeaderSchema.Constants.OH_IsMiscFreightServices]); }
		}

		public bool OH_IsNationalAccount
		{
			get { return new ZBool(row[OrgHeaderSchema.Constants.OH_IsNationalAccount]); }
		}

		public bool OH_IsSalesLead
		{
			get { return new ZBool(row[OrgHeaderSchema.Constants.OH_IsSalesLead]); }
		}

		public bool OH_IsShippingProvider
		{
			get { return new ZBool(row[OrgHeaderSchema.Constants.OH_IsShippingProvider]); }
		}

		public bool OH_IsTransportClient
		{
			get { return new ZBool(row[OrgHeaderSchema.Constants.OH_IsTransportClient]); }
		}

		public bool OH_IsWarehouseClient
		{
			get { return new ZBool(row[OrgHeaderSchema.Constants.OH_IsWarehouseClient]); }
		}

		public string OH_Language
		{
			get { return new ZString(row[OrgHeaderSchema.Constants.OH_Language]); }
		}

		public string OH_FullName
		{
			get { return new ZString(row[OrgHeaderSchema.Constants.OH_FullName]).Trim(); }
		}

		public Guid PK
		{
			get
			{
				if (!fullyInitialized)
				{
					initializeMembersFromDb();
				}
				return OH_PK;
			}
		}
		Guid OH_PK;

		public string UnlocoCode
		{
			get { return new ZString(row[OrgHeaderSchema.Constants.OH_RL_NKClosestPort]).Trim(); }
		}

		public string InvalidUnlocoCode
		{
			get { return string.Empty; }
		}

		public string CountryName
		{
			get
			{
				if (!fullyInitialized)
				{
					initializeMembersFromDb();
				}
				return RN_Desc;
			}
		}
		string RN_Desc = "";

		public string PortName
		{
			get
			{
				if (!fullyInitialized)
				{
					initializeMembersFromDb();
				}
				return RL_PortName;
			}
		}
		string RL_PortName = "";

		#endregion
	}
}
