using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ASYCUDA.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;

namespace Enterprise.Customs.ASYCUDA.Business.UniversalDataTransfer.Testing
{
	public class AsycudaManifestUniversalMessagingHelperTest : TestCaseWithFactory
	{
		public void TestSendViaEHub_UsingAnotherFactoryForIndependentWork()
		{
			var source = AsycudaManifestUniversalMessagingHelperTest.SetUpManifestForEhubTest(Factory, "VUVLI", "ASY");
			var helper = new AsycudaManifestUniversalMessagingHelperForTestingObjectDisposedException(new TestLogger());
			helper.SendViaEHub(source, "", MessageSubTypeCodes.Codes.Original, new List<IMessageParent>());
			AssertNoExceptionThrown(() => source.Factory.Save());
		}

		public void TestSendViaEHub_MessageStatusNotUpdatedWhenFactorySavingFailed() => CombineAssertions(() =>
		{
			var source = AsycudaManifestUniversalMessagingHelperTest.SetUpManifestForEhubTest(Factory, "VUVLI", "ASY");
			var bill = source.Bills[0];
			bill.ABL_MessageStatus = MessageStatusCodeList.Codes.NotSent;
			var helper = new AsycudaManifestUniversalMessagingHelperForTestingMessageStatusIssue(new TestLogger());
			helper.SendViaEHub(source, "", MessageSubTypeCodes.Codes.Original, new[] { bill });
			AssertNoExceptionThrown(() => source.Factory.Save());
			AssertEquals("No message was created and sent", 0, source.Messages.Count);
			AssertEquals("ABL_MessageStatus doesn't get updated when message not sent", MessageStatusCodeList.Codes.NotSent, bill.ABL_MessageStatus);
		});

		public void TestLargeManifestsDontConvertXmlToHtml()
		{
			var bigJob = SetUpLargeManifestForDontConvertXmlToHtmlTest();
			var bill = bigJob.Bills[0];
			for (int i = 0; i < 101; i++)
			{
				bill.Packs.AddNew();
			}
			GetNewAsycudaManifestUniversalMessagingHelper().SendViaEHub(bigJob, "", MessageSubTypeCodes.Codes.Original, new List<IMessageParent>());
			var queuedMessage = bigJob.Messages[0];
			AssertEquals(false, queuedMessage.HasTriedToConvertXmlToHtmlForTesting);
			AssertContains("relatively large", queuedMessage.EM_MessageInterpretation);
		}

		public static AsycudaManifestHeader SetUpManifestForEhubTest(BusinessObjectFactory factory, ZString localPortCode, ZString manifestType)
		{
			PrepareCusCodeDataForTesting(factory);
			var staff = factory.Load<GlbStaff>(GlbStaff.CurrentUser.PK);
			staff.GS_EmailAddress = "recipient.user@forwarder.com";
			factory.Save();
			var source = AsycudaManifestHeaderTestHelper.CreateNicelyPopulatedImportManifestForTesting(localPortCode, manifestType, factory);
			source.AMA_RN_NKConveyanceNationality = ZString.Empty;
			source.AMA_VesselName = "ADMIRALENGRACHT";
			source.Vessel.RV_CarrierCode = "CC";
			source.Vessel.RV_YearOfConstruction = 1979;
			source.Vessel.RV_RadioCallSign = "KBBL";
			source.Vessel.RV_NetRegisterTon = 69000;
			source.AMA_MasterInformation = "Captain Kirk";
			source.AMA_MasterBill = "MASTER123";
			source.RegistrationDate = ZDateTime.BrettsBirthday;
			source.RegistrationNumber = "Entry 123";
			source.RegistrationStatus = "ST1";
			source.AMA_Trailer1RegNo = "TRAILER001";
			source.AMA_Trailer2RegNo = "TRAILER002";
			source.AMA_RN_NKTrailer1RegCountry = "TR";
			source.AMA_RN_NKTrailer2RegCountry = "ZA";
			source.Containers[0].ACN_StowageLocation = "MISSION";
			source.Containers[0].ACN_GoodsWeight = 69m;
			source.Containers[0].ACN_GoodsWeightUQ = Core.Constants.Weight.Pounds;
			var consol = factory.New<ForwardingConsol>();
			consol.JK_MasterBillNum = "MASTER123";
			source.SetParent(consol);
			source.Bills[0].ABL_ManifestUQ = "DJC";
			var pack = source.Bills[0].Packs.AddNew();
			SetupPack(pack, source.Containers[0].PK, factory);
			return source;
		}

		public static void SetupPack(AsycudaPack pack, ZGuid containerPK, BusinessObjectFactory factory)
		{
			pack.APA_GoodsDescription = "Desc";
			pack.APA_MarksAndNumbers = "Marks";
			pack.APA_CommodityCode = "COM";
			pack.APA_PackQty = 11;
			pack.APA_PackUQ = "PX";
			pack.APA_Volume = 3m;
			pack.APA_VolumeUQ = "M3";
			pack.APA_Weight = 6.9m;
			pack.APA_WeightUQ = "KG";
			pack.ContainerPK = containerPK;
			pack.UNDGs.FirstItemForBinding[0].DI_DG = UNDGSubstanceLoader.LoadSubstances(factory, "0004", "a", "IMO").First().PK; // needs to be a real hit in the UNDGSubstance table
		}

		public static void PrepareCusCodeDataForTesting(BusinessObjectFactory factory)
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.PackageTypes, "PackageTypes");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "ManifestCountry");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
			var vuPK = helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "VU", "Vanuatu", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue).PK;
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuPK, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NVC, "0.0.0.0");
			helper.CreateNewOrGetExistingCusCodeList("ZZ", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ManifestCountry, "SB", "Solomon Islands", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var vuVAIR = helper.CreateNewOrGetExistingCusCodeList("VU", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "VAIR", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(vuVAIR.PK, "AIR", "VUVLI");
			var sbHIRA = helper.CreateNewOrGetExistingCusCodeList("SB", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "HIRA", "Customs Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateNewOrGetExistingCusCodeListAttribute(sbHIRA.PK, "AIR", "SBHIR");

			var branch = factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var branchOrgProxy = branch.OrgProxy;
			branchOrgProxy.OH_FullName = "CUSTOMS BROKERS";
			branchOrgProxy.CustomsCodes.RemoveAndDeleteAll();

			factory.Save();
		}

		public static string SampleUxml => LoadSampleUxml(TestFiles.GetTestFilePath("UniversalXmlTest1.xml"));

		public static string LoadSampleUxml(string key) => LoadSampleUxml(Assembly.GetExecutingAssembly(), key);

		public static string LoadSampleUxml(Assembly assembly, string key)
		{
			using (var stream = assembly.GetManifestResourceStream(key))
			using (var sr = new StreamReader(stream))
			{
				return sr.ReadToEnd();
			}
		}

		protected virtual AsycudaManifestUniversalMessagingHelper GetNewAsycudaManifestUniversalMessagingHelper() => new AsycudaManifestUniversalMessagingHelperForTesting(new TestLogger());

		protected virtual AsycudaManifestHeader SetUpLargeManifestForDontConvertXmlToHtmlTest() => SetUpManifestForEhubTest(Factory, "VUVLI", "ASY");

		sealed class AsycudaManifestUniversalMessagingHelperForTesting : AsycudaManifestUniversalMessagingHelper
		{
			public AsycudaManifestUniversalMessagingHelperForTesting(INotifications notifications)
			: base(notifications)
			{
			}

			protected override ZString GetRecipientID(AsycudaManifestHeader header) => "RECIPIENT";
		}

		sealed class AsycudaManifestUniversalMessagingHelperForTestingObjectDisposedException : AsycudaManifestUniversalMessagingHelper
		{
			public AsycudaManifestUniversalMessagingHelperForTestingObjectDisposedException(
				INotifications notifications)
				: base(notifications)
			{
			}

			protected override void DeliverMessage(EHubDelivery delivery, DeliveryContext context, NonPersistentEDICommunicationMode mode, DeliveryStreamWrapperUXML deliveryStream)
			{
				base.DeliverMessage(delivery, context, mode, deliveryStream);
				throw new Exception();
			}

			protected override ZString GetRecipientID(AsycudaManifestHeader header) => "RECIPIENT";
		}

		sealed class AsycudaManifestUniversalMessagingHelperForTestingMessageStatusIssue : AsycudaManifestUniversalMessagingHelper
		{
			public AsycudaManifestUniversalMessagingHelperForTestingMessageStatusIssue(
				INotifications notifications)
				: base(notifications)
			{
			}

			protected override void SaveChanges(BusinessObjectFactory factory)
			{
				throw new Exception();
			}

			protected override string CalculateMessageStatus(IMessageParent messageParent) => MessageStatusCodeList.Codes.Sent;

			protected override ZString GetRecipientID(AsycudaManifestHeader header) => "RECIPIENT";
		}
	}
}
