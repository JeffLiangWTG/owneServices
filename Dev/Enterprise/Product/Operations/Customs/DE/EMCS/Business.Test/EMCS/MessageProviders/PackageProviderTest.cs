using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CargoWise.Customs.DE.MessageContracts.EMCS;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.DE.EMCS.Business.Testing
{
	sealed class PackageProviderTest : Customs.Business.Testing.DataProviderTestCase<PackageProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>(() => new PackageProvider(emcsInvoiceLine, null));
		}

		public void TestConstructor_InvoiceLineIsNull()
		{
			(EMCSPackage emcsPackage, IEMCSPackage packageProvider) = CreatePackageAndProvider();
			AssertExceptionThrown<ArgumentException>(() => new PackageProvider(null, emcsPackage));
		}

		public void TestKindOfPackages()
		{
			(EMCSPackage emcsPackage, IEMCSPackage packageProvider) = CreatePackageAndProvider();
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, packageProvider.KindOfPackages);
				emcsPackage.B5_UnitType = Core.Constants.PkgUnit.Bag;
				AssertEquals(Core.Constants.PkgUnit.Bag, packageProvider.KindOfPackages);
			});
		}

		public void TestNumberOfPackages()
		{
			emcsInvoiceLine.ZG_IsMainPack = ZBool.True;
			(EMCSPackage emcsPackage, IEMCSPackage packageProvider) = CreatePackageAndProvider();
			emcsPackage.B5_UnitCount = 12;

			CombineAssertions(() =>
			{
				emcsPackage.B5_UnitType = "VQ";
				AssertEquals("Null, as not countable", null, packageProvider.NumberOfPackages);

				emcsPackage.B5_UnitType = "BO";
				AssertEquals("Value, as countable", 12L, packageProvider.NumberOfPackages);
			});
		}

		public void TestNumberOfPackages_InnerPack()
		{
			emcsInvoiceLine.ZG_IsMainPack = ZBool.False;
			(EMCSPackage emcsPackage, IEMCSPackage packageProvider) = CreatePackageAndProvider();
			emcsPackage.B5_UnitCount = 12;

			emcsPackage.B5_UnitType = "BO";
			AssertEquals(0L, packageProvider.NumberOfPackages);
		}

		public void TestSealNumber()
		{
			(EMCSPackage emcsPackage, IEMCSPackage packageProvider) = CreatePackageAndProvider();
			CombineAssertions(() =>
			{
				AssertEquals(ZString.Empty, packageProvider.SealNumber);
				emcsPackage.B5_SealNumber = "11DE11111111111115";
				AssertEquals("11DE11111111111115", packageProvider.SealNumber);
			});
		}

		public void TestSealInformation()
		{
			(EMCSPackage emcsPackage, IEMCSPackage packageProvider) = CreatePackageAndProvider();
			CombineAssertions(() =>
			{
				emcsPackage.B5_SealComment = "PACKAGE DESCRIPTION";
				packageProvider = new PackageProvider(emcsInvoiceLine, emcsPackage);
				var description = packageProvider.SealInformation;
				AssertEquals("PACKAGE DESCRIPTION", description.Text);
			});
		}

		public void TestShippingMarks()
		{
			(EMCSPackage package, IEMCSPackage provider) = CreatePackageAndProvider();
			package.B5_MarksAndNumbers = "MARKS AND NUMBERS";
			package.B5_UnitType = "BO";

			AssertEquals("Countable, no parent package", "MARKS AND NUMBERS", provider.ShippingMarks);
		}

		public void TestShippingMarks_NotCountable()
		{
			(EMCSPackage package, IEMCSPackage provider) = CreatePackageAndProvider();
			package.B5_MarksAndNumbers = "MARKS AND NUMBERS";
			package.B5_UnitType = "VQ";

			AssertEquals("Not countable", ZString.Empty, provider.ShippingMarks);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "EMCS Pack Types");
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName(RefCusCodeListAttributeTypes.Codes.Countable, "Desc.", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			var boCode = helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "BO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			boCode.Attributes.AddNew(RefCusCodeListAttributeTypes.Codes.Countable, RefCusCodeListAttributeTypes.Codes.Countable);
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.EMCSPackTypes, "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			declaration = Factory.New<EMCSJobDeclaration>();
			emcsInvoiceLine = declaration.InvoiceHeader.InvoiceLines.AddNew();
		}
		EMCSJobDeclaration declaration;
		EMCSJobComInvoiceLine emcsInvoiceLine;

		protected override PackageProvider GetProvider()
		{
			(EMCSPackage package, IEMCSPackage dataProvider) = CreatePackageAndProvider();
			package.B5_SealComment = "PACKAGE DESCRIPTION";
			return (PackageProvider)dataProvider;
		}

		protected override IEnumerable<Expression<Func<PackageProvider, object>>> GetPropertiesNeedToBeCached()
		{
			yield return x => x.SealInformation;
		}

		(EMCSPackage EmcsPackage, IEMCSPackage PackageProvider) CreatePackageAndProvider()
		{
			var emcsPackage = (EMCSPackage)declaration.EMCSPackages.AddNew();
			var packageProvider = new PackageProvider(emcsInvoiceLine, emcsPackage);
			return (emcsPackage, packageProvider);
		}
	}
}
