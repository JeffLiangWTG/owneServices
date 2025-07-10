using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentWrappers
{
	public class DocCustomsDisbursementChargeCollection : DocumentWrapperCollection
	{
		public DocCustomsDisbursementChargeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public new DocCustomsDisbursementCharge this[int index]
		{
			get { return (DocCustomsDisbursementCharge)base[index]; }
		}

		public new DocCustomsDisbursementCharge this[string chargeCode]
		{
			get { return this.Cast<DocCustomsDisbursementCharge>().FirstOrDefault(x => x.ChargeCode == chargeCode); }
		}
	}
}
