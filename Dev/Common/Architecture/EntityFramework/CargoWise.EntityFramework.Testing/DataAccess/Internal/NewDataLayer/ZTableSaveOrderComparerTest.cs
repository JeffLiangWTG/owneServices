using System;
using CargoWise.Schema;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static CargoWise.EntityFramework.Testing.ZSaverTest;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class ZTableSaveOrderComparerTest : TestCase
	{
		public void TestSort()
		{
			AssertSortOrder(
					new ITableSchema[]
				{
					StmNoteSchema.Instance,
					GlbCompanySchema.Instance,
					GlbBranchSchema.Instance,
					GlbDepartmentSchema.Instance,
					AccTransactionHeaderSchema.Instance,
					JobConsolTransportSchema.Instance,
					JobShipmentSchema.Instance,
					JobConsolSchema.Instance,
					JobHeaderSchema.Instance,
					ProcessTasksSchema.Instance,
				},
					new Tuple<ITableSchema, ITableSchema>[]
				{
					new Tuple<ITableSchema, ITableSchema>(GlbCompanySchema.Instance, GlbBranchSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(GlbDepartmentSchema.Instance, JobHeaderSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(GlbBranchSchema.Instance, JobHeaderSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(GlbDepartmentSchema.Instance, ProcessTasksSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(GlbBranchSchema.Instance, ProcessTasksSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobHeaderSchema.Instance, AccTransactionHeaderSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(GlbDepartmentSchema.Instance, JobConsolTransportSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(GlbBranchSchema.Instance, JobConsolTransportSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(GlbCompanySchema.Instance, StmNoteSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(AccTransactionHeaderSchema.Instance, StmNoteSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobConsolTransportSchema.Instance, StmNoteSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobShipmentSchema.Instance, StmNoteSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobConsolSchema.Instance, StmNoteSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobHeaderSchema.Instance, StmNoteSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(ProcessTasksSchema.Instance, StmNoteSchema.Instance),
				}
			);
		}

		public void TestSort2()
		{
			AssertSortOrder(
					new ITableSchema[]
				{
					JobConsolSchema.Instance,
					JobConShipLinkSchema.Instance,
					JobConsolTransportSchema.Instance,
				},
					new ITableSchema[]
				{
					JobConsolSchema.Instance,
					JobConShipLinkSchema.Instance,
					JobConsolTransportSchema.Instance,
				});
		}

		public void TestCompare()
		{
			ZTableSaveOrderComparer.CleanSaveOrderCacheForTest();

			AssertEquals(0, new ZTableSaveOrderComparer(Array.Empty<ITableSchema>()).Compare(JobConsolTransportSchema.Instance, JobDocsAndCartageSchema.Instance));
			AssertEquals(-1, new ZTableSaveOrderComparer(Array.Empty<ITableSchema>()).Compare(OrgHeaderSchema.Instance, OrgAddressSchema.Instance));
			AssertEquals(-1, new ZTableSaveOrderComparer(Array.Empty<ITableSchema>()).Compare(OrgAddressSchema.Instance, OrgAddressCapabilitySchema.Instance));
			AssertEquals(-1, new ZTableSaveOrderComparer(Array.Empty<ITableSchema>()).Compare(OrgHeaderSchema.Instance, OrgAddressCapabilitySchema.Instance));
			AssertEquals(1, new ZTableSaveOrderComparer(Array.Empty<ITableSchema>()).Compare(OrgAddressSchema.Instance, OrgHeaderSchema.Instance));
			AssertEquals(1, new ZTableSaveOrderComparer(Array.Empty<ITableSchema>()).Compare(OrgAddressCapabilitySchema.Instance, OrgHeaderSchema.Instance));

			AssertEquals(0, new ZTableSaveOrderComparer(Array.Empty<ITableSchema>()).Compare(JobConsolSchema.Instance, JobShipmentSchema.Instance));
		}

		public void TestCompare_ForRegisteredTables()
		{
			ZTableSaveOrderComparer.CleanSaveOrderCacheForTest();

			AssertEquals(-1, new ZTableSaveOrderComparer(Array.Empty<ITableSchema>()).Compare(AccExchangeRateConfigurationViewSchema.Instance, AccJobConfigPivotSchema.Instance));
			AssertEquals(1, new ZTableSaveOrderComparer(Array.Empty<ITableSchema>()).Compare(AccJobConfigPivotSchema.Instance, AccExchangeRateConfigurationViewSchema.Instance));

			AssertEquals(0, new ZTableSaveOrderComparer(Array.Empty<ITableSchema>()).Compare(AccJobConfigPivotSchema.Instance, TestTable1Schema.Instance));
		}

		public void TestSort3()
		{
			AssertSortOrder(
					new ITableSchema[]
				{
					JobDeclarationSchema.Instance,
					JobContainerSchema.Instance,
					CusDecHouseBillSchema.Instance,
					CusContainerSchema.Instance,
					JobComInvoiceHeaderSchema.Instance,
					ProcessTasksSchema.Instance,
					JobComInvHeaderChargeSchema.Instance,
					JobDocsAndCartageSchema.Instance,
					JobComInvoiceLineSchema.Instance,
				},
					new Tuple<ITableSchema, ITableSchema>[]
				{
					new Tuple<ITableSchema, ITableSchema>(JobDeclarationSchema.Instance, CusContainerSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobDeclarationSchema.Instance, CusDecHouseBillSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobDeclarationSchema.Instance, JobComInvoiceHeaderSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobDeclarationSchema.Instance, JobComInvoiceLineSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobContainerSchema.Instance, CusContainerSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobContainerSchema.Instance, JobComInvoiceLineSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(CusContainerSchema.Instance, JobComInvoiceLineSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(CusDecHouseBillSchema.Instance, JobComInvoiceHeaderSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(CusDecHouseBillSchema.Instance, JobComInvoiceLineSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobComInvoiceHeaderSchema.Instance, JobComInvoiceLineSchema.Instance),
				}
			);
		}

		public void TestSort_ForMasterDetail()
		{
			AssertSortOrder(
					new ITableSchema[]
				{
					JobOrderLineSchema.Instance,
					JobOrderLineDeliverContainerSchema.Instance,
					JobOrderLineDeliverySchema.Instance,
					JobOrderHeaderSchema.Instance,
				},
					new ITableSchema[]
				{
					JobOrderHeaderSchema.Instance,
					JobOrderLineSchema.Instance,
					JobOrderLineDeliverySchema.Instance,
					JobOrderLineDeliverContainerSchema.Instance,
				});
		}

		public void TestSort_ForUnconstrainedParentGuidForeignKey()
		{
			AssertSortOrder(
				new ITableSchema[]
				{
					StmALogSchema.Instance,
					ProcessTasksSchema.Instance,
					JobShipmentSchema.Instance,
					JobConShipLinkSchema.Instance,
					JobConsolSchema.Instance,
				},
				new Tuple<ITableSchema, ITableSchema>[]
				{
					new Tuple<ITableSchema, ITableSchema>(JobConsolSchema.Instance, JobConShipLinkSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobShipmentSchema.Instance, JobConShipLinkSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobConShipLinkSchema.Instance, StmALogSchema.Instance),
					new Tuple<ITableSchema, ITableSchema>(JobConShipLinkSchema.Instance, ProcessTasksSchema.Instance),
				}
			);
		}

		public void TestHasParentGuidColumnSortsProperly()
		{
			ZTableSaveOrderComparer.CleanSaveOrderCacheForTest();
			ZTableSaveOrderComparer.CleanParentGUIDCacheForTest();

			AssertSortOrder(new ITableSchema[] { SchemaWithNormalLengthConstrainedForeignKey.schema, SchemaWithUnconstrainedForeignKey.schema },
				new ITableSchema[] { SchemaWithNormalLengthConstrainedForeignKey.schema, SchemaWithUnconstrainedForeignKey.schema });
			AssertSortOrder(new ITableSchema[] { SchemaWithLongHeaderConstrainedForeignKeyForTest.schema, SchemaWithUnconstrainedForeignKey.schema },
				new ITableSchema[] { SchemaWithLongHeaderConstrainedForeignKeyForTest.schema, SchemaWithUnconstrainedForeignKey.schema });
			AssertSortOrder(new ITableSchema[] { SchemaWithLongTailConstrainedForeignKeyForTest.schema, SchemaWithUnconstrainedForeignKey.schema },
				new ITableSchema[] { SchemaWithLongTailConstrainedForeignKeyForTest.schema, SchemaWithUnconstrainedForeignKey.schema });
			AssertSortOrder(new ITableSchema[] { SchemaWithLongHeaderAndLongTailConstrainedForeignKeyForTest.schema, SchemaWithUnconstrainedForeignKey.schema },
				new ITableSchema[] { SchemaWithLongHeaderAndLongTailConstrainedForeignKeyForTest.schema, SchemaWithUnconstrainedForeignKey.schema });
		}

		#region ZTableSaveOrderComparer

		void AssertSortOrder(ITableSchema[] tablesToBeSaved, ITableSchema[] expectedSaveOrder)
		{
			for (int i = 0; i < 10; i++)
			{
				ITableSchema[] actualSaveOrder = RandomSort(tablesToBeSaved);
				actualSaveOrder = ZTableSaveOrderComparer.Sort(actualSaveOrder);
				AssertArrayEqualsByElements("Sort order correct", expectedSaveOrder, actualSaveOrder);
			}
		}

		void AssertSortOrder(ITableSchema[] tablesToBeSaved, Tuple<ITableSchema, ITableSchema>[] dependencies)
		{
			for (int i = 0; i < 10; i++)
			{
				ITableSchema[] actualSaveOrder = RandomSort(tablesToBeSaved);
				actualSaveOrder = ZTableSaveOrderComparer.Sort(actualSaveOrder);
				foreach (var dependency in dependencies)
				{
					Assert(string.Format("Expected {0} to come before {1}", dependency.Item1.TableName, dependency.Item2.TableName), Array.IndexOf(actualSaveOrder, dependency.Item1) < Array.IndexOf(actualSaveOrder, dependency.Item2));
				}
			}
		}

		T[] RandomSort<T>(T[] array)
		{
			T[] result = (T[])array.Clone();
			Random random = new Random(1);
			int[] keys = new int[array.Length];
			for (int i = 0; i < keys.Length; i++)
			{
				keys[i] = random.Next();
			}
			Array.Sort(keys, result);
			return result;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1023:ImmutableRule", Justification = "Testing")]
		abstract class TestSchemaForForeignKeyFiltering : Schema.Schema, ITableSchema
		{
			#region ITableSchema Members

			public bool AddParameterSuffixForPKReference
			{
				get
				{
					return false;
				}
			}

			public string SqlSchemaName
			{
				get { return "TestTableSQLSchema"; }
			}

			public virtual string TableName
			{
				get { return "TestTableName"; }
			}

			public string DatabaseName
			{
				get { return "TestDB"; }
			}

			public SchemaPKColumn PK
			{
				get { throw new NotSupportedException(); }
			}

			public string PkIndexName => null;

			public virtual SchemaColumn GetSchemaColumn(string columnName)
			{
				throw new NotSupportedException();
			}

			public virtual SchemaColumnCollection All
			{
				get { throw new NotSupportedException(); }
			}

			#endregion

		}

		class SchemaWithUnconstrainedForeignKey : TestSchemaForForeignKeyFiltering
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "This is a static constructor and uses reflection. Is not applicable")]
			static SchemaWithUnconstrainedForeignKey()
			{
				schema = new SchemaWithUnconstrainedForeignKey();
				AB_Parent = new SchemaBoolColumn(schema, "AB_Parent", 0, false, false, false);
			}

			#region ITableSchema Members

			public override SchemaColumn GetSchemaColumn(string columnName)
			{
				return columnName == "AB_Parent" ? AB_Parent : null;
			}

			public override string TableName
			{
				get { return "AB_Parent"; }
			}

			public override SchemaColumnCollection All
			{
				get { return new SchemaColumnCollection(new SchemaColumn[] { AB_Parent }); }
			}

			#endregion

			public readonly static SchemaWithUnconstrainedForeignKey schema;
			public readonly static SchemaBoolColumn AB_Parent;
		}

		class SchemaWithNormalLengthConstrainedForeignKey : TestSchemaForForeignKeyFiltering
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "This is a static constructor and uses reflection. Is not applicable")]
			static SchemaWithNormalLengthConstrainedForeignKey()
			{
				schema = new SchemaWithNormalLengthConstrainedForeignKey();
				AB_CD_Parent = new SchemaBoolColumn(schema, "AB_CD_Parent", 0, false, false, false);
			}

			#region ITableSchema Members

			public override SchemaColumn GetSchemaColumn(string columnName)
			{
				return columnName == "AB_CD_Parent" ? AB_CD_Parent : null;
			}

			public override string TableName
			{
				get { return "AB_CD_Parent"; }
			}

			public override SchemaColumnCollection All
			{
				get { return new SchemaColumnCollection(new SchemaColumn[] { AB_CD_Parent }); }
			}

			#endregion

			public readonly static SchemaWithNormalLengthConstrainedForeignKey schema;
			public readonly static SchemaBoolColumn AB_CD_Parent;
		}

		class SchemaWithLongHeaderConstrainedForeignKeyForTest : TestSchemaForForeignKeyFiltering
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "This is a static constructor and uses reflection. Is not applicable")]
			static SchemaWithLongHeaderConstrainedForeignKeyForTest()
			{
				schema = new SchemaWithLongHeaderConstrainedForeignKeyForTest();
				ABC_DE_ParentID = new SchemaBoolColumn(schema, "ABC_DE_ParentID", 0, false, false, false);
			}

			#region ITableSchema Members

			public override SchemaColumn GetSchemaColumn(string columnName)
			{
				return columnName == "ABC_DE_ParentID" ? ABC_DE_ParentID : null;
			}

			public override string TableName
			{
				get { return "ABC_DE_ParentID"; }
			}

			public override SchemaColumnCollection All
			{
				get { return new SchemaColumnCollection(new SchemaColumn[] { ABC_DE_ParentID }); }
			}

			#endregion

			public readonly static SchemaWithLongHeaderConstrainedForeignKeyForTest schema;
			public readonly static SchemaBoolColumn ABC_DE_ParentID;
		}

		class SchemaWithLongTailConstrainedForeignKeyForTest : TestSchemaForForeignKeyFiltering
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "This is a static constructor and uses reflection. Is not applicable")]
			static SchemaWithLongTailConstrainedForeignKeyForTest()
			{
				schema = new SchemaWithLongTailConstrainedForeignKeyForTest();
				AB_CDE_ParentGUID = new SchemaBoolColumn(schema, "AB_CDE_ParentGUID", 0, false, false, false);
			}

			#region ITableSchema Members

			public override SchemaColumn GetSchemaColumn(string columnName)
			{
				return columnName == "AB_CDE_ParentGUID" ? AB_CDE_ParentGUID : null;
			}

			public override string TableName
			{
				get { return "AB_CDE_ParentGUID"; }
			}

			public override SchemaColumnCollection All
			{
				get { return new SchemaColumnCollection(new SchemaColumn[] { AB_CDE_ParentGUID }); }
			}

			#endregion

			public readonly static SchemaWithLongTailConstrainedForeignKeyForTest schema;
			public readonly static SchemaBoolColumn AB_CDE_ParentGUID;
		}

		class SchemaWithLongHeaderAndLongTailConstrainedForeignKeyForTest : TestSchemaForForeignKeyFiltering
		{
			[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "This is a static constructor and uses reflection. Is not applicable")]
			static SchemaWithLongHeaderAndLongTailConstrainedForeignKeyForTest()
			{
				schema = new SchemaWithLongHeaderAndLongTailConstrainedForeignKeyForTest();
				ABC_DEF_ForeignKey = new SchemaBoolColumn(schema, "ABC_DEF_ForeignKey", 0, false, false, false);
			}

			#region ITableSchema Members

			public override SchemaColumn GetSchemaColumn(string columnName)
			{
				return columnName == "ABC_DEF_ForeignKey" ? ABC_DEF_ForeignKey : null;
			}

			public override string TableName
			{
				get { return "ABC_DEF_ForeignKey"; }
			}

			public override SchemaColumnCollection All
			{
				get { return new SchemaColumnCollection(new SchemaColumn[] { ABC_DEF_ForeignKey }); }
			}

			#endregion

			public readonly static SchemaWithLongHeaderAndLongTailConstrainedForeignKeyForTest schema;
			public readonly static SchemaBoolColumn ABC_DEF_ForeignKey;
		}
		#endregion
	}
}
