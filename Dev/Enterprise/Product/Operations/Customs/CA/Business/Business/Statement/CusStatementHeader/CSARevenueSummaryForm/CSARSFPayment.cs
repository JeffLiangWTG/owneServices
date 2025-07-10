using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business
{
	public class CSARSFPayment : AutoCSARSFPayment, ICSARSFItem
	{
		public CSARSFPayment()
		{
		}

		public CSARSFPayment(CusStatementLine statementLine, ZString type, ZString codeId, ZString description)
			: base(statementLine.Factory)
		{
			using (SuspendSettingHasChanges())
			{
				this.statementLineCharge = statementLine.Charges.Cast<CusStatementLineCharge>().FirstOrDefault(x => x.B4_ChargeType == codeId);
				if (statementLineCharge == null)
				{
					statementLineCharge = statementLine.Charges.AddNew();
					statementLineCharge.B4_ChargeType = codeId;
				}
				this.Type = type;
				this.Code = description.Substring(0, 5);
				this.Description = description.Substring(5);
				this.CodeID = codeId;
			}
		}

		readonly CusStatementLineCharge statementLineCharge;

		public bool AlwaysReturnTrue
		{
			get { return true; }
		}

		bool IsAmountReadOnly => this.IsCalculatedAutomatically();

		[ReadOnlyMember(nameof(AlwaysReturnTrue))]
		public override ZString Code
		{
			get => base.Code;
			set => base.Code = value;
		}

		[ReadOnlyMember(nameof(AlwaysReturnTrue))]
		public override ZString Description
		{
			get => base.Description;
			set => base.Description = value;
		}

		[ReadOnlyMember(nameof(IsAmountReadOnly))]
		public override ZDecimal Amount
		{
			get
			{
				return statementLineCharge.B4_ChargeAmount.Round(2);
			}
			set
			{
				statementLineCharge.B4_ChargeAmount = value;
				base.AmountInfo.RefreshBinding();
			}
		}

		#region ICSARSFItem

		ZString ICSARSFItem.LineItemNumber
		{
			get
			{
				return Code;
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
				return ZString.Empty;
			}
		}

		ZString ICSARSFItem.CodeID
		{
			get
			{
				return CodeID;
			}
		}

		#endregion
	}
}
