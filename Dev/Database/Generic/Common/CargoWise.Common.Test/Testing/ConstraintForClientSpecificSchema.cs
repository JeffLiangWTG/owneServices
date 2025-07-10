using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace CargoWise.Common.Testing
{
	[TestsSubclassesOf(typeof(IExtensionObjects), excludedFromTestAttributeType: null, excludedTypesAndTheirDescendants: new [] { typeof(ExtensionObjects) })]
	public abstract class ConstraintForClientSpecificSchema : TestCaseWithFactory
	{
		// WTG.SqlTests/src/SqlTests/TheTest.cs AllIndexesMustSetAllowPageLocksToOff
		public virtual void TestAllIndexesMustSetAllowPageLocksToOffForClientSpecificSchema()
		{
			var sql = @"
SELECT
	CONCAT('[', s.name, '].[', o.name, '].[', i.name, ']') AS name
FROM sys.indexes AS i
JOIN sys.objects AS o ON i.object_id = o.object_id
JOIN sys.schemas AS s ON o.schema_id = s.schema_id
WHERE 1=1
	AND o.is_ms_shipped = 0
	AND o.type = 'U'
	AND s.name NOT IN ('sys', 'cdc')
	AND i.index_id > 0
	AND i.allow_page_locks = 1
	ORDER BY 
	name;
";

			AssertIndex("All Indexes Must Set (ALLOW_PAGE_LOCKS = OFF): ", sql);
		}

		void AssertIndex(string message, string sql)
		{
			var result = RunCommand(sql).Except(WhiteList);
			AssertEquals(message + string.Join($",{System.Environment.NewLine}", result), 0, result.Count());
			string[] RunCommand(string sqlCommand)
			{
				var result = new List<string>();
				using (var command = Db.Connection.Command(sqlCommand))
				using (var reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add((string)reader["name"]);
					}
				}
				return result.ToArray();
			}
		}

		public virtual List<string> WhiteList { get; protected set; } = new List<string>()
		{
			"[dbo].[ClientDispositionSourceUpdate].[PK_UX__QK_PK]",
			"[dbo].[ClientFDAMsgStatusUpdate].[NR_RX__YFS_PK]",
			"[dbo].[ClientFRCusEntryLineWithoutInvoiceAmount].[NR_RC__CL_ClusterKey]",
			"[dbo].[ClientFRCusEntryLineWithoutISTTransactionAdded].[NR_RX__FR1_EventTime_FR1_CL_CH_FR1_LineNumber]",
			"[dbo].[ClientGlbBranchPhoneNumbers].[PK_UX__CBP_GlbBranchPK]",
			"[dbo].[ClientGlbCompanyPhoneNumbers].[PK_UX__CGP_GlbCompanyPK]",
			"[dbo].[ClientGlbStaffPhoneNumbers].[PK_UX__CSP_GlbStaffPK]",
			"[dbo].[ClientOrgAddressFaxNormalization].[PK_UX__COF_AddressPK]",
			"[dbo].[ClientOrgAddressMobileNormalization].[PK_UX__COM_AddressPK]",
			"[dbo].[ClientOrgAddressPhoneNormalization].[PK_UX__COP_AddressPK]",
			"[dbo].[ClientOrgContactItemPhoneNumbers].[PK_UX__CIP_OrgContactItemPK]",
			"[dbo].[ClientOrgContactPhoneNumbers].[PK_UX__CCP_OrgContactPK]",
			"[dbo].[ClientProcessedCopyExportersBankDataToJobComInvoiceHeader].[IX_ClientProcessedCopyExportersBankDataToJobComInvoiceHeader_JE_ClusterKey]",
			"[dbo].[ClientTable4GbPerformApportionment].[NR_RX__GB1_PK]",
			"[dbo].[ClientTableForMovingCARSTMsg].[NR_RX__XZ4_PK]",
			"[dbo].[ClientUSDeclarationTotalEnteredValueTransform].[NR_RC_TEV_Time]",
			"[dbo].[ClientZAJEList].[NR_RX__FDT_PK]",
			"[dbo].[DummyBizo].[NR_RX__Z0_BitFiltered]",
			"[dbo].[DummyBizo].[PK_UX__Z0_PK]",
			"[dbo].[DummyDependentBizo].[PK_UX__ZD1_PK]",
			"[dbo].[DummyLogged].[PK_UX__ZL2_PK]",
			"[dbo].[DummyPivot].[PK_UX__ZDP_PK]",
			"[dbo].[RefDocType].[NR_UC__RT_ReferenceType_RT_DocType]",
			"[dbo].[StmProcessQueue].[_WTG__BillingTransactionMigration]",
			"[dbo].[WhsDocketLine].[_WTG__Transform StmALog to WhsInventoryHoldChangeLog._3]",
		};
	}
}
