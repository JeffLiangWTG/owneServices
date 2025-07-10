using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.DataPurge
{
	[TestedType(typeof(DatabaseBackupForm))]
	sealed class DatabaseBackupFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new DatabaseBackupForm();
		}

		#endregion
	}
}
