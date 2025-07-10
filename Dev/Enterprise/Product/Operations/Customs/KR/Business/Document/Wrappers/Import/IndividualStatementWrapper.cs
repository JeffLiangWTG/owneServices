using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.KR;
using Enterprise.Customs.KR.Messaging;
using Enterprise.DocumentEngineIntegration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class IndividualStatementWrapper : NonPersistentBusinessObject,
		IVisualizerNoteSupporter
	{
		public IndividualStatementWrapper(CusStatementHeader statementHeader)
		{
			IndividualInvoice = statementHeader;
		}

		#region IVisualizerNoteSupporter members
		ZGuid IVisualizerNoteSupporter.PK => IndividualInvoice.PK;
		ZGuid IVisualizerNoteSupporter.ChildBusinessObjectPK => ZGuid.Empty;
		string IVisualizerNoteSupporter.TableCode => CusStatementHeaderSchema.Constants.Prefix;
		#endregion

		public ZDecimal TotalGrossWeightInKG { get; set; }
		public ZInt TotalPackQty { get; set; }
		public ZDateTime DeclarationDate { get; set; }
		public OrganizationDocWrapper Declarant { get; set; }
		public ZString DeclarantCompanyName => Declarant?.CompanyName ?? ZString.Empty;
		public ZString DeclarantPhoneNumber => Declarant?.PhoneNumber ?? ZString.Empty;
		public ZString HouseBillNumber { get; set; }
		public ZString FormattedImportDeclarationNumber => MessageFunctions.GetFormattedEntryNumber(IndividualInvoice?.FirstLine?.B3_EntryNum ?? ZString.Empty, KRJobMessageTypeList.Codes.Import);
		public ZString CustomsOfficeName { get; set; }

		public CusStatementHeader IndividualInvoice { get; }
		public ZString FormattedStatementNumberFirstLine => GetSeparatedStatementNumber().First();
		public ZString FormattedStatementNumberSecondLine => GetSeparatedStatementNumber().Last();
		ZString[] GetSeparatedStatementNumber()
		{
			var result = new ZString[2];
			var statementNumber = IndividualInvoice?.B2_StatementNumber ?? ZString.Empty;
			if (!string.IsNullOrEmpty(statementNumber))
			{
				switch (statementNumber.Length)
				{
					case 15:
						result = [ZString.Empty, statementNumber];
						break;
					case 19:
						result = [statementNumber.SubstringSafe(0, 4), statementNumber.SubstringSafe(4)];
						break;
				}
				result[1] = MessageFunctions.GetFormattedNumber(result[1], [0, 3, 5, 7, 8, 14]);
			}
			return result;
		}
	}
}
