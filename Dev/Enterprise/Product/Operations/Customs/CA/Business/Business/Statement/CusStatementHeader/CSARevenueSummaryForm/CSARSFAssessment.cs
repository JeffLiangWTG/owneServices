using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CSARSFAssessment : AutoCSARSFAssessment, ICSARSFItem
	{
		public CSARSFAssessment()
		{
		}

		public CSARSFAssessment(CusStatementLine cusStatementLine) : base(cusStatementLine.Factory)
		{
			this.cusStatementLine = cusStatementLine;
			this.charge = cusStatementLine.CustomsAssessmentCharge;
		}

		readonly CusStatementLine cusStatementLine;
		readonly CusStatementLineCharge charge;

		[List(nameof(Lookups) + "+" + nameof(CSARSFAssessmentLookups.TypeList))]
		public override ZString Type
		{
			get => charge.B4_ChargeType;
			set
			{
				charge.B4_ChargeType = value;
				CheckMaximumLength(TypeInfo, value);
				TypeInfo.RefreshBinding();
			}
		}

		public override ZDecimal Amount
		{
			get => charge.B4_ChargeAmount.Round(2);
			set
			{
				charge.B4_ChargeAmount = value;
				AmountInfo.RefreshBinding();
			}
		}

		public override ZString PortCode
		{
			get => cusStatementLine.B3_EntryProcessPort;
			set
			{
				cusStatementLine.B3_EntryProcessPort = value;
				PortCodeInfo.RefreshBinding();
			}
		}

		public override ZString ReferenceNumber
		{
			get => cusStatementLine.B3_BrokerReference;
			set
			{
				cusStatementLine.B3_BrokerReference = value;
				ReferenceNumberInfo.RefreshBinding();
			}
		}

		public override void Delete()
		{
			charge?.Delete();
			cusStatementLine?.Delete();
			base.Delete();
		}

		public CSARSFAssessmentLookups Lookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new CSARSFAssessmentLookups(this);
				}
				return lookups;
			}
		}
		CSARSFAssessmentLookups lookups;

		#region ICSARSFItem

		ZString ICSARSFItem.LineItemNumber
		{
			get
			{
				return ReferenceNumber;
			}
		}

		ZDecimal ICSARSFItem.MonetaryAmount
		{
			get
			{
				return Amount;
			}
		}

		ZString ICSARSFItem.Type
		{
			get
			{
				return Type;
			}
		}

		ZString ICSARSFItem.PortCode
		{
			get
			{
				return PortCode;
			}
		}

		ZString ICSARSFItem.CodeID
		{
			get
			{
				return ZString.Empty;
			}
		}

		#endregion
	}
}
