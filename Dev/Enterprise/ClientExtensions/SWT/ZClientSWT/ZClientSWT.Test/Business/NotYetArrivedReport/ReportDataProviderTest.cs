using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.SWT.Testing
{
	public class ReportDataProviderTest : TestCaseWithFactory
	{
		public void TestLoadData()
		{
			TestCaseHelper.RunClientDbCreateScripts();
			ReportDataProvider dataProvider = new ReportDataProvider();
			dataProvider.LoadData(Factory);
			AssertEquals("Should not have data", false, dataProvider.HasData);
			SetupData();
			dataProvider.LoadData(Factory);
			Assert("data should exist", dataProvider.HasData);
			IEnumerator<ZGuid> enumerator = dataProvider.ConsigneePKs.GetEnumerator();
			enumerator.MoveNext();
			ZGuid consigneePk = enumerator.Current;
			AssertEquals(Consignee.PK, consigneePk);
			IEnumerator<ZGuid> enumerator1 = dataProvider.ConsignorPKs.GetEnumerator();
			enumerator1.MoveNext();
			ZGuid consignorPk = enumerator1.Current;
			AssertEquals(Consignor.PK, consignorPk);
		}

		void SetupData()
		{
			ZDateTime currentDate = ZDateTime.Now;
			GlbCompany.CurrentCompany.SetCountry(Core.Constants.CountryCodes.Australia);
			Guid[] orgPKs = new Guid[] { Consignor.PK.ToGuid() };
			SWTDataRegistry.Instance.ConsignorsList.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, orgPKs);
			ForwardingShipment shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "NZAKL";
			shipment.JS_RL_NKDestination = "AUBNE";
			shipment.ConsigneePK = Consignee.PK;
			shipment.ConsignorPK = Consignor.PK;
			shipment.JS_E_ARV = currentDate.AddDays(3);
			shipment.JS_E_DEP = currentDate.AddDays(-2);
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			ForwardingConsol consol = shipment.Consols.AddNew();
			Transport transportLeg = consol.Transports[0];
			transportLeg.JW_IsLinked = false;
			transportLeg.JW_Vessel = "Vessel";
			transportLeg.JW_RL_NKLoadPort = "NZAKL";
			transportLeg.JW_RL_NKDiscPort = "AUBNE";
			transportLeg.JW_ETA = currentDate.AddDays(2);
			transportLeg.JW_ETD = currentDate.AddDays(-3);
			transportLeg.JW_ATD = currentDate.AddDays(-3);
			Factory.Save();
		}

		OrgHeader Consignee
		{
			get
			{
				return consignee ?? (consignee = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "AUBNE")));
			}
		}

		OrgHeader consignee;
		OrgHeader Consignor
		{
			get
			{
				return consignor ?? (consignor = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_RL_NKClosestPort, "NZAKL")));
			}
		}

		OrgHeader consignor;
	}
}
