using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Registry.Testing
{
	[TestedType(typeof(EnableEInvoicingFunctionalityDateTimeRegistryItem))]
	class EnableEInvoicingFunctionalityDateTimeRegistryItemTest : DateTimeRegistryItemTest
	{
		protected override StronglyTypedRegistryItem<DateTime, DateTime> GetNewRegistryItem()
		{
			return new EnableEInvoicingFunctionalityDateTimeRegistryItem("", null, null, null, RegistryStorageFlags.Company, RegistryOptions.IsOnlyForSupport);
		}
	}
}
