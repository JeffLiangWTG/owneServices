using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(DateFilterWithJobFallback))]
	class DateFilterWithJobFallbackTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var bizo = new ProcessHeaderFilterBusinessObject();
			return bizo[ProcessHeader.ModuleFilterConstants.AgreedDeliveryDate];
		}
	}
}
