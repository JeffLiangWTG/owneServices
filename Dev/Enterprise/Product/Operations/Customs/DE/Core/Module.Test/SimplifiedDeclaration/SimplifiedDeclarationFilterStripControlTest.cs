using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business;

namespace Enterprise.Customs.DE.Module.Testing
{
	class SimplifiedDeclarationFilterStripControlTest : TestCaseWithFactory
	{
		public void TestAvailableGridColumns()
		{
			using (var userControl = new SimplifiedDeclarationFilterStripControl(new ActiveBusinessObjectCollection<CusReconEntry>(Factory), new SimplifiedDeclarationFilterStripBusinessObject()))
			{
				var expectedColumns = new[] { CusReconEntry.Schema.DeclarantCode, CusReconEntry.Schema.ImporterCode, CusReconEntry.Schema.RepresentativeCode, CusReconEntry.Schema.BuyingAgentCode,
					CusReconEntry.Schema.CRE_EntryDate, CusReconEntry.Schema.CRE_OriginalEntryNumber, CusReconEntry.Schema.CRE_EntryType, CusReconEntry.Schema.RepresentationType, CusReconEntry.Schema.CRE_SystemCreateUser, CusReconEntry.Schema.CRE_SystemCreateTimeUtc,
					CusReconEntry.Schema.CRE_SystemLastEditUser, CusReconEntry.Schema.CRE_SystemLastEditTimeUtc };
				AssertContainsExactElementsInAnyOrder(expectedColumns, userControl.FilteredGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName));
			}
		}

		public void TestEntryDateColumnFormat()
		{
			using (var userControl = new SimplifiedDeclarationFilterStripControl(new ActiveBusinessObjectCollection<CusReconEntry>(Factory), new SimplifiedDeclarationFilterStripBusinessObject()))
			{
				var entryDateColumnInfo = userControl.FilteredGrid.GetColumnStyle(Customs.Business.CusReconBase.AutoCusReconEntry.Schema.CRE_EntryDate) as ZArchitecture.ZDateEditColumnStyleInfo;
				AssertEquals("CRE_EntryDate DateTimeFormat", ZArchitecture.Core.ZDateTimePickerFormat.Short, entryDateColumnInfo.DateTimeFormat);
			}
		}
	}
}
