using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.IssueManager.Module
{
	public class ErrorLogStatusFilterStripTest : TestCaseWithFactory
	{
		public void TestGetCurrentFilterControls()
		{
			using (ErrorLogSatusFilterStrip strip = new ErrorLogSatusFilterStrip())
			{
				strip.SetDataBinding(new ZArchitecture.Business.Internal.FilterStrip(new ModuleFilterCollection()), "");
				Control[] result = strip.InternalGetCurrentFilterControls(new ErrorLogStatusFilter("error status filter", delegate(ZString value)
				{
					return new ZQuery();
				}, new CodeDescriptionPairList()));
				AssertEquals(1, result.Length);
				AssertEquals(typeof(ErrorLogStatusFilterControl), result[0].GetType());
				result[0].Dispose();
				result = strip.InternalGetCurrentFilterControls(new ModuleTextFilter("hello", GlbStaffSchema.GS_Code));
				AssertNull(result);
			}
		}
	}
}
