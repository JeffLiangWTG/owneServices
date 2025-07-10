using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	public class JobComInvoiceLineTestGeneral_AHECC : JobComInvoiceLineTest
	{
		public void TestDescriptionDefaultedOnTariff()
		{
			var aHECC = Factory.LoadFromNaturalKey<AUCAHECC>(AUCAHECCSchema.UA_AHECC, "9999.99.99")
				?? Factory.New<AUCAHECC>();
			aHECC.UA_AHECC = "9999.99.99";
			aHECC.UA_LongDescription = "MIXED GOODS (INCL. SHIPS' & AIRCRAFT STORES NOT SUBJECT TO EXCISE/CUSTOMS DUTY) COMPRISING FOUR OR MORE COMMODITIES IN A SINGLE CONSIGNMENT WHERE VALUES OF COMMODITIES IS LESS THAN $5000. COMMODITIES VALUED AT $5000 OR MORE CLASSIFY TO KIND.";

			var aHECC2 = Factory.LoadFromNaturalKey<AUCAHECC>(AUCAHECCSchema.UA_AHECC, "9999.99.99");
			AssertEquals("AHECC", aHECC, aHECC2);

			//AHECC.AUCAHECCs.Load(
			Line.JI_Tariff = "9999.99.99";
			AssertEquals("Description", "MIXED GOODS (INCL. SHIPS' & AIRCRAFT STORES NOT SUBJECT TO EXCISE/CUSTOMS DUTY) COMPRISING FOUR OR MORE COMMODITIES IN A SINGLE CONSIGNMENT WHERE VALUES OF COMMODITIES IS LESS THAN $5000. COMMODITIES VALUED AT $5000 OR MORE CLASSIFY TO KIND.", Line.JI_Description);
		}

		public void TestDescriptionStaysAsPartDescriptionEvenWhenTariffOverridden()
		{
			Part.OP_Desc = "Part Description";
			Factory.Save();
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Description", Part.OP_Desc.ToUpper(), Line.JI_Description);

			AUCAHECC newAHECC = Factory.New<AUCAHECC>();
			newAHECC.UA_AHECC = "0000.00.00";
			newAHECC.UA_LongDescription = "AHECC DESCRIPTION";
			Line.JI_Tariff = newAHECC.UA_AHECC;
			AssertEquals("PreCondition:Part should stay", Part, Line.Part);
			AssertEquals("Description Should Stay at Part Description", Part.OP_Desc.ToUpper(), Line.JI_Description);
		}

		public void TestUpdateDescriptionFromShortDescriptionIfThereIsNoLong()
		{
			AUCAHECC newAHECC = Factory.New<AUCAHECC>();
			newAHECC.UA_AHECC = "0000.00.00";
			newAHECC.UA_ShortDescription = "SHORT DESCRIPTION";
			newAHECC.UA_LongDescription = ZString.Empty;
			Line.JI_Tariff = newAHECC.UA_AHECC;
			AssertEquals("Description updated", newAHECC.UA_ShortDescription, Line.JI_Description);
		}

		#region CustomsQuantity

		public void TestCutomsQtyZeroWhenNotRequired()
		{
			AUCAHECC nRAhecc = Factory.New<AUCAHECC>();
			nRAhecc.UA_AHECC = "0000.00.00";
			nRAhecc.UA_UQ = "NR";

			Line.JI_Tariff = nRAhecc.UA_AHECC;
			AssertEquals("Customs Qty zero", 0m, Line.JI_CustomsQuantity);
		}

		public void TestCustomsQtyReadOnlyWhenNotRequired()
		{
			AUCAHECC nRAhecc = Factory.New<AUCAHECC>();
			nRAhecc.UA_AHECC = "0000.00.00";
			nRAhecc.UA_UQ = "NR";

			Line.JI_Tariff = nRAhecc.UA_AHECC;
			AssertEquals("Customs Qty read only", true, Line.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestMappingFromM3ToCU()
		{
			var refPacks = Factory.New<CusRefPacks>();
			refPacks.RP_CommercialPack = "M3";
			refPacks.RP_CustomsPack = "CU";
			refPacks.RP_ConversionFactor = 1;

			AUCAHECC cUAhecc = Factory.New<AUCAHECC>();
			cUAhecc.UA_AHECC = "0000.00.00";
			cUAhecc.UA_UQ = "CU";

			Line.JI_Tariff = cUAhecc.UA_AHECC;
			Line.JI_InvoiceUQ = "M3";
			Line.JI_InvoiceQuantity = 2m;
			AssertEquals("Customs Qty", 2m, Line.JI_CustomsQuantity);
		}

		public void TestMappingFromM2ToSM()
		{
			var refPacks = Factory.New<CusRefPacks>();
			refPacks.RP_CommercialPack = "M2";
			refPacks.RP_CustomsPack = "SM";
			refPacks.RP_ConversionFactor = 1;

			AUCAHECC sMAhecc = Factory.New<AUCAHECC>();
			sMAhecc.UA_AHECC = "0000.00.00";
			sMAhecc.UA_UQ = "SM";

			Line.JI_Tariff = sMAhecc.UA_AHECC;
			Line.JI_InvoiceUQ = "M2";
			Line.JI_InvoiceQuantity = 2m;
			AssertEquals("Customs Qty", 2m, Line.JI_CustomsQuantity);
		}

		public void TestAddMessageErrorToCustomsQtyWhenNotConvertible()
		{
			AssertNotNull(AHECC);
			Line.JI_Tariff = ExportClass.CC_TariffNum;
			Line.JI_InvoiceQuantity = 2;
			Line.JI_InvoiceUQ = "CS";//inconvertible unit
			Line.OldValidation.ValidateJI_CustomsQuantity();
			AssertEquals("Error Expected", true, Line.JI_CustomsQuantityInfo.HasNotifications());
		}

		public void TestAddMessageErrorToCustomsQtyWhenNotConvertibleWithPartKnown()
		{
			AssertNotNull(AHECC);
			Line.JI_PartNo = Part.OP_PartNum;
			Line.JI_InvoiceQuantity = 2;
			Line.JI_InvoiceUQ = "CS";//inconvertible unit
			Line.JI_CustomsQuantity = 0;
			AssertEquals("Error Expected", true, Line.JI_CustomsQuantityInfo.HasNotifications());
		}

		public void TestAddMessageErrorToCustomsQtyWhenZero()
		{
			AssertNotNull(AHECC);
			Line.JI_Tariff = ExportClass.CC_TariffNum;
			Line.OldValidation.ValidateJI_CustomsQuantity();
			AssertEquals("Error Expected", true, Line.JI_CustomsQuantityInfo.HasNotifications());
		}

		public void TestCustomsQuantityAndUQ()
		{
			AssertNotNull(AHECC);
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Customs Unit of Quantity", exportCustomsUQ, Line.JI_CustomsUnitQty);

			Line.JI_InvoiceQuantity = 2m;
			Line.JI_InvoiceUQ = "KG";
			Assert(Line.JI_CustomsQuantity == 2 * 1000m);
		}

		public void TestCustomsQuantityAndUQWithUQEnteredFirst()
		{
			AssertNotNull(AHECC);
			Line.JI_PartNo = Part.OP_PartNum;
			Assert(Line.JI_CustomsUnitQty == exportCustomsUQ);

			Line.JI_InvoiceUQ = "KG";
			Line.JI_InvoiceQuantity = 2m;
			Assert(Line.JI_CustomsQuantity == 2 * 1000m);
		}

		public void TestCustomsQuantityWithSameTypeConversion()
		{
			AssertNotNull(AHECC);
			Line.JI_PartNo = Part.OP_PartNum;
			Line.JI_InvoiceQuantity = 2m;
			Line.JI_InvoiceUQ = Enterprise.Core.Constants.Weight.Ounces;

			//Convert from OZ -> KG -> BO
			ZDecimal expected = Enterprise.Core.Constants.Weight.Convert(2m, "OZ", "KG") * 1000m;
			AssertEquals(decimal.Round(expected, 2), decimal.Round(Line.JI_CustomsQuantity, 2));
		}

		public void TestCalculateCustomsQtyWithoutPartInvoiceUQEnteredFirst()
		{
			Line.JI_Tariff = Class_KG.UJ_Code;
			Line.JI_InvoiceUQ = "KG";
			Line.JI_InvoiceQuantity = 2m;

			AssertEquals("Customs Qty calculated", 2m, Line.JI_CustomsQuantity);
		}

		public void TestCustomsUQ()
		{
			AssertNotNull(AHECC);
			AssertNotNull(ImportAHECC);
			Line.JI_PartNo = Part.OP_PartNum;
			AssertEquals("Customs UQ", exportCustomsUQ, Line.JI_CustomsUnitQty);

			Line.JI_Tariff = ImportTariffNum;
			AssertEquals("Customs UQ", "NR", Line.JI_CustomsUnitQty);
		}

		public void TestCalculateCustomsQtyWithoutPartSameTypeConversion()
		{
			ZString newTariff = "3333.33.33";
			AUCAHECC newAHECC = Factory.New<AUCAHECC>();
			newAHECC.UA_UQ = "KG";
			newAHECC.UA_AHECC = newTariff;
			Factory.Save();

			Line.JI_Tariff = newTariff;
			Line.JI_InvoiceQuantity = 2m;
			Line.JI_InvoiceUQ = "LB";

			ZDecimal expected = decimal.Round(Enterprise.Core.Constants.Weight.Convert(Line.JI_InvoiceQuantity, Line.JI_InvoiceUQ, "KG"), 5);
			AssertEquals("Customs Qty calculated", expected, Line.JI_CustomsQuantity);
		}

		public void TestCalculateCustomsQtyWithoutPart()
		{
			Line.JI_Tariff = Class_KG.UJ_Code;
			Line.JI_InvoiceQuantity = 2m;
			Line.JI_InvoiceUQ = "KG";

			AssertEquals("Customs Qty calculated", 2m, Line.JI_CustomsQuantity);
		}

		public void TestCustomsQtyReadOnlyWhenCalculableFromWeight()
		{
			var ahecc_kg = Factory.LoadFromNaturalKey<AUCAHECC>(AUCAHECCSchema.UA_AHECC, "3406.00.00");

			Line.JI_Tariff = ahecc_kg.UA_AHECC;
			Line.JI_Weight = 2000m;
			Line.JI_WeightUQ = "G";
			AssertEquals("Customs Qty Calculated", false, Line.JI_CustomsQuantityInfo.ReadOnly);
		}

		public void TestRefPacksConversion()
		{
			var refPack = Factory.New<CusRefPacks>();
			refPack.RP_CommercialPack = "M3";
			refPack.RP_CustomsPack = "SM";
			refPack.RP_ConversionFactor = 2m;
			Factory.Save();

			AUCAHECC aHECC = Factory.New<AUCAHECC>();
			aHECC.UA_UQ = "SM";
			aHECC.UA_AHECC = "0000.00.00";

			Line.JI_Tariff = aHECC.UA_AHECC;
			Line.JI_InvoiceUQ = "M3";
			Line.JI_InvoiceQuantity = 20m;

			AssertEquals("Customs Quantity", 40m, Line.JI_CustomsQuantity);
		}

		public void TestRefPacksConversion2()
		{
			AUCAHECC aHECC = Factory.New<AUCAHECC>();
			aHECC.UA_AHECC = "1234.45.65";
			aHECC.UA_UQ = "BC";

			ExportClass.CC_TariffNum = aHECC.UA_AHECC;

			ZGuid partUnitPK = Part.PK; // To flush Part to disk
			OrgPartUnit partUnit = Factory.New<OrgPartUnit>();
			partUnit.OF_OP = partUnitPK;
			partUnit.OF_PackType = "BOX";
			partUnit.OF_ParentPackType = "CTN";
			partUnit.OF_QuantityInParent = 24m;

			Line.JI_PartNo = Part.OP_PartNum;
			Line.JI_InvoiceUQ = "BOX";
			Line.JI_InvoiceQuantity = 48m;
			AssertEquals("Customs Qty", 2m, Line.JI_CustomsQuantity);
		}

		public void TestRefPacksConversion3WithSupplier()
		{
			var refPack = Factory.New<CusRefPacks>();
			refPack.RP_CommercialPack = "BOX";
			refPack.RP_CustomsPack = "BC";
			refPack.RP_OH_Supplier = Header.JZ_OH_Supplier;
			refPack.RP_ConversionFactor = 2m;

			var refPack2 = Factory.New<CusRefPacks>();
			refPack2.RP_CommercialPack = "BOX";
			refPack2.RP_CustomsPack = "BC";
			refPack2.RP_ConversionFactor = 1m;
			Factory.Save();

			AUCAHECC aHECC = Factory.New<AUCAHECC>();
			aHECC.UA_UQ = "BC";
			aHECC.UA_AHECC = "0000.00.00";

			Line.JI_Tariff = aHECC.UA_AHECC;
			Line.JI_InvoiceUQ = "BOX";
			Line.JI_InvoiceQuantity = 20m; //this line has a supplier set in the parent header
			AssertEquals("Customs Qty for the supplier", 40m, Line.JI_CustomsQuantity);
		}

		public void TestRefPacksConversion4WithEmptySupplier()
		{
			var refPack = Factory.New<CusRefPacks>();
			refPack.RP_CommercialPack = "BOX";
			refPack.RP_CustomsPack = "BC";
			refPack.RP_OH_Supplier = Header.JZ_OH_Supplier;
			refPack.RP_ConversionFactor = 2m;

			var refPack2 = Factory.New<CusRefPacks>();
			refPack2.RP_CommercialPack = "BOX";
			refPack2.RP_CustomsPack = "BC";
			refPack2.RP_ConversionFactor = 1m;

			Header.JZ_OH_Supplier = ZGuid.Empty;

			AUCAHECC aHECC = Factory.New<AUCAHECC>();
			aHECC.UA_UQ = "BC";
			aHECC.UA_AHECC = "0000.00.00";
			Line.JI_Tariff = aHECC.UA_AHECC;

			Line.JI_InvoiceUQ = "BOX";
			Line.JI_InvoiceQuantity = 20m;
			AssertEquals("Customs Qty for the supplier", 20m, Line.JI_CustomsQuantity);
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			useCustomsReferenceDataRegItem = AUCustomsDataRegistry.Instance.EnableCWRefForAHECC.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			useCustomsReferenceDataRegItem?.Dispose();
		}

		IDisposable useCustomsReferenceDataRegItem;
	}
}
