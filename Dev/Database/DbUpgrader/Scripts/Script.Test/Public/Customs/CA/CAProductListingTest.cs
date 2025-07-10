using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.CA;
using Enterprise.Build.Database.Script.TestFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.CA
{
	[TestedType(typeof(CAProductListing))]
	class ProductListingTest : DbCreateScriptTest
	{
		public void TestCAProductListing()
		{
			var organisationPk = TestDataCreator.CreateOrganisation(new string('X', OrgHeaderSchema.OH_Code.MaxLength), new string('X', OrgHeaderSchema.OH_FullName.MaxLength), "CAYVR");

			var productPkOfCa = AddProduct(organisationPk, "CA", "CA", "AB", new DateTime(2022, 5, 30), new DateTime(2022, 5, 30), new DateTime(2022, 5, 30), "HTI", 'X');
			var productPkOfHtiUs = AddProduct(organisationPk, "CA", "US", "AL", new DateTime(2022, 6, 05), new DateTime(2022, 6, 05), new DateTime(2022, 6, 05), "HTI", 'A');
			var productPkOfHti = AddProduct(organisationPk, "CA", "CA", "BC", new DateTime(2022, 6, 10), new DateTime(2022, 6, 10), new DateTime(2022, 6, 10), "HTI", 'B');
			var productPkOfHte = AddProduct(organisationPk, "CA", "CA", "AB", new DateTime(2022, 6, 15), new DateTime(2022, 6, 15), new DateTime(2022, 6, 15), "HTE", 'C');
			var productPkOfHteUs = AddProduct(organisationPk, "CA", "US", "AL", new DateTime(2022, 6, 20), new DateTime(2022, 6, 20), new DateTime(2022, 6, 20), "HTE", 'D');
			var productPkOfHteCa = AddProduct(organisationPk, "CA", "CA", "BC", new DateTime(2022, 6, 25), new DateTime(2022, 6, 25), new DateTime(2022, 6, 25), "HTE", 'E');
			AddProduct(organisationPk, "AU", "AU", "", new DateTime(2022, 6, 30), new DateTime(2022, 6, 30), new DateTime(2022, 6, 30), "HTI", 'S');

			var countryCA = Guid.Empty;
			var countryUS = Guid.Empty;
			using (var reader = Db.Connection.Command("SELECT RN_PK,RN_Code FROM dbo.RefCountry WHERE RN_Code='US' OR RN_Code='CA'").ExecuteReader())
			{
				while (reader.Read())
				{
					var countryPk = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);
					var countryCode = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
					if (countryCode == "CA")
					{
						countryCA = countryPk;
					}
					if (countryCode == "US")
					{
						countryUS = countryPk;
					}
				}
			}

			var spSql = "SELECT * FROM CAProductListing ('All', 'OWN', @organisationPK,'','','','','','','','','','','','','')";
			using (var command = Db.Connection.Command(spSql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPk);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var productPk = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);

						if (productPk != Guid.Empty)
						{
							var expectedTreatmentCode = productPk == productPkOfCa ? "XX" : productPk == productPkOfHtiUs ? "AA" : productPk == productPkOfHti ? "BB" : productPk == productPkOfHte ? "CC" : productPk == productPkOfHteUs ? "DD" : productPk == productPkOfHteCa ? "EE" : string.Empty;
							var actualTreatmentCode = (string)reader["TariffTreatmentCode"];

							AssertEquals("Should only get the pivot data which was created for CA.", expectedTreatmentCode, actualTreatmentCode);
						}
					}
				}
			}

			spSql = "SELECT * FROM CAProductListing ('Import Tariff Lookup', 'OWN', @organisationPK,'','','','',@Origin,'','','','','','','','')";
			using (var command = Db.Connection.Command(spSql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPk);
				command.AddParameter("@Origin", SqlDbType.UniqueIdentifier, countryCA);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var productPk = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);

						if (productPk != Guid.Empty)
						{
							var expectedOrigin = productPk == productPkOfCa || productPk == productPkOfHti ? "CA" : string.Empty;
							var actualOrigin = (string)reader["Origin"];

							AssertEquals("Should only get the pivot data which Origin is CA and childType is HTI.", expectedOrigin, actualOrigin);
						}
					}
				}
			}

			spSql = "SELECT * FROM CAProductListing ('Import Tariff Lookup', 'OWN', @organisationPK,'','','','',@Origin,'AB','','','','','','','')";
			using (var command = Db.Connection.Command(spSql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPk);
				command.AddParameter("@Origin", SqlDbType.UniqueIdentifier, countryCA);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var productPk = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);

						if (productPk != Guid.Empty)
						{
							var expectedState = productPk == productPkOfCa ? "AB" : string.Empty;
							var actualState = (string)reader["State"];

							AssertEquals("Should only get the pivot data which Origin is 'CA',State is 'AB' and childType is 'HTI'.", expectedState, actualState);
						}
					}
				}
			}

			spSql = "SELECT * FROM CAProductListing ('Export Tariff Lookup', 'OWN', @organisationPK,'','','','',@Origin,'','','','','','','','')";
			using (var command = Db.Connection.Command(spSql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPk);
				command.AddParameter("@Origin", SqlDbType.UniqueIdentifier, countryCA);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var productPk = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);

						if (productPk != Guid.Empty)
						{
							var expectedType = productPk == productPkOfHte || productPk == productPkOfHteCa || productPk == productPkOfHteUs ? "HTE" : string.Empty;
							var actualType = (string)reader["PivotType"];

							AssertEquals("Should only get the pivot data which childType is 'HTE'.", expectedType, actualType);
						}
					}
				}
			}

			spSql = "SELECT * FROM CAProductListing ('Export Tariff Lookup', 'OWN', @organisationPK,'','','','',@Origin,'AB','','','','','','','')";
			using (var command = Db.Connection.Command(spSql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPk);
				command.AddParameter("@Origin", SqlDbType.UniqueIdentifier, countryCA);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var productPk = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);

						if (productPk != Guid.Empty)
						{
							var expectedType = productPk == productPkOfHte || productPk == productPkOfHteCa || productPk == productPkOfHteUs ? "HTE" : string.Empty;
							var actualType = (string)reader["PivotType"];

							AssertEquals("Should only get the pivot data which childType is 'HTE'.", expectedType, actualType);
						}
					}
				}
			}

			spSql = "SELECT * FROM CAProductListing ('ALL', 'OWN', @organisationPK, '','','','','','','2022-05-29 00:00:00.000','2022-05-31 00:00:00.000','','','','','')";
			using (var command = Db.Connection.Command(spSql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPk);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var productPk = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);

						if (productPk != Guid.Empty)
						{
							var expectedCreatedDate = productPk == productPkOfCa ? new DateTime(2022, 5, 30) : DateTime.UtcNow;
							var actualCreateDate = (DateTime)reader["CreatedDate"];

							AssertEquals("Should only get the pivot data which createDate is '2022-05-30 00:00:00.000'.", expectedCreatedDate, actualCreateDate);
						}
					}
				}
			}

			spSql = "SELECT * FROM CAProductListing ('ALL', 'OWN', @organisationPK, '','','','','','','','','2022-05-29 00:00:00.000','2022-05-31 00:00:00.000','','','')";
			using (var command = Db.Connection.Command(spSql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPk);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var productPk = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);

						if (productPk != Guid.Empty)
						{
							var expectedLastEditedDate = productPk == productPkOfCa ? new DateTime(2022, 5, 30) : DateTime.UtcNow;
							var actualLastEditedDate = (DateTime)reader["LastEditedDate"];

							AssertEquals("Should only get the pivot data which lastEditedDate is '2022-05-30 00:00:00.000'.", expectedLastEditedDate, actualLastEditedDate);
						}
					}
				}
			}

			spSql = "SELECT * FROM CAProductListing ('ALL', 'OWN', @organisationPK, '','','','','','','','','','','2022-05-29 00:00:00.000','2022-05-31 00:00:00.000','')";
			using (var command = Db.Connection.Command(spSql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPk);

				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						var productPk = reader.IsDBNull(0) ? Guid.Empty : reader.GetGuid(0);

						if (productPk != Guid.Empty)
						{
							var expectedAuditedDate = productPk == productPkOfCa ? new DateTime(2022, 5, 30) : DateTime.UtcNow;
							var actualAuditedDate = (DateTime)reader["AuditedDate"];

							AssertEquals("Should only get the pivot data which auditedDate is '2022-05-30 00:00:00.000'.", expectedAuditedDate, actualAuditedDate);
						}
					}
				}
			}
		}

		Guid AddProduct(Guid organisationPK, string countryCode, string origin, string state, DateTime createdDate, DateTime lastEditedDate, DateTime auditedDate, string childType, char charString)
		{
			var productPK = Guid.NewGuid();
			var partRelationPK = Guid.NewGuid();
			var classificationPK = Guid.NewGuid();
			var classPartPivotPK = Guid.NewGuid();
			var caClassificationPK = Guid.NewGuid();

			var createRecordsSql = @"
INSERT INTO dbo.OrgSupplierPart (OP_PK, OP_PartNum, OP_Desc, OP_Division, OP_StockKeepingUnit, OP_SystemCreateTimeUtc, OP_SystemCreateUser, OP_SystemLastEditTimeUtc, OP_SystemLastEditUser) VALUES (@productPK, @partNum, @partDesc, @partDivision, @stockKeepingUnit, @createdDate, '~BP', @lastEditedDate, '~BP')
INSERT INTO dbo.OrgPartRelation (OU_PK, OU_OH, OU_OP, OU_Relationship, OU_SystemCreateTimeUtc, OU_SystemCreateUser, OU_SystemLastEditTimeUtc, OU_SystemLastEditUser) VALUES (@partRelationPK, @organisationPK, @productPK, 'BTH', @createdDate, '~BP', @lastEditedDate, '~BP')
INSERT INTO dbo.CusClassification (CC_PK, CC_LookupCode, CC_Description, CC_RN_NKCountryCode, CC_ClassificationType, CC_TariffNum, CC_AddInfo, CC_SystemCreateTimeUtc, CC_SystemCreateUser, CC_SystemLastEditTimeUtc, CC_SystemLastEditUser) VALUES (@classificationPK, @lookupCode, @lookupCode, @countryCode, 'IMP', @tariffNum, @CC_AddInfo, GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT INTO dbo.CusClassPartPivot (CI_PK, CI_OP, CI_CC, CI_ChildType, CI_TariffNum, CI_RN_NKCountry, CI_SystemCreateTimeUtc, CI_SystemCreateUser, CI_SystemLastEditTimeUtc, CI_SystemLastEditUser,CI_LastAuditedDate,CI_LastAuditedUser)  VALUES (@classPartPivotPK, @productPK, @classificationPK, @childType, @tariffNum, @countryCode, @createdDate, '~BP', @lastEditedDate, '~BP',@auditedDate, '~BP')
INSERT INTO dbo.CusCAClassification (CCA_PK, CCA_ParentID, CCA_ParentTableCode, CCA_RN_NKOrigin, CCA_ProvinceOfOrigin, CCA_GSTStatusCode, CCA_ETExemption, CCA_99TariffCode, CCA_AuthorityNumber, CCA_TRSNumber, CCA_TreatmentCode, CCA_ValueForDutyCode, CCA_SystemCreateTimeUtc, CCA_SystemCreateUser, CCA_SystemLastEditTimeUtc, CCA_SystemLastEditUser) VALUES (@caClassificationPK, @classPartPivotPK, 'CI', @origin, @provinceOfOrigin, @gSTStatusCode, @eTExemption, @tariffCode, @authorityNumber, @tRSNumber, @treatmentCode, @valueForDutyCode, @createdDate, '~BP', @lastEditedDate, '~BP')
";
			var cC_AddInfo = string.Join("*", new string[]
			{
				"99TariffCode=" + new string(charString, CAAddInfoSchema.CA_99TariffCode.MaxLength),
				"AuthorityNumber=" + new string(charString, CAAddInfoSchema.CA_AuthorityNumber.MaxLength),
				"TRSNumber=" + new string(charString, CAAddInfoSchema.CA_TRSNumber.MaxLength),
				"TreatmentCode=" + new string(charString, CAAddInfoSchema.CA_TreatmentCode.MaxLength),
				"ValueForDutyCode=" + new string(charString, CAAddInfoSchema.CA_ValueForDutyCode.MaxLength),
			});

			using (var command = Db.Connection.Command(createRecordsSql))
			{
				command.AddParameter("@organisationPK", SqlDbType.UniqueIdentifier, organisationPK);
				command.AddParameter("@productPK", SqlDbType.UniqueIdentifier, productPK);
				command.AddParameter("@partRelationPK", SqlDbType.UniqueIdentifier, partRelationPK);
				command.AddParameter("@classificationPK", SqlDbType.UniqueIdentifier, classificationPK);
				command.AddParameter("@classPartPivotPK", SqlDbType.UniqueIdentifier, classPartPivotPK);
				command.AddParameter("@caClassificationPK", SqlDbType.UniqueIdentifier, caClassificationPK);
				command.AddParameter("@partNum", SqlDbType.VarChar, new string(charString, OrgSupplierPartSchema.OP_PartNum.MaxLength));
				command.AddParameter("@partDesc", SqlDbType.VarChar, new string(charString, OrgSupplierPartSchema.OP_Desc.MaxLength));
				command.AddParameter("@partDivision", SqlDbType.VarChar, new string(charString, OrgSupplierPartSchema.OP_Division.MaxLength));
				command.AddParameter("@stockKeepingUnit", SqlDbType.VarChar, new string(charString, OrgSupplierPartSchema.OP_StockKeepingUnit.MaxLength));
				command.AddParameter("@lookupCode", SqlDbType.VarChar, new string(charString, CusClassificationSchema.CC_LookupCode.MaxLength));
				command.AddParameter("@countryCode", SqlDbType.VarChar, CusClassificationSchema.CC_RN_NKCountryCode.MaxLength, countryCode);
				command.AddParameter("@tariffNum", SqlDbType.VarChar, new string(charString, CusClassificationSchema.CC_TariffNum.MaxLength));
				command.AddParameter("@CC_AddInfo", SqlDbType.VarChar, cC_AddInfo);
				command.AddParameter("@origin", SqlDbType.VarChar, origin);
				command.AddParameter("@provinceOfOrigin", SqlDbType.VarChar, state);
				command.AddParameter("@gSTStatusCode", SqlDbType.VarChar, new string(charString, CusCAClassificationSchema.CCA_GSTStatusCode.MaxLength));
				command.AddParameter("@eTExemption", SqlDbType.VarChar, new string(charString, CusCAClassificationSchema.CCA_ETExemption.MaxLength));
				command.AddParameter("@tariffCode", SqlDbType.VarChar, new string(charString, CusCAClassificationSchema.CCA_99TariffCode.MaxLength));
				command.AddParameter("@authorityNumber", SqlDbType.VarChar, new string(charString, CusCAClassificationSchema.CCA_AuthorityNumber.MaxLength));
				command.AddParameter("@tRSNumber", SqlDbType.VarChar, new string(charString, CusCAClassificationSchema.CCA_TRSNumber.MaxLength));
				command.AddParameter("@treatmentCode", SqlDbType.VarChar, new string(charString, CusCAClassificationSchema.CCA_TreatmentCode.MaxLength));
				command.AddParameter("@valueForDutyCode", SqlDbType.VarChar, new string(charString, CusCAClassificationSchema.CCA_ValueForDutyCode.MaxLength));
				command.AddParameter("@childType", SqlDbType.VarChar, childType);
				command.AddParameter("@createdDate", SqlDbType.DateTime, createdDate);
				command.AddParameter("@lastEditedDate", SqlDbType.DateTime, lastEditedDate);
				command.AddParameter("@auditedDate", SqlDbType.DateTime, auditedDate);

				command.ExecuteNonQuery();
			}

			return productPK;
		}
	}
}

