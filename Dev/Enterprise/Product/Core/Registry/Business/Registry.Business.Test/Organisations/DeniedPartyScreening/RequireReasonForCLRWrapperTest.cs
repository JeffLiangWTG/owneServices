using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(RequireReasonForCLRWrapper))]
	sealed class RequireReasonForCLRWrapperTest : RegistryBusinessObjectTemplateTestCase
	{
		protected override bool RequiresFactory => false;

		protected override bool RequiresFallbackLevel => false;

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			var itemCollection = new RequireReasonForCLRItemCollection
			{
				new RequireReasonForCLRItem
				{
					Code = "123",
					Title = "Dummy Title",
					ClearingReason = "Dummy Description"
				}
			};

			return new RequireReasonForCLRWrapper(itemCollection)
			{
				RequireReasonForCLR = true
			};
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise() => GetBusinessObjectToClone();
	}
}
