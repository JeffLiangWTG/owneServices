using System.Windows.Forms;
using Enterprise.ErrorReporting.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ErrorReporting.GUI.Test
{
	[TestedType(typeof(ErrorDetailsForm))]
	sealed class ErrorDetailsFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new ErrorDetailsForm(Factory.New<StmErrorReport>());
		}
	}
}
