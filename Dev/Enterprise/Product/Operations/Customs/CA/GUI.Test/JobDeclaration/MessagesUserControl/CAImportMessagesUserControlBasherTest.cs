using System;
using NUnit.Framework;

#if !WINZOR
namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(CAImportMessagesUserControl))]
	sealed class CAImportMessagesUserControlBasherTest : ImportCustomsUserControlBasherTest
	{
		protected override Type UserControlToBashType => typeof(CAImportMessagesUserControl);
	}
}
#endif
