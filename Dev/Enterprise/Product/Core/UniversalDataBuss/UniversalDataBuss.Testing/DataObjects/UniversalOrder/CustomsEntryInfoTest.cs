using System.Collections.Generic;
using Enterprise.UniversalDataBuss.DataObjects.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalDataBuss.DataObjects.Universal.Testing
{
	[TestedType(typeof(CustomsEntryInfo))]
	class CustomsEntryInfoTest : DataObjectTestCase<CustomsEntryInfo>
	{
		protected override Dictionary<string, int> ExpectedMaxLengthValues() => new Dictionary<string, int>
		{
			{ nameof(CustomsEntryInfo.EntryKey), WhsBondedWarehouseAttributeSchema.WB_EntryKey.MaxLength },
			{ nameof(CustomsEntryInfo.InwardsEntryKey), WhsBondedWarehouseAttributeSchema.WB_EntryKey.MaxLength },
			{ nameof(CustomsEntryInfo.DeclarationReference), WhsBondedWarehouseAttributeSchema.WB_DeclarationReference.MaxLength },
			{ nameof(CustomsEntryInfo.AdditionalInformation), WhsBondedWarehouseAttributeSchema.WB_AddInfo.MaxLength },
			{ nameof(CustomsEntryInfo.Tariff), WhsBondedWarehouseAttributeSchema.WB_Tariff.MaxLength },
			{ nameof(CustomsEntryInfo.PrimaryPreference), WhsBondedWarehouseAttributeSchema.WB_PrimaryPreference.MaxLength },
			{ nameof(CustomsEntryInfo.InwardStyle), WhsBondedWarehouseAttributeSchema.WB_InwardStyle.MaxLength },
			{ nameof(CustomsEntryInfo.InwardProcedure), WhsBondedWarehouseAttributeSchema.WB_InwardProcedure.MaxLength },
			{ nameof(CustomsEntryInfo.DataImportMatchingKey), WhsBondedWarehouseAttributeSchema.WB_MatchingKey.MaxLength }
		};
	}
}
