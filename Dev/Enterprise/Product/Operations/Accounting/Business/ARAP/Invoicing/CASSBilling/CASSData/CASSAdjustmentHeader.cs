using System.Globalization;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DataTransfer.Xml;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class CASSAdjustmentHeader : CASSData, IValueObject
	{
		public CASSAdjustmentHeader() : base(null)
		{
		}

		[XmlIgnore]
		[MaxLength(7)]
		public ZString AgentNumber { get; set; }

		[XmlIgnore]
		public ZDateTime BillingPeriodEnd { get; set; }

		[XmlIgnore]
		public ZInt InvoicePeriod
		{
			get
			{
				ZInt ccyypp = ZInt.Zero;

				if (!BillingPeriodEnd.IsEmpty)
				{
					var ccyy = BillingPeriodEnd.Year;
					var pp = BillingPeriodEnd.Day < 16 ? (BillingPeriodEnd.Month * 2 - 1) : BillingPeriodEnd.Month * 2;
					ccyypp = ZInt.Parse(string.Concat(ccyy, pp.ToString(CultureInfo.InvariantCulture).PadLeft(2, '0')));
				}

				return ccyypp;
			}
		}

		[XmlIgnore]
		public CASSAdjustmentLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new CASSAdjustmentLineCollection();
				}
				return lines;
			}
		}
		CASSAdjustmentLineCollection lines;

		[XmlIgnore]
		public bool IsSpecified
		{
			get { return true; }
		}

		[XmlIgnore]
		public bool ShouldCreateElementForEmptyValue { get; set; }
	}
}
