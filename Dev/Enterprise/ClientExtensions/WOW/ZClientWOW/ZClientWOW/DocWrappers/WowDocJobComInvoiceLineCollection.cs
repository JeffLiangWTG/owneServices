using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.AU;

namespace Enterprise.Client.Wow
{
	public class WowDocJobComInvoiceLineCollection : DocJobComInvoiceLineCollection
	{
		public WowDocJobComInvoiceLineCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public new WowDocJobComInvoiceLine this[int index]
		{
			get { return (WowDocJobComInvoiceLine)Elements[index]; }
		}
	}
}
