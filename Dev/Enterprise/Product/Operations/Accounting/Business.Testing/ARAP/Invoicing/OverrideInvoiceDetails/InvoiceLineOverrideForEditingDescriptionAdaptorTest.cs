using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoiceLineOverrideForEditingDescriptionAdaptor))]
	public class InvoiceLineOverrideForEditingDescriptionAdaptorTest : InvoiceLineOverrideAdaptorTest<InvoiceLineOverrideForEditingDescription>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceLineOverrideForEditingDescriptionAdaptor(Factory, System.Array.Empty<ZGuid>());
		}

		protected override InvoiceLineOverrideAdaptor<InvoiceLineOverrideForEditingDescription> GetAdaptor(BusinessObjectFactory factory, params ZGuid[] linePks)
		{
			return new InvoiceLineOverrideForEditingDescriptionAdaptor(factory, linePks);
		}
	}
}
