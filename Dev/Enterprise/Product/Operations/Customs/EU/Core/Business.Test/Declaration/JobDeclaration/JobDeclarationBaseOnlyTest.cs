using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.Common.IE;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.EFTA.TemporaryStorageRegister.Business;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.EU.Registry;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using static Enterprise.Customs.EU.Business.EUCommonConstants;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Customs.EU;
using static Enterprise.Integration.Customs.EUExitControl;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(JobDeclaration))]
	sealed class JobDeclarationBaseOnlyTest : JobDeclarationAbstractTest<JobDeclaration>
	{
		public void TestJE_VoyageFlightNoCaption()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_TransportMode = Core.Constants.TransportModes.Road;
			var info = dec.JE_VoyageFlightNoInfo;
			AssertEquals("JE_VoyageFlightNoInfo.Description - Road", "Registration", info.Description);
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, multipleResourceKey: null, "Registration", dataBoundBusinessObject: new DataBoundBusinessObject(dec));
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

		public void TestMostInterestingLegProvider()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<MostInterestingLegProvider>(dec.MostInterestingLegProvider);
		}

		public void TestJE_OH_SupplierIsSetOnFactorySaving_DoesNotChangeJE_MessageType()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			var supplierDocumentaryAddress = dec.SupplierDocumentaryAddress;
			using (dec.SuspendJE_OH_SupplierSetting())
			{
				supplierDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
				dec.JE_MessageType = "A";
			}
			AssertEquals("PRE - dec.JE_OH_Supplier", ZGuid.Empty, dec.JE_OH_Supplier);
			Factory.Save();
			AssertEquals("dec.JE_OH_Supplier updated", org1.PK, dec.JE_OH_Supplier);
			AssertEquals("dec.JE_MessageType should not be changed", "A", dec.JE_MessageType);
		}

		public void TestJE_OH_SupplierIsSetOnFactorySaving_NonPersistent()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "!@2";
			org2.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			dec.MakeNonPersistent();
			dec.TopGroupInvoice.Delete();
			var supplierDocumentaryAddress = dec.SupplierDocumentaryAddress;
			using (dec.SuspendJE_OH_SupplierSetting())
			{
				supplierDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			}
			AssertEquals("PRE - dec.JE_OH_Supplier", ZGuid.Empty, dec.JE_OH_Supplier);
			AssertEquals("PRE - dec.IsPersistent", false, dec.IsPersistent);
			Factory.Save();
			AssertEquals("JE_OH_Supplier should not be set on factory save for non persistent", ZGuid.Empty, dec.JE_OH_Supplier);

			using (dec.SuspendJE_OH_SupplierSetting())
			{
				supplierDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			}
			Factory.Save();
			AssertEquals("JE_OH_Supplier should not be set on factory save for non persistent", ZGuid.Empty, dec.JE_OH_Supplier);
		}

		public void TestJE_OH_SupplierIsSetOnFactorySaving_JobDeclarationNotInDatabase()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var dec1 = Factory.New<JobDeclaration>();
			var supplierDocumentaryAddress = dec1.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();

			var dec2 = Factory.New<JobDeclaration>();
			dec2.SupplierDocumentaryAddress.Delete();
			dec1.DocAddresses.Remove(supplierDocumentaryAddress);
			dec2.DocAddresses.Add(supplierDocumentaryAddress);
			AssertSame("PRE - dec2.SupplierDocumentaryAddress", supplierDocumentaryAddress, dec2.SupplierDocumentaryAddress);
			AssertEquals("PRE - dec2.IsInDatabase", false, dec2.IsInDatabase);
			AssertEquals("PRE - dec2.JE_OH_SupplierInfo.HasChanges", false, dec2.JE_OH_SupplierInfo.HasChanges);
			AssertEquals("PRE - dec2.JE_OH_Supplier", ZGuid.Empty, dec2.JE_OH_Supplier);
			AssertEquals("PRE - supplierDocumentaryAddress.IsInDatabase", true, supplierDocumentaryAddress.IsInDatabase);
			AssertEquals("PRE - supplierDocumentaryAddress.E2_OA_AddressInfo.HasChanges", false, supplierDocumentaryAddress.E2_OA_AddressInfo.HasChanges);
			AssertEquals("PRE - supplierDocumentaryAddress.E2_AddressOverrideInfo.HasChanges", false, supplierDocumentaryAddress.E2_AddressOverrideInfo.HasChanges);
			Factory.Save();
			AssertEquals("dec2.JE_OH_Supplier updated - dec2.IsInDatabase is false", org1.PK, dec2.JE_OH_Supplier);
		}

		public void TestJE_OH_SupplierIsSetOnFactorySaving_JobDocAddressNotInDatabase()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "!@2";
			org2.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			Factory.Save();
			var supplierDocumentaryAddress = dec.SupplierDocumentaryAddress;
			using (dec.SuspendJE_OH_SupplierSetting())
			{
				supplierDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			}
			AssertEquals("PRE - dec.IsInDatabase", true, dec.IsInDatabase);
			AssertEquals("PRE - dec.JE_OH_SupplierInfo.HasChanges", false, dec.JE_OH_SupplierInfo.HasChanges);
			AssertEquals("PRE - dec.JE_OH_Supplier", ZGuid.Empty, dec.JE_OH_Supplier);
			AssertEquals("PRE - supplierDocumentaryAddress.IsInDatabase", false, supplierDocumentaryAddress.IsInDatabase);
			Factory.Save();
			AssertEquals("dec.JE_OH_Supplier should updated - supplierDocumentaryAddress.IsInDatabase is false", org1.PK, dec.JE_OH_Supplier);
		}

		public void TestJE_OH_SupplierIsSetOnFactorySaving_JE_OH_SupplierIsChanged()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "!@2";
			org2.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			Factory.Save();
			var supplierDocumentaryAddress = dec.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();
			dec.JE_OH_Supplier = org2.PK;
			using (dec.SuspendJE_OH_SupplierSetting())
			{
				supplierDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			}
			AssertEquals("PRE - dec.IsInDatabase", true, dec.IsInDatabase);
			AssertEquals("PRE - dec.JE_OH_SupplierInfo.HasChanges", true, dec.JE_OH_SupplierInfo.HasChanges);
			AssertEquals("PRE - dec.JE_OH_Supplier", org2.PK, dec.JE_OH_Supplier);
			AssertEquals("PRE - supplierDocumentaryAddress.IsInDatabase", true, supplierDocumentaryAddress.IsInDatabase);
			AssertEquals("PRE - supplierDocumentaryAddress.E2_OA_AddressInfo.HasChanges", false, supplierDocumentaryAddress.E2_OA_AddressInfo.HasChanges);
			AssertEquals("PRE - supplierDocumentaryAddress.E2_OA_Address", org1.MainAddress.PK, supplierDocumentaryAddress.E2_OA_Address);
			Factory.Save();
			AssertEquals("dec.JE_OH_Supplier updated to match supplierDocumentaryAddress when JE_OH_Supplier is changed to mismatch", org1.PK, dec.JE_OH_Supplier);
		}

		public void TestJE_OH_SupplierIsSetOnFactorySaving_E2_OA_AddressChanged()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "!@2";
			org2.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			var supplierDocumentaryAddress = dec.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();
			using (dec.SuspendJE_OH_SupplierSetting())
			{
				supplierDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			}
			AssertEquals("PRE - dec.IsInDatabase", true, dec.IsInDatabase);
			AssertEquals("PRE - dec.JE_OH_SupplierInfo.HasChanges", false, dec.JE_OH_SupplierInfo.HasChanges);
			AssertEquals("PRE - dec.JE_OH_Supplier", org1.PK, dec.JE_OH_Supplier);
			AssertEquals("PRE - supplierDocumentaryAddress.IsInDatabase", true, supplierDocumentaryAddress.IsInDatabase);
			AssertEquals("PRE - supplierDocumentaryAddress.E2_OA_AddressInfo.HasChanges", true, supplierDocumentaryAddress.E2_OA_AddressInfo.HasChanges);
			AssertEquals("PRE - supplierDocumentaryAddress.E2_OA_Address", org2.MainAddress.PK, supplierDocumentaryAddress.E2_OA_Address);
			Factory.Save();
			AssertEquals("dec.JE_OH_Supplier updated to match supplierDocumentaryAddress when E2_OA_Address is changed to mismatch", org2.PK, dec.JE_OH_Supplier);
		}

		public void TestJE_OH_SupplierIsSetOnFactorySaving_E2_AddressOverrideChanged()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "!@2";
			org2.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			var supplierDocumentaryAddress = dec.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();
			using (dec.SuspendJE_OH_SupplierSetting())
			{
				supplierDocumentaryAddress.E2_AddressOverride = ZBool.True;
				((INeedRow)supplierDocumentaryAddress).Row[JobDocAddress.Schema.E2_OA_Address] = org1.MainAddress.PK.ToGuid();
			}
			AssertEquals("PRE - dec.IsInDatabase", true, dec.IsInDatabase);
			AssertEquals("PRE - dec.JE_OH_SupplierInfo.HasChanges", false, dec.JE_OH_SupplierInfo.HasChanges);
			AssertEquals("PRE - dec.JE_OH_Supplier", org1.PK, dec.JE_OH_Supplier);
			AssertEquals("PRE - supplierDocumentaryAddress.IsInDatabase", true, supplierDocumentaryAddress.IsInDatabase);
			AssertEquals("PRE - supplierDocumentaryAddress.E2_OA_AddressInfo.HasChanges", false, supplierDocumentaryAddress.E2_OA_AddressInfo.HasChanges);
			AssertEquals("PRE - supplierDocumentaryAddress.E2_OA_Address", org1.MainAddress.PK, supplierDocumentaryAddress.E2_OA_Address);
			AssertEquals("PRE - supplierDocumentaryAddress.E2_AddressOverrideInfo.HasChanges", true, supplierDocumentaryAddress.E2_AddressOverrideInfo.HasChanges);
			Factory.Save();
			AssertEquals("dec.JE_OH_Supplier updated to empty when E2_AddressOverride is changed to true", ZGuid.Empty, dec.JE_OH_Supplier);
		}

		public void TestJE_OH_SupplierIsSetOnFactorySaving_SetToEmpty()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			var supplierDocumentaryAddress = dec.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();
			using (dec.SuspendJE_OH_SupplierSetting())
			{
				supplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			}
			AssertEquals("PRE - dec.IsInDatabase", true, dec.IsInDatabase);
			AssertEquals("PRE - dec.JE_OH_SupplierInfo.HasChanges", false, dec.JE_OH_SupplierInfo.HasChanges);
			AssertEquals("PRE - dec.JE_OH_Supplier", org1.PK, dec.JE_OH_Supplier);
			AssertEquals("PRE - supplierDocumentaryAddress.IsInDatabase", true, supplierDocumentaryAddress.IsInDatabase);
			AssertEquals("PRE - supplierDocumentaryAddress.E2_OA_AddressInfo.HasChanges", true, supplierDocumentaryAddress.E2_OA_AddressInfo.HasChanges);
			AssertEquals("PRE - supplierDocumentaryAddress.E2_OA_Address", ZGuid.Empty, supplierDocumentaryAddress.E2_OA_Address);
			Factory.Save();
			AssertEquals("dec.JE_OH_Supplier updated to empty when E2_OA_Address is changed to empty", ZGuid.Empty, dec.JE_OH_Supplier);
		}

		public void TestJE_OH_ImporterIsSetOnFactorySaving_DoesNotChangeJE_MessageType()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			var importerDocumentaryAddress = dec.ImporterDocumentaryAddress;
			using (dec.SuspendJE_OH_ImporterSetting())
			{
				importerDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
				dec.JE_MessageType = "A";
			}
			AssertEquals("PRE - dec.JE_OH_Importer", ZGuid.Empty, dec.JE_OH_Importer);
			Factory.Save();
			AssertEquals("dec.JE_OH_Importer updated", org1.PK, dec.JE_OH_Importer);
			AssertEquals("dec.JE_MessageType should not be changed", "A", dec.JE_MessageType);
		}

		public void TestJE_OH_ImporterIsSetOnFactorySaving_NonPersistent()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "!@2";
			org2.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			dec.MakeNonPersistent();
			dec.TopGroupInvoice.Delete();
			var importerDocumentaryAddress = dec.ImporterDocumentaryAddress;
			using (dec.SuspendJE_OH_ImporterSetting())
			{
				importerDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			}
			AssertEquals("PRE - dec.JE_OH_Importer", ZGuid.Empty, dec.JE_OH_Importer);
			AssertEquals("PRE - dec.IsPersistent", false, dec.IsPersistent);
			Factory.Save();
			AssertEquals("JE_OH_Importer should not be set on factory save for non persistent", ZGuid.Empty, dec.JE_OH_Importer);

			using (dec.SuspendJE_OH_ImporterSetting())
			{
				importerDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			}
			Factory.Save();
			AssertEquals("JE_OH_Importer should not be set on factory save for non persistent", ZGuid.Empty, dec.JE_OH_Importer);
		}

		public void TestJE_OH_ImporterIsSetOnFactorySaving_JobDeclarationNotInDatabase()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var dec1 = Factory.New<JobDeclaration>();
			var importerDocumentaryAddress = dec1.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();

			var dec2 = Factory.New<JobDeclaration>();
			dec2.ImporterDocumentaryAddress.Delete();
			dec1.DocAddresses.Remove(importerDocumentaryAddress);
			dec2.DocAddresses.Add(importerDocumentaryAddress);
			AssertSame("PRE - dec2.ImporterDocumentaryAddress", importerDocumentaryAddress, dec2.ImporterDocumentaryAddress);
			AssertEquals("PRE - dec2.IsInDatabase", false, dec2.IsInDatabase);
			AssertEquals("PRE - dec2.JE_OH_ImporterInfo.HasChanges", false, dec2.JE_OH_ImporterInfo.HasChanges);
			AssertEquals("PRE - dec2.JE_OH_Importer", ZGuid.Empty, dec2.JE_OH_Importer);
			AssertEquals("PRE - importerDocumentaryAddress.IsInDatabase", true, importerDocumentaryAddress.IsInDatabase);
			AssertEquals("PRE - importerDocumentaryAddress.E2_OA_AddressInfo.HasChanges", false, importerDocumentaryAddress.E2_OA_AddressInfo.HasChanges);
			AssertEquals("PRE - importerDocumentaryAddress.E2_AddressOverrideInfo.HasChanges", false, importerDocumentaryAddress.E2_AddressOverrideInfo.HasChanges);
			Factory.Save();
			AssertEquals("dec2.JE_OH_Importer updated - dec2.IsInDatabase is false", org1.PK, dec2.JE_OH_Importer);
		}

		public void TestJE_OH_ImporterIsSetOnFactorySaving_JobDocAddressNotInDatabase()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "!@2";
			org2.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			Factory.Save();
			var importerDocumentaryAddress = dec.ImporterDocumentaryAddress;
			using (dec.SuspendJE_OH_ImporterSetting())
			{
				importerDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			}
			AssertEquals("PRE - dec.IsInDatabase", true, dec.IsInDatabase);
			AssertEquals("PRE - dec.JE_OH_ImporterInfo.HasChanges", false, dec.JE_OH_ImporterInfo.HasChanges);
			AssertEquals("PRE - dec.JE_OH_Importer", ZGuid.Empty, dec.JE_OH_Importer);
			AssertEquals("PRE - importerDocumentaryAddress.IsInDatabase", false, importerDocumentaryAddress.IsInDatabase);
			Factory.Save();
			AssertEquals("dec.JE_OH_Importer should updated - importerDocumentaryAddress.IsInDatabase is false", org1.PK, dec.JE_OH_Importer);
		}

		public void TestJE_OH_ImporterIsSetOnFactorySaving_JE_OH_ImporterIsChanged()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "!@2";
			org2.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			Factory.Save();
			var importerDocumentaryAddress = dec.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();
			dec.JE_OH_Importer = org2.PK;
			using (dec.SuspendJE_OH_ImporterSetting())
			{
				importerDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			}
			AssertEquals("PRE - dec.IsInDatabase", true, dec.IsInDatabase);
			AssertEquals("PRE - dec.JE_OH_ImporterInfo.HasChanges", true, dec.JE_OH_ImporterInfo.HasChanges);
			AssertEquals("PRE - dec.JE_OH_Importer", org2.PK, dec.JE_OH_Importer);
			AssertEquals("PRE - importerDocumentaryAddress.IsInDatabase", true, importerDocumentaryAddress.IsInDatabase);
			AssertEquals("PRE - importerDocumentaryAddress.E2_OA_AddressInfo.HasChanges", false, importerDocumentaryAddress.E2_OA_AddressInfo.HasChanges);
			AssertEquals("PRE - importerDocumentaryAddress.E2_OA_Address", org1.MainAddress.PK, importerDocumentaryAddress.E2_OA_Address);
			Factory.Save();
			AssertEquals("dec.JE_OH_Importer updated to match importerDocumentaryAddress when JE_OH_Importer is changed to mismatch", org1.PK, dec.JE_OH_Importer);
		}

		public void TestJE_OH_ImporterIsSetOnFactorySaving_E2_OA_AddressChanged()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "!@2";
			org2.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			var importerDocumentaryAddress = dec.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();
			using (dec.SuspendJE_OH_ImporterSetting())
			{
				importerDocumentaryAddress.E2_OA_Address = org2.MainAddress.PK;
			}
			AssertEquals("PRE - dec.IsInDatabase", true, dec.IsInDatabase);
			AssertEquals("PRE - dec.JE_OH_ImporterInfo.HasChanges", false, dec.JE_OH_ImporterInfo.HasChanges);
			AssertEquals("PRE - dec.JE_OH_Importer", org1.PK, dec.JE_OH_Importer);
			AssertEquals("PRE - importerDocumentaryAddress.IsInDatabase", true, importerDocumentaryAddress.IsInDatabase);
			AssertEquals("PRE - importerDocumentaryAddress.E2_OA_AddressInfo.HasChanges", true, importerDocumentaryAddress.E2_OA_AddressInfo.HasChanges);
			AssertEquals("PRE - importerDocumentaryAddress.E2_OA_Address", org2.MainAddress.PK, importerDocumentaryAddress.E2_OA_Address);
			Factory.Save();
			AssertEquals("dec.JE_OH_Importer updated to match importerDocumentaryAddress when E2_OA_Address is changed to mismatch", org2.PK, dec.JE_OH_Importer);
		}

		public void TestJE_OH_ImporterIsSetOnFactorySaving_E2_AddressOverrideChanged()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "!@2";
			org2.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			var importerDocumentaryAddress = dec.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();
			using (dec.SuspendJE_OH_ImporterSetting())
			{
				importerDocumentaryAddress.E2_AddressOverride = ZBool.True;
				((INeedRow)importerDocumentaryAddress).Row[JobDocAddress.Schema.E2_OA_Address] = org1.MainAddress.PK.ToGuid();
			}
			AssertEquals("PRE - dec.IsInDatabase", true, dec.IsInDatabase);
			AssertEquals("PRE - dec.JE_OH_ImporterInfo.HasChanges", false, dec.JE_OH_ImporterInfo.HasChanges);
			AssertEquals("PRE - dec.JE_OH_Importer", org1.PK, dec.JE_OH_Importer);
			AssertEquals("PRE - importerDocumentaryAddress.IsInDatabase", true, importerDocumentaryAddress.IsInDatabase);
			AssertEquals("PRE - importerDocumentaryAddress.E2_OA_AddressInfo.HasChanges", false, importerDocumentaryAddress.E2_OA_AddressInfo.HasChanges);
			AssertEquals("PRE - importerDocumentaryAddress.E2_OA_Address", org1.MainAddress.PK, importerDocumentaryAddress.E2_OA_Address);
			AssertEquals("PRE - importerDocumentaryAddress.E2_AddressOverrideInfo.HasChanges", true, importerDocumentaryAddress.E2_AddressOverrideInfo.HasChanges);
			Factory.Save();
			AssertEquals("dec.JE_OH_Importer updated to empty when E2_AddressOverride is changed to true", ZGuid.Empty, dec.JE_OH_Importer);
		}

		public void TestJE_OH_ImporterIsSetOnFactorySaving_SetToEmpty()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "!@1";
			org1.OH_RL_NKClosestPort = "LVCES";
			var dec = Factory.New<JobDeclaration>();
			var importerDocumentaryAddress = dec.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_OA_Address = org1.MainAddress.PK;
			Factory.Save();
			using (dec.SuspendJE_OH_ImporterSetting())
			{
				importerDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
			}
			AssertEquals("PRE - dec.IsInDatabase", true, dec.IsInDatabase);
			AssertEquals("PRE - dec.JE_OH_ImporterInfo.HasChanges", false, dec.JE_OH_ImporterInfo.HasChanges);
			AssertEquals("PRE - dec.JE_OH_Importer", org1.PK, dec.JE_OH_Importer);
			AssertEquals("PRE - importerDocumentaryAddress.IsInDatabase", true, importerDocumentaryAddress.IsInDatabase);
			AssertEquals("PRE - importerDocumentaryAddress.E2_OA_AddressInfo.HasChanges", true, importerDocumentaryAddress.E2_OA_AddressInfo.HasChanges);
			AssertEquals("PRE - importerDocumentaryAddress.E2_OA_Address", ZGuid.Empty, importerDocumentaryAddress.E2_OA_Address);
			Factory.Save();
			AssertEquals("dec.JE_OH_Importer updated to empty when E2_OA_Address is changed to empty", ZGuid.Empty, dec.JE_OH_Importer);
		}

		public void TestItineraryCountries()
		{
			var declaration = Factory.New<JobDeclaration>();
			var itineraryCountries = declaration.ItineraryCountries;
			AssertType<ItineraryCountryCollection>("Type", itineraryCountries);
			AssertEquals("IsLoaded", true, itineraryCountries.IsLoaded);
			AssertEquals("IsRegisteredEditableChildObject", true, declaration.IsRegisteredEditableChildObject(itineraryCountries));

			AddItineraryCountryForTest(declaration, "AA");
			AddItineraryCountryForTest(declaration, "BB");
			AddItineraryCountryForTest(declaration, "CC");

			AssertEquals("Sequence# of itinerary1", 1, (int)declaration.ItineraryCountries[0].CY_Order);
			AssertEquals("Sequence# of itinerary2", 2, (int)declaration.ItineraryCountries[1].CY_Order);
			AssertEquals("Sequence# of itinerary3", 3, (int)declaration.ItineraryCountries[2].CY_Order);

			declaration.ItineraryCountries.RemoveAndDelete(declaration.ItineraryCountries[2]);
			AddItineraryCountryForTest(declaration, "HH");

			AssertEquals("Updated sequence# of existing itinerary", 3, (int)declaration.ItineraryCountries[2].CY_Order);
		}

		public void TestItineraryCountryList()
		{
			var declaration = Factory.New<JobDeclaration>();
			var itineraryCountries = declaration.ItineraryCountries;

			AssertEquals("No itinerary countries", ZString.Empty, declaration.ItineraryCountryList);

			AddItineraryCountryForTest(declaration, "AA");
			AddItineraryCountryForTest(declaration, "BB");
			AddItineraryCountryForTest(declaration, "CC");
			AssertEquals("Added 3 itineraries", "AA BB CC", declaration.ItineraryCountryList);

			declaration.ItineraryCountries.RemoveAndDelete(declaration.ItineraryCountries[2]);
			AssertEquals("Removed last itinerary", "AA BB", declaration.ItineraryCountryList);

			AddItineraryCountryForTest(declaration, "HH");
			AssertEquals("Added itinerary HH", "AA BB HH", declaration.ItineraryCountryList);
		}

		public void TestItineraryCountries_ReadOnly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals("ItineraryCountries Enabled", false, declaration.ItineraryCountries.ReadOnly);
			declaration.JE_OverrideFreightDefaults = false;
			AssertEquals("ItineraryCountries Disabled", true, declaration.ItineraryCountries.ReadOnly);
		}

		public void TestItineraryCountriesReadonly()
		{
			var declaration = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			AssertEquals("ItineraryCountriesReadonly should be false when JE_OverrideFreightDefaults is true.", false, declaration.ItineraryCountriesReadonly);
			declaration.JE_OverrideFreightDefaults = false;
			AssertEquals("ItineraryCountriesReadonly should be true when JE_OverrideFreightDefaults is false.", true, declaration.ItineraryCountriesReadonly);
		}

		public void TestUniqueVoyageIdentifier()
		{
			var declaration = Factory.New<JobDeclaration>();
			var itineraryCountries = declaration.ItineraryCountries;

			AssertEquals("Initial UniqueVoyageIdentifier should be empty.", ZString.Empty, declaration.UniqueVoyageIdentifier);
			AssertEquals("Initial ItineraryCountries not ReadOnly", false, itineraryCountries.ReadOnly);

			AddItineraryCountryForTest(declaration, "AA");
			AddItineraryCountryForTest(declaration, "BB");
			AddItineraryCountryForTest(declaration, "CC");

			AssertEquals("UniqueVoyageIdentifier after change", "AABBCC", declaration.UniqueVoyageIdentifier);
			AssertEquals("ItineraryCountries not ReadOnly after change", false, itineraryCountries.ReadOnly);
		}

		public void TestUniqueVoyageIdentifier_HasChangesEvent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var itineraryCountries = declaration.ItineraryCountries;

			bool eventFired = false;
			itineraryCountries.HasChangesChanged += (s, e) =>
			{
				if (e.ObjectJustWasChanged)
				{
					eventFired = true;
				}
			};

			declaration.UniqueVoyageIdentifier = "AABBCC";

			AssertEquals("Count after Populate", 3, itineraryCountries.Count);
			AssertEquals("1", "AA", itineraryCountries[0].CY_Code);
			AssertEquals("2", "BB", itineraryCountries[1].CY_Code);
			AssertEquals("3", "CC", itineraryCountries[2].CY_Code);
			AssertEquals("Event not fired on populate", false, eventFired);

			itineraryCountries.RemoveAndDelete(itineraryCountries[1]);
			AddItineraryCountryForTest(declaration, "DD");

			AssertEquals("Identifier after change", "AACCDD", declaration.UniqueVoyageIdentifier);
			AssertEquals("Event fired on change", true, eventFired);

			eventFired = false;
			declaration.UniqueVoyageIdentifier = declaration.UniqueVoyageIdentifier;
			AssertEquals("No event on same identifier", false, eventFired);
		}

		public void TestICusCodeDataTypeSupporter() => CombineAssertions(() =>
		{
			ICusCodeDataTypeSupporter declaration = Factory.New<JobDeclaration>();
			var cusCodeDataTypes = declaration.GetCusCodeDataTypes();
			AssertEquals("COR", typeof(ItineraryCountry), cusCodeDataTypes[CusCodeDataTypeList.Codes.CountryOfRoutingCode]);
			AssertEquals("EUO", typeof(EuOfficeCode), cusCodeDataTypes[CusCodeDataTypeList.Codes.OfficeCode]);
			AssertEquals("TPI", typeof(InlandTransport), cusCodeDataTypes[CusCodeDataTypeList.Codes.TransportInland]);
			AssertEquals("Count", 3, cusCodeDataTypes.Count);
		});

		public void TestCustomsOfficesFiltersOnCustomsCountryOfJurisdiction()
		{
			var yesterday = ZDateTime.Today.AddDays(-1);
			var tomorrow = ZDateTime.Today.AddDays(1);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eun = helper.CreateNewOrGetExistingDataGrouping(EconomicGroupList.Codes.EuropeanUnion, "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, "France", eun);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Martinique, "Martinique", eun);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "Customs Office");
			helper.CreateCusCodeType(RefCusCodeListAttributeTypes.Codes.ROLE, "ROLE");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000001", "Dijon", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Martinique, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "MQ000001", "Fort de France", yesterday, tomorrow, RefCusCodeListAttributeTypes.Codes.ROLE, "EXT");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Martinique))
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Export;
				var mainOfficeRequirement = declaration.CustomsOfficeRequirementHelper.MainOffice;
				mainOfficeRequirement.OfficeRole = "EXT";
				mainOfficeRequirement.IsForeignCountryOnly = false;
				mainOfficeRequirement.IsLocalCountryOnly = true;
				var customsOffice = declaration.Lookups.CustomsOffices;
				customsOffice.Load();
				AssertContainsExactElementsInAnyOrder(new[] { "FR000001" }, customsOffice.Select(x => x.ZZD_Code));
			}
		}

		public void TestAdditionalInfoKeys()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertSequencesEqual(new[]
			{
				  PreviousDocument.Schema.CSI_Description,
				  PreviousDocument.Schema.CSI_Code,
			}, declaration.AdditionalInfoKeys);
		}

		public void TestPreviousDocumentKeys()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertSequencesEqual(new[]
			{
				  PreviousDocument.Schema.CSI_SubType,
				  PreviousDocument.Schema.CSI_Description,
				  PreviousDocument.Schema.CSI_ReferenceNumber,
				  PreviousDocument.Schema.CSI_Code
			}, declaration.PreviousDocumentKeys);
		}

		public void TestClearInvoiceLineValuesIfSame()
		{
			var declaration = Factory.New<JobDeclaration>();
			var line1 = declaration.InvoiceLines.AddNew();
			line1.JI_Description = "Line1";
			var line2 = declaration.InvoiceLines.AddNew();
			line2.JI_Description = "Line2";

			declaration.ClearInvoiceLineValuesIfSame(new ZString("Line1"), JobComInvoiceLine.Schema.JI_Description);
			AssertEquals("Same value with declaration will be cleared", "", line1.JI_Description);
			AssertEquals("Different value from declaration will remain", "Line2", line2.JI_Description);
		}

		public void TestClearInvoiceValuesIfSame()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.JZ_Description = "Header1";
			var invoice2 = declaration.Invoices.AddNew();
			invoice2.JZ_Description = "Header2";

			declaration.ClearInvoiceValuesIfSame(new ZString("Header1"), JobComInvoiceHeader.Schema.JZ_Description);
			AssertEquals("Same value with declaration will be cleared", "", invoice1.JZ_Description);
			AssertEquals("Different value from declaration will remain", "Header2", invoice2.JZ_Description);
		}

		public void TestFilteredInvoiceLines()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InvoiceLineViewCollection<JobComInvoiceLine>>(declaration.FilteredInvoiceLines);
		}

		public void TestIsUCC6AndIsExportAndEquipmentsRequired()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
			{
				declaration.JE_MessageType = "EXP";
				AssertEquals("IsUCC6AndIsExport", false, declaration.IsUCC6AndIsExport);
				AssertEquals("EquipmentsRequired", false, declaration.EquipmentsRequired);

				declaration.JE_MessageType = "IMP";
				AssertEquals("IsUCC6AndIsExport", false, declaration.IsUCC6AndIsExport);
				AssertEquals("EquipmentsRequired", false, declaration.EquipmentsRequired);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_MessageType = "EXP";
				AssertEquals("IsUCC6AndIsExport", true, declaration.IsUCC6AndIsExport);
				AssertEquals("EquipmentsRequired", true, declaration.EquipmentsRequired);

				declaration.JE_MessageType = "IMP";
				AssertEquals("IsUCC6AndIsExport", false, declaration.IsUCC6AndIsExport);
				AssertEquals("EquipmentsRequired", false, declaration.EquipmentsRequired);
			}
		}

		public void TestIsTransportMeansImoShipIdentificationNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMeans = "!";
			AssertEquals("!", false, declaration.IsTransportMeansImoShipIdentificationNumber);
			declaration.JE_TransportMeans = TransportMeansList.Codes.ImoShipIdentificationNumber;
			AssertEquals("10", true, declaration.IsTransportMeansImoShipIdentificationNumber);
		}

		public void TestIsUCC6AndIsImport()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
			{
				declaration.JE_MessageType = "EXP";
				AssertEquals("IsUCC6AndIsExport", false, declaration.IsUCC6AndIsImport);

				declaration.JE_MessageType = "IMP";
				AssertEquals("IsUCC6AndIsExport", false, declaration.IsUCC6AndIsImport);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_MessageType = "EXP";
				AssertEquals("IsUCC6AndIsExport", false, declaration.IsUCC6AndIsImport);

				declaration.JE_MessageType = "IMP";
				AssertEquals("IsUCC6AndIsExport", true, declaration.IsUCC6AndIsImport);
			}
		}

		public void TestMultipleKeyToUseChanged()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZString.Empty;
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				var count = 0;
				void Declaration_MultipleKeyToUseChanged(object sender, EventArgs e) => count++;
				declaration.MultipleKeyToUseChanged += Declaration_MultipleKeyToUseChanged;
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				count = 0;
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				AssertEquals("MultipleKeyToUseChanged should have been triggered when JE_MessageType", 1, count);
				declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
				AssertEquals("MultipleKeyToUseChanged should not have been triggered as JE_MessageType was not changed", 1, count);
				declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
				AssertEquals("MultipleKeyToUseChanged should not have been triggered as JE_TransportMode is not part of MultipleKeysToUse computation", 1, count);
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("MultipleKeyToUseChanged should have been triggered when JE_MessageType", 2, count);
				declaration.JE_ApplicationCode = "XXX";
				AssertEquals("MultipleKeyToUseChanged should have been triggered when JE_ApplicationCode", 3, count);
				declaration.MultipleKeyToUseChanged -= Declaration_MultipleKeyToUseChanged;
			}
		}

		public void TestJE_EntryStyle_Caption_ImportUCC6()
		{
			CombineAssertions("ImportUCC6", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					var info = declaration.JE_EntryStyleInfo;
					AssertEquals("HumanReadableName", "Declaration Type", info.HumanReadableName);
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, declaration.MultipleKeysToUse, "Declaration Type", shortCaption: string.Empty, mediumCaption: string.Empty, fullDescription: "[11 01 001 000] Declaration Type");
				}
			});
		}

		public void TestJE_RL_NKFinalDestination_Caption_ExportUCC6()
		{
			CombineAssertions("ExportUCC6", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					var info = declaration.JE_RL_NKFinalDestinationInfo;
					AssertEquals("HumanReadableName", "Destination", info.HumanReadableName);
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, declaration.MultipleKeysToUse, "Destination", shortCaption: string.Empty, mediumCaption: string.Empty, fullDescription: string.Empty);
				}
			});
		}

		public void TestJE_RL_NKOrigin_Caption_ExportUCC6()
		{
			CombineAssertions("ExportUCC6", () =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					var info = declaration.JE_RL_NKOriginInfo;
					AssertEquals("HumanReadableName", "Dispatch", info.HumanReadableName);
					this.AssertDataBoundResourceStringsWithMultipleResourceKey(info, declaration.MultipleKeysToUse, "Dispatch", shortCaption: string.Empty, mediumCaption: string.Empty, fullDescription: string.Empty);
				}
			});
		}

		public void TestJE_RL_NKOrigin_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertCountryOfDispatchResourceStringDataAttribute(declaration.JE_RL_NKOriginInfo, declaration.MultipleKeysToUse);
		}

		public void TestJE_GoodsOrigin_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertCountryOfDispatchResourceStringDataAttribute(declaration.JE_GoodsOriginInfo, declaration.MultipleKeysToUse);
		}

		public void TestIsOfficeOfExitMeaningfulForDeclaration()
		{
			var declaration = Factory.New<JobDeclarationForTest>();

			declaration.JE_MessageType = "IMP";
			AssertEquals("When declaration is IMP office of exit is not meaningful", false, declaration.IsOfficeOfExitMeaningful_Exposed);

			declaration.JE_MessageType = "EXP";
			AssertEquals("When declaration is EXP office of exit is meaningful", true, declaration.IsOfficeOfExitMeaningful_Exposed);
		}

		public void TestSupportEquipments()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			AssertEquals(true, declaration.SupportEquipments);
		}

		public void TestIAdditionalInfosProvider()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertSame(declaration.AdditionalInfos, ((IAdditionalInfosProvider)declaration).AdditionalInfos);
		}

		public void TestMergeInvoiceToCusEntryForDeclaration()
		{
			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "TSTORG01";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var invGroup = declaration.JobComInvoiceGroupHeaders[0];
			var invHeader = declaration.Invoices.AddNew();
			invHeader.JZ_OH_Supplier = org1.PK;
			invHeader.JZ_InvoiceAmount = 5000;
			invHeader.JZ_InvoiceDate = ZDate.Today;
			invHeader.JZ_InvoiceNumber = "T01";
			invHeader.JZ_JZ_GroupInvoiceFK = invGroup.PK;

			var instuction = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
			instuction.CEI_Style = "10";

			var invLine1 = CreateInvLine(1, "BOOK", "Harry Potter", 100, 100, 10, "65");
			var invLine2 = CreateInvLine(2, "TV", "Haire", 100, 100, 10, "67");

			CreateDoc(declaration, "0100", 120, 30, "JI", invLine1.PK);
			CreateDoc(declaration, "2012", 260, 10, "JI", invLine2.PK);

			declaration.DoMerge();
			Factory.Save();

			declaration.ActiveEntryHeaders[0].AllEntryLines.ListChanged += JobDeclarationTest_ListChanged1;

			invLine2.Delete();
			ErrorReporter.Clear();
			declaration.DoMerge();
			AssertEquals("LastMessageReported should be empty", string.Empty, ErrorReporter.LastMessageReported);

			void JobDeclarationTest_ListChanged1(object sender, ListChangedEventArgs e)
			{
				var lines = (AllCusEntryLineCollection<CusEntryLine>)sender;
				_ = lines[0].ReadOnlySupportingDocuments;
			}

			BaseJobComInvoiceLine CreateInvLine(ZShort lineNo, ZString partNo, ZString description, ZDecimal invQuality, ZDecimal customsQuality, ZDecimal price, ZString taxType)
			{
				var invLine = invHeader.InvoiceLines.AddNew();
				invLine.JI_LineNo = lineNo;
				invLine.JI_JZ = invHeader.PK;
				invLine.JI_PartNo = partNo;
				invLine.JI_Description = description;
				invLine.JI_InvoiceQuantity = invQuality;
				invLine.JI_CustomsQuantity = customsQuality;
				invLine.JI_LinePrice = price;
				invLine.JI_ZZF_NKTaxType = taxType;
				invLine.JI_CEI = instuction.PK;
				return invLine;
			}

			void CreateDoc(JobDeclaration jobDeclaration, ZString code, ZDecimal value, ZDecimal quantity, ZString parentTableCode, ZGuid parentID)
			{
				var document = jobDeclaration.SupportingDocuments.AddNew();
				document.CSI_Code = code;
				document.CSI_Value = value;
				document.CSI_Quantity = quantity;
				document.CSI_DateOfIssue = ZDate.Today;
				document.CSI_ParentTableCode = parentTableCode;
				document.CSI_ParentID = parentID;
			}
		}

		public void TestDefaultMandatoryCustomsOfficesWhenMessageTypeChanges()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = ZString.Empty;
				declaration.CustomsOffices.RemoveAndDeleteAll();

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				var offices = declaration.CustomsOffices.Cast<EuOfficeCode>();
				AssertEquals("Export", "EXT", string.Join(", ", offices.OrderBy(x => x.CY_Code).Select(x => x.CY_Code)));

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				offices = declaration.CustomsOffices.Cast<EuOfficeCode>();
				AssertEquals("Import", "", string.Join(", ", offices.OrderBy(x => x.CY_Code).Select(x => x.CY_Code)));
			});
		}

		public void TestCustomsOfficeCollection()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = ZString.Empty;
			declaration.CustomsOffices.RemoveAndDeleteAll();

			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			AssertEquals("Export", "EXT", string.Join(", ", declaration.CustomsOfficeCollection.OrderBy(x => x.CY_Code).Select(x => x.CY_Code)));

			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			AssertEquals("Import", "", string.Join(", ", declaration.CustomsOfficeCollection.OrderBy(x => x.CY_Code).Select(x => x.CY_Code)));

			declaration.CustomsOffices.AddNew("ABC");
			declaration.CustomsOffices.AddNew("DEF");
			AssertEquals("ABC, DEF", string.Join(", ", declaration.CustomsOfficeCollection.OrderBy(x => x.CY_Code).Select(x => x.CY_Code)));
		}

		public void TestZG_IsTrainingDeclaration_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Training Entry", DataBoundResourceStrings.GetDataForProperty(declaration.ZG_IsTrainingDeclarationInfo).Caption);
		}

		public void TestZG_ShipmentType_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Shipment Type", DataBoundResourceStrings.GetDataForProperty(declaration.ZG_ShipmentTypeInfo).Caption);
		}

		public void TestZG_TypeOfSecurity_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.ZG_TypeOfSecurityInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Security", resourceStringData.Caption);
				AssertEquals("FullDescription", "[11 07 000 000] Security", resourceStringData.FullDescription);
			});
		}

		public void TestZG_LCPDepart_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("EIDR Departure", DataBoundResourceStrings.GetDataForProperty(declaration.ZG_LCPDepartInfo).Caption);
		}

		public void TestZG_LCPInspect_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("EIDR Inspection", DataBoundResourceStrings.GetDataForProperty(declaration.ZG_LCPInspectInfo).Caption);
		}

		public void TestJE_OA_SellerAddress_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Seller", declaration.JE_OA_SellerAddressInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestJE_OA_SellerAddress_Caption_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_OA_SellerAddressInfo, declaration.MultipleKeysToUse, "Seller", shortCaption: string.Empty, mediumCaption: string.Empty, fullDescription: "[13 08 000 000] Seller");
			}
		}

		public void TestZG_BorderTransportMeans_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("ZG_BorderTransportMeans: Caption", "Border T.O.ID.", declaration.ZG_BorderTransportMeansInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestZG_Box18TransportID_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.ZG_Box18TransportIDInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[18] Trans. ID (Inland)", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Trans. ID (Inland)", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Transport ID", resourceStringData.ShortCaption);
				AssertEquals("FullDescription", "[18} Inland Transport Identification", resourceStringData.FullDescription);
			});
		}

		public void TestZG_Box18TransportNationality_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("[18] Nationality", DataBoundResourceStrings.GetDataForProperty(declaration.ZG_Box18TransportNationalityInfo).Caption);
		}

		public void TestDeriveCommonDeclarationStatus()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				var entry1 = declaration.CustomsEntryHeaders.AddNew();
				entry1.CH_Status = MessageStatusList.Codes.AwaitingResponse;
				declaration.DeriveDeclarationStatus();
				AssertEquals("Single Entry JE_EntryStatus", MessageStatusList.Codes.AwaitingResponse, declaration.JE_EntryStatus);
				AssertEquals("Single Entry JE_MessageStatus", MessageStatusList.Codes.AwaitingResponse, declaration.JE_MessageStatus);

				var entry2 = declaration.CustomsEntryHeaders.AddNew();
				entry2.CH_Status = MessageStatusList.Codes.OK;
				entry2.CH_EntryStatus = MessageStatusList.Codes.OK;
				declaration.DeriveDeclarationStatus();
				AssertEquals("Multiple Entry JE_EntryStatus", MessageStatusList.Codes.MultipleStatus, declaration.JE_EntryStatus);
				AssertEquals("Multiple Entry JE_MessageStatus", MessageStatusList.Codes.MultipleStatus, declaration.JE_MessageStatus);
			});
		}

		public void TestDeriveCommonDeclarationStatus_NotIsDeclarationIntegrated()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				TestDeriveCommonDeclarationStatus();
			}
		}

		public void TestEntryStatusFromFailedToAwaitingResponseWithNoSaveBetweenFromSendingMessages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.SetAsFailedFromTransmission();
			entryHeader.CH_Status = MessageStatusList.Codes.AwaitingResponse;
			Factory.Save();
			AssertEquals(MessageStatusList.Codes.AwaitingResponse, declaration.JE_EntryStatus);
		}

		public void TestAddInfoAttributes()
		{
			var declaration = Factory.New<JobDeclaration>();

			AssertEquals("AddInfoLookups.SpecificCircumstanceIndicatorList", declaration.ZG_SpecificCircumstanceIndicatorInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("AddInfoLookups.GatewayList", declaration.ZG_GatewayInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("AddInfoLookups.AuthorisationNumberList", declaration.ZG_AuthorisationNumberInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("AddInfoLookups.Box18TransportCountryList", declaration.ZG_Box18TransportNationalityInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("AddInfoLookups.CommunityTransitStatusIDList", declaration.ZG_CTStatusIDInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals(JobDeclaration.Schema.ZG_VATDeferTypeMaxLength, declaration.ZG_VATDeferTypeInfo.GetAttribute<MaxLengthAttribute>().MaxLength);
			AssertEquals("EU.JobDeclaration.ZG_VATDeferType", declaration.ZG_VATDeferTypeInfo.GetAttribute<ResourceStringDataAttribute>().Key);
			AssertEquals("AddInfoLookups.DeferTypeList", declaration.ZG_VATDeferTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("AddInfoLookups.ShipmentTypeList", declaration.ZG_ShipmentTypeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("AddInfoLookups.StyleOfEntrySOEList", declaration.ZG_StyleOfEntrySOEInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("AddInfoLookups.AgreedPlaceCodeList", declaration.ZG_AgreedPlaceCodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
			AssertEquals("AddInfoLookups.MethodOfPaymentList", declaration.ZG_MethodOfPaymentInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
		}

		public void TestDefaultIATALoadPort_NoException()
		{
			var factory = new BusinessObjectFactory();
			factory.AllowMultipleBusinessObjectsAroundOneRow = false;

			var invoice = factory.New<JobComInvoiceHeader>();
			var declaration = (JobDeclaration)new FakeDeclarationCreatorForInvoice(invoice).HeaderData;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			declaration.JE_IATALoadPort = ZString.Empty;

			Assert(!declaration.Factory.AllowMultipleBusinessObjectsAroundOneRow);
			AssertNoExceptionThrown(delegate
			{
				declaration.JE_RL_NKPortOfLoading = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "MXGDL").RL_Code;
			});
			AssertEquals("declaration.JE_IATALoadPort", "GDL", declaration.JE_IATALoadPort);
		}

		public void TestDefaultIATALoadPort()
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			declarationMock.Protected().Setup<bool>("SupportIATALoadPortDefaulting").Returns(true);
			var declaration = declarationMock.Object;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			declaration.JE_IATALoadPort = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("declaration.JE_IATALoadPort", "SYD", declaration.JE_IATALoadPort);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			declaration.JE_IATALoadPort = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("declaration.JE_IATALoadPort", ZString.Empty, declaration.JE_IATALoadPort);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			declaration.JE_IATALoadPort = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("declaration.JE_IATALoadPort", ZString.Empty, declaration.JE_IATALoadPort);

			declarationMock.Protected().Setup<bool>("SupportIATALoadPortDefaulting").Returns(false);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			declaration.JE_IATALoadPort = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("declaration.JE_IATALoadPort", ZString.Empty, declaration.JE_IATALoadPort);

			declarationMock.Protected().Setup<bool>("SupportIATALoadPortDefaulting").Returns(true);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = declaration.TransportModeAirCodeForTesting;
			declaration.JE_RL_NKPortOfLoading = ZString.Empty;
			declaration.JE_IATALoadPort = ZString.Empty;
			declaration.JE_RL_NKPortOfLoading = "AUSYD";
			AssertEquals("declaration.JE_IATALoadPort", "SYD", declaration.JE_IATALoadPort);
		}

		public void TestConfiguration()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<DeclarationConfiguration>(declaration.Configuration);
		}

		public void TestContainerModeVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			foreach (var code in declaration.Lookups.TransportTypeList.GetAllCodes())
			{
				declaration.JE_TransportMode = code;
				AssertEquals(code, true, declaration.ContainerModeVisible);
			}
		}

		public void TestIsTrainingDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			Factory.Save();
			AssertEquals("ZG_IsTrainingDeclaration HasChanges initially?", false, declaration.ZG_IsTrainingDeclarationInfo.HasChanges);
			AssertEquals($"JE_AddInfo [{declaration.JE_AddInfo}] contains 'IsTrainingDeclaration' initially?", false, declaration.JE_AddInfo.Contains("IsTrainingDeclaration"));

			declaration.ZG_AgreedPlaceCode = "X";
			AssertEquals("ZG_IsTrainingDeclaration HasChanges after setting another AddInfo value?", false, declaration.ZG_IsTrainingDeclarationInfo.HasChanges);

			declaration.ZG_IsTrainingDeclaration = true;
			AssertEquals("ZG_IsTrainingDeclaration HasChanges after setting it to TRUE?", true, declaration.ZG_IsTrainingDeclarationInfo.HasChanges);

			Factory.Save();
			AssertEquals($"JE_AddInfo [{declaration.JE_AddInfo}] contains 'IsTrainingDeclaration=Y' after setting it to TRUE?", true, declaration.JE_AddInfo.Contains("IsTrainingDeclaration=Y"));
			AssertEquals("ZG_IsTrainingDeclaration HasChanges after save?", false, declaration.ZG_IsTrainingDeclarationInfo.HasChanges);

			declaration.ZG_IsTrainingDeclaration = false;
			AssertEquals("ZG_IsTrainingDeclaration HasChanges after setting it to FALSE?", true, declaration.ZG_IsTrainingDeclarationInfo.HasChanges);

			Factory.Save();
			AssertEquals($"JE_AddInfo [{declaration.JE_AddInfo}] contains 'IsTrainingDeclaration' after setting it back to FALSE?", false, declaration.JE_AddInfo.Contains("IsTrainingDeclaration"));
			AssertEquals("ZG_IsTrainingDeclaration HasChanges after 2nd save?", false, declaration.ZG_IsTrainingDeclarationInfo.HasChanges);
		}

		public void TestAmountToGuarantee()
		{
			using (ConfigurationTestHelper.TemporarilySetUseUniversalFeeCalculationConfiguration(Factory, false))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				helper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var currentCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				var procedure = Factory.New<RefCusProcedure>();
				procedure.ZZ6_ProcedureCode = "Ye";
				procedure.ZZ6_PreviousProcedureCode = "12";
				procedure.ZZ6_ShipmentType = "EXP";
				procedure.ZZ6_IsGuaranteeConsumed = YesNoList.Codes.Yes;
				procedure.ZZ6_IsGuaranteeReleased = YesNoList.Codes.Yes;
				procedure.ZZ6_ZZZ_NKDataGrouping = currentCountry;
				procedure.ZZ6_Concession = "367";
				procedure.ZZ6_Description = "description";

				var dty = helper.CreateNewOrGetExistingRateType(currentCountry, Universal.Constants.RateTypes.Duty, "DTY");
				helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts, dty.PK);
				helper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty, dty.PK);
				Factory.Save();

				var declaration = Factory.New<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				invoiceLine.JI_Procedure = "Ye12367";
				var invoiceLine2 = invoice.InvoiceLines.AddNew();
				invoiceLine2.JI_Procedure = "Ye12367";
				var invoiceLine3 = invoice.InvoiceLines.AddNew();
				invoiceLine3.JI_Procedure = "Ye12367";
				var entry = declaration.CustomsEntryHeaders.AddNew();
				var entryLine = entry.MergedLines.AddNew();
				entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.CustomsDutyOnIndustrialProducts).CF_ChargeAmount = 300m;
				entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.Vat).CF_ChargeAmount = 100m;
				entryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.DefinitiveAntiDumpingDuty).CF_ChargeAmount = 5m;
				invoiceLine.JI_CL = entryLine.PK;

				var cancelledEntry = declaration.CustomsEntryHeaders.AddNew();
				cancelledEntry.CH_EntryStatus = EntryStatusList.Codes.Cancelled;
				var cancelledEntryLine = cancelledEntry.MergedLines.AddNew();
				cancelledEntryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.Vat).CF_ChargeAmount = 10m;
				invoiceLine2.JI_CL = cancelledEntryLine.PK;

				var clearedEntry = declaration.CustomsEntryHeaders.AddNew();
				clearedEntry.CH_EntryStatus = EntryStatusList.Codes.Clear;
				var clearedEntryLine = clearedEntry.MergedLines.AddNew();
				clearedEntryLine.Fees.GetOrAddFeeByFeeType(UniversalReferenceConstants.RefCusRateCodes.Vat).CF_ChargeAmount = 20m;
				invoiceLine3.JI_CL = clearedEntryLine.PK;

				AssertEquals("405,00 EUR", declaration.AmountToGuarantee.ToString());
			}
		}

		public void TestRemainingGuaranteeBalance_NoCustomsGuarantee()
		{
			AssertEquals("0,00 EUR", Factory.New<JobDeclaration>().RemainingGuaranteeBalance.ToString());
		}

		public void TestRemainingGuaranteeBalance_HasCustomsGuarantee()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			var customsGuarantee = declaration.CustomsGuarantee;
			customsGuarantee.CusGuaranteeLineTransactions.AddNew().CPL_TranValue = 123.45;
			customsGuarantee.CPH_UnitOfMeasure = "GBP";
			AssertEquals("123,45 GBP", declaration.RemainingGuaranteeBalance.ToString());
		}

		public void TestExposedImporterAndSupplierTURNCodes()
		{
			var gb = RefCountry.LoadFromCountryCode(Factory, Enterprise.Core.Constants.CountryCodes.UnitedKingdom);

			var turnCode1 = "GB43210987654321";
			var org1 = Factory.New<OrgHeader>();
			org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, turnCode1, gb);

			var turnCode2 = "GB12345678901234";
			var org2 = Factory.New<OrgHeader>();
			org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, turnCode2, gb);

			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("declaration.SupplierTurnCode", "", declaration.SupplierTraderId);
			AssertEquals("declaration.ImporterTurnCode", "", declaration.ImporterTraderId);

			declaration.JE_OH_Supplier = org1.PK;
			declaration.JE_OH_Importer = org2.PK;
			AssertEquals("declaration.SupplierTurnCode", turnCode1, declaration.SupplierTraderId);
			AssertEquals("declaration.ImporterTurnCode", turnCode2, declaration.ImporterTraderId);

			declaration.JE_OH_Supplier = org2.PK;
			declaration.JE_OH_Importer = org1.PK;
			AssertEquals("declaration.SupplierTurnCode", turnCode2, declaration.SupplierTraderId);
			AssertEquals("declaration.ImporterTurnCode", turnCode1, declaration.ImporterTraderId);

			declaration.JE_OH_Supplier = ZGuid.Invalid;
			declaration.JE_OH_Importer = ZGuid.Invalid;
			AssertEquals("declaration.SupplierTurnCode", "", declaration.SupplierTraderId);
			AssertEquals("declaration.ImporterTurnCode", "", declaration.ImporterTraderId);
		}

		public void TestBondedWarehousingHelper()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<BondedWarehousingHelper>(declaration.BondedWarehousingHelper);
		}

		public void TestInventorySelectionHeader()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InventorySelectionHeader>(declaration.InventorySelectionHeader);
		}

		public void TestShouldUpdateOutwardLinesWithInventoryDetails()
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			AssertEquals(false, declarationMock.Object.ShouldUpdateOutwardLinesWithInventoryDetails);

			using (BaseJobDeclaration.SetupWHSUniversalXMLForTesting())
			{
				declarationMock.Protected().Setup<bool>("IsInventorySelectionEnabledCore").Returns(true);
				AssertEquals(true, declarationMock.Object.ShouldUpdateOutwardLinesWithInventoryDetails);
			}
		}

		public void TestJE_TransportMeans_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_TransportMeansInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Type of ID", resourceStringData.Caption);
				AssertEquals("MediumCaption", "ID Type", resourceStringData.MediumCaption);
			});
		}

		public void TestJE_TransportMeans_Caption_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_TransportMeansInfo, declaration.MultipleKeysToUse, "Type of ID", shortCaption: string.Empty, mediumCaption: "ID Type", fullDescription: "[19 06 061 000] Arrival transport means < Type of identification");
			}
		}

		public void TestJE_TransportModeInland_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var resourceStringData = DataBoundResourceStrings.GetDataForProperty(declaration.JE_TransportModeInlandInfo);
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[26] Inland M.O.T", resourceStringData.Caption);
				AssertEquals("MediumCaption", "Inland M.O.T.", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Inland", resourceStringData.ShortCaption);
			});
		}

		public void TestJE_TransportModeInland_Caption_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = IEJobMessageTypeList.Codes.Import;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_TransportModeInlandInfo, declaration.MultipleKeysToUse, "Inland M.O.T", shortCaption: "Inland", mediumCaption: "Inland M.O.T.", fullDescription: "[19 04 001 000] Inland mode of transport");
			}
		}

		public void TestAuthorizationUsageUpdater()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<CusAuthorizationUsageUpdater>(declaration.AuthorizationUsageUpdater);
		}

		public void TestIEntryStyleCalculatorFallbackInfoProviderMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			var fallbackInfoProvider = (IEntryStyleCalculatorFallbackInfoProvider)declaration;
			CombineAssertions(() =>
			{
				AssertEquals("GetEntrySubStyleForCommonTransit", EntryStyleListImport.Codes.ImportFromEFTAMember, fallbackInfoProvider.GetEntrySubStyleForCommonTransit(Factory.New<RefCountry>()));

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertEquals("GetEntryStyleForInwardProcessingVATPayment", EntryStyleListImport.Codes.ImportNormal, fallbackInfoProvider.GetEntryStyleForInwardProcessingVATPayment());

				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertEquals("GetEntryStyleForInwardProcessingVATPayment", EntryStyleListExport.Codes.ExportNormal, fallbackInfoProvider.GetEntryStyleForInwardProcessingVATPayment());
			});
		}

		public void TestCustomsSupervisingOffice()
		{
			var declaration = Factory.New<JobDeclaration>();
			var address = declaration.SupervisingOfficeDocAddress;
			CombineAssertions(() =>
			{
				AssertEquals("DocAddressType", DocAddressType.CustomsSupervisingOffice, address.DocAddressType);
				AssertEquals("Default Contact Type", ContactType.Administration, address.DefaultContactType);
			});
		}

		public void TestGovernmentContractor()
		{
			var declaration = Factory.New<JobDeclaration>();
			var address = declaration.GovernmentContractorDocAddress;
			CombineAssertions(() =>
			{
				AssertEquals("DocAddressType", DocAddressType.GovernmentContractor, address.DocAddressType);
				AssertEquals("Default Contact Type", ContactType.Administration, address.DefaultContactType);
			});
		}

		public void TestPlaceOfLoading()
		{
			var declaration = Factory.New<JobDeclaration>();
			var address = declaration.PlaceOfLoadingDocAddress;
			CombineAssertions(() =>
			{
				AssertEquals("DocAddressType", DocAddressType.CustomsPlaceOfLoading, address.DocAddressType);
				AssertEquals("Default Contact Type", ContactType.Administration, address.DefaultContactType);
			});
		}

		public void TestSellingParty()
		{
			var declaration = Factory.New<JobDeclaration>();
			var address = declaration.SellingPartyDocAddress;
			CombineAssertions(() =>
			{
				AssertEquals("DocAddressType", DocAddressType.SellingParty, address.DocAddressType);
				AssertEquals("Default Contact Type", ContactType.Administration, address.DefaultContactType);
			});
		}

		public void TestExporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var address = declaration.ExporterDocAddress;
			CombineAssertions(() =>
			{
				AssertEquals("DocAddressType", DocAddressType.Exporter, address.DocAddressType);
				AssertEquals("Default Contact Type", ContactType.Administration, address.DefaultContactType);
			});
		}

		public void TestRepresentative()
		{
			var declaration = Factory.New<JobDeclaration>();
			var address = declaration.RepresentativeDocAddress;
			CombineAssertions(() =>
			{
				AssertEquals("DocAddressType", DocAddressType.Representative, address.DocAddressType);
				AssertEquals("Default Contact Type", ContactType.Administration, address.DefaultContactType);
			});
		}

		public void TestJE_TransportIDInland_MaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertAesTransitionPeriodDepartureTransportMeansIdentificationNumberMaxLength(declaration, () => declaration.JE_TransportIDInlandInfo.MaxLength, JobDeclarationSchema.JE_TransportIDInland.MaxLength);
		}

		public void TestJE_Trailer1RegNo_MaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertAesTransitionPeriodDepartureTransportMeansIdentificationNumberMaxLength(declaration, () => declaration.JE_Trailer1RegNoInfo.MaxLength, JobDeclarationSchema.JE_Trailer1RegNo.MaxLength);
		}

		public void TestJE_Trailer2RegNo_MaxLength()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertAesTransitionPeriodDepartureTransportMeansIdentificationNumberMaxLength(declaration, () => declaration.JE_Trailer2RegNoInfo.MaxLength, JobDeclarationSchema.JE_Trailer2RegNo.MaxLength);
		}

		void AssertAesTransitionPeriodDepartureTransportMeansIdentificationNumberMaxLength(JobDeclaration declaration, Func<int> getPropertyMaxLength, int nonAesTransitionPeriodMaxLength)
		{
			CombineAssertions(() =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
				{
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.AESTransitionPeriod,
						RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
					{
						AssertEquals("UCC6AndIsExport is True and TransitionPeriodAES30 is True.", JobDeclaration.AesTransitionPeriodDepartureTransportMeansIdentificationNumberMaxLength, getPropertyMaxLength());
					}

					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.AESTransitionPeriod,
						RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
					{
						AssertEquals("UCC6AndIsExport is True and TransitionPeriodAES30 is False.", nonAesTransitionPeriodMaxLength, getPropertyMaxLength());
					}
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
				{
					using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.AESTransitionPeriod,
						RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
					{
						AssertEquals("UCC6AndIsExport is False and TransitionPeriodAES30 is True.", nonAesTransitionPeriodMaxLength, getPropertyMaxLength());
					}
				}
			});
		}

		public void TestContractualPartnerDocAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var address = declaration.ContractualPartnerDocAddress;

			CombineAssertions(() =>
			{
				AssertEquals("DocAddressType", DocAddressType.ContractualPartner, address.DocAddressType);
				AssertEquals("Default Contact Type", ContactType.Consignor, address.DefaultContactType);
			});
		}

		public void TestCarrierEUBorderDocAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("DocAddressType", DocAddressType.Carrier, declaration.CarrierEUBorderDocAddress.DocAddressType);
		}

		public void TestIsUCC6()
		{
			AssertEquals(false, Factory.New<JobDeclaration>().IsUCC6);
		}

		public void TestIsUCC5()
		{
			AssertEquals(false, Factory.New<JobDeclaration>().IsUCC5);
		}

		public void TestIsUCCCompliant()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("Default", false, declaration.IsUCCCompliant);
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					AssertEquals("Is UCC6", true, declaration.IsUCCCompliant);
				}
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC5Core", true))
				{
					AssertEquals("Is UCC5", true, declaration.IsUCCCompliant);
				}
			});
		}

		public void TestIsTransitionPeriodAES30()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("No functionality", false, declaration.IsTransitionPeriodAES30);
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("Functionality is enabled", true, declaration.IsTransitionPeriodAES30);
				}
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Customs.Universal.Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("Functionality is disabled", false, declaration.IsTransitionPeriodAES30);
				}
			});
		}

		public void TestUseIDDDocument()
		{
			AssertEquals("UseIDDDocument is defaulted to false in EU.", false, Factory.New<JobDeclaration>().UseIDDDocument);
		}

		public void TestMultipleKeysToUse_CaptionKeySAD()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals(true, declaration.MultipleKeysToUse.Contains(JobDeclaration.CaptionKeySAD));
		}

		public void TestMultipleKeysToUse_CaptionKeyUCC() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("export", true, declaration.MultipleKeysToUse.Contains(JobDeclaration.CaptionKeyExportUCC6));
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("import", true, declaration.MultipleKeysToUse.Contains(JobDeclaration.CaptionKeyImportUCC6));
				declaration.JE_MessageType = JobMessageTypeList.Codes.MiscellaneousCustoms;
				AssertEquals("default", true, declaration.MultipleKeysToUse.Contains(JobDeclaration.CaptionKeyUCC));
			}
		});

		public void TestMultipleKeysToUse_CaptionKeyBLT() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				declaration.JE_ApplicationCode = "BLT";
				AssertEquals("BLT", true, declaration.MultipleKeysToUse.Contains(JobDeclaration.CaptionKeyBLT));
				declaration.JE_ApplicationCode = "INF";
				AssertEquals("not BLT", false, declaration.MultipleKeysToUse.Contains(JobDeclaration.CaptionKeyBLT));
			}
		});

		public void TestDeclarationNumberCaption() => CombineAssertions(() =>
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Entry Number", DataBoundResourceStrings.GetDataForProperty(declaration.DeclarationNumberInfo).Caption);
			declaration.JE_ApplicationCode = "XXX";
			AssertEquals("MRN", DataBoundResourceStrings.GetDataForPropertyWithMultipleResourceKey(declaration.DeclarationNumberInfo, JobDeclaration.CaptionKeyBLT).Caption);
		});

		public void TestTransportModeConverter()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<TransportModeTranslator>(declaration.TransportModeTranslator);
		}

		public void TestJE_ShipmentIncoTerm_ValueChanged_UCC6Enabled() => AssertJE_ShipmentIncoTerm_ValueChanged(true);

		public void TestJE_ShipmentIncoTerm_ValueChanged_UCC6Disabled() => AssertJE_ShipmentIncoTerm_ValueChanged(false);

		public void TestIncoTermPlace_ReadOnly_UCC6Enabled() => AssertIncoTermPlace_ReadOnly(true);

		public void TestIncoTermPlace_ReadOnly_UCC6Disabled() => AssertIncoTermPlace_ReadOnly(false);

		public void TestAgreedPlaceCodeSupportAndVisible_UCC6Enabled() => AssertAgreedPlaceCodeSupportAndVisible(true);

		public void TestAgreedPlaceCodeSupportAndVisible_UCC6Disabled() => AssertAgreedPlaceCodeSupportAndVisible(false);

		public void TestJE_RL_NKPortOfFirstArrival_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Port of First EU Arrival", declaration.JE_RL_NKPortOfFirstArrivalInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
				AssertEquals("MediumCaption", "First EU Arrival", declaration.JE_RL_NKPortOfFirstArrivalInfo.GetAttribute<ResourceStringDataAttribute>().MediumCaption);
				AssertEquals("ShortCaption", "EU Arrival", declaration.JE_RL_NKPortOfFirstArrivalInfo.GetAttribute<ResourceStringDataAttribute>().ShortCaption);
			});
		}
		public void TestSupportMultipleWarehouseEntry()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("SupportMultipleWarehouseEntry", true, declaration.SupportMultipleWarehouseEntry);
		}

		public void TestIsWarehouseInventoryEnabledRegistryItemDependentProperties_DeclarationInCurrentCompany()
		{
			var declaration = Factory.New<JobDeclarationForWarehouseEnabledTest>();
			CombineAssertions(() =>
			{
				AssertEquals("IsWarehouseEnabled = false, SupportInwardProcessingCore", false, declaration.SupportInwardProcessingCoreExposed);
				AssertEquals("IsWarehouseEnabled = false, SupportsBondedWarehousingCore", false, declaration.SupportsBondedWarehousingCoreExposed);

				using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("IsWarehouseEnabled = true, SupportInwardProcessingCore", true, declaration.SupportInwardProcessingCoreExposed);
					AssertEquals("IsWarehouseEnabled = true, SupportsBondedWarehousingCore", true, declaration.SupportsBondedWarehousingCoreExposed);
				}
			});
		}

		public void TestIsWarehouseInventoryEnabledRegistryItemDependentProperties_DeclarationInDifferentCompany()
		{
			var declaration = Factory.New<JobDeclarationForWarehouseEnabledTest>();
			CombineAssertions(() =>
			{
				using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Env.CurrentCompany.Country.Code))
				{
					using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
					{
						AssertEquals("Enabled for Env company, SupportInwardProcessingCore", false, declaration.SupportInwardProcessingCoreExposed);
						AssertEquals("Enabled for Env company, SupportsBondedWarehousingCore", false, declaration.SupportsBondedWarehousingCoreExposed);
					}

					using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertEquals("Enabled for declaration company, SupportInwardProcessingCore", true, declaration.SupportInwardProcessingCoreExposed);
						AssertEquals("Enabled for declaration company, SupportsBondedWarehousingCore", true, declaration.SupportsBondedWarehousingCoreExposed);
					}
				}
			});
		}

		public void TestIsInventorySelectionEnabledCoreForDifferentCompany()
		{
			var declaration = Factory.New<JobDeclarationForWarehouseEnabledTest>();
			var entryInstruction = Factory.New<CusEntryInstructionForTest>();
			var warehouseAddress = Factory.NewWithValidTestData<OrgAddress>();
			entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
			declaration.CustomsEntryInstructions.Add(entryInstruction);

			CombineAssertions(() =>
			{
				using (GlbCompany.TemporaryLoginInNewCompanyForCountry(Env.CurrentCompany.Country.Code))
				{
					AssertNotEquals("Declaration and Current Company Differ", Env.CurrentCompanyPK, declaration.CompanyPK);
					using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
					{
						Factory.InvalidateCachedProperties();
						entryInstruction.Warehouse.Header.CompanyData.OB_IMUsedBondedWhs = true;
						AssertEquals("InventorySelectionEnabledCore enabled for global", false, declaration.IsInventorySelectionEnabledCoreExposed);
					}

					using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						Factory.InvalidateCachedProperties();
						entryInstruction.Warehouse.Header.CompanyData.OB_IMUsedBondedWhs = true;
						AssertEquals("InventorySelectionEnabledCore enabled for declaration", true, declaration.IsInventorySelectionEnabledCoreExposed);
					}
				}
			});
		}

		public override void TestIsWarehouseOrderFunctionActivated()
		{
			var declaration = Factory.New<JobDeclarationForWarehouseEnabledTest>();
			using (CustomsDataRegistry.Instance.SupportWarehouseOrderLines.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("should be equal to SupportWarehouseOrderLines value", true, declaration.IsWarehouseOrderFunctionActivated);
			}

			using (CustomsDataRegistry.Instance.SupportWarehouseOrderLines.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals("should be equal to SupportWarehouseOrderLines value", false, declaration.IsWarehouseOrderFunctionActivated);
			}
		}

		public void TestIsInventorySelectionEnabledNotEnabled() => AssertIsInventorySelectionEnabledCore(false, company => { });

		public void TestIsInventorySelectionEnabledBondedWhsEnabled() => AssertIsInventorySelectionEnabledCore(true, company => company.OB_IMUsedBondedWhs = true);

		public void TestIsInventorySelectionEnabledTemporaryImportsEnabled() => AssertIsInventorySelectionEnabledCore(true, company => company.OB_CusInventoryForTemporaryImports = true);

		public void TestIsInventorySelectionEnabledTemporaryExportsEnabled() => AssertIsInventorySelectionEnabledCore(true, company => company.OB_CusInventoryForTemporaryExports = true);

		public void TestIsInventorySelectionEnabledInwardProcessingEnabled() => AssertIsInventorySelectionEnabledCore(true, company => company.OB_CusInventoryForInwardProcessing = true);

		public void TestIsInventorySelectionEnabledOutwardProcessingEnabled() => AssertIsInventorySelectionEnabledCore(true, company => company.OB_CusInventoryForOutwardProcessing = true);

		public void TestDefaultIsHighValueOverride()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
			{
				EUCustomsDataRegistry.Instance.DefaultDV1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				AssertEquals("IsHighValueOvrd is defaulted to true when DefaultDV1 Registry is set to yes, declaration is Import and DV1DetailsSupport is true.", true, declaration.ZG_IsHighValueOvrd);

				EUCustomsDataRegistry.Instance.DefaultDV1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				AssertEquals("IsHighValueOvrd is defaulted to false when declaration is Export.", false, declaration.ZG_IsHighValueOvrd);

				EUCustomsDataRegistry.Instance.DefaultDV1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				AssertEquals("IsHighValueOvrd is not defaulted when DefaultDV1 Registry is set to no.", false, declaration.ZG_IsHighValueOvrd);

				EUCustomsDataRegistry.Instance.DefaultDV1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				AssertEquals("IsHighValueOvrd is defaulted to false when declaration is Export.", false, declaration.ZG_IsHighValueOvrd);
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", false))
			{
				EUCustomsDataRegistry.Instance.DefaultDV1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				AssertEquals("IsHighValueOvrd is defaulted to false when DV1DetailsSupport is false.", false, declaration.ZG_IsHighValueOvrd);

				EUCustomsDataRegistry.Instance.DefaultDV1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				AssertEquals("IsHighValueOvrd is defaulted to false when DV1DetailsSupport is false.", false, declaration.ZG_IsHighValueOvrd);

				EUCustomsDataRegistry.Instance.DefaultDV1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
				AssertEquals("IsHighValueOvrd is defaulted to false when DV1DetailsSupport is false.", false, declaration.ZG_IsHighValueOvrd);

				EUCustomsDataRegistry.Instance.DefaultDV1.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				declaration.JE_MessageType = EU.Business.MessageTypeList.Codes.Export;
				AssertEquals("IsHighValueOvrd is defaulted to false when DV1DetailsSupport is false.", false, declaration.ZG_IsHighValueOvrd);
			}
		}

		public void TestDefaultIsHighValueOverride_ClearNonImport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			declaration.ZG_IsHighValueOvrd = true;
			declaration.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
			AssertEquals(false, declaration.ZG_IsHighValueOvrd);
		}

		public void TestDefaultIsHighValueOverride_FallBackCompany()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (EUCustomsDataRegistry.Instance.DefaultDV1.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
				{
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					AssertEquals("Fallback to company level", true, declaration.ZG_IsHighValueOvrd);
				}
			}
		}

		public void TestDefaultIsHighValueOverride_FallBackBranch()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (EUCustomsDataRegistry.Instance.DefaultDV1.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, true))
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
				{
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					AssertEquals("Fallback to branch", true, declaration.ZG_IsHighValueOvrd);
				}
			}
		}

		public void TestDefaultIsHighValueOverride_RegistryFalse()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (EUCustomsDataRegistry.Instance.DefaultDV1.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", true))
				{
					declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
					AssertEquals("DV1Detail box should have value false when Registry value is false", false, declaration.ZG_IsHighValueOvrd);
				}
			}
		}

		public void TestJE_RN_NKTransportNationality_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("[21] Nationality", DataBoundResourceStrings.GetDataForProperty(declaration.JE_RN_NKTransportNationalityInfo).Caption);
		}

		public void TestJE_IATALoadPort_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("[61] Foreign Airport Code", DataBoundResourceStrings.GetDataForProperty(declaration.JE_IATALoadPortInfo).Caption);
		}

		public void TestJE_DeclarantType_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("[14] Rep. Type", DataBoundResourceStrings.GetDataForProperty(declaration.JE_DeclarantTypeInfo).Caption);
		}

		public void TestJE_VesselName_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("[21] Vessel", DataBoundResourceStrings.GetDataForProperty(declaration.JE_VesselNameInfo).Caption);
		}

		public void TestJE_AircraftRegistration_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Aircraft Registration Number", DataBoundResourceStrings.GetDataForProperty(declaration.JE_AircraftRegistrationInfo).Caption);
			AssertEquals("Aircraft Reg No.", DataBoundResourceStrings.GetDataForProperty(declaration.JE_AircraftRegistrationInfo).ShortCaption);
		}

		public void TestJE_OA_DeclarantAddress_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_OA_DeclarantAddressInfo, declaration.MultipleKeysToUse, "[14] Declarant", shortCaption: string.Empty, mediumCaption: string.Empty, fullDescription: "[14] Declarant. Name of the declarant controlling this declaration.");
			}
		}

		public void TestJE_OA_DeclarantAddress_Caption_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_OA_DeclarantAddressInfo, declaration.MultipleKeysToUse, "Declarant", shortCaption: string.Empty, mediumCaption: string.Empty, fullDescription: "[13 05 000 000] Declarant");
			}
		}

		public void TestJE_OA_Representative_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Representative", declaration.JE_OA_RepresentativeInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestJE_OA_Representative_Caption_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_OA_RepresentativeInfo, declaration.MultipleKeysToUse, "Representative", shortCaption: "Represent.", mediumCaption: string.Empty, fullDescription: "[13 06 000 000] Representative");
			}
		}

		public void TestJE_OA_ManufacturerAddress_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Manufacturer", declaration.JE_OA_ManufacturerAddressInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestJE_OwnerRef_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var resourceStringDataAttribute = declaration.JE_OwnerRefInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "[7] Declarant\'s Ref", resourceStringDataAttribute.Caption);
				AssertEquals("ShortCaption", "[7] Dec. Ref", resourceStringDataAttribute.ShortCaption);
			});
		}

		public void TestJE_OH_Buyer_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Buyer", declaration.JE_OH_BuyerInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestJE_OH_Buyer_Caption_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_OH_BuyerInfo, declaration.MultipleKeysToUse, "Buyer", shortCaption: string.Empty, mediumCaption: string.Empty, fullDescription: "[13 09 000 000] Buyer");
			}
		}

		public void TestTransportIDLabel()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("[21] Transport ID", declaration.TransportIDLabel);
		}

		public void TestInlandTransport()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertType<InlandTransportCollection>(declaration.InlandTransports);
		}

		public void TestDefermentPartyDocAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var address = declaration.DefermentPartyDocAddress;
			AssertNotNull("docAddresses.GetDocAddress(DocAddressType.DefermentParty)", address);
			AssertEquals(DocAddressType.DefermentParty, address.DocAddressType);
		}

		public void TestGetEntryStyleCalculationStrategy()
		{
			var declaration = Factory.New<JobDeclarationForEntryStyleCalculationTest>();
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertType<ImportEntryStyleCalculationStrategy>("When IsImport", declaration.GetEntryStyleCalculationStrategyExposed());

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertType<ExportEntryStyleCalculationStrategy>("When IsExport", declaration.GetEntryStyleCalculationStrategyExposed());

				declaration.JE_MessageType = "";
				AssertType<EmptyEntryStyleCalculationStrategy>("When any other declaration type", declaration.GetEntryStyleCalculationStrategyExposed());
			});
		}

		public void TestGetEntryStyleByEntryType()
		{
			var declaration = Factory.New<JobDeclarationForEntryStyleCalculationTest>();
			CombineAssertions(() =>
			{
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				AssertEquals("When IsImport", EntryStyleListImport.Codes.ImportNormal, declaration.GetEntryStyleByEntryTypeExposed());

				declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				AssertEquals("When IsExport", EntryStyleListExport.Codes.ExportNormal, declaration.GetEntryStyleByEntryTypeExposed());

				declaration.JE_MessageType = "";
				AssertEquals("When any other declaration type", "", declaration.GetEntryStyleByEntryTypeExposed());
			});
		}

		public void TestChangingManufacturerAddressUpdatesManufacturer()
		{
			var dec = Factory.New<JobDeclaration>();
			Assert(dec.JE_OA_ManufacturerAddress.IsEmpty);
			Assert(dec.JE_OH_Manufacturer.IsEmpty);

			var manufacturer1 = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturer2 = Factory.NewWithValidTestData<OrgHeader>();
			var manufacturer1address = manufacturer1.Addresses.AddNew();
			var manufacturer2address = manufacturer2.Addresses.AddNew();

			dec.JE_OA_ManufacturerAddress = manufacturer1address.PK;
			AssertEquals(manufacturer1.PK, dec.JE_OH_Manufacturer);

			dec.JE_OA_ManufacturerAddress = manufacturer2address.PK;
			AssertEquals(manufacturer2.PK, dec.JE_OH_Manufacturer);

			dec.JE_OA_ManufacturerAddress = ZGuid.Empty;
			Assert(dec.JE_OH_Manufacturer.IsEmpty);
		}

		public void TestSetAppropriateSupplierAndImporter_SetToNull()
		{
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			var dec = Factory.New<JobDeclaration>();
			var jobDocAddrSup = dec.SupplierDocumentaryAddress;
			var jobDocAddrImp = dec.ImporterDocumentaryAddress;
			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			dec.JE_OverrideFreightDefaults = false;

			dec.JE_OH_Supplier = guid2;
			dec.JE_OH_Importer = guid1;
			jobDocAddrImp.E2_AddressOverride = true;
			jobDocAddrSup.E2_AddressOverride = true;
			Factory.Save();

			AssertEquals(ZGuid.Empty, dec.JE_OH_Supplier);
			AssertEquals(ZGuid.Empty, dec.JE_OH_Importer);

			dec.JE_OH_Supplier = guid2;
			dec.JE_OH_Importer = guid1;
			dec.JE_JS = ZGuid.Empty;
			jobDocAddrSup.E2_OA_Address = ZGuid.Empty;
			jobDocAddrImp.E2_OA_Address = ZGuid.Empty;
			jobDocAddrSup.E2_AddressOverride = false;
			jobDocAddrImp.E2_AddressOverride = false;
			Factory.Save();

			AssertEquals(ZGuid.Empty, dec.JE_OH_Supplier);
			AssertEquals(ZGuid.Empty, dec.JE_OH_Importer);

			dec.JE_OH_Supplier = guid2;
			dec.JE_OH_Importer = guid1;
			dec.JE_JS = shipment.PK;
			dec.JE_OverrideFreightDefaults = true;
			Factory.Save();

			AssertEquals(ZGuid.Empty, dec.JE_OH_Supplier);
			AssertEquals(ZGuid.Empty, dec.JE_OH_Importer);
		}

		public void TestWarehouseDocAddressValueChangedWithNoException()
		{
			var declarationToBeDeleted = Factory.New<JobDeclaration>();
			AssertNotNull(declarationToBeDeleted.WarehouseDocAddress);
			Assert(declarationToBeDeleted.WarehouseDocAddress.E2_OA_Address.IsEmpty);
			var wareHouseDocAddress = declarationToBeDeleted.WarehouseDocAddress;
			Assert(!wareHouseDocAddress.IsDeleted);
			Assert(!declarationToBeDeleted.IsDeleted);
			AssertNoExceptionThrown(() =>
			{
				declarationToBeDeleted.Delete();
			});
			Assert(wareHouseDocAddress.IsDeleted);
			Assert("Declaration can be deleted while it's not in database and it's parentBO is doing Delete()", declarationToBeDeleted.IsDeleted);
		}

		public void TestCustomsEntryInstructions()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<CusEntryInstructionCollection<CusEntryInstruction>>(dec.CustomsEntryInstructions);
		}

		public void TestGuarantees()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<GuaranteeForDeclarationCollection>(dec.Guarantees);
			var guarantee = dec.Guarantees.AddNew();
			AssertType<GuaranteeForDeclaration>(guarantee);
		}

		public void TestSettingDocAddressSetsJeOhExceptWhenNonPersistentEgWhenImportingInvoicesDirectly()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.France))
			{
				var org_paris = Factory.NewWithValidTestData<OrgHeader>();
				org_paris.OH_RL_NKClosestPort = "FRPAR";
				var org2_Atlanta = Factory.NewWithValidTestData<OrgHeader>();
				org_paris.OH_RL_NKClosestPort = "USATL";
				var address11_Lyon = org_paris.Addresses.AddNew();
				var address12_Marseilles = org_paris.Addresses.AddNew();
				var address21_LosAngeles = org2_Atlanta.Addresses.AddNew();
				var address22_StPauls = org2_Atlanta.Addresses.AddNew();
				address11_Lyon.OA_RL_NKRelatedPortCode = "FRLIO";
				address12_Marseilles.OA_RL_NKRelatedPortCode = "FRMRS";
				address21_LosAngeles.OA_RL_NKRelatedPortCode = "USLAX";
				address22_StPauls.OA_RL_NKRelatedPortCode = "USMSP";

				var declaration = Factory.New<JobDeclaration>();
				declaration.SupplierDocumentaryAddress.E2_OA_Address = address21_LosAngeles.PK;
				AssertEquals(org2_Atlanta.PK, declaration.JE_OH_Supplier);
				declaration.ImporterDocumentaryAddress.E2_OA_Address = address11_Lyon.PK;
				AssertEquals(org_paris.PK, declaration.JE_OH_Importer);

				declaration = Factory.New<JobDeclaration>();
				declaration.MakeNonPersistent();
				declaration.SupplierDocumentaryAddress.E2_OA_Address = address22_StPauls.PK;
				AssertEquals(ZGuid.Empty, declaration.JE_OH_Supplier);
				declaration.ImporterDocumentaryAddress.E2_OA_Address = address12_Marseilles.PK;
				AssertEquals(ZGuid.Empty, declaration.JE_OH_Importer);
			}
		}

		[TestDate(1987, 12, 11)]
		public void TestUCRForTranshipment()
		{
			var jobDec = Factory.New<JobDeclaration>();
			jobDec.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789ABC");
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var baseMawb = Factory.New<CusMAWB>();
			var baseHawb = baseMawb.ChildBills.AddNew();
			baseHawb.CS_TranshipmentEntryNum = "T123456";
			baseHawb.CS_JE_CustomsFormalEntry = jobDec.PK;
			Factory.Save();

			AssertMatch("Generating UCR for transhipment hawb entry", new Regex("7..123456789ABC-T123456"), jobDec.JE_UCR);
		}

		public void TestGetNewRelatedDeclarationCore_BJob()
		{
			var source = Factory.New<JobDeclaration>();
			source.ZG_StyleOfEntrySOE = "8";
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			var cto = Factory.NewWithValidTestData<OrgHeader>();
			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			var depot = Factory.NewWithValidTestData<OrgHeader>();
			var containerYard = Factory.NewWithValidTestData<OrgHeader>();
			var spoff = Factory.NewWithValidTestData<OrgHeader>();
			source.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			source.SupplierDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			source.ContainerTerminalOperatorDocAddress.E2_OA_Address = cto.MainAddress.PK;
			source.WarehouseDocAddress.E2_OA_Address = warehouse.MainAddress.PK;
			source.DepotDocAddress.E2_OA_Address = depot.MainAddress.PK;
			source.ContainerYardDocAddress.E2_OA_Address = containerYard.MainAddress.PK;
			source.SupervisingOfficeDocAddress.E2_OA_Address = spoff.MainAddress.PK;
			var result = (JobDeclaration)source.GetNewRelatedDeclaration(Factory);
			AssertEquals(importer.MainAddress.PK, result.ImporterDocumentaryAddress.E2_OA_Address);
			AssertEquals(supplier.MainAddress.PK, result.SupplierDocumentaryAddress.E2_OA_Address);
			AssertEquals(cto.MainAddress.PK, result.ContainerTerminalOperatorDocAddress.E2_OA_Address);
			AssertEquals(warehouse.MainAddress.PK, result.WarehouseDocAddress.E2_OA_Address);
			AssertEquals(depot.MainAddress.PK, result.DepotDocAddress.E2_OA_Address);
			AssertEquals(containerYard.MainAddress.PK, result.ContainerYardDocAddress.E2_OA_Address);
			AssertEquals(spoff.MainAddress.PK, result.SupervisingOfficeDocAddress.E2_OA_Address);
			AssertEquals(ZString.Empty, result.ZG_StyleOfEntrySOE);
		}

		public void TestGetNewRelatedDeclarationCore_SJob()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneeDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.E2_OA_Address = supplier.MainAddress.PK;
			var source = Factory.New<JobDeclaration>();
			source.JE_JS = shipment.PK;
			source.ShipmentSynchroniser.Synchronise(true);
			var result = (JobDeclaration)source.GetNewRelatedDeclaration(Factory);
			AssertEquals(importer.MainAddress.PK, result.ImporterDocumentaryAddress.E2_OA_Address);
			AssertEquals(supplier.MainAddress.PK, result.SupplierDocumentaryAddress.E2_OA_Address);
		}

		public void TestCU_CU_ParentBillIsSynchonisedCorrectly()
		{
			var consol = Factory.New<ForwardingConsol>();
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = localPort.RL_Code;
			consol.JK_RL_NKDischargePort = foreignPort.RL_Code;
			consol.JK_MasterBillNum = "MB1";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_HouseBill = "HB1";
			shipment.JS_RL_NKOrigin = localPort.RL_Code;
			shipment.JS_RL_NKDestination = foreignPort.RL_Code;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment.PK;
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(2, declaration.Bills.Count);
			AssertEquals("MB1", declaration.JE_MasterBill);
			AssertEquals("HB1", declaration.JE_HouseBill);
			var primaryMasterBill = declaration.PrimaryMasterBill;
			var primaryHouseBill = declaration.PrimaryHouseBill;
			AssertCollectionContains(primaryMasterBill, declaration.Bills);
			AssertCollectionContains(primaryHouseBill, declaration.Bills);
			AssertEquals("CU_CU_ParentBill should have been populated", primaryMasterBill.PK, primaryHouseBill.CU_CU_ParentBill);
		}

		public void TestSettingJobDocAddressesDirectlyAndBypassingImporterSupplierWillUpdateMessageTypeAndPorts()
		{
			var dec1 = Factory.New<JobDeclaration>();
			AssertEquals("EXP", dec1.JE_MessageType);

			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, dec1.Country.Code));
			var someLocalOrg = Factory.New<OrgHeader>();
			someLocalOrg.OH_RL_NKClosestPort = localPort.Code;
			var localPalace = someLocalOrg.Addresses.AddNewMainAddress();

			dec1.ImporterDocumentaryAddress.E2_OA_Address = localPalace.PK;
			AssertEquals("IMP", dec1.JE_MessageType);
			AssertEquals(localPort.Code, dec1.JE_RL_NKPortOfArrival);

			var dec2 = Factory.New<JobDeclaration>();
			dec2.JE_MessageType = "IMP";
			dec2.SupplierDocumentaryAddress.E2_OA_Address = localPalace.PK;
			AssertEquals("EXP", dec2.JE_MessageType);
			AssertEquals(localPort.Code, dec2.JE_RL_NKOrigin);
		}

		public void TestICanBeImportOrExport()
		{
			var declaration = Factory.New<JobDeclaration>();
			var impl = declaration as ICanBeImportOrExport;
			AssertEquals("Level", "Header", impl.Level);

			AssertEquals("IsImport", declaration.IsImport, impl.IsImport);
			AssertEquals("IsExport", declaration.IsExport, impl.IsExport);
			AssertEquals("Country Code", declaration.CountryCode, impl.TrueCountryCode);
			AssertEquals("Data Grouping", declaration.GetDefaultDataGroupingCode(), impl.DataGroupingCode);
		}

		public void TestModeOfTransportAtTheBorder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = "";
			AssertEquals("declaration.ModeOfTransportAtTheBorder", "", declaration.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals("declaration.ModeOfTransportAtTheBorder", "4", declaration.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals("declaration.ModeOfTransportAtTheBorder", "1", declaration.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals("declaration.ModeOfTransportAtTheBorder", "3", declaration.ModeOfTransportAtTheBorder);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals("declaration.ModeOfTransportAtTheBorder", "2", declaration.ModeOfTransportAtTheBorder);
		}

		public void TestDeclarantAndTURNCode()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_Code = "DJC";
			company.GC_RN_NKCountryCode = "LV";
			var branch = company.Branches.AddNew();
			branch.GB_RL_NKHomePort = "LVRIX";
			branch.GB_Code = "DJC";
			Factory.Save();

			var gb = RefCountry.LoadFromCountryCode(Factory, Enterprise.Core.Constants.CountryCodes.UnitedKingdom);

			var turnCode1 = "GB43210987654321";
			var org1 = Factory.New<OrgHeader>();
			var orgCode1 = org1.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, turnCode1, gb);

			var turnCode2 = "GB12345678901234";
			var org2 = Factory.New<OrgHeader>();
			var orgCode2 = org2.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, turnCode2, gb);

			var turnCodeToOverideDefault = "GB98765432101234";
			var orgToOverideDefault = Factory.New<OrgHeader>();
			var orgAddressToOverideDefault = orgToOverideDefault.Addresses.AddNew();
			var orgCodeToOverideDefault = orgToOverideDefault.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, turnCodeToOverideDefault, gb);

			using (DisposableEnvironment.ForBranch(branch.PK.ToGuid()))
			{
				branch.GB_OH_OrgProxy = org1.PK;
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("declaration.Declarant", org1.MainAddress, declaration.Declarant);
				AssertEquals("declaration.DeclarantTurnCode", turnCode1, declaration.DeclarantTraderId);

				branch.GB_OH_OrgProxy = org2.PK;
				declaration = Factory.New<JobDeclaration>();
				AssertEquals("declaration.Declarant", org2.MainAddress, declaration.Declarant);
				AssertEquals("declaration.DeclarantTurnCode", turnCode2, declaration.DeclarantTraderId);

				branch.GB_OH_OrgProxy = ZGuid.Invalid;
				declaration = Factory.New<JobDeclaration>();
				AssertEquals("declaration.Declarant", null, declaration.Declarant);
				AssertEquals("declaration.DeclarantTurnCode", "", declaration.DeclarantTraderId);

				declaration.JE_GB = ZGuid.Invalid;
				AssertEquals("declaration.Declarant", null, declaration.Declarant);
				AssertEquals("declaration.DeclarantTurnCode", "", declaration.DeclarantTraderId);

				declaration.JE_OA_DeclarantAddress = orgAddressToOverideDefault.PK;
				AssertEquals("declaration.Declarant", orgAddressToOverideDefault, declaration.Declarant);
				AssertEquals("declaration.DeclarantTurnCode", turnCodeToOverideDefault, declaration.DeclarantTraderId);
			}
		}

		public void TestDeclarantAddressRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._1Self;
			AssertEquals("Declarannt address not required", false, declaration.IsDeclarantAddressRequired);
			declaration.JE_DeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertEquals("Declarannt address is required", true, declaration.IsDeclarantAddressRequired);
		}

		public void TestSettingVesselCalculatesBoxNationalityAtTheBorder()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "HELLO";
			vessel.RV_RN_NKCountryOfReg = Core.Constants.CountryCodes.Fiji;
			declaration.JE_VesselName = "HELLO";
			AssertEquals("FJ", declaration.JE_RN_NKTransportNationality);
		}

		[TestDate(2007, 3, 1)]
		public void TestUCR()
		{
			var jobDec = Factory.New<JobDeclaration>();
			jobDec.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789ABC");
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			Factory.Save();
			AssertEquals("Generating UCR",
							"7" + jobDec.Country.Code + "123456789ABC-" + jobDec.JE_DeclarationReference,
							jobDec.JE_UCR);

			GlbBranch.CurrentBranch.OrgProxy.CustomsCodes.RemoveAndDeleteAll();
			jobDec.JE_GoodsDescription = "goods";
			jobDec.JE_UCR = "AGENTREF";
			Factory.Save();
			AssertEquals("UCR should not have changes", "AGENTREF", jobDec.JE_UCR);
		}

		[TestDate(2009, 8, 9)]
		public void TestUCRWithDeclarantAndClientDucr()
		{
			var jobDec = Factory.New<JobDeclaration>();
			jobDec.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "123456789");
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			var declarant = Factory.New<OrgHeader>();
			declarant.OH_Code = "declarant1";
			jobDec.JE_OA_DeclarantAddress = declarant.MainAddress.PK;

			var vat = declarant.CustomsCodes.AddNew();
			vat.OK_CodeType = "VAT";
			vat.OK_CustomsRegNo = "987654321";

			var trn = declarant.CustomsCodes.AddNew();
			trn.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			trn.OK_CustomsRegNo = "987654321999";

			var party = Factory.NewWithValidTestData<OrgHeader>();
			var partyEori = party.CustomsCodes.AddNew();
			partyEori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			partyEori.OK_CustomsRegNo = "555666444222";
			partyEori.OK_RN_NKCodeCountry = jobDec.Country.Code;

			Factory.Save();
			var declarationReference = jobDec.JE_DeclarationReference;
			AssertEquals(
				"DUCR should come from the declarant, not the org, when the declarant is defined, and then should use the TRN in preference to the VAT. We check the OrgProxy and the VAT in the test called 'TestUCR()'",
				"9" + jobDec.Country.Code + "987654321999-" + declarationReference,
				jobDec.JE_UCR);

			jobDec.JE_OH_Supplier = party.PK;
			AssertEquals("Pre-req", false, jobDec.JE_UCRInfo.ReadOnly);
			jobDec.UseClientEoriForDucr = true;
			AssertEquals(string.Concat("9", jobDec.Country.Code, "555666444222-", declarationReference), jobDec.JE_UCR);
			jobDec.UseClientEoriForDucr = false;
			AssertEquals(string.Concat("9", jobDec.Country.Code, "987654321999-", declarationReference), jobDec.JE_UCR);
			jobDec.ClientReferenceForDucr = "POOP567890123456789";
			AssertEquals(string.Concat("9", jobDec.Country.Code, "987654321999-POOP567890123456789"), jobDec.JE_UCR);
			jobDec.ClientReferenceForDucr = ZString.Empty;
			AssertEquals(string.Concat("9", jobDec.Country.Code, "987654321999-", declarationReference), jobDec.JE_UCR);

			jobDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDec.JE_OH_Importer = party.PK;
			jobDec.JE_UCR = ZString.Empty;
			AssertEquals("Pre-req", false, jobDec.JE_UCRInfo.ReadOnly);
			jobDec.UseClientEoriForDucr = true;
			AssertEquals(string.Concat("9", jobDec.Country.Code, "555666444222-", declarationReference), jobDec.JE_UCR);
			jobDec.UseClientEoriForDucr = false;
			AssertEquals(string.Concat("9", jobDec.Country.Code, "987654321999-", declarationReference), jobDec.JE_UCR);
			jobDec.ClientReferenceForDucr = "POOP";
			AssertEquals(string.Concat("9", jobDec.Country.Code, "987654321999-POOP"), jobDec.JE_UCR);
			jobDec.ClientReferenceForDucr = ZString.Empty;
			AssertEquals(string.Concat("9", jobDec.Country.Code, "987654321999-", declarationReference), jobDec.JE_UCR);
		}

		public void TestUCRReadOnly()
		{
			var jobDec = Factory.New<JobDeclaration>();
			AssertEquals(false, jobDec.JE_UCRInfo.ReadOnly);
		}

		public void TestClientReferenceForDucr()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(ZString.Empty, dec.ClientReferenceForDucr);
			dec.ClientReferenceForDucr = "DANIEL";
			AssertEquals("DANIEL", dec.ClientReferenceForDucr);
			Factory.Save();
			var decReloaded = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals("DANIEL", decReloaded.ClientReferenceForDucr);
		}

		public void TestUseClientEoriForDucr()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals(false, dec.UseClientEoriForDucr);
			dec.UseClientEoriForDucr = true;
			AssertEquals(true, dec.UseClientEoriForDucr);
			Factory.Save();
			var decReloaded = new BusinessObjectFactory().Load<JobDeclaration>(dec.PK);
			AssertEquals(true, decReloaded.UseClientEoriForDucr);
		}

		public void TestUCRAllocationAfterSaveFailure()
		{
			var jobDec = Factory.New<JobDeclaration>();
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Import;

			Db.Connection.BeginTransaction();
			try
			{
				jobDec.OnSaving();
				Assert(!jobDec.JE_UCR.IsEmpty);
				jobDec.OnSaved(false);
				AssertEquals(ZString.Empty, jobDec.JE_UCR);
				Factory.Save();
				Assert(!jobDec.JE_UCR.IsEmpty);
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public void TestHouseBillsCollectionIsOfRightType()
		{
			var declaration = (JobDeclaration)GetNewBusinessObject();
			AssertEquals(typeof(BillCollection<Bill, JobDeclaration>), declaration.Bills.GetType());
		}

		public void TestLookupObjectIsCached()
		{
			var bizO = Factory.New<JobDeclarationForTest>();
			var firstLookup = bizO.Lookups;
			var secondLookup = bizO.Lookups;
			if (bizO.IsLookupsCachedInBase)
			{
				AssertEquals(secondLookup, firstLookup);
			}
			else
			{
				AssertNotEquals(secondLookup, firstLookup);
			}
		}

		public void TestDefaultValuesOnCreation()
		{
			var declaration = Factory.New<JobDeclaration>();

			AssertEquals("", declaration.JE_LocationOfGoods);
			AssertEquals("", declaration.SubLocation);
			AssertEquals("", declaration.JE_RN_NKTransportNationality);
			AssertEquals("", declaration.JE_TransportModeInland);
			AssertEquals(ZDateTime.Empty, declaration.JE_EntryAuthorisationDate);
			AssertEquals("", declaration.JE_PaymentMethod);
		}

		public void TestDefaultValuesForImport()
		{
			GlbDepartment.CurrentDepartment.GE_Import = true;
			GlbDepartment.CurrentDepartment.GE_Export = false;

			var declaration = Factory.New<JobDeclaration>();

			var expectedDeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertEquals(expectedDeclarantType, declaration.JE_DeclarantType);
			AssertEquals("", declaration.ZG_CTStatusID);
		}

		public void TestZG_CTStatusID_Captions()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("Caption is set correctly", "CT Status", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(declaration.ZG_CTStatusID)).Caption);
				AssertEquals("MediumCaption is set correctly", "CT Status", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(declaration.ZG_CTStatusID)).MediumCaption);
				AssertEquals("ShortCaption is set correctly", "CT Status", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(declaration.ZG_CTStatusID)).ShortCaption);
			});
		}

		/// <summary>
		/// System has multiple entry numbers - these numbers should come from the merged entry(s). We want to see the real entry number, not just (M)UCRs
		/// </summary>
		[TestDate(2009, 12, 11)]
		public void TestCustomsEntryNumbersAvailableToShipmentAreCustomsGeneratedNumbersNotJustUCRs()
		{
			var realEntryNumberFromCustoms = "191-12345679";
			var dec = Factory.New<JobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;

			AssertEquals(string.Empty, shipment.CustomsEntryNumber);

			shipment.ResetCusEntryNumbers();    // hacky...
			dec.CustomsEntryHeaders.AddNew();
			Factory.Save();

			var entry = Factory.Load<CusEntryHeader>(dec.CustomsEntryHeaders[0].PK);
			entry.EntryNumber = realEntryNumberFromCustoms;

			string answer = shipment.CustomsEntryNumber;
			AssertEquals("The ID returned to the shipment by GetValidCusEntryNumFilter() should give ONLY the real entry number", realEntryNumberFromCustoms, answer);
		}

		public void TestSetSupervisingOfficeFromDeclarant()
		{
			var dec = Factory.New<JobDeclaration>();
			Factory.Save();

			AssertEquals("Dec should have no SPOFF out of the box", ZGuid.Empty, dec.SupervisingOfficeDocAddress.E2_OA_Address);
			dec.SetSupervisingOfficeFromDeclarant();
			AssertEquals("Dec should have no SPOFF even after pressing the button, as declarant has none", ZGuid.Empty, dec.SupervisingOfficeDocAddress.E2_OA_Address);

			var declarant = Factory.New<OrgHeader>();
			var hmrc = Factory.New<OrgHeader>();
			hmrc.Addresses.AddNewMainAddress();
			hmrc.OH_Code = "HMRC";
			declarant.OH_Code = "DCDEC";
			dec.JE_OA_DeclarantAddress = declarant.Addresses.AddNewMainAddress().PK;
			Factory.Save();
			dec.SetSupervisingOfficeFromDeclarant();
			AssertEquals("Dec should have no SPOFF, declarant is defined but has no related party", ZGuid.Empty, dec.SupervisingOfficeDocAddress.E2_OA_Address);

			var relation = Factory.New<OrgRelatedParty>();
			relation.PR_OH_RelatedParty = hmrc.PK;
			relation.PR_OH_Parent = declarant.PK;
			relation.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;
			relation.PR_PartyType = RelatedPartyTypeList.Codes.CustomsOffice;
			Factory.Save();

			AssertEquals("Dec should have no SPOFF, declarant is defined and has a related party, but we ain't pressed the button yet", ZGuid.Empty, dec.SupervisingOfficeDocAddress.E2_OA_Address);
			dec.SetSupervisingOfficeFromDeclarant();
			AssertEquals("We have a declarant with a releated party, so pressing the button should set the dec's spoff", hmrc.MainAddress.PK, dec.SupervisingOfficeDocAddress.E2_OA_Address);
		}

		public void TestUNDGsAreDefaultedFromProduct()
		{
			var owner = Factory.New<OrgHeader>();
			owner.OH_IsConsignee = true;
			owner.OH_Code = "OWNER";
			var contact1 = owner.Contacts.AddNew();
			contact1.OC_ContactName = "BOB";
			var contact2 = owner.Contacts.AddNew();
			contact2.OC_ContactName = "JOE";

			var part = Enterprise.Customs.EU.Business.MasterFiles.OrgSupplierPart.New(Factory);
			part.OP_PartNum = "PARTZ234";
			part.OP_Desc = "Description";
			var relation = part.RelatedOrganisations.AddOrganisationIfNotExist(owner.PK, OrgPartRelation.RelationshipTypes.Both);
			var undg1 = part.UNDGs.TryGetOrCreate("3208A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			undg1.DI_DGFlashPoint = 10m;
			undg1.DI_OC_DGContact = contact1.PK;
			undg1.DI_TechnicalName = "TECH METALLIC SUBSTANCE";
			undg1.DI_MPMarinePollutant = YesNoList.Codes.Yes;
			Factory.Save();

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Importer = owner.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			AssertEquals(0, invoiceLine.UNDGs.Count);
			invoiceLine.JI_PartNo = "PARTZ234";
			AssertEquals(0, invoiceLine.UNDGs.Count);

			invoiceLine.JI_PartNo = "";
			AssertEquals(0, invoiceLine.UNDGs.Count);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			invoiceLine.JI_PartNo = "PARTZ234";
			AssertNoWarning(invoiceLine.JI_PartNoInfo, JobComInvoiceLineValidation.WarningPartHasMultipleUNDGRecords);
			AssertEquals(1, invoiceLine.UNDGs.Count);
			var undg = invoiceLine.UNDGs[0];
			AssertEquals("3208a", undg.UNDGSubstance.DG_Code);
			AssertEquals(10m, undg.DI_DGFlashPoint);
			AssertEquals(contact1.PK, undg.DI_OC_DGContact);
			AssertEquals("TECH METALLIC SUBSTANCE", undg.DI_TechnicalName);
			AssertEquals(YesNoList.Codes.Yes, undg.DI_MPMarinePollutant);

			invoiceLine.JI_PartNo = "";
			invoiceLine.UNDGs.DeleteAll();
			var undg2 = part.UNDGs.TryGetOrCreate("2015A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			undg2.DI_DGFlashPoint = -14m;
			undg2.DI_OC_DGContact = contact2.PK;
			Factory.Save();
			invoiceLine.JI_PartNo = "PARTZ234";
			AssertEquals(0, invoiceLine.UNDGs.Count);
			AssertHasWarning(invoiceLine.JI_PartNoInfo, JobComInvoiceLineValidation.WarningPartHasMultipleUNDGRecords);
		}

		public void TestDefaultValuesForBox_b14()
		{
			var jobDeclaration = Factory.New<JobDeclaration>();
			var localCountry = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, jobDeclaration.Country.Code);
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, localCountry.Code));

			var foreignCompany = Factory.New<OrgHeader>();
			foreignCompany.OH_Code = "Sid";
			foreignCompany.OH_RL_NKClosestPort = "AUSYD";
			var foreignCompanyAddress = foreignCompany.Addresses.AddNewMainAddress();
			foreignCompanyAddress.OA_Code = "A";

			var localCompany = Factory.New<OrgHeader>();
			localCompany.OH_Code = "Loc";
			localCompany.OH_RL_NKClosestPort = localPort.Code;
			var localCompanyAddress = localCompany.Addresses.AddNewMainAddress();
			localCompanyAddress.OA_Code = "B";

			var localDeclarant = Factory.New<OrgHeader>();
			localDeclarant.OH_Code = "Dec";
			localDeclarant.OH_RL_NKClosestPort = localPort.Code;
			var localDeclarantAddress = localDeclarant.Addresses.AddNewMainAddress();
			localDeclarantAddress.OA_Code = "C";
			var declarantEori = localDeclarant.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000", localCountry);

			jobDeclaration.JE_OA_DeclarantAddress = localDeclarantAddress.PK;

			CombineAssertions(() =>
			{
				jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = localCompanyAddress.PK;
				jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = foreignCompanyAddress.PK;
				var expectedDeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("Import, No EORI codes, repr still at default value", expectedDeclarantType, jobDeclaration.JE_DeclarantType);
				jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = foreignCompanyAddress.PK;
				jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = localCompanyAddress.PK;
				expectedDeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("Export, No EORI codes, repr still at default value", expectedDeclarantType, jobDeclaration.JE_DeclarantType);

				var localCompanyEori = localCompany.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321000", localCountry);
				jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = localCompanyAddress.PK;
				jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = foreignCompanyAddress.PK;
				expectedDeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("Import, No matching EORI codes, repr still default value", expectedDeclarantType, jobDeclaration.JE_DeclarantType);
				jobDeclaration.JE_DeclarantType = "XXX";
				jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = foreignCompanyAddress.PK;
				jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = localCompanyAddress.PK;
				expectedDeclarantType = RepresentationTypeList.Codes._2Direct;
				AssertEquals("Now export, set to unmatching parties", expectedDeclarantType, jobDeclaration.JE_DeclarantType);

				localCompanyEori.OK_CustomsRegNo = declarantEori.OK_CustomsRegNo;
				jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = localCompanyAddress.PK;
				jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = foreignCompanyAddress.PK;
				expectedDeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("Import, Setting parties that match declarant's EORI updates repr to SEL", expectedDeclarantType, jobDeclaration.JE_DeclarantType);

				jobDeclaration.JE_DeclarantType = "XXX";
				jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = foreignCompanyAddress.PK;
				jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = localCompanyAddress.PK;
				expectedDeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("Export, Setting parties to match declarant's EORI updates repr to SEL", expectedDeclarantType, jobDeclaration.JE_DeclarantType);

				jobDeclaration.JE_DeclarantType = "XXX";
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				AssertEquals("Pre-requisite, declarant now has no eori", "", jobDeclaration.DeclarantTraderId);
				AssertEquals("Declarant undefined, defaults to branch, which has no EORI; Repr unchanged by setting unmatching parties", "XXX", jobDeclaration.JE_DeclarantType);

				jobDeclaration.JE_OA_DeclarantAddress = localDeclarantAddress.PK;
				expectedDeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("Export, Setting declarant to match clients' EORI updates repr to SEL", expectedDeclarantType, jobDeclaration.JE_DeclarantType);

				jobDeclaration.JE_DeclarantType = "XXX";
				jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = localCompanyAddress.PK;
				jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = foreignCompanyAddress.PK;
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				jobDeclaration.JE_OA_DeclarantAddress = localDeclarantAddress.PK;
				expectedDeclarantType = RepresentationTypeList.Codes._1Self;
				AssertEquals("Import, Setting declarant to match clients' EORI updates repr to SEL", expectedDeclarantType, jobDeclaration.JE_DeclarantType);

				localDeclarant.CustomsCodes.RemoveAndDeleteAll();
				localCompany.CustomsCodes.RemoveAndDeleteAll();
				foreignCompany.CustomsCodes.RemoveAndDeleteAll();
				localDeclarantAddress.Header.CustomsCodes.RemoveAndDeleteAll();
				jobDeclaration.JE_DeclarantType = "YYY";
				jobDeclaration.JE_OH_Importer = Guid.Empty;
				jobDeclaration.JE_OH_Supplier = Guid.Empty;
				jobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = Guid.Empty;
				jobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = Guid.Empty;
				jobDeclaration.JE_OA_DeclarantAddress = ZGuid.Empty;
				AssertEquals("No orgs at all, should not explode", "YYY", jobDeclaration.JE_DeclarantType);
			});
		}

		public void TestAirContainerIsNotValidationError()
		{
			var declaration = GetJobDeclaration();
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			var container = declaration.CusContainers.AddNew();
			container.CO_FCL_LCL_AIR = Core.Constants.ContainerModes.ULD;
			container.CO_ContainerNumber = "AKL12345DL";
			AssertEquals(true, declaration.ContainersRequired);
			Assert("No validation when trying to add a container to an air job; neither about the presence of a container on an air job nor about check digits", !container.CO_ContainerNumberInfo.Notifications.Any());
		}

		public void TestContainersRequired()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertContainersRequireForAllTransportModes(false, declaration);
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
			AssertContainersRequireForAllTransportModes(true, declaration);
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			AssertContainersRequireForAllTransportModes(false, declaration);
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Bulk;
			AssertContainersRequireForAllTransportModes(false, declaration);
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Liquid;
			AssertContainersRequireForAllTransportModes(false, declaration);
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.RollOnRollOff;
			AssertContainersRequireForAllTransportModes(false, declaration);
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.ULD;
			AssertContainersRequireForAllTransportModes(true, declaration); // If this fails, check that Core.Constants.ContainerModes.IsContainerised() thinks that a ULD is containerised.  If not then someone may have monkeyed with it.
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Loose;
			AssertContainersRequireForAllTransportModes(false, declaration);
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.FCL;
			AssertContainersRequireForAllTransportModes(true, declaration);
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.LCL;
			AssertContainersRequireForAllTransportModes(true, declaration);
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.FTL;
			AssertContainersRequireForAllTransportModes(false, declaration);
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.LTL;
			AssertContainersRequireForAllTransportModes(false, declaration);

			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.Containerised;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.CusContainers.AddNew().CO_ContainerNumber = "AKL12345DL";
			Factory.Save();
			AssertEquals("Container not removed upon saving", 1, declaration.CusContainers.Count);
			declaration.JE_ContainerMode = Enterprise.Core.Constants.ContainerModes.BreakBulk;
			Factory.Save();
			AssertEquals("Container removed upon saving", 0, declaration.CusContainers.Count);
		}

		public override void TestSwitchFromSeaToAirWithContainersDereferencesAndDeletesContainers()
		{
			var jobDeclaration = GetJobDeclaration();
			jobDeclaration.FillWithValidTestData();
			jobDeclaration.JE_TransportMode = Core.Constants.TransportModes.Sea;
			var container = jobDeclaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT123";
			Factory.Save();
			jobDeclaration.JE_TransportMode = jobDeclaration.TransportModeAirCodeForTesting;
			Factory.Save();
			AssertEquals("Containers were deleted or rereferenced on change to Air; they should remain.", 1, jobDeclaration.CusContainers.Count);
		}

		public void TestSetDefaultValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			var expectedDeclarantType = RepresentationTypeList.Codes._2Direct;
			AssertEquals(expectedDeclarantType, declaration.JE_DeclarantType);
			AssertEquals(ShipmentTypeList.Codes.BasicDirect, declaration.ZG_ShipmentType);
			AssertEquals(declaration.Branch.OrgProxy.MainAddress.PK, declaration.JE_OA_DeclarantAddress);
		}

		public void TestPackingGroups()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<DeclarationLevelPackingGroupCollection>(dec.PackingGroups);
		}

		public void TestCloneSupportingDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var supportingDoc = declaration.SupportingDocuments.AddNew();
			supportingDoc.CSI_Code = "?";
			supportingDoc.CSI_Procedure = "A";

			var clonedDec = (JobDeclaration)new Customs.Business.JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			var expectedProcedureWhenCloning = "A";
			CombineAssertions(() =>
			{
				AssertEquals("?", clonedDec.SupportingDocuments[0].CSI_Code);
				AssertEquals("Test Value of CSI_Procedure when clone a Declaration", expectedProcedureWhenCloning, clonedDec.SupportingDocuments[0].CSI_Procedure);
			});
		}

		public void TestJE_RL_NKFinalDestination_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("[17] Destination", DataBoundResourceStrings.GetDataForProperty(declaration.JE_RL_NKFinalDestinationInfo).Caption);
		}

		public void TestFirstPortOfArrivalDefaultsWhenFinalDestinationIsSet()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_RL_NKFinalDestination = "GBLHR";
			AssertEquals("First Port of Arrival should default to the final destination", "GBLHR", declaration.JE_RL_NKPortOfFirstArrival);
		}

		public void TestFirstPortOfArrivalDoesNotDefaultWhenAlreadyPopulated()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_RL_NKPortOfFirstArrival = "GBFXT";
			declaration.JE_RL_NKFinalDestination = "GBLHR";
			AssertEquals("First Port of Arrival should not default to the final destination when its already been set", "GBFXT", declaration.JE_RL_NKPortOfFirstArrival);
		}

		public void TestCustomsOfficeRequirementHelper_ShouldNotShareBetweenJobs()
		{
			var dec1 = Factory.New<JobDeclaration>();
			var dec2 = Factory.New<JobDeclaration>();
			AssertNotEquals("CustomsOfficeRequirementHelpers from two jobs should not reference to the same one, even they are in the same factory.", dec1.CustomsOfficeRequirementHelper, dec2.CustomsOfficeRequirementHelper);
		}

		public void TestDeleteChildren()
		{
			var dec = Factory.New<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions.AddNew();
			dec.JE_MessageType = "IMP";
			cei.CEI_Style = "IFD";

			dec.SupportingDocuments.AddNew();
			dec.PreviousDocuments.AddNew();
			dec.AdditionalInfos.AddNew();

			Factory.Save();

			AssertEquals(3, Factory.Load<CusSupportingInfo>(new ZQuery(CusSupportingInfoSchema.CSI_ParentID, dec.PK)).Length);

			var anotherFactory = new BusinessObjectFactory();
			anotherFactory.Load<JobDeclaration>(dec.PK).Delete();

			AssertEquals(0, anotherFactory.Load<CusSupportingInfo>(new ZQuery(CusSupportingInfoSchema.CSI_ParentID, dec.PK)).Length);
		}

		public void TestWeOnlySupportSingleEntryDecsCosSoMuchDependsOnHavingASingleEntry()
		{
			// Remember, this is only talking about multiple CEH, not CEI.
			var dec = Factory.New<JobDeclarationWithPublicHasSplitEntriesCore>();
			AssertEquals("It's Not OK to change the EU declaration to support multiple entries, but there is much functionality to change. Don't forget that the report (DB function) [Report_GBCustomsEntryPayment] assumes single entries",
				false, dec.HasSplitEntriesForTest);
		}

		public void TestTradersOwnReferenceFullForBox7()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertEquals("", dec.TradersOwnReferenceFullForBox7);
			dec.JE_DeclarationReference = "B00069";
			var expectedTradersOwnReferenceFullForBox7 = "B00069";
			AssertEquals(expectedTradersOwnReferenceFullForBox7, dec.TradersOwnReferenceFullForBox7);
			dec.JE_OwnerRef = "POOP";
			AssertEquals("POOP", dec.TradersOwnReferenceFullForBox7);
			AssertEquals(21, dec.JE_OwnerRefInfo.MaxLength);
		}

		public void TestDefaultAgreedPlaceCode()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, parent: grouping);

			helper.CreateCusCodeType(Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "IncoTerm Key");
			helper.CreateNewOrGetExistingCusCodeList("EUN", Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.IncoTermKey, "1", "Test Code", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var dec = Factory.New<JobDeclaration>();
				AssertEquals("Precondition: EUD_AgreedPlaceCode", "", dec.EUD_AgreedPlaceCode);
				AssertEquals("Precondition: ZG_AgreedPlaceCode", "", dec.ZG_AgreedPlaceCode);

				dec.EUD_AgreedPlaceCode = "X";
				AssertEquals("Precondition: ZG_AgreedPlaceCode", "1", dec.ZG_AgreedPlaceCode);

				dec.ZG_AgreedPlaceCode = "2";
				dec.EUD_AgreedPlaceCode = "Y";
				AssertEquals("Precondition: ZG_AgreedPlaceCode", "2", dec.ZG_AgreedPlaceCode);
			}
		}

		public void TestGetNumericIncoTermModeCodeIncoTermAndFlux_NotUcc6()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, false))
			{
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					foreach (var incoterm in new[] { IncoTerms.ExWorks, IncoTerms.FreeAlongsideShip, IncoTerms.FreeOnBoard })
					{
						AssertEquals(incoterm, "3", declaration.GetNumericIncoTermModeCodeIncoTermAndFlux(incoterm));
					}
					foreach (var incoterm in new[] { IncoTerms.DeliveredAtPlaceUnloaded, IncoTerms.DeliveredAtPlace, IncoTerms.DeliveredDutyPaid })
					{
						AssertEquals(incoterm, "1", declaration.GetNumericIncoTermModeCodeIncoTermAndFlux(incoterm));
					}

					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					foreach (var incoterm in new[] { IncoTerms.ExWorks, IncoTerms.FreeAlongsideShip, IncoTerms.FreeOnBoard })
					{
						AssertEquals(incoterm, "1", declaration.GetNumericIncoTermModeCodeIncoTermAndFlux(incoterm));
					}
					foreach (var incoterm in new[] { IncoTerms.DeliveredAtPlaceUnloaded, IncoTerms.DeliveredAtPlace, IncoTerms.DeliveredDutyPaid })
					{
						AssertEquals(incoterm, "3", declaration.GetNumericIncoTermModeCodeIncoTermAndFlux(incoterm));
					}

					AssertEquals("CFR", ZString.Empty, declaration.GetNumericIncoTermModeCodeIncoTermAndFlux(IncoTerms.CostAndFreight));
				});
			}
		}

		public void TestGetNumericIncoTermModeCodeIncoTermAndFlux_Ucc6()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, true))
			{
				CombineAssertions(() =>
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					foreach (var incoterm in new[] { IncoTerms.ExWorks, IncoTerms.FreeAlongsideShip, IncoTerms.FreeOnBoard,
								IncoTerms.DeliveredAtPlaceUnloaded, IncoTerms.DeliveredAtPlace,
								IncoTerms.DeliveredDutyPaid, IncoTerms.CostAndFreight })
					{
						AssertEquals(incoterm, ZString.Empty, declaration.GetNumericIncoTermModeCodeIncoTermAndFlux(incoterm));
					}
				});
			}
		}

		public void TestJE_GoodsOriginDefault()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);

			var countryTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Spain, "EUSFT", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var europeTradeGroup = helper.CreateTradeGroup("EUN", "EUSFR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.AddCountry(countryTradeGroup, "AB", ZDate.BrettsBirthday, ZDate.Today.AddMonths(2));
			helper.AddCountry(europeTradeGroup, Core.Constants.CountryCodes.Spain, ZDate.BrettsBirthday, ZDate.Today.AddMonths(2));
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_RL_NKOrigin = "ABCDE";
			AssertEquals(Core.Constants.CountryCodes.Spain, dec.JE_GoodsOrigin);
			dec.JE_RL_NKOrigin = "EDCBA";
			AssertEquals("ED", dec.JE_GoodsOrigin);
			dec.JE_RL_NKOrigin = "ABCDE";
			AssertEquals(Core.Constants.CountryCodes.Spain, dec.JE_GoodsOrigin);
		}

		public void TestJE_GoodsDestinationDefault()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var grouping = helper.CreateNewOrGetExistingDataGrouping("EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Spain, parent: grouping);

			var countryTradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.Spain, "EUSFT", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			var europeTradeGroup = helper.CreateTradeGroup("EUN", "EUSFR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2));
			helper.AddCountry(countryTradeGroup, "AB", ZDate.BrettsBirthday, ZDate.Today.AddMonths(2));
			helper.AddCountry(europeTradeGroup, Core.Constants.CountryCodes.Spain, ZDate.BrettsBirthday, ZDate.Today.AddMonths(2));

			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			dec.JE_RL_NKFinalDestination = "ABCDE";
			AssertEquals(Core.Constants.CountryCodes.Spain, dec.JE_GoodsDestination);
			dec.JE_RL_NKFinalDestination = "EDCBA";
			AssertEquals("ED", dec.JE_GoodsDestination);
			dec.JE_RL_NKFinalDestination = "ABCDE";
			AssertEquals(Core.Constants.CountryCodes.Spain, dec.JE_GoodsDestination);
		}

		public void TestPackageMarksAndNumbersAlwaysRequiredValidationMessageCore()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertContains("Package marks are required", dec.PackageMarksAndNumbersAlwaysRequiredValidationMessage);
			AssertNotContains("shipment", dec.PackageMarksAndNumbersAlwaysRequiredValidationMessage);
			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			AssertContains("shipment", dec.PackageMarksAndNumbersAlwaysRequiredValidationMessage);
		}

		public void TestIsWarehoueNeeded()
		{
			var currentCountry = GlbCompany.CurrentCompany.Country.Code;
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var procedureA = helper.CreateRefCusProcedure(currentCountry, "A", "44", "44", "444", "Four", "EXP", group: "EFD");
			procedureA.ZZ6_IntoWarehouse = Universal.WarehouseMoveStatus.Codes.Yes;
			var procedureB = helper.CreateRefCusProcedure(currentCountry, "A", "55", "55", "555", "Five", "EXP", group: "EFD");
			procedureB.ZZ6_OutOfWarehouse = Universal.WarehouseMoveStatus.Codes.Yes;
			var procedureC = helper.CreateRefCusProcedure(currentCountry, "A", "66", "66", "666", "Six", "EXP", group: "EFD");
			Factory.Save();

			var dec = Factory.New<JobDeclaration>();
			var cei = dec.CustomsEntryInstructions.AddNew();
			cei.CEI_Style = "EFD";
			var invoiceHeader = dec.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			AssertEquals(false, dec.IsWarehouseNeeded);

			invoiceLine.JI_Procedure = procedureA.FullCodeCurrentPlusPreviousPlusConcession;
			AssertEquals(true, dec.IsWarehouseNeeded);

			invoiceLine.JI_Procedure = procedureB.FullCodeCurrentPlusPreviousPlusConcession;
			AssertEquals(true, dec.IsWarehouseNeeded);

			invoiceLine.JI_Procedure = procedureC.FullCodeCurrentPlusPreviousPlusConcession;
			AssertEquals(false, dec.IsWarehouseNeeded);
		}

		public void TestCustomsGuarantee()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertNull(declaration.CustomsGuarantee);
		}

		public void TestGetNumericIncoTermModeCodeFromWtgCode()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.France))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				AssertEquals("1", declaration.GetNumericIncoTermModeCodeFromWtgCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.THS));
				AssertEquals("2", declaration.GetNumericIncoTermModeCodeFromWtgCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OTH));
				AssertEquals("3", declaration.GetNumericIncoTermModeCodeFromWtgCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OUT));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Turkey))
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				AssertEquals("3", declaration.GetNumericIncoTermModeCodeFromWtgCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.THS));
				AssertEquals("2", declaration.GetNumericIncoTermModeCodeFromWtgCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OTH));
				AssertEquals("1", declaration.GetNumericIncoTermModeCodeFromWtgCode(OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OUT));
			}
		}

		public void TestDefaultINCOFromOrgLinkAddIncoTermModeAndPlace()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.Poland))
			{
				var currentPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				var otherCountryFilter = new ZQuery(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
				var otherCountry = Factory.LoadTop1<RefCountry>(otherCountryFilter);
				var otherUnloco1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, otherCountry.RN_Code));
				otherCountryFilter.AddToFilter(RefCountrySchema.RN_Code, SQLComparisonOperator.NotEqual, otherCountry.RN_Code);

				var consignee = OrgHeader.New(Factory);
				consignee.OH_RL_NKClosestPort = otherUnloco1.RL_Code;
				var consignor = OrgHeader.New(Factory);
				consignor.MiscServ.OM_EXDefaultIncoTerm = "321";

				var dec = Factory.NewWithValidTestData<JobDeclaration>();
				dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				dec.JE_MessageType = MessageTypeList.Codes.Import;

				var link1 = consignee.SupplierLinks.AddNew(consignor);
				link1.OL_RN_NKImporterCountry = otherCountry.RN_Code;
				var link1TrnModeAir = link1.OrgSupBuyLinkTrnModes.AddNew();
				link1TrnModeAir.PF_IncoTermMode = OrgSupBuyLinkTrnModeCodeDescriptionPairList.Codes.OUT;
				link1TrnModeAir.PF_IncoTermPlace = "testPlace";
				link1TrnModeAir.PF_IncoTerm = "EXW";
				link1TrnModeAir.PF_TransportMode = dec.TransportModeAirCodeForTesting;
				dec.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded;
				AssertEquals("Defaulted Inco Term", Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, dec.JE_ShipmentIncoTerm);
				AssertEquals("Defaulted Inco Term Place", "", dec.JE_ShipmentIncoTermPlace);
				AssertEquals("Defaulted Inco Term Mode", "1", dec.ZG_AgreedPlaceCode);

				dec.JE_OH_Importer = consignee.PK;
				dec.JE_OH_Supplier = consignor.PK;
				dec.JE_MessageType = JobMessageTypeList.Codes.Import;
				dec.JE_TransportMode = dec.TransportModeAirCodeForTesting;
				dec.JE_RL_NKFinalDestination = currentPort.RL_Code;
				AssertEquals("Defaulted Inco Term", "EXW", dec.JE_ShipmentIncoTerm);
				AssertEquals("Defaulted Inco Term Place", "testPlace", dec.JE_ShipmentIncoTermPlace);
				AssertEquals("Defaulted Inco Term Mode", "3", dec.ZG_AgreedPlaceCode);

				dec.JE_ShipmentIncoTerm = Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded;
				AssertEquals("Defaulted Inco Term", Core.Constants.IncoTerms.DeliveredAtPlaceUnloaded, dec.JE_ShipmentIncoTerm);
				AssertEquals("Defaulted Inco Term Place", "testPlace", dec.JE_ShipmentIncoTermPlace);
				AssertEquals("Defaulted Inco Term Mode", "1", dec.ZG_AgreedPlaceCode);
			}
		}

		public void TestIncotermModeValueDependOnJE_MessageType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.France))
			{
				var dec = Factory.NewWithValidTestData<JobDeclaration>();

				dec.JE_MessageType = MessageTypeList.Codes.Import;
				dec.JE_ShipmentIncoTerm = "FOB";
				AssertEquals("Defaulted Inco Term Mode", "3", dec.ZG_AgreedPlaceCode);

				dec.JE_ShipmentIncoTerm = "FAS";

				dec.JE_MessageType = MessageTypeList.Codes.Export;
				dec.JE_ShipmentIncoTerm = "FOB";
				AssertEquals("Defaulted Inco Term Mode", "1", dec.ZG_AgreedPlaceCode);
			}
		}

		public void TestDV1DetailsSupport()
		{
			foreach (var dv1Enabled in new[] { true, false })
			{
				var dec = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(dec, nameof(DeclarationConfiguration.DV1DetailsSupport) + "Core", dv1Enabled))
				{
					AssertEquals($"DeclarationConfiguration.DV1DetailsSupport = {dv1Enabled}", dv1Enabled, dec.DV1DetailsSupport);
				}
			}
		}

		public void TestIPreviousDocumentsProviderMembers()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.SupportingDocuments.AddNew();

			var previousDocumentsProvider = (IPreviousDocumentsProvider)declaration;

			AssertNotNull("IPreviousDocumentsProvider.PreviousDocuments must be not null", previousDocumentsProvider.PreviousDocuments);
			AssertSame("IPreviousDocumentsProvider.PreviousDocuments must be the same of PreviousDocuments", declaration.PreviousDocuments, previousDocumentsProvider.PreviousDocuments);
		}

		public void TestSetupDefermentPartyDocAddress()
		{
			var dec = Factory.New<JobDeclarationForDefermentPartyDocAddressTest>();
			var dec2 = Factory.New<JobDeclarationForDefermentPartyDocAddressTest>();
			AssertEquals("Initially the event handler should not be triggerd.", false, dec.EventHandlerTriggered);

			var orgHeaderDeferment = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderDeferment.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
			OrgAddress addressDeferment = Factory.New<OrgAddress>();
			addressDeferment.OA_Code = "AAA";
			addressDeferment.OA_OH = orgHeaderDeferment.PK;
			addressDeferment.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
			dec.DefermentPartyDocAddress.OrganisationPK = orgHeaderDeferment.PK;

			Assert("After initially setting the doc address, the event handler should be triggerd.", dec.EventHandlerTriggered);

			dec.EventHandlerTriggered = false;
			dec.DefermentPartyDocAddress.E2_ParentID = dec2.PK;

			Assert("After changing the parent of the doc address, the event handler should be triggerd.", dec.EventHandlerTriggered);
			dec.EventHandlerTriggered = false;

			var orgHeaderDeferment2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderDeferment2.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Belgium;
			OrgAddress addressDeferment2 = Factory.New<OrgAddress>();
			addressDeferment2.OA_Code = "BBB";
			addressDeferment2.OA_OH = orgHeaderDeferment.PK;
			addressDeferment2.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
			dec.DefermentPartyDocAddress.OrganisationPK = orgHeaderDeferment2.PK;
			Assert("After changing the organisation, the event handler should be triggerd.", dec.EventHandlerTriggered);
		}

		public void TestEmptyInvoiceLineTaxTypeChangingMessageTypeFromImport()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var invoice1 = declaration.Invoices.AddNew();
			invoice1.InvoiceLines.AddNew();
			invoice1.InvoiceLines.AddNew();

			var invoice2 = declaration.Invoices.AddNew();
			invoice2.InvoiceLines.AddNew();

			SetInvoiceLinesTaxType(invoice1, "VAT");
			SetInvoiceLinesTaxType(invoice2, "VEX");

			declaration.JE_MessageType = MessageTypeList.Codes.Export;

			AssertEquals("Changing MessageType from IMP, JI_ZZF_NKTaxType for All Invoice Lines must be empty", true, declaration.InvoiceLines.Cast<JobComInvoiceLine>().All(x => x.JI_ZZF_NKTaxType.IsEmpty));

			void SetInvoiceLinesTaxType(JobComInvoiceHeader invoice, string taxType) => invoice.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(x => x.JI_ZZF_NKTaxType = "VAT");
		}

		public void TestRepresentativeOrgAddress()
		{
			var declaration = Factory.New<JobDeclaration>();
			var orgHeader = Factory.New<OrgHeader>();

			declaration.JE_OA_Representative = ZGuid.Empty;
			AssertNull(declaration.RepresentativeOrgAddress);

			declaration.JE_OA_Representative = orgHeader.MainAddress.PK;
			AssertEquals(orgHeader.MainAddress.PK, declaration.RepresentativeOrgAddress.PK);
		}

		public void TestEmptyGuaranteesIfNecessaryChangingMessageType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";

			ConfigurationTestHelper.ClearDeclarationConfiguration(declaration);

			var invoiceLineConfigurationMock = new Mock<InstructionConfiguration>();
			invoiceLineConfigurationMock.Protected()
				.Setup<ZBool>("GuaranteesSupportCore", ItExpr.IsAny<JobDeclaration>(), ItExpr.IsAny<CusEntryInstruction>())
				.Returns(ZBool.True);

			using (ConfigurationTestHelper.TemporarilySetConfiguration_Declaration(Factory, "GetNewInstructionConfiguration", invoiceLineConfigurationMock.Object, null))
			{
				var entryInstructionCollection = declaration.CustomsEntryInstructions;

				var entryInstructionH1 = entryInstructionCollection.AddNew();
				entryInstructionH1.CEI_Style = "H1";
				entryInstructionH1.Guarantees.AddNew();
				entryInstructionH1.Guarantees.AddNew();

				var entryInstructionH2 = entryInstructionCollection.AddNew();
				entryInstructionH2.CEI_Style = "H2";
				entryInstructionH2.Guarantees.AddNew();

				declaration.JE_MessageType = "EXP";
				CombineAssertions(() =>
				{
					AssertEquals("Entry Instruction H1 Guarantees Count", 0, entryInstructionH1.Guarantees.Count);
					AssertEquals("Entry Instruction H2 Guarantees Count", 0, entryInstructionH2.Guarantees.Count);
				});
			}
		}

		public void TestJE_LocationOfGoods_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("[30] Goods Location", DataBoundResourceStrings.GetDataForProperty(declaration.JE_LocationOfGoodsInfo).Caption);
		}

		public void TestJE_ShipmentIncoTerm_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("[20.1] Incoterm", DataBoundResourceStrings.GetDataForProperty(declaration.JE_ShipmentIncoTermInfo).Caption);
		}

		public void TestJE_ShipmentIncoTermPlace_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("[20.2] Place", DataBoundResourceStrings.GetDataForProperty(declaration.JE_ShipmentIncoTermPlaceInfo).Caption);
		}

		public void TestJE_TotalWeight_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var resourceStringDataAttribute = declaration.JE_TotalWeightInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Total Weight", resourceStringDataAttribute.Caption);
				AssertEquals("Weight", resourceStringDataAttribute.ShortCaption);
			});
		}

		public void TestJE_TotalVolume_Caption()
		{
			var declaration = Factory.New<JobDeclaration>();
			var resourceStringDataAttribute = declaration.JE_TotalVolumeInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Total Volume", resourceStringDataAttribute.Caption);
				AssertEquals("Volume", resourceStringDataAttribute.MediumCaption);
				AssertEquals("Vol", resourceStringDataAttribute.ShortCaption);
			});
		}

		public void TestEUD_AgreedPlaceCodeResourceStringData()
		{
			var resourceStringData = Factory.New<JobDeclaration>().EUD_AgreedPlaceCodeInfo.GetAttribute<ResourceStringDataAttribute>();
			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Incoterm Place Code", resourceStringData.Caption);
				AssertEquals("FullDescription", "Incoterm Place Code: insert a Country (2 chars) or an UNLOCO (5 chars)", resourceStringData.FullDescription);
				AssertEquals("MediumCaption", "Inco. Place Code", resourceStringData.MediumCaption);
				AssertEquals("ShortCaption", "Inco. Place Code", resourceStringData.ShortCaption);
			});
		}

		public void TestEntryStyle_CalculateBasedOnFallbackProcedure_Import()
		{
			PrepareForCalculatingEntryStyleBasedOnFallBackProcedure(out var orgHeaderLV, out var orgHeaderIT, out var orgHeaderCH, out var orgHeaderGF, out var orgHeaderGB, out var orgHeaderPL);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				CombineAssertions(() =>
				{
					var dec = Factory.New<JobDeclaration>();
					dec.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals("Supplier empty => OriginCountry null => EntryStyleForInwardProcessingVATPayment (Import)", "IM", dec.JE_EntryStyle);

					dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderLV.PK;
					AssertEquals("Supplier LV => originCountryCode = declarationCountryCode => => EntryStyleForInwardProcessingVATPayment (Import)", "IM", dec.JE_EntryStyle);

					dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderIT.PK;
					AssertEquals("Supplier IT => Member of EU => EntryStyleForInwardProcessingVATPayment (Import)", "IM", dec.JE_EntryStyle);

					dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderGF.PK;
					AssertEquals("Supplier GF => Member of EUSFT => ImportFromSpecialTerritory", "CO", dec.JE_EntryStyle);

					dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderPL.PK;
					AssertEquals("Supplier PL => Member of EUSFR and member of EU => ImportFromSpecialTerritory", "CO", dec.JE_EntryStyle);

					dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderGB.PK;
					AssertEquals("Supplier GB => Member of EUSFR but not member of EU => Default EntrySytel (Import)", "IM", dec.JE_EntryStyle);

					dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderCH.PK;
					AssertEquals("Non UCC6: Supplier CH => Member of EUCTP and not member of EU => GetEntrySubStyleForCommonTransit", "EU", dec.JE_EntryStyle);

					dec.SupplierDocumentaryAddress.Delete();
					AssertEquals("Supplier deleted => no change to EntryStyle", "EU", dec.JE_EntryStyle);

					using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
					{
						dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderCH.PK;
						AssertEquals("UCC6: Supplier CH => Member of EUCTP and not member of EU => Default EntryStyle", "IM", dec.JE_EntryStyle);
					}

					dec.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
					AssertEquals("MiscellaneousCustoms => no change to EntryStyle", "IM", dec.JE_EntryStyle);
				});
			}
		}

		public void TestEntryStyle_CalculateBasedOnFallbackProcedure_Export()
		{
			PrepareForCalculatingEntryStyleBasedOnFallBackProcedure(out var orgHeaderLV, out var orgHeaderIT, out var orgHeaderCH, out var orgHeaderGF, out var orgHeaderGB, out var orgHeaderPL);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Latvia))
			{
				CombineAssertions(() =>
				{
					var dec = Factory.New<JobDeclaration>();
					dec.JE_MessageType = MessageTypeList.Codes.Export;
					AssertEquals("Importer empty => OriginCountry null => EntryStyleForInwardProcessingVATPayment (Export)", "EX", dec.JE_EntryStyle);

					dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderLV.PK;
					AssertEquals("Importer LV => originCountryCode = declarationCountryCode => => EntryStyleForInwardProcessingVATPayment (Export)", "EX", dec.JE_EntryStyle);

					dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderIT.PK;
					AssertEquals("Importer IT => Member of EU => EntryStyleForInwardProcessingVATPayment (Export)", "EX", dec.JE_EntryStyle);

					dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderGF.PK;
					AssertEquals("Importer GF => Member of EUSFT => ImportFromSpecialTerritory", "CO", dec.JE_EntryStyle);

					dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderPL.PK;
					AssertEquals("Importer PL => Member of EUSFR and member of EU => ImportFromSpecialTerritory", "CO", dec.JE_EntryStyle);

					dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderGB.PK;
					AssertEquals("Importer GB => Member of EUSFR but not member of EU => Default EntrySytel (Import)", "EX", dec.JE_EntryStyle);

					dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderCH.PK;
					AssertEquals("Non UCC6: Importer CH => Member of EUCTP and not member of EU => GetEntrySubStyleForCommonTransit", "EU", dec.JE_EntryStyle);

					dec.ImporterDocumentaryAddress.Delete();
					AssertEquals("Importer deleted => no change to EntryStyle", "EU", dec.JE_EntryStyle);

					using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
					{
						dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderCH.PK;
						AssertEquals("UCC6: Importer CH => Member of EUCTP and not member of EU => Default EntryStyle", "EX", dec.JE_EntryStyle);
					}

					dec.JE_MessageType = MessageTypeList.Codes.MiscellaneousCustoms;
					AssertEquals("MiscellaneousCustoms => no change to EntryStyle", "EX", dec.JE_EntryStyle);
				});
			}
		}

		public void TestEntryStyle_CalculateBasedOn15And17CodeTypes_Import()
		{
			var latviaCountryCode = Core.Constants.CountryCodes.Latvia;
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO15, "CO15");
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, "IM15");
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "EU15");
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO15, "AE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO15, "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_IM15, "MD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "LA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU15, "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var orgHeaderAE = factory.NewWithValidTestData<OrgHeader>();
			orgHeaderAE.OH_RL_NKClosestPort = "AE";
			var orgHeaderMD = factory.NewWithValidTestData<OrgHeader>();
			orgHeaderMD.OH_RL_NKClosestPort = "MD";
			var orgHeaderLA = factory.NewWithValidTestData<OrgHeader>();
			orgHeaderLA.OH_RL_NKClosestPort = "LA";
			var orgHeaderJP = factory.NewWithValidTestData<OrgHeader>();
			orgHeaderJP.OH_RL_NKClosestPort = "JP";
			var orgHeaderAU = factory.NewWithValidTestData<OrgHeader>();
			orgHeaderAU.OH_RL_NKClosestPort = "AU";

			factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(latviaCountryCode))
			{
				CombineAssertions(() =>
				{
					var dec = factory.New<JobDeclaration>();
					dec.JE_MessageType = MessageTypeList.Codes.Import;
					AssertEquals("Supplier empty => Default EntryStyle (Import)", "IM", dec.JE_EntryStyle);

					dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderAE.PK;
					AssertEquals("Supplier AE => Contained in 'CO15'", "CO", dec.JE_EntryStyle);

					dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderMD.PK;
					AssertEquals("Supplier MD => contained in 'IM15'", "IM", dec.JE_EntryStyle);

					dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderLA.PK;
					AssertEquals("Supplier LA => contained in 'EU15'", "EU", dec.JE_EntryStyle);

					dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderJP.PK;
					AssertEquals("Supplier JP => not contained in any list => Default EntrySytle (Import)", "IM", dec.JE_EntryStyle);

					dec.JE_EntryStyle = string.Empty;
					dec.SupplierDocumentaryAddress.OrganisationPK = orgHeaderAU.PK;
					AssertEquals("Supplier AU => contained in 'EU15' and 'CO15' => Default EntrySytle (Import)", "IM", dec.JE_EntryStyle);
				});
			}
		}

		public void TestEntryStyle_CalculateBasedOn15And17CodeTypes_Export()
		{
			var latviaCountryCode = Core.Constants.CountryCodes.Latvia;
			var factory = new BusinessObjectFactory();
			var helper = new UniversalReferenceTestDataHelper(factory);

			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "CO17");
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "EX17");
			helper.CreateCusCodeType(UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "EU17");
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "KP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_CO17, "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EX17, "BN", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "TJ", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeList(latviaCountryCode, UniversalReferenceConstants.RefCusCodeListType.Code.Code_EU17, "AU", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			var orgHeaderKP = factory.NewWithValidTestData<OrgHeader>();
			orgHeaderKP.OH_RL_NKClosestPort = "KP";
			var orgHeaderBN = factory.NewWithValidTestData<OrgHeader>();
			orgHeaderBN.OH_RL_NKClosestPort = "BN";
			var orgHeaderTJ = factory.NewWithValidTestData<OrgHeader>();
			orgHeaderTJ.OH_RL_NKClosestPort = "TJ";
			var orgHeaderJP = factory.NewWithValidTestData<OrgHeader>();
			orgHeaderJP.OH_RL_NKClosestPort = "JP";
			var orgHeaderAU = factory.NewWithValidTestData<OrgHeader>();
			orgHeaderAU.OH_RL_NKClosestPort = "AU";

			factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(latviaCountryCode))
			{
				CombineAssertions(() =>
				{
					var dec = factory.New<JobDeclaration>();
					dec.JE_MessageType = MessageTypeList.Codes.Export;
					AssertEquals("Importer empty => Default EntryStyle (Export)", "EX", dec.JE_EntryStyle);

					dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderKP.PK;
					AssertEquals("Importer CH => Contained in 'CO17'", "CO", dec.JE_EntryStyle);

					dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderBN.PK;
					AssertEquals("Importer BN => Contained in 'EX17'", "EX", dec.JE_EntryStyle);

					dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderTJ.PK;
					AssertEquals("Non UCC6: Importer TJ => Contained in 'EU17'", "EU", dec.JE_EntryStyle);

					using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(dec, true))
					{
						dec.ImporterDocumentaryAddress.Delete();
						dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderTJ.PK;
						AssertEquals("UCC6: Importer TJ => Contained in 'EU17' => Default EntrySytle (Export)", "EX", dec.JE_EntryStyle);
					}

					dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderJP.PK;
					AssertEquals("SuImporterpplier JP => not contained in any list => Default EntrySytle (Import)", "EX", dec.JE_EntryStyle);

					dec.JE_EntryStyle = string.Empty;
					dec.ImporterDocumentaryAddress.OrganisationPK = orgHeaderAU.PK;
					AssertEquals("Importer AU => contained in 'EU17' and 'CO17' => Default EntrySytle (Export)", "EX", dec.JE_EntryStyle);
				});
			}
		}

		public void TestIsEntryStyleExportToSpecialTerritory()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Export;
			CombineAssertions(() =>
			{
				declaration.JE_EntryStyle = "EX";
				AssertEquals("When JE_EntryStyle = EX, IsEntryStyleExportToSpecialTerritory", false, declaration.IsEntryStyleExportToSpecialTerritory);

				declaration.JE_EntryStyle = "CO";
				AssertEquals("When JE_EntryStyle = CO, IsEntryStyleExportToSpecialTerritory", true, declaration.IsEntryStyleExportToSpecialTerritory);
			});
		}

		public void TestIsSecurityAllowed()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = "EXP";
				AssertEquals("For EXP, IsSecurityAllowed", true, declaration.IsSecurityAllowed());

				declaration.JE_MessageType = "IMP";
				AssertEquals("For IMP, IsSecurityAllowed", false, declaration.IsSecurityAllowed());
			}

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: false))
			{
				declaration.JE_MessageType = "EXP";
				AssertEquals("For Non UCC6 EXP, IsSecurityAllowed", false, declaration.IsSecurityAllowed());
			}
		}

		public void TestEUD_AgreedPlaceCodeValidationSupport()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals($"We need delete the prperty and test when {nameof(JobEUDeclaration)} is removed", true, declaration.EUD_AgreedPlaceCodeValidationSupport);
		}

		public void TestICusGoodsLocationTypeSupporter()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Italy))
			{
				var declaration = Factory.New<JobDeclaration>();
				AssertEquals("Enterprise.Customs.IT.Business.Declaration.CusGoodsLocation", (declaration as ICusGoodsLocationTypeSupporter).GoodsLocationType.FullName);
			}
		}

		public void TestZG_AgreedPlaceCodeValidationSupport()
		{
			var declaration = Factory.New<JobDeclaration>();
			CombineAssertions(() =>
			{
				AssertEquals("Default value true", true, declaration.ZG_AgreedPlaceCodeValidationSupport);
				declaration = Factory.New<JobDeclarationForTest>();
				AssertEquals("No validation for ZG_AgreedPlaceCode in test-object", false, declaration.ZG_AgreedPlaceCodeValidationSupport);
			});
		}

		public void TestSetDefaultInlandTransportCodeIfRequired()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.TransportMeansDependencyForTest = EUCommonConstants.TransportModeSource.InlandTransportMode;
			CombineAssertions("Test cases when JE_TransportMeans depends on inland transport mode", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
					AssertEquals("JE_TransportMeans should not be defaulted when Ucc6 Export is disabled.", ZString.Empty, declaration.JE_TransportMeans);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
					AssertEquals("JE_TransportMeans should not be defaulted when Ucc6 Import is disabled.", ZString.Empty, declaration.JE_TransportMeans);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
					AssertEquals("JE_TransportMeans should be defaulted when is enabled.", "40", declaration.JE_TransportMeans);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
					AssertEquals("JE_TransportMeans should be defaulted when Ucc6 Export is enabled.", "30", declaration.JE_TransportMeans);
				}
			});

			declaration.TransportMeansDependencyForTest = EUCommonConstants.TransportModeSource.TransportModeAtBorder;
			declaration.JE_TransportMeans = ZString.Empty;
			CombineAssertions("Test cases when JE_TransportMeans depends on declaration transport mode", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					declaration.JE_TransportMode = TransportTypeList.Codes.Air;
					AssertEquals("JE_TransportMeans should not be defaulted when Ucc6 Export is disabled.", ZString.Empty, declaration.JE_TransportMeans);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					declaration.JE_TransportMode = TransportTypeList.Codes.Road;
					AssertEquals("JE_TransportMeans should not be defaulted when Ucc6 Import is disabled.", ZString.Empty, declaration.JE_TransportMeans);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					declaration.JE_TransportMode = TransportTypeList.Codes.Air;
					AssertEquals("JE_TransportMeans should be defaulted when is enabled.", "40", declaration.JE_TransportMeans);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					declaration.JE_TransportMode = TransportTypeList.Codes.Road;
					AssertEquals("JE_TransportMeans should be defaulted when Ucc6 Export is enabled.", "30", declaration.JE_TransportMeans);
				}
			});

			declaration.TransportMeansDependencyForTest = EUCommonConstants.TransportModeSource.None;
			declaration.JE_TransportMeans = ZString.Empty;
			CombineAssertions("Test cases when JE_TransportMeans doesn't depend on any transport mode", () =>
			{
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", false))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					declaration.JE_TransportMode = TransportTypeList.Codes.Air;
					declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
					AssertEquals("JE_TransportMeans should not be defaulted calculated when TransportMeansDependency is None.", ZString.Empty, declaration.JE_TransportMeans);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					declaration.JE_TransportMode = TransportTypeList.Codes.Road;
					declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
					AssertEquals("JE_TransportMeans should not be defaulted calculated when TransportMeansDependency is None.", ZString.Empty, declaration.JE_TransportMeans);
				}

				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
				{
					declaration.JE_MessageType = MessageTypeList.Codes.Export;
					declaration.JE_TransportMode = TransportTypeList.Codes.Air;
					declaration.JE_TransportModeInland = TransportTypeList.Codes.Air;
					AssertEquals("JE_TransportMeans should not be defaulted calculated when TransportMeansDependency is None.", ZString.Empty, declaration.JE_TransportMeans);

					declaration.JE_MessageType = MessageTypeList.Codes.Import;
					declaration.JE_TransportMode = TransportTypeList.Codes.Road;
					declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;
					AssertEquals("JE_TransportMeans should not be defaulted calculated when TransportMeansDependency is None.", ZString.Empty, declaration.JE_TransportMeans);
				}
			});
		}

		public void TestTransportModeValueForTransportMeans()
		{
			var declarationMock = Factory.NewMoq<JobDeclaration>();
			var declaration = declarationMock.Object;
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			declaration.JE_TransportModeInland = TransportTypeList.Codes.Road;

			declarationMock.Protected().Setup<EUCommonConstants.TransportModeSource>("TransportMeansDependencyCore").Returns(EUCommonConstants.TransportModeSource.TransportModeAtBorder);
			AssertEquals("Value for TransportMeans calculation should be JE_TransportMode when Transport Means Dependency is Transport Mode at Border.", TransportTypeList.Codes.Air, declaration.TransportModeValueForTransportMeans);

			declarationMock.Protected().Setup<EUCommonConstants.TransportModeSource>("TransportMeansDependencyCore").Returns(EUCommonConstants.TransportModeSource.InlandTransportMode);
			AssertEquals("Value for TransportMeans calculation should be JE_TransportModeInland when Transport Mean sDependency is Inland Transport Mode.", TransportTypeList.Codes.Road, declaration.TransportModeValueForTransportMeans);

			declarationMock.Protected().Setup<EUCommonConstants.TransportModeSource>("TransportMeansDependencyCore").Returns(EUCommonConstants.TransportModeSource.None);
			AssertEquals("Value for TransportMeans calculation should be empty when Transport Means Dependency is None.", ZString.Empty, declaration.TransportModeValueForTransportMeans);
		}

		public void TestGoodsLocationDescription_Caption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			AssertEquals("Location of Goods", DataBoundResourceStrings.GetDataForProperty(typeof(JobDeclaration), nameof(declaration.GoodsLocationDescription)).Caption);
		}

		public void TestGoodsLocationDescription()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			CombineAssertions(() =>
			{
				AssertNull("GoodsLocation doesn't exist", Customs.Business.CusGoodsLocation.Load<CusGoodsLocation>(declaration, CusGoodsLocationUseList.Codes.Declaration));
				AssertEquals("GoodsLocationDescription empty when there's no GoodsLocation", ZString.Empty, declaration.GoodsLocationDescription);

				var goodsLocation = declaration.GoodsLocation;
				goodsLocation.CGL_Qualifier = CusGoodsLocationQualifierList.Codes.UnLocode;
				AssertEquals("GoodsLocationDescription when there's GoodsLocation", CusGoodsLocationQualifierList.Codes.UnLocode, declaration.GoodsLocationDescription);
			});
		}

		public void TestGoodsLocation()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var goodsLocation = declaration.GoodsLocation;
			CombineAssertions(() =>
			{
				AssertEquals("CGL_ParentID", declaration.PK, goodsLocation.CGL_ParentID);
				AssertEquals("CGL_ParentTableCode", JobDeclarationSchema.Constants.Prefix, goodsLocation.CGL_ParentTableCode);
				AssertEquals("CGL_LocationUse", CusGoodsLocationUseList.Codes.Declaration, goodsLocation.CGL_LocationUse);
				AssertSame("Cached", goodsLocation, declaration.GoodsLocation);
				AssertEquals("IsRegisteredEditableChildObject", true, declaration.IsRegisteredEditableChildObject(goodsLocation));
			});
		}

		public void TestBusinessObjectsWithRelatedEventsForCusExitHeader()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.New<ICusExitHeader>();
			exitHeader.CXH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			exitHeader.CXH_ParentID = declaration.PK;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertCollectionContains("EXP", exitHeader, declaration.BusinessObjectsWithRelatedEvents);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertCollectionNotContains("non EXP", exitHeader, declaration.BusinessObjectsWithRelatedEvents);
			});
		}

		public void TestBusinessObjectsWithRelatedEventsForCusExitReport()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			var exitHeader = Factory.New<ICusExitHeader>();
			exitHeader.CXH_ParentTableCode = JobDeclarationSchema.Constants.Prefix;
			exitHeader.CXH_ParentID = declaration.PK;
			var exitReport = Factory.New<ICusExitReport>();
			exitReport.CER_CXH_Header = exitHeader.PK;

			CombineAssertions(() =>
			{
				declaration.JE_MessageType = MessageTypeList.Codes.Export;
				AssertCollectionContains("EXP", exitReport, declaration.BusinessObjectsWithRelatedEvents);

				declaration.JE_MessageType = MessageTypeList.Codes.Import;
				AssertCollectionNotContains("non EXP", exitReport, declaration.BusinessObjectsWithRelatedEvents);
			});
		}

		public void TestJE_OH_DutyPayerCaption()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			AssertEquals("Caption Duty Payer", "Duty Payer", declaration.JE_OH_DutyPayerInfo.GetAttribute<ResourceStringDataAttribute>().Caption);
		}

		public void TestJE_OH_DutyPayerCaption_Import()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetDeclarationConfiguration(declaration, "IsUCC6Core", true))
			{
				this.AssertDataBoundResourceStringsWithMultipleResourceKey(declaration.JE_OH_DutyPayerInfo, declaration.MultipleKeysToUse, "Duty Payer", shortCaption: string.Empty, mediumCaption: string.Empty, fullDescription: "[13 21 000 000] Person paying the customs duty");
			}
		}

		public void TestIsRequestedProcedureEnable()
		{
			var declaration = Factory.New<JobDeclaration>();
			Assert("Should not enable Requested Procedure", !declaration.IsRequestedProcedureEnable);
		}

		public void TestShouldTSRegisterManagementSelectInventoryMenuItemBeVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.GoodsLocation.Address.AuthorisationNumber = "Location";

			var premises = Factory.New<ICusTempStorageRegPremises>();
			premises.SRP_Type = "AAA";
			premises.SRP_CustomsLocation = "Location";

			var registryRegisterEnabled = ObjectFactory.Get<IEUCustomsRegistry>().RegisterEnabled;
			var registryRegisterEnabledDeveloperOnly = ObjectFactory.Get<IEUCustomsRegistry>().RegisterEnabledDeveloperOnly;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				CombineAssertions(() =>
				{
					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						AssertEquals("When RegisterEnabled = false and RegisterEnabledDeveloperOnly = false, ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible is false", false, declaration.ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible);
					}

					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					{
						AssertEquals("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = false and there is a premises with location != entry instruction's authorization ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible is true", true, declaration.ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible);
					}

					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						AssertEquals("When RegisterEnabled = false and RegisterEnabledDeveloperOnly = true and there is a premises with location != entry instruction's authorization ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible is true", true, declaration.ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible);
					}

					using (registryRegisterEnabled.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					using (registryRegisterEnabledDeveloperOnly.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
					{
						premises.SRP_CustomsLocation = "AAA";
						AssertEquals("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = true and there is a premises with location != entry instruction's authorization ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible is false", false, declaration.ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible);

						premises.SRP_CustomsLocation = "Location";
						AssertEquals("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = true and there is a premises with location = entry instruction's authorization ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible is true", true, declaration.ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible);

						declaration.JE_LocationOfGoods = "Location";
						entryInstruction1.GoodsLocation.Address.AuthorisationNumber = "Location1";
						AssertEquals("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = true and there is no premises with location = entry instruction's authorization ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible is false", false, declaration.ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible);

						var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
						entryInstruction2.GoodsLocation.Address.AuthorisationNumber = "location2";

						var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
						entryInstruction3.GoodsLocation.Address.AuthorisationNumber = "Location";

						AssertEquals("When RegisterEnabled = true and RegisterEnabledDeveloperOnly = true and there is at least one a premises with location = entry instruction's authorization no ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible is true", true, declaration.ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible);

						premises.Delete();
						AssertEquals("When PRegisterEnabled = true and RegisterEnabledDeveloperOnly = true but there is no premises ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible is false", false, declaration.ShouldTSRegisterManagementSelectInventoryMenuItemBeVisible);
					}
				});
			}
		}

		public void TestPreviousDocumentValidationDecider()
		{
			var declaration = Factory.New<JobDeclaration>();

			using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, configurationValue: true))
			{
				declaration.JE_MessageType = EUJobMessageTypeList.Codes.Import;
				AssertType<UCC6ImportPreviousDocumentValidationDecider>(((IPreviousDocumentsProviderWithValidationDecider)declaration).ValidationDecider);
			}
		}

		#region MapSelectedInventoryFromTS
		public void TestMapSelectedInventoryFromTS_AssertMappingCorrectParent()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.GoodsLocation.Address.AuthorisationNumber = "12";
			var entryInstruction2 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.GoodsLocation.Address.AuthorisationNumber = "60";

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.InvoiceLines.AddNew();
			invoiceLine.JI_CEI = entryInstruction1.PK;

			CombineAssertions(() =>
			{
				var selectedLines = CreateMinimalMockRegLines("12");
				declaration.MapSelectedInventoryFromTS(selectedLines);
				AssertEquals("Assert that all the selected line added to the entry instruction with the AuthorisationNumber = 12", 9, invoiceHeader.InvoiceLines.Count);

				selectedLines = CreateMinimalMockRegLines("60");
				declaration.MapSelectedInventoryFromTS(selectedLines);
				AssertEquals("Assert that all the selected line added to the entry instruction with the AuthorisationNumber = 60", 17, entryInstruction2.Invoices.FirstOrDefault()?.InvoiceLines.Count ?? 0);

				selectedLines = CreateMinimalMockRegLines("82");
				declaration.MapSelectedInventoryFromTS(selectedLines);
				AssertEquals("If no entrystruction with AuthorisationNumber equals to to the selected or with an empty one is available no new invoicline created", 17, declaration.InvoiceLines.Count);

				var entryInstruction3 = declaration.CustomsEntryInstructions.AddNew();
				selectedLines = CreateMinimalMockRegLines("82");
				declaration.MapSelectedInventoryFromTS(selectedLines);
				AssertEquals("If no entrystruction with AuthorisationNumber equals to to the selected but there is one without this is used", 25, entryInstruction3.Invoices.FirstOrDefault()?.InvoiceLines.Count ?? 0);

				var declaration2 = Factory.New<JobDeclaration>();
				var entryInstruction4 = declaration2.CustomsEntryInstructions.AddNew();
				declaration2.MapSelectedInventoryFromTS(selectedLines);
				AssertEquals("If no header founded create a new one", 1, declaration2.Invoices.Count);
				AssertEquals("Invoice lines created in the new invoice header", 8, declaration2.Invoices.FirstOrDefault().InvoiceLines.Count);
			});

			CusTempStorageSelectableRegLineCollection CreateMinimalMockRegLines(ZString customLocation)
			{
				var mockPremises1 = new Mock<ICusTempStorageRegPremises>();
				mockPremises1.Setup(p => p.SRP_CustomsLocation).Returns(customLocation);

				var mockPremises2 = new Mock<ICusTempStorageRegPremises>();
				mockPremises2.Setup(p => p.SRP_CustomsLocation).Returns(customLocation);

				var mockHeader1 = new Mock<ICusTempStorageRegHeader>();
				mockHeader1.Setup(h => h.Premises).Returns(mockPremises1.Object);
				mockHeader1.Setup(h => h.SRH_Reference).Returns("TSDRef");
				mockHeader1.Setup(h => h.SRH_ArrivalDate).Returns(ZDate.Today);

				var mockHeader2 = new Mock<ICusTempStorageRegHeader>();
				mockHeader2.Setup(h => h.Premises).Returns(mockPremises2.Object);
				mockHeader2.Setup(h => h.SRH_Reference).Returns("TSDRef");
				mockHeader2.Setup(h => h.SRH_ArrivalDate).Returns(ZDate.Today);

				var regLine = Factory.New<CusTempStorageRegLine>();
				var collectionPivot = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine);

				var itemList1 = new RegLineItemQuantityCollection();
				var itemList2 = new RegLineItemQuantityCollection();
				for (int i = 0; i < 4; i++)
				{
					itemList1.Add(RegLineItemQuantity.LoadNew(collectionPivot.AddChild(Factory.New<CusTempStorageRegLineItem>())));
					itemList2.Add(RegLineItemQuantity.LoadNew(collectionPivot.AddChild(Factory.New<CusTempStorageRegLineItem>())));
				}

				var mockRegLine1 = new Mock<ICusTempStorageRegLine>();
				mockRegLine1.Setup(r => r.RegHeader).Returns(mockHeader1.Object);
				mockRegLine1.Setup(r => r.RegLineItemQuantities).Returns(itemList1);
				var mockRegLine2 = new Mock<ICusTempStorageRegLine>();
				mockRegLine2.Setup(r => r.RegHeader).Returns(mockHeader2.Object);
				mockRegLine2.Setup(r => r.RegLineItemQuantities).Returns(itemList2);

				var regLine1 = new CusTempStorageSelectableRegLine(mockRegLine1.Object);
				var regLine2 = new CusTempStorageSelectableRegLine(mockRegLine2.Object);
				var collection = new CusTempStorageSelectableRegLineCollection()
				{
					regLine1,
					regLine2
				};
				return collection;
			}
		}

		public void TestMapSelectedInventoryFromTS_TestMappingLineDetails()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.GoodsLocation.Address.AuthorisationNumber = "12";

			var selectedLines = CreateMinimalMockRegLine();
			declaration.MapSelectedInventoryFromTS(selectedLines);

			var invoiceLine = entryInstruction1.Invoices.First().InvoiceLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Assert that [33] Tariff = CusTempStorageRegLineItem.SRI_Tariff", "12345", invoiceLine.JI_Tariff);
				AssertEquals("Assert that Goods Description = CusTempStorageRegLineItem.SRI_GoodsDescription", "TestGoods", invoiceLine.JI_Description);
				AssertEquals("Assert that [35] Gross Weight = Selected Gross Weight to Draw apportioned", (ZDecimal)10, invoiceLine.JI_Weight);
				AssertEquals("Assert that CUS Code = CusTempStorageRegLineItem.SRI_CusC4Number", "1234", invoiceLine.ZG_CusNumber);
			});

			CusTempStorageSelectableRegLineCollection CreateMinimalMockRegLine()
			{
				var mockPremises = new Mock<ICusTempStorageRegPremises>();
				mockPremises.Setup(p => p.SRP_CustomsLocation).Returns("12");

				var mockHeader = new Mock<ICusTempStorageRegHeader>();
				mockHeader.Setup(h => h.Premises).Returns(mockPremises.Object);
				mockHeader.Setup(h => h.SRH_Reference).Returns("TSDRef");
				mockHeader.Setup(h => h.SRH_ArrivalDate).Returns(ZDate.Today);

				var regLine = Factory.New<CusTempStorageRegLine>();
				var regItem = Factory.New<CusTempStorageRegLineItem>();
				regItem.SRI_Tariff = "12345";
				regItem.SRI_GoodsDescription = "TestGoods";
				regItem.SRI_CusC4Number = "1234";
				var collectionPivot = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine);
				var pivot = collectionPivot.AddChild(regItem);
				pivot.SRV_GrossWeight = 10;

				var itemList = new RegLineItemQuantityCollection
				{
					RegLineItemQuantity.LoadNew(pivot)
				};

				var mockRegLine = new Mock<ICusTempStorageRegLine>();
				mockRegLine.Setup(r => r.RegHeader).Returns(mockHeader.Object);
				mockRegLine.Setup(r => r.RegLineItemQuantities).Returns(itemList);
				mockRegLine.Setup(r => r.GrossWeightRemainingCalculated).Returns(10);
				var selectedRegLine = new CusTempStorageSelectableRegLine(mockRegLine.Object);
				selectedRegLine.GrossWeightToDraw = 10;
				var collection = new CusTempStorageSelectableRegLineCollection()
				{
					selectedRegLine,
				};
				return collection;
			}
		}

		public void TestMapSelectedInventoryFromTS_TestMappingLineDetailsGrossWeigth()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.GoodsLocation.Address.AuthorisationNumber = "12";

			var selectedLines = CreateMinimalMockRegLine();
			declaration.MapSelectedInventoryFromTS(selectedLines);

			var invoiceLines = entryInstruction1.Invoices.First().InvoiceLines;
			CombineAssertions(() =>
			{
				Assert("Assert that there is an invoice line with 3 as Weigth", invoiceLines.Where(i => i.JI_Weight == 3).Any());
				Assert("Assert that there is an invoice line with 2 as Weigth", invoiceLines.Where(i => i.JI_Weight == 2).Any());
			});

			CusTempStorageSelectableRegLineCollection CreateMinimalMockRegLine()
			{
				var mockPremises = new Mock<ICusTempStorageRegPremises>();
				mockPremises.Setup(p => p.SRP_CustomsLocation).Returns("12");

				var mockHeader = new Mock<ICusTempStorageRegHeader>();
				mockHeader.Setup(h => h.Premises).Returns(mockPremises.Object);
				mockHeader.Setup(h => h.SRH_Reference).Returns("TSDRef");
				mockHeader.Setup(h => h.SRH_ArrivalDate).Returns(ZDate.Today);

				var regLine = Factory.New<CusTempStorageRegLine>();
				var regItem1 = Factory.New<CusTempStorageRegLineItem>();
				regItem1.SRI_GoodsDescription = "inv1";
				var regItem2 = Factory.New<CusTempStorageRegLineItem>();
				regItem2.SRI_GoodsDescription = "inv2";
				var collectionPivot = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine);
				var pivot1 = collectionPivot.AddChild(regItem1);
				pivot1.SRV_GrossWeight = 4;
				var pivot2 = collectionPivot.AddChild(regItem2);
				pivot2.SRV_GrossWeight = 6;

				var itemList = new RegLineItemQuantityCollection
				{
					RegLineItemQuantity.LoadNew(pivot1),
					RegLineItemQuantity.LoadNew(pivot2)
				};

				var mockRegLine = new Mock<ICusTempStorageRegLine>();
				mockRegLine.Setup(r => r.RegHeader).Returns(mockHeader.Object);
				mockRegLine.Setup(r => r.RegLineItemQuantities).Returns(itemList);
				mockRegLine.Setup(r => r.GrossWeightRemainingCalculated).Returns(10);
				var selectedRegLine = new CusTempStorageSelectableRegLine(mockRegLine.Object);
				selectedRegLine.GrossWeightToDraw = 5;
				var collection = new CusTempStorageSelectableRegLineCollection()
				{
					selectedRegLine,
				};
				return collection;
			}
		}

		public void TestMapSelectedInventoryFromTS_TestMappingPreviousDocumentsIMP() => AssertMapSelectedInventoryFromTS_TestMappingPreviousDocuments(ZString.Empty, ZString.Empty, UniversalReferenceConstants.PreviusDocumentType.SummaryDeclaration);

		public void TestMapSelectedInventoryFromTS_TestMappingPreviousDocumentsImpStyleH2() => AssertMapSelectedInventoryFromTS_TestMappingPreviousDocuments(ZString.Empty, ImportDeclarationTypeList.H2, UniversalReferenceConstants.SupportingDocumentTypes._337);

		public void TestMapSelectedInventoryFromTS_TestMappingPreviousDocumentsExpSubStyleExs() => AssertMapSelectedInventoryFromTS_TestMappingPreviousDocuments(UniversalReferenceConstants.RefCusCodeListEntrySubStyle.EXS, "", UniversalReferenceConstants.SupportingDocumentTypes.N337);

		public void AssertMapSelectedInventoryFromTS_TestMappingPreviousDocuments(ZString subStyle, ZString style, ZString expectedDocType)
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.GoodsLocation.Address.AuthorisationNumber = "12";
			entryInstruction1.CEI_Style = style;
			entryInstruction1.CEI_SubStyle = subStyle;

			var selectedLines = CreateMinimalMockRegLineDocument();
			declaration.MapSelectedInventoryFromTS(selectedLines);

			var invoiceLine = (JobComInvoiceLine)entryInstruction1.Invoices.First().InvoiceLines.First();
			CombineAssertions(() =>
			{
				var previousDoc = invoiceLine.PreviousDocuments.First();
				AssertEquals("Assert that Previous Document/Type is correct", expectedDocType, previousDoc.CSI_Code);
				AssertEquals("Assert that Previous Document/Class = X", PreviousDocumentClassList.Codes.SummaryDeclaration, previousDoc.CSI_SubType);
				AssertEquals("Assert that Previous Document/Reference = CusTempStorageRegHeader.SRH_Reference", "TSDRef", previousDoc.CSI_ReferenceNumber);
				AssertEquals("Assert that Previous Document/Line N� = CusTempStorageRegLineItem.SRI_GoodsItemNumber", 1, previousDoc.CSI_LineNo);
			});

			CusTempStorageSelectableRegLineCollection CreateMinimalMockRegLineDocument()
			{
				var mockPremises = new Mock<ICusTempStorageRegPremises>();
				mockPremises.Setup(p => p.SRP_CustomsLocation).Returns("12");

				var mockHeader = new Mock<ICusTempStorageRegHeader>();
				mockHeader.Setup(h => h.Premises).Returns(mockPremises.Object);
				mockHeader.Setup(h => h.SRH_Reference).Returns("TSDRef");
				mockHeader.Setup(h => h.SRH_ArrivalDate).Returns(ZDate.Today);

				var regLine = Factory.New<CusTempStorageRegLine>();
				var regItem = Factory.New<CusTempStorageRegLineItem>();
				regItem.SRI_GoodsItemNumber = 1;
				var collectionPivot = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine);
				var pivot = collectionPivot.AddChild(regItem);

				var itemList = new RegLineItemQuantityCollection
				{
					RegLineItemQuantity.LoadNew(pivot)
				};

				var mockRegLine = new Mock<ICusTempStorageRegLine>();
				mockRegLine.Setup(r => r.RegHeader).Returns(mockHeader.Object);
				mockRegLine.Setup(r => r.RegLineItemQuantities).Returns(itemList);
				var selectedRegLine = new CusTempStorageSelectableRegLine(mockRegLine.Object);
				var collection = new CusTempStorageSelectableRegLineCollection()
				{
					selectedRegLine,
				};
				return collection;
			}
		}

		public void TestMapSelectedInventoryFromTS_TestMappingPackages()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.GoodsLocation.Address.AuthorisationNumber = "12";
			entryInstruction1.CEI_SubStyle = UniversalReferenceConstants.RefCusCodeListEntrySubStyle.EXS;

			var selectedLines = CreateMinimalMockRegLinePackage("NE");
			declaration.MapSelectedInventoryFromTS(selectedLines);

			var invoiceLine = entryInstruction1.Invoices.First().InvoiceLines[0];

			CombineAssertions(() =>
			{
				AssertEquals("Assert that Packing/Packing Details/Marks = CusTempStorageRegLine.SRL_PackageMarks ", "1234567890123:PEUGEOT:308", declaration.Packages[0].CW_MarksAndNos);
				AssertEquals("Assert that Packing/Packing Details/Pack Type = CusTempStorageRegLine.SRL_PackageType ", "NE", declaration.Packages[0].CW_PackType);
				AssertEquals("Assert that Packing/Packing Details/Pack Qty = Selected Packages to draw", 4, declaration.Packages[0].CW_PackQty);
				Assert("Assert that the package is linked to the invoice line", invoiceLine.PackagesForInvoiceLinesForBindingOnly[0].IsLinked);
			});
		}

		public void TestMapSelectedInventoryFromTS_TestMappingVehicles()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.GoodsLocation.Address.AuthorisationNumber = "12";
			entryInstruction1.CEI_SubStyle = UniversalReferenceConstants.RefCusCodeListEntrySubStyle.EXS;

			var bill = declaration.Bills.AddNew();
			bill.CU_BillType = BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB123";

			var selectedLines = CreateMinimalMockRegLinePackage("FR");
			declaration.MapSelectedInventoryFromTS(selectedLines);
			var invoiceLine = entryInstruction1.Invoices.First().InvoiceLines[0];

			CombineAssertions(() =>
			{
				AssertEquals("Assert that Inv. Line/Line Details/Vehicle/VIN = Part1", "1234567890123", invoiceLine.Vehicles[0].CVH_VehicleIdentificationNumber);
				AssertEquals("Assert that Inv. Line/Line Details/Vehicle/Brand = Part2 ", "PEUGEOT", invoiceLine.Vehicles[0].CVH_BrandName);
				AssertEquals("Assert that Inv. Line/Line Details/Vehicle/Model = Part3", "308", invoiceLine.Vehicles[0].CVH_ModelName);
			});
		}

		CusTempStorageSelectableRegLineCollection CreateMinimalMockRegLinePackage(ZString packType)
		{
			var mockPremises = new Mock<ICusTempStorageRegPremises>();
			mockPremises.Setup(p => p.SRP_CustomsLocation).Returns("12");

			var mockHeader = new Mock<ICusTempStorageRegHeader>();
			mockHeader.Setup(h => h.Premises).Returns(mockPremises.Object);
			mockHeader.Setup(h => h.SRH_Reference).Returns("TSDRef");
			mockHeader.Setup(h => h.SRH_ArrivalDate).Returns(ZDate.Today);

			var regLine = Factory.New<CusTempStorageRegLine>();
			var regItem = Factory.New<CusTempStorageRegLineItem>();
			regItem.SRI_GoodsItemNumber = 1;
			var collectionPivot = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine);
			var pivot = collectionPivot.AddChild(regItem);

			var itemList = new RegLineItemQuantityCollection
			{
				RegLineItemQuantity.LoadNew(pivot)
			};

			var mockRegLine = new Mock<ICusTempStorageRegLine>();
			mockRegLine.Setup(r => r.RegHeader).Returns(mockHeader.Object);
			mockRegLine.Setup(r => r.RegLineItemQuantities).Returns(itemList);
			mockRegLine.Setup(r => r.SRL_PackageType).Returns(packType);
			mockRegLine.Setup(r => r.SRL_PackageMarks).Returns("1234567890123:PEUGEOT:308");
			var selectedRegLine = new CusTempStorageSelectableRegLine(mockRegLine.Object);
			selectedRegLine.PackagesToDraw = 4;

			var collection = new CusTempStorageSelectableRegLineCollection()
				{
					selectedRegLine,
				};
			return collection;
		}

		public void TestMapSelectedInventoryFromTS_TestMappingSupportingDocuments()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction1 = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction1.GoodsLocation.Address.AuthorisationNumber = "12";

			var selectedLines = CreateMinimalMockRegLine();
			declaration.MapSelectedInventoryFromTS(selectedLines);

			var invoiceLine = entryInstruction1.Invoices.First().InvoiceLines[0];
			CombineAssertions(() =>
			{
				AssertEquals("Assert that Inv. Line/ [44] Supporting Documents/Type = 1217", UniversalReferenceConstants.SupportingDocumentTypes._1217, invoiceLine.SupportingDocuments[0].CSI_Code);
				AssertEquals("Assert that Inv. Line/ [44] Supporting Documents/Reference = CusTempStorageRegHeader.SRH_Reference ", "TSDRef", invoiceLine.SupportingDocuments[0].CSI_ReferenceNumber);
			});

			CusTempStorageSelectableRegLineCollection CreateMinimalMockRegLine()
			{
				var mockPremises = new Mock<ICusTempStorageRegPremises>();
				mockPremises.Setup(p => p.SRP_CustomsLocation).Returns("12");

				var mockHeader = new Mock<ICusTempStorageRegHeader>();
				mockHeader.Setup(h => h.Premises).Returns(mockPremises.Object);
				mockHeader.Setup(h => h.SRH_Reference).Returns("TSDRef");
				mockHeader.Setup(h => h.SRH_ArrivalDate).Returns(ZDate.Today);

				var regLine = Factory.New<CusTempStorageRegLine>();
				var regItem = Factory.New<CusTempStorageRegLineItem>();
				var collectionPivot = new CusTempStorageRegLineItemPivotCollection<CusTempStorageRegLineItemPivot>(regLine);
				var pivot = collectionPivot.AddChild(regItem);

				var itemList = new RegLineItemQuantityCollection
				{
					RegLineItemQuantity.LoadNew(pivot)
				};

				var mockRegLine = new Mock<ICusTempStorageRegLine>();
				mockRegLine.Setup(r => r.RegHeader).Returns(mockHeader.Object);
				mockRegLine.Setup(r => r.RegLineItemQuantities).Returns(itemList);
				var selectedRegLine = new CusTempStorageSelectableRegLine(mockRegLine.Object);

				var collection = new CusTempStorageSelectableRegLineCollection()
				{
					selectedRegLine,
				};
				return collection;
			}
		}
		#endregion

		public void TestContainerControlCheckboxVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Default ContainerControlCheckboxVisible is set to false", false, declaration.ContainerControlCheckboxVisible);
		}

		public void TestContainerUnloadedCheckboxVisible()
		{
			var declaration = Factory.New<JobDeclaration>();
			AssertEquals("Default ContainerUnloadedCheckboxVisible is set to false", false, declaration.ContainerUnloadedCheckboxVisible);
		}

		protected override Type ExpectedDeclarationLevelPackageCollectionType => typeof(BaseDeclarationLevelPackageCollection<Package>);

		protected override BaseJobDeclaration GetJobDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.JE_MessageType = MessageTypeList.Codes.Import;
			return declaration;
		}

		void AssertContainersRequireForAllTransportModes(bool required, JobDeclaration declaration)
		{
			AssertEquals(required, declaration.ContainersRequired);
			declaration.JE_TransportMode = TransportTypeList.Codes.Air;
			AssertEquals(required, declaration.ContainersRequired);
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			AssertEquals(required, declaration.ContainersRequired);
			declaration.JE_TransportMode = TransportTypeList.Codes.Road;
			AssertEquals(required, declaration.ContainersRequired);
			declaration.JE_TransportMode = TransportTypeList.Codes.Rail;
			AssertEquals(required, declaration.ContainersRequired);
			declaration.JE_TransportMode = "XXX";
			AssertEquals(required, declaration.ContainersRequired);
		}

		void AssertCountryOfDispatchResourceStringDataAttribute(ZPropertyInfo propertyInfo, IReadOnlyList<string> multipleResourceKeys)
		{
			this.AssertDataBoundResourceStringsWithMultipleResourceKey(propertyInfo, multipleResourceKeys, shortCaption: "[15] Dispatch", caption: "[15] Country/Region of Dispatch", fullDescription: "[15] Country/Region of Dispatch of the goods");
		}

		void AssertIsInventorySelectionEnabledCore(bool expectedResult, Action<OrgCompanyData> setup)
		{
			var declaration = Factory.New<JobDeclarationForWarehouseEnabledTest>();
			var entryInstruction = Factory.New<CusEntryInstructionForTest>();
			var warehouseAddress = Factory.NewWithValidTestData<OrgAddress>();
			entryInstruction.CEI_OA_Warehouse = warehouseAddress.PK;
			declaration.CustomsEntryInstructions.Add(entryInstruction);
			setup(entryInstruction.Warehouse.Header.CompanyData);

			CombineAssertions(() =>
			{
				using (CustomsDataRegistry.Instance.EnableWarehouseInventory.SetTemporaryValue(declaration.CompanyPK.ToGuid(), Guid.Empty, Guid.Empty, true))
				{
					AssertEquals("Registry set", expectedResult, declaration.IsInventorySelectionEnabledCoreExposed);
				}

				Factory.InvalidateCachedProperties();
				AssertEquals("Registry not set", false, declaration.IsInventorySelectionEnabledCoreExposed);
			});
		}

		void AssertJE_ShipmentIncoTerm_ValueChanged(bool ucc6)
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, ucc6))
				{
					declaration.EUD_AgreedPlaceCode = "abc";
					declaration.JE_ShipmentIncoTerm = IncoTerms.CarriageAndInsurancePaidTo;
					AssertEquals("IncoTerm is not Other", "abc", declaration.EUD_AgreedPlaceCode);

					declaration.JE_ShipmentIncoTerm = IncoTerms.Other;
					AssertEquals("IncoTerm is Other", ucc6 ? ZString.Empty : new ZString("abc"), declaration.EUD_AgreedPlaceCode);
				}
			});
		}

		void AssertIncoTermPlace_ReadOnly(bool ucc6)
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, ucc6))
				{
					AssertEquals("Invalid Unloco", false, declaration.JE_ShipmentIncoTermPlaceInfo.ReadOnly);
					declaration.EUD_AgreedPlaceCode = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode)).Code;
					AssertEquals("Valid Unloco", ucc6, declaration.JE_ShipmentIncoTermPlaceInfo.ReadOnly);
				}
			});
		}

		void AssertAgreedPlaceCodeSupportAndVisible(bool ucc6)
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.New<JobDeclaration>();
				using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndSetIsUCC6Configuration(declaration, ucc6))
				{
					declaration.JE_ShipmentIncoTerm = IncoTerms.Other;
					AssertEquals("IncoTerms is Other", ZBool.False, declaration.AgreedPlaceCodeSupportAndVisible);

					declaration.JE_ShipmentIncoTerm = IncoTerms.CarriageAndInsurancePaidTo;
					AssertEquals("IncoTerms is not Other", ucc6, declaration.AgreedPlaceCodeSupportAndVisible);
				}
			});
		}

		void PrepareForCalculatingEntryStyleBasedOnFallBackProcedure(out OrgHeader orgHeaderLV, out OrgHeader orgHeaderIT, out OrgHeader orgHeaderCH, out OrgHeader orgHeaderGF, out OrgHeader orgHeaderGB, out OrgHeader orgHeaderPL)
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

			var euctp = helper.CreateTradeGroup("EUN", "EUCTP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(euctp, "CH", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var eusft = helper.CreateTradeGroup("EUN", "EUSFT", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eusft, "GF", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			var eusfr = helper.CreateTradeGroup("EUN", "EUSFR", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.AddCountry(eusfr, "GB", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);
			helper.AddCountry(eusfr, "PL", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTimeValue.Date);

			orgHeaderLV = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderLV.OH_RL_NKClosestPort = "LV";

			//An EU country without the special territories
			orgHeaderIT = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderIT.OH_RL_NKClosestPort = "IT";

			//A country eligible to a common transit procedure
			orgHeaderCH = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderCH.OH_RL_NKClosestPort = "CH";

			//An EU special territories
			orgHeaderGF = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderGF.OH_RL_NKClosestPort = "GF";

			//A non EU country that has the special territories
			orgHeaderGB = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderGB.OH_RL_NKClosestPort = "GB";

			//An EU country that has the special territories
			orgHeaderPL = Factory.NewWithValidTestData<OrgHeader>();
			orgHeaderPL.OH_RL_NKClosestPort = "PL";

			Factory.Save();
		}

		sealed class JobDeclarationForTest : JobDeclaration
		{
			public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool IsOfficeOfExitMeaningful_Exposed => IsOfficeOfExitMeaningfulForDeclaration;

			public new bool IsLookupsCachedInBase => base.IsLookupsCachedInBase;

			protected override CusGuaranteeHeader GetCustomsGuaranteeCore => customsGuarantee ?? (customsGuarantee = Factory.NewWithValidTestData<CusGuaranteeHeader>());
			CusGuaranteeHeader customsGuarantee;

			protected override bool ZG_AgreedPlaceCodeValidationSupportCore => false;

			public EUCommonConstants.TransportModeSource TransportMeansDependencyForTest;

			protected override EUCommonConstants.TransportModeSource TransportMeansDependencyCore => TransportMeansDependencyForTest;
		}

		sealed class JobDeclarationForWarehouseEnabledTest : JobDeclaration
		{
			public JobDeclarationForWarehouseEnabledTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public bool SupportsBondedWarehousingCoreExposed => SupportsBondedWarehousingCore;

			public bool SupportInwardProcessingCoreExposed => SupportInwardProcessingCore;

			public bool IsInventorySelectionEnabledCoreExposed => IsInventorySelectionEnabledCore;
		}

		sealed class JobDeclarationWithPublicHasSplitEntriesCore : JobDeclaration
		{
			public JobDeclarationWithPublicHasSplitEntriesCore(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			internal bool HasSplitEntriesForTest => HasSplitEntriesCore;
		}

		static void AddItineraryCountryForTest(JobDeclaration declaration, string country)
		{
			var itineraryCountry = declaration.ItineraryCountries.AddNew();
			itineraryCountry.CY_Code = country;
		}
	}

	public sealed class JobDeclarationForTestOnly_RequestedProcedure : JobDeclaration
	{
		public JobDeclarationForTestOnly_RequestedProcedure(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override bool IsRequestedProcedureEnableCore => true;
	}
}
