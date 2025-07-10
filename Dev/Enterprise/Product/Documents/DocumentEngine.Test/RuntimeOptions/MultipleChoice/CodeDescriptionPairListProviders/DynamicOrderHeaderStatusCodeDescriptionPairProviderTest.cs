using System.Data;
using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	sealed class DynamicOrderHeaderStatusCodeDescriptionPairProviderTest : DynamicKeyedCodePairListProviderTest<ZGuid, DynamicOrderHeaderStatusCodeDescriptionPairProvider>
	{
		protected override void SetupDataForFirstKey(BusinessObjectFactory factory)
		{
			Client1.MiscServ.OrderStatusList = Client1ListBlob;
		}

		protected override void SetupDataForSecondKey(BusinessObjectFactory factory)
		{
			Client2.MiscServ.OrderStatusList = Client2ListBlob;
		}

		protected override ZGuid FirstKey
		{
			get { return Client1.PK; }
		}

		protected override ZGuid SecondKey
		{
			get { return Client2.PK; }
		}

		protected override DynamicOrderHeaderStatusCodeDescriptionPairProvider GetListProvider()
		{
			return new DynamicOrderHeaderStatusCodeDescriptionPairProvider(Factory);
		}

		protected override ReadOnlyCodeDescriptionPairList ExpectedValuesForFirstKey
		{
			get { return GetExpectedForClient(Client1List); }
		}

		protected override ReadOnlyCodeDescriptionPairList ExpectedValuesForSecondKey
		{
			get { return GetExpectedForClient(Client2List); }
		}

		CodeDescriptionPairList GetExpectedForClient(ReadOnlyCodeDescriptionPairList client)
		{
			CodeDescriptionPairList list = ObjectFactory.Get<Enterprise.Freight.Integration.Forwarding.IOrderStatusListProvider>().GetOrderLineStatusList();
			list.AddRangeOverwriteIfExists(client);
			return list;
		}

		ZBlob Client1ListBlob
		{
			get
			{
				if (client1ListBlob == null)
				{
					client1ListBlob = GetClientListBlob(Client1List[0].Code, Client1List[0].Description, Client1List[1].Code, Client1List[1].Description);
				}
				return client1ListBlob;
			}
		}
		ZBlob client1ListBlob;

		ZBlob Client2ListBlob
		{
			get
			{
				if (client2ListBlob == null)
				{
					client2ListBlob = GetClientListBlob(Client2List[0].Code, Client2List[0].Description, Client2List[1].Code, Client2List[1].Description);
				}
				return client2ListBlob;
			}
		}
		ZBlob client2ListBlob;

		ReadOnlyCodeDescriptionPairList Client2List
		{
			get { return client2List ?? (client2List = GetClient2List()); }
		}
		ReadOnlyCodeDescriptionPairList client2List;

		ReadOnlyCodeDescriptionPairList Client1List
		{
			get { return client1List ?? (client1List = GetClient1List()); }
		}
		ReadOnlyCodeDescriptionPairList client1List;

		ReadOnlyCodeDescriptionPairList GetClient1List()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("INC", "Client 1 Test1 (INC) -- should override any system or registry settings");
			list.AddPair("C12", "Client 1 Test2");
			return list;
		}

		ReadOnlyCodeDescriptionPairList GetClient2List()
		{
			CodeDescriptionPairList list = new CodeDescriptionPairList();
			list.AddPair("C21", "Client 2 Test1");
			list.AddPair("C22", "Client 2 Test2");
			return list;
		}

		ZBlob GetClientListBlob(string code1, string description1, string code2, string description2)
		{
			ZBlob result = new ZBlob();
			DataSet data = new DataSet();
			DataTable table = new DataTable("ListTable");
			DataColumn codeColumn = new DataColumn("Code", typeof(string));
			DataColumn descColumn = new DataColumn("Description", typeof(string));
			table.Columns.Add(codeColumn);
			table.Columns.Add(descColumn);

			DataRow row = table.NewRow();
			row[codeColumn] = code1;
			row[descColumn] = description1;
			table.Rows.Add(row);

			row = table.NewRow();
			row[codeColumn] = code2;
			row[descColumn] = description2;
			table.Rows.Add(row);

			data.Tables.Add(table);

			using (MemoryStream stream = new MemoryStream())
			{
				data.WriteXml(stream, XmlWriteMode.WriteSchema);
				result = stream.ToArray();
			}
			return result;
		}

		OrgHeader Client1
		{
			get
			{
				return client1 ?? (client1 = Factory.LoadTop1<OrgHeader>(new ZQuery()));
			}
		}
		OrgHeader client1;

		OrgHeader Client2
		{
			get
			{
				return client2 ?? (client2 = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.StartsWith, "B")));
			}
		}
		OrgHeader client2;
	}
}
