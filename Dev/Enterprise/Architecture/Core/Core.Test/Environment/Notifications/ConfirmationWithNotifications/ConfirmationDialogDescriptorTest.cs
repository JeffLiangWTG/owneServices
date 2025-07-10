using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	[TestedType(typeof(ConfirmationDialogDescriptor))]
	sealed class ConfirmationDialogDescriptorTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ConfirmationDialogDescriptor("", "");
		}
	}
}
