using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;

namespace Enterprise.Customs.EU.Business
{
	public class ItineraryCountry : CusCodeData, IShortSequenceNumberLine
	{
		public ItineraryCountry(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(JobDeclaration));

		public new ItineraryCountryLookups Lookups => (ItineraryCountryLookups)base.Lookups;

		protected override CusCodeDataLookups GetNewLookups() => new ItineraryCountryLookups(this);

		public new ItineraryCountryValidation Validation => (ItineraryCountryValidation)base.Validation;

		protected override CusCodeDataValidation GetNewValidation() => new ItineraryCountryValidation(this);

		JobDeclaration Declaration => (JobDeclaration)Parent;

		[ReadOnly(true)]
		[ResourceStringData("F8227ECD-0504-4B9A-83FC-E1C6D9AFBA00", Caption = "Sequence")]
		public override ZShort CY_Order
		{
			get => base.CY_Order;
			set
			{
				var oldValue = CY_Order;
				base.CY_Order = ShortSequenceNumberGenerator.EnsureValidSequenceNumber(value);
				if (!IsCopying)
				{
					Declaration?.ItineraryCountries.SequenceNumberCalculator.RecalculateWhenRenumbered(this, oldValue);
				}
			}
		}

		[MaxLength(2)]
		[ResourceStringData("D8E7E871-42B4-4D2C-892B-25476CE62F3F", Caption = "Country")]
		[List(nameof(Lookups) + "." + nameof(ItineraryCountryLookups.CountryList))]
		public override ZString CY_Code { get => base.CY_Code; set => base.CY_Code = value; }

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
			var declaration = parentLoaders.LoadBusinessObject(Factory, CY_ParentTableCode, oldValue) as JobDeclaration;
			declaration?.ItineraryCountries.SequenceNumberCalculator.RecalculateWhenAboutToBeDetachedOrDeleted(this);
		}

		void AttachedToParent()
		{
			Declaration?.ItineraryCountries.SequenceNumberCalculator.RecalculateWhenAdded(this);
		}

		#endregion
	}
}
