using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.KR.GUI.Testing
{
	sealed class SubsequentUserControlTest : TestCaseWithFactory
	{
		public void TestRequestToExtendReExportGrid()
		{
			using (var control = new ImportMessageUserControl())
			{
				var grid = (ZGrid)control.Controls.Find("RequestToExtendReExportDateGrid", true)[0];

				var columnsList = new List<ImportMessageUserControlTest.Columns>();
				columnsList.Add(CreateColumnsType(nameof(EDIMessageWrapper.GOVCBRD72Message) + "+" + nameof(ImportD72Details.SequenceNo), typeof(ZCalcEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(EDIMessageWrapper.GOVCBRD72Message) + "+" + nameof(ImportD72Details.MessageStatus), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(EDIMessageWrapper.GOVCBRD72Message) + "+" + nameof(ImportD72Details.MessageStatusDescription), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(EDIMessageWrapper.GOVCBRD72Message) + "+" + nameof(ImportD72Details.AcceptedDate), typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(EDIMessageWrapper.GOVCBRD72Message) + "+" + nameof(ImportD72Details.DecisionDate), typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(EDIMessageWrapper.GOVCBRD72Message) + "+" + nameof(ImportD72Details.NoticeTypeDescription), typeof(ZTextBoxColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(EDIMessageWrapper.GOVCBRD72Message) + "+" + nameof(ImportD72Details.BeforeReExportScheduledDate), typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(EDIMessageWrapper.GOVCBRD72Message) + "+" + nameof(ImportD72Details.AfterReExportScheduledDate), typeof(ZDateEditColumnStyleInfo)));
				columnsList.Add(CreateColumnsType(nameof(EDIMessageWrapper.GOVCBRD72Message) + "+" + nameof(ImportD72Details.ReasonDescription), typeof(ZTextBoxColumnStyleInfo)));

				Assert(grid, columnsList);
			}
		}

		void Assert(ZGrid grid, List<ImportMessageUserControlTest.Columns> columnsList)
		{
			foreach (ImportMessageUserControlTest.Columns col in columnsList)
			{
				var checkColumn = grid.GetColumnStyle(col.Names);
				AssertNotNull(checkColumn);
				AssertEquals(col.InfoType, checkColumn.GetType());
				AssertEquals("IsReadOnly", true, checkColumn.IsReadOnly);

				if (col.Names == nameof(ImportAmendmentDetails.AmendType1) || col.Names == nameof(ImportAmendmentDetails.AmendType2)
					|| col.Names == nameof(ImportAmendmentDetails.FaultParty) || col.Names == nameof(ImportAmendmentDetails.ReasonCode)
					|| col.Names == nameof(ImportAmendmentDetails.NoticeType))
				{
					Assert(!checkColumn.IsVisible);
				}
				else
				{
					Assert(checkColumn.IsVisible);
				}
			}
		}

		ImportMessageUserControlTest.Columns CreateColumnsType(ZString names, System.Type infoType)
		{
			var result = new ImportMessageUserControlTest.Columns();
			result.Names = names;
			result.InfoType = infoType;
			return result;
		}
	}
}
