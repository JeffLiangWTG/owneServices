using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.SystemMerge.GUI.Testing
{
	sealed class SysMergeProductXmlDataTransferExportorTest : TestCaseWithFactory
	{
		public void TestExportToFile()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "@#$CODE$#@";
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "PartNum";
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_OH = org.PK;
			relation.OU_OP = product.PK;
			Factory.Save();

			var director = new SysMergeProductXmlDataTransferExporter
			{
				DefaultFileName = "ProductXmlExportTestFile_CE4C69C4BDAC49089656406A7E596830"
			};
			ZString expectedFileName = Path.Combine(EnvProxy.Instance.TempPath, director.DefaultFileName + ".xml");
			ZFormModaliser.FileNameToSelectInShowCommonDialog = expectedFileName;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			try
			{
				director.PromptUserAndExport(new BusinessObject[] { org });
				Assert("File should exist: " + expectedFileName, File.Exists(expectedFileName));
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}

		public void TestExportToFileShowErroMessageWhenThereIsNoProducts()
		{
			var director = new SysMergeProductXmlDataTransferExporter();
			director.PromptUserAndExport(Array.Empty<BusinessObject>());
			AssertEquals("There is nothing to export.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestGetElementsToExport()
		{
			Guid product1 = new Guid("A70EB073-C019-4C40-BFBA-43EE07FA51D0");
			Guid product2 = new Guid("B62DB453-84B1-4BCC-AC5D-36C3F0ABA235");
			Guid product3 = new Guid("AB3134AA-78AE-4248-BDFD-40FA6CC17411");
			Guid product4 = new Guid("F640D623-1F33-4E15-B61D-57F9C0D77F52");

			CreateSampleOrganizationAndProduct(product1, "Code1", "LeilaFullName1");
			CreateSampleOrganizationAndProduct(product2, "Code2", "LeilaFullName2");
			CreateSampleOrganizationAndProduct(product3, "Code3", "TestFullName1");
			CreateSampleOrganizationAndProduct(product4, "Code4", "TestFullName2");

			Factory.Save();

			var director = new SysMergeProductXmlDataTransferExporter
			{
				DefaultFileName = "ProductXmlExportTestFile_CE4C69C4BDAC49089656406A7E596830"
			};
			ZString expectedFileName = Path.Combine(EnvProxy.Instance.TempPath, director.DefaultFileName + ".xml");
			ZFormModaliser.FileNameToSelectInShowCommonDialog = expectedFileName;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			try
			{
				var exportQuery = new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "Leila");
				director.PromptUserAndExport(exportQuery);

				XDocument doc = XDocument.Load(expectedFileName);
				XNamespace ns = "http://www.edi.com.au/EnterpriseService/";

				var productQuery = from el in doc.Descendants(ns + "OrgSupplierPart")
								   select el;
				List<XElement> prodctList = productQuery.ToList();
				AssertEquals(2, prodctList.Count);

				List<string> productPkList = prodctList.Select(c => c.Element(ns + "PK").Value).ToList();
				Assert(productPkList.Contains(product1.ToString()));
				Assert(productPkList.Contains(product2.ToString()));

				Assert("File should exist: " + expectedFileName, File.Exists(expectedFileName));
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}

		public void CreateSampleOrganizationAndProduct(Guid pk, string code, string fullName)
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = code;
			org.OH_FullName = fullName;
			var product = Factory.NewWithPrimaryKey<OrgSupplierPart>(pk);
			product.OP_PartNum = product.PK.ToString().Replace("-", "");
			var relation = Factory.New<OrgPartRelation>();
			relation.OU_OH = org.PK;
			relation.OU_OP = product.PK;
		}
	}
}
