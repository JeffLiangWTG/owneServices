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
	[TestedType(typeof(OrgContactCertificateSubscriber))]
	class OrgContactCertificateSubscriberTest : CertificateSubscriberTest<OrgContactCertificateSubscriber>
	{
		protected override string ParentTablePrefix => OrgContactSchema.Constants.Prefix;

		OrgContact orgContact;

		protected override ZGuid PersonPK => orgContact.OC_PER;

		protected override GlbPerson GetPersonFromCertificate(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO) => factory.LoadTop1<OrgContact>(new ZQuery(OrgContactSchema.PK, bizO.XZ_ParentID)).Person;

		protected override GenRegCertAccredMaintList CreateParentRecords(BusinessObjectFactory factory)
		{
			orgContact = SubscriberTestUtilities.CreateTestOrgContact(factory);

			var certificate = factory.New<GenRegCertAccredMaintList>();
			certificate.XZ_ParentID = orgContact.PK;
			certificate.XZ_ParentTableCode = ParentTablePrefix;
			certificate.XZ_RefNumber = parentNumber;
			certificate.XZ_Type = CertificateTypePairList.Codes.CA1;

			factory.Save();

			return certificate;
		}

		protected override GenRegCertAccredMaintList CreatePlaceholderRecords(BusinessObjectFactory factory)
		{
			orgContact = SubscriberTestUtilities.CreateTestOrgContact(factory);

			var certificate = factory.New<GenRegCertAccredMaintList>();
			certificate.XZ_ParentID = orgContact.PK;
			certificate.XZ_ParentTableCode = ParentTablePrefix;
			certificate.XZ_RefNumber = placeHolderNumber;
			certificate.XZ_Type = CertificateTypePairList.Codes.PA1;

			factory.Save();

			return certificate;
		}

		protected override void AssertQUEResultRecordExists(BusinessObjectFactory factory, GenRegCertAccredMaintList bizO)
		{
			var personPK = factory.Load<OrgContact>(bizO.XZ_ParentID).OC_PER;
			AssertQueuedRecordExists(factory, personPK, GlbPersonSchema.Constants.Prefix);
		}

		public override void TestCustomFilter()
		{
			var subscriber = new OrgContactCertificateSubscriber();

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
				row[GenRegCertAccredMaintListSchema.XZ_ParentTableCode.Name] = OrgContactSchema.Constants.Prefix;
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
