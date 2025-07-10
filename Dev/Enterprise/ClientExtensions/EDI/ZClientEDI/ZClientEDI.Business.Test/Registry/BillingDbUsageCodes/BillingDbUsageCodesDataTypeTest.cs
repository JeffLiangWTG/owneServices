using System.Text;
using Enterprise.Client.EDI.Billing.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(BillingDbUsageCodesDataType))]
	class BillingDbUsageCodesDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<BillingDbUsageCodesDataType>
	{
		#region Implementation

		protected override BillingDbUsageCodesDataType GetNewDataType()
		{
			return new BillingDbUsageCodesDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "BillingDbUsageCodesRegistryEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new BillingDbUsageCodesCollection();
			var item1 = collection1.AddNew();
			item1.Category = "ACC";
			item1.PriceItemCode = "IT1";
			item1.PriceHeaderCode = BillingConstants.PriceHeaderType.ABMCustoms;

			var item2 = collection1.AddNew();
			item2.Category = "CSU";
			item2.PriceItemCode = "CSU";
			item2.PriceHeaderCode = BillingConstants.PriceHeaderType.STL;
			item2.KeyRefIndex1 = 1;

			var collection2 = new BillingDbUsageCodesCollection();
			var item2a = collection2.AddNew();
			item2a.Category = "ACC";
			item2a.PriceItemCode = "GTS";
			item2a.PriceHeaderCode = BillingConstants.PriceHeaderType.STL;
			item2a.KeyRefIndex1 = 3;

			#region ByteArrayValue

			string xml1 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfBillingDbUsageCodes xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">"
+ @"<BillingDbUsageCodes><Category>ACC</Category><PriceItemCode>IT1</PriceItemCode><PriceHeaderCode>ABM</PriceHeaderCode><KeyRefIndex1>0</KeyRefIndex1></BillingDbUsageCodes>"
+ @"<BillingDbUsageCodes><Category>CSU</Category><PriceItemCode>CSU</PriceItemCode><PriceHeaderCode>STL</PriceHeaderCode><KeyRefIndex1>1</KeyRefIndex1></BillingDbUsageCodes>"
+ @"</ArrayOfBillingDbUsageCodes>";

			string xml2 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfBillingDbUsageCodes xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">"
+ @"<BillingDbUsageCodes><Category>ACC</Category><PriceItemCode>GTS</PriceItemCode><PriceHeaderCode>STL</PriceHeaderCode><KeyRefIndex1>3</KeyRefIndex1></BillingDbUsageCodes>"
+ "</ArrayOfBillingDbUsageCodes>";

			byte[] byteArrayValue1 = Encoding.Unicode.GetBytes(xml1);
			byte[] byteArrayValue2 = Encoding.Unicode.GetBytes(xml2);

			#endregion

			return new ValidSampleAndBinaryValueInDB[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, byteArrayValue1),
				new ValidSampleAndBinaryValueInDB(collection2, byteArrayValue2)
			};
		}

		#endregion
	}
}
