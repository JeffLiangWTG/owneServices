using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.CustomerService.Business.Testing
{
	[TestedType(typeof(IncidentApprovalAttachment))]
	sealed class IncidentApprovalAttachmentTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new IncidentApprovalAttachment();
		}

		#endregion
	}
}
