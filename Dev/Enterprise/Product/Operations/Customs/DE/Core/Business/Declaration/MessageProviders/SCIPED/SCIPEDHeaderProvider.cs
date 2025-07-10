using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCIPEDHeaderProvider : MonthlyClosingDecHeaderProvider, ISCIPEDHeader
	{
		public SCIPEDHeaderProvider(CusReconDeclaration declaration, string messageRole)
			: base(declaration, messageRole)
		{
		}

		string IMonthlyClosingDecHeader.LocalClearanceProcedure => Factory.GetValue(ref localClearanceProcedure, () =>
		{
			var codeToSearch = string.Empty;
			if (Declaration.CRD_DeclarationType == MonthlyClosingDeclarationTypeList.Codes.VAV)
			{
				codeToSearch = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
			}
			else if (Declaration.CRD_DeclarationType == MonthlyClosingDeclarationTypeList.Codes.AAV)
			{
				codeToSearch = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			}
			return EntryInstruction.GetCusAuthorizationUsageNumber(codeToSearch);
		});
		CachedProperty<string> localClearanceProcedure;

		string IMonthlyClosingDecHeader.ProcedureAuthorization => ClearanceAuthorization?.CPH_Number;

		public IReadOnlyCollection<ISCIPEDBody> Bodies => bodies ?? (bodies = Declaration.CusReconEntries.Cast<CusReconEntry>().Select(e => new SCIPEDBodyProvider(e, IsModificationMessage)).Where(p => p.Lines.Any()).ToArray());
		IReadOnlyCollection<ISCIPEDBody> bodies;
	}
}
