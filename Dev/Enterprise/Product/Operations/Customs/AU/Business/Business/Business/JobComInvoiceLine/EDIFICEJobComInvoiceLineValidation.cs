using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class EDIFICEJobComInvoiceLineValidation : ImportJobComInvoiceLineValidation
	{
		public EDIFICEJobComInvoiceLineValidation(JobComInvoiceLine line)
			: base(line)
		{
			dutyCalculator = line.DutyCalculator;
		}

		public override bool NeedsCustomsUQ => false;

		public override ZString CustomsUQ => ZString.Empty;

		public override ZString SecondUQ => ZString.Empty;

		public override CodeDescriptionPairList LineValuationBasisList
		{
			get { return EDIFICEValuationBasis.Line_List; }
		}

		public override CodeDescriptionPairList LinePrefix_List
		{
			get
			{
				var parentLine = Parent.ParentLine;
				return Parent.Factory.GetCachedValue("AUEDIFICEI_LinePrefix_List_" + parentLine == null ? "P" : "C", () =>
					{
						var result = new CodeDescriptionPairList();
						if (parentLine != null)
						{
							result.AddPair(JobComInvoiceLine.LinePrefixString.Trailer, "Trailer");
						}
						else
						{
							result.AddPair(JobComInvoiceLine.LinePrefixString.Normal, "Normal");
							result.AddPair(JobComInvoiceLine.LinePrefixString.Parent, "Parent");
						}
						return result;
					});
			}
		}

		protected void ValidateParentTrailerPair()
		{
			if (Parent.JI_LinePrefix == JobComInvoiceLine.LinePrefixString.Parent)
			{
				if (Parent.ChildLine == null)
				{
					Parent.JI_LinePrefixInfo.AddMessageError("You indicated this line as parent, but there is no corresponding trailer line.");
				}
				else if (Parent.ChildLine.JI_LinePrefix != JobComInvoiceLine.LinePrefixString.Trailer)
				{
					Parent.JI_LinePrefixInfo.AddMessageError("This line has a trailer line, but the line prefix hasn't been set.");
				}
			}
			else if (Parent.JI_LinePrefix == JobComInvoiceLine.LinePrefixString.Trailer)
			{
				if (Parent.ParentLine == null)
				{
					Parent.JI_LinePrefixInfo.AddMessageError("You indicated this line as trailer, but there is no corresponding parent line.");
				}
				else if (Parent.ParentLine.JI_LinePrefix != JobComInvoiceLine.LinePrefixString.Parent)
				{
					if (Parent.ParentLine.ParentLine == Parent)
					{
						Parent.JI_LinePrefixInfo.AddMessageError(Parent.JI_ParentLineCode + " is not a parent line. You have to enter a 'P' in the line of the pair that is above the other line.");
					}
					else if (Parent.ParentLine.ParentLine != null)
					{
						Parent.JI_LinePrefixInfo.AddMessageError(Parent.JI_ParentLineCode + " has a parent line. Please remove 'T' from this line.");
					}
				}
			}
		}

		public override CodeDescriptionPairList InstrumentTypeList
		{
			get { return new CustomsInstrumentTypeList(); }
		}

		#region Check Methods

		protected override void CheckJI_Tariff()
		{
			base.CheckJI_Tariff();
			if (IsTariffMandatory)
			{
				new ImportTariffValidator().Validate(Parent.JI_TariffInfo, dutyCalculator, Parent.AggregatedZA_ORG, Parent.AddInfo.AggregatedZA_PRF, Parent.DateOfValuation);
			}
		}

		protected override void CheckJI_Calc_Invoice()
		{
			base.CheckJI_Calc_Invoice();
			if (Parent.ParentLine != null && Parent.ParentLine.JI_Calc_Invoice != Parent.JI_Calc_Invoice)
			{
				Parent.JI_Calc_InvoiceInfo.AddMessageError("Parent line " + Parent.ParentLine.JI_ParentLineCode + " belongs to a different invoice. Please change the invoice number of this trailer line.");
			}
		}

		protected override void CheckJI_LinePrefix()
		{
			base.CheckJI_LinePrefix();
			ListValidation.MessageErrorIfInvalidCode(Parent.JI_LinePrefixInfo, Parent.JI_LinePrefix_List);
			ValidateParentTrailerPair();

			ValidateJI_Calc_Invoice();
			ValidateJI_LinePrice();
		}

		protected override void CheckJI_LinePrice()
		{
			base.CheckJI_LinePrice();
			if (Parent.ParentLine != null && !Parent.JI_LinePrice.IsDefault)
			{
				Parent.JI_LinePriceInfo.AddMessageError("Trailer line cannot have a line price.");
			}
		}

		#endregion

		#region Implementation

		protected DutyCalculator dutyCalculator;

		#endregion
	}
}
