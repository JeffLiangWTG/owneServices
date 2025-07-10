using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI.Testing
{
	public class CarnetEntryUserControlTest : TestCaseWithFactory
	{
		public void TestCarnetEntryGrid()
		{
			using (var control = new CarnetEntryUserControl())
			{
				control.Show();

				using (var grid = control.FindSingle<ZGrid>("CarnetEntryGrid"))
				{
					var index = 0;
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index]).CaptionResourceString.Caption, "Carnet Certificate No.");
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(Business.CusEntryHeader.CarnetCertificateNo));
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index]).CaptionResourceString.Caption, "Message Type");
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, CusEntryHeader.Schema.CH_MessageType);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index]).CaptionResourceString.Caption, "Message Type Desc.");
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, nameof(Business.CusEntryHeader.MessageTypeDescription));
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index]).CaptionResourceString.Caption, "Msg. Status");
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, CusEntryHeader.Schema.CH_Status);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index]).CaptionResourceString.Caption, "Msg. Status Desc.");
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, CusEntryHeader.Schema.MessageStatusDescription);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index]).CaptionResourceString.Caption, "Ent. Status");
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, CusEntryHeader.Schema.CH_EntryStatus);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index]).CaptionResourceString.Caption, "Ent. Status Desc.");
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, CusEntryHeader.Schema.EntryHeaderStatusDescription);
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index]).CaptionResourceString.Caption, "Entry Submitted Date");
					AssertEquals(((ZGridColumnInfo)grid.ColumnStyles[index++]).ColumnName, CusEntryHeader.Schema.CH_EntrySubmittedDate);
				}
			}
		}
	}
}
