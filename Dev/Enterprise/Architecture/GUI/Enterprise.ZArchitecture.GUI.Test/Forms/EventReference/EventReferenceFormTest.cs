using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Test.Forms.EventReference
{
	[TestedType(typeof(EventReferenceForm))]
	internal sealed class EventReferenceFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var bo = new Business.EventReference(Events.CustomisableEvent00Code, ZString.Empty);
			return new EventReferenceForm(bo);
		}

		#endregion
	}
}
