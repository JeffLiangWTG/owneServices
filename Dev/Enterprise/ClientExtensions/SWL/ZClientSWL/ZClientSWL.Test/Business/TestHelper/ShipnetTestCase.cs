using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.SWL.Business.Testing
{
	public class ShipnetTestCase : TestCaseWithFactory, IDisposable
	{
		public ShipnetTestCase()
		{
		}

		public ShipnetTestCase(BusinessObjectFactory factory)
		{
			fFactory = factory;
		}

		public void DeleteAnyExistingShipnetCarriers()
		{
			OrgHeaderCollection shipnetCarriers = new OrgHeaderCollection(Factory, new ShipnetCarrierFilter().GetFilter());
			shipnetCarriers.Load();
			shipnetCarriers.RemoveAndDeleteAll();
		}

		#region ShipnetCarrier
		public OrgHeader ShipnetCarrier
		{
			get
			{
				if (fShipnetCarrier == null)
				{
					fShipnetCarrier = CreateNewShippingLine("SHIPNET SHIPPING LINE");
					fShipnetCarrier.OH_Code = "ZSHIPNETCAR";
					OrgPatternMatchOverride patternMatch = fShipnetCarrier.CreatePatternMatchOverrideForTest();
					patternMatch.OO_ForeignCode = "FGN" + fShipnetCarrier.OH_Code;
					patternMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
					patternMatch.OO_LocalGuid = fShipnetCarrier.PK;
					patternMatch = fShipnetCarrier.CreatePatternMatchOverrideForTest();
					patternMatch.OO_ForeignCode = "FGN" + TestObjectCreator.LocalClient.OH_Code;
					patternMatch.OO_Relationship = Core.Constants.OrgPatternMatchOverrideRelationships.Organisation;
					patternMatch.OO_LocalGuid = TestObjectCreator.LocalClient.PK;
					SetOrgToBeShipnetCarrier(fShipnetCarrier.CompanyData);
				}

				return fShipnetCarrier;
			}
		}

		OrgHeader fShipnetCarrier;
		public OrgHeader CreateNewShippingLine(string name)
		{
			OrgHeader result = Factory.New<OrgHeader>();
			result.OH_FullName = name;
			result.OH_RL_NKClosestPort = GlbCompany.CurrentCompany.OrgProxy.OH_RL_NKClosestPort;
			result.MainAddress.OA_Address1 = "32 Glendon Lane";
			result.MainAddress.OA_City = "Singleton";
			result.MainAddress.OA_PostCode = "2330";
			result.OH_IsShippingProvider = true;
			result.OH_IsShippingLine = true;
			return result;
		}

		#endregion
		#region ShipnetCarrierSettings
		public ShipnetSetupBusinessObject ShipnetCarrierSettings
		{
			get
			{
				if (fShipnetCarrierSettings == null)
				{
					ShipnetSetupBusinessObject registryBizObj = (ShipnetSetupBusinessObject)SWLDataRegistry.Instance.GetShipnetSetupBusinessObject(ShipnetCarrier.CompanyData.PK);
					fShipnetCarrierSettings = (ShipnetSetupBusinessObject)registryBizObj.Clone(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, ShipnetCarrier.CompanyData.PK.ToGuid()), ShipnetCarrier.Factory);
				}

				return fShipnetCarrierSettings;
			}
		}

		ShipnetSetupBusinessObject fShipnetCarrierSettings;
		#endregion
		public void SetOrgToBeShipnetCarrier(OrgCompanyData carrierCompanyData)
		{
			ShipnetSetupBusinessObject shipnetSetup = new ShipnetSetupBusinessObject(carrierCompanyData.Factory);
			shipnetSetup.CompanyDataPK = carrierCompanyData.PK;
			PopulateWithDefaultShipnetCharges(shipnetSetup);
			SWLDataRegistry.Instance.SetOrDeleteShipnetSetupBusinessObject(carrierCompanyData.PK, shipnetSetup);
		}

		public void PopulateWithDefaultShipnetCharges(ShipnetSetupBusinessObject shipnetSetup)
		{
			if (shipnetSetup != null)
			{
				shipnetSetup.IsShipnetCarrier = true;
				shipnetSetup.DebtorControlCode = "DEBTORCODE";
				shipnetSetup.CreditorControlCode = "CREDITORCODE";
				shipnetSetup.CommunicationMode.EK_Filename = Path.GetFileNameWithoutExtension(TestFile.Filename);
				shipnetSetup.CommunicationMode.EK_FileFormat = ((ZString)Path.GetExtension(TestFile.Filename)).TrimStart('.');
				shipnetSetup.CommunicationMode.EK_Destination = TestDirectoryInShortPathName;
				shipnetSetup.CommunicationMode.EK_CommunicationsTransport = ShipnetExportCommunicationsTransportMappingList.Codes.File;
				shipnetSetup.CommunicationMode.EK_Module = EDICommunicationsMode.Modules.Shipnet;
				ShipnetChargeGroup group = shipnetSetup.ChargeGroups.AddNew();
				group.ChargeGroupCode = "GROUPCODE1";
				group.ChargeGroupDescription = "GROUP1 DESCRIPTION";
				group.Charges.AddNew(TestChargeWithGST.PK);
				group = shipnetSetup.ChargeGroups.AddNew();
				group.ChargeGroupCode = "GROUPCODE2";
				group.ChargeGroupDescription = "GROUP2 DESCRIPTION";
				group.Charges.AddNew(TestChargeWithGSTFree.PK);
			}
		}

		public ZString GetExpectedExportFilename(ZString filename)
		{
			return Path.GetFileNameWithoutExtension(filename) + ZDateTime.Now.ToString(ShipnetCarrierObject.DateTimeExtension) + Path.GetExtension(filename);
		}

		#region TestFile
		public TempFile TestFile
		{
			get
			{
				if (fTestFile == null)
				{
					fTestFile = TempFile.New();
				}

				return fTestFile;
			}
		}

		TempFile fTestFile;
		#endregion
		#region TestChargeWithGST
		public AccChargeCode TestChargeWithGST
		{
			get
			{
				return TestObjectCreator.CC1;
			}
		}

		#endregion
		#region TestChargeWithGST
		public AccChargeCode TestChargeWithGSTFree
		{
			get
			{
				return TestObjectCreator.CC3;
			}
		}

		#endregion
		#region TestObjectCreator
		public TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}

				return fTestObjectCreator;
			}
		}

		TestObjectCreator fTestObjectCreator;
		#endregion
		static class PathHelper
		{
			public static string GetShortPathName(string longName)
			{
				StringBuilder shortNameBuffer = new StringBuilder(256);
				int bufferSize = shortNameBuffer.Capacity;
				int result = GetShortPathName(longName, shortNameBuffer, bufferSize);
				if (result == 0)
				{
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}

				return shortNameBuffer.ToString();
			}

			[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
			static extern int GetShortPathName([MarshalAs(UnmanagedType.LPTStr)] string path, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder shortPath, int shortPathLength);
		}

		#region TestDirectory
		public ZString TestDirectory
		{
			get
			{
				if (fTestDirectory.IsEmpty)
				{
					fTestDirectory = Path.Combine(EnvProxy.Instance.TempPath, "Shipnet\\");
					if (!Directory.Exists(fTestDirectory))
					{
						Directory.CreateDirectory(fTestDirectory);
					}
				}

				return fTestDirectory;
			}
		}

		ZString fTestDirectory;
		public ZString TestDirectoryInShortPathName
		{
			get
			{
				return PathHelper.GetShortPathName(TestDirectory);
			}
		}

		#endregion
		#region VesselWithoutAttribute
		public RefVessel VesselWithoutAttribute
		{
			get
			{
				if (fVesselWithoutAttribute == null)
				{
					fVesselWithoutAttribute = Factory.New<RefVessel>();
					fVesselWithoutAttribute.RV_Code = "VESSEL WITHOUT ATTRIBUTE";
					fVesselWithoutAttribute.RV_LloydsNumber = "ZZVWOA1";
				}

				return fVesselWithoutAttribute;
			}
		}

		RefVessel fVesselWithoutAttribute;
		#endregion
		#region VesselWithAttribute
		public RefVessel VesselWithAttribute
		{
			get
			{
				if (fVesselWithAttribute == null)
				{
					fVesselWithAttribute = Factory.New<RefVessel>();
					fVesselWithAttribute.RV_Code = "VESSEL WITH ATTRIBUTE";
					fVesselWithAttribute.RV_CustomAttrib1 = "VWA1";
					fVesselWithAttribute.RV_LloydsNumber = "ZZVWA12";
				}

				return fVesselWithAttribute;
			}
		}

		RefVessel fVesselWithAttribute;
		#endregion
		public void CleanUp()
		{
			TempDirectory.DeleteDirectory(TestDirectory);
			TestFile.Dispose();
			fTestFile = null;
		}

		protected override BusinessObjectFactory NewFactory()
		{
			return fFactory ?? base.NewFactory();
		}

		protected override void SetUp()
		{
			base.SetUp();
			SWLDataRegistry.Instance.ShipnetBackupDirectoryItem.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, Path.Combine(TestDirectory, "Backup"));
		}

		protected override void TearDown()
		{
			CleanUp();
			base.TearDown();
		}

		readonly BusinessObjectFactory fFactory;
		#region IDisposable Members
		public void Dispose()
		{
			CleanUp();
		}
		#endregion
	}
}
