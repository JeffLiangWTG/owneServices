using System;
using System.Collections.Generic;
using System.Reflection;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentScanning.Business.Test
{
	public class AssemblyDataLookupTest : TransactionedTestCase
	{
		public void TestGetAssemblyDataFromDocManagerCode_NoExceptionWhenNoUserContext()
		{
			using (Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNoExceptionThrown("No exception is thrown out because of empty company", () => AssemblyDataLookup.GetAssemblyDataFromDocManagerCode(Core.Constants.DocManagerCodes.Shipment));
			}
		}

		public void TestDocManagerCodesHaveAssemblyData()
		{
			ZStringBuilder problems = new ZStringBuilder();
			GlbCompany.CurrentCompany.SetCountry("ER");

			List<string> providerCodes = new List<string>();

			foreach (IAssemblyDataProvider provider in AssemblyDataLoader.GetAssemblyDataProviders())
			{
				providerCodes.Add(provider.DocManagerCode);
			}

			foreach (FieldInfo fieldInfo in typeof(Core.Constants.DocManagerCodes).GetFields(BindingFlags.Public | BindingFlags.Static))
			{
				if (fieldInfo.FieldType == typeof(string))
				{
					string value = (string)fieldInfo.GetValue(null);
					if (!string.IsNullOrEmpty(value) && value.Length == 3)
					{
						if (!providerCodes.Contains(value))
						{
							problems.Append("Could not load AssemblyData for code: [" + value + "] on [Enterprise.Core.Constants.DocManagerCodes." + fieldInfo.Name + "].");
						}
					}
					else
					{
						problems.Append("Expected all Constants on this list to be 3 in Length. Please check: [Enterprise.Core.Constants.DocManagerCodes." + fieldInfo.Name + "].");
					}
				}
			}

			Assert(problems.ToStringWithNewLineBetweenAppends(), problems.IsEmpty);
		}

		public void TestIsReferenceTypeSupported()
		{
			Assert(AssemblyDataLookup.IsDocManagerCodeValid(Core.Constants.DocManagerCodes.Shipment));
			Assert(!AssemblyDataLookup.IsDocManagerCodeValid("ABC"));
		}

		public void TestGetAssemblyDataFromReferenceType()
		{
			var d = AssemblyDataLookup.GetAssemblyDataFromDocManagerCode(Core.Constants.DocManagerCodes.Shipment);
			AssertEquals("Shipment", d.HumanReadableName);
			AssertEquals(Core.Constants.DocManagerCodes.Shipment, d.DocManagerCode);
			AssertEquals("SCL", d.ReferenceType);
			AssertEquals(ModuleIDs.JobShipment, d.ModuleID);
		}

		public void TestFormCategoryForAllocation()
		{
			CodeDescriptionPairList allocationList = AssemblyDataLookup.DocManagerCodesForAllocation;
			CodeDescriptionPairList wholeList = AssemblyDataLookup.DocManagerCodes;

			Assert("Allocation list has less elements than the whole list - some types are restricted", wholeList.Count > allocationList.Count);
			AssertEquals("Allocation list shouldn't contain any of the voyage properties", false, allocationList.ContainsCode("SEA"));
			AssertEquals("Allocation list shouldn't contain any of the voyage properties", false, allocationList.ContainsCode("RAI"));
			AssertEquals("Allocation list shouldn't contain any of the voyage properties", false, allocationList.ContainsCode("ROA"));
			AssertEquals("Allocation list shouldn't contain any of the voyage properties", false, allocationList.ContainsCode("AIR"));
			AssertEquals("Proper list should contain the voyage properties", true, wholeList.ContainsCode("SEA"));
			AssertEquals("Proper list should contain the voyage properties", true, wholeList.ContainsCode("RAI"));
			AssertEquals("Proper list should contain the voyage properties", true, wholeList.ContainsCode("ROA"));
			AssertEquals("Proper list should contain the voyage properties", true, wholeList.ContainsCode("AIR"));
		}

		public void TestGetFormCategoryListForRefDocType()
		{
			CodeDescriptionPairList sourceList = AssemblyDataLookup.DocManagerCodes;
			Assert("Source list contains UNA code", sourceList.ContainsCode("UNA"));

			CodeDescriptionPairList afterList = AssemblyDataLookup.ReferenceTypeList;
			Assert("After list doesn't contain UNA code", !afterList.ContainsCode("UNA"));
			Assert("AfterList contains  an ALL code", afterList.ContainsCode("ALL"));
			Assert("Source List still contains UNA code", sourceList.ContainsCode("UNA"));
		}

		public void TestDataLookupAcrossCountriesDoesNotCacheWrongInformation()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();

			GlbCompany company1 = factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			GlbBranch branch1 = factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company1.PK;

			GlbCompany company2 = factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
			GlbBranch branch2 = factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company2.PK;

			factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch1.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Assert(AssemblyDataLookup.IsDocManagerCodeValid(Core.Constants.DocManagerCodes.AirCargoHouse));
			}
			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch2.PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				Assert(!AssemblyDataLookup.IsDocManagerCodeValid(Core.Constants.DocManagerCodes.AirCargoHouse));
			}
		}

		public void TestDataLookupForAMS()
		{
			GlbCompany.CurrentCompany.SetCountry("SG");
			IAssemblyData assemblyAMS = AssemblyDataLookup.GetAssemblyDataFromDocManagerCode("AMS");
			AssertEquals("We should have AMS type", "Enterprise.Customs.US.AMS.Business.CusInBondHeader", assemblyAMS.BusinessObjectType.FullName);

			GlbCompany.CurrentCompany.SetCountry("US");
			IAssemblyData assemblyAMS1 = AssemblyDataLookup.GetAssemblyDataFromDocManagerCode("AMS");
			AssertEquals("We should have AMS type", "Enterprise.Customs.US.AMS.Business.CusInBondHeader", assemblyAMS1.BusinessObjectType.FullName);
		}

		public void TestGetBizoFromCode()
		{
			var shipment = (BusinessObject)masterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			var org = masterFactory.LoadTop1<OrgHeader>(new ZQuery());
			masterFactory.Save();

			var shipCode = shipment[JobShipmentSchema.JS_UniqueConsignRef].ToString();
			var orgCode = org.OH_Code;

			var shipmentLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Shipment, shipCode, false);
			var orgLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Organisation, orgCode, false);
			AssertEquals("Correct shipment should be returned", shipment.PK, shipmentLoaded.PK);
			AssertEquals("Correct Org should be returned", org.PK, orgLoaded.PK);
		}

		public void TestGetBizoFromComplexCode()
		{
			var org = masterFactory.LoadTop1<OrgHeader>(new ZQuery());

			var order = (BusinessObject)masterFactory.New<Enterprise.Integration.Forwarding.IOrder>();
			order[JobOrderHeaderSchema.JD_OrderNumber] = "0777A";
			order[JobOrderHeaderSchema.JD_OA_BuyerAddress] = org.MainAddress.PK;

			masterFactory.Save();

			AssertNull("Cannot load order due to invalid company code.", AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, "0777A", "Invalid", false));
			var orderLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, "0777A", org.OH_Code, false);
			AssertEquals("Correct order should be returned", order.PK, orderLoaded.PK);

			orderLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, "0777A " + org.OH_Code, false);
			AssertEquals("Correct order should be returned", order.PK, orderLoaded.PK);
		}

		public void TestGetBizoFromCodeWithSpace()
		{
			var product = (BusinessObject)masterFactory.New<Enterprise.Integration.Customs.Shared.IGlobalOrgSupplierPart>();
			product[AutoOrgSupplierPart.Schema.OP_PartNum] = "CST10001 00001";
			product[AutoOrgSupplierPart.Schema.OP_Desc] = "CST Test Product";
			masterFactory.Save();

			var productLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Product, "CST10001 00001", false);
			AssertEquals("product should be returned", product.PK, productLoaded.PK);
		}

		public void TestGetBizoFromCodeWithSpaceAndValidCompanyCode()
		{
			var codePart = "CST10001";
			var org1 = masterFactory.LoadTop1<OrgHeader>(new ZQuery());
			var org2 = GlbCompany.CurrentCompany.OrgProxy;

			var order1 = (BusinessObject)masterFactory.New<Enterprise.Integration.Forwarding.IOrder>();
			order1[JobOrderHeaderSchema.JD_OrderNumber] = codePart;
			order1[JobOrderHeaderSchema.JD_OA_BuyerAddress] = org1.MainAddress.PK;

			var order1_2 = (BusinessObject)masterFactory.New<Enterprise.Integration.Forwarding.IOrder>();
			order1_2[JobOrderHeaderSchema.JD_OrderNumber] = codePart;
			order1_2[JobOrderHeaderSchema.JD_OA_BuyerAddress] = org2.MainAddress.PK;

			var order2 = (BusinessObject)masterFactory.New<Enterprise.Integration.Forwarding.IOrder>();
			order2[JobOrderHeaderSchema.JD_OrderNumber] = codePart + " " + org1.OH_Code;
			order2[JobOrderHeaderSchema.JD_OA_BuyerAddress] = org1.MainAddress.PK;

			var order2_2 = (BusinessObject)masterFactory.New<Enterprise.Integration.Forwarding.IOrder>();
			order2_2[JobOrderHeaderSchema.JD_OrderNumber] = codePart + " " + org2.OH_Code;
			order2_2[JobOrderHeaderSchema.JD_OA_BuyerAddress] = org2.MainAddress.PK;

			masterFactory.Save();

			// order1
			var orderLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, codePart, org1.OH_Code, false);
			AssertEquals("order1 should be returned with given company code", order1.PK, orderLoaded.PK);

			orderLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, codePart + " " + org1.OH_Code, false);
			AssertEquals("order1 should be returned with code and company code in one string", order1.PK, orderLoaded.PK);

			//oreder 1_2
			orderLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, codePart, org2.OH_Code, false);
			AssertEquals("order1_2 should be returned with given company code", order1_2.PK, orderLoaded.PK);

			orderLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, codePart + " " + org2.OH_Code, false);
			AssertEquals("order1_2 should be returned with code and company code in one string", order1_2.PK, orderLoaded.PK);

			// order2
			orderLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, codePart + " " + org1.OH_Code, org1.OH_Code, false);
			AssertEquals("order2 should be returned with given company code", order2.PK, orderLoaded.PK);

			orderLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, codePart + " " + org1.OH_Code + " " + org1.OH_Code, false);
			AssertEquals("order2 should be returned with code and company code in one string", order2.PK, orderLoaded.PK);

			// order2_2
			orderLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, codePart + " " + org2.OH_Code, org2.OH_Code, false);
			AssertEquals("order2_2 should be returned with given company code", order2_2.PK, orderLoaded.PK);

			orderLoaded = AssemblyDataLookup.GetBusinessObjectFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, codePart + " " + org2.OH_Code + " " + org2.OH_Code, false);
			AssertEquals("order2_2 should be returned with code and company code in one string", order2_2.PK, orderLoaded.PK);
		}

		public void TestGetPKFromCode()
		{
			BusinessObject shipment = (BusinessObject)masterFactory.New<Enterprise.Integration.Freight.ICommonShipment>();
			OrgHeader org = masterFactory.LoadTop1<OrgHeader>(new ZQuery());
			masterFactory.Save();

			ZString shipCode = shipment[JobShipmentSchema.JS_UniqueConsignRef].ToString();
			ZString orgCode = org.OH_Code;

			AssertEquals("Correct shipment PK should be returned", shipment.PK, AssemblyDataLookup.GetPKFromCode(masterFactory, Core.Constants.DocManagerCodes.Shipment, shipCode, false));
			AssertEquals("Correct Org PK should be returned", org.PK, AssemblyDataLookup.GetPKFromCode(masterFactory, Core.Constants.DocManagerCodes.Organisation, orgCode, false));
		}

		public void TestGetPKFromCodeThrowNonUniqueAllocationCodeException()
		{
			var container1 = masterFactory.NewWithValidTestData<CommonContainer>();
			container1.JC_ContainerNum = "00001";

			var container2 = masterFactory.NewWithValidTestData<CommonContainer>();
			container2.JC_ContainerNum = "00001";

			masterFactory.Save();

			AssertExceptionThrown<NonUniqueAllocationCodeException>(() => AssemblyDataLookup.GetPKFromCode(masterFactory, "CNT", "00001", true));
		}

		public void TestGetPKFromCodeWhenCodeIsPk()
		{
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.UnitedKingdom);
			var houseBill = (BusinessObject)masterFactory.New<Enterprise.Integration.Customs.GB.CCSUK.ICusHAWB>(); // this guy's AssemblyData supports the PK-based lookup
			masterFactory.Save();
			var hawbPkFromBarCode = houseBill.PK.ToString();
			AssertEquals("Correct HAWB PK should be returned, if not then perhaps an AU AssemblyData was loaded or the GB HAWB AssemblyData no longer supports PK-based lookups", houseBill.PK, AssemblyDataLookup.GetPKFromCode(masterFactory, Core.Constants.DocManagerCodes.AirCargoHouse, hawbPkFromBarCode, false));
		}

		public void TestGetPKFromComplexCode()
		{
			OrgHeader org = masterFactory.LoadTop1<OrgHeader>(new ZQuery());

			BusinessObject order = (BusinessObject)masterFactory.New<Enterprise.Integration.Forwarding.IOrder>();
			order[JobOrderHeaderSchema.JD_OrderNumber] = "0777A";
			order[JobOrderHeaderSchema.JD_OA_BuyerAddress] = org.MainAddress.PK;

			masterFactory.Save();

			AssertEquals("Correct order PK should be returned", order.PK, AssemblyDataLookup.GetPKFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, "0777A", org.OH_Code, false));
			AssertEquals("Correct order PK should be returned", order.PK, AssemblyDataLookup.GetPKFromCode(masterFactory, Core.Constants.DocManagerCodes.Order, "0777A " + org.OH_Code, false));
		}

		//		TODO: Don't delete this, useful test to see what kinds of base table names certain bizOs share
		//		public void TestShowMeAllTheBaseTableNames()
		//		{
		//			ZString Results = System.Environment.NewLine;
		//			foreach(AssemblyData Element in AssemblyDataLookup.Data)
		//			{
		//				BusinessObject BizO = MasterFactory.New(Element.BusinessObjectType);
		//				Results += Element.HumanReadableName + ": base table " + BizO.TableName + System.Environment.NewLine;
		//			}
		//			Fail(Results);
		//		}

		public void TestAllAssemblyData_WhenCurrentCompanyIsNull_ShouldNotReload()
		{
			var assemblyData = AssemblyDataLookup.AllAssemblyData;
			AssertNotNull(assemblyData);
			using (Environment.Env.SetTemporaryUserContext(Guid.Empty, Guid.Empty, Guid.Empty))
			{
				AssertNull(GlbCompany.CurrentCompany);
				AssertNotNull(AssemblyDataLookup.AllAssemblyData);
			}
		}

		public void TestCleanupMemoryAfterTest()
		{
			Assert(AssemblyDataLookup.IsDocManagerCodeValid(Core.Constants.DocManagerCodes.Shipment));
			Assert(AssemblyDataLookup.StateIsSet());
			Overridable.ResetAll();
			AssertEquals(false, AssemblyDataLookup.StateIsSet());
		}

		protected override void SetUp()
		{
			base.SetUp();
			masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			AssemblyDataLookup.ClearDataForTesting();
		}

		protected override void TearDown()
		{
			AssemblyDataLookup.ClearDataForTesting();
			base.TearDown();
		}

		DocumentFactory masterFactory;
	}

	internal abstract class AssemblyDataTestCase : TransactionedTestCase
	{
		public void TestAllAssemblyDataElementsHaveACodeProperty()
		{
			var problems = new ZStringBuilder();
			var factory = new DbBackendDocumentFactory(new BusinessObjectFactory());

			foreach (var provider in AssemblyDataLoader.GetAssemblyDataProviders())
			{
				var dataHolder = new AssemblyDataHolder(provider);
				var type = GetMostRelevantType(dataHolder, factory);

				try
				{
					CodePropertyAttribute.CodePropertyNameFromType(type);
				}
				catch (NoCodePropertyException)
				{
					problems.Append("BusinessObject [" + type + "] CodeProperty is missing");
				}
			}

			Assert(problems.ToStringWithNewLineBetweenAppends(), problems.IsEmpty);
		}

		Type GetMostRelevantType(IAssemblyData assemblyData, DocumentFactory factory)
		{
			var type = assemblyData.BusinessObjectType;
			if (TypeDecider.GetTypeDeciderFromType(type) is ITypeDecider typeDecider && typeDecider.GetTypeForNew() is { } newType)
			{
				type = newType;
			}

			if (assemblyData.GetBusinessObjectCollection(factory) is { } collection && collection.TypeOfElements.IsSubclassOf(type))
			{
				type = collection.TypeOfElements;
			}

			return type;
		}

		public void TestAllCollectionsAreSuitableForFindbox()
		{
			ZString results = ZString.Empty;

			foreach (var pair in AssemblyDataLookup.AllAssemblyData)
			{
				if (pair.Value.IsAllowedForUnallocatedeDocs)
				{
					IBusinessObjectCollection collection = pair.Value.GetBusinessObjectCollection(new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory()));
					if (collection != null)
					{
						Type type = collection.GetType();

						if (IsInInheritanceHierarchy(type, typeof(ManyToManyBusinessObjectCollection)) ||
							typeof(IDependentBusinessObjectCollection).IsAssignableFrom(type) ||
							IsInInheritanceHierarchy(type, typeof(FlatHierarchyBusinessObjectCollection)))
						{
							results += type + " is not a plain BusinessObjectCollection. A plain BusinessObjectCollection (suitable for a module findbox) is required for DocumentScanning." + System.Environment.NewLine;
						}
					}
				}
			}

			Assert(results, results.IsEmpty);
		}

		public void TestAllBusinessObjectsImplementIDocManagerSupport()
		{
			var results = ZString.Empty;
			var factory = new DbBackendDocumentFactory(new BusinessObjectFactory());

			foreach (var pair in AssemblyDataLookup.AllAssemblyData)
			{
				var assemblyData = pair.Value;
				var businessObjectType = GetMostRelevantType(assemblyData, factory);

				if (!typeof(IDocManagerSupport).IsAssignableFrom(businessObjectType))
				{
					results += System.Environment.NewLine + pair.Value.BusinessObjectType.ToString() + " does not implement IDocManagerSupport";
				}
			}

			Assert(results, results.IsEmpty);
		}

		public virtual void TestAllDocManagerCodesMatchInterfaces()
		{
			var factory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			foreach (var pair in AssemblyDataLookup.AllAssemblyData)
			{
				var businessObjectType = GetMostRelevantType(pair.Value, factory);
				var element = pair.Value;
				BusinessObject testObject1;
				if (businessObjectType.IsSubclassOf(typeof(JobHeader)))
				{
					testObject1 = MasterFactory.FactoryForEverythingExceptEDocs.NewJobForTesting<JobHeader>();
				}
				else if (businessObjectType.Name == (typeof(ViewQuotedBooking)).Name)
				{
					var testObject = factory.New<ViewQuotedBooking>();
					if (element.DocManagerCode == Core.Constants.DocManagerCodes.OneOffQuote)
					{
						var quote = factory.NewWithValidTestData<RatingHeader>();
						quote.TH_OneTimeQuote = true;
						testObject.VB_TH = quote.PK;

						AssertEquals("QU1", ((IDocManagerSupport)testObject).DocManagerInfo.DocManagerCode);
					}
					else if (element.DocManagerCode == Core.Constants.DocManagerCodes.Booking)
					{
						var booking = factory.NewWithValidTestData<ForwardingShipment>();
						testObject.VB_JS = booking.PK;

						AssertEquals("BKG", ((IDocManagerSupport)testObject).DocManagerInfo.DocManagerCode);
					}
					continue;
				}
				else
				{
					testObject1 = MasterFactory.New(businessObjectType);
				}

				if (!IsDuplicateBusinessObjectType(businessObjectType)) // for AU classification
				{
					Assert("Code for " + businessObjectType.ToString() + " (" + ((IDocManagerSupport)testObject1).DocManagerInfo.DocManagerCode + ") does not match string in Lookup (" + element.DocManagerCode + ")",
						element.DocManagerCode == ((IDocManagerSupport)testObject1).DocManagerInfo.DocManagerCode);
				}
				else
				{
					// the way this tests the elements is a little bit of a hack, but the test will still fail if we haven't tested everything
					switch (element.DocManagerCode)
					{
						case "IMC":
							testObject1[CusClassificationSchema.CC_ClassificationType] = "IMP";
							AssertEquals("IMC", ((IDocManagerSupport)testObject1).DocManagerInfo.DocManagerCode);
							break;
						case "EXC":
							testObject1[CusClassificationSchema.CC_ClassificationType] = "EXP";
							AssertEquals("EXC", ((IDocManagerSupport)testObject1).DocManagerInfo.DocManagerCode);
							break;
						case "SEA":
							testObject1[JobVoyageSchema.JV_AirSeaRoad] = "SEA";
							AssertEquals("SEA", ((IDocManagerSupport)testObject1).DocManagerInfo.DocManagerCode);
							break;
						case "ROA":
							testObject1[JobVoyageSchema.JV_AirSeaRoad] = "ROA";
							AssertEquals("ROA", ((IDocManagerSupport)testObject1).DocManagerInfo.DocManagerCode);
							break;
						case "RAI":
							testObject1[JobVoyageSchema.JV_AirSeaRoad] = "RAI";
							AssertEquals("RAI", ((IDocManagerSupport)testObject1).DocManagerInfo.DocManagerCode);
							break;
						case "AIR":
							testObject1[JobVoyageSchema.JV_AirSeaRoad] = "AIR";
							AssertEquals("AIR", ((IDocManagerSupport)testObject1).DocManagerInfo.DocManagerCode);
							break;
						case "CRT":
							AssertEquals("CRT", ((IDocManagerSupport)testObject1).DocManagerInfo.DocManagerCode);
							break;
						case "GCL":
							testObject1[RatingHeaderSchema.TH_GC] = ZGuid.Empty;
							AssertEquals("GCL", ((IDocManagerSupport)testObject1).DocManagerInfo.DocManagerCode);
							break;
						case "CST":
							AssertEquals("CST", ((IDocManagerSupport)testObject1).DocManagerInfo.DocManagerCode);
							break;
						case "GCO":
							testObject1[RatingHeaderSchema.TH_GC] = ZGuid.Empty;
							AssertEquals("GCO", ((IDocManagerSupport)testObject1).DocManagerInfo.DocManagerCode);
							break;
						default:
							Assert(string.Format("Found a duplicate business object ({0}) in the assembly data test case that wasn't tested!", testObject1.ToString()), false);
							break;
					}
				}
			}
		}

		protected bool IsDuplicateBusinessObjectType(Type lookFor)
		{
			int typeCount = 0;

			foreach (var pair in AssemblyDataLookup.AllAssemblyData)
			{
				if (pair.Value.BusinessObjectType == lookFor)
				{
					typeCount++;
				}
			}

			return (typeCount > 1);
		}

		protected bool IsInInheritanceHierarchy(Type typeToInspect, Type target)
		{
			if (typeToInspect == typeof(object))
			{
				return false;
			}

			if (typeToInspect != target)
			{
				return IsInInheritanceHierarchy(typeToInspect.BaseType, target);
			}
			else
			{
				return true;
			}
		}

		protected virtual bool AddClientSpecificElementTests(ZString result, IAssemblyData element)
		{
			return false;
		}

		protected override void SetUp()
		{
			base.SetUp();
			MasterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
			Lookup = new AssemblyDataLookup();
			AssemblyDataLookup.ClearDataForTesting();
		}

		protected override void TearDown()
		{
			AssemblyDataLookup.ClearDataForTesting();
			base.TearDown();
		}

		protected DocumentFactory MasterFactory;
		protected AssemblyDataLookup Lookup;
	}

	internal class PluginAssemblyDataValidTestForEDI : AssemblyDataTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			AssemblyDataLookup.ClearDataForTesting();
			overridenClientAssembly = ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI);
		}

		IDisposable overridenClientAssembly;

		protected override void TearDown()
		{
			AssemblyDataLookup.ClearDataForTesting();
			overridenClientAssembly.Dispose();
			base.TearDown();
		}
	}
}
