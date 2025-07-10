using System;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Core
{
	public abstract class CreateSystemOrgScripts
	{
		#region SuppressResourceStringsCheckRegion

		public Guid Create()
		{
			Guid pk = GetOrgPK();
			if (pk == Guid.Empty)
			{
				try
				{
					CreateOrg();
					CreateAddress();
					CreatePatternMatch();
					AdditionalActions();
					return OrgPK;
				}
				catch (Exception) when (GetOrgPK() != Guid.Empty)
				{
				}
			}
			return pk;
		}

		#region Properties

		protected internal abstract string OrgCode { get; }
		protected internal abstract string OrgName { get; }
		protected internal virtual string OA_Code { get { return "OFC: NO ADDRESS SPECIFIED"; } }
		protected internal virtual string OA_Address1 { get { return "NO ADDRESS SPECIFIED"; } }
		protected internal virtual string OA_Address2 { get { return "SYSTEM DEFINED ORGANISATION"; } }
		protected internal virtual string OA_City { get { return "NA"; } }
		protected internal virtual string OA_State { get { return "NSW"; } }
		protected internal abstract string OS_FullCompanyName { get; }
		protected internal abstract string OS_CompanyName1 { get; }
		protected internal abstract string OS_CompanyName2 { get; }
		protected internal abstract string OS_CompanyName3 { get; }
		protected internal abstract string OS_CompanyName4 { get; }
		protected internal abstract string OS_Address1 { get; }
		protected internal abstract string OS_Address2 { get; }
		protected internal abstract string OS_Address3 { get; }
		protected internal abstract string OS_Address4 { get; }
		protected internal abstract string OS_City { get; }
		protected internal abstract string OS_State { get; }

		#endregion

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		Guid GetOrgPK()
		{
			object obj;
			using (DbCommand cmd = Db.Connection.Command("SELECT OH_PK FROM dbo.OrgHeader WHERE OH_CODE = '" + OrgCode + "'"))
			{
				obj = cmd.ExecuteScalar();
			}
			return obj == null ? Guid.Empty : (Guid)obj;
		}

		#region Create OrgHeader

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void CreateOrg()
		{
			string sql = @"INSERT INTO dbo.OrgHeader
									( 
										" + OrgHeaderSchema.PK.Name + ", " +
										OrgHeaderSchema.OH_IsActive.Name + ", " +
										OrgHeaderSchema.OH_Code.Name + ", " +
										OrgHeaderSchema.OH_FullName.Name + ", " +
										OrgHeaderSchema.OH_RL_NKClosestPort.Name + ", " +
										OrgHeaderSchema.OH_Language.Name + ", " +
										OrgHeaderSchema.OH_IsConsignee.Name + ", " +
										OrgHeaderSchema.OH_IsConsignor.Name + ", " +
										OrgHeaderSchema.OH_IsTransportClient.Name + ", " +
										OrgHeaderSchema.OH_IsWarehouseClient.Name + ", " +
										OrgHeaderSchema.OH_IsForwarder.Name + "," +
										OrgHeaderSchema.OH_IsShippingProvider.Name + ", " +
										OrgHeaderSchema.OH_IsBroker.Name + ", " +
										OrgHeaderSchema.OH_IsMiscFreightServices.Name + ", " +
										OrgHeaderSchema.OH_IsCompetitor.Name + ", " +
										OrgHeaderSchema.OH_IsSalesLead.Name +
									@")
									VALUES 
									(
										'" + OrgPK.ToString() + @"', 
										1, 
										'" + OrgCode + @"', 
										'" + OrgName + @"', 
										'AUSYD', 
										'" + Enterprise.Core.SharedConstants.Languages.English + @"', 
										1, 
										1, 
										1, 
										1, 
										1, 
										1, 
										1, 
										1, 
										1, 
										1
									)";

			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Create Main Address

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void CreateAddress()
		{
			string sql = @"INSERT INTO dbo.OrgAddress (" +
									  OrgAddressSchema.PK.Name + ", " +
									  OrgAddressSchema.OA_IsActive.Name + ", " +
									  OrgAddressSchema.OA_Code.Name + ", " +
									  OrgAddressSchema.OA_Language.Name + ", " +
									  OrgAddressSchema.OA_Address1.Name + ", " +
									  OrgAddressSchema.OA_Address2.Name + ", " +
									  OrgAddressSchema.OA_City.Name + ", " +
									  OrgAddressSchema.OA_State.Name + ", " +
									  OrgAddressSchema.OA_OH.Name + "" +
									  ") VALUES ('" + AddressPK.ToString() + "', 1, '" + OA_Code + "', '" + Enterprise.Core.SharedConstants.Languages.English + "', '" + OA_Address1 + "', '" + OA_Address2 + "', '" + OA_City + "', '" + OA_State + "', '" + OrgPK.ToString() + "')";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}

			sql = @"INSERT INTO dbo.OrgAddressCapability (" +
									OrgAddressCapabilitySchema.PK.Name + ", " +
									OrgAddressCapabilitySchema.PZ_OA.Name + ", " +
									OrgAddressCapabilitySchema.PZ_AddressType.Name + ", " +
									OrgAddressCapabilitySchema.PZ_IsMainAddress.Name +
									") VALUES ('" + Guid.NewGuid().ToString() + "', '" + AddressPK.ToString() + "', 'OFC', 1)";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Create Pattern Match Record

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory", Justification = "Baseline")]
		void CreatePatternMatch()
		{
			string sql = @"INSERT INTO dbo.OrgPatternMatch (" +
									  OrgPatternMatchSchema.PK.Name + ", " +
									  OrgPatternMatchSchema.OS_FullCompanyName.Name + ", " +
									  OrgPatternMatchSchema.OS_CompanyName1.Name + ", " +
									  OrgPatternMatchSchema.OS_CompanyName2.Name + ", " +
									  OrgPatternMatchSchema.OS_CompanyName3.Name + ", " +
									  OrgPatternMatchSchema.OS_CompanyName4.Name + ", " +
									  OrgPatternMatchSchema.OS_IsCorporation.Name + ", " +
									  OrgPatternMatchSchema.OS_IsPOBox.Name + ", " +
									  OrgPatternMatchSchema.OS_Address1.Name + ", " +
									  OrgPatternMatchSchema.OS_Address2.Name + ", " +
									  OrgPatternMatchSchema.OS_Address3.Name + ", " +
									  OrgPatternMatchSchema.OS_Address4.Name + ", " +
									  OrgPatternMatchSchema.OS_City.Name + ", " +
									  OrgPatternMatchSchema.OS_State.Name + ", " +
									  OrgPatternMatchSchema.OS_UNLOCO.Name + ", " +
									  OrgPatternMatchSchema.OS_OH.Name + ", " +
									  OrgPatternMatchSchema.OS_OA.Name +
									  ") VALUES (newid(), '" + OS_FullCompanyName + "', '" + OS_CompanyName1 + "', '" + OS_CompanyName2 + "', '" + OS_CompanyName3 + "', '" + OS_CompanyName4 + "'" +
									  ", 0, 0, '" + OS_Address1 + "', '" + OS_Address2 + "', '" + OS_Address3 + "', '" + OS_Address4 + "', '" + OS_City + "', '" + OS_State + "', 'AUSYD', " +
									  "'" + OrgPK.ToString() + "', '" + AddressPK.ToString() + "')";
			using (var cmd = Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
		}

		#endregion

		#region Additional

		protected virtual void AdditionalActions()
		{ }

		#endregion

		protected Guid OrgPK = Guid.NewGuid();
		protected Guid AddressPK = Guid.NewGuid();

		#endregion
	}
}
