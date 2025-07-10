using System;
using System.Data;
using CargoWise.Data;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DbUpgrader.Transformation.DataModification.Testing
{
	abstract class MoveSupplierBookingLineJobDocAddressTest : DataTransformationTestCase
	{
		#region Implementation

		protected Guid CreateOrgHeader(string code, string name = "", string closestPort = "")
		{
			Guid pk = Guid.NewGuid();

			string insertSql = "INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES (@OH_PK, @OH_Code, @OH_FullName, @OH_RL_NKClosestPort)";

			using (var command = TestConnection.Command(insertSql))
			{
				command.AddParameterBasedOnDbColumn("@OH_PK", pk, OrgHeaderSchema.PK);
				command.AddParameterBasedOnDbColumn("@OH_Code", code, OrgHeaderSchema.OH_Code);
				command.AddParameterBasedOnDbColumn("@OH_FullName", name, OrgHeaderSchema.OH_FullName);
				command.AddParameterBasedOnDbColumn("@OH_RL_NKClosestPort", closestPort, OrgHeaderSchema.OH_RL_NKClosestPort);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		protected Guid CreateOrgAddress(Guid orgPK, string addressCode, string address1, string address2 = "", string city = "", string postcode = "", string state = "", string companyNameOverride = "")
		{
			Guid pk = Guid.NewGuid();

			var createAddressSql = @"insert into dbo.OrgAddress(OA_PK, OA_OH, OA_Code, OA_Address1, OA_Address2, OA_City, OA_PostCode, OA_State, OA_CompanyNameOverride)
				values(@addressPK, @orgPK, @addressCode, @address1, @address2, @city, @postcode, @state, @companyNameOverride)";

			using (var command = TestConnection.Command(createAddressSql))
			{
				command.AddParameterBasedOnDbColumn("@addressPK", pk, OrgAddressSchema.PK);
				command.AddParameterBasedOnDbColumn("@orgPK", orgPK, OrgAddressSchema.OA_OH);
				command.AddParameterBasedOnDbColumn("@addressCode", addressCode, OrgAddressSchema.OA_Code);
				command.AddParameterBasedOnDbColumn("@address1", address1, OrgAddressSchema.OA_Address1);
				command.AddParameterBasedOnDbColumn("@address2", address2, OrgAddressSchema.OA_Address2);
				command.AddParameterBasedOnDbColumn("@city", city, OrgAddressSchema.OA_City);
				command.AddParameterBasedOnDbColumn("@postcode", postcode, OrgAddressSchema.OA_PostCode);
				command.AddParameterBasedOnDbColumn("@state", state, OrgAddressSchema.OA_State);
				command.AddParameterBasedOnDbColumn("@companyNameOverride", companyNameOverride, OrgAddressSchema.OA_CompanyNameOverride);

				command.ExecuteNonQuery();
			}

			return pk;
		}

		protected Guid CreateSupplierBookingLine(Guid bookingHeaderPK)
		{
			Guid pk = Guid.NewGuid();

			const string sql = @"
				insert into dbo.SupplierBookingLine(DL_PK, DL_DH_BookingHeader, DL_SystemCreateTimeUtc, DL_SystemLastEditTimeUtc)
				values(@pk, @bookingHeader, @createTime, @lastEditTime)
				";

			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@bookingHeader", SqlDbType.UniqueIdentifier, bookingHeaderPK);
				command.AddParameterBasedOnDbColumn("@createTime", DateTime.Now, SupplierBookingLineSchema.DL_SystemCreateTimeUtc);
				command.AddParameterBasedOnDbColumn("@lastEditTime", DateTime.Now, SupplierBookingLineSchema.DL_SystemLastEditTimeUtc);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		protected Guid CreateJobDocAddress(Guid bookingLinePK, Guid orgAddressPK, string addressType, string address1, string address2, string city,
			string postCode, string state, string countryCode, string phone, string fax, string contact, string mobile, string email, string companyName)
		{
			Guid pk = Guid.NewGuid();

			string insertSql = @"INSERT INTO dbo.JobDocAddress (
E2_PK
,E2_ParentID
,E2_ParentTableCode
,E2_AddressType
,E2_OA_Address
,E2_Address1
,E2_Address2
,E2_City
,E2_Postcode
,E2_State
,E2_RN_NKCountryCode
,E2_Phone
,E2_Fax
,E2_Contact
,E2_Mobile
,E2_Email
,E2_CompanyName
)
VALUES (
@pk
,@parentPK
,@parentTableCode
,@addressType
,@orgAddressPK
,@address1
,@address2
,@city
,@postCode
,@state
,@countryCode
,@phone
,@fax
,@contact
,@mobile
,@email
,@companyName
)";

			using (var command = TestConnection.Command(insertSql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@parentPK", SqlDbType.UniqueIdentifier, bookingLinePK);
				command.AddParameter("@orgAddressPK", SqlDbType.UniqueIdentifier, orgAddressPK);
				command.AddParameterBasedOnDbColumn("@parentTableCode", "DL", JobDocAddressSchema.E2_ParentTableCode);
				command.AddParameterBasedOnDbColumn("@addressType", addressType, JobDocAddressSchema.E2_AddressType);
				command.AddParameterBasedOnDbColumn("@address1", address1, JobDocAddressSchema.E2_Address1);
				command.AddParameterBasedOnDbColumn("@address2", address2, JobDocAddressSchema.E2_Address2);
				command.AddParameterBasedOnDbColumn("@city", city, JobDocAddressSchema.E2_City);
				command.AddParameterBasedOnDbColumn("@postCode", postCode, JobDocAddressSchema.E2_Postcode);
				command.AddParameterBasedOnDbColumn("@state", state, JobDocAddressSchema.E2_State);
				command.AddParameterBasedOnDbColumn("@countryCode", countryCode, JobDocAddressSchema.E2_RN_NKCountryCode);
				command.AddParameterBasedOnDbColumn("@phone", phone, JobDocAddressSchema.E2_Phone);
				command.AddParameterBasedOnDbColumn("@fax", fax, JobDocAddressSchema.E2_Fax);
				command.AddParameterBasedOnDbColumn("@contact", contact, JobDocAddressSchema.E2_Contact);
				command.AddParameterBasedOnDbColumn("@mobile", mobile, JobDocAddressSchema.E2_Mobile);
				command.AddParameterBasedOnDbColumn("@email", email, JobDocAddressSchema.E2_Email);
				command.AddParameterBasedOnDbColumn("@companyName", companyName, JobDocAddressSchema.E2_CompanyName);
				command.ExecuteNonQuery();
			}

			return pk;
		}

		#endregion
	}
}
