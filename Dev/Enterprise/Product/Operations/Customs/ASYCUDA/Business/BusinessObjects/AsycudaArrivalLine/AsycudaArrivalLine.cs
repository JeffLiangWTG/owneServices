using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public class AsycudaArrivalLine : ManifestBase.AsycudaArrivalLine
		, Integration.Customs.ASYCUDA.IAsycudaArrivalLine
	{
		public AsycudaArrivalLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : ManifestBase.AsycudaArrivalLine.Schema
		{
			public const string ATL_BillNumber = "ATL_BillNumber";
		}

		[List(nameof(ManifestHeader) + "." + nameof(AsycudaManifestHeader.Bills))]
		public override ZGuid ATL_ABL_AsycudaBill
		{
			get => base.ATL_ABL_AsycudaBill;
			set => base.ATL_ABL_AsycudaBill = value;
		}

		public AsycudaBill Bill
		{
			get
			{
				if (!IsDeleted && (fBill == null || fBill.PK != ATL_ABL_AsycudaBill))
				{
					fBill = ATL_ABL_AsycudaBill.IsValid ? Factory.Load<AsycudaBill>(ATL_ABL_AsycudaBill) : null;
				}

				return fBill != null && !fBill.IsDeleted ? fBill : null;
			}
		}
		AsycudaBill fBill;

		[ResourceStringData("ArrivalDetails|3788A4E1-01CA-4BEB-9DF5-864E1D3742F0", Caption = "Bill No.")]
		public ZString ATL_BillNumber => Bill?.ABL_BillNumber ?? ZString.Empty;
		public ZPropertyInfo ATL_BillNumberInfo => GetZPropertyInfo(Schema.ATL_BillNumber);

		[ResourceStringData("ArrivalDetails|45C28A52-776F-4659-9D11-C3DE124773AF", Caption = "Cargo Status")]
		public override ZString ATL_CargoStatus { get => base.ATL_CargoStatus; set => base.ATL_CargoStatus = value; }

		[ResourceStringData("ArrivalDetails|348834A9-28B2-4E4E-8646-68D1FB032B9C", Caption = "Reference")]
		public override ZString ATL_Reference { get => base.ATL_Reference; set => base.ATL_Reference = value; }

		public new AsycudaArrivalHeader ArrivalHeader
		{
			get
			{
				if (arrivalHeader == null)
				{
					arrivalHeader = Factory.Load<AsycudaArrivalHeader>(ATL_ATH);
				}
				return arrivalHeader;
			}
		}
		AsycudaArrivalHeader arrivalHeader;

		public AsycudaManifestHeader ManifestHeader
		{
			get
			{
				if (manifestHeader == null)
				{
					manifestHeader = ArrivalHeader.ManifestHeader;
				}
				return manifestHeader;
			}
		}
		AsycudaManifestHeader manifestHeader;

		public new AsycudaArrivalLineValidation Validation => (AsycudaArrivalLineValidation)base.Validation;

		protected override ManifestBase.AsycudaArrivalLineValidation GetNewValidation() => new AsycudaArrivalLineValidation(this);

		public new AsycudaArrivalLineLookups Lookups => (AsycudaArrivalLineLookups)base.Lookups;

		protected override ManifestBase.AsycudaArrivalLineLookups GetNewLookups() => new AsycudaArrivalLineLookups(this);

		[ResourceStringData("ArrivalDetails|3D7D6222-768C-42A0-BE3B-95E65951AB76", Caption = "Arrival Qty.")]
		public override ZInt ATL_Quantity { get => base.ATL_Quantity; set => base.ATL_Quantity = value; }

		[ResourceStringData("ArrivalDetails|A230E0E0-C723-4E78-B0CF-E33FC67FAAAD", Caption = "Expected Qty.")]
		public ZInt ATL_ExpectedQty => Factory.GetValue(ref expectedQty, () =>
		{
			var result = ZInt.Zero;

			if (ATL_ABL_AsycudaBill != System.Guid.Empty)
			{
				var bill = ManifestHeader.Bills.OfType<AsycudaBill>().FirstOrDefault(x => x.PK == ATL_ABL_AsycudaBill);
				ZInt billQty = bill?.ABL_ManifestQty ?? 0;
				if (billQty > 0)
				{
					result = billQty - GetExpectedQtyFromArrivalHeader(bill);
				}
			}
			return result;
		});

		CachedProperty<ZInt> expectedQty;

		public ZPropertyInfo ATL_ExpectedQtyInfo
		{
			get { return GetZPropertyInfo(nameof(ATL_ExpectedQty)); }
		}

		ZInt GetExpectedQtyFromArrivalHeader(AsycudaBill bill)
		{
			ZInt result = ZInt.Zero;

			if (ManifestHeader != null)
			{
				var findFlightByDate = ManifestHeader.ArrivalHeaders.Cast<AsycudaArrivalHeader>().Where(x => x.ATH_ETAAtDischargePort < ArrivalHeader.ATH_ETAAtDischargePort);
				foreach (AsycudaArrivalHeader oneHeader in findFlightByDate)
				{
					var ealierFlight = oneHeader.ArrivalDetails.Find(x => x.ATL_ABL_AsycudaBill == bill.PK);
					foreach (AsycudaArrivalLine qty in ealierFlight)
					{
						result += qty.ATL_Quantity;
					}
				}
			}
			return result;
		}

		[List(nameof(Lookups) + "." + nameof(AsycudaArrivalLineLookups.WeightUQList))]
		[ResourceStringData("ArrivalDetails|ADC7B9AD-C3E2-459B-8F1B-E74D7AB23F4F", Caption = "Weight Unit")]
		public override ZString ATL_WeightUQ { get => base.ATL_WeightUQ; set => base.ATL_WeightUQ = value; }

		[ResourceStringData("ArrivalDetails|4BDC9D2C-54D1-4370-A65C-BFE4612684BB", Caption = "Arrived Weight")]
		public override ZDecimal ATL_Weight { get => base.ATL_Weight; set => base.ATL_Weight = value; }

		[List(nameof(Lookups) + "." + nameof(AsycudaArrivalLineLookups.Packs))]
		public override ZGuid ATL_APA_AsycudaPack
		{
			get => base.ATL_APA_AsycudaPack;
			set => base.ATL_APA_AsycudaPack = value;
		}

		public AsycudaPack Pack
		{
			get
			{
				if (!IsDeleted && (fPack == null || fPack.PK != ATL_APA_AsycudaPack))
				{
					fPack = ATL_APA_AsycudaPack.IsValid ? Factory.Load<AsycudaPack>(ATL_APA_AsycudaPack) : null;
				}

				return fPack != null && !fPack.IsDeleted ? fPack : null;
			}
		}
		AsycudaPack fPack;
	}
}
