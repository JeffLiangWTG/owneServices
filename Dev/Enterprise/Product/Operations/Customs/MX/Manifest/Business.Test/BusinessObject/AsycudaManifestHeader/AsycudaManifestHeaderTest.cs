using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.ASYCUDA.Business.CodeDescriptionPairLists;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaManifestHeader))]
	sealed class AsycudaManifestHeaderTest : ASYCUDA.Business.Testing.AsycudaManifestHeaderAbstractTest
	{
		public void TestBills()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			AssertType<AsycudaBillCollection>(header.Bills);
		}

		public void TestIAsycudaManifestHeader()
		{
			var bizObj = GetNewBusinessObject();
			bizObj.FillWithValidTestData();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.MXManifest.IAsycudaManifestHeader>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<ASYCUDA.Business.AsycudaManifestHeader>(bizObj.PK).GetType());
		}

		public void TestDefaultGetTypes()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Mexico, MXManifestTypes.Codes.MAN);
			CombineAssertions(() =>
			{
				AssertEquals("Default Bill Type", typeof(AsycudaBill), header.GetBillType());
				AssertEquals("Default Container Type", typeof(AsycudaContainer), header.GetContainerType());
			});
		}

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.SuspendCheckBusinessObjectType();
			header.AMA_JobReference = "C1234";
			return header;
		}

		public void TestManifestNature()
		{
			var uymanifestTypes = new MXManifestTypes().All;
			var man = uymanifestTypes.FirstOrDefault(x => x.Code == MXManifestTypes.Codes.MAN);
			AssertContainsExactElementsInAnyOrder(new[] { ShipmentTypeList.Codes.Import23, ShipmentTypeList.Codes.Export22 }, man.ManifestNatures.GetAllCodes());
		}

		public void TestDefaultCustomsDischargePort()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;

			header.AMA_RL_NKPortOfDischarge = "MXAAA";
			AssertEquals(ZString.Empty, header.AMA_CustomsDischargePort);

			var ports = new List<(string, string, Guid)>();
			ports.Add((LocoMapSystemUsageList.Codes.CustomsPortCodeList, "100", Core.Constants.CountryGuids.Mexico));
			ports.Add((USLocoMapSystemUsageList.Codes.SCK, "200", Core.Constants.CountryGuids.UnitedStates));

			SetupPortWithRefLocoMaps("MXBBB", ports);

			header.AMA_RL_NKPortOfDischarge = "MXBBB";
			AssertEquals("100", header.AMA_CustomsDischargePort);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;

			SetupPortWithRefLocoMaps("UYCCC", ports);

			header.AMA_RL_NKPortOfDischarge = "UYCCC";
			AssertEquals("200", header.AMA_CustomsDischargePort);
		}

		public void TestDefaultCustomsLoadPort()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = ShipmentTypeList.Codes.Import23;

			header.AMA_RL_NKPortOfLoading = "UYAAA";
			AssertEquals(ZString.Empty, header.AMA_CustomsLoadPort);

			var ports = new List<(string, string, Guid)>();
			ports.Add((USLocoMapSystemUsageList.Codes.SCK, "100", Core.Constants.CountryGuids.UnitedStates));
			ports.Add((LocoMapSystemUsageList.Codes.CustomsPortCodeList, "200", Core.Constants.CountryGuids.Mexico));

			SetupPortWithRefLocoMaps("UYBBB", ports);

			header.AMA_RL_NKPortOfLoading = "UYBBB";
			AssertEquals("100", header.AMA_CustomsLoadPort);

			header.AMA_Nature = ShipmentTypeList.Codes.Export22;

			SetupPortWithRefLocoMaps("MXCCC", ports);

			header.AMA_RL_NKPortOfLoading = "MXCCC";
			AssertEquals("200", header.AMA_CustomsLoadPort);
		}

		void SetupPortWithRefLocoMaps(ZString portCode, List<(string, string, Guid)> ports)
		{
			var port = Factory.New<RefUNLOCO>();
			port.RL_Code = portCode;

			foreach (var lPort in ports)
			{
				var locoMap = port.RefLocoMaps.AddNew();
				locoMap.RY_SystemUsage = lPort.Item1;
				locoMap.RY_LocalPortCode = lPort.Item2;
				locoMap.RY_RN = lPort.Item3;
			}
		}

		public void TestGetMessageSendingNotification_NotExists()
		{
			var header = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Mexico, MXManifestTypes.Codes.MAN);
			var message = header.MessageSendingNotificationHelper.GetNotifications();
			AssertNotContains(ValidationsConstants.MustBeLoggedInUnderMXToSendMXMessages, message);
		}

		public void TestGetNewMessageChooser()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;

			header.Bills.AddNew();

			var items = header.Bills.Cast<ISelectionItem>();

			AssertType<MessageChooser>("Default message chooser type.", header.GetNewMessageChooser(items, ZString.Empty, false));
			AssertType<MXMessageChooser>("Should return MXMessageChooser.", header.GetNewMessageChooser(items, MessageSubTypeCodes.Codes.Original, false));
		}

		protected override Type ExpectedTypeOfContainer => typeof(AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>);
	}
}
