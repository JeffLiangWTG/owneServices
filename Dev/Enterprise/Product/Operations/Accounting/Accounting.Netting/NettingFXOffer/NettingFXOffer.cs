using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Netting
{
	public class NettingFXOffer : AutoNettingFXOffer
	{
		public NettingFXOffer(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public NettingOrganisation Participant
		{
			get { return Factory.Load<NettingOrganisation>(NFO_NSO_Organisation); }
		}

		[RelatedBusinessObject("Participant")]
		public override ZGuid NFO_NSO_Organisation
		{
			get { return base.NFO_NSO_Organisation; }
			set { base.NFO_NSO_Organisation = value; }
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			NFO_ApprovalStatus = "APP";
			NFO_RX_NKCurrency = "XXX";
			NFO_Type = "REQ";
			NFO_Value = 1;
		}
#endif
	}
}
