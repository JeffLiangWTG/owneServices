using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(ChargeCodeWithTypeCollection))]
	public class ChargeCodeWithTypeCollectionTest : RegistryBusinessObjectCollectionTemplateTestCase<ChargeCodeWithTypeCollection>
	{
		public void TestEnsureDefaultPartyTypesExist()
		{
			Assert("Precondition: tested collection should be empty", this.Collection.Count == 0);
			var partyTypes = new OrgProfitSharePartyLookups().PartyTypes;

			this.Collection.EnsureDefaultPartyTypesExist();
			Assert(
				"All default party types (from lookup) should be present in the collection",
				partyTypes
					.OfType<CodeDescriptionPair>()
					.All(p => this.Collection[p.MultilingualDescription] != null));
		}

		public void TestGetDefault()
		{
			CodeDescriptionPairList cachedPartyTypes;
			RegistryFactory.Instance.TryGetValueFromCacheOnly("ChargeCodeWithTypeCollection.GetPartyTypes", out cachedPartyTypes);
			AssertNull("Precondition", cachedPartyTypes);

			var defaultValue = ChargeCodeWithTypeCollection.GetDefaultCollection();
			AssertEquals("This registry item depends on OrgProfitSharePartyLookups.PartyTypes, please take a quick look here", 8, defaultValue.Count);

			RegistryFactory.Instance.TryGetValueFromCacheOnly("ChargeCodeWithTypeCollection.GetPartyTypes", out cachedPartyTypes);
			AssertNotNull("Cached OrgProfitSharePartyTypes", cachedPartyTypes);

			foreach (CodeDescriptionPair pair in cachedPartyTypes)
			{
				string partyType = pair.Description;
				AssertNotNull("Must contain all party types", defaultValue[partyType]);
			}
		}

		public void TestGetCode()
		{
			var sendingAgentChargeCode = ZGuid.NewZGuid();
			var receivingAgentChargeCode = ZGuid.NewZGuid();
			var controllingAgentChargeCode = ZGuid.NewZGuid();
			var headOfficeChargeCode = ZGuid.NewZGuid();
			var gatewayAgentChargeCode = ZGuid.NewZGuid();
			var leadGatewayAgentChargeCode = ZGuid.NewZGuid();
			var defaultChargeCode = AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value;

			var collection = ChargeCodeWithTypeCollection.GetDefaultCollection();
			AssertEquals("ChargeCode is default", defaultChargeCode, collection.GetCode("SEN"));
			AssertEquals("ChargeCode is default", defaultChargeCode, collection.GetCode("RCV"));
			AssertEquals("ChargeCode is default", defaultChargeCode, collection.GetCode("CON"));
			AssertEquals("ChargeCode is default", defaultChargeCode, collection.GetCode("HDF"));
			AssertEquals("ChargeCode is default", defaultChargeCode, collection.GetCode("GWA"));
			AssertEquals("ChargeCode is default", defaultChargeCode, collection.GetCode("LGA"));
			AssertEquals("ChargeCode is empty", ZGuid.Empty, collection.GetCode("CRAP"));

			RegistryFactory.Instance.TryGetValueFromCacheOnly("ChargeCodeWithTypeCollection.GetPartyTypes", out CodeDescriptionPairList partyTypes);
			var codeWithType = collection[partyTypes.GetDescriptionFromCode("SEN")];
			codeWithType.UseDefaultProfitShareChargeCode = false;
			codeWithType.ChargeCode = sendingAgentChargeCode;

			codeWithType = collection[partyTypes.GetDescriptionFromCode("RCV")];
			codeWithType.UseDefaultProfitShareChargeCode = false;
			codeWithType.ChargeCode = receivingAgentChargeCode;

			codeWithType = collection[partyTypes.GetDescriptionFromCode("CON")];
			codeWithType.UseDefaultProfitShareChargeCode = false;
			codeWithType.ChargeCode = controllingAgentChargeCode;

			codeWithType = collection[partyTypes.GetDescriptionFromCode("HDF")];
			codeWithType.UseDefaultProfitShareChargeCode = false;
			codeWithType.ChargeCode = headOfficeChargeCode;

			codeWithType = collection[partyTypes.GetDescriptionFromCode("GWA")];
			codeWithType.UseDefaultProfitShareChargeCode = false;
			codeWithType.ChargeCode = gatewayAgentChargeCode;

			codeWithType = collection[partyTypes.GetDescriptionFromCode("LGA")];
			codeWithType.UseDefaultProfitShareChargeCode = false;
			codeWithType.ChargeCode = leadGatewayAgentChargeCode;

			AssertEquals("ChargeCode", sendingAgentChargeCode, collection.GetCode("SEN"));
			AssertEquals("ChargeCode", receivingAgentChargeCode, collection.GetCode("RCV"));
			AssertEquals("ChargeCode", controllingAgentChargeCode, collection.GetCode("CON"));
			AssertEquals("ChargeCode", headOfficeChargeCode, collection.GetCode("HDF"));
			AssertEquals("ChargeCode", gatewayAgentChargeCode, collection.GetCode("GWA"));
			AssertEquals("ChargeCode", leadGatewayAgentChargeCode, collection.GetCode("LGA"));
			AssertEquals("ChargeCode is empty", ZGuid.Empty, collection.GetCode("CRAP"));
		}

		public void TestAllowNew()
		{
			AssertEquals("Must not allow new rows", false, this.Collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals("Must not allow removal of the rows", false, this.Collection.AllowRemove);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ChargeCodeWithTypeCollection GetCollectionToTest()
		{
			return new ChargeCodeWithTypeCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ChargeCodeWithType();
		}

		#endregion
	}
}
