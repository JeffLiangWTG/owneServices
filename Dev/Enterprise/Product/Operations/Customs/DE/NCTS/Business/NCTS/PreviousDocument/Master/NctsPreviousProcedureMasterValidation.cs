using System;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NctsPreviousProcedureMasterValidation : ZValidation
	{
		public NctsPreviousProcedureMasterValidation(NctsPreviousProcedureMaster parent)
			: base(parent)
		{
		}

		NctsPreviousProcedureMaster Parent => (NctsPreviousProcedureMaster)ParentFilter;

		public override Type AutoValidationType => typeof(NctsPreviousProcedureMasterValidation);

		public override void ValidateAll()
		{
			ValidateCSI_Procedure();
			ValidateAuthorizationNumber();
			ValidateCSI_CustomsOffice();
		}

		public void ValidateCSI_Procedure()
		{
			ValidateCalculatedProperty(Parent.CSI_ProcedureInfo);
		}

		protected void CheckCSI_Procedure()
		{
			var parent = Parent;
			var procedure = parent.CSI_Procedure;
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.CSI_ProcedureInfo);
			var goodsItem = (parent.Parent as NctsDepartureCargoDesc);
			if (goodsItem != null
				&& !procedure.IsEmpty
				&& goodsItem.PreviousDocuments.All(d => d.CSI_Code != procedure))
			{
				parent.CSI_ProcedureInfo.AddMessageError(Res.GetString("e8699ab7-2a77-44dc-9055-d04c91ad0c67", "[NR0052] Type {0} is required in Tab Previous Documents of this Goods Item to use this Previous Procedure.", procedure));
			}
		}

		public void ValidateAuthorizationNumber()
		{
			ValidateCalculatedProperty(Parent.AuthorizationNumberInfo);
		}

		protected void CheckAuthorizationNumber()
		{
			var parent = Parent;
			var procedure = parent.CSI_Procedure;
			if (NctsPreviousDocumentHelper.IsAvailable(procedure, NctsPreviousDocument.Schema.AuthorizationNumber))
			{
				var info = parent.AuthorizationNumberInfo;

				switch (procedure)
				{
					case NctsPreviousProcedureList.Codes._9DEZ:
					case NctsPreviousProcedureList.Codes._9DEY when !parent.SimplifiedGrantAuthorizationFlag:
						ListValidation.MessageErrorIfInvalidCodeOrEmpty(info);
						break;
					case NctsPreviousProcedureList.Codes._9DEY:
						ListValidation.MessageErrorIfInvalidCode(info);
						break;
				}
			}
		}

		public void ValidateCSI_CustomsOffice()
		{
			ValidateCalculatedProperty(Parent.CSI_CustomsOfficeInfo);
		}

		protected void CheckCSI_CustomsOffice()
		{
			var parent = Parent;
			if (NctsPreviousDocumentHelper.IsAvailable(parent.CSI_Procedure, NctsPreviousDocument.Schema.CSI_CustomsOffice) && parent.CSI_Procedure == NctsPreviousProcedureList.Codes._9DEY)
			{
				ListValidation.MessageErrorIfInvalidCode(parent.CSI_CustomsOfficeInfo, ResString.GetMultilingualString("1EBD613B-1520-4001-B2CF-AFD08F887B3A", "The entered Customs Office is not a Main Office in Germany."));
				if (parent.SimplifiedGrantAuthorizationFlag)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parent.CSI_CustomsOfficeInfo);
				}
			}
		}
	}
}
