using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.DataAdapters.Testing
{
	sealed class CusBondDetailValueObjectDataAdapterTest : TestCaseWithFactory
	{
		public void TestExportBondDetails()
		{
			CusBondDetail bond = GetBondDetails();
			Xsd.BondDetail xmlBond = new Xsd.BondDetail();
			MakeExport(bond, xmlBond);

			AssertEquals(Xsd.BondDetailActivityCode.Item1, xmlBond.ActivityCode);
			AssertEquals(12m, xmlBond.Amount);
			AssertEquals(new ZDateTime(2007, 1, 1), xmlBond.Effective);
			AssertEquals(new ZDateTime(2007, 1, 1), xmlBond.Expiry);
			AssertEquals("7777", xmlBond.FiledPort);
			AssertEquals("13", xmlBond.Number);
			AssertEquals("891", xmlBond.SuretyCode);
			AssertEquals("9", xmlBond.Type);
		}

		public void TestImportBondDetails()
		{
			CusBondDetail bond = GetBondDetails();
			Xsd.BondDetail xmlBond = new Xsd.BondDetail();
			MakeExport(bond, xmlBond);

			CusBondDetail bond1 = Factory.New<CusBondDetail>();
			DataAdapter.ImportFromValueObject(bond1, xmlBond, Context);

			AssertEquals("1", bond1.PW_ActivityCode);
			AssertEquals(12m, bond1.PW_BondAmount);
			AssertEquals(new ZDateTime(2007, 1, 1), bond1.PW_BondEffectiveDate);
			AssertEquals(new ZDateTime(2007, 1, 1), bond1.PW_BondExpiryDate);
			AssertEquals("7777", bond1.PW_BondFiledPort);
			AssertEquals("13", bond1.PW_BondNumber);
			AssertEquals("891", bond1.PW_SuretyCode);
			AssertEquals("9", bond1.PW_BondType);
		}

		CusBondDetail GetBondDetails()
		{
			CusBondDetail bondData = Factory.New<CusBondDetail>();
			bondData.PW_ActivityCode = "_1";
			bondData.PW_BondEffectiveDate = new ZDateTime(2007, 1, 1);
			bondData.PW_BondType = "9";
			bondData.PW_BondAmount = 12m;
			bondData.PW_BondExpiryDate = new ZDateTime(2007, 1, 1);
			bondData.PW_BondFiledPort = "7777";
			bondData.PW_BondNumber = "13";
			bondData.PW_SuretyCode = "891";
			return bondData;
		}

		void MakeExport(CusBondDetail bond, Xsd.BondDetail xmlBond)
		{
			DataAdapter.ExportToValueObject(bond, xmlBond, USExportContext);
		}

		CusBondDetailValueObjectDataAdapter DataAdapter
		{
			get { return dataAdapter ?? (dataAdapter = new CusBondDetailValueObjectDataAdapter()); }
		}
		CusBondDetailValueObjectDataAdapter dataAdapter;

		ValueObjectImportContext Context
		{
			get { return context ?? (context = new ValueObjectImportContext(Factory, Notification)); }
		}
		ValueObjectImportContext context;

		public ValueObjectExportContext USExportContext
		{
			get { return usExportContext ?? (usExportContext = new ValueObjectExportContext(Notification)); }
		}
		ValueObjectExportContext usExportContext;

		NotificationBuffer Notification
		{
			get { return notification ?? (notification = new NotificationBuffer()); }
		}
		NotificationBuffer notification;
	}
}
