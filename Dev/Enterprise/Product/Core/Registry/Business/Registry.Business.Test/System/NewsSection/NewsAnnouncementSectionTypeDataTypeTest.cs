using System.Text;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(NewsAnnouncementSectionTypeRegistryDataType))]
	sealed class NewsAnnouncementSectionTypeDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<NewsAnnouncementSectionTypeRegistryDataType>
	{
		protected override string ExpectedEditorName
		{
			get { return "NewsAnnouncementSectionTypeRegistryItemEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection = new NewsAnnouncementSectionTypeCollection();
			var item = collection.AddNew();
			item.Code = "TE2";
			item.Description = (NoResString)"Description2";
			item.OrderItemsBy = NewsSectionSortTypeList.Codes.PublishedTime;

			var emptyBinaryValue = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfNewsAnnouncementSectionType xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" />");
			var byteArrayValue = Encoding.Unicode.GetBytes("<?xml version=\"1.0\" encoding=\"utf-16\"?><ArrayOfNewsAnnouncementSectionType xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\"><NewsAnnouncementSectionType><CodeMaxLength>3</CodeMaxLength><Code>TE2</Code><Description>Description2</Description><OrderItemsBy>PBT</OrderItemsBy><SystemDefined>false</SystemDefined></NewsAnnouncementSectionType></ArrayOfNewsAnnouncementSectionType>");

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(new NewsAnnouncementSectionTypeCollection(), emptyBinaryValue),
				new ValidSampleAndBinaryValueInDB(collection,  byteArrayValue),
			};
		}

		protected override NewsAnnouncementSectionTypeRegistryDataType GetNewDataType()
		{
			return new NewsAnnouncementSectionTypeRegistryDataType();
		}
	}
}
