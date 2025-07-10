using System.ComponentModel;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.OperationalActions
{
	public class CreditCODDataObject : AutoCreditCODDataObject
	{
		public CreditCODDataObject(FrCreditCODApplicator header, BusinessObjectFactory factory) : base(factory)
		{
			this.Header = header;
		}
		public FrCreditCODApplicator Header;

		public CusEntryHeader ReleasingEntryHeader => Factory.GetCachedValue("FrCreditCODItemApplicator_ReleasingEntryHeader_" + ReleasingEntryReference, delegate
		{
			if (ReleasingEntryReference.IsEmpty)
			{
				return null;
			}

			return (CusEntryHeader)CusEntryHeader.LoadForBGMReference(Factory, ReleasingEntryReference);
		});

		public CusEntryHeader PreviousEntryHeader => Factory.GetCachedValue("FrCreditCODItemApplicator_PreviousEntryHeader_" + PreviousEntryReference, delegate
		{
			if (PreviousEntryReference.IsEmpty)
			{
				return null;
			}

			return (CusEntryHeader)CusEntryHeader.LoadForBGMReference(Factory, PreviousEntryReference);
		});

		public CusEntryLine PreviousEntryLine => Factory.GetCachedValue("FrCreditCODItemApplicator_PreviousEntryLine_" + PreviousEntryReference + "_" + PreviousEntryLineNo, delegate
		{
			if (PreviousEntryReference.IsEmpty || PreviousEntryLineNo.IsEmpty)
			{
				return null;
			}

			return PreviousEntryHeader?.AllEntryLines.Cast<CusEntryLine>().FirstOrDefault(x => x.CL_LineNumber == PreviousEntryLineNo);
		});

		[List(nameof(Lookups) + "." + nameof(CreditCODDataObjectLookups.CreditMethodList))]
		public override ZString CreditMethod
		{
			get => base.CreditMethod;
			set
			{
				var oldValue = CreditMethod;
				base.CreditMethod = value;

				if (oldValue != CreditMethod)
				{
					if (CreditMethod == CreditMethodList.Codes.CreditPreviousEntry)
					{
						PreviousEntryLineNo = ZInt.Zero;
						Amount = ZDecimal.Zero;
					}
					else if (CreditMethod == CreditMethodList.Codes.CreditPreviousEntryLine)
					{
						PreviousEntryLineNo = ReleasingEntryHeader?.RandomEntryLine?.RandomLine?.JI_PreviousEntryLineNumber ?? ZInt.Zero;
						Amount = ZDecimal.Zero;
					}
					else if (CreditMethod == CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine)
					{
						PreviousEntryLineNo = ReleasingEntryHeader?.RandomEntryLine?.RandomLine?.JI_PreviousEntryLineNumber ?? ZInt.Zero;
					}
				}
			}
		}

		[ResourceStringData("b06616d4-343a-4789-a4dd-4d13f97a9c5f", Caption = "Description")]
		public ZString CreditMethodDescription => Lookups.CreditMethodList.GetDescriptionFromCode(CreditMethod);

		[List(nameof(Lookups) + "." + nameof(CreditCODDataObjectLookups.ReleasingEntryNoList))]
		public override ZString ReleasingEntryReference
		{
			get => base.ReleasingEntryReference;
			set
			{
				var oldValue = ReleasingEntryReference;
				base.ReleasingEntryReference = value;

				if (oldValue != ReleasingEntryReference)
				{
					var previousEntryNo = ReleasingEntryHeader?.RandomEntryLine?.RandomLine?.JI_PreviousEntryNumber ?? ZString.Empty;
					if (!previousEntryNo.IsEmpty)
					{
						var previousEntry = new Customs.Business.CusEntryHeader.Loader(Factory).FindByEntryNumberAndCurrentCompany(previousEntryNo);
						if (previousEntry != null)
						{
							PreviousEntryReference = previousEntry.CH_BGMReference;
						}
					}
					if (CreditMethod == CreditMethodList.Codes.CreditPreviousEntryLine || CreditMethod == CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine)
					{
						PreviousEntryLineNo = ReleasingEntryHeader?.RandomEntryLine?.RandomLine?.JI_PreviousEntryLineNumber ?? ZInt.Zero;
					}
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CreditCODDataObjectLookups.PreviousEntryNoList))]
		public override ZString PreviousEntryReference { get => base.PreviousEntryReference; set => base.PreviousEntryReference = value; }

		[ReadOnlyMember(nameof(PreviousEntryLineNoReadOnly))]
		public override ZInt PreviousEntryLineNo { get => base.PreviousEntryLineNo; set => base.PreviousEntryLineNo = value; }
		public bool PreviousEntryLineNoReadOnly => CreditMethod != CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine && CreditMethod != CreditMethodList.Codes.CreditPreviousEntryLine;

		[ReadOnlyMember(nameof(AmountReadOnly))]
		public override ZDecimal Amount { get => base.Amount; set => base.Amount = value; }
		public bool AmountReadOnly => CreditMethod != CreditMethodList.Codes.CreditPartialAmountFromPreviousEntryLine;

		[ReadOnly(true)]
		public override ZString Currency { get => base.Currency; set => base.Currency = value; }

		public CreditCODDataObjectLookups Lookups => new CreditCODDataObjectLookups(this);

		protected override CreditCODDataObjectValidation GetNewValidation()
		{
			return new CreditCODDataObjectValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			Currency = Core.Constants.CurrencyCodes.EuropeanUnion;
		}
	}
}
