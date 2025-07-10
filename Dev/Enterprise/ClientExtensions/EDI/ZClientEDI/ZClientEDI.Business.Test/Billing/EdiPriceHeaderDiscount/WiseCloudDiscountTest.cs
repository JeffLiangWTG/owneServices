using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Xml;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	[TestedType(typeof(WiseCloudDiscount))]
	internal class WiseCloudDiscountTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidation()
		{
			var discount = new WiseCloudDiscount();
			discount.ExpiryMonthsFromAgreedGoLive = 0;
			discount.RunPreSaveValidation();
			AssertNoErrors(discount.ExpiryMonthsFromAgreedGoLiveInfo);
			discount.ExpiryMonthsFromAgreedGoLive = -1;
			discount.RunPreSaveValidation();
			AssertHasErrors(discount.ExpiryMonthsFromAgreedGoLiveInfo);
		}

		public void TestXmlSerialization()
		{
			var discount = new WiseCloudDiscount();
			discount.ExpiryMonthsFromAgreedGoLive = 10;

			var serializer = ZXmlSerializer.New(typeof(WiseCloudDiscount));
			string xml = "";
			using (var writer = new StringWriter())
			{
				serializer.Serialize(writer, discount);
				xml = writer.ToString();
			}

			var reader = new StringReader(xml);
			var discount2 = (WiseCloudDiscount)serializer.Deserialize(reader);
			AssertEquals(10, discount2.ExpiryMonthsFromAgreedGoLive);
		}
	}
}
