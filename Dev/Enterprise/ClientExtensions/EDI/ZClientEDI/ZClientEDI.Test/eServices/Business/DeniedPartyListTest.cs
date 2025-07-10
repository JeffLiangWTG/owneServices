using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.eServices.Business.Testing
{
	[TestedType(typeof(DeniedPartyList))]
	internal sealed class DeniedPartyListTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			return new DeniedPartyList();
		}
		#endregion
	}
}
