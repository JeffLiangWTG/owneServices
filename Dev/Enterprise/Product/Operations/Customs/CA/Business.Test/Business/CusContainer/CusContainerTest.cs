using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusContainer))]
	sealed class CusContainerTest : Customs.Business.Testing.BaseCusContainerTest<CusContainer, JobDeclaration>
	{
		public void TestTypeDecider()
		{
			Assert("Update BaseCusContainerTypeDecider to include a decider for this class", Factory.New(typeof(Customs.Business.BaseCusContainer)).GetType() == typeof(CusContainer));
		}

		public void TestDeclaration()
		{
			var declaration = (JobDeclaration)GetJobDeclaration();
			var container = declaration.CusContainers.AddNew();
			AssertEquals(declaration, container.Declaration);
		}

		public void TestLookupsCachesInstance()
		{
			var container = (CusContainer)GetNewBusinessObject();
			var lookup1 = container.Lookups;
			var lookup2 = container.Lookups;
			AssertEquals(lookup2, lookup1);
		}

		public void TestSettingTypeSetsISOCode()
		{
			var refContainer = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP");
			var container = Factory.New<CusContainer>();
			container.CO_RC = refContainer.PK;
			AssertEquals("42G0", container.CA_ContainerSizeOrISOCode);
		}

		public override void TestFindContainerOnShipmentByContainerNumber()
		{
			var port1 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var port2 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, port1.RL_RN_NKCountryCode }));
			var port3 = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, SQLComparisonOperator.NotEqual, new[] { GlbCompany.CurrentCompany.GC_RN_NKCountryCode, port1.RL_RN_NKCountryCode, port2.RL_RN_NKCountryCode }));
			var localPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_RN_NKCountryCode, GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_RL_NKOrigin = port1.RL_Code;
			shipment.JS_RL_NKDestination = port3.RL_Code;

			var consol1 = (ForwardingConsol)shipment.Consols.AddNew(typeof(ForwardingConsol));
			consol1.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol1.JK_RL_NKLoadPort = port2.RL_Code;
			consol1.JK_RL_NKDischargePort = localPort.RL_Code;
			var container1 = consol1.Containers.AddNew();
			container1.JC_ContainerNum = "CONT1234567";

			var consol2 = (ForwardingConsol)shipment.Consols.AddNew(typeof(ForwardingConsol));
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_RL_NKLoadPort = localPort.RL_Code;
			consol2.JK_RL_NKDischargePort = port3.RL_Code;
			var container2 = consol2.Containers.AddNew();
			container2.JC_ContainerNum = "CONT8901234";

			var line1 = shipment.OuterPackLines.AddNew();
			container1.PackLines.Add(line1);
			line1.JL_ActualWeight = 100m;
			line1.JL_ActualWeightUQ = "LB";

			var line2 = shipment.OuterPackLines.AddNew();
			container2.PackLines.Add(line2);
			line2.JL_ActualWeight = 150m;
			line2.JL_ActualWeightUQ = "LB";

			Factory.Save(); // Stop JobContainer from being deleted
			var dec = GetJobDeclaration();
			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			dec.JE_JS = shipment.PK;
			dec.ShipmentSynchroniser.Synchronise(true);
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(1, dec.CusContainers.Count);
			var cusContainer = dec.CusContainers[0];
			AssertEquals(container1, cusContainer.JobContainer);
			AssertEquals("CONT1234567", cusContainer.CO_ContainerNumber);

			cusContainer.CO_ContainerNumber = "CONT8901234";
			AssertNotEquals(container2, cusContainer.JobContainer);
			AssertEquals(true, cusContainer.JobContainer.IsInDatabase);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;
			AssertEquals(1, dec.CusContainers.Count);
			cusContainer = dec.CusContainers[0];
			AssertEquals("CONT8901234", cusContainer.CO_ContainerNumber);
			AssertEquals(container2, cusContainer.JobContainer);
			AssertNotEquals(container1, cusContainer.JobContainer);
			AssertEquals(true, cusContainer.JobContainer.IsInDatabase);
		}

		#region Implementation

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bo)
		{
			return new Customs.Business.BaseCusContainer.CustomLabelsProvider(((CusContainer)bo).Declaration);
		}

		IDisposable asecSetup;

		protected override void SetUp()
		{
			base.SetUp();
			asecSetup = TransactionNumberTestHelper.SetupCompanyASECNumberForTest();
			TransactionNumberTestHelper.SetupTransactionNumberFountainForTest(Factory);
		}

		protected override void TearDown()
		{
			asecSetup.Dispose();
			base.TearDown();
		}

		#endregion
	}
}
