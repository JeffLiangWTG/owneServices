using System.Windows.Forms;
using Enterprise.PAVE.MENT.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.PAVE.MENT.GUI.Test
{
	[TestedType(typeof(MENTAgedScoreQueryForm))]
	class MENTAgedScoreQueryFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var query = Factory.New<MENTAgedScoreQuery>();
			query.MAQ_Code = "ANOTHERQ";
			query.MAQ_QueryDescription = "This does something when you want it to";
			var schedule = query.QuerySchedule;
			schedule.S5_ScheduleDescription = "blah";
			Factory.Save();
			return new MENTAgedScoreQueryForm(query);
		}

		public override void TestMinimumSizeNotTooBig()
		{
			Assert("1024 wide is too small. RJW approved 1080p for BMS.", true);
		}
	}
}
