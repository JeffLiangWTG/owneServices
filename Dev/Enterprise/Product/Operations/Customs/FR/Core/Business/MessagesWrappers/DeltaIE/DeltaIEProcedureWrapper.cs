using System.Collections.Generic;
using System.Collections.ObjectModel;
using CargoWise.Customs.FR.MessageDefinitions.DeltaIE.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.FR.Business.Declaration;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE
{
	public class DeltaIEProcedureWrapper : IProcedure
	{
		DeltaIEProcedureWrapper(JobComInvoiceLine invoiceLine)
		{
			this.customsOffice = invoiceLine.Declaration?.JE_CustomsOffice ?? string.Empty;
			this.procedure = invoiceLine.JI_Procedure;
		}
		readonly ZString procedure;
		readonly string customsOffice;

		public static DeltaIEProcedureWrapper New(JobComInvoiceLine invoiceLine) => invoiceLine == null ? null : new DeltaIEProcedureWrapper(invoiceLine);

		public ICollection<IAdditionalProcedure> AdditionalProcedure => additionalProcedure ?? (additionalProcedure = GetAdditionalPocedure());
		ICollection<IAdditionalProcedure> additionalProcedure;

		ICollection<IAdditionalProcedure> GetAdditionalPocedure()
		{
			var result = new Collection<IAdditionalProcedure>();

			var additionalProcedure = procedure.SubstringSafe(4, 3);

			if (!additionalProcedure.IsEmpty)
			{
				result.Add(AdditionalProcedureWrapper.New(additionalProcedure, customsOffice));
			}

			return result;
		}

		public string PreviousProcedure => previousProcedure ?? (previousProcedure = procedure.SubstringSafe(2, 2));
		string previousProcedure;

		public string RequestedProcedure => requestedProcedure ?? (requestedProcedure = procedure.SubstringSafe(0, 2));
		string requestedProcedure;
	}
}
