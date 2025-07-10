using CargoWise.EntityFramework;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EdiDepositAdjustCollection : ActiveBusinessObjectCollection<EdiDepositAdjust>
	{
		public EdiDepositAdjustCollection(EDIOrgHeader parent)
			: base(parent.Factory, parent, new ZQuery(), EdiDepositAdjustSchema.DEA_OH)
		{
		}

		public EdiDepositAdjustCollection(EDIOrgHeader parent, string chargeCode)
			: base(parent.Factory, parent, new ZQuery(EdiDepositAdjustSchema.DEA_ChargeCode, chargeCode), EdiDepositAdjustSchema.DEA_OH)
		{
			ChargeCode = chargeCode;
		}

		protected override void SetDefaultsForNewElementCore(EdiDepositAdjust newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.DEA_GC = Env.CurrentCompany.PK;
			if (ChargeCode != null)
			{
				newElement.DEA_ChargeCode = ChargeCode;
			}
		}

		readonly string ChargeCode;
	}
}

