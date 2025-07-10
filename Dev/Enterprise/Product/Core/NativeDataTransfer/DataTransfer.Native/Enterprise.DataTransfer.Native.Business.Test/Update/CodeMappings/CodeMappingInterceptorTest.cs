using CargoWise.EntityFramework;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.CodeMappings;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.CodeMappings
{
	public class CodeMappingInterceptorTest : TransactionedTestCase
	{
		public void TestCodeMapping()
		{
			SetupCodeMapping();

			var definition = TestUtil.FindEntityDefinition("UNLOCO", "RefUNLOCO");
			var root = new Entity(definition, sessionServices) { Action = EntityAction.UPDATE };
			root["Code"] = foreignCode;

			var entitySet = new EntitySet("UNLOCO") { Root = root };

			handler.Function = DummyMethod;
			handler.Invoke(entitySet);

			AssertEquals(localCode, root["Code"]);
		}

		public void TestCodeMapping_OwnerCode_Is_Not_Empty()
		{
			var ownerOrg = "ABCDEFG";
			SetupData(ownerOrg);

			var definition = TestUtil.FindEntityDefinition("UNLOCO", "RefUNLOCO");
			var root = new Entity(definition, sessionServices) { Action = EntityAction.UPDATE };
			root["Code"] = foreignCode;

			var entitySet = new EntitySet("UNLOCO") { Root = root };

			updateSetting.OwnerCode = ownerOrg;

			handler = new CodeMappingInterceptor(updateSetting, sessionServices);
			handler.Function = DummyMethod;
			handler.Invoke(entitySet);

			AssertEquals(localCode, root["Code"]);
		}

		public void TestCodeMapping_With_DifferentTableName()
		{
			SetupCodeMapping();

			var podDefinition = TestUtil.FindEntityDefinition("Order", "JobOrderHeader.PortOfDischarge");
			var portOfDischarge = new Entity(podDefinition, sessionServices) { Action = EntityAction.UPDATE };
			portOfDischarge["Code"] = foreignCode;

			var orderDefinition = TestUtil.FindEntityDefinition("Order", "JobOrderHeader");
			var order = new Entity(orderDefinition, sessionServices);
			order.ParentCollection.Add(portOfDischarge);

			var entitySet = new EntitySet("Order") { Root = order };

			handler = new CodeMappingInterceptor(updateSetting, sessionServices);
			handler.Function = DummyMethod;
			handler.Invoke(entitySet);

			AssertEquals(localCode, portOfDischarge["Code"]);
		}

		public void TestCodeMapping_For_Children()
		{
			SetupCodeMapping();

			var unlocoDefinition = TestUtil.FindEntityDefinition("Country", "RefCountry.RefUNLOCO");
			var unloco = new Entity(unlocoDefinition, sessionServices) { Action = EntityAction.UPDATE };
			unloco["Code"] = foreignCode;

			var countryDefinition = TestUtil.FindEntityDefinition("Country", "RefCountry");
			var country = new Entity(countryDefinition, sessionServices);
			country.ChildrenCollection.Add(unloco);

			var entitySet = new EntitySet("UNLOCO") { Root = country };

			handler = new CodeMappingInterceptor(updateSetting, sessionServices);
			handler.Function = DummyMethod;
			handler.Invoke(entitySet);

			AssertEquals(localCode, unloco["Code"]);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			TestUtil.AlterDummyTable();
			sessionServices = new AncillaryImportServices();
			updateSetting = SetupSetting();
			handler = new CodeMappingInterceptor(updateSetting, sessionServices);
		}

		void SetupCodeMapping()
		{
			var factory = new BusinessObjectFactory();

			var orgPK = EDICodeMapper.GetDefaultOrgPK();
			var orgHeader = factory.Load<OrgHeader>(orgPK);

			var port = factory.New<RefUNLOCO>();
			port.RL_Code = localCode;
			var country = factory.LoadTop1<RefCountry>(new ZQuery());
			port.RL_RN_NKCountryCode = country.Code;

			var relationship = "PTC";

			var ov = orgHeader.CreatePatternMatchOverrideForTest();
			ov.OO_Relationship = relationship;
			ov.OO_OH = orgPK;
			ov.OO_ForeignCode = foreignCode;
			ov.OO_LocalGuid = port.PK;
			factory.Save();
		}

		void SetupData(string ownerOrg)
		{
			var factory = new BusinessObjectFactory();
			var orgHeader = factory.New<OrgHeader>();
			orgHeader.OH_Code = ownerOrg;

			var orgPK = orgHeader.PK.ToGuid();
			var port = factory.New<RefUNLOCO>();
			port.RL_Code = localCode;
			var country = factory.LoadTop1<RefCountry>(new ZQuery());
			port.RL_RN_NKCountryCode = country.Code;

			var relationship = "PTC";

			var ov = orgHeader.CreatePatternMatchOverrideForTest();
			ov.OO_Relationship = relationship;
			ov.OO_OH = orgPK;
			ov.OO_ForeignCode = foreignCode;
			ov.OO_LocalGuid = port.PK;
			factory.Save();
		}

		CodeMappingSetting SetupSetting()
		{
			var result = new CodeMappingSetting();
			var context = new EntityContext(sessionServices, new FactoryProvider());
			result.Context = context;
			return result;
		}

		void DummyMethod(IEntitySet entitySet)
		{
		}

		#endregion

		AncillaryImportServices sessionServices;
		CodeMappingInterceptor handler;
		CodeMappingSetting updateSetting;
		readonly string foreignCode = "ZUBIN";
		readonly string localCode = "ZZ";
	}
}
