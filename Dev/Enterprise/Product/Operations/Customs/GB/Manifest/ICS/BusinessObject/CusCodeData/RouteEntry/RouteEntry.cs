using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.GB.ICS.Business
{
	public class RouteEntry : CusCodeData
		, IShortSequenceNumberLine
	{
		public RouteEntry(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.IcsRouteEntry;
		}

		AsycudaManifestHeaderBase Header => (AsycudaManifestHeaderBase)Parent;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(AsycudaManifestHeaderBase));

		public new RouteEntryLookups Lookups => (RouteEntryLookups)base.Lookups;
		protected override CusCodeDataLookups GetNewLookups() => new RouteEntryLookups(this);

		public new RouteEntryValidation Validation => (RouteEntryValidation)base.Validation;
		protected override CusCodeDataValidation GetNewValidation() => new RouteEntryValidation(this);

		#region Properties

		[List(nameof(Lookups) + "." + nameof(RouteEntryLookups.CountryCodeList))]
		[ResourceStringData("GB.ICS.RouteEntry.CY_Data", Caption = "Country/Region Code", MediumCaption = "Ctry/Rgn.", ShortCaption = "C/R")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		[ResourceStringData("GB.ICS.RouteEntry.CY_Order", Caption = "Leg Number", ShortCaption = "Leg")]
		public override ZShort CY_Order
		{
			get => base.CY_Order;
			set
			{
				var oldValue = CY_Order;
				base.CY_Order = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying)
				{
					Header?.Itinerary.SequenceNumberCalculator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}

		#endregion

		#region IShortSequenceNumberLine

		public ZShort SequenceNumber { get => CY_Order; set => CY_Order = value; }
		public ZGuid FKToHeader => CY_ParentID;

		public override ZGuid CY_ParentID
		{
			get => base.CY_ParentID;
			set
			{
				var oldValue = CY_ParentID;
				base.CY_ParentID = value;
				if (!IsCopying && oldValue != CY_ParentID && !CY_ParentID.IsValid)
				{
					DetachedFromParent(oldValue);
				}
			}
		}

		public override ZString CY_ParentTableCode
		{
			get => base.CY_ParentTableCode;
			set
			{
				var oldValue = CY_ParentTableCode;
				base.CY_ParentTableCode = value;
				if (!IsCopying && oldValue != CY_ParentTableCode && !CY_ParentTableCode.IsEmpty)
				{
					AttachedToParent();
				}
			}
		}

		void DetachedFromParent(ZGuid oldValue)
		{
			var header = parentLoaders.LoadBusinessObject(Factory, CY_ParentTableCode, oldValue) as AsycudaManifestHeader;
			header?.Itinerary.SequenceNumberCalculator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		}

		void AttachedToParent()
		{
			Header?.Itinerary.SequenceNumberCalculator.RecalculateWhenAdded(this);
		}

		#endregion
	}
}
