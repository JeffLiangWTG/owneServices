using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5TVLineMessageData : NonPersistentBusinessObject
	{
		public GOVCBR5TVLineMessageData(GOVCBR5TVMessageData parent) : base(parent.Factory)
		{
			Parent = parent;
		}
		public GOVCBR5TVMessageData Parent { get; }

		public ZShort EntryLineNo { get; set; }
		public ZInt BeforeOrAfterAmendmentIndicator { get; set; }
		public ZString HSCode { get; set; }
		public ZString HSCodeDescription
		{
			get
			{
				if (!HSCode.IsEmpty && hSCodeDescription.IsEmpty)
				{
					hSCodeDescription = new TariffView.Loader(Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.KoreaSouth, HSCode, Parent.ImportDeclarationDate == ZDateTime.Empty ? ZDateTime.Today : Parent.ImportDeclarationDate)?.ZZ1_Description ?? ZString.Empty;
				}
				return hSCodeDescription;
			}
		}
		ZString hSCodeDescription;
		public ZDecimal DutyRate { get; set; }
		public ZDecimal CustomsValueKRW { get; set; }
		public ZDecimal DutyAmount { get; set; }
		public ZDecimal VAT { get; set; }
		public ZDecimal IndividualConsumptionTax { get; set; }
		public ZDecimal LiquorTax { get; set; }
		public ZDecimal TransportationTax { get; set; }
		public ZDecimal SpecialAgriculturalTax { get; set; }
		public ZDecimal EducationTax { get; set; }
		public ZDecimal LinesTotalTax { get; set; }
	}
	public class GOVCBR5TVLineMessageDataCollection : NonPersistentBusinessObjectCollection<GOVCBR5TVLineMessageData>
	{
		public GOVCBR5TVLineMessageDataCollection(GOVCBR5TVMessageData parent) : base(parent.Factory)
		{
			Argument.NotNull(parent, nameof(parent));
			Parent = parent;
		}

		public GOVCBR5TVMessageData Parent { get; }
		protected override BusinessObject CreateNonPersistentBusinessObject() => new GOVCBR5TVLineMessageData(Parent);
		protected override bool AllowNewCore => false;
	}
}
