using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.Customs.DataTransfer.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.ZArchitecture;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AUShipmentDeclarationValueObjectDataAdapter))]
	public class AUShipmentDeclarationValueObjectDataAdapterTest : ShipmentDeclarationValueObjectDataAdapterTest
	{
		protected override Type GetDataAdapterType()
		{
			return typeof(AUShipmentDeclarationValueObjectDataAdapter);
		}

		public void TestGetJobDeclaration()
		{
			Xsd.XmlInterchange interchange = new Xsd.XmlInterchange();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, interchange, new NotificationBuffer());
			AUShipmentDeclarationValueObjectDataAdapterForTest adapter = new AUShipmentDeclarationValueObjectDataAdapterForTest();
			BaseJobDeclaration testDeclaration = adapter.GetJobDeclaration(context);
			AssertEquals("type of Declaration", typeof(JobDeclaration), testDeclaration.GetType());
		}

		protected override void AssertDelegation()
		{
			adapter = ShipmentDeclarationValueObjectDataAdapter.New();
			AssertEquals("Default Adapter Type", typeof(AUShipmentDeclarationValueObjectDataAdapter), adapter.GetType());
			TestDeclarationValueObjectDataAdapter.Register();
			adapter = ShipmentDeclarationValueObjectDataAdapter.New();
			AssertEquals("Adapter Overriden New Delegate Type", typeof(TestDeclarationValueObjectDataAdapter), adapter.GetType());
		}

		class AUShipmentDeclarationValueObjectDataAdapterForTest : AUShipmentDeclarationValueObjectDataAdapter
		{
			public new BaseJobDeclaration GetJobDeclaration(IValueObjectImportContext context)
			{
				return base.GetJobDeclaration(context);
			}
		}
	}
}
