using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class Import5TMHeaderCreator
	{
		public Import5TMHeader Create(CusEntryHeader entry, JobDeclarationMiscMessageSendingObject messageSendingObject)
		{
			var import5TMData = new Import5TMHeader();
			import5TMData.ImportDeclarationNumber = messageSendingObject.EntryNumber;

			PopulateImport5TMPayer(entry.Declaration, import5TMData);
			PopulateImport5TMEntryLine(messageSendingObject, import5TMData);

			return import5TMData;
		}

		void PopulateImport5TMPayer(JobDeclaration declaration, Import5TMHeader import5TMData)
		{
			if (declaration.DutyPayer != null)
			{
				import5TMData.Payer = new Organisation(RoleType.Payer)
				{
					IsIndividual = declaration.DutyPayer.GetIsIndividual(),
				};

				if (declaration.PayerBusinessNumber != null)
				{
					import5TMData.Payer.SetRegistrationIDNumbers(new IDNumberAndType[] { declaration.PayerBusinessNumber });
				}
			}
		}

		void PopulateImport5TMEntryLine(JobDeclarationMiscMessageSendingObject messageSendingObject, Import5TMHeader import5TMData)
		{
			var orderedEntryLines = messageSendingObject.MessageSendingEntryLines.Cast<MessageSendingEntryLineObject>().Where(x => x.IsGoldOrItsProduct).OrderBy(x => x.EntryLineNo);
			var entryLineDataList = new List<Import5TMLine>();

			foreach (MessageSendingEntryLineObject line in orderedEntryLines)
			{
				var entryLineData = new Import5TMLine()
				{
					EntryLineNo = line.EntryLineNo,
					HSCode = line.HSCode,
					HSDescription = line.HSDescription,
					ValueForVAT = line.ValueForVAT,
					VAT = line.VAT,
					NetWeightInKG = Core.Constants.Weight.Convert(line.NetWeightInKG, line.NetWeightUnit, Core.Constants.Weight.Kilograms)
				};
				entryLineDataList.Add(entryLineData);
			}
			import5TMData.EntryLines = entryLineDataList.Count > 0 ? entryLineDataList.ToArray() : null;
		}
	}
}
