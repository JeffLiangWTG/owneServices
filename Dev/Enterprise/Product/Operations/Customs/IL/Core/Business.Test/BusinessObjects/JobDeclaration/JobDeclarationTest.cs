using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(JobDeclaration))]
	class JobDeclarationTest : Customs.Business.Testing.BaseJobDeclarationTest<JobDeclaration>
	{
		public override void TestAreMultipleEntryInstructionsAllowed()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(false, declaration.AreMultipleEntryInstructionsAllowed);
		}

		public override void TestLocalCurrencyCoreOverride()
		{
			AssertEquals("Replace this with the correct currency code when implemented in a real country", Core.Constants.CurrencyCodes.Israel, Factory.New<JobDeclaration>().LocalCurrencyCode);
		}

		public override void TestIsDeclarationWithEntryInstruction()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("IL Declaration should support EntryInstructions", !declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
		}

		public void TestMergeManagerType()
		{
			AssertType<MergeManager>("Should be a Customs.IL.Business.LineMerger.MergeManager", Factory.New<JobDeclaration>().MergeManager);
		}

		public void TestHouseBillsCollectionIsOfRightType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertType<BillCollection<Bill, JobDeclaration>>(declaration.Bills);
		}

		public void TestLookupObjectIsCached()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			var firstLookup = declaration.Lookups;
			var secondLookup = declaration.Lookups;
			AssertEquals(secondLookup, firstLookup);
		}

		/***
		 * !!! Test to be removed in production code !!!
		 * Please remove the whole method TestTypeDecider from the production code once the following test case passes.
		 * It is only meant as an initial completeness check immdiately after the country project is set up.
		 ***/
		public void TestTypeDecider()
		{
			AssertType<JobDeclaration>("Update dbo.JobDeclaration to include a decider for this class", Factory.New<JobDeclaration>());
		}

		public void TestFilteredInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		public void TestSupportingDocumentCollection()
		{
			var jobDeclaration = GetNewBusinessObject() as JobDeclaration;
			AssertType<SupportingDocumentCollection>(jobDeclaration.SupportingDocuments);
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var actualTypes = ((Integration.Customs.ICusSupportingInfoTypeSupporter)jobDeclaration).GetCusSupportingInfoTypes();
			AssertEquals(typeof(SupportingDocument), actualTypes["SUP"]);
		}

		public void TestGetFetchStrategies()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var expectedTypes = new[] { typeof(Customs.Business.FetchStrategies.CusSupportingInfoTypeSupporterFetchStrategy) };
			var actualTypes = ((IAdditionalBusinessObjectFetchStrategyProvider)jobDeclaration).GetFetchStrategies().Select(c => c.GetType());
			AssertContainsExactElementsInAnyOrder(expectedTypes, actualTypes);
		}

		public void TestGetCustomsEntryInstructionProvider()
		{
			var jobDeclaration = base.Factory.New<JobDeclaration>();
			AssertType<EntryInstructionProvider>(jobDeclaration.CustomsEntryInstructionProvider);
		}

		public void TestCustomsEntryInstructions()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			AssertType<CusEntryInstruction>(jobDeclaration.CustomsEntryInstructions.AddNew());
		}

		public void TestValidation()
		{
			var instruction = Factory.New<JobDeclaration>();
			AssertType<JobDeclarationValidation>(instruction.Validation);

			AssertEquals(typeof(AutoILJobDeclarationValidation), typeof(JobDeclarationValidation).BaseType);
		}

		public void TestCaptions()
		{
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.JE_MessageType), false, x => x.Caption == "Entry Type");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.JE_MessageSubType), false, x => x.Caption == "Entry Style");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.DeclarationNumber), false, x => x.Caption == "Declaration Number");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.JE_MasterBillIssuedDate), false, x => x.Caption == "Issue Date Time");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.JE_TransportMeans), false, x => x.Caption == "Cargo Type Code");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.JE_RL_NKOrigin), false, x => x.Caption == "Port Of Origin");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.JE_RL_NKFinalDestination), false, x => x.Caption == "Final Destination");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.JE_LocationOfGoods), false, x => x.Caption == "Goods Location");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.JE_TotalNoOfPacks), false, x => x.Caption == "Quantity");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.JE_CustomsOffice), false, x => x.Caption == "Customs Office");
			AssertHasCustomAttribute<ResourceStringDataAttribute>(typeof(JobDeclaration), nameof(JobDeclaration.JE_ManifestNumber), false, x => x.Caption == "Manifest");
		}

		public void TestJE_MessageSubTypeDefaults()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();

			jobDeclaration.JE_MessageType = "IMP";
			AssertEquals("When JE_MessageType is 'IMP' JE_MessageSubType should be 'IM'", (ZString)"IM", jobDeclaration.JE_MessageSubType);

			jobDeclaration.JE_MessageType = "EXP";
			AssertEquals("When JE_MessageType is 'EXP' JE_MessageSubType should be 'EX'", (ZString)"EX", jobDeclaration.JE_MessageSubType);
		}

		public void TestJE_TransportMeansDefaults()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = "IMP";
			AssertNullOrEmpty("Default value for JE_TransportMeans when import should be empty", jobDeclaration.JE_TransportMeans);

			jobDeclaration.JE_TransportMode = "AIR";
			AssertEquals("When JE_TransportMode is 'AIR' when import JE_TransportMeans should be '1'", (ZString)"1", jobDeclaration.JE_TransportMeans);

			jobDeclaration.JE_TransportMode = "SEA";
			AssertEquals("When JE_TransportMode is 'SEA' when import JE_TransportMeans should be '11'", (ZString)"11", jobDeclaration.JE_TransportMeans);

			jobDeclaration.JE_TransportMode = "ROA";
			AssertEquals("When JE_TransportMode is 'ROA' when import JE_TransportMeans should be '20'", (ZString)"20", jobDeclaration.JE_TransportMeans);

			jobDeclaration.JE_MessageType = "EXP";
			AssertEquals("When JE_TransportMode is 'ROA' when import JE_TransportMeans should be '30'", (ZString)"30", jobDeclaration.JE_TransportMeans);

			jobDeclaration.JE_TransportMode = "SEA";
			AssertEquals("When JE_TransportMode is 'SEA' when import JE_TransportMeans should be '13'", (ZString)"13", jobDeclaration.JE_TransportMeans);

			jobDeclaration.JE_TransportMode = "AIR";
			AssertEquals("When JE_TransportMode is 'AIR' when import JE_TransportMeans should be '16'", (ZString)"16", jobDeclaration.JE_TransportMeans);
		}

		public void TestJE_MasterBillIssuedDate()
		{
			// real test will be created in WF2
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MasterBillIssuedDate = ZDateTime.FromSqlFormat("2021-01-01 00:00:00.000");

			AssertEquals(ZDateTime.FromSqlFormat("2021-01-01 00:00:00.000"), jobDeclaration.JE_MasterBillIssuedDate);
		}

		public void TestJE_VoyageFlightNoCaption()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = Core.Constants.TransportModes.Road;
			var info = dec.JE_VoyageFlightNoInfo;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Road", "Voyage", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Sea", "Voyage", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Air", "Flight", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Flight", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
			dec.JE_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Rail", "Voyage / Flight No", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Voyage / Flight No", shortCaption: "Voyage/Flight", fullDescription: "A unique reference assigned by a carrier to identify a specific journey of an aircraft or vessel.", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
		}

		public void TestShipmentSynchroniser()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_JS = shipment.PK;
			AssertType<ILJobDeclarationSynchroniser>(jobDeclaration.ShipmentSynchroniser);
		}

		public void TestJE_LocationOfGoods_ListAttribute()
		{
			AssertHasCustomAttribute<ListAttribute>(typeof(JobDeclaration), "JE_LocationOfGoods", false, attrib => attrib.ListDataSourceMember == "Lookups.LocationOfGoodsCollection");
		}
	}
}

