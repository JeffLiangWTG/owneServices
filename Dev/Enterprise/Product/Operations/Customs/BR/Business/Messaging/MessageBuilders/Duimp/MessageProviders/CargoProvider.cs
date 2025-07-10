using CargoWise.Common;
using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using static Enterprise.Customs.BR.Business.Constants;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class CargoProvider : ICargo
	{
		CargoProvider(CusEntryInstruction entryInstruction)
		{
			this.entryInstruction = Argument.NotNull(entryInstruction, nameof(entryInstruction));
			declaration = Argument.NotNull(entryInstruction.JobDeclaration, nameof(entryInstruction.JobDeclaration));
		}

		public static CargoProvider New(CusEntryInstruction entryInstruction) => entryInstruction == null ? null : new CargoProvider(entryInstruction);

		readonly CusEntryInstruction entryInstruction;
		readonly JobDeclaration declaration;

		public string Identification => entryInstruction.BillNumber;

		public string DeclaredUnitCode => declaration.JE_CustomsOffice;

		public string DispatchModality => declaration.JE_DispatchModality;

		public string TypeOfIdentification
		{
			get
			{
				if (!entryInstruction.BillType.IsEmpty)
				{
					return declaration.IsTransportByWater switch
					{
						true => CargoIdentificationType.CE,
						_ => CargoIdentificationType.RUC
					};
				}
				return string.Empty;
			}
		}

		public ICharge Insurance => declaration.JE_DispatchModality.IsEmpty ? null : fInsurance ??= InsuranceProvider.New(entryInstruction);
		ICharge fInsurance;

		public ICharge Freight => null;

		public string CountryOfOrigin => declaration.IsCargoProvenanceAvailable ? declaration.JE_GoodsOrigin : null;
	}
}
