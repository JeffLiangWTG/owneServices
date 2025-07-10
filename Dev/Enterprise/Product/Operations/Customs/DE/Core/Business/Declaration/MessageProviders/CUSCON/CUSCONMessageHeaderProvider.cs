using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts.Import;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Messaging;

namespace Enterprise.Customs.DE.Business.Declaration
{
	public sealed class CUSCONMessageHeaderProvider : ImportMessageHeaderProvider
	{
		public CUSCONMessageHeaderProvider(CusEntryHeader entryHeader) : base(entryHeader)
		{
		}

		public override string MessageGroup
		{
			get
			{
				var result = string.Empty;
				var entryInstruction = EntryHeader.EntryInstruction;
				if (messageSubTypeConditionsDictionary.TryGetValue(entryInstruction.CEI_Style, out var subStyleMessageGroupCombinations))
				{
					if (subStyleMessageGroupCombinations.Item1.Contains(entryInstruction.CEI_SubStyle))
					{
						result = subStyleMessageGroupCombinations.Item2;
					}
				}
				return result;
			}
		}

		public override IImportHeader Header => header ?? (header = new CUSCONHeaderProvider(EntryHeader));
		IImportHeader header;

		readonly ImmutableDictionary<ZString, Tuple<ZString[], ZString>> messageSubTypeConditionsDictionary = new Dictionary<ZString, Tuple<ZString[], ZString>>
		{
			{ ImportEntryTypeList.Codes.SingleDeclarationFreeCirculation, new Tuple<ZString[], ZString>(new ZString[] { ImportSubStyleList.Codes.D, ImportSubStyleList.Codes.E }, ImportMessageSubTypeList.Codes.FreeCirculationPrematureInputSingleDeclaration)  },
			{ ImportEntryTypeList.Codes.EntryInDeclarantsRecordsFreeCirculation, new Tuple<ZString[], ZString>(new ZString[] { ImportSubStyleList.Codes.F }, ImportMessageSubTypeList.Codes.FreeCirculationSimplifiedPrematureDeclaration)  },
			{ ImportEntryTypeList.Codes.SimplifiedDeclarationFreeCirculation, new Tuple<ZString[], ZString>(new ZString[] { ImportSubStyleList.Codes.F }, ImportMessageSubTypeList.Codes.FreeCirculationSimplifiedPrematureDeclaration)  },
			{ ImportEntryTypeList.Codes.SingleDeclarationBondedWarehouse, new Tuple<ZString[], ZString>(new ZString[] { ImportSubStyleList.Codes.D }, ImportMessageSubTypeList.Codes.BondedWarehouseSinglePrematureDeclaration)  },
			{ ImportEntryTypeList.Codes.EntryInDeclarantsRecordsBondedWarehouse, new Tuple<ZString[], ZString>(new ZString[] { ImportSubStyleList.Codes.F }, ImportMessageSubTypeList.Codes.BondedWarehouseSimplifiedPrematureDeclaration)  },
			{ ImportEntryTypeList.Codes.SimplifiedDeclarationBondedWarehouse, new Tuple<ZString[], ZString>(new ZString[] { ImportSubStyleList.Codes.F }, ImportMessageSubTypeList.Codes.BondedWarehouseSimplifiedPrematureDeclaration)  },
			{ ImportEntryTypeList.Codes.SingleDeclarationInwardProcessing, new Tuple<ZString[], ZString>(new ZString[] { ImportSubStyleList.Codes.D }, ImportMessageSubTypeList.Codes.InwardProcessingSinglePrematureDeclaration)  },
			{ ImportEntryTypeList.Codes.EntryInDeclarantsRecordsInwardProcessing, new Tuple<ZString[], ZString>(new ZString[] { ImportSubStyleList.Codes.F }, ImportMessageSubTypeList.Codes.InwardProcessingSimplifiedPrematureDeclaration)  },
			{ ImportEntryTypeList.Codes.SimplifiedDeclarationInwardProcessing, new Tuple<ZString[], ZString>(new ZString[] { ImportSubStyleList.Codes.F }, ImportMessageSubTypeList.Codes.InwardProcessingSimplifiedPrematureDeclaration)  }
		}.ToImmutableDictionary();
	}
}
