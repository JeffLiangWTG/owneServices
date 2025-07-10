using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.CN.Business;

namespace Enterprise.Customs.CN.GUI.Testing
{
	abstract class JobDeclarationFormTest : Customs.GUI.Testing.BaseJobDeclarationFormAbstractTest<JobDeclaration>
	{
		public override void TestMinimumSizeNotTooBig()
		{
			using (var form = GetFormToBashCore())
			{
				FormHelper.AssertMinimumSizeNotTooBig(form);
			}
		}

		protected override BaseJobDeclaration CreateDeclarationForPerformanceTest(BusinessObjectFactory factory)
		{
			var result = factory.New<JobDeclaration>();
			result.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			result.JE_MessageType = MessageTypeForFormBashing;
			result.JE_TransportMode = Core.Constants.TransportModes.Sea;
			return result;
		}

		protected override bool AllowTabBackwardCore(Control control, Control previousControl)
		{
			return base.AllowTabBackwardCore(control, previousControl) || TabIndexBackwards.Any(backward => ControlsMatchPair(backward, control, previousControl));
		}

		IEnumerable<(string Name1, string Name2)> TabIndexBackwards
		{
			get
			{
				yield return ("AEOCodeTextBox", "GovRegNumTypeDropEdit");
				yield return ("CustomsCodeTextBox", "GovRegNumTextBox");
				yield return ("SocialCreditCodeTextBox", "GovRegNumTypeDropEdit");
			}
		}

		static bool ControlsMatchPair((string Name1, string Name2) pair, Control control1, Control control2)
		{
			return pair.Name1 == control1.Name && pair.Name2 == control2.Name || pair.Name1 == control2.Name && pair.Name2 == control1.Name;
		}
	}
}
