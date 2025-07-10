using System.Text;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(StlRawUsageReportRefCaptionDataType))]
	class StlRawUsageReportRefCaptionDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<StlRawUsageReportRefCaptionDataType>
	{
		#region Implementation

		protected override StlRawUsageReportRefCaptionDataType GetNewDataType()
		{
			return new StlRawUsageReportRefCaptionDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "StlRawUsageReportRefCaptionRegistryEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new StlRawUsageReportRefCaptionCollection();
			var item11 = collection1.AddNew();
			item11.UsageCode = "USR";
			item11.UsageDescription = "Staff";
			item11.Ref1Caption = "Staff Code";
			item11.Ref2Caption = "Staff Name";

			var item12 = collection1.AddNew();
			item12.UsageCode = "OPM";
			item12.UsageDescription = "Opportunity Manager";
			item12.Ref1Caption = "Opportunity ID";

			var collection2 = new StlRawUsageReportRefCaptionCollection();
			var item21 = collection2.AddNew();
			item21.UsageCode = "CMR";
			item21.UsageDescription = "Campaign";
			item21.Ref1Caption = "Campaign Name";

			#region ByteArrayValue

			string xml1 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfStlRawUsageReportRefCaption xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">"
+ @"<StlRawUsageReportRefCaption><Category>STL</Category><UsageCode>USR</UsageCode><UsageDescription>Staff</UsageDescription><Ref1Caption>Staff Code</Ref1Caption><Ref2Caption>Staff Name</Ref2Caption><Ref3Caption /><Ref4Caption /><Ref5Caption /></StlRawUsageReportRefCaption>"
+ @"<StlRawUsageReportRefCaption><Category>STL</Category><UsageCode>OPM</UsageCode><UsageDescription>Opportunity Manager</UsageDescription><Ref1Caption>Opportunity ID</Ref1Caption><Ref2Caption /><Ref3Caption /><Ref4Caption /><Ref5Caption /></StlRawUsageReportRefCaption>"
+ @"</ArrayOfStlRawUsageReportRefCaption>";

			string xml2 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfStlRawUsageReportRefCaption xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">"
+ @"<StlRawUsageReportRefCaption><Category>STL</Category><UsageCode>CMR</UsageCode><UsageDescription>Campaign</UsageDescription><Ref1Caption>Campaign Name</Ref1Caption><Ref2Caption /><Ref3Caption /><Ref4Caption /><Ref5Caption /></StlRawUsageReportRefCaption>"
+ "</ArrayOfStlRawUsageReportRefCaption>";

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
