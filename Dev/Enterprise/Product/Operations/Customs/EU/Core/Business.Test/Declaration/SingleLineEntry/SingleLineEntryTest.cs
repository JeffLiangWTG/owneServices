using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(SingleLineEntry))]
	public class SingleLineEntryTest : NonPersistentBusinessObjectTestCase
	{
		public void TestCreateLIC99()
		{
			var sle = GetSingleLineEntry();
			AssertEquals(expected: true, sle.CreateLIC99Info.ReadOnly);
			var declaration = Factory.New<JobDeclaration>();
			sle = new SingleLineEntry(declaration);
			AssertEquals(expected: false, sle.CreateLIC99Info.ReadOnly);
			declaration.JE_MessageType = "IMP";
			sle = new SingleLineEntry(declaration);
			AssertEquals(expected: true, sle.CreateLIC99Info.ReadOnly);
		}

		public void TestCPCCode_ResourceString()
		{
			CombineAssertions(() =>
			{
				var singleLineEntry = GetSingleLineEntry();
				AssertEquals("Caption", "CPC Code", DataBoundResourceStrings.GetDataForProperty(singleLineEntry.CPCCodeInfo).Caption);
				AssertEquals("FullDescription", "Customs Procedure Code.", DataBoundResourceStrings.GetDataForProperty(singleLineEntry.CPCCodeInfo).FullDescription);
			});
		}

		public void TestInvoiceNumber_ResourceString()
		{
			var singleLineEntry = GetSingleLineEntry();
			AssertEquals("Caption", "Invoice Number", DataBoundResourceStrings.GetDataForProperty(singleLineEntry.InvoiceNumberInfo).Caption);
		}

		public void TestPrice_ResourceString()
		{
			CombineAssertions(() =>
			{
				var singleLineEntry = GetSingleLineEntry();
				AssertEquals("Caption", "Price", DataBoundResourceStrings.GetDataForProperty(singleLineEntry.PriceInfo).Caption);
				AssertEquals("FullDescription", "Line Price.", DataBoundResourceStrings.GetDataForProperty(singleLineEntry.PriceInfo).FullDescription);
			});
		}

		public void TestNetWeight_ResourceString()
		{
			CombineAssertions(() =>
			{
				var singleLineEntry = GetSingleLineEntry();
				AssertEquals("Caption", "Net Weight", DataBoundResourceStrings.GetDataForProperty(singleLineEntry.NetWeightInfo).Caption);
				AssertEquals("FullDescription", "The net weight of the item.", DataBoundResourceStrings.GetDataForProperty(singleLineEntry.NetWeightInfo).FullDescription);
			});
		}

		public void TestCreateLIC99_ResourceString()
		{
			var singleLineEntry = GetSingleLineEntry();
			AssertEquals("Caption", "Add LIC99 statement?", DataBoundResourceStrings.GetDataForProperty(singleLineEntry.CreateLIC99Info).Caption);
		}

		public void TestTariffNumber_MaxLen()
		{
			AssertEquals("Tariff Number uses TariffFormatter which one returns up to 13 char", 13, GetSingleLineEntry().TariffNumberInfo.MaxLength);
		}

		public void TestNetWeight_DecimalPlaces()
		{
			var jiNetWeightDecimalPlaces = typeof(JobComInvoiceLine)
				.GetProperty(nameof(JobComInvoiceLine.JI_NetWeight))
				.GetCustomAttribute<DecimalPlacesAttribute>();
			var netWeightDecimalPlaces = typeof(SingleLineEntry)
				.GetProperty(nameof(SingleLineEntry.NetWeight))
				.GetCustomAttribute<DecimalPlacesAttribute>();

			AssertEquals("NetWeight should have the same amount of decimal places as specified in JI_NetWeight", jiNetWeightDecimalPlaces.DecimalPlaces, netWeightDecimalPlaces.DecimalPlaces);
		}

		public void TestTariffNumber_ListAttribute()
		{
			var tariffNumberPropInfo = typeof(SingleLineEntry).GetProperty(nameof(SingleLineEntry.TariffNumber));
			var listAttr = tariffNumberPropInfo.GetCustomAttribute<ListAttribute>();

			CombineAssertions("TariffNumber should have a list attribute", () =>
			{
				AssertNotNull(listAttr);
				AssertEquals("Lookups.Tariffs", listAttr.ListDataSourceMember);
			});
		}

		public void TestCPCCode_ListAttribute()
		{
			var cpcCodePropInfo = typeof(SingleLineEntry).GetProperty(nameof(SingleLineEntry.CPCCode));
			var listAttr = cpcCodePropInfo.GetCustomAttribute<ListAttribute>();

			CombineAssertions("CPCCode should have a list attribute", () =>
			{
				AssertNotNull(listAttr);
				AssertEquals("Lookups.CPCList", listAttr.ListDataSourceMember);
			});
		}

		public void TestCPCCode_MaxLen()
		{
			AssertEquals("CPC Code max length should be 7", 7, GetSingleLineEntry().CPCCodeInfo.MaxLength);
		}

		public void TestDataGroupingShouldReflectDeclarationsDefaultGrouping()
		{
			var declaration = Factory.New<JobDeclaration>();

			var singleLineEntry = new SingleLineEntry(declaration);
			AssertEquals(declaration.GetDefaultDataGroupingCode(), singleLineEntry.DataGrouping);
		}

		public void TestDataGroupingShouldBeEmptyWhenNoDeclarationExists()
		{
			var singleLineEntry = GetSingleLineEntry();
			AssertEquals(string.Empty, singleLineEntry.DataGrouping);
		}

		public void TestTariffTypeShouldReflectDeclarationsTariffType()
		{
			var declaration = Factory.New<JobDeclaration>();
			var singleLineEntry = new SingleLineEntry(declaration);

			declaration.JE_MessageType = Universal.Constants.TariffTypes.Import;
			AssertEquals(Universal.Constants.TariffTypes.Import, singleLineEntry.TariffType);

			declaration.JE_MessageType = Universal.Constants.TariffTypes.Export;
			AssertEquals(Universal.Constants.TariffTypes.Export, singleLineEntry.TariffType);
		}

		public void TestTariffTypeShouldBeImportWhenNoDeclarationExists()
		{
			var singleLineEntry = GetSingleLineEntry();
			AssertEquals(Universal.Constants.TariffTypes.Import, singleLineEntry.TariffType);
		}

		public void TestEffectiveDateShouldReflectDeclarationsDateOfValuation()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Universal.Constants.TariffTypes.Import;
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			var num = CusEntryNumber.New(entryHeader, Universal.Constants.TariffTypes.Import, declaration.CountryCode);
			num.CE_EntryNum = "ES0000001";
			num.CE_IssueDate = ZDateTime.BrettsBirthday;
			Factory.Save();

			var singleLineEntry = new SingleLineEntry(declaration);

			AssertEquals(ZDateTime.BrettsBirthday, singleLineEntry.EffectiveDate);
		}

		public void TestEffectiveDateShouldBeTodayWhenNoDeclarationExists()
		{
			var singleLineEntry = GetSingleLineEntry();
			AssertEquals(ZDateTime.Today, singleLineEntry.EffectiveDate);
		}

		SingleLineEntry GetSingleLineEntry() => new SingleLineEntry(Factory);
	}
}
