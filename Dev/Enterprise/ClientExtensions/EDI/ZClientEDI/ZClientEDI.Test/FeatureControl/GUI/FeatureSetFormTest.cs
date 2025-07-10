using System.Collections;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Enterprise.Client.EDI.FeatureControl.Business;
using Enterprise.Core.Forms;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.FeatureControl.GUI.Testing
{
	[TestedType(typeof(FeatureSetForm))]
	public class FeatureSetFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var featureSet = Factory.New<FeatureControlSet>();
			var form = new FeatureSetForm(featureSet);
			return form;
		}

		public void TestAuditTabpage()
		{
			using (var form = GetFormToBashCore() as FeatureSetForm)
			{
				form.Show();
				var propertyInfo = typeof(FeatureSetForm)
					.GetProperty("ShowAuditTab", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				var propertyValue = (bool)propertyInfo.GetValue(form);
				AssertEquals(true, propertyValue);
			}
		}

		public void TestDatabaseGridColumns()
		{
			using (var form = GetFormToBashCore() as FeatureSetForm)
			{
				form.Show();
				var columns = form.DatabaseGridForTest.InnerGrid.ColumnStyles;
				CombineAssertions(() =>
				{
					AssertHasColumn(columns, "LD_DatabaseNumber");
					AssertHasColumn(columns, "EDIWebAccessOrg+OH_Code");
					AssertHasColumn(columns, "EDIWebAccessOrg+OH_FullName");
					AssertHasColumn(columns, "LD_ReleaseRing");
					AssertHasColumn(columns, "LD_LicenceType");
					AssertHasColumn(columns, "LD_ServerCode");
					AssertHasColumn(columns, "LD_FeatureSetConfigDateUtc");
				});
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);
			Assert("InnerGrid should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}

		public void TestFormCanAttachWithoutSecurity()
		{
			using (var form = GetFormToBashCore() as FeatureSetForm)
			{
				Assert(form is ICanAttachWithoutSecurity);
			}
		}
	}
}
