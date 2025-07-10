using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.DE.Business
{
	public sealed class SCWPEDHeaderProvider : MonthlyClosingDecHeaderProvider, ISCWPEDHeader
	{
		public SCWPEDHeaderProvider(CusReconDeclaration declaration, string messageRole)
			: base(declaration, messageRole)
		{
		}

		string IMonthlyClosingDecHeader.LocalClearanceProcedure => Factory.GetValue(ref localClearanceProcedure, () =>
		{
			var codeToSearch = string.Empty;
			switch (Declaration.CRD_DeclarationType)
			{
				case MonthlyClosingDeclarationTypeList.Codes.VZL:
					codeToSearch = CusAuthorizationHeaderTypeList.Codes.SimplifiedDeclaration;
					break;
				case MonthlyClosingDeclarationTypeList.Codes.AZL:
					codeToSearch = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
					break;
			}
			return EntryInstruction.GetCusAuthorizationUsageNumber(codeToSearch);
		});
		CachedProperty<string> localClearanceProcedure;

		protected override string GetProcedureAuthorizationCore()
		{
			string result = null;
			var permit = Factory.Load<CusAuthorisationHeader>(Declaration.CRD_CPH_ReconClearanceAuthorisation);
			if (permit != null)
			{
				result = permit.CPH_Number;
			}
			return result;
		}

		public IReadOnlyCollection<ISCWPEDBody> Bodies => bodies ?? (bodies = Declaration.CusReconEntries.Cast<CusReconEntry>().Select(e => new SCWPEDBodyProvider(e, IsModificationMessage)).Where(p => p.Lines.Any()).ToArray());
		IReadOnlyCollection<ISCWPEDBody> bodies;
	}
}
