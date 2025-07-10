using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.DocumentWrappers.Customs.EU.EMCS.Testing;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.EMCS.DocumentWrappers.Testing
{
	[TestedType(typeof(IEEMCSDeclarationWrapper))]
	public class IEEMCSDeclarationWrapperTest : TestCaseWithFactory
	{
		public virtual void TestWrapperProperties()
		{
			EMCSDeclarationWrapperTestHelper.SetupReferenceTestData(Factory);

			declaration = Factory.New<EMCSJobDeclaration>();
			declaration.JE_DeclarationReference = "Foo";
			declaration.ZG_GuarantorType = EMCSGuarantorTypeList.Codes.Consignor;
			declaration.JE_DateAtOrigin = ZDateTime.BrettsBirthday;
			declaration.InvoiceNumber = "INV0001";
			declaration.InvoiceDate = ZDateTime.BrettsBirthday;
			declaration.JourneyTimeNumericPart = 11;
			declaration.JourneyTimeFormatPart = JourneyTimeUnitList.Codes.Days;
			declaration.EADNumber = "EADNUM1234";
			declaration.JE_GoodsOrigin = Core.Constants.CountryCodes.Portugal;
			declaration.JE_GoodsDestination = Core.Constants.CountryCodes.UnitedKingdom;
			wrapper = IEEMCSDeclarationWrapper.New(declaration, Factory);

			CombineAssertions(() =>
			{
				declaration.InvoiceLines.DeleteAll();
				AssertItem(1);
				AssertItem(2);
				AssertItem(3);
			});
		}

		void AssertItem(int lineNumber)
		{
			var box = string.Empty;
			switch (lineNumber)
			{
				case 1:
					box = "A";
					break;
				case 2:
					box = "B";
					break;
				case 3:
					box = "C";
					break;
			}

			var box24Label = string.Format("Box24{0}SoldInWarehouseLabel", box);
			var box24 = string.Format("Box24{0}SoldInWarehouse", box);
			var box241Label = string.Format("Box24{0}ProducedInUKLabel", box);
			var box241 = string.Format("Box24{0}ProducedInUK", box);

			var invoiceLine = EMCSDeclarationWrapperTestHelper.AddInvoiceLine(declaration, isMainPack: true, $"Description {lineNumber}", 15.5m, 10.4m, $"{lineNumber}23456789", Core.Constants.CountryCodes.Portugal);

			AssertEquals(box24Label, ZString.Empty, wrapper.GetPropertyValue(box24Label));
			AssertEquals(box24, ZString.Empty, wrapper.GetPropertyValue(box24));
			AssertEquals(box241Label, ZString.Empty, wrapper.GetPropertyValue(box241Label));
			AssertEquals(box241, ZString.Empty, wrapper.GetPropertyValue(box241));
		}

		public void TestBoxZAdministrativeReferenceCode()
		{
			declaration = Factory.New<EMCSJobDeclaration>();
			wrapper = IEEMCSDeclarationWrapper.New(declaration, Factory);
			declaration.EADNumber = "EADNUM1234";
			AssertEquals("When EADNumber has value", "EADNUM1234", wrapper.BoxZAdministrativeReferenceCode);

			declaration.EADNumber = ZString.Empty;
			AssertEquals("When EADNumber is empty", "FALLBACK", wrapper.BoxZAdministrativeReferenceCode);
		}

		EMCSJobDeclaration declaration;
		IEEMCSDeclarationWrapper wrapper;
	}
}
