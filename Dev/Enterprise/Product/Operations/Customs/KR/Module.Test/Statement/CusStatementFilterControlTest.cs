using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.KR.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Module.Testing
{
	[TestedType(typeof(CusStatementFilterControl))]
	sealed class CusStatementFilterControlTest : TestCaseWithFactory
	{
		public void TestFilterGrid()
		{
			using (var filterControl = new CusStatementFilterControl(new CusStatementHeaderCollection(Factory), new CusStatementFilterStripBusinessObject()))
			{
				filterControl.Show();

				AssetColoumn(filterControl, nameof(CusStatementHeader.B2_PaymentStatusName));
				AssetColoumn(filterControl, nameof(CusStatementHeader.B2_StatementTypeName));
				AssetColoumn(filterControl, nameof(CusStatementHeader.FormattedStatementNumber));
				AssetColoumn(filterControl, nameof(CusStatementHeader.CustomsOfficeName));
				AssetColoumn(filterControl, nameof(CusStatementHeader.PayerCompanyName));
				AssetColoumn(filterControl, nameof(CusStatementHeader.FormattedImporterID));
				AssetColoumn(filterControl, nameof(CusStatementHeader.B2_ProcessDate));
				AssetColoumn(filterControl, nameof(CusStatementHeader.B2_DueDate));
				AssetColoumn(filterControl, nameof(CusStatementHeader.B2_PaymentAuthorizationDate));
				AssetColoumn(filterControl, nameof(CusStatementHeader.B2_StatementAmount));
				AssetColoumn(filterControl, nameof(CusStatementHeader.B2_PaymentParty));
				AssetColoumn(filterControl, nameof(CusStatementHeader.CreateDateTimeInLocalTimeZone));
				AssetColoumn(filterControl, nameof(CusStatementHeader.FormattedAccountNumber));
				AssetColoumn(filterControl, nameof(CusStatementHeader.B2_StatusName));

				AssetColoumn(filterControl, CusStatementHeader.Schema.B2_PaymentStatus, false);
				AssetColoumn(filterControl, CusStatementHeader.Schema.B2_StatementType, false);
				AssetColoumn(filterControl, CusStatementHeader.Schema.B2_ProcessPort, false);
				AssetColoumn(filterControl, nameof(CusStatementHeader.PeriodFrom), false);
				AssetColoumn(filterControl, nameof(CusStatementHeader.B2_PaymentPartyName), false);
				AssetColoumn(filterControl, CusStatementHeader.Schema.B2_PaymentType, false);
				AssetColoumn(filterControl, nameof(CusStatementHeader.B2_PaymentTypeName), false);
				AssetColoumn(filterControl, nameof(CusStatementHeader.TotalVATBaseAmountForVATReport), false);
				AssetColoumn(filterControl, CusStatementHeader.Schema.B2_SystemCreateUser, false);
				AssetColoumn(filterControl, CusStatementHeader.Schema.B2_SystemCreateTimeUtc, false);
				AssetColoumn(filterControl, CusStatementHeader.Schema.B2_Status, false);
			}

			void AssetColoumn(CusStatementFilterControl filterControl, string columnName, bool isVisible = true)
			{
				var column = filterControl.FilteredGrid.GetColumnStyle(columnName);
				AssertNotNull($"{columnName} is not null", column);
				AssertEquals($"Visible of {columnName}", isVisible, column.IsVisible);
			}
		}
	}
}
