using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.ProductCatalog.Outgoing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BR.Business.ProductCatalog
{
	public class ForeignOperatorProvider : IForeignOperator
	{
		public ForeignOperatorProvider(ForeignOperatorMessageSendingObject sendingObject)
		{
			//VPS fix IDE0052 - removed unread variables
			this.sendingObject = Argument.NotNull(sendingObject, nameof(sendingObject));
			ForeignOperator = Argument.NotNull(sendingObject.ForeignOperator, nameof(sendingObject.ForeignOperator));
			Owner = Argument.NotNull(ForeignOperator.Owner, nameof(ForeignOperator.Owner));
			OrgHeaderForeignOperator = ForeignOperator.ForeignOperator;
		}
		readonly ForeignOperatorMessageSendingObject sendingObject;

		OrgHeader OrgHeaderForeignOperator { get; set; }
		OrgHeader Owner { get; set; }
		CusBRForeignOperator ForeignOperator { get; set; }

		public int Sequence => 1;

		public string RootCpfCnpj => Owner.GetRootCNPJFromCNPJ();

		public string Code => ForeignOperator.BFR_AuthorityIdentifier;

		public string Version => string.Empty;

		public string RegistrationNumber => OrgHeaderForeignOperator.GetTinCode();

		public string Name => OrgHeaderForeignOperator?.OH_FullName ?? string.Empty;

		public string Situation
		{
			get
			{
				switch (sendingObject.Action)
				{
					case ActionList.Codes.Activate:
					case ActionList.Codes.CreateNewVersion:
						return Constants.Situation.Active;
					default:
						return Constants.Situation.Deactive;
				}
			}
		}

		public string Address => OrgHeaderForeignOperator != null ? OrgHeaderForeignOperator.MainAddress.Address1 + " " + OrgHeaderForeignOperator.MainAddress.Address2 : string.Empty;

		public string City => OrgHeaderForeignOperator?.CityName ?? string.Empty;

		public string SubDivisionCountryCode => OrgHeaderForeignOperator != null ? OrgHeaderForeignOperator.MainAddress.OA_RN_NKCountryCode + "-" + OrgHeaderForeignOperator.MainAddress.OA_State : string.Empty;

		public string Country => OrgHeaderForeignOperator?.MainAddress.OA_RN_NKCountryCode ?? string.Empty;

		public string ZipCode => OrgHeaderForeignOperator?.MainAddress.Postcode ?? string.Empty;

		public string InternalCode => OrgHeaderForeignOperator.GetInternalCode();

		public string Email => OrgHeaderForeignOperator.GetForeignOperatorEmail();

		public string ReferenceDate => string.Empty;

		public IEnumerable<IAdditionalIdentification> AdditionalIdentification => null;
	}
}
