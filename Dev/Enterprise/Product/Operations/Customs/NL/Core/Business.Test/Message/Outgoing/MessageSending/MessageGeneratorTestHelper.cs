using System.IO;
using System.Reflection;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.NL.Business.Testing;

class MessageGeneratorTestHelper
{
	public static ZString GetExpectedMessageXML(ZString path)
	{
		using (var inStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path))
		{
			return new StreamReader(inStream).ReadToEnd();
		}
	}

	public static SupportingDocument CreateSupportingDocument(SupportingDocumentCollection supportingDocuments, ZShort itemNumber, ZString referenceNumber, ZString referenceNumber2, ZDateTime dateOfExpiry)
	{
		var supportingDoc = supportingDocuments.AddNew();
		supportingDoc.CSI_ItemNumber = itemNumber;
		supportingDoc.CSI_ReferenceNumber = referenceNumber;
		supportingDoc.CSI_AdditionalDescription = referenceNumber2;
		supportingDoc.CSI_DateOfExpiry = dateOfExpiry;
		return supportingDoc;
	}

	public static SupportingDocument CreateSupportingDocument(SupportingDocumentCollection supportingDocuments, ZShort itemNumber, ZString referenceNumber, ZString referenceNumber2, ZDateTime dateOfExpiry, ZString code)
	{
		var supportingDoc = CreateSupportingDocument(supportingDocuments, itemNumber, referenceNumber, referenceNumber2, dateOfExpiry);
		supportingDoc.CSI_Code = code;
		return supportingDoc;
	}
}
