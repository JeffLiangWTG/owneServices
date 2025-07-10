using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.AuditDataServices.MDM.Subscribers;
using Enterprise.AuditDataServices.Subscription.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.AuditDataServices.MDM.Testing
{
	[TestedType(typeof(GlbPersonCertificateSubscriber))]
	class GlbPersonCertificateSubscriberTest : CertificateSubscriberTest<GlbPersonCertificateSubscriber>
	{
		protected override string ParentTablePrefix => GlbPersonSchema.Constants.Prefix;

		protected override GlbPerson GetPersonFromCertificate(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO) => factory.LoadTop1<GlbPerson>(new ZQuery(GlbPersonSchema.PK, bizO.XZ_ParentID));

		GlbPerson glbPerson;

		protected override ZGuid PersonPK => glbPerson.PK;

		protected override GenRegCertAccredMaintList CreateParentRecords(BusinessObjectFactory factory)
		{
			glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var certificate = factory.New<GenRegCertAccredMaintList>();
			certificate.XZ_ParentID = glbPerson.PK;
			certificate.XZ_ParentTableCode = ParentTablePrefix;
			certificate.XZ_RefNumber = parentNumber;
			certificate.XZ_Type = CertificateTypePairList.Codes.CA1;
			certificate.MasterParent = glbPerson;

			factory.Save();

			return certificate;
		}

		protected override GenRegCertAccredMaintList CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			glbPerson = SubscriberTestUtilities.CreateTestGlbPerson(factory);

			var certificate = factory.New<GenRegCertAccredMaintList>();
			certificate.XZ_ParentID = glbPerson.PK;
			certificate.XZ_ParentTableCode = ParentTablePrefix;
			certificate.XZ_RefNumber = placeHolderNumber;
			certificate.XZ_Type = CertificateTypePairList.Codes.PA1;
			certificate.MasterParent = glbPerson;

			factory.Save();

			return certificate;
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO)
		{
			AssertQueuedRecordExists(factory, bizO.XZ_ParentID, GlbPersonSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new GlbPersonCertificateSubscriber();

			var table = GetTestDataTable();
			var rowToKeep = GetPopulatedDataRow(table, false);
			var rowToDelete = GetPopulatedDataRow(table, true);
			table.AcceptChanges();

			AssertEquals(2, table.Rows.Count);

			RunCustomFilter(rowToKeep, subscriber);
			RunCustomFilter(rowToDelete, subscriber);
			table.AcceptChanges();

			AssertEquals(1, table.Rows.Count);
			AssertCollectionContains(rowToKeep, table.Rows);
			AssertCollectionNotContains(rowToDelete, table.Rows);
		}

		DataRow GetPopulatedDataRow(DataTable table, bool shouldBeFiltered)
		{
			var row = table.NewRow();
			if (shouldBeFiltered)
			{
				row[GenRegCertAccredMaintListSchema.XZ_Type.Name] = DBNull.Value;
				row[GenRegCertAccredMaintListSchema.XZ_RefNumber.Name] = DBNull.Value;
				row[GenRegCertAccredMaintListSchema.XZ_ParentTableCode.Name] = "BAD";
			}
			else
			{
				row[GenRegCertAccredMaintListSchema.XZ_Type.Name] = "Type";
				row[GenRegCertAccredMaintListSchema.XZ_RefNumber.Name] = "Ref";
				row[GenRegCertAccredMaintListSchema.XZ_ParentTableCode.Name] = GlbPersonSchema.Constants.Prefix;
			}
			table.Rows.Add(row);
			return row;
		}

		protected override DataTable GetTestDataTable()
		{
			var changeTable = new DataTable();
			var tableColumns = new SchemaColumn[] { GenRegCertAccredMaintListSchema.XZ_Type, GenRegCertAccredMaintListSchema.XZ_RefNumber, GenRegCertAccredMaintListSchema.XZ_ParentTableCode };
			foreach (var column in tableColumns)
			{
				changeTable.Columns.Add(column.Name, column.DotNetType);
			}
			return changeTable;
		}
	}
}
