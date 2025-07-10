namespace Enterprise.ZArchitecture.GUI.Internal
{
	sealed class ZListUserControl_Test : NUnit.Framework.TestCase
	{
		public void TestPreventErrorReportForMissingListAttribute()
		{
			using (ZListUserControl control1 = new Testing.DummyListControl(), control2 = new Testing.DummyListControl())
			{
				AssertEquals(0, control1.preventErrorReportForMissingListAttribute);
				AssertEquals(0, control2.preventErrorReportForMissingListAttribute);
				using (control1.PreventErrorReportForMissingListAttribute())
				{
					AssertEquals(1, control1.preventErrorReportForMissingListAttribute);
					AssertEquals(0, control2.preventErrorReportForMissingListAttribute);
					using (control2.PreventErrorReportForMissingListAttribute())
					{
						AssertEquals(1, control1.preventErrorReportForMissingListAttribute);
						AssertEquals(1, control2.preventErrorReportForMissingListAttribute);
					}
					AssertEquals(1, control1.preventErrorReportForMissingListAttribute);
					AssertEquals(0, control2.preventErrorReportForMissingListAttribute);
				}
				AssertEquals(0, control1.preventErrorReportForMissingListAttribute);
				AssertEquals(0, control2.preventErrorReportForMissingListAttribute);
			}
		}
	}
}
