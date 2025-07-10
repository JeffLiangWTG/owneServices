using System.Windows.Forms;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Licencing.GUI.Testing
{
	[TestedType(typeof(MessageChooserForm))]
	internal sealed class MessageChooserFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new MessageChooserForm(new CodeDescriptionPairList());
		}

		#endregion
	}
}
