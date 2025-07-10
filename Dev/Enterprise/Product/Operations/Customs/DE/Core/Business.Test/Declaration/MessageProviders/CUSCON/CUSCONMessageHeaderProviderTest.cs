using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.DE.Business.CodeDescriptionPairLists;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.DE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Testing
{
	[TestedType(typeof(CUSCONMessageHeaderProvider))]
	class CUSCONMessageHeaderProviderTest : ImportMessageHeaderProviderTest<CUSCONMessageHeaderProvider>
	{
		public void TestConstructor_Null()
		{
			AssertExceptionThrown<ArgumentException>(() => new CUSCONMessageHeaderProvider(null));
		}

		public void TestMessageGroup()
		{
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;

			var messageSubTypeConditionsDictionary = new Dictionary<ZString, Tuple<ZString[], ZString>>()
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
			};

			CombineAssertions(() =>
			{
				foreach (var messageSubTypeCondition in messageSubTypeConditionsDictionary)
				{
					foreach (var subStyle in messageSubTypeCondition.Value.Item1)
					{
						var declarationSubStyleToSet = subStyle;
						var declarationTypeToSet = messageSubTypeCondition.Key;
						var expectedMessageSubType = messageSubTypeCondition.Value.Item2;
						entryInstruction.CEI_Style = declarationTypeToSet;
						entryInstruction.CEI_SubStyle = declarationSubStyleToSet;

						AssertEquals($"CEI_Style = {declarationTypeToSet}, CEI_SubStyle = {declarationSubStyleToSet}", expectedMessageSubType, Provider.MessageGroup);
					}
				}
			});
		}

		public void TestHeader()
		{
			AssertType<CUSCONHeaderProvider>(Provider.Header);
		}

		protected override CUSCONMessageHeaderProvider GetProvider() => new CUSCONMessageHeaderProvider(entryHeader);
	}
}
