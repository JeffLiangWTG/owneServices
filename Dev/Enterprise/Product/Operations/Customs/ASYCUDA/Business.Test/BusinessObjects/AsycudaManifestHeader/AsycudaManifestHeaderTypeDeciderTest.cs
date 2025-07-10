using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.Universal;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.ASYCUDA.Business.Testing
{
	sealed class AsycudaManifestHeaderTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForBinding()
		{
			var typeDecider = new AsycudaManifestHeaderTypeDecider();
			AssertEquals(typeof(AsycudaManifestHeader), typeDecider.GetTypeForBinding());
		}

		public void TestGetTypeForNew()
		{
			var typeDecider = new AsycudaManifestHeaderTypeDecider();
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
			AssertEquals(ObjectFactory.GetType<Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				AssertEquals("NewZealand", ObjectFactory.GetType<Integration.Customs.ASYCUDA.NZManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("NewZealand", ObjectFactory.GetType<Integration.Customs.ASYCUDA.NZManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Singapore))
			{
				AssertEquals("Singapore", ObjectFactory.GetType<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("Singapore", ObjectFactory.GetType<Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.SouthAfrica))
			{
				AssertEquals("SouthAfrica", ObjectFactory.GetType<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("SouthAfrica", ObjectFactory.GetType<Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				AssertEquals("UnitedStates", ObjectFactory.GetType<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("UnitedStates", ObjectFactory.GetType<Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Uruguay))
			{
				AssertEquals("Uruguay", ObjectFactory.GetType<Integration.Customs.ASYCUDA.UYManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("Uruguay", ObjectFactory.GetType<Integration.Customs.ASYCUDA.UYManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Mexico))
			{
				AssertEquals("Mexico", ObjectFactory.GetType<Integration.Customs.ASYCUDA.MXManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("Mexico", ObjectFactory.GetType<Integration.Customs.ASYCUDA.MXManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Chile))
			{
				AssertEquals("Chile", ObjectFactory.GetType<Integration.Customs.ASYCUDA.CLManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("Chile", ObjectFactory.GetType<Integration.Customs.ASYCUDA.CLManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
			{
				AssertEquals("United Kingdom", ObjectFactory.GetType<Integration.Customs.GB.GBGVMS.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("United Kingdom", ObjectFactory.GetType<Integration.Customs.GB.GBGVMS.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				AssertEquals("Brazil", ObjectFactory.GetType<Integration.Customs.ASYCUDA.BRManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("Brazil", ObjectFactory.GetType<Integration.Customs.ASYCUDA.BRManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Colombia))
			{
				AssertEquals("Colombia", ObjectFactory.GetType<Integration.Customs.ASYCUDA.COManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("Colombia", ObjectFactory.GetType<Integration.Customs.ASYCUDA.COManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Argentina))
			{
				AssertEquals("Argentina", ObjectFactory.GetType<Integration.Customs.ASYCUDA.ARManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("Argentina", ObjectFactory.GetType<Integration.Customs.ASYCUDA.ARManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.India))
			{
				AssertEquals("India", ObjectFactory.GetType<Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("India", ObjectFactory.GetType<Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Israel))
			{
				AssertEquals("Israel", ObjectFactory.GetType<Integration.Customs.ASYCUDA.ILManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("Israel", ObjectFactory.GetType<Integration.Customs.ASYCUDA.ILManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Peru))
			{
				AssertEquals("Peru", ObjectFactory.GetType<Integration.Customs.ASYCUDA.PEManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("Peru", ObjectFactory.GetType<Integration.Customs.ASYCUDA.PEManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedArabEmirates))
			{
				AssertEquals("UnitedArabEmirates", ObjectFactory.GetType<Integration.Customs.ASYCUDA.AEManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("UnitedArabEmirates", ObjectFactory.GetType<Integration.Customs.ASYCUDA.AEManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Ireland))
			{
				AssertEquals("Ireland", ObjectFactory.GetType<Integration.Customs.IE.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("Ireland", ObjectFactory.GetType<Integration.Customs.IE.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			{
				AssertEquals("Vietnam", ObjectFactory.GetType<Integration.Customs.ASYCUDA.VNManifest.IAsycudaManifestHeader>(), typeDecider.GetTypeForNew());
				AssertEquals("Vietnam", ObjectFactory.GetType<Integration.Customs.ASYCUDA.VNManifest.IAsycudaManifestHeader>(), new BusinessObjectFactory().New<AsycudaManifestHeader>().GetType());
			}
		}

		public void TestGetTypeForLoad()
		{
			using (ObjectFactory.Get<Integration.Customs.NZ.INZCustomsDataRegistry>().EnableInwardCargoReportManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.GVMS, CountryCodes.UnitedKingdom, ZDate.Today, true))
			using (ObjectFactory.Get<Integration.Customs.GB.IGBCustomsDataRegistry>().EnableIcsManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Get<Integration.Customs.GB.IGBCustomsDataRegistry>().EnableSSGBManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Get<Integration.Customs.CL.ICLCustomsRegistry>().EnableGlobalManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Get<Integration.Customs.MX.IMXCustomsDataRegistry>().EnableMXManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Get<Integration.Customs.CO.ICOCustomsDataRegistry>().EnableCOManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Get<Integration.Customs.AR.IARCustomsDataRegistry>().EnableARManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Get<Integration.Customs.PE.IPECustomsDataRegistry>().EnablePEManifests.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Get<Integration.Customs.IN.IINCustomsDataRegistry>().INEnableConsolGeneralManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Get<Integration.Customs.IN.IINCustomsDataRegistry>().INEnableImportGeneralManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (ObjectFactory.Get<Integration.Customs.VN.IVNCustomsDataRegistry>().EnableVNManifest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var header1 = Factory.New<ManifestBase.AsycudaManifestHeader>();
				header1.AMA_JobReference = "123";
				header1.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ZAOutturnAndGateInOrOut;
				header1.AMA_RN_NKCountry = Core.Constants.CountryCodes.SouthAfrica;
				var header2 = Factory.New<ManifestBase.AsycudaManifestHeader>();
				header2.AMA_JobReference = "456";
				header2.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				header2.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
				var header3 = Factory.New<ManifestBase.AsycudaManifestHeader>();
				header3.AMA_JobReference = "789";
				header3.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.ShippingLine;
				header3.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
				var header4 = Factory.New<ManifestBase.AsycudaManifestHeader>();
				header4.AMA_JobReference = "012";
				header4.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				header4.AMA_RN_NKCountry = Core.Constants.CountryCodes.Fiji;
				var header5 = Factory.New<ManifestBase.AsycudaManifestHeader>();
				header5.AMA_JobReference = "345";
				header5.AMA_ApplicationCode = "BBK";
				header5.AMA_RN_NKCountry = Core.Constants.CountryCodes.Eritrea;
				var header6 = Factory.New<ManifestBase.AsycudaManifestHeader>();
				header6.AMA_JobReference = "999";
				header6.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TRETrade;
				header6.AMA_RN_NKCountry = Core.Constants.CountryCodes.Turkey;
				var header7 = Factory.New<ManifestBase.AsycudaManifestHeader>();
				header7.AMA_JobReference = "555";
				header7.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.Consolidator;
				header7.AMA_RN_NKCountry = Core.Constants.CountryCodes.Brazil;
				var header8 = Factory.New<ManifestBase.AsycudaManifestHeader>();
				header8.AMA_JobReference = "H7 V1 UCC5";
				header8.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.EuH7V1;
				header8.AMA_ManifestType = "EH7";
				header8.AMA_RN_NKCountry = CountryCodes.Ireland;
				var header9 = Factory.New<ManifestBase.AsycudaManifestHeader>();
				header9.AMA_JobReference = "H7 V2 UCC6";
				header9.AMA_ApplicationCode = ApplicationCodeTypeList.Codes.EuH7V2;
				header9.AMA_ManifestType = "EH7";
				header9.AMA_RN_NKCountry = CountryCodes.Ireland;

				var headerNZ = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.NewZealand, "ICR", ApplicationCodeTypeList.Codes.ShippingLine);
				headerNZ.AMA_JobReference = "42";
				var headerSG = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Singapore, "MGI");
				headerSG.AMA_JobReference = "678";
				var headerZA = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.SouthAfrica, "HAB");
				headerZA.AMA_JobReference = "234";
				var headerACE = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedStates, "IAM");
				headerACE.AMA_JobReference = "901";
				var headerUY = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Uruguay, "MAN");
				headerUY.AMA_JobReference = "105";
				var headerMX = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Mexico, "MAN");
				headerMX.AMA_JobReference = "721";
				var headerCL = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Chile, "MAN");
				headerCL.AMA_JobReference = "996";
				var headerUKGVMS = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedKingdom, "GVM");
				headerUKGVMS.AMA_JobReference = "722";
				var headerUKICS = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedKingdom, "ICS", ApplicationCodeTypeList.Codes.ShippingLine);
				headerUKICS.AMA_JobReference = "725";
				var headerUKSS = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.UnitedKingdom, "S&S", ApplicationCodeTypeList.Codes.ShippingLine);
				headerUKSS.AMA_JobReference = "888";
				var headerBR = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Brazil, "MAN");
				headerBR.AMA_JobReference = "777";
				var headerCO = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Colombia, "MAN");
				headerCO.AMA_JobReference = "216";
				var headerAR = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Argentina, "MAN");
				headerAR.AMA_JobReference = "798";
				var headerTW = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Taiwan, "MAN");
				headerTW.AMA_JobReference = "987";
				var headerTWBCD = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Taiwan, "X1", ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration);
				headerTWBCD.AMA_JobReference = "988";
				var headerPE = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.Peru, "MAN");
				headerPE.AMA_JobReference = "989";
				var headerINCGM = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.India, "CGM", ApplicationCodeTypeList.Codes.Consolidator);
				headerINCGM.AMA_JobReference = "CGM001";
				var headerINIGM = AsycudaManifestHeaderHelper.CreateNew(Factory, Core.Constants.CountryCodes.India, "IGM", ApplicationCodeTypeList.Codes.ShippingLine);
				headerINIGM.AMA_JobReference = "IGM001";
				var headerVN = AsycudaManifestHeaderHelper.CreateNew(Factory, CountryCodes.VietNam, "VSW");
				headerVN.AMA_JobReference = "246";
				Factory.Save();

				var anotherFactory = new BusinessObjectFactory();
				CombineAssertions(() =>
				{
					Assert("ZA OUT", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), header1.PK) is Integration.Customs.ZA.IAsycudaManifestHeader);
					Assert("ER NVC", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), header2.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader);
					Assert("ER VOC", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), header3.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader);
					Assert("FJ NVC", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), header4.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader);
					AssertEquals("ER BBK", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), header5.PK).GetType(), typeof(ManifestBase.AsycudaManifestHeader));
					Assert("TRETrade", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), header6.PK) is Integration.Customs.ASYCUDA.TRETrade.IAsycudaManifestHeader);
					Assert("BR NVC", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), header7.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader);
					Assert("EH7", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), header8.PK) is Integration.Customs.IEH7.IAsycudaManifestHeader);
					Assert("EH7", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), header9.PK) is Integration.Customs.IEH7.IAsycudaManifestHeader);
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
					{
						Assert("NewZealand", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerNZ.PK) is Integration.Customs.ASYCUDA.NZManifest.IAsycudaManifestHeader);
					}
					Assert("Singapore", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerSG.PK) is Integration.Customs.ASYCUDA.SGAccess.IAsycudaManifestHeader);
					Assert("SouthAfrica", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerZA.PK) is Integration.Customs.ASYCUDA.ZAManifest.IAsycudaManifestHeader);
					Assert("UnitedStates", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerACE.PK) is Integration.Customs.ASYCUDA.ACEManifest.IAsycudaManifestHeader);
					Assert("Uruguay", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerUY.PK) is Integration.Customs.ASYCUDA.UYManifest.IAsycudaManifestHeader);
					Assert("Mexico", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerMX.PK) is Integration.Customs.ASYCUDA.MXManifest.IAsycudaManifestHeader);
					Assert("Chile", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerCL.PK) is Integration.Customs.ASYCUDA.CLManifest.IAsycudaManifestHeader);
					Assert("United Kingdom", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerUKGVMS.PK) is Integration.Customs.GB.GBGVMS.IAsycudaManifestHeader);
					Assert("United Kingdom", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerUKICS.PK) is Integration.Customs.GB.GBICS.IAsycudaManifestHeader);
					Assert("United Kingdom", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerUKSS.PK) is Integration.Customs.GB.GBICS.IAsycudaManifestHeader);
					Assert("Colombia", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerCO.PK) is Integration.Customs.ASYCUDA.COManifest.IAsycudaManifestHeader);
					Assert("Argentina", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerAR.PK) is Integration.Customs.ASYCUDA.ARManifest.IAsycudaManifestHeader);
					Assert("Taiwan", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerTW.PK) is Integration.Customs.ASYCUDA.TWManifest.IAsycudaManifestHeader);
					Assert("Taiwan BCD", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerTWBCD.PK) is Integration.Customs.ASYCUDA.TWBriefCustomsDeclaration.IAsycudaManifestHeader);
					using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
					{
						Assert("Brazil", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerBR.PK) is Integration.Customs.ASYCUDA.ASYCUDAManifest.IAsycudaManifestHeader);
					}
					Assert("Peru", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerPE.PK) is Integration.Customs.ASYCUDA.PEManifest.IAsycudaManifestHeader);
					Assert("IN CGM", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerINCGM.PK) is Integration.Customs.ASYCUDA.INManifest.ICGMAsycudaManifestHeader);
					Assert("IN IGM", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerINIGM.PK) is Integration.Customs.ASYCUDA.INManifest.IIGMAsycudaManifestHeader);
					Assert("Vietnam", new BusinessObjectFactory().Load(typeof(AsycudaManifestHeader), headerVN.PK) is Integration.Customs.ASYCUDA.VNManifest.IAsycudaManifestHeader);
				});
			}
		}
	}
}
