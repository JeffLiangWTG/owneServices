using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public abstract class WhsPopulatePickableDocketStrategy : WhsPopulateDocketStrategy
	{
		#region Constructor

		public WhsPopulatePickableDocketStrategy(BusinessObject docket)
			: base(docket)
		{
		}

		#endregion

		#region Properties

		#region Consignee

		public override OrganisationWrapper Consignee
		{
			get
			{
				if (consignee == null)
				{
					consignee = new OrganisationWrapper(OrganisationUsageType.Consignee, PickableDocketBO.ConsigneeDocAddress, Factory);
				}
				return consignee;
			}
		}
		OrganisationWrapper consignee;

		#endregion

		#region ConsigneeAddress

		public override AddressWrapper ConsigneeAddress
		{
			get
			{
				return consigneeAddress ?? (consigneeAddress = new AddressWrapper(PickableDocketBO.ConsigneeDocAddress, Factory));
			}
		}
		AddressWrapper consigneeAddress;

		#endregion

		#region HandlingInstructions

		public override LabelValuePairWrapper HandlingInstructions
		{
			get
			{
				return new LabelValuePairWrapper(Res.GetString("7fafc8ea-9329-45d5-9e3a-6e8a9f64f536", "Special Instructions"), PickableDocketBO.WD_HandlingInstructions, Factory);
			}
		}

		#endregion

		#region IncoTerm

		public override CodeAndDescriptionWrapper IncoTerm
		{
			get
			{
				return new CodeAndDescriptionWrapper(PickableDocketBO.WD_INCO, PickableDocketBO.Lookups.INCOTerms, Factory);
			}
		}

		#endregion

		#region JobNumberHeading

		public override ZString JobNumberHeading
		{
			get
			{
				return Res.GetString("b48c75be-5917-4543-b2de-4aae45331e80", "Order Number");
			}
		}

		#endregion

		#region TransportCompany

		public override OrganisationWrapper TransportCompany => transportCompany ?? (transportCompany = PickableDocketBO.GetTransportCompany(Factory));

		OrganisationWrapper transportCompany;

		#endregion

		#endregion

		#region Implementation

		WhsPickableDocket PickableDocketBO
		{
			get { return pickableDocketBO ?? (pickableDocketBO = (WhsPickableDocket)WrappedBO); }
		}

		WhsPickableDocket pickableDocketBO;

		#endregion
	}
}
