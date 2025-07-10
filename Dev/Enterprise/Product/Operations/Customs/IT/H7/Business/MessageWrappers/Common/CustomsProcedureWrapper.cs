using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Common;
using CargoWise.Customs.IT.MessageContracts.Declaration;

namespace Enterprise.Customs.IT.H7.Business;

public sealed class CustomsProcedureWrapper : ICustomsProcedure
{
	public CustomsProcedureWrapper(AsycudaBill bill)
	{
		this.bill = bill;
	}

	readonly AsycudaBill bill;

	public string Procedure => null;

	public string PreviousProcedure => null;

	public IReadOnlyCollection<string> AdditionalProcedures
	{
		get
		{
			if (additionalProcedures == null)
			{
				var additionalProcedureList = new List<string>();
				bill.ABL_Procedure.Split("+")
					.Where(p => !p.IsEmpty)
					.ForEach(p => additionalProcedureList.Add(p));

				additionalProcedures = additionalProcedureList.AsReadOnly();
			}

			return additionalProcedures;
		}
	}

	ReadOnlyCollection<string> additionalProcedures;
}
