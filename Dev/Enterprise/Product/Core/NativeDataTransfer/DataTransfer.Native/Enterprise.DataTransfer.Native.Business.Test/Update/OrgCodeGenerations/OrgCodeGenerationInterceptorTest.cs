using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Organizations.CodeGeneration;
using Enterprise.DataTransfer.Native.Adapter;
using Enterprise.DataTransfer.Native.Adapter.ImportServices;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Business.Update.CodeMappings;
using Enterprise.DataTransfer.Native.Common;
using Enterprise.DataTransfer.Native.Common.Converters;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Finders;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgCodeGenerations
{
	public class OrgCodeGenerationInterceptorTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestInvoke_When_CodeMapping_Existed()
		{
			repository.Setup(m => m.MapLocalCode(It.IsAny<OrgPatternMatchOverride>()))
				.Returns("ABC");

			interceptor.Function = DummyMethod;
			interceptor.Invoke(entitySet);
			repository.Verify(m => m.CreateCodeMapping(It.IsAny<Common.CodeMappings.CodeMapping>()), Times.Never());
		}

		[ExpectNoExceptions]
		public void TestInvoke_Entity_Is_Not_InsertOrMerge()
		{
			// 1. Org Code should not be Generated
			// 2. New Record for Code Mapping should be Created
			// 3. Organization Code should not be replaced by Generated Code

			// Arrange
			var definition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var org = new Entity(definition, sessionServices);
			org.Action = EntityAction.DELETE;
			org["Code"] = "ForeignCode";
			entitySet = new EntitySet("Organization") { Root = org };

			repository.Setup(m => m.MapLocalCode(It.IsAny<OrgPatternMatchOverride>()))
				.Returns(string.Empty);
			converter.Setup(m => m.Convert(It.IsAny<IEntity>()))
				.Returns((IOrgCodeInfo)null);

			// Act
			interceptor.Function = DummyMethod;
			interceptor.Invoke(entitySet);

			// Assert
			generator.Verify(
				m => m.Generate(It.IsAny<IOrgCodeInfo>(), It.IsAny<BusinessObjectFactory>()),
				Times.Never());

			repository.Verify(
				m => m.CreateCodeMapping(It.IsAny<Common.CodeMappings.CodeMapping>()),
				Times.Never());

			AssertEquals("ForeignCode", entitySet.Root["Code"]);
		}

		[ExpectNoExceptions]
		public void TestInvoke_EntityCode_Is_Not_Existed()
		{
			// 1. Org Code should be Generated
			// 2. No Record for Code Mapping is Created
			// 3. Organization Code should be replaced by Generated Code

			// Arrange
			var definition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var org = new Entity(definition, sessionServices);
			org.Action = EntityAction.MERGE;
			entitySet = new EntitySet("Organization") { Root = org };

			repository.Setup(m => m.MapLocalCode(It.IsAny<OrgPatternMatchOverride>()))
				.Returns(string.Empty);
			converter.Setup(m => m.Convert(It.IsAny<IEntity>()))
				.Returns((IOrgCodeInfo)null);
			generator.Setup(m => m.Generate(It.IsAny<IOrgCodeInfo>(), It.IsAny<BusinessObjectFactory>()))
				.Returns("ABC");

			// Act
			interceptor.Function = DummyMethod;
			interceptor.Invoke(entitySet);

			// Assert
			generator.Verify(
				m => m.Generate(It.IsAny<IOrgCodeInfo>(), It.IsAny<BusinessObjectFactory>()),
				Times.Once());
			repository.Verify(
				m => m.CreateCodeMapping(It.IsAny<Common.CodeMappings.CodeMapping>()),
				Times.Never());

			AssertEquals("ABC", entitySet.Root["Code"]);
		}

		[ExpectNoExceptions]
		public void TestInvoke()
		{
			// 1. Org Code should be Generated
			// 2. New Record for Code Mapping should be Created
			// 3. Organization Code should be replaced by Generated Code

			// Arrange
			var definition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var org = new Entity(definition, sessionServices);
			org.Action = EntityAction.MERGE;
			org["Code"] = "EFG";
			entitySet = new EntitySet("Organization") { Root = org };

			repository.Setup(m => m.MapLocalCode(It.IsAny<OrgPatternMatchOverride>()))
				.Returns(string.Empty);
			converter.Setup(m => m.Convert(It.IsAny<IEntity>()))
				.Returns((IOrgCodeInfo)null);
			generator.Setup(m => m.Generate(It.IsAny<IOrgCodeInfo>(), It.IsAny<BusinessObjectFactory>()))
				.Returns("ABC");

			// Act
			interceptor.Function = DummyMethod;
			interceptor.Invoke(entitySet);

			// Assert
			generator.Verify(
				m => m.Generate(It.IsAny<IOrgCodeInfo>(), It.IsAny<BusinessObjectFactory>()),
				Times.Once());
			repository.Verify(
				m => m.CreateCodeMapping(It.IsAny<Common.CodeMappings.CodeMapping>()),
				Times.Once());

			AssertEquals("ABC", entitySet.Root["Code"]);
		}

		[ExpectNoExceptions]
		public void TestInvoke_AncillaryImportServices_CodeMapping_Duplicate()
		{
			// 1. Org Code should be Generated
			// 2. New Record for Code Mapping should be Created
			// 3. Organization Code should be replaced by Generated Code

			// Arrange
			var definition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var org = new Entity(definition, sessionServices);
			org.Action = EntityAction.MERGE;
			org["Code"] = "EFG";
			entitySet = new EntitySet("Organization") { Root = org };

			repository.Setup(m => m.MapLocalCode(It.IsAny<OrgPatternMatchOverride>()))
				.Returns(string.Empty);
			converter.Setup(m => m.Convert(It.IsAny<IEntity>()))
				.Returns((IOrgCodeInfo)null);
			generator.Setup(m => m.Generate(It.IsAny<IOrgCodeInfo>(), It.IsAny<BusinessObjectFactory>()))
				.Returns("ABC");

			// Act
			interceptor.Function = DummyMethod;
			interceptor.Invoke(entitySet);
			var code1 = entitySet.Root["Code"];
			interceptor.Invoke(entitySet);
			var code2 = entitySet.Root["Code"];

			// Assert
			repository.Verify(
							m => m.CreateCodeMapping(It.IsAny<Common.CodeMappings.CodeMapping>()),
							Times.Exactly(2));	// Called once per invocation.
			AssertEquals("ABC", code1);
			AssertEquals("ABC", code2);
		}

		public void TestUpdateBrandNewOrgJustCreated()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "DUMMY CORP";
			orgHeader.OH_IsActive = true;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Fly Street";
			address.OA_City = "SYDNEY";
			address.OA_Email = "Jone.Don@cargowise.com";

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Jone.Don";
			contact.OC_Phone = "+6182308001";

			Factory.Save();

			using (var targetStream = new MemoryStream())
			{
				Export(orgHeader, targetStream);

				targetStream.Position = 0;
				Assert(new StreamReader(targetStream).ReadToEnd().Contains(string.Format(@"<PK>{0}</PK>
        <Code>DUMCORSYD</Code>", orgHeader.PK)));

				var importHandler = Import(targetStream);

				string expectedLog = @"OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgMiscServ - 0 inserts, 0 updates, 0 deletes
OrgContact - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 0 inserts, 0 updates, 0 deletes".Trim();

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
				var pattern = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals(1, pattern.Length);
				AssertEquals("DUMCORSYD", pattern.First().OO_ForeignCode);
				AssertEquals("DUMCORSYD", pattern.First().OO_LocalCode);
				Factory.ReloadAll<OrgHeader>();
				orgHeader = Factory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals("DUMCORSYD", orgHeader.OH_Code);
			}
		}

		public void TestUpdateBrandNewOrgJustCreated_ChangeCode()
		{
			var orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(0, orgPatternMatchOverride.Length);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "DUMMY CORP";
			orgHeader.OH_IsActive = true;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Fly Street";
			address.OA_City = "SYDNEY";
			address.OA_Email = "Jone.Don@cargowise.com";

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Jone.Don";
			contact.OC_Phone = "+6182308001";

			Factory.Save();

			using (var targetStream = new MemoryStream())
			{
				Export(orgHeader, targetStream);

				targetStream.Position = 0;
				var xml = XElement.Load(targetStream);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				var testCode = "TESTCODE";
				xml.Descendants(ns + "Code").First().Value = testCode;
				targetStream.SetLength(0);
				xml.Save(targetStream);
				targetStream.Position = 0;
				Assert(new StreamReader(targetStream).ReadToEnd().Contains(string.Format(@"<PK>{0}</PK>
        <Code>{1}</Code>", orgHeader.PK, testCode)));

				var importHandler = Import(targetStream);

				string expectedLog = @"OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgMiscServ - 0 inserts, 0 updates, 0 deletes
OrgContact - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 0 inserts, 0 updates, 0 deletes".Trim();

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals(1, orgPatternMatchOverride.Length);
				AssertEquals("TESTCODE", orgPatternMatchOverride.First().OO_ForeignCode);
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_LocalCode);
				Factory.ReloadAll<OrgHeader>();
				orgHeader = Factory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals("New code is linked to old code in OrgPatternMatchOverride but OrgHeader code should be unchanged", "DUMCORSYD", orgHeader.OH_Code);
			}
		}

		public void TestUpdateBrandNewOrgJustCreated_ChangeName()
		{
			var orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(0, orgPatternMatchOverride.Length);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "DUMMY CORP";
			orgHeader.OH_IsActive = true;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Fly Street";
			address.OA_City = "SYDNEY";
			address.OA_Email = "Jone.Don@cargowise.com";

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Jone.Don";
			contact.OC_Phone = "+6182308001";

			Factory.Save();

			using (var targetStream = new MemoryStream())
			{
				Export(orgHeader, targetStream);

				targetStream.Position = 0;
				var xml = XElement.Load(targetStream);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				var testFullName = "FullName Test";
				xml.Descendants(ns + "FullName").First().Value = testFullName;
				targetStream.SetLength(0);
				xml.Save(targetStream);
				targetStream.Position = 0;

				Assert(new StreamReader(targetStream).ReadToEnd().Contains(string.Format(@"<PK>{0}</PK>
        <Code>DUMCORSYD</Code>
        <IsActive>true</IsActive>
        <FullName>{1}</FullName>", orgHeader.PK, testFullName)));

				var importHandler = Import(targetStream);

				string expectedLog = @"OrgHeader - 0 inserts, 1 updates, 0 deletes
OrgMiscServ - 0 inserts, 0 updates, 0 deletes
OrgContact - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 0 inserts, 0 updates, 0 deletes".Trim();

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals(1, orgPatternMatchOverride.Length);
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_ForeignCode);
				AssertEquals("FULTESSYD", orgPatternMatchOverride.First().OO_LocalCode);
				Factory.ReloadAll<OrgHeader>();
				orgHeader = Factory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals("FULTESSYD", orgHeader.OH_Code);
				var orgHeaders = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "FULTESSYD1"));
				AssertEquals(0, orgHeaders.Length);
			}
		}

		public void TestUpdateBrandNewOrgJustCreated_ChangePK_NotDeleteSubPKs()
		{
			var orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(0, orgPatternMatchOverride.Length);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "DUMMY CORP";
			orgHeader.OH_IsActive = true;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Fly Street";
			address.OA_City = "SYDNEY";
			address.OA_Email = "Jone.Don@cargowise.com";

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Jone.Don";
			contact.OC_Phone = "+6182308001";

			Factory.Save();

			using (var targetStream = new MemoryStream())
			{
				Export(orgHeader, targetStream);

				targetStream.Position = 0;
				var xml = XElement.Load(targetStream);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				var pk = Guid.NewGuid();
				xml.Descendants(ns + "PK").First().Value = pk.ToString();
				targetStream.SetLength(0);
				xml.Save(targetStream);
				targetStream.Position = 0;

				Assert(new StreamReader(targetStream).ReadToEnd().Contains(string.Format(@"<PK>{0}</PK>", pk)));

				var importHandler = Import(targetStream);

				string expectedLog =
@"Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals(0, orgPatternMatchOverride.Length);
				Factory.ReloadAll<OrgHeader>();
				orgHeader = Factory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals("DUMCORSYD", orgHeader.OH_Code);
			}
		}

		public void TestUpdateBrandNewOrgJustCreated_ChangePK_DeleteSubPKs()
		{
			var orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(0, orgPatternMatchOverride.Length);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "DUMMY CORP";
			orgHeader.OH_IsActive = true;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			Factory.Save();

			using (var targetStream = new MemoryStream())
			{
				Export(orgHeader, targetStream);

				targetStream.Position = 0;
				var xml = XElement.Load(targetStream);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				var pk = Guid.NewGuid();
				xml.Descendants(ns + "PK").First().Value = pk.ToString();
				xml.Descendants(ns + "OrgMiscServ").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "EXDefaultCntryOfOrigin").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "FWDefCurrency").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "OrgAddress").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "OrgAddressCapability").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "RelatedPortCode").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "CountryCode").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "OrgCompanyData").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "OrgInvoiceRollupOrGroup").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "APDefltCurrency").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "ARDDefltCurrency").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "ControllingBranch").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "GlbCompany").Elements(ns + "PK").First().Remove();
				xml.Descendants(ns + "ClosestPort").Elements(ns + "PK").First().Remove();
				targetStream.SetLength(0);
				xml.Save(targetStream);
				targetStream.Position = 0;

				//AssertEquals("", new StreamReader(targetStream).ReadToEnd());
				Assert(new StreamReader(targetStream).ReadToEnd().Contains(string.Format(@"<PK>{0}</PK>", pk)));

				var importHandler = Import(targetStream);

				string expectedLog = @"OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes";

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals(1, orgPatternMatchOverride.Length);
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_ForeignCode);
				AssertEquals("DUMCORSYD1", orgPatternMatchOverride.First().OO_LocalCode);
				Factory.ReloadAll<OrgHeader>();
				orgHeader = Factory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals("DUMCORSYD", orgHeader.OH_Code);
				var orgHeader1 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "DUMCORSYD1"));
				AssertNotNull(orgHeader1);
			}
		}

		public void TestUpdateBrandNewOrgJustCreated_NoPK()
		{
			var orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(0, orgPatternMatchOverride.Length);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "DUMMY CORP";
			orgHeader.OH_IsActive = true;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Fly Street";
			address.OA_City = "SYDNEY";
			address.OA_Email = "Jone.Don@cargowise.com";

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Jone.Don";
			contact.OC_Phone = "+6182308001";

			Factory.Save();

			using (var targetStream = new MemoryStream())
			{
				Export(orgHeader, targetStream);

				targetStream.Position = 0;
				var xml = XElement.Load(targetStream);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				xml.Descendants(ns + "PK").Remove();
				targetStream.SetLength(0);
				xml.Save(targetStream);
				targetStream.Position = 0;

				Assert(!new StreamReader(targetStream).ReadToEnd().Contains("<PK>"));
				var importHandler = Import(targetStream);

				string expectedLog = @"OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgMiscServ - 0 inserts, 0 updates, 0 deletes
OrgContact - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 0 inserts, 0 updates, 0 deletes".Trim();

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals(1, orgPatternMatchOverride.Length);
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_ForeignCode);
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_LocalCode);
				Factory.ReloadAll<OrgHeader>();
				orgHeader = Factory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals("DUMCORSYD", orgHeader.OH_Code);
				var orgHeaders = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "DUMCORSYD1"));
				AssertEquals(0, orgHeaders.Length);
			}
		}

		public void TestUpdateBrandNewOrgJustCreated_NoPK_NoCode()
		{
			var orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(0, orgPatternMatchOverride.Length);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "DUMMY CORP";
			orgHeader.OH_IsActive = true;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Fly Street";
			address.OA_City = "SYDNEY";
			address.OA_Email = "Jone.Don@cargowise.com";

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Jone.Don";
			contact.OC_Phone = "+6182308001";

			Factory.Save();

			using (var targetStream = new MemoryStream())
			{
				Export(orgHeader, targetStream);

				targetStream.Position = 0;
				var xml = XElement.Load(targetStream);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				xml.Descendants(ns + "PK").Remove();
				xml.Descendants(ns + "Code").Remove();
				targetStream.SetLength(0);
				xml.Save(targetStream);
				targetStream.Position = 0;

				Assert(!new StreamReader(targetStream).ReadToEnd().Contains("<PK>"));

				var importHandler = Import(targetStream);

				string expectedLog =
@"Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.".Trim();

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
			}
		}

		public void TestUpdateBrandNewOrgJustCreated_1ForeignCode_2LocalCode()
		{
			var orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(0, orgPatternMatchOverride.Length);

			var pk = Guid.NewGuid();
			var orgXML1 = string.Format(orgXML, "true", pk, "DUMCORSYD", "AAAA BBBB");
			var orgXML2 = string.Format(orgXML, "true", pk, "DUMCORSYD", "CCCC DDDD");

			using (var orgStream1 = new MemoryStream(Encoding.UTF8.GetBytes(orgXML1)))
			using (var orgStream2 = new MemoryStream(Encoding.UTF8.GetBytes(orgXML2)))
			{
				var importHandler = Import(orgStream1);
				importHandler.Import(orgStream2);

				string expectedLog = @"OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
OrgCountryData - 1 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes
OrgHeader - 0 inserts, 1 updates, 0 deletes
OrgMiscServ - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 0 inserts, 0 updates, 0 deletes
OrgCountryData - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 0 inserts, 0 updates, 0 deletes".Trim();

				AssertEquals("Only orgHeader FullName should be updated", expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

				var orgHeader = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "AAABBBSYD"));
				AssertEquals(1, orgHeader.Length);
				AssertEquals("AAABBBSYD", orgHeader.First().OH_Code);
				AssertEquals("CCCC DDDD", orgHeader.First().OH_FullName);
				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_ForeignCode);
				AssertEquals("AAABBBSYD", orgPatternMatchOverride.First().OO_LocalCode);
				AssertEquals(1, orgPatternMatchOverride.Length);
			}
		}

		public void TestUpdateBrandNewOrgJustCreated_HavingCodeMapping_NoPK()
		{
			var orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(0, orgPatternMatchOverride.Length);

			var pk = Guid.NewGuid();
			var orgXML1 = string.Format(orgXML, "true", pk, "DUMCORSYD", "AAAA BBBB");

			using (var orgStream1 = new MemoryStream(Encoding.UTF8.GetBytes(orgXML1)))
			{
				var importHandler = Import(orgStream1);

				string expectedLog = @"OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
OrgCountryData - 1 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes".Trim();

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

				((MemoryLogger)sessionServices.Logger).Clear();
				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals(1, orgPatternMatchOverride.Length);
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_ForeignCode);
				AssertEquals("AAABBBSYD", orgPatternMatchOverride.First().OO_LocalCode);
				var oldLocalGuid = orgPatternMatchOverride.First().OO_LocalGuid;
				AssertNotEquals(pk, oldLocalGuid);

				orgStream1.Position = 0;
				var xml = XElement.Parse(orgXML1);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				xml.Descendants(ns + "PK").First().Remove();
				orgStream1.SetLength(0);
				xml.Save(orgStream1);
				orgStream1.Position = 0;

				Assert(!new StreamReader(orgStream1).ReadToEnd().Contains("<PK>" + pk + "</PK>"));

				importHandler = Import(orgStream1);

				expectedLog = @"OrgHeader - 0 inserts, 0 updates, 0 deletes
OrgMiscServ - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 0 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 0 inserts, 0 updates, 0 deletes
OrgCountryData - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 0 inserts, 0 updates, 0 deletes".Trim();

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals(1, orgPatternMatchOverride.Length);
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_ForeignCode);
				AssertEquals("AAABBBSYD", orgPatternMatchOverride.First().OO_LocalCode);
				AssertEquals(oldLocalGuid, orgPatternMatchOverride.First().OO_LocalGuid);
			}
		}

		public void TestUpdateBrandNewOrgJustCreated_HavingCodeMapping_ChangePK_ChangeName()
		{
			var orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(0, orgPatternMatchOverride.Length);

			var pk = Guid.NewGuid();
			var orgXML1 = string.Format(orgXML, "true", pk, "DUMCORSYD", "AAAA BBBB");

			using (var orgStream1 = new MemoryStream())
			{
				var bytes = Encoding.UTF8.GetBytes(orgXML1);
				orgStream1.Write(bytes, 0, bytes.Length);
				var importHandler = Import(orgStream1);

				string expectedLog = @"OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
OrgCountryData - 1 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes".Trim();

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

				((MemoryLogger)sessionServices.Logger).Clear();
				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals(1, orgPatternMatchOverride.Length);
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_ForeignCode);
				AssertEquals("AAABBBSYD", orgPatternMatchOverride.First().OO_LocalCode);
				var oldLocalGuid = orgPatternMatchOverride.First().OO_LocalGuid;
				AssertNotEquals(pk, oldLocalGuid);

				var orgHeader = Factory.Load<OrgHeader>(oldLocalGuid);
				AssertNotNull(orgHeader);

				orgStream1.Position = 0;
				var xml = XElement.Parse(orgXML1);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				var newPk = Guid.NewGuid();
				xml.Descendants(ns + "PK").First().Value = newPk.ToString();
				xml.Descendants(ns + "FullName").First().Value = "TEST COMP";
				foreach (var node in xml.Descendants(ns + "Language"))
				{
					node.Value = Core.SharedConstants.Languages.ChineseSimplified;
				}
				orgStream1.SetLength(0);
				xml.Save(orgStream1);
				orgStream1.Position = 0;

				Assert(new StreamReader(orgStream1).ReadToEnd().Contains("<PK>" + newPk + "</PK>"));

				importHandler = Import(orgStream1);

				expectedLog = @"OrgHeader - 0 inserts, 1 updates, 0 deletes
OrgMiscServ - 0 inserts, 0 updates, 0 deletes
OrgAddress - 0 inserts, 1 updates, 0 deletes
OrgAddressCapability - 0 inserts, 0 updates, 0 deletes
OrgCompanyData - 0 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 0 inserts, 0 updates, 0 deletes
OrgCountryData - 0 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 0 inserts, 0 updates, 0 deletes".Trim();

				AssertEquals("Full Name, Language updated on OrgHeader, Language updated on OrgAddress", expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals(1, orgPatternMatchOverride.Length);
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_ForeignCode);
				AssertEquals("AAABBBSYD", orgPatternMatchOverride.First().OO_LocalCode);
				AssertEquals(oldLocalGuid, orgPatternMatchOverride.First().OO_LocalGuid);

				Factory.ReloadAll<OrgHeader>();
				orgHeader = Factory.Load<OrgHeader>(oldLocalGuid);
				AssertNotNull(orgHeader);
				AssertEquals("AAABBBSYD", orgHeader.OH_Code);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, orgHeader.OH_Language);
				AssertEquals("TEST COMP", orgHeader.OH_FullName);
			}
		}

		public void TestUpdateBrandNewOrgJustCreated_HavingCodeMapping_ChangePK_ChangeName_ExistingOrgWithChangedPK()
		{
			var orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(0, orgPatternMatchOverride.Length);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_FullName = "DUMMY CORP";
			orgHeader.OH_IsActive = true;
			orgHeader.OH_RL_NKClosestPort = "AUSYD";

			var address = orgHeader.Addresses.AddNew();
			address.OA_Address1 = "Fly Street";
			address.OA_City = "SYDNEY";
			address.OA_Email = "Jone.Don@cargowise.com";

			var contact = orgHeader.Contacts.AddNew();
			contact.OC_ContactName = "Jone.Don";
			contact.OC_Phone = "+6182308001";

			Factory.Save();

			var pk = Guid.NewGuid();
			var orgXML1 = string.Format(orgXML, "true", pk, "DUMCORSYD", "AAAA BBBB");

			using (var orgStream1 = new MemoryStream())
			{
				var bytes = Encoding.UTF8.GetBytes(orgXML1);
				orgStream1.Write(bytes, 0, bytes.Length);
				var importHandler = Import(orgStream1);

				string expectedLog = @"OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
OrgCountryData - 1 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes".Trim();

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals(1, orgPatternMatchOverride.Length);
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_ForeignCode);
				AssertEquals("AAABBBSYD", orgPatternMatchOverride.First().OO_LocalCode);
				var oldLocalGuid = orgPatternMatchOverride.First().OO_LocalGuid;
				AssertNotEquals(pk, oldLocalGuid);

				var orgHeader1 = Factory.Load<OrgHeader>(oldLocalGuid);
				AssertNotNull(orgHeader1);

				orgStream1.Position = 0;
				var xml = XElement.Parse(orgXML1);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				xml.Descendants(ns + "PK").First().Value = orgHeader.PK.ToString();
				xml.Descendants(ns + "FullName").First().Value = "TEST COMP";
				foreach (var node in xml.Descendants(ns + "Language"))
				{
					node.Value = "FNC";
				}
				orgStream1.SetLength(0);
				xml.Save(orgStream1);
				orgStream1.Position = 0;

				Assert(new StreamReader(orgStream1).ReadToEnd().Contains("<PK>" + orgHeader.PK + "</PK>"));

				importHandler = Import(orgStream1);
				expectedLog =
@"Error occurred trying to import file. Please fix the error and try importing the file again.
No insert/update action performed.";

				AssertContains(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));
			}
		}

		public void TestUpdateBrandNewOrgJustCreated_HavingCodeMapping_ChangeCode()
		{
			var orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(0, orgPatternMatchOverride.Length);

			var pk = Guid.NewGuid();
			var orgXML1 = string.Format(orgXML, "true", pk, "DUMCORSYD", "AAAA BBBB");

			using (var orgStream1 = new MemoryStream())
			{
				var bytes = Encoding.UTF8.GetBytes(orgXML1);
				orgStream1.Write(bytes, 0, bytes.Length);
				var importHandler = Import(orgStream1);

				string expectedLog = @"OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
OrgCountryData - 1 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes".Trim();

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

				((MemoryLogger)sessionServices.Logger).Clear();
				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
				AssertEquals(1, orgPatternMatchOverride.Length);
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_ForeignCode);
				AssertEquals("AAABBBSYD", orgPatternMatchOverride.First().OO_LocalCode);
				var oldLocalGuid = orgPatternMatchOverride.First().OO_LocalGuid;
				AssertNotEquals("AAABBBSYD", oldLocalGuid);

				var orgHeader = Factory.Load<OrgHeader>(oldLocalGuid);
				AssertNotNull(orgHeader);

				orgStream1.Position = 0;
				var xml = XElement.Parse(orgXML1);
				XNamespace ns = "http://www.cargowise.com/Schemas/Native/2011/11";
				xml.Descendants(ns + "Code").First().Value = "TESCOMSYD";
				foreach (var node in xml.Descendants(ns + "Language"))
				{
					node.Value = Core.SharedConstants.Languages.ChineseSimplified;
				}
				orgStream1.SetLength(0);
				xml.Save(orgStream1);
				orgStream1.Position = 0;

				importHandler = Import(orgStream1);

				expectedLog = @"OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
OrgCountryData - 1 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes".Trim();

				AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

				var query = new ZQuery();
				query.OrderBy = OrgPatternMatchOverrideSchema.Constants.OO_LocalCode;
				orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(query);
				AssertEquals(2, orgPatternMatchOverride.Length);
				AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_ForeignCode);
				AssertEquals("AAABBBSYD", orgPatternMatchOverride.First().OO_LocalCode);
				AssertEquals(oldLocalGuid, orgPatternMatchOverride.First().OO_LocalGuid);
				AssertEquals("TESCOMSYD", orgPatternMatchOverride[1].OO_ForeignCode);
				AssertEquals("AAABBBSYD1", orgPatternMatchOverride[1].OO_LocalCode);
				AssertNotEquals(oldLocalGuid, orgPatternMatchOverride[1].OO_LocalGuid);

				orgHeader = Factory.Load<OrgHeader>(orgPatternMatchOverride[1].OO_LocalGuid);
				AssertNotNull(orgHeader);
				AssertEquals("AAABBBSYD1", orgHeader.OH_Code);
				AssertEquals(Core.SharedConstants.Languages.ChineseSimplified, orgHeader.OH_Language);
				AssertEquals("AAAA BBBB", orgHeader.OH_FullName);
			}
		}

		public void TestHavingCodeMapping_WhenGenerateCodeRegistryFalse_EnableCodeMappingTrue()
		{
			AssertHavingCodeMappingOrNot_WithDifferentRegistryAndEnableCodeMappingSetting(false, "true", 1);
		}

		public void TestNotHavingCodeMapping_WhenGenerateCodeRegistryFalse_EnableCodeMappingFalse()
		{
			AssertHavingCodeMappingOrNot_WithDifferentRegistryAndEnableCodeMappingSetting(false, "false", 0);
		}

		public void TestHavingCodeMapping_WhenGenerateCodeRegistryTrue_EnableCodeMappingTrue()
		{
			AssertHavingCodeMappingOrNot_WithDifferentRegistryAndEnableCodeMappingSetting(true, "true", 1);
		}

		public void TestNotHavingCodeMapping_WhenGenerateCodeRegistryTrue_EnableCodeMappingFalse()
		{
			AssertHavingCodeMappingOrNot_WithDifferentRegistryAndEnableCodeMappingSetting(true, "false", 0);
			var orgHeaders = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "DUMCORSYD"));
			AssertEquals(1, orgHeaders.Length);
		}

		void AssertHavingCodeMappingOrNot_WithDifferentRegistryAndEnableCodeMappingSetting(bool canUserEditOrganisationCode, string enableCodeMapping, int expectedOrgPatternMatchOverrideCount)
		{
			var orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
			AssertEquals(0, orgPatternMatchOverride.Length);

			var originalValue = RawDataRegistry.Instance.CanUserEditOrganisationCode.Value;
			RawDataRegistry.Instance.CanUserEditOrganisationCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, canUserEditOrganisationCode);
			try
			{
				using (var stream = new MemoryStream())
				{
					var pk = Guid.NewGuid();
					var orgXML1 = string.Format(orgXML, enableCodeMapping, pk, "DUMCORSYD", "AAAA BBBB");
					var bytes = Encoding.UTF8.GetBytes(orgXML1);
					stream.Write(bytes, 0, bytes.Length);
					var importHandler = Import(stream);

					var expectedLog = @"OrgHeader - 1 inserts, 0 updates, 0 deletes
OrgMiscServ - 1 inserts, 0 updates, 0 deletes
OrgAddress - 1 inserts, 0 updates, 0 deletes
OrgAddressCapability - 1 inserts, 0 updates, 0 deletes
OrgCompanyData - 1 inserts, 0 updates, 0 deletes
OrgInvoiceRollupOrGroup - 1 inserts, 0 updates, 0 deletes
OrgCountryData - 1 inserts, 0 updates, 0 deletes
OrgRateTariffLevel - 1 inserts, 0 updates, 0 deletes".Trim();

					AssertEquals(expectedLog, string.Join("\r\n", ((MemoryLogger)sessionServices.Logger).Buffer.Logs().Select(log => log.Message).ToArray()));

					((MemoryLogger)sessionServices.Logger).Clear();
					orgPatternMatchOverride = Factory.Load<OrgPatternMatchOverride>(new ZQuery());
					AssertEquals(expectedOrgPatternMatchOverrideCount, orgPatternMatchOverride.Length);

					if (orgPatternMatchOverride.Any())
					{
						AssertEquals("DUMCORSYD", orgPatternMatchOverride.First().OO_ForeignCode);
						AssertEquals("AAABBBSYD", orgPatternMatchOverride.First().OO_LocalCode);
					}
				}
			}
			finally
			{
				RawDataRegistry.Instance.CanUserEditOrganisationCode.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			// Setup EntitySet
			sessionServices = new AncillaryImportServices();
			var definition = TestUtil.FindEntityDefinition("Organization", "OrgHeader");
			var org = new Entity(definition, sessionServices);
			org.Action = EntityAction.MERGE;
			org["Code"] = "ForeignCode";
			entitySet = new EntitySet("Organization") { Root = org };

			// Setup Setting
			setting = new OrgCodeGenerationSetting("ABC")
			{
				Interceptor = interceptor,
				Enable = true,
				EnableCodeMapping = true,
				Context = new UpdateContext(sessionServices, new FactoryProvider())
			};

			mocks = new MockRepository(MockBehavior.Default);
			repository = mocks.Create<ICodeMappingHelper>();
			converter = mocks.Create<IEntityConverter<IOrgCodeInfo>>();
			generator = mocks.Create<IOrgCodeGenerator>();

			// Setup Interceptor
			interceptor = new OrgCodeGenerationInterceptor(setting, sessionServices)
			{
				CodeMappingRepository = repository.Object,
				CodeGenerator = generator.Object,
				CodeInfoConverter = converter.Object
			};
		}
		AncillaryImportServices sessionServices;

		ImportHandler Import(MemoryStream targetStream)
		{
			var importHandler = new ImportHandler(sessionServices);
			targetStream.Position = 0;
			importHandler.Import(targetStream);
			return importHandler;
		}

		static void Export(OrgHeader orgHeader, MemoryStream targetStream)
		{
			var definitionFinder = new DefinitionFinder() { Cache = EntitySetDefinitionCache.GetInstance() };
			var converter = new BusinessObjectToEntityConverter() { DefinitionFinder = definitionFinder };
			var serializer = new NativeXmlSerializer() { Converter = converter };
			var exporter = new NativeXmlExportService() { Serializer = serializer };
			exporter.Export(new[] { orgHeader }, targetStream);
		}

		void DummyMethod(IEntitySet entitySet)
		{
		}

		const string orgXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>EDICUS</OwnerCode>
    <EnableCodeMapping>{0}</EnableCodeMapping>
  </Header>
  <Body>
    <Organization version=""2.0"">
      <OrgHeader Action=""MERGE"">
        <PK>{1}</PK>
        <Code>{2}</Code>
        <IsActive>true</IsActive>
        <FullName>{3}</FullName>
        <IsConsignee>false</IsConsignee>
        <IsConsignor>true</IsConsignor>
        <IsTransportClient>false</IsTransportClient>
        <IsWarehouseClient>false</IsWarehouseClient>
        <IsForwarder>false</IsForwarder>
        <IsShippingProvider>true</IsShippingProvider>
        <IsAirWholesaler>false</IsAirWholesaler>
        <IsSeaWholesaler>false</IsSeaWholesaler>
        <IsRailProvider>false</IsRailProvider>
        <IsLineHaulProvider>false</IsLineHaulProvider>
        <IsMiscFreightServices>false</IsMiscFreightServices>
        <IsAirCTO>false</IsAirCTO>
        <IsAirLine>false</IsAirLine>
        <IsBroker>false</IsBroker>
        <IsContainerYard>false</IsContainerYard>
        <IsLocalTransport>false</IsLocalTransport>
        <IsPackDepot>false</IsPackDepot>
        <IsSeaCTO>false</IsSeaCTO>
        <IsShippingLine>false</IsShippingLine>
        <IsUnpackDepot>false</IsUnpackDepot>
        <IsRailHead>false</IsRailHead>
        <IsRoadFreightDepot>false</IsRoadFreightDepot>
        <IsShippingConsortium>false</IsShippingConsortium>
        <IsFumigationContractor>false</IsFumigationContractor>
        <IsGlobalAccount>false</IsGlobalAccount>
        <IsNationalAccount>false</IsNationalAccount>
        <IsSalesLead>false</IsSalesLead>
        <IsCompetitor>false</IsCompetitor>
        <IsTempAccount>false</IsTempAccount>
        <IsPersonalEffectsAccount>false</IsPersonalEffectsAccount>
        <IsDistributionCentre>false</IsDistributionCentre>
        <IsUserFlag1>false</IsUserFlag1>
        <IsUserFlag2>false</IsUserFlag2>
        <IsUserFlag3>false</IsUserFlag3>
        <IsUserFlag4>false</IsUserFlag4>
        <IsUserFlag5>false</IsUserFlag5>
        <IsUserFlag6>false</IsUserFlag6>
        <IsUserFlag7>false</IsUserFlag7>
        <IsUserFlag8>false</IsUserFlag8>
        <IsUserFlag9>false</IsUserFlag9>
        <IsUserFlag10>false</IsUserFlag10>
        <IsUserFlag11>false</IsUserFlag11>
        <IsUserFlag12>false</IsUserFlag12>
        <IsUserFlag13>false</IsUserFlag13>
        <IsUserFlag14>false</IsUserFlag14>
        <Language>EN</Language>
        <ScreeningStatus>UNK</ScreeningStatus>
        <SystemLastEditTimeUtc>2015-10-20T11:00:00</SystemLastEditTimeUtc>
        <SystemCreateTimeUtc>2015-10-20T10:52:00</SystemCreateTimeUtc>
        <IsUserFlag15>false</IsUserFlag15>
        <IsUserFlag16>false</IsUserFlag16>
        <IsUserFlag17>false</IsUserFlag17>
        <IsUserFlag18>false</IsUserFlag18>
        <IsUserFlag19>false</IsUserFlag19>
        <IsUserFlag20>false</IsUserFlag20>
        <IsUserFlag21>false</IsUserFlag21>
        <IsUserFlag22>false</IsUserFlag22>
        <IsUserFlag23>false</IsUserFlag23>
        <IsUserFlag24>false</IsUserFlag24>
        <OrgMiscServ Action=""MERGE"">
          <PK>bd35a6f9-7503-4e2b-a86c-8cb9acd334e0</PK>
          <Airline3CharCode></Airline3CharCode>
          <IMEftCustomsFromImport>false</IMEftCustomsFromImport>
          <IMEftQuarantineFromImport>false</IMEftQuarantineFromImport>
          <IMEftHoldUntilPayAuthorised>false</IMEftHoldUntilPayAuthorised>
          <IMEstDaysDeliveryAir>0</IMEstDaysDeliveryAir>
          <IMEstDaysDeliveryFCL>0</IMEstDaysDeliveryFCL>
          <IMEstDaysDeliveryLCL>0</IMEstDaysDeliveryLCL>
          <IMMaxEFTAmount>0.0000</IMMaxEFTAmount>
          <IMMinEFTAmount>0.0000</IMMinEFTAmount>
          <IMEFTBankAccount></IMEFTBankAccount>
          <IMEFTBankBSB></IMEFTBankBSB>
          <IMIsGSTDeferred>false</IMIsGSTDeferred>
          <IMMergeCustomsInvoiceLinesBy>NON</IMMergeCustomsInvoiceLinesBy>
          <IMOrderLineAttrib1></IMOrderLineAttrib1>
          <IMOrderLineAttrib2></IMOrderLineAttrib2>
          <IMOrderLineAttrib3></IMOrderLineAttrib3>
          <IMOriginalSeaBills>3</IMOriginalSeaBills>
          <IMCopySeaBills>3</IMCopySeaBills>
          <IMSendImportDocsTo>IMP</IMSendImportDocsTo>
          <IMSendSeaImportDocsTo>IMP</IMSendSeaImportDocsTo>
          <IMImporterCategory>STD</IMImporterCategory>
          <IMAirDepotFreeDays>1</IMAirDepotFreeDays>
          <IMSeaDepotFreeDays>3</IMSeaDepotFreeDays>
          <IMImporterOwnsPartNumbers>false</IMImporterOwnsPartNumbers>
          <IMOrderStatusCodePairList></IMOrderStatusCodePairList>
          <IMOrderLineStatusCodePairList></IMOrderLineStatusCodePairList>
          <IMLastOrderReference></IMLastOrderReference>
          <IMDefaultToNewOrdersToNextOrderNum>false</IMDefaultToNewOrdersToNextOrderNum>
          <IMFCLEquipmentNeeded></IMFCLEquipmentNeeded>
          <IMLCLEquipmentNeeded></IMLCLEquipmentNeeded>
          <IMAirEquipmentNeeded></IMAirEquipmentNeeded>
          <IMDefaultINCOTerm>FOB</IMDefaultINCOTerm>
          <IMAutoImpJobRefered>false</IMAutoImpJobRefered>
          <IMAutoPopulateOwnerRefWithOrderNums>DEF</IMAutoPopulateOwnerRefWithOrderNums>
          <IMPartAttrib1Type></IMPartAttrib1Type>
          <IMPartAttrib1Name></IMPartAttrib1Name>
          <IMPartAttrib1IsMandatory>false</IMPartAttrib1IsMandatory>
          <IMAttrib1IsKey>false</IMAttrib1IsKey>
          <IMPartAttrib2Type></IMPartAttrib2Type>
          <IMPartAttrib2Name></IMPartAttrib2Name>
          <IMPartAttrib2IsMandatory>false</IMPartAttrib2IsMandatory>
          <IMAttrib2IsKey>false</IMAttrib2IsKey>
          <IMPartAttrib3Type></IMPartAttrib3Type>
          <IMPartAttrib3Name></IMPartAttrib3Name>
          <IMPartAttrib3IsMandatory>false</IMPartAttrib3IsMandatory>
          <IMAttrib3IsKey>false</IMAttrib3IsKey>
          <IMSerialNumberIsKey>false</IMSerialNumberIsKey>
          <IMUseSerialNumber>false</IMUseSerialNumber>
          <IMUseExpiryDate>false</IMUseExpiryDate>
          <IMUsePackingDate>false</IMUsePackingDate>
          <IMImporterRequiresOrderNumbersOnDocs>false</IMImporterRequiresOrderNumbersOnDocs>
          <IMJobRequireOrderTrackLink>false</IMJobRequireOrderTrackLink>
          <IMDocumentAddressPreference></IMDocumentAddressPreference>
          <IMDefaultWarehousePickOption>AUT</IMDefaultWarehousePickOption>
          <IMInvoiceDetailReportSort>DEF</IMInvoiceDetailReportSort>
          <IMInvoiceDetailReportSort2>DEF</IMInvoiceDetailReportSort2>
          <IMInvoiceDetailReportSort3>DEF</IMInvoiceDetailReportSort3>
          <IMShowDutyOnWarehouseEntries>false</IMShowDutyOnWarehouseEntries>
          <LandedCostMarginPercent1>0.00</LandedCostMarginPercent1>
          <LandedCostMarginPercent2>0.00</LandedCostMarginPercent2>
          <LandedCostMarginPercent3>0.00</LandedCostMarginPercent3>
          <LastArchiveDate></LastArchiveDate>
          <EXJobRequireOrderTrackLink>false</EXJobRequireOrderTrackLink>
          <EXExporterRequiresOrderNumbersOnDocs>false</EXExporterRequiresOrderNumbersOnDocs>
          <EXGoodsDescription></EXGoodsDescription>
          <EXExporterCategory>STD</EXExporterCategory>
          <EXFCLEquipmentNeeded></EXFCLEquipmentNeeded>
          <EXLCLEquipmentNeeded></EXLCLEquipmentNeeded>
          <EXAirEquipmentNeeded></EXAirEquipmentNeeded>
          <EXAllowedToPrintOriginalBL>false</EXAllowedToPrintOriginalBL>
          <EXDocumentAddressPreference></EXDocumentAddressPreference>
          <EXDefaultDGContactPhoneUsed></EXDefaultDGContactPhoneUsed>
          <EXDefaultInvoicePriceFromProductLastCost></EXDefaultInvoicePriceFromProductLastCost>
          <EXHandlingInstuctions></EXHandlingInstuctions>
          <EXDefaultIncoTerm>FOB</EXDefaultIncoTerm>
          <EXMergeCustomsInvoiceLinesBy>NON</EXMergeCustomsInvoiceLinesBy>
          <EXPreAllocPrefix></EXPreAllocPrefix>
          <FWAgentCategory>STD</FWAgentCategory>
          <FWAgentBelongsToGroup>false</FWAgentBelongsToGroup>
          <FWHandlesAir>false</FWHandlesAir>
          <FWHandlesSea>false</FWHandlesSea>
          <FWHandlesSeaForPortOrCountry></FWHandlesSeaForPortOrCountry>
          <FWHandlesAirForPortOrCountry></FWHandlesAirForPortOrCountry>
          <FWRequestForCreditAllowed>false</FWRequestForCreditAllowed>
          <FWBillCollectFeesOnSingleInvoice>true</FWBillCollectFeesOnSingleInvoice>
          <FWDealDirectlyWithUltimates>false</FWDealDirectlyWithUltimates>
          <FWIATACode></FWIATACode>
          <FWIATAAccountNumber></FWIATAAccountNumber>
          <FWDirectAMSReporter>false</FWDirectAMSReporter>
          <CRCarrierCategory></CRCarrierCategory>
          <SVServicesCategory></SVServicesCategory>
          <CMSalesCategory></CMSalesCategory>
          <CMCompetitorActivity>FRT</CMCompetitorActivity>
          <CMLastCallDate></CMLastCallDate>
          <CMLastUnactionedCallDate></CMLastUnactionedCallDate>
          <CMClientSize></CMClientSize>
          <CMNoOfEmployees>0</CMNoOfEmployees>
          <CMEstimatedDateToClose></CMEstimatedDateToClose>
          <CMGrowthOutlook></CMGrowthOutlook>
          <CMFollowUpDate></CMFollowUpDate>
          <CMDoesExports>false</CMDoesExports>
          <CMDoesImports>false</CMDoesImports>
          <CMUseTradeLaneFigures>true</CMUseTradeLaneFigures>
          <CMTotalClientRevenue>0.0000</CMTotalClientRevenue>
          <CMPercentage>0.000</CMPercentage>
          <CMAcheivableClientRevenue>0.0000</CMAcheivableClientRevenue>
          <CMEstimatedProfit>0.0000</CMEstimatedProfit>
          <CMWarehouseRevenue>0.0000</CMWarehouseRevenue>
          <CMConsultingRevenue>0.0000</CMConsultingRevenue>
          <CMOverallClientRelation>0</CMOverallClientRelation>
          <CMClientsDesireToRemain>0</CMClientsDesireToRemain>
          <CMEaseClientCanBePoached>0</CMEaseClientCanBePoached>
          <CMAmountOfElectronicIntegration>0</CMAmountOfElectronicIntegration>
          <CMOverallEffectOfClientOnAirfreightCosts></CMOverallEffectOfClientOnAirfreightCosts>
          <CMOverallEffectOfClientOnLCLCosts></CMOverallEffectOfClientOnLCLCosts>
          <CMOverallEffectOfClientOnTEUCosts></CMOverallEffectOfClientOnTEUCosts>
          <CMOverallEffectOfClientOnWarehousingCosts></CMOverallEffectOfClientOnWarehousingCosts>
          <CMOverallEffectOfClientOnOtherCosts></CMOverallEffectOfClientOnOtherCosts>
          <CMAmountOfBusinessWon>0</CMAmountOfBusinessWon>
          <CMSalesTerritory></CMSalesTerritory>
          <CMIsHouseAccount>false</CMIsHouseAccount>
          <CMCommission></CMCommission>
          <CMClientCommenced></CMClientCommenced>
          <CMClientPortalHomePage></CMClientPortalHomePage>
          <CICompetitorCategory></CICompetitorCategory>
          <CMDistanceCalculationProvider>DEF</CMDistanceCalculationProvider>
          <CMDistanceCalculationVersion></CMDistanceCalculationVersion>
          <CMDistanceCalculationMethod></CMDistanceCalculationMethod>
          <CMPaidUpCapital>0.0000</CMPaidUpCapital>
          <CMEstablishedDate></CMEstablishedDate>
          <CIEstimatedStaffThisLocation>0</CIEstimatedStaffThisLocation>
          <CITypeOfService></CITypeOfService>
          <CIEstimatedStaffThisCountry>0</CIEstimatedStaffThisCountry>
          <CISellingStyle></CISellingStyle>
          <CITurnover>0.0000</CITurnover>
          <CIProfit>0.0000</CIProfit>
          <CICapitalEmployed>0.0000</CICapitalEmployed>
          <CICompetitiveRanking>0</CICompetitiveRanking>
          <CIStrength></CIStrength>
          <CIWeaknesses></CIWeaknesses>
          <CIOpportunities></CIOpportunities>
          <CIThreats></CIThreats>
          <WhsOverrideSystemPickingRules>false</WhsOverrideSystemPickingRules>
          <WhsPickFromDefaultFIFO>0</WhsPickFromDefaultFIFO>
          <WhsPickFromFullPallets>0</WhsPickFromFullPallets>
          <WhsPickFromConsolidatedFullPallets>0</WhsPickFromConsolidatedFullPallets>
          <WhsPickFromPickFaces>0</WhsPickFromPickFaces>
          <WhsPickFromPalletOverflow>0</WhsPickFromPalletOverflow>
          <WhsPickFromBrokenPallets>0</WhsPickFromBrokenPallets>
          <WhsPickFromFIFOBulkOnly>0</WhsPickFromFIFOBulkOnly>
          <WhsPickSortArrivalDate>0</WhsPickSortArrivalDate>
          <WhsPickSortExpiryDate>0</WhsPickSortExpiryDate>
          <WhsPickSortPackingDate>0</WhsPickSortPackingDate>
          <WhsPickSortPickFace>0</WhsPickSortPickFace>
          <WhsPickSortLocationRow>0</WhsPickSortLocationRow>
          <WhsPickSortLocationColumn>0</WhsPickSortLocationColumn>
          <WhsPickSortLocationLevel>0</WhsPickSortLocationLevel>
          <WhsExpiryNotificationFromDefaults>true</WhsExpiryNotificationFromDefaults>
          <WhsExpiryNotificationPeriod>0</WhsExpiryNotificationPeriod>
          <WhsClientInvoiceFormat></WhsClientInvoiceFormat>
          <WhsOrderNumberUniquenessStrategy></WhsOrderNumberUniquenessStrategy>
          <WhsOverrideSystemPutawayRules>false</WhsOverrideSystemPutawayRules>
          <WhsPutawayToLocation>0</WhsPutawayToLocation>
          <WhsPutawayToPickFace>0</WhsPutawayToPickFace>
          <WhsPutawayToProductArea>0</WhsPutawayToProductArea>
          <WhsPutawayToClientArea>0</WhsPutawayToClientArea>
          <WhsPutawaySortLocationColumn>0</WhsPutawaySortLocationColumn>
          <WhsPutawaySortLocationLevel>0</WhsPutawaySortLocationLevel>
          <WhsPutawaySortLocationRow>0</WhsPutawaySortLocationRow>
          <WhsPutawaySameProductTogether>false</WhsPutawaySameProductTogether>
          <WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>true</WhsEDIChangeMessageCancelOrderLinesNotIncludedInMessage>
          <WhsGenerateBackOrdersOnShortfalls>false</WhsGenerateBackOrdersOnShortfalls>
          <WhsAutoPckEdiOrders>false</WhsAutoPckEdiOrders>
          <WhsOrderFulfillmentRule>NON</WhsOrderFulfillmentRule>
          <WhsPackingSlipOrderBy>DEF</WhsPackingSlipOrderBy>
          <WhsDefaultWarehousePickMode>ASP</WhsDefaultWarehousePickMode>
          <WhsDefaultWarehouseRollUp>false</WhsDefaultWarehouseRollUp>
          <WhsTransportPayer>DEF</WhsTransportPayer>
          <MinimumShelfLifeAccepted>0</MinimumShelfLifeAccepted>
          <WhsABCAnalysisEnabled>false</WhsABCAnalysisEnabled>
          <WhsABCAnalysisMethod>DEF</WhsABCAnalysisMethod>
          <WhsABCAnalysisPeriod>DEF</WhsABCAnalysisPeriod>
          <IsScanPackQtyAllowed>false</IsScanPackQtyAllowed>
          <IsLabelPrintedOnClosePackage>true</IsLabelPrintedOnClosePackage>
          <CustomAttrib1></CustomAttrib1>
          <CustomAttrib2></CustomAttrib2>
          <CustomAttrib3></CustomAttrib3>
          <CustomDate1></CustomDate1>
          <CustomDate2></CustomDate2>
          <CustomDate3></CustomDate3>
          <CustomDecimal1>0.000</CustomDecimal1>
          <CustomDecimal2>0.000</CustomDecimal2>
          <CustomDecimal3>0.000</CustomDecimal3>
          <CustomFlag1>false</CustomFlag1>
          <CustomFlag2>false</CustomFlag2>
          <CustomFlag3>false</CustomFlag3>
          <CustomFlag4>false</CustomFlag4>
          <IMOwnsProducts>false</IMOwnsProducts>
          <EXOwnsProducts>false</EXOwnsProducts>
          <CRVoyageRecyclingPeriodInMonths>-1</CRVoyageRecyclingPeriodInMonths>
          <IsAutoPackAllowed>true</IsAutoPackAllowed>
          <WhsOrderDefaultPickPriority>0</WhsOrderDefaultPickPriority>
          <IMAllowOrders>true</IMAllowOrders>
          <ConsigneeAuthorityToLeave>DEF</ConsigneeAuthorityToLeave>
          <ConsignorAuthorityToLeave>DEF</ConsignorAuthorityToLeave>
          <CMPeriodOfActivity></CMPeriodOfActivity>
          <CMIndustryVertical></CMIndustryVertical>
          <IMDefaultServiceLevel TableName=""RefServiceLevel"" />
          <EXDefaultServiceLevel TableName=""RefServiceLevel"" />
          <EXDefCurrency TableName=""RefCurrency"" />
          <EXDefaultCntryOfOrigin TableName=""RefCountry"">
            <Code>AU</Code>
            <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
          </EXDefaultCntryOfOrigin>
          <EXDefaultDGContact TableName=""OrgContact"" />
          <FWDefCurrency TableName=""RefCurrency"">
            <Code>AUD</Code>
            <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
          </FWDefCurrency>
          <CMMainImportCmdty TableName=""RefCommodityCode"" />
          <CMMainExportCmdty TableName=""RefCommodityCode"" />
          <WhsDefaultWarehouseArea TableName=""WhsArea"" />
          <WhsDefaultWarehouse TableName=""GlbBranch"" />
          <WhsPackingSlip TableName=""StmTemplate"" />
          <CMPreferredPaymentCompany TableName=""GlbCompany"" />
        </OrgMiscServ>
        <OrgAddressCollection>
          <OrgAddress Action=""MERGE"">
            <PK>93e6d2e9-b044-4b7d-93e7-922f8dc48fa9</PK>
            <IsActive>true</IsActive>
            <Code>TEST ADDRESS</Code>
            <Language>EN</Language>
            <CompanyNameOverride></CompanyNameOverride>
            <Address1>TEST ADDRESS</Address1>
            <Address2></Address2>
            <State>NSW</State>
            <PostCode>2000</PostCode>
            <Phone>+61432986999</Phone>
            <Fax></Fax>
            <Mobile></Mobile>
            <PickupFromTimeOnly></PickupFromTimeOnly>
            <PickupToTimeOnly></PickupToTimeOnly>
            <DeliverFromTimeOnly></DeliverFromTimeOnly>
            <DeliverToTimeOnly></DeliverToTimeOnly>
            <DoNotAttendFrom></DoNotAttendFrom>
            <DoNotAttendTo></DoNotAttendTo>
            <DockLeveler>false</DockLeveler>
            <ForkLift>false</ForkLift>
            <PalletJack>false</PalletJack>
            <ContainerHandling></ContainerHandling>
            <AccessPoint></AccessPoint>
            <LabourRequired></LabourRequired>
            <CommunicationRequired></CommunicationRequired>
            <Dock_Height></Dock_Height>
            <FCLEquipmentNeeded>WUP</FCLEquipmentNeeded>
            <LCLEquipmentNeeded>PSL</LCLEquipmentNeeded>
            <AIREquipmentNeeded>PSL</AIREquipmentNeeded>
            <Latitude>0.00000</Latitude>
            <Longitude>0.00000</Longitude>
            <Email></Email>
            <UseCumulativeFreeWaitingTime>false</UseCumulativeFreeWaitingTime>
            <DeliveryRoute></DeliveryRoute>
            <DeliveryRouteSequence>0</DeliveryRouteSequence>
            <OtherWarehouseFacilities></OtherWarehouseFacilities>
            <LoadingUnloadingConstraints></LoadingUnloadingConstraints>
            <ValidationStatus>NYV</ValidationStatus>
            <AddressMap></AddressMap>
            <AuthorityToLeave>DEF</AuthorityToLeave>
            <City>SYDNEY</City>
            <GroupNumber>0</GroupNumber>
            <OrgAddressCapabilityCollection>
              <OrgAddressCapability Action=""MERGE"">
                <PK>bba22062-0c49-43b1-a005-2c3ada473faa</PK>
                <AddressType>OFC</AddressType>
                <IsMainAddress>true</IsMainAddress>
              </OrgAddressCapability>
            </OrgAddressCapabilityCollection>
            <RelatedPortCode TableName=""RefUNLOCO"">
              <Code>AUSYD</Code>
              <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
            </RelatedPortCode>
            <CountryCode TableName=""RefCountry"">
              <Code>AU</Code>
              <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
            </CountryCode>
          </OrgAddress>
        </OrgAddressCollection>
        <OrgCompanyDataCollection>
          <OrgCompanyData Action=""MERGE"">
            <PK>32380ef4-fd4f-4831-b5c3-106ee62cb2d6</PK>
            <IsDebtor>false</IsDebtor>
            <IsCreditor>false</IsCreditor>
            <APCategory></APCategory>
            <APExternalCreditorCode></APExternalCreditorCode>
            <APCreditLimit>0.0000</APCreditLimit>
            <APPayInvoiceAfterPostingDefault>false</APPayInvoiceAfterPostingDefault>
            <APPaymentTermDays>0</APPaymentTermDays>
            <APPaymentTerms>COD</APPaymentTerms>
            <APCreditAgreedPaymentMethod></APCreditAgreedPaymentMethod>
            <APWHTApplicable>false</APWHTApplicable>
            <APAirlineAccountNumber></APAirlineAccountNumber>
            <APQualityAssured>false</APQualityAssured>
            <APQualityAssuredCheckedDate></APQualityAssuredCheckedDate>
            <APCostsSelfBilled>false</APCostsSelfBilled>
            <APPrintContractorForm>false</APPrintContractorForm>
            <APVATConfig>DEF</APVATConfig>
            <ARCategory></ARCategory>
            <ARExternalDebtorCode></ARExternalDebtorCode>
            <ARQualityAssured>false</ARQualityAssured>
            <ARQualityAssuredCheckedDate></ARQualityAssuredCheckedDate>
            <ARAutoUpdateRates>true</ARAutoUpdateRates>
            <ARCombinedStatementInvoice>false</ARCombinedStatementInvoice>
            <ARConsolidatedAccountingCategory></ARConsolidatedAccountingCategory>
            <ARForeignCurrStatement>false</ARForeignCurrStatement>
            <ARBuyersConsolInvoicingStyle>DEF</ARBuyersConsolInvoicingStyle>
            <ARCreditLimit>0.0000</ARCreditLimit>
            <ARTemporaryCreditLimitIncrease>0.0000</ARTemporaryCreditLimitIncrease>
            <ARTemporaryCreditLimitIncreaseExpiry></ARTemporaryCreditLimitIncreaseExpiry>
            <ARCreditRating></ARCreditRating>
            <ARCreditAgreedPaymentMethod></ARCreditAgreedPaymentMethod>
            <ARSeaInvoiceTermDays>COD</ARSeaInvoiceTermDays>
            <ARSeaInvoiceTerms>0</ARSeaInvoiceTerms>
            <ARAirInvoiceTermDays>COD</ARAirInvoiceTermDays>
            <ARAirInvoiceTerms>0</ARAirInvoiceTerms>
            <ARInvoiceTerms>COD</ARInvoiceTerms>
            <ARInvoiceTermDays>0</ARInvoiceTermDays>
            <ARTreatDisbursementsAsStandardValue>0.0000</ARTreatDisbursementsAsStandardValue>
            <ARDisbursementInvoiceTerms>COD</ARDisbursementInvoiceTerms>
            <ARDisbursementInvoiceTermDays>0</ARDisbursementInvoiceTermDays>
            <ARDontShowTaxOnDocs>false</ARDontShowTaxOnDocs>
            <ARSellersConsolInvoicingStyle>DEF</ARSellersConsolInvoicingStyle>
            <AROnCreditHold>false</AROnCreditHold>
            <ARCreditApproved>true</ARCreditApproved>
            <ARUseSettlementGroupCreditLimit>false</ARUseSettlementGroupCreditLimit>
            <ARAccountAndCreditReviewDue></ARAccountAndCreditReviewDue>
            <ARAllowMultiCurrencyPayment>false</ARAllowMultiCurrencyPayment>
            <ARPreviousChequeDrawer></ARPreviousChequeDrawer>
            <ARPreviousChequeDrawerBank></ARPreviousChequeDrawerBank>
            <ARPreviousChequeDrawerBankBranch></ARPreviousChequeDrawerBankBranch>
            <ARReceiptInvoiceAfterPostingDefault>false</ARReceiptInvoiceAfterPostingDefault>
            <AREftCustomsPaymentMethod></AREftCustomsPaymentMethod>
            <ARWHTApplicable>false</ARWHTApplicable>
            <ARIncludeInwardsWhsConsolidatedInvoice>true</ARIncludeInwardsWhsConsolidatedInvoice>
            <ARIncludeOutwardsWhsConsolidatedInvoice>true</ARIncludeOutwardsWhsConsolidatedInvoice>
            <ARWhsStorageCalcMethod>MAX</ARWhsStorageCalcMethod>
            <CRIsShipsAgencyPrincipal>false</CRIsShipsAgencyPrincipal>
            <ARUseSystemDefaultUplifts>true</ARUseSystemDefaultUplifts>
            <ARExportAirCollectUplift>0.000</ARExportAirCollectUplift>
            <ARExportSeaCollectUplift>0.000</ARExportSeaCollectUplift>
            <ARImportAirCollectUplift>0.000</ARImportAirCollectUplift>
            <ARImportSeaCollectUplift>0.000</ARImportSeaCollectUplift>
            <ARUseSystemDefaultUpliftsMinimums>true</ARUseSystemDefaultUpliftsMinimums>
            <ARExportAirCollectUpliftMinimum>0.0000</ARExportAirCollectUpliftMinimum>
            <ARExportSeaCollectUpliftMinimum>0.0000</ARExportSeaCollectUpliftMinimum>
            <ARImportAirCollectUpliftMinimum>0.0000</ARImportAirCollectUpliftMinimum>
            <ARImportSeaCollectUpliftMinimum>0.0000</ARImportSeaCollectUpliftMinimum>
            <ARWarehouseRatingPeriod>DEF</ARWarehouseRatingPeriod>
            <ARCreditCardType></ARCreditCardType>
            <ARCreditCardNum></ARCreditCardNum>
            <ARCreditCardAdditionalInfo></ARCreditCardAdditionalInfo>
            <ARCreditCardExpire></ARCreditCardExpire>
            <ARCreditCardHolder></ARCreditCardHolder>
            <ARVATConfig>DEF</ARVATConfig>
            <RateSecurityGroup></RateSecurityGroup>
            <EXBillAgentChargesDirect>false</EXBillAgentChargesDirect>
            <IMBillAgentChargesDirect>false</IMBillAgentChargesDirect>
            <IMUsedBondedWhs>false</IMUsedBondedWhs>
            <WhsClientFreeStorageDays>0</WhsClientFreeStorageDays>
            <WhsOverrideFreeStorage>false</WhsOverrideFreeStorage>
            <WhsIncludeReleaseChargesOnShipment>true</WhsIncludeReleaseChargesOnShipment>
            <ARCustomerSelfBillsRevenue>false</ARCustomerSelfBillsRevenue>
            <WhsChargeStorageInAdvance>true</WhsChargeStorageInAdvance>
            <OrgInvoiceRollupOrGroupCollection>
              <OrgInvoiceRollupOrGroup Action=""MERGE"">
                <PK>1f6ea073-5e03-4997-b950-e23dd6dac55b</PK>
                <JobType>ALL</JobType>
                <TransportMode>ALL</TransportMode>
                <ServiceDirection>ALL</ServiceDirection>
                <GroupOrSubTotal>DEF</GroupOrSubTotal>
                <GroupOrSubtotalStyle>DEF</GroupOrSubtotalStyle>
                <InvoiceLineDisplayOption>DEF</InvoiceLineDisplayOption>
                <InvoicePostingStyle>DEF</InvoicePostingStyle>
              </OrgInvoiceRollupOrGroup>
            </OrgInvoiceRollupOrGroupCollection>
            <APDefltCurrency TableName=""RefCurrency"">
              <Code>AUD</Code>
              <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
            </APDefltCurrency>
            <APCreditorGroup TableName=""OrgCreditorGroup"" />
            <APDefaultBankAccount TableName=""AccBankAccount"" />
            <APDefaultChargeCode TableName=""AccChargeCode"" />
            <ARPayToAccount TableName=""AccBankAccount"" />
            <ARDDefltCurrency TableName=""RefCurrency"">
              <Code>AUD</Code>
              <PK>53feb23f-a7b0-4e0d-af5d-f6f6e5417399</PK>
            </ARDDefltCurrency>
            <ARDebtorGroup TableName=""OrgDebtorGroup"" />
            <ControllingBranch TableName=""GlbBranch"">
              <Code>SYS</Code>
              <PK>fdd429d2-648c-4895-8f9f-06e90ded2be5</PK>
            </ControllingBranch>
            <GlbCompany>
              <Code>EDI</Code>
              <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
            </GlbCompany>
          </OrgCompanyData>
        </OrgCompanyDataCollection>
        <OrgCountryDataCollection>
          <OrgCountryData Action=""MERGE"">
            <PK>2fbf2b41-6d06-460b-bf11-dc9467f81d3a</PK>
            <EXApprovedOrMajorExporter>NO</EXApprovedOrMajorExporter>
            <EXApprovalMethod></EXApprovalMethod>
            <EXApprovalNumber></EXApprovalNumber>
            <EXExportPermissionDetails></EXExportPermissionDetails>
            <EXApprovalExpiryDate></EXApprovalExpiryDate>
            <EXSiteInspectionDate></EXSiteInspectionDate>
            <EXPermitNumber></EXPermitNumber>
            <EXE3Signed>false</EXE3Signed>
            <LastReviewedOn></LastReviewedOn>
            <ImportCustomsDefaultAddInfo></ImportCustomsDefaultAddInfo>
            <CustomsEconomicGroupAddInfo></CustomsEconomicGroupAddInfo>
            <ImportEntryPaymentPreference></ImportEntryPaymentPreference>
            <ImportQuarantinePaymentPreference></ImportQuarantinePaymentPreference>
            <MakePartsBothImportAndExport>false</MakePartsBothImportAndExport>
            <SystemCreateTimeUtc>2015-10-20T10:52:00</SystemCreateTimeUtc>
            <SystemLastEditTimeUtc>2015-10-20T11:00:00</SystemLastEditTimeUtc>
            <ReviewedByUser TableName=""GlbStaff"" />
            <ClientCountryRelation TableName=""RefCountry"">
              <Code>AU</Code>
              <PK>e4eb6e97-aa78-4c46-bf86-35b7fbb5fb0e</PK>
            </ClientCountryRelation>
            <ApprovedLocation TableName=""OrgAddress"" />
          </OrgCountryData>
        </OrgCountryDataCollection>
        <OrgRateTariffLevelCollection>
          <OrgRateTariffLevel Action=""MERGE"">
            <PK>910209e9-77ff-4526-aff1-dd617097a67a</PK>
            <TariffType>DEF</TariffType>
            <Mode>ALL</Mode>
            <Direction>ALL</Direction>
            <TariffLevel>0</TariffLevel>
            <ApplyGroupRate>false</ApplyGroupRate>
            <GlbCompany>
              <Code>EDI</Code>
              <PK>878d7aca-ffc3-49fc-9710-969ca0c0f2ac</PK>
            </GlbCompany>
          </OrgRateTariffLevel>
        </OrgRateTariffLevelCollection>
        <ClosestPort TableName=""RefUNLOCO"">
          <Code>AUSYD</Code>
          <PK>ad87872e-96b8-4c9a-bc0b-6a4088247a64</PK>
        </ClosestPort>
      </OrgHeader>
    </Organization>
  </Body>
</Native>";

		#endregion

		OrgCodeGenerationInterceptor interceptor;
		OrgCodeGenerationSetting setting;
		EntitySet entitySet;

		Mock<ICodeMappingHelper> repository;
		Mock<IOrgCodeGenerator> generator;
		Mock<IEntityConverter<IOrgCodeInfo>> converter;
		MockRepository mocks;
	}
}
