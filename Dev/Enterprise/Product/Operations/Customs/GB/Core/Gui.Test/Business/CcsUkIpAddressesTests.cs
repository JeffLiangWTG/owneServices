#if !WINZOR
using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.GB.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.Registry.Business.Testing;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GUI.Registry.Testing
{
	[TestedType(typeof(CcsukIpaddressesSetting))]
	public class CcsUkIpAddressesTests : RegistryBusinessObjectTemplateTestCase<CcsukIpaddressesSetting>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override CcsukIpaddressesSetting GetBusinessObjectToClone()
		{
			return new CcsukIpaddressesSetting(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override CcsukIpaddressesSetting GetBusinessObjectToSerialise()
		{
			return MakeNewCcsukIpAddress(1);
		}

		public static CcsukIpaddressesSetting MakeNewCcsukIpAddress(int index)
		{
			var ip = new CcsukIpaddressesSetting();
			ip.LocalIpAddress = "10.44.1." + index.ToString();
			ip.CcsukParticipantIpAddress = "172.22.1." + index.ToString();
			ip.Sequence = index * 10;
			ip.FriendlyName = "Daniel." + index;
			return ip;
		}

		public void TestValidationOfIpAddressFields()
		{
			var pair = MakeNewCcsukIpAddress(1);
			AssertNoErrorContaining(pair.CcsukParticipantIpAddressInfo, "required");
			AssertNoErrorContaining(pair.LocalIpAddressInfo, "required");
			AssertNoErrorContaining(pair.CcsukParticipantIpAddressInfo, "valid");
			AssertNoErrorContaining(pair.LocalIpAddressInfo, "valid");

			pair.LocalIpAddress = "";
			pair.CcsukParticipantIpAddress = "";
			AssertHasErrorContaining(pair.CcsukParticipantIpAddressInfo, "required");
			AssertHasErrorContaining(pair.LocalIpAddressInfo, "required");
			AssertNoErrorContaining(pair.CcsukParticipantIpAddressInfo, "valid");
			AssertNoErrorContaining(pair.LocalIpAddressInfo, "valid");

			pair.LocalIpAddress = "xxx";
			pair.CcsukParticipantIpAddress = "yyy";
			AssertNoErrorContaining(pair.CcsukParticipantIpAddressInfo, "required");
			AssertNoErrorContaining(pair.LocalIpAddressInfo, "required");
			AssertHasErrorContaining(pair.CcsukParticipantIpAddressInfo, "valid");
			AssertHasErrorContaining(pair.LocalIpAddressInfo, "valid");

			pair = MakeNewCcsukIpAddress(256);
			AssertHasErrorContaining(pair.CcsukParticipantIpAddressInfo, "valid");
			AssertHasErrorContaining(pair.LocalIpAddressInfo, "valid");
		}

		public void TestValidationOfTransportWhenOtherFieldsChanged()
		{
			var pair = MakeNewCcsukIpAddress(1);
			EnvProxy.SetHostedLocationForTest("LON");
			AssertEquals("pre-req", true, EnvProxy.IsHostedWithCargowise);
			pair.Transport = CcsukIpTransportList.Codes.IPC;

			pair.TransportInfo.ClearAllNotifications();
			AssertNoErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");
			pair.Sequence = 1;
			AssertHasErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");

			pair.TransportInfo.ClearAllNotifications();
			AssertNoErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");
			pair.LocalIpAddress = "111";
			AssertHasErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");

			pair.TransportInfo.ClearAllNotifications();
			AssertNoErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");
			pair.CcsukParticipantIpAddress = "222";
			AssertHasErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");

			pair.TransportInfo.ClearAllNotifications();
			AssertNoErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");
			pair.FriendlyName = "333";
			AssertHasErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");

			pair.Transport = CcsukIpTransportList.Codes.VPN;
			pair.Sequence = 4;
			pair.LocalIpAddress = "555";
			pair.CcsukParticipantIpAddress = "666";
			pair.FriendlyName = "777";
			AssertNoErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");
		}

		public void TestValidationOfTransport()
		{
			var pair = MakeNewCcsukIpAddress(1);
			EnvProxy.SetHostedLocationForTest("LON");
			AssertEquals("pre-req", true, EnvProxy.IsHostedWithCargowise);
			AssertNoErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");
			pair.Transport = CcsukIpTransportList.Codes.IPC;
			AssertHasErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");
			pair.Transport = CcsukIpTransportList.Codes.VPN;
			AssertNoErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");

			EnvProxy.SetHostedLocationForTest(string.Empty);
			AssertEquals("pre-req", false, EnvProxy.IsHostedWithCargowise);
			AssertNoErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");
			pair.Transport = CcsukIpTransportList.Codes.IPC;
			AssertNoErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");
			pair.Transport = CcsukIpTransportList.Codes.VPN;
			AssertNoErrorContaining(pair.TransportInfo, "IPConnect is not allowed for hosted clients.  Select only VPN.");
		}

		public void TestDefaultTransportBasedOnIfHostedWithCargoWiseAndTransportValue()
		{
			var pair = MakeNewCcsukIpAddress(1);
			EnvProxy.SetHostedLocationForTest("LON");
			AssertEquals("pre-req", true, EnvProxy.IsHostedWithCargowise);
			AssertEquals("pre-req", "", pair.Transport);
			AssertEquals("default to VPN if transport is blank and hosted", "VPN", pair.DefaultTransportBasedOnIfHostedWithCargoWiseAndTransportValue);
			pair.Transport = "IPC";
			AssertEquals("default to transport value set", "IPC", pair.DefaultTransportBasedOnIfHostedWithCargoWiseAndTransportValue);

			EnvProxy.SetHostedLocationForTest(string.Empty);
			pair.Transport = "";
			AssertEquals("pre-req", false, EnvProxy.IsHostedWithCargowise);
			AssertEquals("pre-req", "", pair.Transport);
			AssertEquals("default to IPC if transport is blank and not hosted", "IPC", pair.DefaultTransportBasedOnIfHostedWithCargoWiseAndTransportValue);
			pair.Transport = "VPN";
			AssertEquals("default to transport value set", "VPN", pair.DefaultTransportBasedOnIfHostedWithCargoWiseAndTransportValue);
		}

		public void TestFullDescription()
		{
			var pair = MakeNewCcsukIpAddress(0);
			pair.Transport = "C5";
			AssertEquals("Profile: DANIEL.0; Participant:172.22.1.0; Local:10.44.1.0; Transport:C5", pair.FullDescription);
		}
	}

	[TestedType(typeof(CcsukIpaddressesSettingCollectionRegistryItem))]
	public class CcsukIpaddressesSettingCollectionTest1 : StronglyTypedRegistryItemTestCase<CcsukIpAddressesSettingCollection>
	{
		protected override StronglyTypedRegistryItem<CcsukIpAddressesSettingCollection, CcsukIpAddressesSettingCollection> GetNewRegistryItem()
		{
			return new CcsukIpaddressesSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System);
		}
	}

	[TestedType(typeof(CcsukIpAddressesSettingCollection))]
	public class CcsukIpaddressesSettingCollectionTest2 : RegistryBusinessObjectCollectionTemplateTestCase<CcsukIpAddressesSettingCollection>
	{
		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return CcsUkIpAddressesTests.MakeNewCcsukIpAddress(1);
		}

		protected override CcsukIpAddressesSettingCollection GetCollectionToTest()
		{
			return new CcsukIpAddressesSettingCollection();
		}
	}

	[TestedType(typeof(CcsukIpAddressesRegistryDataType))]
	public class CcsukIpaddressesSettingRegistryDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<CcsukIpAddressesRegistryDataType>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "Factory is required here, as it provides access to retrieved Db Values.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0028:Collection initialization can be simplified", Justification = "Add function runs code.")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0300:Simplify collection initialization", Justification = "Leaving test in existing, working, state.")]
		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var factory = new BusinessObjectFactory();
			var item1 = CcsUkIpAddressesTests.MakeNewCcsukIpAddress(1);
			var item2 = CcsUkIpAddressesTests.MakeNewCcsukIpAddress(2);
			var item3 = CcsUkIpAddressesTests.MakeNewCcsukIpAddress(3);
			var coll = new CcsukIpAddressesSettingCollection();
			coll.Add(item1);
			coll.Add(item2);
			coll.Add(item3);

			var bytes = new byte[]
			{
				60,0,63,0,120,0,109,0,108,0,32,0,118,0,101,0,114,0,115,0,105,0,111,0,110,0,61,0,34,0,49,0,46,0,48,0,34,0,32,0,101,0,110,0,99,0,111,0,100,0,105,0,110,0,103,0,61,0,34,0,117,0,116,0,102,
				0,45,0,49,0,54,0,34,0,63,0,62,0,60,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,99,0,115,0,117,0,107,0,73,0,112,0,97,0,100,0,100,0,114,0,101,0,115,0,115,0,101,0,115,0,83,0,101,0,116,
				0,116,0,105,0,110,0,103,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,0,115,0,105,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,
				0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,0,101,0,109,0,97,0,45,0,105,0,110,0,115,0,116,0,97,0,110,0,99,0,101,0,34,0,32,0,120,0,109,0,108,0,110,0,115,0,58,0,120,
				0,115,0,100,0,61,0,34,0,104,0,116,0,116,0,112,0,58,0,47,0,47,0,119,0,119,0,119,0,46,0,119,0,51,0,46,0,111,0,114,0,103,0,47,0,50,0,48,0,48,0,49,0,47,0,88,0,77,0,76,0,83,0,99,0,104,
				0,101,0,109,0,97,0,34,0,62,0,60,0,67,0,99,0,115,0,117,0,107,0,73,0,112,0,97,0,100,0,100,0,114,0,101,0,115,0,115,0,101,0,115,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,76,0,111,
				0,99,0,97,0,108,0,73,0,112,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,0,49,0,48,0,46,0,52,0,52,0,46,0,49,0,46,0,49,0,60,0,47,0,76,0,111,0,99,0,97,0,108,0,73,0,112,0,65,0,100,
				0,100,0,114,0,101,0,115,0,115,0,62,0,60,0,67,0,99,0,115,0,117,0,107,0,80,0,97,0,114,0,116,0,105,0,99,0,105,0,112,0,97,0,110,0,116,0,73,0,112,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,
				0,49,0,55,0,50,0,46,0,50,0,50,0,46,0,49,0,46,0,49,0,60,0,47,0,67,0,99,0,115,0,117,0,107,0,80,0,97,0,114,0,116,0,105,0,99,0,105,0,112,0,97,0,110,0,116,0,73,0,112,0,65,0,100,0,100,
				0,114,0,101,0,115,0,115,0,62,0,60,0,70,0,114,0,105,0,101,0,110,0,100,0,108,0,121,0,78,0,97,0,109,0,101,0,62,0,68,0,65,0,78,0,73,0,69,0,76,0,46,0,49,0,60,0,47,0,70,0,114,0,105,0,101,
				0,110,0,100,0,108,0,121,0,78,0,97,0,109,0,101,0,62,0,60,0,83,0,101,0,113,0,117,0,101,0,110,0,99,0,101,0,62,0,49,0,48,0,60,0,47,0,83,0,101,0,113,0,117,0,101,0,110,0,99,0,101,0,62,0,60,
				0,47,0,67,0,99,0,115,0,117,0,107,0,73,0,112,0,97,0,100,0,100,0,114,0,101,0,115,0,115,0,101,0,115,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,67,0,99,0,115,0,117,0,107,0,73,0,112,
				0,97,0,100,0,100,0,114,0,101,0,115,0,115,0,101,0,115,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,76,0,111,0,99,0,97,0,108,0,73,0,112,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,
				0,49,0,48,0,46,0,52,0,52,0,46,0,49,0,46,0,50,0,60,0,47,0,76,0,111,0,99,0,97,0,108,0,73,0,112,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,0,60,0,67,0,99,0,115,0,117,0,107,0,80,
				0,97,0,114,0,116,0,105,0,99,0,105,0,112,0,97,0,110,0,116,0,73,0,112,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,0,49,0,55,0,50,0,46,0,50,0,50,0,46,0,49,0,46,0,50,0,60,0,47,0,67,
				0,99,0,115,0,117,0,107,0,80,0,97,0,114,0,116,0,105,0,99,0,105,0,112,0,97,0,110,0,116,0,73,0,112,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,0,60,0,70,0,114,0,105,0,101,0,110,0,100,0,108,
				0,121,0,78,0,97,0,109,0,101,0,62,0,68,0,65,0,78,0,73,0,69,0,76,0,46,0,50,0,60,0,47,0,70,0,114,0,105,0,101,0,110,0,100,0,108,0,121,0,78,0,97,0,109,0,101,0,62,0,60,0,83,0,101,0,113,
				0,117,0,101,0,110,0,99,0,101,0,62,0,50,0,48,0,60,0,47,0,83,0,101,0,113,0,117,0,101,0,110,0,99,0,101,0,62,0,60,0,47,0,67,0,99,0,115,0,117,0,107,0,73,0,112,0,97,0,100,0,100,0,114,0,101,
				0,115,0,115,0,101,0,115,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,0,67,0,99,0,115,0,117,0,107,0,73,0,112,0,97,0,100,0,100,0,114,0,101,0,115,0,115,0,101,0,115,0,83,0,101,0,116,0,116,
				0,105,0,110,0,103,0,62,0,60,0,76,0,111,0,99,0,97,0,108,0,73,0,112,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,0,49,0,48,0,46,0,52,0,52,0,46,0,49,0,46,0,51,0,60,0,47,0,76,0,111,
				0,99,0,97,0,108,0,73,0,112,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,0,60,0,67,0,99,0,115,0,117,0,107,0,80,0,97,0,114,0,116,0,105,0,99,0,105,0,112,0,97,0,110,0,116,0,73,0,112,0,65,
				0,100,0,100,0,114,0,101,0,115,0,115,0,62,0,49,0,55,0,50,0,46,0,50,0,50,0,46,0,49,0,46,0,51,0,60,0,47,0,67,0,99,0,115,0,117,0,107,0,80,0,97,0,114,0,116,0,105,0,99,0,105,0,112,0,97,
				0,110,0,116,0,73,0,112,0,65,0,100,0,100,0,114,0,101,0,115,0,115,0,62,0,60,0,70,0,114,0,105,0,101,0,110,0,100,0,108,0,121,0,78,0,97,0,109,0,101,0,62,0,68,0,65,0,78,0,73,0,69,0,76,0,46,
				0,51,0,60,0,47,0,70,0,114,0,105,0,101,0,110,0,100,0,108,0,121,0,78,0,97,0,109,0,101,0,62,0,60,0,83,0,101,0,113,0,117,0,101,0,110,0,99,0,101,0,62,0,51,0,48,0,60,0,47,0,83,0,101,0,113,
				0,117,0,101,0,110,0,99,0,101,0,62,0,60,0,47,0,67,0,99,0,115,0,117,0,107,0,73,0,112,0,97,0,100,0,100,0,114,0,101,0,115,0,115,0,101,0,115,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0,60,
				0,47,0,65,0,114,0,114,0,97,0,121,0,79,0,102,0,67,0,99,0,115,0,117,0,107,0,73,0,112,0,97,0,100,0,100,0,114,0,101,0,115,0,115,0,101,0,115,0,83,0,101,0,116,0,116,0,105,0,110,0,103,0,62,0
			};

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(coll, bytes)
			};
		}

		protected override CcsukIpAddressesRegistryDataType GetNewDataType()
		{
			return new CcsukIpAddressesRegistryDataType();
		}

		protected override string ExpectedEditorName
		{
			get
			{
				return "CcsukIpAddressesRegistryItemEditor";
			}
		}
	}

	[TestedType(typeof(CcsukIpAddressesRegistryItemEditor))]
	public class CcsukIpAddressesRegistryItemEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation

		protected override RegistryItemEditor GetEditor()
		{
			return new CcsukIpAddressesRegistryItemEditor(RegistryItem.DataType, null, null);
		}

		protected override bool GetEditorPaneEnabledState(System.Windows.Forms.Control editorPane)
		{
			return !((CcsukIpAddressesControl)editorPane).ReadOnly;
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(CcsukIpAddressesControl);
		}

		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new CcsukIpaddressesSettingCollectionRegistryItem("", (NoResString)"", "", "", RegistryStorageFlags.System);
		}

		protected override object[] GetValidRegistryValues()
		{
			var collection = new CcsukIpAddressesSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
			collection.Add(CcsUkIpAddressesTests.MakeNewCcsukIpAddress(1));
			collection.Add(CcsUkIpAddressesTests.MakeNewCcsukIpAddress(2));
			return new object[] { collection };
		}

		protected override void AssertSetAndGetValuesEqual(object setValue, object getValue)
		{
			var collection1 = (CcsukIpAddressesSettingCollection)setValue;
			var collection2 = (CcsukIpAddressesSettingCollection)getValue;

			AssertEquals("GetValueFromEditorPane().Count", collection1.Count, collection2.Count);

			for (int i = 0; i < collection1.Count; ++i)
			{
				AssertEquals(collection1[i].CcsukParticipantIpAddress, collection2[i].CcsukParticipantIpAddress);
				AssertEquals(collection1[i].LocalIpAddress, collection2[i].LocalIpAddress);
				AssertEquals(collection1[i].FriendlyName, collection2[i].FriendlyName);
				AssertEquals(collection1[i].Sequence, collection2[i].Sequence);
			}
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get { return RegistryItemEditor.EditorPaneAnchor.All; }
		}

		#endregion
	}

	[TestedType(typeof(CcsukIpAddressesControl))]
	public class CcsukIpAddressesControlTest : RegistryZUserControlTestCase
	{
		protected override IBusiness GetNewBusinessEntity()
		{
			return new CcsukIpAddressesSettingCollection(new FallbackLevel(Env.CurrentCompany.PK, Env.CurrentBranch.PK, Env.CurrentDepartment.PK), Factory);
		}

		protected override bool IsControlOrBusinessEntityReadOnly(RegistryZUserControl control, IBusiness businessEntity)
		{
			return ((CcsukIpAddressesControl)control).AddressesGrid.ReadOnly;
		}
	}
}
#endif
