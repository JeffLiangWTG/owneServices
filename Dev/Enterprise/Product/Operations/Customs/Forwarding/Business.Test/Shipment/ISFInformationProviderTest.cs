using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.US.ISF;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.Forwarding.Business.Testing
{
	class ISFInformationProviderTest : TestCaseWithFactory
	{
		[TestDate(2011, 01, 01)]
		[ExpectNoExceptions]
		public void TestISFBillData()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var proxyOrg = Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy);
			proxyOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXX", Core.Constants.CountryCodes.UnitedStates);
			proxyOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "XXXY", Core.Constants.CountryCodes.UnitedStates);
			var header1 = Factory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header1.BF_JobReference = "JOB1";
			header1.BF_SystemCreateTimeUtc = new ZDateTime(2010, 12, 2);
			var bill1 = Factory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill1.BB_BF = header1.PK;
			bill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "BILL1";
			bill1.BB_CustomsStatus = "S1";
			var bill2 = Factory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill2.BB_BF = header1.PK;
			bill2.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill2.BB_BillNum = "XXXABILL2";
			bill2.BB_CustomsStatus = "S3";
			Factory.Save();
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_SystemCreateTimeUtc = ZDateTime.Now;
			shipment.JS_HouseBill = "BILL1";
			shipment.JS_TransportMode = Core.Constants.TransportModes.Air;

			var isfInformation = new ISFInformationProvider(shipment);

			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("").Using(CustomComparers.TypeComparison), "Should not match when it's not Sea");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo("").Using(CustomComparers.TypeComparison), "Should not match when it's not Sea");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo("").Using(CustomComparers.TypeComparison), "Should not match when it's not Sea");

			shipment.JS_TransportMode = Core.Constants.TransportModes.AirSea;
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("BILL1").Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
			NUnit.Framework.Assert.That(shipment.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
			NUnit.Framework.Assert.That(shipment.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");

			isfInformation = new ISFInformationProvider(shipment);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("BILL1").Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
			NUnit.Framework.Assert.That(shipment.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
			NUnit.Framework.Assert.That(shipment.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
			shipment.JS_HouseBill = "BILL2";
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match For BILL2");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match For BILL2");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match For BILL2");
			var bill3 = Factory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill3.BB_BF = header1.PK;
			bill3.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill3.BB_BillNum = "XXXXBILL2";
			bill3.BB_CustomsStatus = "S2";
			Factory.Save();
			shipment = Factory.New<ForwardingShipment>();
			isfInformation = new ISFInformationProvider(shipment);
			shipment.JS_SystemCreateTimeUtc = ZDateTime.Now;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "BILL2";
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("XXXXBILL2").Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with OrgProxy's SCAC");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S2).Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with OrgProxy's SCAC");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S2).Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with OrgProxy's SCAC");
			bill3.BB_BillNum = "XXXYBILL2";
			Factory.Save();
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("XXXYBILL2").Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with OrgProxy's CCP SCAC");
			bill3.BB_BillNum = "XXXXBILL2";
			Factory.Save();
			var amsNumber = shipment.Numbers.AddNew();
			amsNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
			amsNumber.CE_EntryNum = "XXXABILL1";
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match for AMS Number; AMS Number takes precedent to House Bill");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match for AMS Number; AMS Number takes precedent to House Bill");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match for AMS Number; AMS Number takes precedent to House Bill");
			amsNumber.CE_EntryNum = "XXXABILL2";
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("XXXABILL2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S3).Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S3).Using(CustomComparers.TypeComparison));
			shipment = Factory.New<ForwardingShipment>();
			isfInformation = new ISFInformationProvider(shipment);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "BILL2";
			shipment.JS_SystemCreateTimeUtc = new ZDateTime(2009, 1, 1, 1, 1, 1);
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match as data is out of range");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match as data is out of range");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match as data is out of range");
			var shippingLine = Factory.New<OrgHeader>();
			shippingLine.OH_Code = "SHP2";
			shippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXX", Core.Constants.CountryCodes.UnitedStates);
			shipment = Factory.New<ForwardingShipment>();
			isfInformation = new ISFInformationProvider(shipment);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_AgentType = Core.Constants.AgentType.Direct;
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "BILL1";
			Factory.Save();
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("BILL1").Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
			NUnit.Framework.Assert.That(shipment.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
			NUnit.Framework.Assert.That(shipment.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
			consol1.JK_MasterBillNum = "XXXABILL1";
			Factory.Save();
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match");
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_AgentType = Core.Constants.AgentType.Direct;
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_MasterBillNum = "BILL2";
			consol2.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
			shipment.JS_HouseBill = ZString.Empty;
			Factory.Save();
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("XXXXBILL2").Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with ShppingLine's SCAC");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S2).Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with ShppingLine's SCAC");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S2).Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with ShppingLine's SCAC");
			bill3.BB_BillNum = "XXXYBILL3";
			var shippingLine2 = Factory.New<OrgHeader>();
			shippingLine2.OH_Code = "SHP3";
			shippingLine2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "XXXY", Core.Constants.CountryCodes.UnitedStates);
			consol2.JK_MasterBillNum = "BILL3";
			consol2.JK_OA_ShippingLineAddress = shippingLine2.MainAddress.PK;
			shipment.JS_HouseBill = ZString.Empty;
			Factory.Save();
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("XXXYBILL3").Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with ShppingLine's CCP SCAC");
		}

		[TestDate(2011, 01, 01)]
		[ExpectNoExceptions]
		public void TestISFBillDataUseSCACFromBillIssuingParty()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var header1 = Factory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header1.BF_JobReference = "JOB1";
			header1.BF_SystemCreateTimeUtc = new ZDateTime(2010, 12, 2);
			var bill1 = Factory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill1.BB_BF = header1.PK;
			bill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "BILL1";
			bill1.BB_CustomsStatus = "S1";
			var bill2 = Factory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill2.BB_BF = header1.PK;
			bill2.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill2.BB_BillNum = "XXXABILL2";
			bill2.BB_CustomsStatus = "S3";
			var bill3 = Factory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill3.BB_BF = header1.PK;
			bill3.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill3.BB_BillNum = "ABCDBILL2";
			bill3.BB_CustomsStatus = "S2";
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "TEST1";
			org.OH_FullName = "Test House Bill Issuing Party";
			org.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "ABCD", Core.Constants.CountryCodes.UnitedStates);
			Factory.Save();
			var shipment = Factory.New<ForwardingShipment>();
			var isfInformation = new ISFInformationProvider(shipment);
			shipment.JS_SystemCreateTimeUtc = ZDateTime.Now;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_HouseBill = "BILL2";
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("ABCDBILL2").Using(CustomComparers.TypeComparison), "Match to ISF ABCDBILL2 when combine with HouseBillIssuingParty's SCAC");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S2).Using(CustomComparers.TypeComparison), "Match to ISF ABCDBILL2 when combine with HouseBillIssuingParty's SCAC");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S2).Using(CustomComparers.TypeComparison), "Match to ISF ABCDBILL2 when combine with HouseBillIssuingParty's SCAC");
			shipment = Factory.New<ForwardingShipment>();
			isfInformation = new ISFInformationProvider(shipment);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = Core.Constants.AgentType.Direct;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "BILL2";
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = org.PK;
			shipment.JS_HouseBill = ZString.Empty;
			Factory.Save();
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("ABCDBILL2").Using(CustomComparers.TypeComparison), "Match to ISF ABCDBILL2 when combine with MasterBillIssuingParty's SCAC");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S2).Using(CustomComparers.TypeComparison), "Match to ISF ABCDBILL2 when combine with MasterBillIssuingParty's SCAC");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S2).Using(CustomComparers.TypeComparison), "Match to ISF ABCDBILL2 when combine with MasterBillIssuingParty's SCAC");
		}

		[TestDate(2011, 01, 01)]
		[ExpectNoExceptions]
		public void TestISFBillDataUseCaching()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			var newFactory = new BusinessObjectFactory();
			newFactory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy).CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXX", Core.Constants.CountryCodes.UnitedStates);
			var shippingLine = newFactory.New<OrgHeader>();
			shippingLine.OH_Code = "SHP2";
			shippingLine.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXX", Core.Constants.CountryCodes.UnitedStates);
			shippingLine.MainAddress.OA_Address1 = "ADDRESS1";
			for (var i = 1; i < 8; i++)
			{
				var company = newFactory.New<GlbCompany>();
				company.GC_Code = "KD" + i.ToString();
				company.GC_Name = company.GC_Code + " NAME";
				var proxy = newFactory.New<OrgHeader>();
				proxy.OH_Code = "ORGCODE" + i.ToString();
				proxy.OH_FullName = proxy.OH_Code + " NAME";
				proxy.MainAddress.OA_Address1 = "ADD1";
				company.GC_OH_OrgProxy = proxy.PK;
				if (i != 3)
				{
					proxy.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, i % 2 == 0 ? "XXXA" : "XXX" + i.ToString(), Core.Constants.CountryCodes.UnitedStates);
				}
			}

			var header1 = newFactory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header1.BF_JobReference = "JOB1";
			header1.BF_SystemCreateTimeUtc = new ZDateTime(2010, 12, 2);
			var bill1 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill1.BB_BF = header1.PK;
			bill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "BILL1";
			bill1.BB_CustomsStatus = "S1";
			var header2 = newFactory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header1.BF_JobReference = "JOB2";
			header1.BF_SystemCreateTimeUtc = new ZDateTime(2010, 12, 2);
			var bill2 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill2.BB_BF = header2.PK;
			bill2.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill2.BB_BillNum = "XXXCBILL2";
			bill2.BB_CustomsStatus = "S3";
			var header3 = newFactory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header3.BF_JobReference = "JOB3";
			header3.BF_SystemCreateTimeUtc = new ZDateTime(2010, 12, 2);
			var bill3 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill3.BB_BF = header3.PK;
			bill3.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill3.BB_BillNum = "XXXXBILL2";
			bill3.BB_CustomsStatus = "S2";
			var header4 = newFactory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header4.BF_JobReference = "JOB4";
			header4.BF_SystemCreateTimeUtc = new ZDateTime(2010, 12, 2);
			var bill4 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill4.BB_BF = header4.PK;
			bill4.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill4.BB_BillNum = "XXX5BILL4";
			bill4.BB_CustomsStatus = "S4";
			var header5 = newFactory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header5.BF_JobReference = "JOB5";
			header5.BF_SystemCreateTimeUtc = new ZDateTime(2010, 12, 2);
			var bill5 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill5.BB_BF = header5.PK;
			bill5.BB_BillType = BillTypeList.Codes.OceanBillOfLading;
			bill5.BB_BillNum = "XXXGBILL51234567";
			bill5.BB_CustomsStatus = "S5";
			var header6 = newFactory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header6.BF_JobReference = "JOB6";
			header6.BF_SystemCreateTimeUtc = new ZDateTime(2010, 12, 2);
			var bill6 = newFactory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill6.BB_BF = header6.PK;
			bill6.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill6.BB_BillNum = "XXXABILL6";
			bill6.BB_CustomsStatus = "S6";
			newFactory.Save();
			CombineAssertions(() =>
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_HouseBill = "BILL1";
				shipment.JS_SystemCreateTimeUtc = ZDateTime.Now;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
				var isfInformation = new ISFInformationProvider(shipment);
				var companyDBHits = Factory.GetTableHitCount(GlbCompanySchema.Constants.TableName);
				var orgHeaderDBHits = Factory.GetTableHitCount(OrgHeaderSchema.Constants.TableName);
				var cusCodeDBHits = Factory.GetTableHitCount(OrgCusCodeSchema.Constants.TableName);
				var isfBillDBHits = Factory.GetTableHitCount(CusISFBillSchema.Constants.TableName);
				var isfHeaderDBHits = Factory.GetTableHitCount(CusISFHeaderSchema.Constants.TableName);
				NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("").Using(CustomComparers.TypeComparison), "Should not match when it's not Sea");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo("").Using(CustomComparers.TypeComparison), "Should not match when it's not Sea");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo("").Using(CustomComparers.TypeComparison), "Should not match when it's not Sea");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(GlbCompanySchema.Constants.TableName), Is.EqualTo(companyDBHits), "Should not be loading extra GlbCompany");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(OrgHeaderSchema.Constants.TableName), Is.EqualTo(orgHeaderDBHits), "Should not be loading extra OrgHeader");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(OrgCusCodeSchema.Constants.TableName), Is.EqualTo(cusCodeDBHits), "Should not be loading extra OrgCusCode");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFHeaderSchema.Constants.TableName), Is.EqualTo(isfHeaderDBHits), "Should not be loading extra CusISFBill");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFBillSchema.Constants.TableName), Is.EqualTo(isfBillDBHits), "Should not be loading extra CusISFHeader");
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("BILL1").Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(OrgCusCodeSchema.Constants.TableName), Is.EqualTo(cusCodeDBHits + 2), "Should load extra OrgCusCode");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFHeaderSchema.Constants.TableName), Is.EqualTo(isfHeaderDBHits + 1), "Should load extra CusISFHeader");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFBillSchema.Constants.TableName), Is.EqualTo(isfBillDBHits + 1), "Should load extra CusISFBill");
				shipment.JS_GoodsDescription = "HELLO";
				NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("BILL1").Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(OrgCusCodeSchema.Constants.TableName), Is.EqualTo(cusCodeDBHits + 2), "Should not load extra OrgCusCode");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFHeaderSchema.Constants.TableName), Is.EqualTo(isfHeaderDBHits + 1), "Should not load extra CusISFHeader");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFBillSchema.Constants.TableName), Is.EqualTo(isfBillDBHits + 1), "Should not load extra CusISFBill");
				shipment.JS_HouseBill = "BILL2";
				NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("XXXXBILL2").Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with OrgProxy's SCAC");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S2).Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with OrgProxy's SCAC");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S2).Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with OrgProxy's SCAC");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(OrgCusCodeSchema.Constants.TableName), Is.EqualTo(cusCodeDBHits + 2), "Should not load extra OrgCusCode");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFHeaderSchema.Constants.TableName), Is.EqualTo(isfHeaderDBHits + 2), "Should load extra CusISFHeader To Match ISF XXXXBILL2");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFBillSchema.Constants.TableName), Is.EqualTo(isfBillDBHits + 2), "Should load extra CusISFBill To Match ISF XXXXBILL2");
				shipment = Factory.New<ForwardingShipment>();
				isfInformation = new ISFInformationProvider(shipment);
				shipment.JS_SystemCreateTimeUtc = ZDateTime.Now;
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_HouseBill = "BILL2";
				var amsNumber = shipment.Numbers.AddNew();
				amsNumber.CE_EntryType = CustomsReferenceNumberType.CustomsAdditionalReferenceNumbersCodes.AMS;
				amsNumber.CE_EntryNum = "XXXABILL1";
				NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match for AMS Number; AMS Number takes precedent to House Bill");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match for AMS Number; AMS Number takes precedent to House Bill");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match for AMS Number; AMS Number takes precedent to House Bill");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(OrgCusCodeSchema.Constants.TableName), Is.EqualTo(cusCodeDBHits + 2), "Should not load extra OrgCusCode");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFHeaderSchema.Constants.TableName), Is.EqualTo(isfHeaderDBHits + 2), "Should not load extra CusISFHeader To Match ISF XXXABILL1");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFBillSchema.Constants.TableName), Is.EqualTo(isfBillDBHits + 3), "Should load extra CusISFBill To Match ISF XXXABILL1");
				amsNumber.CE_EntryNum = "XXXCBILL2";
				NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("XXXCBILL2").Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S3).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S3).Using(CustomComparers.TypeComparison));
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(OrgCusCodeSchema.Constants.TableName), Is.EqualTo(cusCodeDBHits + 2), "Should not load extra OrgCusCode");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFHeaderSchema.Constants.TableName), Is.EqualTo(isfHeaderDBHits + 3), "Should load extra CusISFHeader To Match ISF XXXCBILL2");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFBillSchema.Constants.TableName), Is.EqualTo(isfBillDBHits + 4), "Should load extra CusISFBill To Match ISF XXXCBILL2");
				shipment = Factory.New<ForwardingShipment>();
				isfInformation = new ISFInformationProvider(shipment);
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				shipment.JS_HouseBill = "BILL2";
				shipment.JS_SystemCreateTimeUtc = new ZDateTime(2009, 1, 1, 1, 1, 1);
				NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match as data is out of range");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match as data is out of range");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match as data is out of range");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(OrgCusCodeSchema.Constants.TableName), Is.EqualTo(cusCodeDBHits + 2), "Should not load extra OrgCusCode");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFHeaderSchema.Constants.TableName), Is.EqualTo(isfHeaderDBHits + 3), "Should not load extra CusISFHeader To Match ISF BILL2");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFBillSchema.Constants.TableName), Is.EqualTo(isfBillDBHits + 4), "Should not load extra CusISFBill To Match ISF BILL2");
				shipment = Factory.New<ForwardingShipment>();
				isfInformation = new ISFInformationProvider(shipment);
				shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
				var consol1 = shipment.Consols.AddNew();
				consol1.JK_AgentType = Core.Constants.AgentType.Direct;
				consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol1.JK_MasterBillNum = "BILL1";
				Factory.Save();
				NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("BILL1").Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(OrgCusCodeSchema.Constants.TableName), Is.EqualTo(cusCodeDBHits + 2), "Should not load extra OrgCusCode");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFHeaderSchema.Constants.TableName), Is.EqualTo(isfHeaderDBHits + 3), "Should not load extra CusISFHeader To Match ISF BILL1");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFBillSchema.Constants.TableName), Is.EqualTo(isfBillDBHits + 5), "Should load extra CusISFBill To Match ISF BILL1");
				consol1.JK_MasterBillNum = "XXXABILL1";
				Factory.Save();
				NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Match");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(OrgCusCodeSchema.Constants.TableName), Is.EqualTo(cusCodeDBHits + 2), "Should not load extra OrgCusCode");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFHeaderSchema.Constants.TableName), Is.EqualTo(isfHeaderDBHits + 3), "Should not load extra CusISFHeader To Match ISF XXXABILL1");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFBillSchema.Constants.TableName), Is.EqualTo(isfBillDBHits + 5), "Should not load extra CusISFBill To Match ISF XXXABILL1");
				var consol2 = shipment.Consols.AddNew();
				consol2.JK_AgentType = Core.Constants.AgentType.Direct;
				consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol2.JK_MasterBillNum = "BILL2";
				consol2.JK_OA_ShippingLineAddress = shippingLine.MainAddress.PK;
				shipment.JS_HouseBill = ZString.Empty;
				Factory.Save();
				NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("XXXXBILL2").Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with ShppingLine's SCAC");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S2).Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with ShppingLine's SCAC");
				NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S2).Using(CustomComparers.TypeComparison), "Match to ISF XXXXBILL2 when combine with ShppingLine's SCAC");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(OrgCusCodeSchema.Constants.TableName), Is.EqualTo(cusCodeDBHits + 3), "Should load extra OrgCusCode");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFHeaderSchema.Constants.TableName), Is.EqualTo(isfHeaderDBHits + 3), "Should not load extra CusISFHeader To Match ISF XXXXBILL2");
				NUnit.Framework.Assert.That(Factory.GetTableHitCount(CusISFBillSchema.Constants.TableName), Is.EqualTo(isfBillDBHits + 6), "Should load extra CusISFBill To Match ISF XXXXBILL2");
			});
		}

		[TestDate(2011, 01, 01)]
		[ExpectNoExceptions]
		public void TestISFStatusLoopupForDirectShipment()
		{
			FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1);
			Factory.Load<OrgHeader>(GlbCompany.CurrentCompany.GC_OH_OrgProxy).CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "XXXX", Core.Constants.CountryCodes.UnitedStates);
			var header1 = Factory.New<Integration.Customs.US.ISF.ICusISFHeader>();
			header1.BF_JobReference = "JOB1";
			header1.BF_SystemCreateTimeUtc = new ZDateTime(2010, 12, 2);
			var bill1 = Factory.New<Integration.Customs.US.ISF.ICusISFBill>();
			bill1.BB_BF = header1.PK;
			bill1.BB_BillType = BillTypeList.Codes.HouseBillOfLading;
			bill1.BB_BillNum = "BILL1";
			bill1.BB_CustomsStatus = "S1";
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_SystemCreateTimeUtc = ZDateTime.Now;
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			var isfInformation = new ISFInformationProvider(shipment);
			var consol1 = shipment.Consols.AddNew();
			consol1.JK_AgentType = Core.Constants.AgentType.Direct;
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_MasterBillNum = "BILL1";
			Factory.Save();
			NUnit.Framework.Assert.That(shipment.JS_HouseBill, Is.EqualTo("").Using(CustomComparers.TypeComparison), "No Housebill");
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("BILL1").Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
			NUnit.Framework.Assert.That(shipment.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
			NUnit.Framework.Assert.That(shipment.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S1).Using(CustomComparers.TypeComparison), "Match To ISF BILL1");
			shipment.JS_HouseBill = "WHATEVER";
			Factory.Save();
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("BILL1").Using(CustomComparers.TypeComparison), "Still match to ISF BILL1");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S1).Using(CustomComparers.TypeComparison), "Still match to ISF BILL1");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S1).Using(CustomComparers.TypeComparison), "Still match to ISF BILL1");
			shipment.JS_HouseBill = "";
			Factory.Save();
			NUnit.Framework.Assert.That(isfInformation.ISFBillNumber, Is.EqualTo("BILL1").Using(CustomComparers.TypeComparison), "Still match to ISF BILL1");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatus, Is.EqualTo(DispositionCodeList.Codes.S1).Using(CustomComparers.TypeComparison), "Still match to ISF BILL1");
			NUnit.Framework.Assert.That(isfInformation.ISFBillStatusDescription, Is.EqualTo(DispositionCodeList.Descriptions.S1).Using(CustomComparers.TypeComparison), "Still match to ISF BILL1");
		}
	}
}
