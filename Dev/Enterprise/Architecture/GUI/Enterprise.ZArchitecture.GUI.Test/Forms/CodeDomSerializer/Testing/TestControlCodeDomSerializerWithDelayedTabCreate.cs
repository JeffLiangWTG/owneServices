using System;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	sealed class TestControlCodeDomSerializerWithDelayedTabCreate : ControlCodeDomSerializerWithDelayedTabCreate
	{
#if !WINZOR
		protected override Type TabPageType
		{
			get { return typeof(MockTabPage); }
		}
#endif
	}
}
