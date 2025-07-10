using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Native.Adapter.Utils;
using Enterprise.DataTransfer.Native.Common.Definitions.EntitySetDefinitions.Repository;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static NUnit.Framework.XmlAssertions;
using SchemaVersionManager = Enterprise.DataTransfer.Native.Common.SchemaVersionManager;

namespace Enterprise.DataTransfer.Native.Adapter
{
	public class NativeXmlSerializerTest : TransactionedTestCase
	{
		public void TestExportWithNaturalKey()
		{
			var oH_Pk = TestUtil.PrepareOrgHeaderTableData();
			var oC_Pk = TestUtil.PrepareOrgContactTableData(oH_Pk);
			var factory = new BusinessObjectFactory();

			var input = factory.Load<OrgHeader>(oH_Pk);
			var result = exporter.Export(new[] { input });

			using (var reader = new StreamReader(result))
			{
				var element = XElement.Load(reader);
				var pkElements = element.Descendants(ns + "PK");

				AssertEquals("PKs are in Data Elements even if there is a natural key", 2, pkElements.Count());
				AssertEquals("PKs are in Data Elements even if there is a natural key", 1, pkElements.Count(item => item.Value == oH_Pk.ToString()));
				AssertEquals("PKs are in Data Elements even if there is a natural key", 1, pkElements.Count(item => item.Value == oC_Pk.ToString()));
			}
		}

		public void TestExportWithNoNaturalKey()
		{
			TestUtil.AddColumnsToDummy();
			TestUtil.AddDummyBizoToGlobalDefinitions();
			EntitySetDefinitionCache.SetAlternateDefinitionLoaderForTesting(TestUtil.GetDefinitionLoaderForTest());
			var z0_Pk = TestUtil.PrepareDummyTableData();
			var factory = new BusinessObjectFactory();
			var input = factory.Load<DummyBusinessObject>(z0_Pk);
			var result = exporter.Export(new[] { input });

			using (var reader = new StreamReader(result))
			{
				var element = XElement.Load(reader);
				var pkElements = element.Descendants(ns + "PK");

				AssertEquals("PK is in Data Element if there is no natural key", 3, pkElements.Count());
				AssertEquals("PK is in Data Element if there is no natural key", 1, pkElements.Count(item => item.Value == z0_Pk.ToString()));
			}
		}

		public void TestExportWithFilter()
		{
			var rateID = Guid.NewGuid();
			var sqlInsertRefExchangeRate = $@"
INSERT INTO RefDatabase_RefExchangeRateZZ
 ([ZZN_PK], [ZZN_ExRateType], [ZZN_StartDate], [ZZN_EndDate], [ZZN_Rate], [ZZN_RX_NKExCurrency], [ZZN_RN_NKCountry], [ZZN_AsPublished])
VALUES('{rateID}', 'CUS', GETDATE(), DATEADD(day, 1, GETDATE()), 4.8, 'CNY', 'CN', 1);
";

			Db.Connection.ExecuteNonQuery(sqlInsertRefExchangeRate);

			var factory = new BusinessObjectFactory();
			var orgHeader = factory.NewWithValidTestData<OrgHeader>();

			var company1 = factory.NewWithValidTestData<GlbCompany>();
			company1.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			company1.GC_OH_OrgProxy = orgHeader.PK;

			var company2 = factory.NewWithValidTestData<GlbCompany>();
			company2.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			company2.GC_OH_OrgProxy = orgHeader.PK;

			factory.Save();

			rateID = (Guid)Db.Connection.ExecuteScalar($@"SELECT uuid = CONVERT(uniqueidentifier, HASHBYTES('SHA2_256', CONCAT(ZZN_PK, '{company1.GC_Code}'))) FROM RefDatabase_RefExchangeRateZZ
WHERE ZZN_PK = '{rateID}'");

			var rate = factory.LoadTop1<RefExchangeRate>(new ZQuery(RefExchangeRateSchema.PK, rateID));

			CombineAssertions(() =>
			{
				var result = exporter.Export(new[] { rate }, factory,
					table =>
					{
						return table.Rows.Cast<DataRow>().SingleOrDefault(dr => dr.Field<Guid>(RefExchangeRateSchema.RE_GC.Name) == company1.PK);
					});
				using (var reader = new StreamReader(result))
				{
					var element = XElement.Load(reader);
					var ratePK = element.Descendants(ns + "RefExchangeRate").Elements(ns + "PK").Single().Value;

					AssertEquals("PK comparison", rate.PK.ToString(), ratePK);
				}
			});
		}

		public void TestSerializeToStream_2011_11()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(NativeXmlInfo.Namespace_2011_11))
			{
				var oH_Pk = TestUtil.PrepareOrgHeaderTableData();
				var oC_Pk = TestUtil.PrepareOrgContactTableData(oH_Pk);
				var factory = new BusinessObjectFactory();

				var input = factory.Load<OrgHeader>(oH_Pk);
				var result = exporter.SerializeToStream(new[] { input, input });
				result.Position = 0;
				var reader = new StreamReader(result);
				var element = XElement.Load(reader);

				AssertEquals("Native", element.Name.LocalName);
				AssertEquals(NativeXmlInfo.Namespace_2011_11, element.Name.Namespace.ToString());

				AssertEquals(1, element.Descendants(ns + "Body").Count());
				AssertEquals(2, element.Descendants(ns + "Organization").Count());

				var pkElements = element.Descendants(ns + "PK");

				AssertEquals("PKs are in Data Elements even if there is a natural key", 4, pkElements.Count());
				AssertEquals("PKs are in Data Elements even if there is a natural key", 2, pkElements.Count(item => item.Value == oH_Pk.ToString()));
				AssertEquals("PKs are in Data Elements even if there is a natural key", 2, pkElements.Count(item => item.Value == oC_Pk.ToString()));
			}
		}

		public void TestSerializeToStreamContainsTimestamp()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(NativeXmlInfo.Namespace_2011_11))
			{
				var oH_Pk = TestUtil.PrepareOrgHeaderTableData();
				var oC_Pk = TestUtil.PrepareOrgContactTableData(oH_Pk);
				var factory = new BusinessObjectFactory();

				var input = factory.Load<OrgHeader>(oH_Pk);
				var dataContext = DataContextFactory.New();
				dataContext.Timestamp = 1095379198;
				var result = exporter.SerializeToStream(input, dataContext, null);
				result.Position = 0;

				using (var reader = new StreamReader(result))
				{
					var content = reader.ReadToEnd();
					AssertIsXml(content).HavingExactlyOneChildNode("Header/DataContext/Timestamp", n => n.WithValue("1095379198"));
				}
			}
		}

		public void TestExportContainsTimestamp()
		{
			ObjectFactory.Get<IXMLDataProvider>().SetIgnoreTimestampForTest(false);
			TestUtil.AddColumnsToDummy();
			TestUtil.AddDummyBizoToGlobalDefinitions();
			EntitySetDefinitionCache.SetAlternateDefinitionLoaderForTesting(TestUtil.GetDefinitionLoaderForTest());
			var z0_Pk = TestUtil.PrepareDummyTableData();
			var factory = new BusinessObjectFactory();
			var input = factory.Load<DummyBusinessObject>(z0_Pk);
			var result = exporter.Export(new[] { input });

			using (var reader = new StreamReader(result))
			{
				var content = reader.ReadToEnd();
				AssertIsXml(content).HavingExactlyOneChildNode("Header/DataContext/Timestamp", n => n.WithValue(XMLDataProviderConstant.DefaultTimestamp.ToString()));
			}
		}

		//TODO: Will be uncommented under WI00034844 and made to pass.
		//public void TestSerializeToStream_2012_11()
		//{
		//  using (SchemaVersionManager.SetNamespaceForTesting(NativeXmlInfo.Namespace_2012_11))
		//  {
		//    var OH_Pk = TestUtil.PrepareOrgHeaderTableData();
		//    var OC_Pk = TestUtil.PrepareOrgContactTableData(OH_Pk);
		//    var factory = new BusinessObjectFactory();

		//    var input = factory.Load<OrgHeader>(OH_Pk);
		//    var result = exporter.SerializeToStream(new[] { input, input });
		//    result.Position = 0;
		//    var reader = new StreamReader(result);
		//    var element = XElement.Load(reader);

		//    AssertEquals("Native", element.Name.LocalName);
		//    AssertEquals(NativeXmlInfo.Namespace_2012_11, element.Name.Namespace.ToString());

		//    AssertEquals(1, element.Descendants(ns + "Body").Count());
		//    AssertEquals(2, element.Descendants(ns + "Organization").Count());

		//    var pkElements = element.Descendants(ns + "PK");

		//    AssertEquals("PKs are in Data Elements even if there is a natural key", 4, pkElements.Count());
		//    AssertEquals("PKs are in Data Elements even if there is a natural key", 2, pkElements.Count(item => item.Value == OH_Pk.ToString()));
		//    AssertEquals("PKs are in Data Elements even if there is a natural key", 2, pkElements.Count(item => item.Value == OC_Pk.ToString()));
		//  }
		//}
		//TODO: Will be uncommented under WI00034844 and made to pass.

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			exporter = new NativeXmlSerializer
			{
				Converter = new BusinessObjectToEntityConverter
				{
					DefinitionFinder = TestUtil.GetEntitySetDefinitionFinder()
				}
			};
			ns = NativeXmlInfo.Namespace_2011_11;
		}

		NativeXmlSerializer exporter;
		XNamespace ns;

		#endregion
	}
}
