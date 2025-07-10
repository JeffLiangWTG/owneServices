using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing;

[TestedType(typeof(TemporaryStorageDutyAndTax))]
sealed class TemporaryStorageDutyAndTaxTest : EnterpriseBusinessObjectTestCase
{
	public void TestCaptions() => CombineAssertions(() =>
	{
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_ChargeType), false, x => x.Caption == "Type");
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_ChargeType), false, x => x.FullDescription == "Type of duty or tax.");
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_MethodOfCalculation), false, x => x.Caption == "Method");
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_MethodOfCalculation), false, x => x.MediumCaption == "Calculation method");
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_MethodOfCalculation), false, x => x.FullDescription == "Method of calculation.");
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_BaseValue), false, x => x.Caption == "Base");
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_BaseValue), false, x => x.MediumCaption == "Base value");
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_BaseValue), false, x => x.FullDescription == "Base value for calculation.");
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_Rate), false, x => x.Caption == "Rate");
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_Rate), false, x => x.FullDescription == "Rate for calculation.");
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_ChargeAmount), false, x => x.Caption == "Amount");
		AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_ChargeAmount), false, x => x.FullDescription == "Calculated amount.");
	});

	public void TestAllReadOnly() => CombineAssertions(() =>
	{
		AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_ChargeType), false, x => x.IsReadOnly);
		AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_MethodOfCalculation), false, x => x.IsReadOnly);
		AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_BaseValue), false, x => x.IsReadOnly);
		AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(TemporaryStorageDutyAndTax), nameof(TemporaryStorageDutyAndTax.AET_ChargeAmount), false, x => x.IsReadOnly);
	});

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject(factory);

	protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObject(Factory);

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject(Factory);

	static TemporaryStorageDutyAndTax GetNewBusinessObject(BusinessObjectFactory factory)
	{
		var temporaryStorageDutyAndTax =  factory.New<TemporaryStorageHeader>().Bills.AddNew().PackedItems.AddNew().DutiesAndTaxes.AddNew();
		temporaryStorageDutyAndTax.AET_MethodOfCalculation = "X";
		return temporaryStorageDutyAndTax;
	}

	public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues() => new List<ZString>() { TemporaryStorageDutyAndTax.Schema.AET_API_AsycudaPackedItem };
}
