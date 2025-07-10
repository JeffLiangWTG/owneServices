using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class RouteEntry : CusCodeData
		, IShortSequenceNumberLine
		, ISynchroniserReadOnlyMembersProvider
	{
		public RouteEntry(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CY_Type = CusCodeDataTypeList.Codes.EUICS2RouteEntry;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("FAA57C5B-AF69-4474-BA14-64D80B243A33", "Route Entry");

		AsycudaManifestHeader Header => (AsycudaManifestHeader)Parent;

		protected override TypeLoaderCollection parentLoaders => new TypeLoaderCollection(typeof(AsycudaManifestHeader));

		public new RouteEntryLookups Lookups => (RouteEntryLookups)base.Lookups;
		protected override CusCodeDataLookups GetNewLookups() => new RouteEntryLookups(this);

		public new RouteEntryValidation Validation => (RouteEntryValidation)base.Validation;
		protected override CusCodeDataValidation GetNewValidation() => new RouteEntryValidation(this);

		#region Properties

		[ResourceStringData("EU.ICS2.RouteEntry.LegNumber", Caption = "Leg Number", ShortCaption = "Leg")]
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

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(RouteEntryLookups.CountryCodeList))]
		[ResourceStringData("EU.ICS2.RouteEntry.CountryCode", Caption = "Country Code", ShortCaption = "Country")]
		public override ZString CY_Data
		{
			get => base.CY_Data;
			set => base.CY_Data = value;
		}

		[ResourceStringData("EU.ICS2.RouteEntry.CountryName", Caption = "Country Name")]
		public ZString CountryName
		{
			get
			{
				var result = string.Empty;

				if (!CY_Data.IsEmpty)
				{
					result = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, CY_Data))?.Description ?? string.Empty;
				}

				return result;
			}
		}

		public ZPropertyInfo CountryNameInfo => GetZPropertyInfo(nameof(CountryName));

		[List(nameof(Lookups) + "." + nameof(RouteEntryLookups.RefUNLOCOList))]
		[ResourceStringData("EU.ICS2.RouteEntry.Port", Caption = "Port")]
		public override ZString CY_Code
		{
			get => base.CY_Code;
			set
			{
				if (CY_Code != value)
				{
					base.CY_Code = value;
					CY_Data = Port?.Country?.Code ?? string.Empty;
				}
			}
		}

		public RefUNLOCO Port => Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, CY_Code);

		public bool IsInEU => Port?.IsInEU ?? false;

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

		#region ISynchroniserReadOnlyMembersProvider

		List<string> ISynchroniserReadOnlyMembersProvider.SynchroniserReadOnlyMembers => synchroniserReadOnlyMembers ?? (synchroniserReadOnlyMembers = new List<string> { Schema.CY_Order, Schema.CY_Code, Schema.CY_Data });
		List<string> synchroniserReadOnlyMembers;

		#endregion
	}
}
