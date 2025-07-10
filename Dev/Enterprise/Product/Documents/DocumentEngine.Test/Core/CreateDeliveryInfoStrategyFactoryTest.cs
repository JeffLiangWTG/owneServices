using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class CreateDeliveryInfoStrategyFactoryTest : TestCaseWithFactory
	{
		public void TestCreate()
		{
			AssertCreate(Enterprise.Core.Constants.ContactNotifyModes.Email, c => new Email(c));
			AssertCreate(Enterprise.Core.Constants.ContactNotifyModes.Ftp, c => new Ftp(c, ZGuid.Empty));
		}

		public void AssertCreate(string deliveryMethod, Func<DocDeliveryContact, DeliveryMethod> getDeliveryMethodFunc)
		{
			var strategyFactory = new CreateDeliveryInfoStrategyFactory();
			var deliveryContact = new DocDeliveryContact(Factory);
			deliveryContact.DeliveryMethod = deliveryMethod;

			deliveryContact.AttachmentType = AttachmentTypeList.Codes.Xls;
			AssertType(typeof(DefaultCreateDeliveryInfoStrategy), strategyFactory.Create(getDeliveryMethodFunc(deliveryContact)));

			deliveryContact.AttachmentType = AttachmentTypeList.Codes.Pdf;
			AssertType(typeof(DefaultCreateDeliveryInfoStrategy), strategyFactory.Create(getDeliveryMethodFunc(deliveryContact)));

			deliveryContact.AttachmentType = AttachmentTypeList.Codes.Csv;
			var strategy = strategyFactory.Create(getDeliveryMethodFunc(deliveryContact));
			AssertType(typeof(CsvCreateDeliveryInfoStrategy), strategy);
			AssertEquals(false, ((CsvCreateDeliveryInfoStrategy)strategy).IncludeColumnHeadings);

			deliveryContact.AttachmentType = AttachmentTypeList.Codes.CsvWithHeadings;
			strategy = strategyFactory.Create(getDeliveryMethodFunc(deliveryContact));
			AssertType(typeof(CsvCreateDeliveryInfoStrategy), strategy);
			AssertEquals(true, ((CsvCreateDeliveryInfoStrategy)strategy).IncludeColumnHeadings);

			deliveryContact.AttachmentType = AttachmentTypeList.Codes.Xml;
			AssertType(typeof(XmlCreateDeliveryInfoStrategy), strategyFactory.Create(getDeliveryMethodFunc(deliveryContact)));

			deliveryContact.AttachmentType = AttachmentTypeList.Codes.Txt_Pipe;
			AssertType(typeof(TxtCreateDeliveryInfoStrategy), strategyFactory.Create(getDeliveryMethodFunc(deliveryContact)));
			deliveryContact.AttachmentType = AttachmentTypeList.Codes.Txt_Comm;
			AssertType(typeof(TxtCreateDeliveryInfoStrategy), strategyFactory.Create(getDeliveryMethodFunc(deliveryContact)));
			deliveryContact.AttachmentType = AttachmentTypeList.Codes.Txt_Semi;
			AssertType(typeof(TxtCreateDeliveryInfoStrategy), strategyFactory.Create(getDeliveryMethodFunc(deliveryContact)));
		}
	}
}
