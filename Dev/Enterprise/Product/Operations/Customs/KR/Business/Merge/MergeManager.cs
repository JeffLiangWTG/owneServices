using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;

namespace Enterprise.Customs.KR.Business
{
	public class MergeManager : Customs.Business.MergeManager
	{
		public MergeManager(JobDeclaration declaration) : base(declaration)
		{ }

		new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
		protected override Customs.Business.LineMerger GetNewLineMergerCore() => new LineMerger(Declaration);
		protected override string GetReasonCannotMerge()
		{
			var result = base.GetReasonCannotMerge();

			if (result.Length == 0)
			{
				if (Declaration.IsLocalExport)
				{
					if (Declaration.JE_MessageSubType.IsEmpty)
					{
						result = Res.GetString("768B1148-6BD4-456B-8E19-84A7B3470DFA", "Please enter the Export Type to proceed with Merge.");
					}
					else if (!Declaration.Lookups.MessageSubTypeList.ContainsCode(Declaration.JE_MessageSubType))
					{
						result = Res.GetString("142FA38A-78D2-49BA-9849-63C371B8F0DD", "A valid value must be entered in the Export Type to proceed with Merge.");
					}
				}

				if (Declaration.UNIPASSDeclarantID.IsEmpty)
				{
					result = Res.GetString("D4034182-1487-4980-AED1-B4C66D04E17C", "You can't merge this declaration because UNIPASS Declarant ID is not entered.\r\nPlease Go to Registry -> Customs -> South Korea -> UNIPASS Declarant ID and enter a code.");
				}
				else if (Declaration.DeclarationMessagesHaveBeenSent(reloadMessages: true))
				{
					if (Declaration.IsLocalExport)
					{
						if (LocalExportTransactionNatureCodeList.Is5DQ((ZString)Declaration.JE_MessageSubTypeInfo.OriginalValue))
						{
							if (LocalExportTransactionNatureCodeList.Is5DP(Declaration.JE_MessageSubType))
							{
								result = GetLEXErrorMessage(ElectronicDocumentTypeList.Codes._5DP);
							}
						}
						else if (LocalExportTransactionNatureCodeList.Is5DP((ZString)Declaration.JE_MessageSubTypeInfo.OriginalValue))
						{
							if (LocalExportTransactionNatureCodeList.Is5DQ(Declaration.JE_MessageSubType))
							{
								result = GetLEXErrorMessage(ElectronicDocumentTypeList.Codes._5DQ);
							}
						}
					}

					if ((ZString)Declaration.JE_MessageTypeInfo.OriginalValue != Declaration.JE_MessageType)
					{
						result = Res.GetString("AF6ED118-5E54-4989-94FA-105B7979C728", "You have changed the shipment type and you can't merge this declaration any more as messages have already been sent.");
					}
				}
			}
			return result;
		}

		string GetLEXErrorMessage(string messageType)
		{
			return Res.GetString("13B31DC5-3A51-4C5C-9BE5-AC67338AB53C", "The merge of this Declaration failed because there is already a message sent and the Export Type was changed to the {0} type.", messageType);
		}
	}
}
