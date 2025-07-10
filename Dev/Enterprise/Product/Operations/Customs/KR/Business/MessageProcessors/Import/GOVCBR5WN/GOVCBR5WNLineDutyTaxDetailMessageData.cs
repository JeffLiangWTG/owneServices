using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5WNLineDutyTaxDetailMessageData : NonPersistentBusinessObject
	{
		public GOVCBR5WNLineDutyTaxDetailMessageData(BusinessObjectFactory factory) : base(factory)
		{
		}
		public BeforeOrAfterAmendment BeforeOrAfterAmendmentIndicator { get; set; }
		public ZString DutyTaxType { get; set; }
		public ZDecimal BaseValue { get; set; }
		public ZDecimal DutyTaxRate { get; set; }
		public ZDecimal ReducedOrExemptAmount { get; set; }
		public ZDecimal DutyTaxAmount { get; set; }
		public ZDecimal IncreaseDutyTaxAmount { get; set; }
	}

	public class GOVCBR5WNLineDutyTaxDetailMessageDataCollection : NonPersistentBusinessObjectCollection<GOVCBR5WNLineDutyTaxDetailMessageData>
	{
		public GOVCBR5WNLineDutyTaxDetailMessageDataCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public GOVCBR5WNLineDutyTaxDetailMessageData this[string index]
		{
			get
			{
				GOVCBR5WNLineDutyTaxDetailMessageData result = null;

				var regex = new Regex(@"^\s*""(?<dutyTaxType>CUD|IND|ENV|ACT|5AB|5DC|5CL|VAT),(\s*(?<beforeAfter>[1-2]{1})"")\s*$");
				var parameters = regex.Match(index);
				if (parameters.Success)
				{
					string dutyTaxType = parameters.Groups["dutyTaxType"].Value;
					string beforeAfter = parameters.Groups["beforeAfter"].Value;

					result = this.Cast<GOVCBR5WNLineDutyTaxDetailMessageData>().FirstOrDefault(x => x.DutyTaxType == dutyTaxType && ((int)x.BeforeOrAfterAmendmentIndicator).ToString() == beforeAfter);
				}

				return result;
			}
		}

		protected override BusinessObject CreateNonPersistentBusinessObject() => new GOVCBR5WNLineDutyTaxDetailMessageData(Factory);
		protected override bool AllowNewCore => false;
	}
}
