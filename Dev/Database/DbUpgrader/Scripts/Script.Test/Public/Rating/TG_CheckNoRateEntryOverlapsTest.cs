using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Rating;
using CargoWise.Schema;
using Enterprise.DbUpgrader.Shared;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Rating.Testing
{
	[TestedType(typeof(TG_CheckNoRateEntryOverlaps))]
	class TG_CheckNoRateEntryOverlapsTest : DBCreateTriggerScriptTest
	{
		public void TestAllRateEntrySchemaColumnsAreCovered()
		{
			var allSchemaResults = RateEntrySchema.All.Select(x => x.Name);
			var keyAndNonKeyColumns = keyColumns.Concat(systemColumns).Concat(nonKeyColumns).Concat(datesAndPkColumns).Select(x => x.Name);

			AssertContainsExactElementsInAnyOrder(allSchemaResults, keyAndNonKeyColumns);
		}

		public void TestKeyColumns()
		{
			foreach (SchemaColumn column in keyColumns)
			{
				AssertKeyColumnMakesDuplicateUnique(column);
			}
		}

		public void TestNonKeyColumns()
		{
			foreach (SchemaColumn column in nonKeyColumns)
			{
				AssertNonKeyColumnDoesntMakeDuplicateUnique(column);
			}
		}

		public void TestDateColumns()
		{
			AssertDateOverlap(false, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2012, 02, 02), null);
			AssertDateOverlap(false, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2012, 02, 02), new DateTime(2013, 01, 01));
			AssertDateOverlap(false, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2011, 02, 03), null);
			AssertDateOverlap(false, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2011, 02, 03), new DateTime(2013, 01, 01));
			AssertDateOverlap(true, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2011, 02, 02), null);
			AssertDateOverlap(true, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2011, 02, 02), new DateTime(2013, 01, 01));
			AssertDateOverlap(true, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2011, 01, 01), null);
			AssertDateOverlap(true, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2011, 01, 01), new DateTime(2013, 01, 01));
			AssertDateOverlap(true, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2010, 05, 05), new DateTime(2010, 06, 06));
			AssertDateOverlap(true, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2009, 01, 01), null);
			AssertDateOverlap(true, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2009, 01, 01), new DateTime(2010, 01, 01));
			AssertDateOverlap(true, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2009, 01, 01), new DateTime(2011, 02, 02));
			AssertDateOverlap(true, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2009, 01, 01), new DateTime(2011, 03, 02));
			AssertDateOverlap(false, new DateTime(2010, 01, 01), new DateTime(2011, 02, 02), new DateTime(2009, 01, 01), new DateTime(2009, 10, 10));
		}

		public void TestAddingNewStandardCosting_Duplicate_ThrowsException()
		{
			var ratingHeader = new ActiveRowWrapper(RatingHeaderSchema.Instance);
			ratingHeader[RatingHeaderSchema.TH_RateType] = "COS";
			ratingHeader[RatingHeaderSchema.TH_GC] = GlbCompanyPK;
			ratingHeader.Save();

			var entry1 = NewSaveableEntry(ratingHeader);
			var entry2 = NewSaveableEntry(ratingHeader);

			AssertOverlaps(entry1, entry2);
		}

		public void TestAddingNewClientRateEntry_Duplicate_ThrowsException()
		{
			var clientRate = NewRatingHeader1();
			clientRate[RatingHeaderSchema.TH_GC] = GlbCompanyPK;
			clientRate.Save();

			var entry1 = NewSaveableEntry(clientRate);
			entry1[RateEntrySchema.TI_RateCategory] = "AIR";
			entry1[RateEntrySchema.TI_Mode] = "LSE";
			entry1[RateEntrySchema.TI_OriginLRC] = "CN";
			entry1.Save();

			var entry2 = NewSaveableEntry(clientRate);
			entry2[RateEntrySchema.TI_RateCategory] = "AIR";
			entry2[RateEntrySchema.TI_Mode] = "LSE";
			entry2[RateEntrySchema.TI_OriginLRC] = "CN";

			AssertDuplicateRateEntryExpectionWasThrown(entry2);
		}

		public void TestAddingNewCostingEntry_Duplicate_ThrowsException()
		{
			var costing = new ActiveRowWrapper(RatingHeaderSchema.Instance);
			costing[RatingHeaderSchema.TH_RateType] = "COS";
			costing[RatingHeaderSchema.TH_GC] = GlbCompanyPK;
			costing[RatingHeaderSchema.TH_OH] = new Guid(orgPK1);
			costing.Save();

			var entry1 = NewSaveableEntry(costing);
			entry1[RateEntrySchema.TI_RateCategory] = "AIR";
			entry1[RateEntrySchema.TI_Mode] = "LSE";
			entry1[RateEntrySchema.TI_OriginLRC] = "CN";
			entry1.Save();

			var entry2 = NewSaveableEntry(costing);
			entry2[RateEntrySchema.TI_RateCategory] = "AIR";
			entry2[RateEntrySchema.TI_Mode] = "LSE";
			entry2[RateEntrySchema.TI_OriginLRC] = "CN";

			AssertDuplicateRateEntryExpectionWasThrown(entry2);
		}

		public void TestAddingNewStandardCostingEntry_Duplicate_ThrowsException()
		{
			var costing = new ActiveRowWrapper(RatingHeaderSchema.Instance);
			costing[RatingHeaderSchema.TH_RateType] = "COS";
			costing[RatingHeaderSchema.TH_GC] = GlbCompanyPK;
			costing.Save();

			var entry1 = NewSaveableEntry(costing);
			entry1[RateEntrySchema.TI_RateCategory] = "AIR";
			entry1[RateEntrySchema.TI_Mode] = "LSE";
			entry1[RateEntrySchema.TI_OriginLRC] = "CN";
			entry1.Save();

			var entry2 = NewSaveableEntry(costing);
			entry2[RateEntrySchema.TI_RateCategory] = "AIR";
			entry2[RateEntrySchema.TI_Mode] = "LSE";
			entry2[RateEntrySchema.TI_OriginLRC] = "CN";

			AssertDuplicateRateEntryExpectionWasThrown(entry2);
		}

		public void TestAddingCompanyTariffEntry_Duplicate_ThrowsException()
		{
			var companyTariff = new ActiveRowWrapper(RatingHeaderSchema.Instance);
			companyTariff[RatingHeaderSchema.TH_RateType] = "GLB";
			companyTariff[RatingHeaderSchema.TH_GlobalRateDescription] = "Base Company Tariff";
			companyTariff[RatingHeaderSchema.TH_GC] = GlbCompanyPK;
			companyTariff.Save();

			var entry1 = NewSaveableEntry(companyTariff);
			entry1[RateEntrySchema.TI_RateCategory] = "WHS";
			entry1[RateEntrySchema.TI_Mode] = "ALL";
			entry1.Save();

			var entry2 = NewSaveableEntry(companyTariff);
			entry2[RateEntrySchema.TI_RateCategory] = "WHS";
			entry2[RateEntrySchema.TI_Mode] = "ALL";

			AssertDuplicateRateEntryExpectionWasThrown(entry2);
		}

		public void TestAddingQuotationEntry_Duplicate_ThrowsException()
		{
			var date = DateTime.Today;
			var quotation = new ActiveRowWrapper(RatingHeaderSchema.Instance);
			quotation[RatingHeaderSchema.TH_RateType] = "QTE";
			quotation[RatingHeaderSchema.TH_QuoteDate] = date;
			quotation[RatingHeaderSchema.TH_QuoteNumber] = "QUOTE209375";
			quotation[RatingHeaderSchema.TH_GC] = GlbCompanyPK;
			quotation.Save();

			var entry1 = NewSaveableEntry(quotation);
			entry1[RateEntrySchema.TI_RateCategory] = "DST";
			entry1[RateEntrySchema.TI_Mode] = "ROA";
			entry1[RateEntrySchema.TI_DestinationLRC] = "AU";
			entry1.Save();

			var entry2 = NewSaveableEntry(quotation);
			entry2[RateEntrySchema.TI_RateCategory] = "DST";
			entry2[RateEntrySchema.TI_Mode] = "ROA";
			entry2[RateEntrySchema.TI_DestinationLRC] = "AU";

			AssertDuplicateRateEntryExpectionWasThrown(entry2);
		}

		public void TestAddingEntryFromNoCompany_Duplicate_ThrowsException()
		{
			var clientRate = NewRatingHeader1();
			clientRate.Save();

			var entry1 = new ActiveRowWrapper(RateEntrySchema.Instance)
			{
				[RateEntrySchema.TI_TH] = clientRate.PK,
				[RateEntrySchema.TI_GC_Publisher] = GlbCompanyPK,
				[RateEntrySchema.TI_RateStartDate] = new DateTime(2012, 01, 10),
				[RateEntrySchema.TI_RateCategory] = "WHS",
				[RateEntrySchema.TI_Mode] = "ALL"
			};
			entry1.Save();

			var entry2 = new ActiveRowWrapper(RateEntrySchema.Instance)
			{
				[RateEntrySchema.TI_TH] = clientRate.PK,
				[RateEntrySchema.TI_GC_Publisher] = GlbCompanyPK,
				[RateEntrySchema.TI_RateStartDate] = new DateTime(2012, 01, 10),
				[RateEntrySchema.TI_RateCategory] = "WHS",
				[RateEntrySchema.TI_Mode] = "ALL"
			};

			AssertDuplicateRateEntryExpectionWasThrown(entry2);
		}

		void AssertDuplicateRateEntryExpectionWasThrown(ActiveRowWrapper entry2)
		{
			var expectedMessage = $"{entry2.PK.ToString().ToUpper()}";

			AssertExceptionThrown(typeof(SqlException), expectedMessage, () => entry2.Save());
		}

		public void TestInsertMultipleEntries_WhenTheyHaveOverlappingExistingEntries_ThenShouldThrowExceptionWithConcatenatedPKs()
		{
			var publisher = new ActiveRowWrapper(GlbCompanySchema.Instance)
			{
				[GlbCompanySchema.GC_Code] = "CO1",
				[GlbCompanySchema.GC_Name] = "Company 1",
				[GlbCompanySchema.GC_RN_NKCountryCode] = "AU",
				[GlbCompanySchema.GC_RX_NKLocalCurrency] = "AUD"
			};

			var rate = new ActiveRowWrapper(RatingHeaderSchema.Instance)
			{
				[RatingHeaderSchema.TH_RateType] = "COS",
			};

			publisher.Save();
			rate.Save();

			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();
			var guid3 = Guid.NewGuid();
			var guid4 = Guid.NewGuid();
			var guid5 = Guid.NewGuid();

			var sql = $@"
INSERT INTO dbo.RateEntry (TI_PK, TI_TH, TI_GC_Publisher, TI_RateStartDate, TI_RateEndDate, TI_RateCategory, TI_Mode, TI_OriginLRC, TI_DestinationLRC, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
VALUES
	('{guid1}', '{rate.PK}', '{publisher.PK}', '2012-01-05', '2012-01-10', 'AIR', 'LSE', 'AU', 'NZ', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{guid2}', '{rate.PK}', '{publisher.PK}', '2012-01-11', '2012-01-20', 'AIR', 'LSE', 'AU', 'NZ', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}

			sql = $@"
INSERT INTO dbo.RateEntry (TI_PK, TI_TH, TI_GC_Publisher, TI_RateStartDate, TI_RateEndDate, TI_RateCategory, TI_Mode, TI_OriginLRC, TI_DestinationLRC, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
VALUES
	('{guid3}', '{rate.PK}', '{publisher.PK}', '2012-01-01', '2012-01-08', 'AIR', 'LSE', 'AU', 'NZ', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{guid4}', '{rate.PK}', '{publisher.PK}', '2012-01-06', '2012-01-15', 'AIR', 'LSE', 'AU', 'NZ', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{guid5}', '{rate.PK}', '{publisher.PK}', '2012-01-12', '2012-01-25', 'AIR', 'LSE', 'AU', 'NZ', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				var exception = AssertExceptionThrown<SqlException>(() => command.ExecuteNonQuery());
				var exceptionMessage = exception.Message;
				AssertEquals("Print 3 entries PK, the message length is the length of 3 PKs and 2 symbols", Guid.Empty.ToString().Length * 3 + 2, exceptionMessage.Length);

				CombineAssertions("All PKs from the new entries which overlap with existing ones", () =>
				{
					AssertContains("PK from entry 3", guid3.ToString().ToUpper(), exceptionMessage);
					AssertContains("PK from entry 4", guid4.ToString().ToUpper(), exceptionMessage);
					AssertContains("PK from entry 5", guid5.ToString().ToUpper(), exceptionMessage);
				});
			}
		}

		public void TestUpdateOverlappingEntries_WhenTheyHaveOverlappingExistingEntries_ThenShouldThrowExceptionWithConcatenatedPKs()
		{
			var publisher = new ActiveRowWrapper(GlbCompanySchema.Instance)
			{
				[GlbCompanySchema.GC_Code] = "CO1",
				[GlbCompanySchema.GC_Name] = "Company 1",
				[GlbCompanySchema.GC_RN_NKCountryCode] = "AU",
				[GlbCompanySchema.GC_RX_NKLocalCurrency] = "AUD"
			};

			var rate = new ActiveRowWrapper(RatingHeaderSchema.Instance)
			{
				[RatingHeaderSchema.TH_RateType] = "COS",
			};

			publisher.Save();
			rate.Save();

			var guid1 = Guid.NewGuid();
			var guid2 = Guid.NewGuid();
			var guid3 = Guid.NewGuid();
			var guid4 = Guid.NewGuid();
			var guid5 = Guid.NewGuid();

			var sql = $@"
INSERT INTO dbo.RateEntry (TI_PK, TI_TH, TI_GC_Publisher, TI_RateStartDate, TI_RateEndDate, TI_RateCategory, TI_Mode, TI_OriginLRC, TI_DestinationLRC, TI_SystemLastEditTimeUtc, TI_SystemLastEditUser, TI_SystemCreateTimeUtc, TI_SystemCreateUser)
VALUES
	('{guid1}', '{rate.PK}', '{publisher.PK}', '2012-01-05', '2012-01-10', 'AIR', 'LSE', 'AU', 'NZ', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{guid2}', '{rate.PK}', '{publisher.PK}', '2012-01-11', '2012-01-20', 'AIR', 'LSE', 'AU', 'NZ', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{guid3}', '{rate.PK}', '{publisher.PK}', '2013-01-11', '2013-01-20', 'AIR', 'LSE', 'AU', 'NZ', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{guid4}', '{rate.PK}', '{publisher.PK}', '2014-01-11', '2014-01-20', 'AIR', 'LSE', 'AU', 'NZ', GetUtcDate(), '~BP', GetUtcDate(), '~BP'),
	('{guid5}', '{rate.PK}', '{publisher.PK}', '2015-01-11', '2015-01-20', 'AIR', 'LSE', 'AU', 'NZ', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
";
			using (var command = Db.Connection.Command(sql))
			{
				AssertNoExceptionThrown(() => command.ExecuteNonQuery());
			}

			sql = $@"
UPDATE dbo.RateEntry
SET 
    TI_RateStartDate = CASE 
        WHEN TI_PK = '{guid3}' THEN '2012-01-01'
        WHEN TI_PK = '{guid4}' THEN '2012-01-06'
        WHEN TI_PK = '{guid5}' THEN '2012-01-12'
    END,
    TI_RateEndDate = CASE 
        WHEN TI_PK = '{guid3}' THEN '2012-01-08'
        WHEN TI_PK = '{guid4}' THEN '2012-01-15'
        WHEN TI_PK = '{guid5}' THEN '2012-01-25'
    END
WHERE TI_PK IN ('{guid3}', '{guid4}', '{guid5}');
";
			using (var command = Db.Connection.Command(sql))
			{
				var exception = AssertExceptionThrown<SqlException>(() => command.ExecuteNonQuery());
				var exceptionMessage = exception.Message;
				AssertEquals("Print 3 entries PK, the message length is the length of 3 PKs and 2 symbols", Guid.Empty.ToString().Length * 3 + 2, exceptionMessage.Length);

				CombineAssertions("All PKs from the new entries which overlap with existing ones", () =>
				{
					AssertContains("PK from entry 3", guid3.ToString().ToUpper(), exceptionMessage);
					AssertContains("PK from entry 4", guid4.ToString().ToUpper(), exceptionMessage);
					AssertContains("PK from entry 5", guid5.ToString().ToUpper(), exceptionMessage);
				});
			}
		}

		#region Implementation

		void AssertNonKeyColumnDoesntMakeDuplicateUnique(SchemaColumn column)
		{
			var ratingHeader = NewRatingHeader1();
			ratingHeader[RatingHeaderSchema.TH_GC] = GlbCompanyPK;
			ratingHeader.Save();

			var entry1 = NewSaveableEntry(ratingHeader);
			var entry2 = NewSaveableEntry(ratingHeader);

			entry1[column] = GetNonDefaultValue1(column);
			entry2[column] = GetNonDefaultValue2(column);

			AssertOverlaps(entry1, entry2);
			Clear();
		}

		void AssertKeyColumnMakesDuplicateUnique(SchemaColumn column)
		{
			var ratingHeader = NewRatingHeader1();
			ratingHeader[RatingHeaderSchema.TH_GC] = GlbCompanyPK;
			ratingHeader.Save();

			var entry1 = NewSaveableEntry(ratingHeader);
			var entry2 = NewSaveableEntry(ratingHeader);

			if (column == RateEntrySchema.TI_TH)
			{
				var ratingHeader2 = NewRatingHeader2();
				ratingHeader2[RatingHeaderSchema.TH_GC] = GlbCompanyPK;
				ratingHeader2.Save();

				entry2[RateEntrySchema.TI_TH] = ratingHeader2.PK;
			}
			else
			{
				entry1[column] = GetNonDefaultValue1(column);
				entry2[column] = GetNonDefaultValue2(column);
			}

			if (column == RateEntrySchema.TI_ParentID)
			{
				entry1[RateEntrySchema.TI_ParentTableCode] = "WW";
				entry2[RateEntrySchema.TI_ParentTableCode] = "WW";
			}

			AssertDoesntOverlap($"Failed column: {column.Name}", entry1, entry2);
			Clear();
		}

		void AssertDateOverlap(bool overlaps, DateTime startDate1, DateTime? endData1, DateTime startDate2, DateTime? endDate2)
		{
			var ratingHeader = new ActiveRowWrapper(RatingHeaderSchema.Instance);
			ratingHeader[RatingHeaderSchema.TH_GC] = GlbCompanyPK;
			ratingHeader[RatingHeaderSchema.TH_RateType] = "QTE";
			ratingHeader[RatingHeaderSchema.TH_QuoteDate] = startDate1;
			ratingHeader[RatingHeaderSchema.TH_QuoteNumber] = startDate1.ToShortDateString();
			ratingHeader.Save();

			var entry1 = NewSaveableEntry(ratingHeader);
			var entry2 = NewSaveableEntry(ratingHeader);

			entry1[RateEntrySchema.TI_RateStartDate] = startDate1;

			if (endData1.HasValue)
			{
				entry1[RateEntrySchema.TI_RateEndDate] = endData1;
			}

			if (endDate2.HasValue)
			{
				entry2[RateEntrySchema.TI_RateEndDate] = endDate2;
			}

			entry2[RateEntrySchema.TI_RateStartDate] = startDate2;

			if (overlaps)
			{
				AssertOverlaps(entry1, entry2);
			}
			else
			{
				AssertDoesntOverlap("Should not overlap", entry1, entry2);
			}

			Clear();
		}

		object GetNonDefaultValue1(SchemaColumn column)
		{
			switch (column.ColumnType)
			{
				case SchemaColumnType.Short:
					return Convert.ToInt16(3);

				case SchemaColumnType.Int:
					return 8;

				case SchemaColumnType.String:
					if (column.Name.StartsWith("TI_RateCategory"))
					{
						return "DST";
					}
					if (column.Name.StartsWith("TI_Mode"))
					{
						return "LSE";
					}
					if (column.Name.StartsWith("TI_PaymentTerm"))
					{
						return "PPD";
					}
					if (column.Name.StartsWith("TI_GatewayAgentType"))
					{
						return "RAG";
					}
					if (column.Name.StartsWith("TI_RX_NKCurrency"))
					{
						return "UAH";
					}
					if (column.Name.StartsWith("TI_AircraftType"))
					{
						return "CAO";
					}
					if (column.Name.StartsWith("TI_ShipmentConsolidationStatus"))
					{
						return "STS";
					}
					if (column.Name.StartsWith("TI_CreationSource"))
					{
						return "ADW";
					}
					if (column.Name.StartsWith("TI_IsNonOperatedReefer"))
					{
						return "Y";
					}
					if (column.Name.StartsWith("TI_YardUnitType"))
					{
						return "CNT";
					}
					if (column.Name.StartsWith("TI_YardUnitLoad"))
					{
						return "EMP";
					}
					if (column.Name.StartsWith("TI_EstimateType"))
					{
						return "STL";
					}
					if (column.Name.StartsWith("TI_MNRGroup"))
					{
						return "CEDEX";
					}

					if (column == RateEntrySchema.TI_ParentTableCode)
					{
						return WhsWarehouseSchema.Constants.Prefix;
					}

					return "AAA";

				case SchemaColumnType.Decimal:
					return 5m;

				case SchemaColumnType.Bool:
					return true;

				case SchemaColumnType.Guid:
					if (column.Name.StartsWith("TI_OH"))
					{
						return new Guid(orgPK1);
					}

					if (column.Name.StartsWith("TI_OA"))
					{
						return new Guid(addressPK1);
					}

					if (column.Name.StartsWith("TI_RRC_RepairCode"))
					{
						var repairCode = new ActiveRowWrapper(RefRepairCodeSchema.Instance);
						repairCode[RefRepairCodeSchema.RRC_Group] = "CEDEX";
						repairCode[RefRepairCodeSchema.RRC_Code] = "123";
						repairCode[RefRepairCodeSchema.RRC_Description] = "CEDEX group";
						repairCode[RefRepairCodeSchema.RRC_ServiceType] = "RPR";
						repairCode.Save();
						return repairCode.PK;
					}

					if (column.Name.StartsWith("TI_RCC_ComponentCode"))
					{
						var componentCode = new ActiveRowWrapper(RefMRComponentCodeSchema.Instance);
						componentCode[RefMRComponentCodeSchema.RCC_Group] = "CEDEX";
						componentCode[RefMRComponentCodeSchema.RCC_Code] = "123";
						componentCode[RefMRComponentCodeSchema.RCC_Description] = "CEDEX group";
						componentCode.Save();
						return componentCode.PK;
					}

					if (column.Name.StartsWith("TI_RMC_Material"))
					{
						var materialCode = new ActiveRowWrapper(RefMaterialSchema.Instance);
						materialCode[RefMaterialSchema.RMC_Group] = "CEDEX";
						materialCode[RefMaterialSchema.RMC_Code] = "123";
						materialCode[RefMaterialSchema.RMC_Description] = "CEDEX group";
						materialCode.Save();
						return materialCode.PK;
					}

					if (column.Name.StartsWith("TI_REG_EquipmentGrade"))
					{
						var equipmentGrade = new ActiveRowWrapper(RefEquipmentGradeSchema.Instance);
						equipmentGrade[RefEquipmentGradeSchema.REG_Code] = "AMO";
						equipmentGrade[RefEquipmentGradeSchema.REG_Description] = "AMO Grade";
						equipmentGrade.Save();
						return equipmentGrade.PK;
					}

					if (column.Name.StartsWith("TI_RC"))
					{
						return new Guid(containerPK1);
					}

					if (column.Name.StartsWith("TI_TZ"))
					{
						return NewZone(new Guid(orgPK2), "Zone1").PK;
					}

					if (column == RateEntrySchema.TI_GC_Publisher)
					{
						var newCompany = new ActiveRowWrapper(GlbCompanySchema.Instance)
						{
							[GlbCompanySchema.GC_Code] = "ABC",
							[GlbCompanySchema.GC_Name] = "Company = ABC",
							[GlbCompanySchema.GC_RN_NKCountryCode] = "AU",
							[GlbCompanySchema.GC_RX_NKLocalCurrency] = "AUD"
						};
						newCompany.Save();

						return newCompany.PK;
					}

					if (column == RateEntrySchema.TI_ParentID)
					{
						return NewWarehouse(addressPK1, "AAA", "FTZ").PK;
					}

					if (column == RateEntrySchema.TI_R9_FromSuburb || column == RateEntrySchema.TI_R9_ToSuburb)
					{
						return new Guid(suburbPK1);
					}

					break;
			}

			throw new NotImplementedException($"Cannot provide value for {column.ColumnType} {column.Name}");
		}

		object GetNonDefaultValue2(SchemaColumn column)
		{
			switch (column.ColumnType)
			{
				case SchemaColumnType.Short:
					return Convert.ToInt16(4);

				case SchemaColumnType.Int:
					return 9;

				case SchemaColumnType.String:
					if (column.Name.StartsWith("TI_RateCategory"))
					{
						return "ORG";
					}
					if (column.Name.StartsWith("TI_Mode"))
					{
						return "LCL";
					}
					if (column.Name.StartsWith("TI_PaymentTerm"))
					{
						return "CCX";
					}
					if (column.Name.StartsWith("TI_GatewayAgentType"))
					{
						return "SAG";
					}
					if (column.Name.StartsWith("TI_AircraftType"))
					{
						return "PAX";
					}
					if (column.Name.StartsWith("TI_ShipmentConsolidationStatus"))
					{
						return "CNS";
					}
					if (column.Name.StartsWith("TI_IsNonOperatedReefer"))
					{
						return "";
					}
					if (column.Name.StartsWith("TI_YardUnitType"))
					{
						return "CHS";
					}
					if (column.Name.StartsWith("TI_YardUnitLoad"))
					{
						return "LAD";
					}
					if (column.Name.StartsWith("TI_EstimateType"))
					{
						return "MAC";
					}
					if (column.Name.StartsWith("TI_MNRGroup"))
					{
						return "MERC";
					}

					if (column == RateEntrySchema.TI_ParentTableCode)
					{
						return "";
					}

					return "BBB";

				case SchemaColumnType.Decimal:
					return 6m;

				case SchemaColumnType.Bool:
					return false;

				case SchemaColumnType.Guid:
					if (column.Name.StartsWith("TI_OH"))
					{
						return new Guid(orgPK2);
					}

					if (column.Name.StartsWith("TI_OA"))
					{
						return new Guid(addressPK2);
					}

					if (column.Name.StartsWith("TI_RRC_RepairCode"))
					{
						var repairCode = new ActiveRowWrapper(RefRepairCodeSchema.Instance);
						repairCode[RefRepairCodeSchema.RRC_Group] = "CEDEX";
						repairCode[RefRepairCodeSchema.RRC_Code] = "456";
						repairCode[RefRepairCodeSchema.RRC_Description] = "CEDEX group";
						repairCode[RefRepairCodeSchema.RRC_ServiceType] = "RPR";
						repairCode.Save();
						return repairCode.PK;
					}

					if (column.Name.StartsWith("TI_RCC_ComponentCode"))
					{
						var componentCode = new ActiveRowWrapper(RefMRComponentCodeSchema.Instance);
						componentCode[RefMRComponentCodeSchema.RCC_Group] = "CEDEX";
						componentCode[RefMRComponentCodeSchema.RCC_Code] = "456";
						componentCode[RefMRComponentCodeSchema.RCC_Description] = "CEDEX group";
						componentCode.Save();
						return componentCode.PK;
					}

					if (column.Name.StartsWith("TI_RMC_Material"))
					{
						var materialCode = new ActiveRowWrapper(RefMaterialSchema.Instance);
						materialCode[RefMaterialSchema.RMC_Group] = "CEDEX";
						materialCode[RefMaterialSchema.RMC_Code] = "456";
						materialCode[RefMaterialSchema.RMC_Description] = "CEDEX group";
						materialCode.Save();
						return materialCode.PK;
					}

					if (column.Name.StartsWith("TI_REG_EquipmentGrade"))
					{
						var equipmentGrade = new ActiveRowWrapper(RefEquipmentGradeSchema.Instance);
						equipmentGrade[RefEquipmentGradeSchema.REG_Code] = "COT";
						equipmentGrade[RefEquipmentGradeSchema.REG_Description] = "Cotton Quality";
						equipmentGrade.Save();
						return equipmentGrade.PK;
					}

					if (column.Name.StartsWith("TI_RC"))
					{
						return new Guid(containerPK2);
					}

					if (column.Name.StartsWith("TI_TZ"))
					{
						return NewZone(new Guid(orgPK1), "Zone2").PK;
					}

					if (column == RateEntrySchema.TI_GC_Publisher)
					{
						var newCompany = new ActiveRowWrapper(GlbCompanySchema.Instance)
						{
							[GlbCompanySchema.GC_Code] = "123",
							[GlbCompanySchema.GC_Name] = "Company = 123",
							[GlbCompanySchema.GC_RN_NKCountryCode] = "AU",
							[GlbCompanySchema.GC_RX_NKLocalCurrency] = "AUD"
						};
						newCompany.Save();

						return newCompany.PK;
					}

					if (column == RateEntrySchema.TI_ParentID)
					{
						return NewWarehouse(addressPK2, "BBB", "TRW").PK;
					}

					if (column == RateEntrySchema.TI_R9_FromSuburb || column == RateEntrySchema.TI_R9_ToSuburb)
					{
						return new Guid(suburbPK2);
					}

					break;
			}

			throw new NotImplementedException($"Cannot provide value for {column.ColumnType} {column.Name}");
		}

		void AssertOverlaps(ActiveRowWrapper entry1, ActiveRowWrapper entry2)
		{
			entry1.Save();
			AssertExceptionThrown<SqlException>(() => entry2.Save());
		}

		void AssertDoesntOverlap(string extraMessage, ActiveRowWrapper entry1, ActiveRowWrapper entry2)
		{
			entry1.Save();
			AssertEquals($"Non-duplicate RateEntry inserted. {extraMessage}", 1, entry2.Save());
		}

		void Clear()
		{
			Db.Connection.BeginTransaction();
			using (var command = Db.Connection.Command(@"
DELETE FROM dbo.RatingHeader
DELETE FROM dbo.RateEntry
DELETE FROM dbo.RateTransportZones
DELETE FROM dbo.RateTransportProvider
DELETE FROM dbo.WhsWarehouse"))
			{
				command.ExecuteNonQuery();
			}
			Db.Connection.CommitTransaction();
		}

		ActiveRowWrapper NewSaveableEntry(ActiveRowWrapper ratingHeader) => new ActiveRowWrapper(RateEntrySchema.Instance)
		{
			[RateEntrySchema.TI_TH] = ratingHeader.PK,
			[RateEntrySchema.TI_GC_Publisher] = ratingHeader[RatingHeaderSchema.TH_GC],
			[RateEntrySchema.TI_RateStartDate] = new DateTime(2012, 01, 10),
			[RateEntrySchema.TI_RateCategory] = "ORG",
			[RateEntrySchema.TI_Mode] = "LCL"
		};

		// Note: below guid strings are prefilled data in test DB.
		const string containerPK1 = "109C3436-298A-4780-B64D-52DC719F0E15";
		const string containerPK2 = "4698C669-01D2-48C4-B15A-5635C6D1A913";

		const string addressPK1 = "6507E0BB-A9FF-43CF-8C46-D6EB9016A767";
		const string addressPK2 = "D065E495-0F9B-4AD8-A92B-4AD2F7D3A41A";

		const string orgPK1 = "BAFA4CEF-6863-4BB4-98CC-2444FE7E55DE";
		const string orgPK2 = "19216855-4F6F-4855-AB81-7835E83CB7F2";

		const string suburbPK1 = "79C3FC2D-4238-4BF7-A760-A9A224BCCADD";
		const string suburbPK2 = "6417EBD2-5615-4DA8-8621-524A1800F7FB";

		static ActiveRowWrapper NewWarehouse(string addressPK, string code, string type)
		{
			var warehouse = new ActiveRowWrapper(WhsWarehouseSchema.Instance)
			{
				[WhsWarehouseSchema.WW_OA_WarehouseAddress] = new Guid(addressPK),
				[WhsWarehouseSchema.WW_GB_RelatedCompanyBranch] = new Guid("54226AD9-9E8A-4E29-A7F7-E73A7D18DDF1"),
				[WhsWarehouseSchema.WW_IsVirtualWarehouse] = (type != "TRW"),
				[WhsWarehouseSchema.WW_WarehouseCode] = code,
				[WhsWarehouseSchema.WW_WarehouseType] = type,
				[WhsWarehouseSchema.WW_WLT_DefaultLocationType] = new Guid("16C9FD62-730A-42ED-A20E-699606FFF360")
			};

			warehouse.Save();
			return warehouse;
		}

		static ActiveRowWrapper NewZone(Guid ownerPK, string zoneName)
		{
			var set = new ActiveRowWrapper(RateTransportProviderSchema.Instance)
			{
				[RateTransportProviderSchema.TP_OH_RelatedParty] = ownerPK,
				[RateTransportProviderSchema.TP_RN_NKCountry] = "AU"
			};

			set.Save();
			var zone = new ActiveRowWrapper(RateTransportZonesSchema.Instance)
			{
				[RateTransportZonesSchema.TZ_TP] = set.PK,
				[RateTransportZonesSchema.TZ_ZoneName] = zoneName
			};

			zone.Save();
			return zone;
		}

		Guid GlbCompanyPK
		{
			get
			{
				if (glbCompanyPK == Guid.Empty)
				{
					glbCompanyPK = (Guid)TestConnection.ExecuteScalar("SELECT TOP 1 GC_PK FROM dbo.GlbCompany");
				}

				return glbCompanyPK;
			}
		}

		Guid glbCompanyPK;

		static ActiveRowWrapper NewRatingHeader1() => new ActiveRowWrapper(RatingHeaderSchema.Instance)
		{
			[RatingHeaderSchema.TH_RateType] = "SAL",
			[RatingHeaderSchema.TH_OH] = new Guid(orgPK1)
		};

		static ActiveRowWrapper NewRatingHeader2() => new ActiveRowWrapper(RatingHeaderSchema.Instance)
		{
			[RatingHeaderSchema.TH_RateType] = "SAL",
			[RatingHeaderSchema.TH_OH] = new Guid(orgPK2)
		};

		readonly SchemaColumn[] keyColumns =
		{
				RateEntrySchema.TI_RateCategory,
				RateEntrySchema.TI_Mode,
				RateEntrySchema.TI_OriginLRC,
				RateEntrySchema.TI_RateOrigin,
				RateEntrySchema.TI_DestinationLRC,
				RateEntrySchema.TI_RateDestination,
				RateEntrySchema.TI_AircraftType,
				RateEntrySchema.TI_ViaLRC,
				RateEntrySchema.TI_PlannedLoadLRC,
				RateEntrySchema.TI_PlannedDischargeLRC,
				RateEntrySchema.TI_FirstLoadLRC,
				RateEntrySchema.TI_LastDischargeLRC,
				RateEntrySchema.TI_FirstRouteSetLoadPortLRC,
				RateEntrySchema.TI_LastRouteSetDischargePortLRC,
				RateEntrySchema.TI_RS_NKServiceLevel_NI,
				RateEntrySchema.TI_PL_NKCarrierServiceLevel,
				RateEntrySchema.TI_RS_NKGatewayServiceLevel,
				RateEntrySchema.TI_RS_NKShipmentGatewayServiceLevel,
				RateEntrySchema.TI_RH_NKCommodityCode,
				RateEntrySchema.TI_RCC_ComponentCode,
				RateEntrySchema.TI_ContainerUnitSection,
				RateEntrySchema.TI_RMC_Material,
				RateEntrySchema.TI_RRC_RepairCode,
				RateEntrySchema.TI_EstimateType,
				RateEntrySchema.TI_REG_EquipmentGrade,
				RateEntrySchema.TI_MNRGroup,
				RateEntrySchema.TI_FMCTariffID,
				RateEntrySchema.TI_CartagePickupAddressPostCode,
				RateEntrySchema.TI_CartageDeliveryAddressPostCode,
				RateEntrySchema.TI_TransitTime,
				RateEntrySchema.TI_PaymentTerm,
				RateEntrySchema.TI_GatewayAgentType,
				RateEntrySchema.TI_Frequency,
				RateEntrySchema.TI_FrequencyUnit,
				RateEntrySchema.TI_IsCrossTrade,
				RateEntrySchema.TI_IsTact,
				RateEntrySchema.TI_MatchContainerRateClass,
				RateEntrySchema.TI_ContractNumber,
				RateEntrySchema.TI_ShipmentConsolidationStatus,
				RateEntrySchema.TI_HBLDeliveryMode,
				RateEntrySchema.TI_IsNonOperatedReefer,
				RateEntrySchema.TI_YardUnitType,
				RateEntrySchema.TI_YardUnitLoad,

				RateEntrySchema.TI_TH,

				// nullables
				RateEntrySchema.TI_OH_TransportProvider,
				RateEntrySchema.TI_OH_Supplier,
				RateEntrySchema.TI_OH_Consignor,
				RateEntrySchema.TI_OH_Consignee,
				RateEntrySchema.TI_OH_ControllingCustomer,
				RateEntrySchema.TI_OA_CartagePickupAddressOverride,
				RateEntrySchema.TI_OA_CartageDeliveryAddressOverride,
				RateEntrySchema.TI_ParentID,
				RateEntrySchema.TI_TZ_OriginZone,
				RateEntrySchema.TI_TZ_DestinationZone,
				RateEntrySchema.TI_R9_FromSuburb,
				RateEntrySchema.TI_R9_ToSuburb,
				RateEntrySchema.TI_RC
			};

		readonly SchemaColumn[] systemColumns =
		{
				RateEntrySchema.TI_SystemCreateTimeUtc,
				RateEntrySchema.TI_SystemLastEditTimeUtc,
				RateEntrySchema.TI_SystemCreateUser,
				RateEntrySchema.TI_SystemLastEditUser,
				RateEntrySchema.TI_DataChecked,
				RateEntrySchema.TI_IsValid,
			};

		readonly SchemaColumn[] nonKeyColumns =
		{
				RateEntrySchema.TI_CreationSource,
				RateEntrySchema.TI_LineOrder,
				RateEntrySchema.TI_PageClosingText,
				RateEntrySchema.TI_PageHeading,
				RateEntrySchema.TI_PageOpeningText,
				RateEntrySchema.TI_BuyersConsolRateMode,
				RateEntrySchema.TI_OH_AgentOverride,
				RateEntrySchema.TI_QuotePageIncoTerm,
				RateEntrySchema.TI_RX_NKCurrency,
				RateEntrySchema.TI_GC_Publisher,
				RateEntrySchema.TI_ParentTableCode,
				RateEntrySchema.TI_IsExcludedFromAutoRating,
				RateEntrySchema.TI_ContractNumberLinked,
				RateEntrySchema.TI_ProviderReferenceID,
			};

		readonly SchemaColumn[] datesAndPkColumns =
		{
				RateEntrySchema.TI_RateStartDate,
				RateEntrySchema.TI_RateEndDate,
				RateEntrySchema.PK
			};
		#endregion
	}
}
