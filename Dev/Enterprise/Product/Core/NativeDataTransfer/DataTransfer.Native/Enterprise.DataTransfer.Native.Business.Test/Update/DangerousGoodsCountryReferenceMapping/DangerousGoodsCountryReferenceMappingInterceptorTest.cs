using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataTransfer.Native.Business.Update
{
	public class DangerousGoodsCountryReferenceMappingInterceptorTest : TestCaseWithFactory
	{
		public void TestThrowExceptionWhenInvalidStorageInstruction()
		{
			const string errorMessage =
				"StorageInstruction value XXX is invalid. Only one of the following are allowed: TBC - To be confirmed by Safety Data Sheet, ACD - Acid, GAS - Gas, BAS - Base, OXS - Oxidizing, MSC - Miscellaneous Dangerous, WAT - Water Reactive, FLL - Flammable Liquid, LIT - Lithium, EXP - Explosive, RAD - Radioactive, ORG - Organic peroxide, FLS - Flammable solid, TOX - Toxic";

			var undgCountryReferencePivot1 = GetCountryReferencePivotEntity(storageInstruction: "XXX");
			var dangerousGoodsReferenceMappingSet1 = new EntitySet("DangerousGoodsCountryReferenceMapping") { Root = undgCountryReferencePivot1 };
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), errorMessage, () => interceptor.Invoke(dangerousGoodsReferenceMappingSet1));

			var undgCountryReferencePivot2 = GetCountryReferencePivotEntity(storageInstruction: StorageInstructionList.Codes.ToBeConfirmedBySafetyDataSheet);
			var dangerousGoodsReferenceMappingSet2 = new EntitySet("DangerousGoodsCountryReferenceMapping") { Root = undgCountryReferencePivot2 };
			AssertNoExceptionThrown(() => interceptor.Invoke(dangerousGoodsReferenceMappingSet2));

			var undgCountryReferencePivot3 = GetCountryReferencePivotEntity();
			var dangerousGoodsReferenceMappingSet3 = new EntitySet("DangerousGoodsCountryReferenceMapping") { Root = undgCountryReferencePivot3 };
			AssertNoExceptionThrown(() => interceptor.Invoke(dangerousGoodsReferenceMappingSet3));

			var undgCountryReferencePivot4 = GetCountryReferencePivotEntity(storageInstruction: "");
			var dangerousGoodsReferenceMappingSet4 = new EntitySet("DangerousGoodsCountryReferenceMapping") { Root = undgCountryReferencePivot4 };
			AssertNoExceptionThrown(() => interceptor.Invoke(dangerousGoodsReferenceMappingSet4));
		}

		public void TestThrowExceptionWhenNoSecurityRight()
		{
			const string errorMessage = "You do not have security right to import UNDGCountryReference XMLs. Please enable security: Maintain -> Reference Files -> Dangerous Goods -> Edit -> Allow for Country/Region Regulations to be attached/detached";

			Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = false;
			var undgCountryReferencePivot = GetCountryReferencePivotEntity();
			var dangerousGoodsReferenceMappingSet = new EntitySet("DangerousGoodsCountryReferenceMapping") { Root = undgCountryReferencePivot };
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), errorMessage, () => interceptor.Invoke(dangerousGoodsReferenceMappingSet));

			Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = true;
			AssertNoExceptionThrown(() => interceptor.Invoke(dangerousGoodsReferenceMappingSet));
		}

		public void TestThrowExceptionWhenEntityIsNotFranceICPE()
		{
			const string errorMessage = "Importing UNDGCountryReference XMLs is only supported for FR ICPE";
			var undgCountryReferencePivot1 = GetCountryReferencePivotEntity(Core.Constants.CountryCodes.Singapore, Core.Constants.UNDGCountryReference.Type.PSA);
			var dangerousGoodsReferenceMappingSet1 = new EntitySet("DangerousGoodsCountryReferenceMapping") { Root = undgCountryReferencePivot1 };
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), errorMessage, () => interceptor.Invoke(dangerousGoodsReferenceMappingSet1));

			var undgCountryReferencePivot2 = GetCountryReferencePivotEntity(Core.Constants.CountryCodes.France, Core.Constants.UNDGCountryReference.Type.PSA);
			var dangerousGoodsReferenceMappingSet2 = new EntitySet("DangerousGoodsCountryReferenceMapping") { Root = undgCountryReferencePivot2 };
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), errorMessage, () => interceptor.Invoke(dangerousGoodsReferenceMappingSet2));

			var undgCountryReferencePivot3 = GetCountryReferencePivotEntity(Core.Constants.CountryCodes.Singapore, Core.Constants.UNDGCountryReference.Type.ICPE);
			var dangerousGoodsReferenceMappingSet3 = new EntitySet("DangerousGoodsCountryReferenceMapping") { Root = undgCountryReferencePivot3 };
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), errorMessage, () => interceptor.Invoke(dangerousGoodsReferenceMappingSet3));

			var undgCountryReferencePivot4 = GetCountryReferencePivotEntity(Core.Constants.CountryCodes.France, Core.Constants.UNDGCountryReference.Type.ICPE);
			var dangerousGoodsReferenceMappingSet4 = new EntitySet("DangerousGoodsCountryReferenceMapping") { Root = undgCountryReferencePivot4 };
			AssertNoExceptionThrown(() => interceptor.Invoke(dangerousGoodsReferenceMappingSet4));
		}

		Entity GetCountryReferencePivotEntity(string countryCode = Core.Constants.CountryCodes.France, string type = Core.Constants.UNDGCountryReference.Type.ICPE, string storageInstruction = null)
		{
			var undgCountryReferencePivot = new Entity(TestUtil.FindEntityDefinition("DangerousGoodsCountryReferenceMapping", "UNDGCountryReferencePivot"), sessionServices)
			{
				["UNNO"] = "1234",
				["Variant"] = "A",
				["Standard"] = "IAT",
			};

			var country = new Entity(TestUtil.FindEntityDefinition("DangerousGoodsCountryReferenceMapping", "UNDGCountryReferencePivot.UNDGCountryReference.Country"), sessionServices);
			country["Code"] = countryCode;

			var undgCountryReference = new Entity(TestUtil.FindEntityDefinition("DangerousGoodsCountryReferenceMapping", "UNDGCountryReferencePivot.UNDGCountryReference"), sessionServices)
			{
				["Type"] = type
			};
			if (storageInstruction != null)
			{
				undgCountryReferencePivot["StorageInstruction"] = storageInstruction;
			}
			undgCountryReference.ParentCollection.Add(country);
			undgCountryReferencePivot.ParentCollection.Add(undgCountryReference);

			return undgCountryReferencePivot;
		}

		protected override void SetUp()
		{
			base.SetUp();
			sessionServices = new AncillaryImportServices();
			var context = new UpdateContext(sessionServices, new FactoryProvider());
			var setting = new DangerousGoodsCountryReferenceMappingSetting { Enable = true, Context = context };

			interceptor = new DangerousGoodsCountryReferenceMappingInterceptor(setting, sessionServices)
			{
				Function = x => { }
			};
			previousSecurityRight = Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed;
			Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = true;
		}

		protected override void TearDown()
		{
			base.TearDown();
			Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = previousSecurityRight;
		}

		DangerousGoodsCountryReferenceMappingInterceptor interceptor;
		AncillaryImportServices sessionServices;
		bool previousSecurityRight;
	}
}
