using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Messaging.Business.Testing
{
	[TestedType(typeof(DiagnosticConsolHelper))]
	public class DiagnosticConsolTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new DiagnosticConsolHelper(Factory);
		}

		#endregion
	}
}
