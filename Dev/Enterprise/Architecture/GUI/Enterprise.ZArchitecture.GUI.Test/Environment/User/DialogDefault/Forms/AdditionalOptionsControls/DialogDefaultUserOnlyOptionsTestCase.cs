using System.Windows.Forms;
using Enterprise.ZArchitecture.Environment.DialogDefault;

namespace Enterprise.Core.DialogDefault.Testing
{
	sealed class DialogDefaultUserOnlyOptionsTestCase : DialogDefaultAdditionalOptionsChildTestCase
	{
		protected override Control GetControlCore(DialogDefaultContext context)
		{
			return new DialogDefaultUserOnlyOptions(context);
		}
	}
}
