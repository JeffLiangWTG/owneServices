using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.DE.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.Module.Testing
{
	public class ExportStatusRequestFilterControlTest : TestCaseWithFactory
	{
		public void TestColumns()
		{
			using (var form = new ZForm())
			using (var filterControl = new ExportStatusRequestFilterStripControl(new StatusRequestCollection(Factory, GlbBranch.CurrentBranch), new ExportStatusRequestFilterBusinessObject()))
			{
				form.Controls.Add((filterControl));
				form.Show();

				var gird = filterControl.Grid;
				CombineAssertions(() =>
				{
					AssertColumn(gird.GetColumnStyle(StatusRequest.Schema.Module), width: 53, casing: CharacterCasing.Upper);
					AssertColumn(gird.GetColumnStyle(StatusRequest.Schema.MovementReferenceNumber), width: 129, casing: CharacterCasing.Upper);
					AssertColumn(gird.GetColumnStyle(StatusRequest.Schema.EM_Status), width: 44, casing: CharacterCasing.Upper);
					AssertColumn(gird.GetColumnStyle(StatusRequest.Schema.Response), width: 61, casing: CharacterCasing.Normal);
					AssertColumn(gird.GetColumnStyle(StatusRequest.Schema.EM_SystemCreateUser), width: 66, casing: CharacterCasing.Normal);
					AssertColumn(gird.GetColumnStyle(StatusRequest.Schema.EM_SystemCreateTimeUtc), width: 100, casing: CharacterCasing.Normal);
					AssertColumn(gird.GetColumnStyle(StatusRequest.Schema.EM_SystemLastEditUser), width: 66, casing: CharacterCasing.Normal);
					AssertColumn(gird.GetColumnStyle(StatusRequest.Schema.EM_SystemLastEditTimeUtc), width: 100, casing: CharacterCasing.Normal);
				});
			}

			void AssertColumn(ZGridColumnInfo info, int width, CharacterCasing casing)
			{
				AssertEquals(info.ColumnName + " width", width, info.Width);
				AssertEquals(info.ColumnName + " casing", casing, info.CharacterCasing);
			}
		}
	}
}
