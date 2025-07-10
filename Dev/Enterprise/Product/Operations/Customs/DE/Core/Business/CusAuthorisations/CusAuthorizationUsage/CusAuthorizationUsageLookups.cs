using System.Collections;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Customs.Business.CusAuthorizationHeaderTypeList;

namespace Enterprise.Customs.DE.Business
{
	public class CusAuthorizationUsageLookups : EU.Business.CusAuthorizationUsageLookups
	{
		public CusAuthorizationUsageLookups(CusAuthorizationUsage parent) : base(parent)
		{
		}

		public override ICollection CodeList
		{
			get
			{
				var result = new CodeDescriptionPairList();
				var instruction = Parent.Instruction as CusEntryInstruction;
				if (instruction != null && (JobDeclaration?.IsExport ?? false))
				{
					if (instruction.IsSDEExportOrSDEOutwardProcessing())
					{
						result.AddPair(Codes.SimplifiedDeclaration, Descriptions.SimplifiedDeclaration);
					}

					if (instruction.IsCCLExport())
					{
						result.AddPair(Codes.CentralizedClearance, Descriptions.CentralizedClearance);

						if (instruction.Style2ndDigitIs0())
						{
							result.AddPair(Codes.CustomsWarehousingCWP, Descriptions.CustomsWarehousingCWP);
							result.AddPair(Codes.CustomsWarehousingCW1, Descriptions.CustomsWarehousingCW1);
							result.AddPair(Codes.CustomsWarehousingCW2, Descriptions.CustomsWarehousingCW2);
						}
					}

					if (instruction.IsOPOOutwardProcessing())
					{
						result.AddPair(Codes.OutwardProcessing, Descriptions.OutwardProcessing);
					}

					if (ExportDeclarationTypeTimeList.IsMultipleDeclarationForExport(instruction.CEI_SubStyle))
					{
						result.AddPair(Codes.EntryOfDataInTheDeclarantsRecords, Descriptions.EntryOfDataInTheDeclarantsRecords);
					}
				}

				if (result.Count == 0)
				{
					result = (CodeDescriptionPairList)base.CodeList;
				}
				return result;
			}
		}
	}
}
