using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.EU.Business.CusTempStorage.Testing.TemporaryStorageTestHelper;
using static Enterprise.Customs.EU.Business.TemporaryStorageHelper;
using static Enterprise.Integration.Customs.TemporaryStorage;
using EUInterfaces = Enterprise.Integration.Customs.EU;

namespace Enterprise.Customs.EU.Business.CusTempStorage.Testing
{
	[TestedType(typeof(TemporaryStorageHeader))]
	sealed class TemporaryStorageHeaderTest : TemporaryStorageHeaderAbstractTest<TemporaryStorageHeader>
	{
		protected override BusinessObject GetNewBusinessObjectForDefaultLightValidationTest()
		{
			var bizO = Factory.New<TemporaryStorageHeader>();
			bizO.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.IntendedControl;
			return bizO;
		}

		public static string[] GetNoEditAllowedCustomsStatuses() => new[]
		{
			UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated,
			UniversalReferenceConstants.PNTS.CustomsStatus.UnderControl,
			UniversalReferenceConstants.PNTS.CustomsStatus.IntendedControl,
			UniversalReferenceConstants.PNTS.CustomsStatus.IrregularityUnderInvestigation,
			UniversalReferenceConstants.PNTS.CustomsStatus.ProofOfUnionStatusPresented,
			UniversalReferenceConstants.PNTS.CustomsStatus.MeasuresRequired,
			UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageEnded
		};

		public static string[] GetAmendableFieldsEditAllowedCustomsStatuses() => new[]
		{
			UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStoragePreLodged,
			UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStorageActivated,
			UniversalReferenceConstants.PNTS.CustomsStatus.PresentationNotificationLinked,
			UniversalReferenceConstants.PNTS.CustomsStatus.PresentationNotificationNotLinked
		};

		public void TestIsNoEditAllowedCustomsStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			foreach (var status in GetNoEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				AssertEquals($"header.IsNoEditAllowedCustomsStatus() - {status}", true, header.IsNoEditAllowedCustomsStatus());
				AssertEquals($"TemporaryStorageHeader.IsNoEditAllowedCustomsStatus({status})", true, TemporaryStorageHeader.IsNoEditAllowedCustomsStatus(status));
			}

			header.CustomsStatus = "!@";
			AssertEquals("header.IsNoEditAllowedCustomsStatus() - !@", false, header.IsNoEditAllowedCustomsStatus());
			AssertEquals("TemporaryStorageHeader.IsNoEditAllowedCustomsStatus(!@)", false, TemporaryStorageHeader.IsNoEditAllowedCustomsStatus("!@"));
		}

		public void TestIsAmendableFieldsEditAllowedCustomsStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			foreach (var status in GetAmendableFieldsEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				AssertEquals($"header.IsAmendableFieldsEditAllowedCustomsStatus() - {status}", true, header.IsAmendableFieldsEditAllowedCustomsStatus());
				AssertEquals($"TemporaryStorageHeader.IsAmendableFieldsEditAllowedCustomsStatus({status})", true, TemporaryStorageHeader.IsAmendableFieldsEditAllowedCustomsStatus(status));
			}

			header.CustomsStatus = "!@";
			AssertEquals("header.IsAmendableFieldsEditAllowedCustomsStatus() - !@", false, header.IsNoEditAllowedCustomsStatus());
			AssertEquals("TemporaryStorageHeader.IsAmendableFieldsEditAllowedCustomsStatus(!@)", false, TemporaryStorageHeader.IsAmendableFieldsEditAllowedCustomsStatus("!@"));
		}

		public void TestHasNoMasterBill()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.Bills.AddNew();

			header.HasNoMasterBill = true;
			AssertEquals("No MasterBill is expected in the Bills collection when HasNoMasterBill is true.", 1, header.Bills.Count);

			header.HasNoMasterBill = false;
			AssertEquals("Bills collections should contain all the bills including MasterBill when HasNoMasterBill is false.", 2, header.Bills.Count);
		}

		public void TestReadOnlyPropertiesUnderTSDStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.ENSReuse = 0;
			header.GoodsLocation.CGL_Type = "B";

			foreach (var status in GetNoEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				var propertyInfos = header.ZPropertyInfoHash;
				foreach (ZPropertyInfo propertyInfo in propertyInfos)
				{
					Assert(propertyInfo.ReadOnly);
				}
			}

			foreach (var status in GetAmendableFieldsEditAllowedCustomsStatuses())
			{
				header.CustomsStatus = status;
				header.ENSReuse = 0;

				if (header.IsPreLodged())
				{
					CombineAssertions("ENSReuse is false and Customs Status is pre-lodged", () =>
					{
						Assert(header.AMA_MessageTypeInfo.ReadOnly);
						Assert(header.IsENSReuseInfo.ReadOnly);
						Assert(header.AMA_OA_DeclarantInfo.ReadOnly);
						Assert(!header.DeclarationDateInfo.ReadOnly);
						Assert(!header.AMA_DateAtCustomsOfficeInfo.ReadOnly);
						Assert(!header.AMA_CustomsOfficeInfo.ReadOnly);
						Assert(!header.PresentationCustomsOfficeInfo.ReadOnly);
						Assert(!header.AMA_OA_PresenterInfo.ReadOnly);
						Assert(!header.AMA_OA_CarrierInfo.ReadOnly);
						Assert(!header.AMA_OA_RepresentativeInfo.ReadOnly);
						Assert(!header.ArrivalTransportMeansCodeInfo.ReadOnly);
						Assert(!header.AuthorizationTypeInfo.ReadOnly);
						Assert(!header.AuthorizationNumberInfo.ReadOnly);
						Assert(!header.PlaceOfUnloadingInfo.ReadOnly);
					});
				}
				else
				{
					CombineAssertions("ENSReuse is false and Customs Status is accepted", () =>
					{
						Assert(header.AMA_MessageTypeInfo.ReadOnly);
						Assert(header.IsENSReuseInfo.ReadOnly);
						Assert(header.AMA_OA_DeclarantInfo.ReadOnly);
						Assert(header.DeclarationDateInfo.ReadOnly);
						Assert(!header.AMA_DateAtCustomsOfficeInfo.ReadOnly);
						Assert(header.AMA_CustomsOfficeInfo.ReadOnly);
						Assert(!header.PresentationCustomsOfficeInfo.ReadOnly);
						Assert(!header.AMA_OA_PresenterInfo.ReadOnly);
						Assert(!header.AMA_OA_CarrierInfo.ReadOnly);
						Assert(!header.AMA_OA_RepresentativeInfo.ReadOnly);
						Assert(!header.ArrivalTransportMeansCodeInfo.ReadOnly);
						Assert(!header.AuthorizationTypeInfo.ReadOnly);
						Assert(!header.AuthorizationNumberInfo.ReadOnly);
						Assert(!header.PlaceOfUnloadingInfo.ReadOnly);
					});
				}

				header.ENSReuse = 1;
				CombineAssertions("If status allows editing modifiable fields and ENSReuse is true, all the fields which were reused from ICS can't be updated.", () =>
				{
					Assert(header.AMA_MessageTypeInfo.ReadOnly);
					Assert(header.IsENSReuseInfo.ReadOnly);
					Assert(header.DeclarationDateInfo.ReadOnly);
					Assert(header.AMA_DateAtCustomsOfficeInfo.ReadOnly);
					Assert(header.AMA_CustomsOfficeInfo.ReadOnly);
					Assert(header.PresentationCustomsOfficeInfo.ReadOnly);
					Assert(header.AMA_OA_PresenterInfo.ReadOnly);
					Assert(header.AMA_OA_DeclarantInfo.ReadOnly);
					Assert(header.AMA_OA_RepresentativeInfo.ReadOnly);
					Assert(header.GoodsLocationDescriptionInfo.ReadOnly);
					Assert(header.ArrivalTransportMeansCodeInfo.ReadOnly);
					Assert(header.AuthorizationTypeInfo.ReadOnly);
					Assert(header.AuthorizationNumberInfo.ReadOnly);
					Assert(header.PlaceOfUnloadingInfo.ReadOnly);
					Assert(header.AMA_OA_CarrierInfo.ReadOnly);
				});
			}
		}

		public void TestMessageTypeReadOnlyUnderTSPStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.TemporaryStoragePreLodged;
			header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;

			CombineAssertions("AMA_MessageType is not readonly when AMA_MessageType is TS and CustomsStatus is TSP.", () =>
			{
				Assert(header.AMA_MessageTypeInfo.ReadOnly);

				header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
				Assert("PreLodgedTempStorage && TSP", !header.AMA_MessageTypeInfo.ReadOnly);

				header.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
				Assert("PresentationNotification && TSP", !header.AMA_MessageTypeInfo.ReadOnly);

				header.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.PresentationNotificationLinked;
				Assert("PresentationNotification && not TSP", header.AMA_MessageTypeInfo.ReadOnly);
				header.AMA_MessageType = PNTSMessageTypeList.Codes.PreLodgedTempStorage;
				Assert("PreLodgedTempStorage && not TSP", header.AMA_MessageTypeInfo.ReadOnly);

				header.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
				Assert("not PreLodgedTempStorage && not PresentationNotification && not TSP", header.AMA_MessageTypeInfo.ReadOnly);
			});
		}

		public void TestCusGoodsLocationProvider()
		{
			var bizO = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			bizO.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;

			var provider = bizO as ICusGoodsLocationProvider;
			AssertEquals(ZString.Empty, provider.GoodsLocationDescription);

			AssertNotNull(provider.GoodsLocation);
			provider.GoodsLocation.CGL_Qualifier = "Q";
			provider.GoodsLocation.CGL_Type = "T";
			AssertEquals("Q;T", provider.GoodsLocationDescription);
			Assertion.AssertEquals(ZString.Empty, provider.ProviderKey);
		}

		public void TestBills()
		{
			var tempHeader = (TemporaryStorageHeader)GetNewBusinessObject();
			var bills = tempHeader.Bills;
			AssertType<TemporaryStorageBillCollection<TemporaryStorageBill, TemporaryStorageHeader>>(bills);
			Factory.Save();
			var bill = bills.AddNew();
			CombineAssertions("Bill can load Parent Temp. Header", () =>
			{
				AssertEquals("All bills in the collection should be linked to the tempHeader", bill.ABL_AMA, tempHeader.PK);
				AssertNotNull("All bills can load parent header", bill.Header);
				AssertEquals("The parent header should be loaded correctly", bill.Header.PK, tempHeader.PK);
			});
		}

		public void TestIDocManagerSupport()
		{
			IDocManagerSupport temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			AssertType<DocManagerInfo>("Type", temporaryStorageHeader.DocManagerInfo);
			AssertEquals("DocManagerCode", Core.Constants.DocManagerCodes.TempStorageHeaderUCC6, temporaryStorageHeader.DocManagerInfo.DocManagerCode);
		}

		public void TestIJobInvoicingPlugInMembers()
		{
			var tempHeader = (TemporaryStorageHeader)GetNewBusinessObject();
			var billing = tempHeader.InvoicingSupporter;
			AssertType<TemporaryStorageInvoicingSupporter>(billing);

			var tempHeaderParent = (IJobHeaderParent)GetNewBusinessObject();
			tempHeader.AMA_JobReference = "TestJob";
			AssertEquals("Billing linked to the tempHeader", "TestJob", tempHeader.AMA_JobReference);
			AssertEquals(false, tempHeaderParent.AllowInvoiceDeletion);
		}

		public void TestMessagingProvider()
		{
			var bizO = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			bizO.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			AssertEquals("Enterprise.Customs.FR.Business.CusTempStorage.TemporaryStorageMessagingProvider", bizO.MessagingProvider.GetType().FullName);

			bizO.AMA_RN_NKCountry = Core.Constants.CountryCodes.Latvia;
			AssertNull(bizO.MessagingProvider);
		}

		[TestDate(2022, 12, 19)]
		public void TestLRN_GenerationBasedOnSupportLRNGenerationConfiguration()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1230789654", "BE");

			using (TemporaryStorageConfigurationTestHelper.TemporarilyClearConfigurationAndSetSupportLRNGeneration(temporaryStorageHeader, configurationValue: true))
			{
				temporaryStorageHeader.LRN = "";
				Factory.Save();
				AssertEquals("When SupportLRNGeneration is active, After Factory.Save(), LRN", "2212307896540000000001", temporaryStorageHeader.LRN);
			}

			using (TemporaryStorageConfigurationTestHelper.TemporarilyClearConfigurationAndSetSupportLRNGeneration(temporaryStorageHeader, configurationValue: false))
			{
				temporaryStorageHeader.LRN = "";
				Factory.Save();
				AssertEquals("When SupportLRNGeneration is not active, After Factory.Save(), LRN", "", temporaryStorageHeader.LRN);
			}
		}

		[TestDate(2022, 12, 19)]
		public void TestLRN_AutomaticallyGeneratedWhenEmpty()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1230789654", "BE");
			(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);

			var lrnNumber = temporaryStorageHeader.GenerateLocalReferenceNumber();
			AssertEquals("Length of Local Reference Number should be 22 characters", 22, lrnNumber.Length);

			AssertStartsWith("Expected Local Reference Number to start with the last 2 characters of the current year.", "22", lrnNumber);

			string partThatShouldStartWithEORI = lrnNumber.Substring(2);
			AssertStartsWith("Expected Local Reference Number, after the first 2 characters, to contain the EORI of the Declarant.", "1230789654", partThatShouldStartWithEORI);

			string partThatShouldBe10RandomDigits = lrnNumber.Substring(12);
			AssertEquals("Expected Local Reference Number, after the first 12 characters, sequence 0000000001", "0000000001", partThatShouldBe10RandomDigits);
			AssertEquals("LRN", "2212307896540000000001", lrnNumber);

			(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 10);
			AssertEquals("LRN when the next sequence number is 10", "2212307896540000000010", temporaryStorageHeader.GenerateLocalReferenceNumber());
		}

		[TestDate(2022, 12, 19)]
		public void TestGenerateLocalReferenceNumber_Greece()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EL123654789", "GR");
			(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
			AssertEquals("2212365478900000000001", temporaryStorageHeader.GenerateLocalReferenceNumber());
		}

		[TestDate(2022, 12, 19)]
		public void TestGenerateLocalReferenceNumber_EoriDoesNotStartWithCountryCode()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EORI1230789654", "BE");
			(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
			AssertEquals("22EORI1230789654000001", temporaryStorageHeader.GenerateLocalReferenceNumber());
		}

		[TestDate(2022, 12, 19)]
		public void TestGenerateLocalReferenceNumber_EoriFromCompany()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.Branch.Company.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "1230789654", "BE");
			(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
			AssertEquals("2212307896540000000001", temporaryStorageHeader.GenerateLocalReferenceNumber());
		}

		[TestDate(2022, 10, 20)]
		public void TestGenerateLocalReferenceNumber_NoEori()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			AssertEquals(ZString.Empty, temporaryStorageHeader.GenerateLocalReferenceNumber());
		}

		[TestDate(2022, 10, 20)]
		public void TestGenerateLocalReferenceNumber_InvalidEori()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.Branch.OrgProxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "BE1234567890123456", "BE");
			(temporaryStorageHeader as ILRNGenerator).LrnNumberFountain.SetNext(Factory, 1);
			AssertEquals(ZString.Empty, temporaryStorageHeader.GenerateLocalReferenceNumber());
		}

		public void TestCodePropertyAndDescriptionProperty()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			Factory.Save();
			var codeDescription = (ICodeDescription)header;
			var jobReference = header.AMA_JobReference;
			CombineAssertions(() =>
			{
				AssertEquals("Code should be equal to AMA_JobReference", jobReference, codeDescription.Code);
				AssertEquals("Description should be equal to AMA_JobReference", jobReference, codeDescription.Description);
				AssertEquals("HumanReadableShortcutName should be equal to AMA_JobReference", jobReference, header.HumanReadableShortcutName);
			});
		}

		public void TestValidationType()
		{
			AssertType<TemporaryStorageHeaderValidation>(Factory.New<TemporaryStorageHeader>().Validation);
		}

		public void TestLookupsType()
		{
			AssertType<TemporaryStorageHeaderLookups>(Factory.New<TemporaryStorageHeader>().Lookups);
		}

		public void TestDataGrouping()
		{
			var bizO = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			AssertEquals("DataGrouping should reflect CustomsCountryOfJusridiction of  AMA_RN_NKCountry.", Core.Constants.CountryCodes.Latvia, bizO.DataGrouping);
			bizO.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			AssertEquals("DataGrouping should reflect CustomsCountryOfJusridiction of  AMA_RN_NKCountry.", Core.Constants.CountryCodes.France, bizO.DataGrouping);
		}

		public void TestMessages()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var message1 = Factory.New<EDIMessage>();
			message1.EM_LinkedObject = header;
			var message2 = Factory.New<EDIMessage>();
			message2.EM_LinkedObject = header;
			AssertContainsExactElementsInAnyOrder(new[] { message1, message2 }, header.Messages.Cast<EDIMessage>());
		}

		public void TestCaptions()
		{
			CombineAssertions(() =>
			{
				AssertEquals("AMA_MessageType", "Message Mode", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_MessageType)).Caption);
				AssertEquals("AMA_JobReference", "Job #", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_JobReference)).Caption);
				AssertEquals("AMA_DateAtCustomsOffice", "Goods Presentation Date", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_DateAtCustomsOffice)).Caption);
				AssertEquals("AMA_CustomsOffice", "Supervising Customs Office", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_CustomsOffice)).Caption);
				AssertEquals("AMA_GB", "Branch", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_GB)).Caption);
				AssertEquals("AMA_OA_Declarant", "Declarant", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_OA_Declarant)).Caption);
				AssertEquals("AMA_OA_Presenter", "Person Presenting the Goods", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_OA_Presenter)).Caption);
				AssertEquals("AMA_OA_Representative", "Representative", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_OA_Representative)).Caption);
				AssertEquals("AMA_TransportMode", "Transport Mode", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_TransportMode)).Caption);
				AssertEquals("AMA_SystemCreateTimeUtc", "Audit Details", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_SystemCreateTimeUtc)).Caption);
				AssertEquals("AMA_OA_Carrier", "Carrier", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_OA_Carrier)).Caption);
				AssertEquals("DeclarationDate", "Declaration Date", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.DeclarationDate)).Caption);
				AssertEquals("IsENSReuse", "ENS Re-use", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.IsENSReuse)).Caption);
				AssertEquals("HasHouseConsignment", "Has House Consignment?", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_Calc_HasHouseConsignment)).Caption);
				AssertEquals("TransportType", "Transport Type", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.TransportType)).Caption);
				AssertEquals("PlaceOfUnloading", "Place of Unloading", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.PlaceOfUnloading)).Caption);
				AssertEquals("PlaceOfLoading", "Place of Loading", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.PlaceOfLoading)).Caption);
				AssertEquals("MessageStatus", "Message Status", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_MessageStatus)).Caption);
				AssertEquals("CustomsStatus", "Customs Status", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.CustomsStatus)).Caption);
				AssertEquals("CustomsStatus Date", "Customs Status Date", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.CustomsStatusDate)).Caption);
				AssertEquals("LRN", "LRN #", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.LRN)).Caption);
				AssertEquals("MRN", "MRN #", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.MRN)).Caption);
				AssertEquals("CRN", "CRN #", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.CRN)).Caption);
				AssertEquals("FRN", "FRN #", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.FRN)).Caption);
				AssertEquals("PresentationCustomsOffice", "Presentation Customs Office", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.PresentationCustomsOffice)).Caption);
				AssertEquals("Broker", "Broker", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.AMA_GS_NKCustomsAgent)).Caption);
				AssertEquals("EstimatedDateOfArrival Caption", "Estimated Date of Arrival", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.EstimatedDateOfArrival)).Caption);
				AssertEquals("EstimatedDateOfArrival Short Caption", "ETA", DataBoundResourceStrings.GetDataForProperty(typeof(TemporaryStorageHeader), nameof(TemporaryStorageHeader.EstimatedDateOfArrival)).ShortCaption);
			});
		}

		public void TestDefaultValue()
		{
			var tempHeader = Factory.New<TemporaryStorageHeader>();
			AssertEquals("Default AMA_ApplicationCode", ApplicationCodeTypeList.Codes.TemporaryStorage, tempHeader.AMA_ApplicationCode);
			AssertEquals("Default AMA_AgentType", "AGT", tempHeader.AMA_AgentType);
			AssertEquals("Default AMA_RN_NkCountry", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, tempHeader.AMA_RN_NKCountry);
			AssertEquals("Default AMA_ManifestType", ZString.Empty, tempHeader.AMA_ManifestType);
			AssertEquals("Default Declarant to CurrentBranch orgProxy if CurrentBranch has orgProxy", GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK, tempHeader.AMA_OA_Declarant);

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			var tempHeader2 = Factory.New<TemporaryStorageHeader>();
			AssertEquals("Default Declarant to CurrentCompany orgProxy if CurrentBranch has no orgProxy", GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK, tempHeader2.AMA_OA_Declarant);

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			var temHeader3 = Factory.New<TemporaryStorageHeader>();
			AssertEquals("Declarant is empty if both CurrentBranch & CurrentCompany have no orgProxy", ZGuid.Empty, temHeader3.AMA_OA_Declarant);
		}

		public void TestAMA_GS_NKCustomsAgent_NotDefaultedOnSavingNewlyCreatedHeader_WhenCurrentUserIsSystemAccount_AndNotFilledManually()
		{
			CombineAssertions("For newly created TemporaryStorageHeader, CusAgent should not be defaulted if current user is a system account.", () =>
			{
				var tempHeader = (TemporaryStorageHeader)GetNewBusinessObject();
				Assert("Prerequisite: CurrentUser is a system account.", GlbStaff.CurrentUser.GS_IsSystemAccount);

				AssertEquals("For newly created TemporaryStorageHeader, CusAgent should be empty before saving.", ZString.Empty, tempHeader.AMA_GS_NKCustomsAgent);

				Factory.Save();
				tempHeader.Reload();
				AssertEquals("For newly created TemporaryStorageHeader, CusAgent should be empty after saving if current user is a system account.", ZString.Empty, tempHeader.AMA_GS_NKCustomsAgent);
			});
		}

		public void TestAMA_GS_NKCustomsAgent_ShouldBeDefaultedOnSavingNewlyCreatedHeader_WhenCurrentUserIsNotSystemAccount_AndNotFilledManually()
		{
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			CombineAssertions("For newly created TemporaryStorageHeader, CusAgent should be defaulted if current user isn't a system account.", () =>
			{
				var tempHeader = (TemporaryStorageHeader)GetNewBusinessObject();
				Assert("Prerequisite: CurrentUser isn't a system account.", !GlbStaff.CurrentUser.GS_IsSystemAccount);

				AssertEquals("For newly created TemporaryStorageHeader, CusAgent should be empty before saving.", ZString.Empty, tempHeader.AMA_GS_NKCustomsAgent);

				Factory.Save();
				tempHeader.Reload();
				AssertEquals("For newly created TemporaryStorageHeader, CusAgent should be defaulted after saving if current user isn't a system account.", GlbStaff.CurrentUser.GS_Code, tempHeader.AMA_GS_NKCustomsAgent);
			});
		}

		public void TestAMA_GS_NKCustomsAgent_NoActionsTaken_WhenFilledManually()
		{
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;
			var tempHeader = (TemporaryStorageHeader)GetNewBusinessObject();
			tempHeader.AMA_GS_NKCustomsAgent = "XXX";

			Factory.Save();
			tempHeader.Reload();
			AssertEquals("For newly created TemporaryStorageHeader, no actions should be taken during saving if CusAgent is filled manually.", "XXX", tempHeader.AMA_GS_NKCustomsAgent);
		}

		public void TestAMA_GS_NKCustomsAgent_NoActionsTakenForExistingHeader()
		{
			CombineAssertions(() =>
			{
				var tempHeader = (TemporaryStorageHeader)GetNewBusinessObject();
				Factory.Save();
				tempHeader.Reload();
				AssertEquals("Prerequisite: tempHeader.AMA_GS_NKCustomsAgent should be empty.", ZString.Empty, tempHeader.AMA_GS_NKCustomsAgent);

				GlbStaff.CurrentUser.GS_IsSystemAccount = false;
				SetIrrelevantProperty();
				Factory.Save();
				tempHeader.Reload();
				AssertEquals("For existing TeTemporaryStorageHeader, no actions should be taken during saving.", ZString.Empty, tempHeader.AMA_GS_NKCustomsAgent);

				void SetIrrelevantProperty()
				{
					tempHeader.AMA_TransportMode = "AIR";
				}
			});
		}

		public void TestAMA_GS_NKCustomsAgent_NoActionsTakenWhenConfigurationIsNotActive()
		{
			GlbStaff.CurrentUser.GS_IsSystemAccount = false;

			var temporaryStorageHeader = (TemporaryStorageHeader)GetNewBusinessObject();
			using var configuration = TemporaryStorageConfigurationTestHelper.TemporarilyClearConfigurationAndSetSupportAgentDefaulting(temporaryStorageHeader, configurationValue: false);
			Factory.Save();
			AssertEquals("When SupportAgentDefaulting is not active, AMA_GS_NKCustomsAgent should not be defaulted after saving.", "", temporaryStorageHeader.AMA_GS_NKCustomsAgent);
		}

		public void TestDeclarationDate()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = storageHeader.PK;
			entryNumber.CE_ParentTable = storageHeader.TableName;
			entryNumber.CE_EntryType = "LRN";
			entryNumber.CE_EntryNum = "970628";
			entryNumber.CE_IssueDate = new ZDate(1997, 6, 28);
			Factory.Save();

			AssertEquals("DeclarationDate", new ZDate(1997, 6, 28), storageHeader.DeclarationDate);
		}

		public void TestIsENSReuse()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			storageHeader.IsENSReuse = true;
			AssertEquals("ENSReuse can be relected by IsENSReuse", new ZByte(1), storageHeader.ENSReuse);

			storageHeader.IsENSReuse = false;
			AssertEquals("ENSReuse can be relected by IsENSReuse", new ZByte(0), storageHeader.ENSReuse);

			storageHeader.ENSReuse = ZByte.Zero;
			AssertEquals("IsENSReuse can be relected by ENSReuse", false, storageHeader.IsENSReuse);
			storageHeader.ENSReuse = new ZByte(1);
			AssertEquals("IsENSReuse can be relected by ENSReuse", true, storageHeader.IsENSReuse);

			Assert(!storageHeader.IsENSReuseInfo.ReadOnly);
			storageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.PresentationNotification;
			Assert(storageHeader.IsENSReuseInfo.ReadOnly);
		}

		public void TestAMA_Calc_HasHouseConsignment_NotifyUnableToCompleteActionAsHouseBillExistsDelegate_Exception()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			AssertNoExceptionThrown("Should not throw an exception when there is no HWB and NotifyUnableToCompleteActionAsHouseBillExistsDelegate has not been set", () =>
			{
				storageHeader.AMA_Calc_HasHouseConsignment = false;
			});

			storageHeader.Bills.AddNew();

			AssertExceptionThrown<InvalidOperationException>("Should throw an exception when there is at least an HWB and NotifyUnableToCompleteActionAsHouseBillExistsDelegate has not been set", () =>
			{
				storageHeader.AMA_Calc_HasHouseConsignment = false;
			});
		}

		public void TestAMA_Calc_HasHouseConsignment_ValueChanged()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var errorMessage = string.Empty;
			const string expectedErrorMessage = "This action cannot be completed due to the presence of at least one house bill. Kindly remove all existing house bills and attempt the action again.";
			storageHeader.NotifyUnableToCompleteActionAsHouseBillExists += (s) => errorMessage = s;

			storageHeader.AMA_Nature = string.Empty;

			storageHeader.AMA_Calc_HasHouseConsignment = true;
			AssertEquals("No error message shown when changing and there are no HouseBills", string.Empty, errorMessage);
			AssertEquals("H", storageHeader.AMA_Nature);

			storageHeader.AMA_Calc_HasHouseConsignment = false;
			AssertEquals("No error message shown when changing and there are no HouseBills", string.Empty, errorMessage);
			AssertEquals("M", storageHeader.AMA_Nature);

			storageHeader.AMA_Nature = "H";
			storageHeader.Bills.AddNew();
			storageHeader.AMA_Calc_HasHouseConsignment = false;
			AssertEquals("An error is shown if changing the value of AMA_Nature to 'M' when there are HouseBills", expectedErrorMessage, errorMessage);
			AssertEquals("H", storageHeader.AMA_Nature);
		}

		public void TestAMA_Calc_HasHouseConsignment_WhenAMA_NatureChanged()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			storageHeader.AMA_Nature = "M";
			AssertEquals("Returns false when AMA_Nature = 'M'", false, storageHeader.AMA_Calc_HasHouseConsignment);
			storageHeader.AMA_Nature = "H";
			AssertEquals("Returns true when AMA_Nature = 'H'", true, storageHeader.AMA_Calc_HasHouseConsignment);
		}

		public void TestAMA_Calc_HasHouseConsignment_BillsRefreshBinding()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			// Test we call RefreshBinding on bills collection
			var refreshCount = 0;
			storageHeader.Bills.ListChanged += (s, e) => refreshCount++;

			storageHeader.AMA_Calc_HasHouseConsignment = true;
			AssertEquals("Bills refresh binding has been called", 1, refreshCount);

			storageHeader.AMA_Calc_HasHouseConsignment = true;
			AssertEquals("Bills refresh binding has been called", 2, refreshCount);
		}

		public void TestChangeAMA_Nature()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			storageHeader.AMA_Nature = string.Empty;
			storageHeader.AMA_Calc_HasHouseConsignment = false;
			storageHeader.AMA_Nature = "A";
			AssertEquals("A", storageHeader.AMA_Nature);

			storageHeader.AMA_Nature = string.Empty;
			storageHeader.AMA_Calc_HasHouseConsignment = true;
			storageHeader.AMA_Nature = "B";
			AssertEquals("B", storageHeader.AMA_Nature);
		}

		public void TestTransportType()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var arrivalTransportMeans = Factory.New<ArrivalTransportMeans>();
			arrivalTransportMeans.TPM_ParentID = storageHeader.PK;
			arrivalTransportMeans.TPM_ParentTableCode = "AMA";
			arrivalTransportMeans.TPM_TypeOfIdentification = "10";
			Factory.Save();

			AssertEquals("TransportType can be saved in the ArrivalTransportMeans.TP_TypeOfIdentification", "10", storageHeader.TransportType);
		}

		public void TestTransportTypeCaption()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(storageHeader.TransportTypeInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Transport Type", resourceStringData.Caption);
				AssertEquals("FullDescription", "[19 06 061 000] Arrival Transport Means > Type of Identification", resourceStringData.FullDescription);
			});
		}

		public void TestArrivalTransportMeans()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			storageHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;

			var arrivalTransportMeans = Factory.New<ArrivalTransportMeans>();
			arrivalTransportMeans.TPM_ParentID = storageHeader.PK;
			arrivalTransportMeans.TPM_ParentTableCode = "AMA";
			arrivalTransportMeans.TPM_IdentificationNumber = "IMO001122";
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("Arrival Transport Means can be save in the ArrivalTransportMeans, storageHeader.ArrivalTransportMeans PK should equal the ArrivalTransportMeans.PK we have created.", arrivalTransportMeans.PK, storageHeader.ArrivalTransportMeans.PK);
				AssertEquals("ArrivalTransportMeansCode can be saved in the ArrivalTransportMeans.TPM_IdentificationNumber", "IMO001122", storageHeader.ArrivalTransportMeansCode);
			});
		}

		public void TestPlaceOfUnloading()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			storageHeader.MasterBill.ABL_RL_NKPortOfDischarge = "PORT1";
			Factory.Save();

			storageHeader.Reload();
			AssertEquals("Place of Unloading", "PORT1", storageHeader.PlaceOfUnloading);
		}

		public void TestPlaceOfUnloadingMaxLength()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			AssertEquals(5, storageHeader.PlaceOfLoadingInfo.MaxLength);
		}

		public void TestPlaceOfLoading()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			storageHeader.MasterBill.ABL_RL_NKPortOfLoading = "PORT0";
			Factory.Save();

			storageHeader.Reload();
			AssertEquals("Place of Loading", "PORT0", storageHeader.PlaceOfLoading);
		}

		public void TestPlaceOfLoadingMaxLength()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			AssertEquals(5, storageHeader.PlaceOfLoadingInfo.MaxLength);
		}

		public void TestEstimatedDateOfArrival()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			storageHeader.MasterBill.ABL_A_ARV = ZDate.BrettsBirthday;
			Factory.Save();

			storageHeader.Reload();
			AssertEquals("Estimated Date of Arrival", ZDate.BrettsBirthday, storageHeader.EstimatedDateOfArrival);
		}

		public void TestCustomsStatus()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = storageHeader.PK;
			entryNumber.CE_EntryType = "ASY";
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			entryNumber.CE_EntryNum = "970628";
			entryNumber.CE_EntryStatus = "STA";
			Factory.Save();

			storageHeader.Reload();
			AssertEquals($"Customs Status can be loaded correctly", "STA", storageHeader.CustomsStatus);
		}

		public void TestCustomsStatusDescription()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: dataGrouping);
			helper.CreateCusCodeList(Core.Constants.CountryCodes.Latvia, UniversalReferenceConstants.RefCusCodeListType.Code.TemporaryStorageCustomsStatus, "TSA", "Temporary Storage Activated", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			AssertEquals("Empty code", ZString.Empty, storageHeader.CustomsStatusDescription);

			storageHeader.CustomsStatus = "TSA";
			AssertEquals("Valid code", "Temporary Storage Activated", storageHeader.CustomsStatusDescription);

			storageHeader.CustomsStatus = "@@@";
			AssertEquals("Invalid code", ZString.Empty, storageHeader.CustomsStatusDescription);
		}

		public void TestMessageStatusDescription()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();

			AssertEquals("Empty code", ZString.Empty, storageHeader.CustomsStatusDescription);

			storageHeader.AMA_MessageStatus = PNTSMessageStatusList.Codes.Sent;
			AssertEquals("Valid code", PNTSMessageStatusList.Descriptions.Sent, storageHeader.MessageStatusDescription);

			storageHeader.AMA_MessageStatus = "@@@";
			AssertEquals("Invalid code", ZString.Empty, storageHeader.MessageStatusDescription);
		}

		public void TestLRN()
		{
			AssertEntryNumber(CusEntryNumberTypes.Standard.LocalReferenceNumber, nameof(TemporaryStorageHeader.LRN));
		}

		public void TestMRN()
		{
			AssertEntryNumber(CusEntryNumberTypes.Standard.MovementReferenceNumber, nameof(TemporaryStorageHeader.MRN));
		}

		public void TestCRN()
		{
			AssertEntryNumber(CusEntryNumberTypes.EU.CustomsRegistrationNumber, nameof(TemporaryStorageHeader.CRN));
		}

		public void TestFRN()
		{
			AssertEntryNumber(CusEntryNumberTypes.EU.FunctionalReferenceNumber, nameof(TemporaryStorageHeader.FRN));
		}

		void AssertEntryNumber(ZString entryType, ZString propertyName)
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = storageHeader.PK;
			entryNumber.CE_ParentTable = storageHeader.TableName;
			entryNumber.CE_EntryType = entryType;
			entryNumber.CE_RN_NKCountryCode = GlbCompany.CurrentCompany.Country.Code;
			entryNumber.CE_EntryNum = "970628";
			Factory.Save();

			storageHeader.Reload();
			AssertEquals($"{entryType} number can be loaded correctly", "970628", storageHeader.GetType().GetProperty(propertyName).GetValue(storageHeader));
		}

		public void TestPresentationCustomsOffice()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			AssertEquals(ZString.Empty, storageHeader.PresentationCustomsOffice);

			var codeData = storageHeader.PresentationCustomsOfficeCode;
			codeData.CY_ParentID = storageHeader.PK;
			codeData.CY_Type = "EUO";
			codeData.CY_Code = "PRE";
			codeData.CY_ParentTableCode = AsycudaManifestHeaderSchema.Constants.Prefix;
			AssertEquals(ZString.Empty, storageHeader.PresentationCustomsOffice);

			codeData.CY_Data = "7758258";
			AssertEquals("7758258", storageHeader.PresentationCustomsOffice);
		}

		public void TestPresentationCustomsOfficeCode()
		{
			var storageHeader = Factory.New<TemporaryStorageHeader>();
			storageHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			CombineAssertions(() =>
			{
				AssertEquals("PK", storageHeader.PK, storageHeader.PresentationCustomsOfficeCode.CY_ParentID);
				AssertEquals("CY_ParentTableCode", AsycudaManifestHeaderSchema.Constants.Prefix, storageHeader.PresentationCustomsOfficeCode.CY_ParentTableCode);
				AssertEquals("CY_Type", "EUO", storageHeader.PresentationCustomsOfficeCode.CY_Type);
				AssertEquals("CY_Code", "PRE", storageHeader.PresentationCustomsOfficeCode.CY_Code);
			});
		}

		public void TestGetCusTempStorageJobHeaderProcessTaskCollection()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertType("WorkflowItems's type should be ProcessTaskCollection<TemporaryStorageHeaderProcessTask, TemporaryStorageHeader>", typeof(ProcessTaskCollection<TemporaryStorageHeaderProcessTask, TemporaryStorageHeader>), ((IWorkflowProvider)header).WorkflowItems);
		}

		public void TestJobReferenceFormat()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			Factory.Save();
			AssertEquals("TSD0000001", header.AMA_JobReference);
			var header2 = Factory.New<TemporaryStorageHeader>();
			Factory.Save();
			AssertEquals("TSD0000002", header2.AMA_JobReference);
			var header3 = Factory.New<TemporaryStorageHeader>();
			Factory.Save();
			AssertEquals("TSD0000003", header3.AMA_JobReference);
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<TemporaryStorageContainer, TemporaryStorageHeader>);

		public void TestCustomsStatusReadOnly()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals("CustomsStatus should always be read only.", true, header.CustomsStatusInfo.ReadOnly);
		}

		public void TestCustomsStatusDateReadOnly()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals("CustomsStatusDate should always be read only.", true, header.CustomsStatusDateInfo.ReadOnly);
		}

		public void TestMessageStatusReadOnly()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals("MessageStatus should always be read only.", true, header.AMA_MessageStatusInfo.ReadOnly);
		}

		public void TestCustomsStatusDate()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var entryNumber = Factory.NewWithValidTestData<CusEntryNumber>();
			entryNumber.CE_ParentID = storageHeader.PK;
			entryNumber.CE_EntryType = CusEntryNumberTypes.ASYCUDA.AsycudaRegistration;
			entryNumber.CE_EntryNum = "970628";
			entryNumber.CE_IssueDate = new ZDateTime(1997, 6, 28, 12, 34, 56);
			Factory.Save();

			AssertEquals("CustomsStatusDate", new ZDateTime(1997, 6, 28, 12, 34, 56), storageHeader.CustomsStatusDate);
		}

		public void TestRegistrationEntryNumberUpdatedByCustomsStatusDate()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals(ZDateTime.Empty, header.RegistrationEntryNumber?.CE_IssueDate ?? ZDateTime.Empty);
			header.CustomsStatusDate = new ZDateTime(1997, 6, 28, 12, 34, 56);
			AssertEquals("RegistrationEntryNumber.CE_IssueDate should follow CustomsStatusDate.", new ZDateTime(1997, 6, 28, 12, 34, 56), header.RegistrationEntryNumber.CE_IssueDate);
		}

		public void TestRegistrationEntryNumberUpdatedByCustomsStatus()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals(ZString.Empty, header.RegistrationEntryNumber?.CE_EntryStatus ?? ZString.Empty);
			header.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated;
			AssertEquals("RegistrationEntryNumber.CE_EntryStatus should follow CustomsStatus.", UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated, header.RegistrationEntryNumber.CE_EntryStatus);
		}

		public void TestRegistrationEntryNumber()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.CustomsStatus = "XXX";
			AssertNotNull("RegistrationEntryNumber should be created if not already created.", header.RegistrationEntryNumber);
			AssertEquals("RegistrationEntryNumber type should be ASY.", CusEntryNumberTypes.ASYCUDA.AsycudaRegistration, header.RegistrationEntryNumber.CE_EntryType);
		}

		public void TestCustomsStatusLoggedOnCustomsStatusChange()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var customsStatusLogs = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code));
			AssertEquals("No information regarding CustomsStatus should have been logged.", 0, customsStatusLogs.Length);

			header.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated;
			Factory.Save();
			customsStatusLogs = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code));
			AssertEquals("A new log for Customs Status should have been added on Customs status change.", 1, customsStatusLogs.Length);
			AssertEquals("New log reference should match new Customs status.", UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated, customsStatusLogs[0].SL_Reference);

			header.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated;
			Factory.Save();
			customsStatusLogs = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code));
			AssertEquals("No new log for Customs Status should have been added because header's Customs status didn't change.", 1, customsStatusLogs.Length);

			header.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.MeasuresRequired;
			Factory.Save();
			customsStatusLogs = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code));
			AssertEquals("A new log for Customs Status should have been added on Customs status change.", 2, customsStatusLogs.Length);

			header.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated;
			header.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.MeasuresRequired;
			Factory.Save();
			customsStatusLogs = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code));
			AssertEquals("No new log for Customs Status should have been added because it didn't change from previous saved value.", 2, customsStatusLogs.Length);

			header.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.TSDInvalidated;
			header.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.MeasuresRequired;
			header.CustomsStatus = UniversalReferenceConstants.PNTS.CustomsStatus.IntendedControl;
			Factory.Save();
			customsStatusLogs = header.Logs.Find(new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.CustomsEntryStatus.Code));
			AssertEquals("Only Customs Status change upon saving should be logged.", 3, customsStatusLogs.Length);
		}

		public void TestLoadOrCreateCusAuthorizationUsage()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			AssertNull("No CusAuthorizationUsage has been created yet.", header.AuthorizationUsage);

			var authorisationUsage = header.AuthorizationUsageOrNew;
			CombineAssertions("An authorisation usage should have been created:", () =>
			{
				AssertNotNull("CusAuthorizationUsage should have been created.", authorisationUsage);
				AssertEquals("The new authorisation usage should have been loaded.", authorisationUsage.PK, header.AuthorizationUsage.PK);
				AssertEquals("No new authorisation usage should have been created when invoking LoadOrCreateCusAuthorizationUsage again.", authorisationUsage.PK, header.AuthorizationUsageOrNew.PK);
			});
		}

		public void TestAuthorizationPropertiesChangeWithGoodsLocationType()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var authorisationUsage = header.AuthorizationUsageOrNew;
			AssertEquals("AuthorizationType should be empty as AGC_Code is empty", ZString.Empty, header.AuthorizationType);

			header.GoodsLocation.CGL_Type = "B";

			AssertEquals("AuthorizationType should have been change to TST", AuthorizationTypeList.Codes.TST, header.AuthorizationType);

			header.AuthorizationOwner = orgHeader.PK;
			header.AuthorizationNumber = "123";

			header.GoodsLocation.CGL_Type = "C";
			AssertEquals("AuthorizationType should have been emptied", ZString.Empty, header.AuthorizationType);
			AssertEquals("AuthorizationType should have been emptied", ZGuid.Empty, header.AuthorizationOwner);
			AssertEquals("AuthorizationType should have been emptied", ZString.Empty, header.AuthorizationNumber);
		}

		public void TestAuthorizationUsageIsNotInDB()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var authorisationUsage = header.AuthorizationUsageOrNew;

			Assert("AuthorisationUsage should not be in DB", !authorisationUsage.IsInDatabase);

			authorisationUsage.FillWithValidTestData();
			Factory.Save();
			Assert("AuthorisationUsage should be in DB", authorisationUsage.IsInDatabase);
		}

		public void TestAuthorizationType()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var authorisationUsage = header.AuthorizationUsage;
			AssertNull("Prerequisite : AuthorisationUsage should be null", authorisationUsage);
			AssertEquals("AuthorizationType should be empty as authorisationUsage is null", ZString.Empty, header.AuthorizationType);

			authorisationUsage = header.AuthorizationUsageOrNew;
			AssertEquals("AuthorizationType should be empty as AGC_Code is empty", ZString.Empty, header.AuthorizationType);

			AssertNotNull("AuthorisationUsage is not null", header.AuthorizationUsage);
			authorisationUsage.AGC_Code = "Code";
			header.AuthorizationType = ZString.Empty;
			AssertNull("AuthorisationUsage is null as AuthorizationType is now empty.", header.AuthorizationUsage);

			authorisationUsage = header.AuthorizationUsageOrNew;
			authorisationUsage.AGC_Code = "Code";
			AssertEquals("AuthorizationType should be equal to AGC_Code", "Code", header.AuthorizationType);

			header.AuthorizationNumber = "Cod";
			header.AuthorizationOwner = orgHeader.PK;

			header.AuthorizationType = ZString.Empty;
			Assert("Number and owner should be empty", header.AuthorizationOwner.IsEmpty && header.AuthorizationNumber.IsEmpty);
		}

		public void TestAuthorisationNumberIsSetEmpty()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var authorisationUsage = header.AuthorizationUsageOrNew;
			header.AuthorizationOwner = orgHeader.PK;
			header.AuthorizationNumber = "Cod";

			authorisationUsage.AGC_Code = "Code";
			Assert("Number and owner should not be empty.", !header.AuthorizationOwner.IsEmpty && !header.AuthorizationNumber.IsEmpty);

			header.AuthorizationType = ZString.Empty;
			Assert("Number and owner should be empty as AuthorizationType is now Empty.", header.AuthorizationOwner.IsEmpty && header.AuthorizationNumber.IsEmpty);
		}

		public void TestAuthorizationOwner()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var authorisationUsage = header.AuthorizationUsage;
			AssertNull("Prerequisite: authorisationUsage should be null", authorisationUsage);
			AssertEquals("AuthorizationOwner should be empty as authorisationUsage is null", ZGuid.Empty, header.AuthorizationOwner);

			authorisationUsage = header.AuthorizationUsageOrNew;
			AssertEquals("AuthorizationOwner should be empty as AGC_OH_Owner is empty", ZGuid.Empty, header.AuthorizationOwner);

			authorisationUsage.AGC_OH_Owner = orgHeader.PK;
			AssertEquals("AuthorizationOwner should be equal to AGC_OH_Owner", orgHeader.PK, header.AuthorizationOwner);
		}

		public void TestAuthorizationOwnerReadOnly()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var authorisationUsage = header.AuthorizationUsageOrNew;
			authorisationUsage.AGC_Code = "TST";
			AssertEquals("AuthorizationOwner should not be read only as AuthorizationType is not empty", false, header.AuthorizationOwnerInfo.ReadOnly);

			authorisationUsage.AGC_Code = ZString.Empty;
			AssertEquals("AuthorizationOwner should be read only as AuthorizationType is empty", true, header.AuthorizationOwnerInfo.ReadOnly);
		}

		public void TestAuthorizationNumber()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var authorisationUsage = header.AuthorizationUsage;
			AssertNull("Prerequisite : authorisationUsage should be null", authorisationUsage);
			AssertEquals("AuthorizationNumber should be empty as authorisationUsage is null", ZString.Empty, header.AuthorizationNumber);

			authorisationUsage = header.AuthorizationUsageOrNew;
			AssertEquals("AuthorizationNumber should be empty as AGC_Number is empty", ZString.Empty, header.AuthorizationNumber);

			authorisationUsage.AGC_Number = "num";
			AssertEquals("AuthorizationNumber should be equal to AGC_Number", "num", header.AuthorizationNumber);
		}

		public void TestAuthorizationNumberReadOnly()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();

			var authorisationUsage = header.AuthorizationUsageOrNew;
			authorisationUsage.AGC_Code = "TST";
			AssertEquals("AuthorizationNumber should not be read only as AuthorizationType is not empty", false, header.AuthorizationNumberInfo.ReadOnly);

			authorisationUsage.AGC_Code = ZString.Empty;
			AssertEquals("AuthorizationNumber should be read only as AuthorizationType type is empty", true, header.AuthorizationNumberInfo.ReadOnly);
		}

		public void TestCreateNewTemporaryStorageContainerCollection()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			temporaryStorageHeader.Containers.AddNew();
			AssertType<AsycudaContainerCollection<TemporaryStorageContainer, TemporaryStorageHeader>>(temporaryStorageHeader.Containers);
		}

		public void TestPreviousDocument()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var document = temporaryStorageHeader.PreviousDocuments.AddNew();
			AssertType<TemporaryStoragePreviousDocumentCollection<TemporaryStoragePreviousDocument>>(temporaryStorageHeader.PreviousDocuments);
		}

		public void TestPackedItemRelationship()
		{
			var temporaryStorageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			AssertEquals(AsycudaPackPackedItemPivotCollection.RelationshipType.Many, temporaryStorageHeader.PackedItemRelationship);
		}

		public void TestLayoutProviderKey()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_RN_NKCountry = "EU";
			AssertEquals("Get common code", "EU", temporaryStorageHeader.LayoutProviderKey);
			temporaryStorageHeader.AMA_RN_NKCountry = "PR";
			AssertEquals("Get jurisdiction", "US", temporaryStorageHeader.LayoutProviderKey);
		}

		public void TestIsTransfer()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();

			CombineAssertions(() =>
			{
				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				AssertEquals("MessageType = 'DC', is not Transfer", false, temporaryStorageHeader.IsTransfer);

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
				AssertEquals("MessageType = 'TF', is Transfer'", true, temporaryStorageHeader.IsTransfer);
			});
		}

		public void TestIsDeconsolidation()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();

			CombineAssertions(() =>
			{
				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;
				AssertEquals("MessageType = 'TC', is not Deconsolidation", false, temporaryStorageHeader.IsDeconsolidation);

				temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				AssertEquals("MessageType = 'DC', is Deconsolidation'", true, temporaryStorageHeader.IsDeconsolidation);
			});
		}

		public void TestClearUnusedValues_Transfer()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;

			var shipper = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var notifyParty = Factory.New<OrgHeader>();
			var carrier = Factory.New<OrgHeader>();
			temporaryStorageHeader.ENSReuse = 1;
			temporaryStorageHeader.AMA_TransportMode = "10";
			temporaryStorageHeader.TransportType = "10";
			temporaryStorageHeader.ArrivalTransportMeansCode = "10";
			temporaryStorageHeader.PresentationCustomsOffice = "BE010001";
			temporaryStorageHeader.AMA_DateAtCustomsOffice = ZDateTime.Now;
			temporaryStorageHeader.EstimatedDateOfArrival = ZDateTime.Now;
			temporaryStorageHeader.AMA_OA_Carrier = carrier.MainAddress.PK;
			temporaryStorageHeader.PlaceOfUnloading = "BEANR";
			var container = temporaryStorageHeader.Containers.AddNew();
			container.ACN_ContainerNumber = "MSCU1234566";
			var bill = temporaryStorageHeader.Bills.AddNew();
			bill.ABL_UCRNumber = "UCR-nr";
			bill.ABL_OA_Shipper = shipper.MainAddress.PK;
			bill.ConsigneeOrgPK = consignee.PK;
			bill.ConsignorOrgPK = shipper.PK;
			bill.NotifyPartyOrgPK = notifyParty.PK;
			bill.ABL_ShipperName = "Shipper Name";
			bill.ABL_ShipperStreet1 = "Shipper Street 1";
			bill.ABL_ShipperStreet2 = "Shipper Street 2";
			bill.ABL_ShipperCity = "Shipper City";
			bill.ABL_RN_NKShipperCountry = "BE";
			bill.ABL_ShipperState = "Shipper State";
			bill.ABL_ShipperPostcode = "1234";
			bill.ABL_ShipperPhone = "Shipper Phone";
			bill.ABL_ShipperRegNoType = "1";
			bill.ABL_ShipperRegNo = "Shipper RegNo";
			bill.ABL_OA_Consignee = consignee.PK;
			bill.ABL_ConsigneeName = "Consignee Name";
			bill.ABL_ConsigneeStreet1 = "Consignee Street 1";
			bill.ABL_ConsigneeStreet2 = "Consignee Street 2";
			bill.ABL_ConsigneeCity = "Consignee City";
			bill.ABL_RN_NKConsigneeCountry = "FR";
			bill.ABL_ConsigneeState = "Consignee State";
			bill.ABL_ConsigneePostcode = "5678";
			bill.ABL_ConsigneePhone = "Consignee Phone";
			bill.ABL_ConsigneeRegNoType = "2";
			bill.ABL_ConsigneeRegNo = "Consignee RegNo";
			bill.ABL_OA_NotifyParty = notifyParty.MainAddress.PK;
			bill.ABL_NotifyPartyName = "Notify Name";
			bill.ABL_NotifyPartyStreet1 = "Notify Street 1";
			bill.ABL_NotifyPartyStreet2 = "Notify Street 2";
			bill.ABL_NotifyPartyCity = "Notify City";
			bill.ABL_RN_NKNotifyPartyCountry = "DE";
			bill.ABL_NotifyPartyState = "Notify State";
			bill.ABL_NotifyPartyPostcode = "9876";
			bill.ABL_NotifyPartyPhone = "Notify Phone";
			bill.ABL_NotifyPartyRegNoType = "3";
			bill.ABL_NotifyPartyRegNo = "Notify RegNo";
			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_GoodsDescription = "Test";
			var supportingDocument = bill.SupportingDocuments.AddNew();
			supportingDocument.CSI_ReferenceNumber = "Sup Doc Ref";
			var previousDocument = bill.PreviousDocuments.AddNew();
			previousDocument.CSI_ReferenceNumber = "Prev Doc Ref";
			var additionalInfo = bill.AdditionalInfos.AddNew();
			additionalInfo.CSI_ReferenceNumber = "Add Info Ref";
			var supplyChainActor = bill.SupplyChainActors.AddNew();
			supplyChainActor.CFR_Reference = "Supply Chain Actor Reference";
			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "Test";
			pack.ContainerPK = container.PK;
			pack.APA_MarksAndNumbers = "Pack Marks And Numbers";

			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Transfer;
			CombineAssertions("Value should be cleared when AMA_MessageType = 'TF'", () =>
			{
				AssertEquals("ENSReuse", ZByte.Zero, temporaryStorageHeader.ENSReuse);
				AssertNullOrEmpty("AMA_TransportMode", temporaryStorageHeader.AMA_TransportMode);
				AssertNullOrEmpty("TransportType", temporaryStorageHeader.AMA_TransportMode);
				AssertNullOrEmpty("ArrivalTransportMeansCode", temporaryStorageHeader.ArrivalTransportMeansCode);
				AssertNullOrEmpty("PresentationCustomsOffice", temporaryStorageHeader.PresentationCustomsOffice);
				AssertEquals("AMA_DateAtCustomsOffice", ZDateTime.Empty, temporaryStorageHeader.AMA_DateAtCustomsOffice);
				AssertEquals("EstimatedDateOfArrival", ZDateTime.Empty, temporaryStorageHeader.EstimatedDateOfArrival);
				AssertEquals("AMA_OA_Carrier", ZGuid.Empty, temporaryStorageHeader.AMA_OA_Carrier);
				AssertNullOrEmpty("PlaceOfUnloading", temporaryStorageHeader.PlaceOfUnloading);

				Assert("Containers", temporaryStorageHeader.Containers.IsNullOrEmpty());

				AssertNullOrEmpty("Bill UCR Number", bill.ABL_UCRNumber);
				AssertEquals("Bill ABL_OA_Shipper", ZGuid.Empty, bill.ABL_OA_Shipper);
				AssertEquals("Bill ConsigneeOrgPk", ZGuid.Empty, bill.ConsigneeOrgPK);
				AssertEquals("Bill ConsignorOrgPk", ZGuid.Empty, bill.ConsignorOrgPK);
				AssertEquals("Bill NotifyPartyOrgPk", ZGuid.Empty, bill.NotifyPartyOrgPK);
				AssertNullOrEmpty("Bill ABL_ShipperName", bill.ABL_ShipperName);
				AssertNullOrEmpty("Bill ABL_ShipperStreet1", bill.ABL_ShipperStreet1);
				AssertNullOrEmpty("Bill ABL_ShipperStreet2", bill.ABL_ShipperStreet2);
				AssertNullOrEmpty("Bill ABL_ShipperCity", bill.ABL_ShipperCity);
				AssertNullOrEmpty("Bill ABL_ShipperPostcode", bill.ABL_ShipperPostcode);
				AssertNullOrEmpty("Bill ABL_RN_NKShippercountry", bill.ABL_RN_NKShipperCountry);
				AssertNullOrEmpty("Bill ABL_ShipperState", bill.ABL_ShipperState);
				AssertNullOrEmpty("Bill ABL_ShipperPhone", bill.ABL_ShipperPhone);
				AssertNullOrEmpty("Bill ABL_ShipperRegNoType", bill.ABL_ShipperRegNoType);
				AssertNullOrEmpty("Bill ABL_ShipperRegNo", bill.ABL_ShipperRegNo);
				AssertEquals("Bill ABL_OA_Consignee", ZGuid.Empty, bill.ABL_OA_Consignee);
				AssertNullOrEmpty("Bill ABL_ConsigneeName", bill.ABL_ConsigneeName);
				AssertNullOrEmpty("Bill ABL_ConsigneeStreet1", bill.ABL_ConsigneeStreet1);
				AssertNullOrEmpty("Bill ABL_ConsigneeStreet2", bill.ABL_ConsigneeStreet2);
				AssertNullOrEmpty("Bill ABL_ConsigneeCity", bill.ABL_ConsigneeCity);
				AssertNullOrEmpty("Bill ABL_ConsigneePostcode", bill.ABL_ConsigneePostcode);
				AssertNullOrEmpty("Bill ABL_RN_NKConsigneecountry", bill.ABL_RN_NKConsigneeCountry);
				AssertNullOrEmpty("Bill ABL_ConsigneeState", bill.ABL_ConsigneeState);
				AssertNullOrEmpty("Bill ABL_ConsigneePhone", bill.ABL_ConsigneePhone);
				AssertNullOrEmpty("Bill ABL_ConsigneeRegNoType", bill.ABL_ConsigneeRegNoType);
				AssertNullOrEmpty("Bill ABL_ConsigneeRegNo", bill.ABL_ConsigneeRegNo);
				AssertEquals("Bill ABL_OA_NotifyParty", ZGuid.Empty, bill.ABL_OA_Consignee);
				AssertNullOrEmpty("Bill ABL_NotifyPartyName", bill.ABL_NotifyPartyName);
				AssertNullOrEmpty("Bill ABL_NotifyPartyStreet1", bill.ABL_NotifyPartyStreet1);
				AssertNullOrEmpty("Bill ABL_NotifyPartyStreet2", bill.ABL_NotifyPartyStreet2);
				AssertNullOrEmpty("Bill ABL_NotifyPartyCity", bill.ABL_NotifyPartyCity);
				AssertNullOrEmpty("Bill ABL_NotifyPartyPostcode", bill.ABL_NotifyPartyPostcode);
				AssertNullOrEmpty("Bill ABL_RN_NKNotifyPartycountry", bill.ABL_RN_NKNotifyPartyCountry);
				AssertNullOrEmpty("Bill ABL_NotifyPartyState", bill.ABL_NotifyPartyState);
				AssertNullOrEmpty("Bill ABL_NotifyPartyPhone", bill.ABL_NotifyPartyPhone);
				AssertNullOrEmpty("Bill ABL_NotifyPartyRegNoType", bill.ABL_NotifyPartyRegNoType);
				AssertNullOrEmpty("Bill ABL_NotifyPartyRegNo", bill.ABL_NotifyPartyRegNo);

				Assert("Bill PackedItems", bill.PackedItems.IsNullOrEmpty());
				Assert("Bill SupportingDocuments", bill.SupportingDocuments.IsNullOrEmpty());
				Assert("Bill PreviousDocuments", bill.PreviousDocuments.IsNullOrEmpty());
				Assert("Bill AdditionalInfos", bill.AdditionalInfos.IsNullOrEmpty());
				Assert("Bill SupplyChainActors", bill.SupplyChainActors.IsNullOrEmpty());

				AssertEquals("Bill Packs", 1, bill.Packs.Count);
				AssertEquals("Bill Pack ContainerPK", ZGuid.Empty, pack.ContainerPK);
				AssertNullOrEmpty("Bill Pack APA_MarkAndNumbers", pack.APA_MarksAndNumbers);
			});
		}

		public void TestClearUnusedValues_Deconsolidation()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.CombinedTemporaryStorage;

			var shipper = Factory.New<OrgHeader>();
			var consignee = Factory.New<OrgHeader>();
			var notifyParty = Factory.New<OrgHeader>();
			var carrier = Factory.New<OrgHeader>();
			temporaryStorageHeader.ENSReuse = 1;
			temporaryStorageHeader.AMA_TransportMode = "10";
			temporaryStorageHeader.TransportType = "10";
			temporaryStorageHeader.ArrivalTransportMeansCode = "10";
			temporaryStorageHeader.ArrivalTransportMeans.TPM_IdentificationNumber = "ROA";
			temporaryStorageHeader.PresentationCustomsOffice = "BE010001";
			temporaryStorageHeader.AMA_DateAtCustomsOffice = ZDateTime.Now;
			temporaryStorageHeader.EstimatedDateOfArrival = ZDateTime.Now;
			temporaryStorageHeader.AMA_OA_Carrier = carrier.MainAddress.PK;
			temporaryStorageHeader.PlaceOfUnloading = "BEANR";
			var container = temporaryStorageHeader.Containers.AddNew();
			container.ACN_ContainerNumber = "MSCU1234566";
			var masterBill = temporaryStorageHeader.Bills.AddNew();
			masterBill.ABL_BolType = TemporaryStorageBill.ChildBolCode;
			masterBill.ABL_UCRNumber = "UCR-nr";
			masterBill.ABL_OA_Shipper = shipper.MainAddress.PK;
			masterBill.ConsigneeOrgPK = consignee.PK;
			masterBill.ConsignorOrgPK = shipper.PK;
			masterBill.NotifyPartyOrgPK = notifyParty.PK;
			masterBill.ABL_ShipperName = "Shipper Name";
			masterBill.ABL_ShipperStreet1 = "Shipper Street 1";
			masterBill.ABL_ShipperStreet2 = "Shipper Street 2";
			masterBill.ABL_ShipperCity = "Shipper City";
			masterBill.ABL_RN_NKShipperCountry = "BE";
			masterBill.ABL_ShipperState = "Shipper State";
			masterBill.ABL_ShipperPostcode = "1234";
			masterBill.ABL_ShipperPhone = "Shipper Phone";
			masterBill.ABL_ShipperRegNoType = "1";
			masterBill.ABL_ShipperRegNo = "Shipper RegNo";
			masterBill.ABL_OA_Consignee = consignee.PK;
			masterBill.ABL_ConsigneeName = "Consignee Name";
			masterBill.ABL_ConsigneeStreet1 = "Consignee Street 1";
			masterBill.ABL_ConsigneeStreet2 = "Consignee Street 2";
			masterBill.ABL_ConsigneeCity = "Consignee City";
			masterBill.ABL_RN_NKConsigneeCountry = "FR";
			masterBill.ABL_ConsigneeState = "Consignee State";
			masterBill.ABL_ConsigneePostcode = "5678";
			masterBill.ABL_ConsigneePhone = "Consignee Phone";
			masterBill.ABL_ConsigneeRegNoType = "2";
			masterBill.ABL_ConsigneeRegNo = "Consignee RegNo";
			masterBill.ABL_OA_NotifyParty = notifyParty.MainAddress.PK;
			masterBill.ABL_NotifyPartyName = "Notify Name";
			masterBill.ABL_NotifyPartyStreet1 = "Notify Street 1";
			masterBill.ABL_NotifyPartyStreet2 = "Notify Street 2";
			masterBill.ABL_NotifyPartyCity = "Notify City";
			masterBill.ABL_RN_NKNotifyPartyCountry = "DE";
			masterBill.ABL_NotifyPartyState = "Notify State";
			masterBill.ABL_NotifyPartyPostcode = "9876";
			masterBill.ABL_NotifyPartyPhone = "Notify Phone";
			masterBill.ABL_NotifyPartyRegNoType = "3";
			masterBill.ABL_NotifyPartyRegNo = "Notify RegNo";
			var packedItem = masterBill.PackedItems.AddNew();
			packedItem.API_GoodsDescription = "Test";
			var supportingDocument = masterBill.SupportingDocuments.AddNew();
			supportingDocument.CSI_ReferenceNumber = "Sup Doc Ref";
			var previousDocument = masterBill.PreviousDocuments.AddNew();
			previousDocument.CSI_ReferenceNumber = "Prev Doc Ref";
			var additionalInfo = masterBill.AdditionalInfos.AddNew();
			additionalInfo.CSI_ReferenceNumber = "Add Info Ref";
			var supplyChainActor = masterBill.SupplyChainActors.AddNew();
			supplyChainActor.CFR_Reference = "Supply Chain Actor Reference";
			var pack = masterBill.Packs.AddNew();
			pack.APA_GoodsDescription = "Test";
			pack.ContainerPK = container.PK;
			pack.APA_MarksAndNumbers = "Pack Marks And Numbers";

			var bill = temporaryStorageHeader.Bills.AddNew();
			bill.ABL_UCRNumber = "UCR-nr";
			bill.ABL_OA_Shipper = shipper.MainAddress.PK;
			bill.ConsigneeOrgPK = consignee.PK;
			bill.ConsignorOrgPK = shipper.PK;
			bill.NotifyPartyOrgPK = notifyParty.PK;
			bill.ABL_ShipperName = "Shipper Name";
			bill.ABL_ShipperStreet1 = "Shipper Street 1";
			bill.ABL_ShipperStreet2 = "Shipper Street 2";
			bill.ABL_ShipperCity = "Shipper City";
			bill.ABL_RN_NKShipperCountry = "BE";
			bill.ABL_ShipperState = "Shipper State";
			bill.ABL_ShipperPostcode = "1234";
			bill.ABL_ShipperPhone = "Shipper Phone";
			bill.ABL_ShipperRegNoType = "1";
			bill.ABL_ShipperRegNo = "Shipper RegNo";
			bill.ABL_OA_Consignee = consignee.PK;
			bill.ABL_ConsigneeName = "Consignee Name";
			bill.ABL_ConsigneeStreet1 = "Consignee Street 1";
			bill.ABL_ConsigneeStreet2 = "Consignee Street 2";
			bill.ABL_ConsigneeCity = "Consignee City";
			bill.ABL_RN_NKConsigneeCountry = "FR";
			bill.ABL_ConsigneeState = "Consignee State";
			bill.ABL_ConsigneePostcode = "5678";
			bill.ABL_ConsigneePhone = "Consignee Phone";
			bill.ABL_ConsigneeRegNoType = "2";
			bill.ABL_ConsigneeRegNo = "Consignee RegNo";
			bill.ABL_OA_NotifyParty = notifyParty.MainAddress.PK;
			bill.ABL_NotifyPartyName = "Notify Name";
			bill.ABL_NotifyPartyStreet1 = "Notify Street 1";
			bill.ABL_NotifyPartyStreet2 = "Notify Street 2";
			bill.ABL_NotifyPartyCity = "Notify City";
			bill.ABL_RN_NKNotifyPartyCountry = "DE";
			bill.ABL_NotifyPartyState = "Notify State";
			bill.ABL_NotifyPartyPostcode = "9876";
			bill.ABL_NotifyPartyPhone = "Notify Phone";
			bill.ABL_NotifyPartyRegNoType = "3";
			bill.ABL_NotifyPartyRegNo = "Notify RegNo";
			var packedItemHouseBill = bill.PackedItems.AddNew();
			packedItem.API_GoodsDescription = "Test";
			var supportingDocumentHouseBill = bill.SupportingDocuments.AddNew();
			supportingDocument.CSI_ReferenceNumber = "Sup Doc Ref";
			var previousDocumentHouseBill = bill.PreviousDocuments.AddNew();
			previousDocument.CSI_ReferenceNumber = "Prev Doc Ref";
			var additionalInfoHouseBill = bill.AdditionalInfos.AddNew();
			additionalInfo.CSI_ReferenceNumber = "Add Info Ref";
			var supplyChainActorHouseBill = bill.SupplyChainActors.AddNew();
			supplyChainActor.CFR_Reference = "Supply Chain Actor Reference";
			var packHouseBill = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "Test";
			pack.ContainerPK = container.PK;
			pack.APA_MarksAndNumbers = "Pack Marks And Numbers";

			temporaryStorageHeader.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
			CombineAssertions("Value should be cleared when AMA_MessageType = 'DC'", () =>
			{
				AssertEquals("ENSReuse", ZByte.Zero, temporaryStorageHeader.ENSReuse);
				AssertNullOrEmpty("AMA_TransportMode", temporaryStorageHeader.AMA_TransportMode);
				AssertNullOrEmpty("TransportType", temporaryStorageHeader.ArrivalTransportMeans.TPM_IdentificationNumber);
				AssertNullOrEmpty("ArrivalTransportMeansCode", temporaryStorageHeader.ArrivalTransportMeansCode);
				AssertNullOrEmpty("PresentationCustomsOffice", temporaryStorageHeader.PresentationCustomsOffice);
				AssertEquals("AMA_DateAtCustomsOffice", ZDateTime.Empty, temporaryStorageHeader.AMA_DateAtCustomsOffice);
				AssertEquals("EstimatedDateOfArrival", ZDateTime.Empty, temporaryStorageHeader.EstimatedDateOfArrival);
				AssertEquals("AMA_OA_Presenter", ZGuid.Empty, temporaryStorageHeader.AMA_OA_Presenter);
				AssertEquals("LocationOfGoodsDescription", ZString.Empty, temporaryStorageHeader.GoodsLocationDescription);
				AssertEquals("LocationOfGoods - Type", ZString.Empty, temporaryStorageHeader.GoodsLocation.CGL_Type);
				AssertEquals("LocationOfGoods - Qualifier", ZString.Empty, temporaryStorageHeader.GoodsLocation.CGL_Qualifier);
				AssertEquals("LocationOfGoods - GovRegNum", ZString.Empty, temporaryStorageHeader.GoodsLocation.Address.E2_GovRegNum);
				AssertEquals("LocationOfGoods - Address1", ZString.Empty, temporaryStorageHeader.GoodsLocation.Address.E2_Address1);
				AssertEquals("LocationOfGoods - Address2", ZString.Empty, temporaryStorageHeader.GoodsLocation.Address.E2_Address2);
				AssertEquals("LocationOfGoods - Postcode", ZString.Empty, temporaryStorageHeader.GoodsLocation.Address.E2_Postcode);
				AssertEquals("LocationOfGoods - City", ZString.Empty, temporaryStorageHeader.GoodsLocation.Address.E2_City);
				AssertEquals("LocationOfGoods - Country Code", ZString.Empty, temporaryStorageHeader.GoodsLocation.Address.E2_RN_NKCountryCode);
				AssertEquals("LocationOfGoods - Contact", ZString.Empty, temporaryStorageHeader.GoodsLocation.Address.E2_Contact);
				AssertEquals("LocationOfGoods - Phone", ZString.Empty, temporaryStorageHeader.GoodsLocation.Address.E2_Phone);
				AssertEquals("LocationOfGoods - E-mail", ZString.Empty, temporaryStorageHeader.GoodsLocation.Address.E2_Email);
				AssertEquals("LocationOfGoods - Longitude", ZDecimal.Zero, temporaryStorageHeader.GoodsLocation.Address.E2_Longitude);
				AssertEquals("LocationOfGoods - Latitude", ZDecimal.Zero, temporaryStorageHeader.GoodsLocation.Address.E2_Latitude);
				AssertNull("AuthorizationUsage - Type/Number/Owner", temporaryStorageHeader.AuthorizationUsage);
				AssertEquals("AMA_OA_Carrier", ZGuid.Empty, temporaryStorageHeader.AMA_OA_Carrier);
				AssertNullOrEmpty("PlaceOfUnloading", temporaryStorageHeader.PlaceOfUnloading);
				AssertEquals("AMA_Calc_HasHouseConsignments", ZBool.True, temporaryStorageHeader.AMA_Calc_HasHouseConsignment);

				AssertEquals("Containers", 1, temporaryStorageHeader.Containers.Count);

				AssertNullOrEmpty("MasterBill UCR Number", masterBill.ABL_UCRNumber);
				AssertEquals("MasterBill ABL_OA_Shipper", ZGuid.Empty, masterBill.ABL_OA_Shipper);
				AssertEquals("MasterBill ConsigneeOrgPk", ZGuid.Empty, masterBill.ConsigneeOrgPK);
				AssertEquals("MasterBill ConsignorOrgPk", ZGuid.Empty, masterBill.ConsignorOrgPK);
				AssertEquals("MasterBill NotifyPartyOrgPk", ZGuid.Empty, masterBill.NotifyPartyOrgPK);
				AssertEquals("MasterBill GrossWeight", ZDecimal.Zero, masterBill.ABL_GrossWeight);
				AssertEquals("MasterBill GrossWeightUQ", ZString.Empty, masterBill.ABL_GrossWeightUQ);
				AssertNullOrEmpty("MasterBill ABL_ShipperName", masterBill.ABL_ShipperName);
				AssertNullOrEmpty("MasterBill ABL_ShipperStreet1", masterBill.ABL_ShipperStreet1);
				AssertNullOrEmpty("MasterBill ABL_ShipperStreet2", masterBill.ABL_ShipperStreet2);
				AssertNullOrEmpty("MasterBill ABL_ShipperCity", masterBill.ABL_ShipperCity);
				AssertNullOrEmpty("MasterBill ABL_ShipperPostcode", masterBill.ABL_ShipperPostcode);
				AssertNullOrEmpty("MasterBill ABL_RN_NKShippercountry", masterBill.ABL_RN_NKShipperCountry);
				AssertNullOrEmpty("MasterBill ABL_ShipperState", masterBill.ABL_ShipperState);
				AssertNullOrEmpty("MasterBill ABL_ShipperPhone", masterBill.ABL_ShipperPhone);
				AssertNullOrEmpty("MasterBill ABL_ShipperRegNoType", masterBill.ABL_ShipperRegNoType);
				AssertNullOrEmpty("MasterBill ABL_ShipperRegNo", masterBill.ABL_ShipperRegNo);
				AssertEquals("MasterBill ABL_OA_Consignee", ZGuid.Empty, masterBill.ABL_OA_Consignee);
				AssertNullOrEmpty("MasterBill ABL_ConsigneeName", masterBill.ABL_ConsigneeName);
				AssertNullOrEmpty("MasterBill ABL_ConsigneeStreet1", masterBill.ABL_ConsigneeStreet1);
				AssertNullOrEmpty("MasterBill ABL_ConsigneeStreet2", masterBill.ABL_ConsigneeStreet2);
				AssertNullOrEmpty("MasterBill ABL_ConsigneeCity", masterBill.ABL_ConsigneeCity);
				AssertNullOrEmpty("MasterBill ABL_ConsigneePostcode", masterBill.ABL_ConsigneePostcode);
				AssertNullOrEmpty("MasterBill ABL_RN_NKConsigneecountry", masterBill.ABL_RN_NKConsigneeCountry);
				AssertNullOrEmpty("MasterBill ABL_ConsigneeState", masterBill.ABL_ConsigneeState);
				AssertNullOrEmpty("MasterBill ABL_ConsigneePhone", masterBill.ABL_ConsigneePhone);
				AssertNullOrEmpty("MasterBill ABL_ConsigneeRegNoType", masterBill.ABL_ConsigneeRegNoType);
				AssertNullOrEmpty("MasterBill ABL_ConsigneeRegNo", masterBill.ABL_ConsigneeRegNo);
				AssertEquals("MasterBill ABL_OA_NotifyParty", ZGuid.Empty, masterBill.ABL_OA_Consignee);
				AssertNullOrEmpty("MasterBill ABL_NotifyPartyName", masterBill.ABL_NotifyPartyName);
				AssertNullOrEmpty("MasterBill ABL_NotifyPartyStreet1", masterBill.ABL_NotifyPartyStreet1);
				AssertNullOrEmpty("MasterBill ABL_NotifyPartyStreet2", masterBill.ABL_NotifyPartyStreet2);
				AssertNullOrEmpty("MasterBill ABL_NotifyPartyCity", masterBill.ABL_NotifyPartyCity);
				AssertNullOrEmpty("MasterBill ABL_NotifyPartyPostcode", masterBill.ABL_NotifyPartyPostcode);
				AssertNullOrEmpty("MasterBill ABL_RN_NKNotifyPartycountry", masterBill.ABL_RN_NKNotifyPartyCountry);
				AssertNullOrEmpty("MasterBill ABL_NotifyPartyState", masterBill.ABL_NotifyPartyState);
				AssertNullOrEmpty("MasterBill ABL_NotifyPartyPhone", masterBill.ABL_NotifyPartyPhone);
				AssertNullOrEmpty("MasterBill ABL_NotifyPartyRegNoType", masterBill.ABL_NotifyPartyRegNoType);
				AssertNullOrEmpty("MasterBill ABL_NotifyPartyRegNo", masterBill.ABL_NotifyPartyRegNo);

				Assert("MasterBill PackedItems", masterBill.PackedItems.IsNullOrEmpty());
				Assert("MasterBill SupportingDocuments", masterBill.SupportingDocuments.IsNullOrEmpty());
				Assert("MasterBill PreviousDocuments", masterBill.PreviousDocuments.IsNullOrEmpty());
				Assert("MasterBill AdditionalInfos", masterBill.AdditionalInfos.IsNullOrEmpty());
				Assert("MasterBill SupplyChainActors", masterBill.SupplyChainActors.IsNullOrEmpty());
				Assert("MasterBill Packs", masterBill.Packs.IsNullOrEmpty());
			});
		}

		public void TestAMA_Calc_HasHouseConsignment_ReadOnly()
		{
			CombineAssertions(() =>
			{
				var header = Factory.New<TemporaryStorageHeader>();
				AssertEquals("Flag AMA_Calc_HasHouseConsignmnent_ReadOnly", false, header.AMA_Calc_HasHouseConsignment_ReadOnly);
				header.AMA_MessageType = PNTSMessageTypeList.Codes.Deconsolidation;
				AssertEquals("Flag AMA_Calc_HasHouseConsignmnent_ReadOnly", true, header.AMA_Calc_HasHouseConsignment_ReadOnly);
			});
		}

		public void TestGuarantee()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var guarantee = header.Guarantee;
			guarantee.PW_BondNumber = "ref";
			guarantee.PW_Override = true;
			guarantee.PW_BondAmount = new ZDecimal(10.01);
			Factory.Save();

			guarantee = Factory.Load<TemporaryStorageHeaderGuarantee>(header.Guarantee.PK);

			CombineAssertions("Assert a guarantee is create and the values are well valorized", () =>
			{
				AssertEquals("guarantee.PW_BondNumber", guarantee.PW_BondNumber, "ref");
				AssertEquals("guarantee.PW_BondAmount", guarantee.PW_BondAmount, new ZDecimal(10.01));
				AssertEquals("guarantee.PW_RX_NKCurrency", guarantee.PW_RX_NKCurrency, "EUR");
				AssertEquals("guarantee.PW_Override", guarantee.PW_Override, true);
				AssertEquals("guarantee.PW_ParentID", guarantee.PW_ParentID, header.PK);
				AssertEquals("guarantee.PW_BondType", guarantee.PW_BondType, EUGuaranteeTypeList.Codes.COD);
			});
		}

		public void TestGuaranteeBondType()
		{
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			AssertEquals("GuaranteeBondType should be equal to COD for EU.", EUGuaranteeTypeList.Codes.COD, header.GuaranteeBondType);
		}

		public void TestApportionedAmountToGuaranteeLiabilityAmount()
		{
			TemporaryStorageTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.Latvia);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "19860101";
			guaranteeHeader.CPH_Type = EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;

			var guaranteeLineTransaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransaction.CPL_Reference = "Ref";
			guaranteeLineTransaction.CPL_TransactionType = PermitTransactionTypeList.Codes.OBL;
			guaranteeLineTransaction.CPL_TranValue = 150m;
			guaranteeLineTransaction.CPL_Reference = "XJ5 - 00003877";
			Factory.Save();
			TemporaryStorageTestHelper.SetUpTariff(Factory);

			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			var header = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			header.AMA_GB = branch.PK;
			var bill = header.Bills.AddNew();
			var guarantee = header.Guarantee;
			var goodsItem = Factory.New<TemporaryStoragePackedItemForTest>();
			bill.PackedItems.Add(goodsItem);
			goodsItem.CalculateAndRefreshAllDutyAmountFromTariffRatesForTesting = true;

			var guarantee1 = header.Guarantee;
			guarantee.PW_Override = true;
			guarantee.PW_BondAmount = 10m;
			guarantee.PW_BondNumber = "19860101";

			CombineAssertions(() =>
			{
				AssertEquals("[PRE-CONDITION] LiabilityAmount before setting needed data to calculate it", 0m, goodsItem.LiabilityAmount);

				Factory.Save();
				AssertEquals("When Override is true, PW_BondAmount has the value set by user", 10m, guarantee.PW_BondAmount);

				guarantee.PW_Override = false;
				AssertEquals("When Override is false, PW_BondAmount is set to empty if there is no LiabilityAmount, before saving", 0m, guarantee.PW_BondAmount);
				Factory.Save();
				AssertEquals("When Override is false, PW_BondAmount is set to empty if there is no LiabilityAmount, after saving", 0m, guarantee.PW_BondAmount);

				goodsItem.API_Tariff = TemporaryStorageTestHelper.TestTariffCode;
				goodsItem.API_GoodsValue = 1_000m;
				goodsItem.API_RN_NKGoodsOrigin = "EU";
				AssertEquals("[PRE-CONDITION] LiabilityAmount after setting needed data to calculate it", 120m, goodsItem.LiabilityAmount);

				AssertEquals("When Override is false, PW_BondAmount is changed to LiabilityAmount (when changed) before saving because it is updated when values are changed", 120m, guarantee.PW_BondAmount);
				Factory.Save();
				AssertEquals("When Override is false, PW_BondAmount is set to LiabilityAmount (when changed) after saving", 120m, guarantee.PW_BondAmount);

				goodsItem.API_GoodsValue = 1_200m;
				AssertEquals("[PRE-CONDITION] LiabilityAmount after changing monetary value", 144m, goodsItem.LiabilityAmount);

				AssertEquals("When Override is false, PW_BondAmount is not changed to LiabilityAmount (when changed a second time) before saving because it is updated when values are changed", 144m, guarantee.PW_BondAmount);
				Factory.Save();
				AssertEquals("When Override is false, PW_BondAmount is set to LiabilityAmount (when changed a second time) after saving", 144m, guarantee.PW_BondAmount);
			});
		}

		public void TestValidationDecider()
		{
			var temporaryStorageHeader = Factory.New<TemporaryStorageHeader>();
			AssertType<TemporaryStorageHeaderValidationDecider>("ValidationDecider Type", temporaryStorageHeader.ValidationDecider);
		}

		public void TestGetC0009CountryCodes()
		{
			TemporaryStorageTestHelper.SetupC0009ForCountries(Factory, Core.Constants.CountryCodes.Spain, Core.Constants.CountryCodes.France);

			var header = Factory.New<TemporaryStorageHeader>();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunZZZ = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Latvia, parent: eunZZZ);
			helper.CreateNewOrGetExistingCusCodeType(EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0063, "C0063 Desc");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_C0063, Core.Constants.CountryCodes.Germany, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { Core.Constants.CountryCodes.Spain, Core.Constants.CountryCodes.France }, header.GetC0009CountryCodes());
		}

		public void TestDefaultDataGroupingCode()
		{
			var header = Factory.New<TemporaryStorageHeader>();

			var company = Factory.New<GlbCompany>();
			company.GC_Code = "XXX";
			company.GC_Name = "COMP TEST";
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var branch = company.Branches.AddNew();
			branch.GB_Code = "YYY";
			branch.GB_BranchName = "BRANCH TEST";

			CombineAssertions(() =>
			{
				AssertEquals("Global Company PreCondition LV", Core.Constants.CountryCodes.Latvia, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				AssertEquals("Header with no company takes country from Global Company", Core.Constants.CountryCodes.Latvia, header.DefaultDataGroupingCode);

				header.AMA_GB = branch.PK;
				AssertEquals("Header with company takes country from Company", Core.Constants.CountryCodes.Germany, header.DefaultDataGroupingCode);
			});
		}

		public void TestGetCusSupportingInfoTypes()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var supportingInfoTypes = header.GetCusSupportingInfoTypes();

			CombineAssertions(() =>
			{
				AssertEquals("SupportingInfoTypes Count", 1, supportingInfoTypes.Count);
				AssertEquals("Expected type for PreviousDocument", typeof(TemporaryStoragePreviousDocument), supportingInfoTypes[Common.EU.CusSupportingInfoTypeList.Codes.PreviousDocument]);
			});
		}

		public void TestIsIDocumentSupportable()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var documentSupportable = header as IDocumentSupportable;

			CombineAssertions(() =>
			{
				AssertNotNull("IDocumentSupportable", documentSupportable);
				AssertEquals("TableName", "AsycudaManifestHeader", documentSupportable.TableName);
				AssertType<TemporaryStorageHeaderDocumentSupporter>("DocumentSupporter", documentSupportable.DocumentSupporter);
			});
		}

		public void TestIsIEDocsProvider()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var eDocsProvider = header as IEDocsProvider;

			CombineAssertions(() =>
			{
				AssertNotNull("IEDocsProvider", eDocsProvider);

				var eDocsProviderSupporter = eDocsProvider.GetEDocsProviderSupporter();

				AssertNotNull("EDocsProviderSupporter", eDocsProviderSupporter);
				AssertType<JobInvoicingEDocsProviderSupporter>("EDocsProviderSupporter type", eDocsProviderSupporter);
			});
		}

		public void TestArrivalTransportMeansChildObjectIsValidated()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			AssertEquals("PRE-CONDITION", false, header.ArrivalTransportMeans.Notifications.Any());
			header.RunPreSaveValidation();
			AssertEquals("POST-CONDITION", true, header.ArrivalTransportMeans.Notifications.Any());
		}

		#region ReserveTemporaryStorageGoods

		public void TestGetEntryLineDataDeclaredToReserveTSGoods()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			var bill1 = header.Bills.AddNew();
			var packedItem1 = bill1.PackedItems.AddNew();
			var packedItem2 = bill1.PackedItems.AddNew();
			var bill2 = header.Bills.AddNew();
			var packedItem3 = bill2.PackedItems.AddNew();
			var packedItem4 = bill2.PackedItems.AddNew();

			CombineAssertions(() =>
			{
				var (listReturned, messageReturned) = header.GetEntryLineDataDeclaredToReserveTSGoods();
				AssertEquals("Method returns empty enumerable when there are no documents in the declaration", 0, listReturned.Count());
				AssertEquals("Method returns empty string when there are no documents in the declaration", ZString.Empty, messageReturned);

				var previousDoc1 = packedItem1.PreviousDocuments.AddNew();
				previousDoc1.CSI_ReferenceNumber = "Reference";
				previousDoc1.CSI_Code = "BBB";

				var previousDoc2 = packedItem2.PreviousDocuments.AddNew();
				previousDoc2.CSI_ReferenceNumber = "Reference2";
				previousDoc2.CSI_Code = "337";

				var previousDoc3 = packedItem3.PreviousDocuments.AddNew();
				previousDoc3.CSI_ReferenceNumber = "Reference3";
				previousDoc3.CSI_Code = "AAA";

				var previousDoc4 = packedItem4.PreviousDocuments.AddNew();
				previousDoc4.CSI_ReferenceNumber = "Reference";
				previousDoc4.CSI_Code = "337";

				(listReturned, messageReturned) = header.GetEntryLineDataDeclaredToReserveTSGoods();
				AssertEquals("Method returns empty enumerable even when there are documents in the declaration", 0, listReturned.Count());
				AssertEquals("Method returns empty string even when there are documents in the declaration", ZString.Empty, messageReturned);
			});
		}

		public void TestTemporaryStorageTransactionInternalReferenceNumber()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.LRN = "AAA";

			CombineAssertions(() =>
			{
				AssertEquals("TemporaryStorageTransactionInternalReferenceNumber is set to empty when departure", ZString.Empty, header.TemporaryStorageTransactionInternalReferenceNumber);
				AssertEquals("TemporaryStorageTransactionInternalReferenceType is set to empty when departure", ZString.Empty, header.TemporaryStorageTransactionInternalReferenceType);
			});
		}

		public void TestTemporaryStorageTransactionCommentPrefix()
		{
			var header = Factory.New<TemporaryStorageHeaderForTest>();
			header.ManuallySet_TemporaryStorageTransactionCommentPrefix = "Prefix";
			AssertEquals("TemporaryStorageTransactionCommentPrefix is set by default", "Prefix", header.TemporaryStorageTransactionCommentPrefix);
		}

		#endregion

		#region ConfirmTemporaryStorageGoodsConsumption

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_TemporaryStorageRegisterNotEnabled()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var (header, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					header.CustomsStatus = "AAA";

					AssertEquals("CustomsStatus is changed to AAA", "AAA", header.CustomsStatus);
					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_EmptyGoodsLocation()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (header, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(locationInHeader: ZString.Empty);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					header.CustomsStatus = "AAA";

					AssertEquals("CustomsStatus is changed to AAA", "AAA", header.CustomsStatus);
					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_LocationNotManagedInPremises()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (header, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(locationInPremises: "9999000005");

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					header.CustomsStatus = "AAA";

					AssertEquals("CustomsStatus is changed to AAA", "AAA", header.CustomsStatus);
					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldntDoAction_FlagFalse()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (header, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldConfirm: false);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					header.CustomsStatus = "AAA";

					AssertEquals("CustomsStatus is changed to AAA", "AAA", header.CustomsStatus);
					AssertEquals("regLineTransaction was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Pending, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusToCancel()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (header, regLineTransaction, _) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction();

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					header.CustomsStatus = "AAA";

					AssertEquals("CustomsStatus is changed to AAA", "AAA", header.CustomsStatus);
					AssertEquals("regLineTransaction was changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction.SRT_TransactionStatus);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_ShouldDoActionWithCustomsStatusToConfirm()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				SetUpRefData();
				Factory.Save();

				var oldComment = "Extra Old Comment";

				var orgHeader = SetUpOrgHeader();
				var header = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);

				var regHeader1 = SetUpTmpRegHeader();

				var regLine1 = regHeader1.CusTempStorageRegLines.AddNew();
				regLine1.SRL_LineNumber = 1;
				regLine1.SRL_PackageType = "NE";
				var regLine2 = regHeader1.CusTempStorageRegLines.AddNew();
				regLine2.SRL_LineNumber = 2;
				regLine2.SRL_PackageType = "VQ";

				var regLineTransaction1 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, 10, 6);
				regLineTransaction1.SRT_Comments = oldComment;
				var regLineTransaction2 = SetUpTransaction(regLine1, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -10, -2);

				var regLineTransaction3 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Deleted, -5, -5);
				var regLineTransaction4 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6);
				var regLineTransaction5 = SetUpTransaction(regLine2, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, 10, 6);

				var regHeader2 = SetUpTmpRegHeader(appCode: "BBB", reference: "reference2");

				var regLine3 = regHeader2.CusTempStorageRegLines.AddNew();
				regLine3.SRL_LineNumber = 1;
				regLine3.SRL_PackageType = "AA";
				var regLine4 = regHeader2.CusTempStorageRegLines.AddNew();
				regLine4.SRL_LineNumber = 3;
				regLine4.SRL_PackageType = "VG";

				var regLineTransaction6 = SetUpTransaction(regLine3, CusTempStorageRegLineTransactionStatusList.Codes.Pending, 5, 6);
				var regLineTransaction7 = SetUpTransaction(regLine4, CusTempStorageRegLineTransactionStatusList.Codes.Pending, -5, -6);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					header.CustomsStatus = "BBB";

					AssertEquals("CustomsStatus is changed to BBB", "BBB", header.CustomsStatus);

					AssertConfirmedTransaction("regLineTransaction1", regLineTransaction1, comment: ExpectedComment + " - " + oldComment);
					AssertConfirmedTransaction("regLineTransaction2", regLineTransaction2);
					AssertEquals("regLineTransaction3 was not changed", CusTempStorageRegLineTransactionStatusList.Codes.Deleted, regLineTransaction3.SRT_TransactionStatus);
					AssertConfirmedTransaction("regLineTransaction4", regLineTransaction4);
					AssertEquals("regLineTransaction5 was not changed since it wasn't PND", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, regLineTransaction5.SRT_TransactionStatus);
					AssertConfirmedTransaction("regLineTransaction6", regLineTransaction6);
					AssertConfirmedTransaction("regLineTransaction7", regLineTransaction7);

					AssertEquals("regLine1 CustomsStatus is changed to CLS because the PackagesRemaining is 0", "CLS", regLine1.SRL_CustomsStatus);
					AssertEquals("regLine2 CustomsStatus is changed to CLS because the RemainingGrossWeight is 0", "CLS", regLine2.SRL_CustomsStatus);
					AssertEquals("regHeader1 Status is changed to CLS because all RegLines associated to it have CustomsStatus CLS", "CLS", regHeader1.SRH_Status);

					AssertEquals("regLine3 CustomsStatus is not changed to OPN because the PackagesRemaining is not 0", "OPN", regLine3.SRL_CustomsStatus);
					AssertEquals("regLine4 CustomsStatus is not changed to OPN because the RemainingGrossWeight is not 0", "OPN", regLine4.SRL_CustomsStatus);
					AssertEquals("regHeader2 Status is not changed to OPN because not all RegLines associated to it have CustomsStatus CLS", "OPN", regHeader2.SRH_Status);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_WriteOff()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = SetUpOrgHeader();
				var header = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
				var regHeader = SetUpTmpRegHeader();
				var guarantee = SetUpGauranteeForTempStorage(regHeader, -3.0m, orgHeader);
				var regLine = SetUpRegLine(regHeader);

				SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

				var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 2, grossWeight: -2);
				var regLineTransaction2 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 1, grossWeight: -1);

				CombineAssertions(() =>
				{
					AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);
					AssertEquals("[PreReq] Bond Amount transaction2 is default", 0.0m, regLineTransaction2.SRT_BondAmount);

					header.CustomsStatus = "BBB";

					AssertEquals("2 Write off transactions in Guarantee are created", 2, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("Bond Amount transaction1 is calculated", -2.0m, regLineTransaction1.SRT_BondAmount);
					AssertEquals("Bond Amount transaction2 is calculated", -1.0m, regLineTransaction2.SRT_BondAmount);

					var writeOffTransactions = guarantee.CusGuaranteeLineTransactions.Cast<BaseCusGuaranteeLineTransaction>().Where(x => x.CPL_Comment.StartsWith("Write-off"));
					AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(0), "Write-off transaction 1", RegHeaderReference, 2.0m);
					AssertWriteOffTransaction(writeOffTransactions.ElementAtOrDefault(1), "Write-off transaction 2", RegHeaderReference, 1.0m);
				});
			}

			void AssertWriteOffTransaction(BaseCusGuaranteeLineTransaction transaction, ZString transactionName, ZString expectedReference, ZDecimal expectedTranValue)
			{
				AssertEquals(transactionName + "'s CPL_TransactionType is TRA", "TRA", transaction.CPL_TransactionType);
				AssertEquals(transactionName + "'s CPL_Reference", expectedReference, transaction.CPL_Reference);
				AssertEquals(transactionName + "'s CPL_TransactionDate is Issue Date", issueDate, transaction.CPL_TransactionDate);
				AssertEquals(transactionName + "'s CPL_Comment is Write-off + reference + MRN", string.Format("Write-off TS {0} {1}", expectedReference, MRNCode), transaction.CPL_Comment);
				AssertEquals(transactionName + "'s CPL_TranValue", expectedTranValue, transaction.CPL_TranValue);
				AssertEquals(transactionName + "'s CPL_Transaction Status is CON", "CON", transaction.CPL_TransactionStatus);
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_NoPendingAmount()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = SetUpOrgHeader();
				var header = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
				var regHeader = SetUpTmpRegHeader();
				var guarantee = SetUpGauranteeForTempStorage(regHeader, 0.0m, orgHeader);
				var regLine = SetUpRegLine(regHeader);

				SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

				var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 3, grossWeight: -3);

				CombineAssertions(() =>
				{
					AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

					header.CustomsStatus = "BBB";

					AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionIfNeeded_PendingAmountPositive()
		{
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var orgHeader = SetUpOrgHeader();
				var header = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress);
				var regHeader = SetUpTmpRegHeader();
				var guarantee = SetUpGauranteeForTempStorage(regHeader, 3.0m, orgHeader);
				var regLine = SetUpRegLine(regHeader);

				SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, packageQty: 3, grossWeight: 3, transactionType: CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance, bondAmount: 3.0m);

				var regLineTransaction1 = SetUpTransaction(regLine, CusTempStorageRegLineTransactionStatusList.Codes.Pending, packageQty: 3, grossWeight: -3);

				CombineAssertions(() =>
				{
					AssertEquals("[PreReq] No Write off transactions in Guarantee", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("[PreReq] Bond Amount transaction1 is default", 0.0m, regLineTransaction1.SRT_BondAmount);

					header.CustomsStatus = "BBB";

					AssertEquals("No Write off transactions in Guarantee are created ", 0, guarantee.CusGuaranteeLineTransactions.Count(x => x.CPL_Comment.StartsWith("Write-off")));
					AssertEquals("Bond Amount transaction1 is calculated", -3.0m, regLineTransaction1.SRT_BondAmount);

					var expectedError = "|RES=Reference reference has a positive balance of 3.00 EUR. Please check the existing transactions for this reference and create a manual adjustment if needed.|TYP=TS Guarantee";
					AssertEquals("New event in logs", expectedError, header.Logs.MostRecentLogByEventTime(Events.ErrorReport).SL_Reference);
				});
			}
		}

		#region ConfirmTemporaryStorageGoodsConsumptionWhenCustomStatusNotEmpty

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenCustomStatusNotEmpty_WhenCCC_WithPNDTransaction()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldGetPreviousDocuments: true);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					header.CustomsStatus = "CCC";

					AssertEquals("CustomsStatus is changed to CCC", "CCC", header.CustomsStatus);
					AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenCustomStatusNotEmpty_WhenCCC_WithCONTransaction()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(shouldGetPreviousDocuments: true);
				regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					AssertEquals("Prereq: RegLineTransaction count", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					header.CustomsStatus = "CCC";

					AssertEquals("CustomsStatus is changed to CCC", "CCC", header.CustomsStatus);
					AssertEquals("new regLineTransaction was not created", 1, regLine.CusTempStorageRegLineTransactions.Count(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenCustomStatusNotEmpty_WhenIsBBB()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					header.CustomsStatus = "BBB";

					AssertEquals("CustomsStatus is changed to BBB", "BBB", header.CustomsStatus);
					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenCustomStatusNotEmpty_WhenIsBBB_HavingFormatDocRef_DocRefShorterThan18()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var docReference = "1234565";
				var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference, docReference: docReference);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					header.CustomsStatus = "BBB";

					AssertEquals("CustomsStatus is changed to BBB", "BBB", header.CustomsStatus);
					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenCustomStatusNotEmpty_WhenIsBBB_HavingFormatDocRef_DocRefLongerThan18_WithoutFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var docReference = "1234565789456123789";
				var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference, docReference: docReference);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					header.CustomsStatus = "BBB";

					AssertEquals("CustomsStatus is changed to BBB", "BBB", header.CustomsStatus);
					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenCustomStatusNotEmpty_WhenIsBBB_HavingFormatDocRef_DocRefLongerThan18_WithFormatting()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var docReference = "1234565789456123789";
				var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true, shouldFormatDocumentNumber: true, regHeaderReference: docReference + "AAA", docReference: docReference);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
					header.CustomsStatus = "BBB";

					AssertEquals("CustomsStatus is changed to BBB", "BBB", header.CustomsStatus);
					AssertEquals("regLineTransaction was created", true, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					var transaction = regLine.CusTempStorageRegLineTransactions.FirstOrDefault(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance);
					AssertEquals("regLineTransaction created with SRT_TransactionStatus", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
					AssertEquals("regLineTransaction created with SRT_ReferenceType", CusEntryNumberTypes.Standard.MovementReferenceNumber, transaction.SRT_ReferenceType);
					AssertEquals("regLineTransaction created with SRT_Reference", MRNCode, transaction.SRT_Reference);
					AssertEquals("regLineTransaction created with SRT_Comments", ExpectedComment, transaction.SRT_Comments);
				});
			}
		}

		public void TestConfirmTemporaryStorageGoodsConsumptionWhenCustomStatusNotEmpty_WhenIsEmpty()
		{
			var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var (header, regLineTransaction, regLine) = SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction(createTransaction: false, shouldGetPreviousDocuments: true);

				CombineAssertions(() =>
				{
					AssertEquals("Prereq: CustomsStatus is empty", ZString.Empty, header.CustomsStatus);
					AssertEquals("Prereq: RegLineTransaction count", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));

					header.CustomsStatus = ZString.Empty;
					AssertEquals("CustomsStatus is changed to empty", ZString.Empty, header.CustomsStatus);
					AssertEquals("regLineTransaction was not created (empty)", false, regLine.CusTempStorageRegLineTransactions.Any(x => x.SRT_TransactionType != CusTempStorageRegLineTransactionTypeList.Codes.OpeningBalance));
				});
			}
		}

		#endregion

		TemporaryStorageHeader SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(OrgAddress orgAddress, string locationInHeader = LocationInHeader, string locationInPremises = LocationInHeader
			, bool shouldConfirm = true, bool shouldGetPreviousDocuments = false, bool shouldFormatDocumentNumber = false, string docReference = RegHeaderReference)
		{
			var header = Factory.New<TemporaryStorageHeaderForTest>();
			header.AMA_JobReference = DeclarationReference;
			header.LRN = TSReference;

			CusEntryNumber cusEntryNumber = CusEntryNumber.LoadOrCreate(header, "MRN", header.CountryCode);
			cusEntryNumber.CE_EntryIsSystemGenerated = true;
			cusEntryNumber.CE_EntryNum = MRNCode;
			cusEntryNumber.CE_IssueDate = issueDate;

			header.ManuallySet_ShouldConfirmTemporaryStorageGoodsConsumption = shouldConfirm;
			header.ManuallySet_TemporaryStorageTransactionInternalReferenceNumberCore = TSReference;
			header.ManuallySet_TemporaryStorageTransactionInternalReferenceTypeCore = InternalReferenceType;
			header.ManuallySet_TemporaryStorageTransactionCommentPrefix = CommentPrefix;
			header.ManuallySet_CustomsStatusToCancelTemporaryStoragePendingTransactions = new ZString[] { "AAA" };
			header.ManuallySet_CustomsStatusToConfirmTemporaryStoragePendingTransactions = new ZString[] { "BBB" };
			header.ManuallySet_CustomsStatusToNotCreateTemporaryStorageTransactions = new ZString[] { ZString.Empty };
			header.ManuallySet_ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation = shouldFormatDocumentNumber;
			header.ShouldGetPreviousDocumentsDeclared = shouldGetPreviousDocuments;
			header.ManuallySet_PreviousDocumentCodeForDataToReserveTemporaryStorageGoods = "337";
			header.SetCodeForGetPreviousDocumentsDeclared = "337";
			header.SetReferenceForGetPreviousDocumentsDeclared = docReference;
			header.SetLineNoForGetPreviousDocumentsDeclared = 1;

			header.GoodsLocation.Address.AuthorisationNumber = locationInHeader;

			var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
			premises.SRP_Type = "ADT";
			premises.SRP_CustomsLocation = locationInPremises;
			premises.SRP_Code = "X";
			premises.SRP_Description = "DESC";
			premises.SRP_OA_PremisesAddress = orgAddress.PK;

			return header;
		}

		(TemporaryStorageHeader header, ICusTempStorageRegLineTransaction regLineTransaction, ICusTempStorageRegLine regLine) SetUpDataForConfirmTemporaryStorageGoodsConsumptionIfNeededWithOneTransaction
			(string locationInHeader = LocationInHeader, string locationInPremises = LocationInHeader, bool shouldConfirm = true, bool createTransaction = true, bool shouldGetPreviousDocuments = true
			, bool shouldFormatDocumentNumber = false, string regHeaderReference = RegHeaderReference, string docReference = RegHeaderReference)
		{
			var orgHeader = SetUpOrgHeader();
			var header = SetUpHeaderForConfirmTemporaryStorageGoodsConsumptionIfNeeded(orgHeader.MainAddress, locationInHeader: locationInHeader, locationInPremises: locationInPremises, shouldConfirm: shouldConfirm, shouldGetPreviousDocuments: shouldGetPreviousDocuments, shouldFormatDocumentNumber: shouldFormatDocumentNumber, docReference: docReference);

			var regHeader = SetUpTmpRegHeader(reference: regHeaderReference);
			var regLine = Factory.New<EUInterfaces.ICusTempStorageRegLine>();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_CustomsStatus = "OPN";
			regLine.SRL_PackageType = "VQ";
			regLine.SRL_SRH = regHeader.PK;
			var regLineTransaction = (ICusTempStorageRegLineTransaction)null;
			if (createTransaction)
			{
				regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
				regLineTransaction.SRT_TransactionType = CusTempStorageRegLineTransactionTypeList.Codes.Transaction;
				regLineTransaction.SRT_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Pending;
				regLineTransaction.SRT_InternalReferenceNumber = TSReference;
				regLineTransaction.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
				regLineTransaction.SRT_SRL = regLine.PK;
			}

			var regLineItem = Factory.New<EUInterfaces.ICusTempStorageRegLineItem>();
			regLineItem.SRI_GoodsItemNumber = 1;

			var regLineItemPivot1 = Factory.New<EUInterfaces.ICusTempStorageRegLineItemPivot>();
			regLineItemPivot1.SRV_SRI_Item = regLineItem.PK;
			regLineItemPivot1.SRV_SRL_Line = regLine.PK;
			Factory.Save();

			return (header, regLineTransaction, regLine);
		}

		ICusTempStorageRegLineTransaction SetUpTransaction(ICusTempStorageRegLine regLine, ZString transactionStatus, int packageQty = 0, decimal grossWeight = 0, string transactionType = "TRN", decimal bondAmount = 0.0m)
		{
			var regLineTransaction = regLine.CusTempStorageRegLineTransactions.AddNew();
			regLineTransaction.SRT_TransactionType = transactionType;
			regLineTransaction.SRT_TransactionStatus = transactionStatus;
			regLineTransaction.SRT_InternalReferenceType = CusTempStorageRegLineTransactionInternalReferenceTypeList.Codes.Others;
			regLineTransaction.SRT_PackageQty = packageQty;
			regLineTransaction.SRT_GrossWeight = grossWeight;

			if (transactionType == "OBL")
			{
				regLineTransaction.SRT_BondAmount = bondAmount;
			}
			else
			{
				regLineTransaction.SRT_InternalReferenceNumber = TSReference;
			}

			return regLineTransaction;
		}

		void AssertConfirmedTransaction(ZString transactionName, ICusTempStorageRegLineTransaction transaction, string comment = ExpectedComment)
		{
			AssertEquals(transactionName + "'s SRT_TransactionStatus was changed", CusTempStorageRegLineTransactionStatusList.Codes.Confirmed, transaction.SRT_TransactionStatus);
			AssertEquals(transactionName + "'s SRT_ReferenceType was changed", "MRN", transaction.SRT_ReferenceType);
			AssertEquals(transactionName + "'s SRT_Reference was changed", MRNCode, transaction.SRT_Reference);
			AssertEquals(transactionName + "'s SRT_Comments was changed", comment, transaction.SRT_Comments);
			AssertEquals(transactionName + "'s SRT_TransactionDate was changed", issueDate.ToOffset(), transaction.SRT_TransactionDate);
			AssertEquals(transactionName + "'s SRT_PhysicalInOutDate was changed", issueDate.ToOffset(), transaction.SRT_PhysicalInOutDate);
		}

		void SetUpRefData()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "UN Package Types");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VQ", "VQ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NF", "NF", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"AA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		}

		EUInterfaces.ICusTempStorageRegHeader SetUpTmpRegHeader(string appCode = "AAA", string reference = RegHeaderReference)
		{
			var regHeader = Factory.New<EUInterfaces.ICusTempStorageRegHeader>();
			regHeader.SRH_AppCode = appCode;
			regHeader.SRH_Reference = reference;

			return regHeader;
		}

		CusGuaranteeHeader SetUpGauranteeForTempStorage(EUInterfaces.ICusTempStorageRegHeader regHeader, ZDecimal value, OrgHeader orgHeader)
		{
			var cusGuarantee = Factory.New<CusGuaranteeHeader>();
			cusGuarantee.CPH_Number = "Test1";
			cusGuarantee.CPH_OH_PermitHolder = orgHeader.PK;
			cusGuarantee.CPH_Type = EUGuaranteeTypeList.Codes.TST;
			cusGuarantee.CPH_SubType = "1";
			cusGuarantee.CPH_StartDate = ZDate.BrettsBirthday;
			cusGuarantee.CPH_Balance = 1000.0m;

			var commonGuarantee = Factory.New<CommonGuarantee>();
			commonGuarantee.PW_BondNumber = "Test1";
			commonGuarantee.PW_ParentID = regHeader.PK;
			commonGuarantee.PW_ParentTableCode = CusBondDetailSchema.Constants.Prefix;
			commonGuarantee.PW_CPH_Guarantee = cusGuarantee.PK;

			var guarantee = ((CommonGuarantee)(regHeader.Guarantee)).CusGuarantee;
			var guaranteeLineTransaction = guarantee.CusGuaranteeLineTransactions.AddNew();
			guaranteeLineTransaction.CPL_Reference = RegHeaderReference;
			guaranteeLineTransaction.CPL_TransactionStatus = CusTempStorageRegLineTransactionStatusList.Codes.Confirmed;
			guaranteeLineTransaction.CPL_TranValue = value;

			guarantee.AddTransaction("OPENING", "OPENING", ZString.Empty, ZString.Empty, 1000.0m, 0, transactionType: Customs.Business.PermitTransactionTypeList.Codes.OBL, isAggregated: true);

			return guarantee;
		}

		ICusTempStorageRegLine SetUpRegLine(ICusTempStorageRegHeader regHeader)
		{
			var regLine = regHeader.CusTempStorageRegLines.AddNew();
			regLine.SRL_LineNumber = 1;
			regLine.SRL_PackageType = "BX";

			return regLine;
		}

		OrgHeader SetUpOrgHeader()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "AAA";
			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = orgHeader.PK;
			orgAddress.OA_Address1 = "Address";

			return orgHeader;
		}

		const string RegHeaderReference = "reference";
		const string TSReference = "ES00001";
		const string InternalReferenceType = "OTH";
		const string MRNCode = "20ES00999930006184";
		const string LocationInHeader = "9999000002";
		const string CommentPrefix = "Prefix";
		const string DeclarationReference = "B00000001";
		const string ExpectedComment = CommentPrefix + " " + DeclarationReference;
		readonly ZDateTime issueDate = new ZDateTime(2024, 06, 10, 11, 11, 11);

		#endregion

		public void TestIsSent()
		{
			var header = Factory.New<TemporaryStorageHeader>();
			header.AMA_MessageStatus = LogicalStatusList.Codes.Failed;
			CombineAssertions(() =>
			{
				AssertEquals("When Message Status is not SNT, then false", false, header.IsSent);

				header.AMA_MessageStatus = LogicalStatusList.Codes.Sent;
				AssertEquals("When Message Status is SNT, then true", true, header.IsSent);
			});
		}

		public void TestCountryCode()
		{
			var branch = Factory.New<GlbBranch>();
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = "LV";

			var header = Factory.New<TemporaryStorageHeader>();

			CombineAssertions(() =>
			{
				AssertEquals("If no company in Header, then Country is the one on the Current Company", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, header.CountryCode);

				header.AMA_GB = branch.PK;
				branch.GB_GC = ZGuid.Invalid;

				AssertEquals("If company is invalid, then Country is the one on the Current Company", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, header.CountryCode);

				branch.GB_GC = company.PK;
				AssertEquals("If company in Header is valid, then Country is the one on that Company", "LV", header.CountryCode);
			});
		}

		public void TestShouldTSRegisterManagementSelectInventoryMenuItemBeVisible() => CombineAssertions(() =>
		{
			var orgAddress = GetOrgAddress();
			CreateCusTempStorageRegPremises(CusTempStorageRegPremisesTypeList.Codes.TemporaryStorageWarehouse, "Managed ADT Location", "P01");
			CreateCusTempStorageRegPremises(CusTempStorageRegPremisesTypeList.Codes.ExportStorageFacility, "Managed LAM Location", "P02");
			Factory.Save();

			var header = Factory.New<TemporaryStorageHeaderForTest>();

			AssertShouldTSRegisterManagementSelectInventoryMenuItemBeVisible(true, true, string.Empty, "Managed ADT Location");
			AssertShouldTSRegisterManagementSelectInventoryMenuItemBeVisible(false, false, string.Empty, "Managed ADT Location");
			AssertShouldTSRegisterManagementSelectInventoryMenuItemBeVisible(false, true, "1234567890", "Managed ADT Location");
			AssertShouldTSRegisterManagementSelectInventoryMenuItemBeVisible(false, true, string.Empty, "Managed LAM Location");
			AssertShouldTSRegisterManagementSelectInventoryMenuItemBeVisible(false, true, string.Empty, "Unmanaged Location");

			void AssertShouldTSRegisterManagementSelectInventoryMenuItemBeVisible(bool expectedValue, bool temporaryStorageEnabled, string entryNum, string location, [CallerLineNumber] int line = 0)
			{
				var registry = ObjectFactory.Get<EUInterfaces.IEUCustomsRegistry>().RegisterEnabled;
				using (registry.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, temporaryStorageEnabled))
				{
					header.MRN = entryNum;
					header.GoodsLocation.Address.AuthorisationNumber = location;
					AssertEquals($"[{line}]: RegisterEnabled = '{temporaryStorageEnabled}', MRN = '{entryNum}', AuthorisationNumber = '{location}'.", expectedValue, header.ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible);
				}
			}

			void CreateCusTempStorageRegPremises(string type, string customsLocation, string code)
			{
				var premises = Factory.New<EUInterfaces.ICusTempStorageRegPremises>();
				premises.SRP_Type = type;
				premises.SRP_CustomsLocation = customsLocation;
				premises.SRP_Code = code;
				premises.SRP_Description = $"{code} - Description";
				premises.SRP_OA_PremisesAddress = orgAddress.PK;
			}

			OrgAddress GetOrgAddress()
			{
				var orgHeader = Factory.New<OrgHeader>();
				orgHeader.OH_Code = "TST";
				var orgAddress = orgHeader.Addresses.AddNew();
				orgAddress.Address1 = "Test Address";
				return orgAddress;
			}
		});

		public void TestIsMessageTypeMatchingToTSRegisterManagementSelectInventory() => AssertEquals(false, Factory.New<TemporaryStorageHeader>().IsMessageTypeMatchingToTSRegisterManagementSelectInventory);

		protected override LightValidationTester GetNewLightValidationTester(BusinessObject bizObjToTest)
		{
			return new TemporaryStorageHeaderLightValidationTester(bizObjToTest);
		}

		public override List<ZString> GetExcludedColumns_OnlySomeSubclassesAreSetDefaultValues()
		{
			return new List<ZString>() { AsycudaManifestHeader.Schema.AMA_OA_Declarant };
		}

		sealed class TemporaryStorageHeaderLightValidationTester : LightValidationTester
		{
			public TemporaryStorageHeaderLightValidationTester(BusinessObject bo) : base(bo)
			{
			}

			protected override bool ShouldTestProperty(ZPropertyInfo info)
			{
				var propertyName = info.Name;
				return base.ShouldTestProperty(info)
					&& propertyName != TemporaryStorageBill.Schema.ABL_RL_NKPortOfDischarge
					&& propertyName != CusEntryNumber.Schema.CE_EntryStatus;
			}
		}
	}

	[TestedType(typeof(ProcessTaskCollection<TemporaryStorageHeaderProcessTask, TemporaryStorageHeader>))]
	class TemporaryStorageHeaderProcessTaskCollectionTest : Enterprise.MasterFiles.Business.Testing.ProcessTaskCollectionTest<ProcessTaskCollection<TemporaryStorageHeaderProcessTask, TemporaryStorageHeader>>
	{
		public void TestAddNewProcessTask()
		{
			ProcessTaskCollection<TemporaryStorageHeaderProcessTask, TemporaryStorageHeader> collection = GetCollectionToTestCore();
			AssertEquals(typeof(TemporaryStorageHeaderProcessTask), collection.AddNew().GetType());
		}

		protected override ProcessTaskCollection<TemporaryStorageHeaderProcessTask, TemporaryStorageHeader> GetCollectionToTestCore()
			=> (ProcessTaskCollection<TemporaryStorageHeaderProcessTask, TemporaryStorageHeader>)Factory.NewWithValidTestData<TemporaryStorageHeader>().WorkflowItems;
	}

	[TestedType(typeof(TemporaryStorageHeader))]
	sealed class TemporaryStorageHeaderWorkflowProviderTest : Enterprise.MasterFiles.Business.Testing.WorkflowProviderTest<TemporaryStorageHeader, ProcessTaskCollection<TemporaryStorageHeaderProcessTask, TemporaryStorageHeader>>
	{
		protected override ZString ExpectedWorkflowType => new TemporaryStorageHeaderWorkflowDescriptor().Code;
	}

	#region TemporaryStorageHeaderForTest

	sealed class TemporaryStorageHeaderForTest : TemporaryStorageHeader
	{
		public TemporaryStorageHeaderForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) GetEntryLineDataDeclaredToReserveTSGoodsCore() => ManuallySet_GetEntryLineDataDeclaredToReserveTSGoodsCore();
		public (IEnumerable<DeclarationDataToReserveTSGoods> dataToReserve, ZString errorInData) ManuallySet_GetEntryLineDataDeclaredToReserveTSGoodsCore()
		{
			if (ShouldGetPreviousDocumentsDeclared)
			{
				var doc = Factory.New<PreviousDocument>();
				doc.CSI_Code = SetCodeForGetPreviousDocumentsDeclared;
				doc.CSI_ReferenceNumber = SetReferenceForGetPreviousDocumentsDeclared;
				doc.CSI_LineNo = SetLineNoForGetPreviousDocumentsDeclared;
				var dataToReturn = new List<DeclarationDataToReserveTSGoods>()
					{
						new ()
						{
							Document = doc,
							Packages = new (ZString, ZInt, ZString, ZBool)[]
							{
								("FR", 1, "VIN1", false),
								("BX", 8, "", false),
								("VQ", 0, "", true),
							},
							TotalGrossWeight = 30.6m
						}
					};

				if (ShouldHaveSecondPreviousDocumentDeclared)
				{
					var doc2 = Factory.New<PreviousDocument>();
					doc2.CSI_Code = SetCodeForGetPreviousDocumentsDeclared;
					doc2.CSI_ReferenceNumber = SetReferenceForGetPreviousDocumentsDeclared2;
					doc2.CSI_LineNo = SetLineNoForGetPreviousDocumentsDeclared;
					dataToReturn.Add(
						new DeclarationDataToReserveTSGoods()
						{
							Document = doc2,
							Packages = new (ZString, ZInt, ZString, ZBool)[]
							{
									("VQ", 1, "", true),
							},
							TotalGrossWeight = 20m,
							TotalGrossWeightForVINs = 0m,
						});
				}

				return (dataToReturn, ZString.Empty);
			}
			else
			{
				return (Enumerable.Empty<DeclarationDataToReserveTSGoods>(), ZString.Empty);
			}
		}

		public ZBool ShouldGetPreviousDocumentsDeclared { private get; set; }
		public ZBool ShouldHaveSecondPreviousDocumentDeclared { private get; set; }
		public ZString SetCodeForGetPreviousDocumentsDeclared { private get; set; }
		public ZString SetReferenceForGetPreviousDocumentsDeclared { private get; set; }
		public ZString SetReferenceForGetPreviousDocumentsDeclared2 { private get; set; }
		public ZInt SetLineNoForGetPreviousDocumentsDeclared { private get; set; }

		protected override ZBool ShouldConfirmTemporaryStorageGoodsConsumption => ManuallySet_ShouldConfirmTemporaryStorageGoodsConsumption;

		public bool ManuallySet_ShouldConfirmTemporaryStorageGoodsConsumption { private get; set; }

		protected override ZString TemporaryStorageTransactionInternalReferenceNumberCore => ManuallySet_TemporaryStorageTransactionInternalReferenceNumberCore;

		public ZString ManuallySet_TemporaryStorageTransactionInternalReferenceNumberCore { private get; set; }

		protected override ZString TemporaryStorageTransactionInternalReferenceTypeCore => ManuallySet_TemporaryStorageTransactionInternalReferenceTypeCore;

		public ZString ManuallySet_TemporaryStorageTransactionInternalReferenceTypeCore { private get; set; }

		protected override IReadOnlyList<ZString> CustomsStatusToCancelTemporaryStoragePendingTransactions => ManuallySet_CustomsStatusToCancelTemporaryStoragePendingTransactions;

		public ZString[] ManuallySet_CustomsStatusToCancelTemporaryStoragePendingTransactions { private get; set; }

		protected override IReadOnlyList<ZString> CustomsStatusToConfirmTemporaryStoragePendingTransactions => ManuallySet_CustomsStatusToConfirmTemporaryStoragePendingTransactions;

		public ZString[] ManuallySet_CustomsStatusToConfirmTemporaryStoragePendingTransactions { private get; set; }

		protected override IReadOnlyList<ZString> CustomsStatusToNotCreateTemporaryStorageTransactions => ManuallySet_CustomsStatusToNotCreateTemporaryStorageTransactions;

		public ZString[] ManuallySet_CustomsStatusToNotCreateTemporaryStorageTransactions { private get; set; }

		protected override ZString PreviousDocumentCodeForDataToReserveTemporaryStorageGoods => ManuallySet_PreviousDocumentCodeForDataToReserveTemporaryStorageGoods;

		public ZString ManuallySet_PreviousDocumentCodeForDataToReserveTemporaryStorageGoods { private get; set; }

		protected override ZString TemporaryStorageTransactionCommentPrefixCore => ManuallySet_TemporaryStorageTransactionCommentPrefix;

		public ZString ManuallySet_TemporaryStorageTransactionCommentPrefix { private get; set; }

		protected override ZBool ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation => ManuallySet_ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation;

		public ZBool ManuallySet_ShouldFormatDocumentNumberForTSGoodsConsumptionConfirmation { private get; set; }

		protected override ZString GetDocumentNumberFormat(ZString dsdtMRN) => dsdtMRN + "AAA";

		protected override bool IsMessageTypeMatchingToTSRegisterManagementSelectInventoryCore => true;
	}

	#endregion
}
