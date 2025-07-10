using CargoWise.EntityFramework;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoiceLineOverrideForEditingSequenceAdaptor))]
	public class InvoiceLineOverrideForEditingSequenceAdaptorTest : InvoiceLineOverrideAdaptorTest<InvoiceLineOverrideForEditingSequence>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new InvoiceLineOverrideForEditingSequenceAdaptor(Factory, System.Array.Empty<ZGuid>());
		}

		protected override InvoiceLineOverrideAdaptor<InvoiceLineOverrideForEditingSequence> GetAdaptor(BusinessObjectFactory factory, params ZGuid[] linePks)
		{
			return new InvoiceLineOverrideForEditingSequenceAdaptor(factory, linePks);
		}
	}
}
