using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Accounting.DataTransfer.Invoices.Testing
{
	public class InvoiceXmlValueObjectSerializerTestCase : TestCaseWithFactory
	{
		public void TestIsCrossLedger()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			header.OH_Code = "TEST01";
			NotificationBuffer notify = new NotificationBuffer();
			ValueObjectImportContext context = new ValueObjectImportContext(Factory, notify);
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = ObjectFactory.Get<IProductRegistration>().Key.EnterpriseCode;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = GlbCompany.CurrentCompany.GC_Code;

			Assert(!InvoiceXmlValueObjectSerializer.IsCrossLedger(context));

			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.EnterpriseCode = header.OH_Code;
			((Xsd.XmlInterchange)context.Interchange).InterchangeInfo.Source.CompanyCode = header.OH_Code;

			Assert(InvoiceXmlValueObjectSerializer.IsCrossLedger(context));
		}
	}
}
