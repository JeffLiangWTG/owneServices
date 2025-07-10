using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.FR.Registry;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class IncoTermChargesConfigurationKeyTest : TestCaseWithFactory
	{
		public void TestEffectiveAgreedPlaceCode_UCC5()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();

			invoice.ZG_AgreedPlaceCode = "1";
			AssertEquals("In Delta G, EffectiveAgreedPlaceCode should be exactly ZG_AgreedPlaceCode.", "1", new IncoTermChargesConfigurationKey(invoice).EffectiveAgreedPlaceCode);

			invoice.ZG_AgreedPlaceCode = "2";
			AssertEquals("In Delta G, EffectiveAgreedPlaceCode should be exactly ZG_AgreedPlaceCode.", "2", new IncoTermChargesConfigurationKey(invoice).EffectiveAgreedPlaceCode);
		}

		public void TestEffectiveAgreedPlaceCode_UCC6()
		{
			using (FRCustomsDataRegistry.Instance.EnableDeltaIEForImports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
				declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
				var invoice = declaration.Invoices.AddNew();

				invoice.ZG_AgreedPlaceCode = "FRLEH";
				AssertEquals("In Delta IE, we should convert ZG_AgreedPlaceCode from UNLOCO to 1 character when looking for applicable charges.", "1", new IncoTermChargesConfigurationKey(invoice).EffectiveAgreedPlaceCode);

				invoice.ZG_AgreedPlaceCode = "DEHAM";
				AssertEquals("In Delta IE, we should convert ZG_AgreedPlaceCode from UNLOCO to 1 character when looking for applicable charges.", "2", new IncoTermChargesConfigurationKey(invoice).EffectiveAgreedPlaceCode);

				invoice.ZG_AgreedPlaceCode = "CHAAB";
				AssertEquals("CH is not member of EU.", "3", new IncoTermChargesConfigurationKey(invoice).EffectiveAgreedPlaceCode);

				invoice.ZG_AgreedPlaceCode = "USNYC";
				AssertEquals("In Delta IE, we should convert ZG_AgreedPlaceCode from UNLOCO to 1 character when looking for applicable charges.", "3", new IncoTermChargesConfigurationKey(invoice).EffectiveAgreedPlaceCode);

				invoice.ZG_AgreedPlaceCode = "GYAHL";
				AssertEquals("DROM should be considered as OutsideUnion.", "3", new IncoTermChargesConfigurationKey(invoice).EffectiveAgreedPlaceCode);
			}
		}
	}
}
