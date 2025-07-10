using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	// Each required Warehouse business object should have it's own Population Strategy subclassed from here.
	public abstract class WhsPopulateStrategy
	{
		#region Construction

		protected WhsPopulateStrategy(BusinessObject businessObject)
		{
			if (GetType() != typeof(WhsPopulateEmptyJobStrategy))
			{
				Argument.NotNull(businessObject, "businessObject", "Argument can only be null, if the current type is WhsPoulateEmptyJobStrategy");
			}

			WhsDocketLabelControl docketLabel = businessObject as WhsDocketLabelControl;
			if (docketLabel != null)
			{
				this.docketLabel = docketLabel;
				wrappedBO = docketLabel.Docket;
			}
			else
			{
				this.wrappedBO = businessObject;
			}
		}

		public static WhsPopulateStrategy NewPopulateStrategy(BusinessObject bizO)
		{
			WhsPopulateStrategy result;

			var pick = bizO as WhsPick;
			if (pick != null)
			{
				result = new WhsPopulatePickStrategy(pick);
			}
			else
			{
				var docketLabel = bizO as WhsDocketLabelControl;
				var docket = docketLabel != null ? docketLabel.Docket : bizO as WhsDocket;
				result = docket != null ? GetNewPopulationStrategy(docket, bizO) : new WhsPopulateEmptyJobStrategy();
			}

			return result;
		}

		static WhsPopulateStrategy GetNewPopulationStrategy(WhsDocket docket, BusinessObject businessObject)
		{
			switch (docket)
			{
				case WhsAdjustment adjustment:
					return new WhsPopulateAdjustmentStrategy(adjustment);

				case WhsOrder order:
					return new WhsPopulateOrderStrategy(order);

				case WhsComponentOrder componentOrder:
					return new WhsPopulateComponentOrderStrategy(componentOrder);

				default:
					return new WhsPopulateEmptyJobStrategy();
			}
		}

		#endregion

		#region Properties
		// EVERY PROPERTY EVER REQUIRED FOR ANY WAREHOUSE DOCUMENT MUST RESIDE HERE.  (Please add in alphabetical order)

		#region ConsolidatedInvoiceRef

		public virtual ZString ConsolidatedInvoiceRef
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region Consignee

		public virtual OrganisationWrapper Consignee
		{
			get { return null; }
		}

		#endregion

		#region ConsigneeAddress

		public virtual AddressWrapper ConsigneeAddress
		{
			get { return null; }
		}

		#endregion

		#region Destination

		public virtual PlaceAndDateWrapper Destination
		{
			get { return null; }
		}

		#endregion

		#region HandlingInstructions

		public virtual LabelValuePairWrapper HandlingInstructions
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region IncoTerm

		public virtual CodeAndDescriptionWrapper IncoTerm
		{
			get { return null; }
		}

		#endregion

		#region InvoiceNumber

		public virtual ZString InvoiceNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region JobNumber

		public virtual ZString JobNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region JobNumberHeading

		public virtual ZString JobNumberHeading
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region PickMethod

		public virtual LabelValuePairWrapper PickMethod
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region SecondaryHeading

		public virtual ZString SecondaryHeading
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region SecondaryNumber

		public virtual ZString SecondaryNumber
		{
			get { return SecondaryReference.Value; }
		}

		#endregion

		#region ServiceLevel

		public virtual CodeAndDescriptionWrapper ServiceLevel
		{
			get { return null; }
		}

		#endregion

		#region SecondaryReference

		public virtual LabelValuePairWrapper SecondaryReference
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#region TotalNumberOfLabels

		public virtual ZInt TotalNumberOfLabels
		{
			get
			{
				return ZInt.Zero;
			}
		}

		#endregion

		#region TotalNumberOfPackageLabels

		public virtual ZInt TotalNumberOfPackageLabels
		{
			get { return ZInt.Zero; }
		}

		#endregion

		#region TransportCompany

		public virtual OrganisationWrapper TransportCompany
		{
			get { return null; }
		}

		#endregion

		#region TransportReference

		public virtual LabelValuePairWrapper TransportReference
		{
			get { return LabelValuePairWrapper.Empty; }
		}

		#endregion

		#endregion

		#region Implementation

		internal BusinessObject WrappedBO
		{
			get { return wrappedBO; }
		}
		readonly BusinessObject wrappedBO;

		internal BusinessObjectFactory Factory
		{
			get { return WrappedBO.Factory; }
		}

		internal WhsDocketLabelControl DocketLabel
		{
			get { return docketLabel; }
		}
		readonly WhsDocketLabelControl docketLabel;

		#endregion
	}

	// WhsPopulateStrategy and WhsPoulateEmptyJobStrategy are effectively the same thing.
	public sealed class WhsPopulateEmptyJobStrategy : WhsPopulateStrategy
	{
		public WhsPopulateEmptyJobStrategy()
			: base(null)
		{
		}
	}
}
