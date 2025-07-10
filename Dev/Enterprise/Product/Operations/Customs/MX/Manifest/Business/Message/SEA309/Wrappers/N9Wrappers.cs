using CargoWise.Common;
using CargoWise.Customs.MX.MessageContracts;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.MX.Manifest.Business
{
	internal class N9Wrappers : IN9ReferenceIdentification
	{
		readonly AsycudaBill bill;
		readonly ZBool isHBLReference;

		public N9Wrappers(AsycudaBill bill, bool isHBLReference)
		{
			this.bill = Argument.NotNull(bill, "asycudaBill cannot be null");
			this.isHBLReference = isHBLReference;
		}

		string IN9ReferenceIdentification.ReferenceIdentification => isHBLReference ? SEA309Constants.HBLReference : bill.Header.AMA_Nature == ShipmentTypeList.Codes.Import23 ? SEA309Constants.ImpReference : SEA309Constants.ExpReference;

		string IN9ReferenceIdentification.ReferenceIdentification2
		{
			get
			{
				if (isHBLReference)
				{
					var reference = new ZStringBuilder();
					reference.Append(SEA309Helper.CarrierCode(bill.Header));
					reference.Append(bill.Header.AMA_MasterBill);
					return reference.ToString();
				}
				else
				{
					return bill.Header.AMA_CustomsOffice;
				}
			}
		}

		string IN9ReferenceIdentification.FreeFormDescription => ZString.Empty;

		string IN9ReferenceIdentification.Date => ZString.Empty;

		string IN9ReferenceIdentification.Time => ZString.Empty;

		string IN9ReferenceIdentification.TimeCode => ZString.Empty;

		string IN9ReferenceIdentification.ReferenceNumber => ZString.Empty;

		string IN9ReferenceIdentification.ReferenceIdentification3 => ZString.Empty;

		string IN9ReferenceIdentification.ReferenceIdentification4 => ZString.Empty;

		string IN9ReferenceIdentification.ReferenceIdentification5 => ZString.Empty;

		string IN9ReferenceIdentification.ReferenceIdentification6 => ZString.Empty;

		string IN9ReferenceIdentification.ReferenceIdentification7 => ZString.Empty;

		string IN9ReferenceIdentification.ReferenceIdentification8 => ZString.Empty;
	}
}
