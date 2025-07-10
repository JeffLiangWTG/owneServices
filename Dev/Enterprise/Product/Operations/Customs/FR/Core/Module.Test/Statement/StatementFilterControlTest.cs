using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.FR.Business.CusStatement;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.Module.Statement.Testing
{
	class StatementFilterControlTest : TestCaseWithFactory
	{
		public void TestCustomsColumns()
		{
			using (var form = new ZForm())
			{
				var filterController = new StatementFilterControl(new CusStatementHeaderCollection(Factory), new StatementFilterStripBusinessObject());
				form.Controls.Add(filterController);
				form.Show();

				var columnStyles = filterController.Grid.ColumnStyles;
				var columnNames = columnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName);
				AssertContainsExactElementsInAnyOrder(new[]
				{
					nameof(CusStatementHeader.B2_StatementNumber),
					nameof(CusStatementHeader.CorrelationID),
					nameof(CusStatementHeader.EntryNumber),
					nameof(CusStatementHeader.B2_PaymentType),
					nameof(CusStatementHeader.PaymentTypeDescription),
					nameof(CusStatementHeader.B2_ProcessDate),
					nameof(CusStatementHeader.B2_StatementType),
					nameof(CusStatementHeader.StatementTypeDescription),
					nameof(CusStatementHeader.B2_EntryFilerCode),
					nameof(CusStatementHeader.B2_AccountNo),
					nameof(CusStatementHeader.B2_BranchDesignation),
					nameof(CusStatementHeader.BranchDesignationDescription),
					nameof(CusStatementHeader.B2_ImporterCustomsID),
					nameof(CusStatementHeader.ImporterFullName),
					nameof(CusStatementHeader.B2_DueDate),
					nameof(CusStatementHeader.B2_Status),
					nameof(CusStatementHeader.StatusDescription),
					nameof(CusStatementHeader.B2_SystemCreateTimeUtc),
					nameof(CusStatementHeader.B2_SystemCreateUser),
					nameof(CusStatementHeader.B2_SystemCreateBranch),
					nameof(CusStatementHeader.B2_SystemCreateDepartment),
					nameof(CusStatementHeader.B2_SystemLastEditTimeUtc),
					nameof(CusStatementHeader.B2_SystemLastEditUser)
				}, columnNames);
			}
		}

		public void TestCustomsGroups()
		{
			using (var form = new ZForm())
			{
				var filterController = new StatementFilterControl(new CusStatementHeaderCollection(Factory), new StatementFilterStripBusinessObject());
				form.Controls.Add(filterController);
				form.Show();

				var columnInfos = filterController.Grid.ColumnStyles.Cast<ZGridColumnInfo>().ToList();

				AssertEquals("Payment Method", columnInfos.Single(x => x.ColumnName == nameof(CusStatementHeader.B2_PaymentType)).GroupName.Caption);
				AssertEquals("Payment Method", columnInfos.Single(x => x.ColumnName == nameof(CusStatementHeader.PaymentTypeDescription)).GroupName.Caption);

				AssertEquals("Frequency", columnInfos.Single(x => x.ColumnName == nameof(CusStatementHeader.B2_StatementType)).GroupName.Caption);
				AssertEquals("Frequency", columnInfos.Single(x => x.ColumnName == nameof(CusStatementHeader.StatementTypeDescription)).GroupName.Caption);

				AssertEquals("Type", columnInfos.Single(x => x.ColumnName == nameof(CusStatementHeader.B2_BranchDesignation)).GroupName.Caption);
				AssertEquals("Type", columnInfos.Single(x => x.ColumnName == nameof(CusStatementHeader.BranchDesignationDescription)).GroupName.Caption);

				AssertEquals("Status", columnInfos.Single(x => x.ColumnName == nameof(CusStatementHeader.B2_Status)).GroupName.Caption);
				AssertEquals("Status", columnInfos.Single(x => x.ColumnName == nameof(CusStatementHeader.StatusDescription)).GroupName.Caption);
			}
		}

		public void TestCharacterCasing()
		{
			using (var form = new ZForm())
			{
				var filterController = new StatementFilterControl(new CusStatementHeaderCollection(Factory), new StatementFilterStripBusinessObject());
				form.Controls.Add(filterController);
				form.Show();

				var columnInfos = filterController.Grid.ColumnStyles.Cast<ZGridColumnInfo>().ToList();
				AssertEquals(CharacterCasing.Normal, columnInfos.Single(x => x.ColumnName == nameof(CusStatementHeader.PaymentTypeDescription)).CharacterCasing);
				AssertEquals(CharacterCasing.Normal, columnInfos.Single(x => x.ColumnName == nameof(CusStatementHeader.StatementTypeDescription)).CharacterCasing);
				AssertEquals(CharacterCasing.Normal, columnInfos.Single(x => x.ColumnName == nameof(CusStatementHeader.BranchDesignationDescription)).CharacterCasing);
				AssertEquals(CharacterCasing.Normal, columnInfos.Single(x => x.ColumnName == nameof(CusStatementHeader.StatusDescription)).CharacterCasing);
			}
		}
	}
}
