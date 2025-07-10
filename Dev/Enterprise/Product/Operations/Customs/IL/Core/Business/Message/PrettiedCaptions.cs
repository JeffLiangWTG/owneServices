
namespace Enterprise.Customs.IL.Business.Message
{
	public static class PrettiedCaptions
	{
		public static class Section
		{
			public static string ResponseSection => Res.GetString("7F2B389F-60BF-4128-881E-B5A177A9CBB4", "Response");
			public static string RequestSection => Res.GetString("5CADE2D9-0E0A-45AC-BDDB-14606A0EF0E6", "Request");
		}

		public static class Exceptions
		{
			public static string ExceptionsSection => Res.GetString("3FB698FF-CB4E-4F1F-985E-57BE2266D219", "Exceptions");
			public static string ExceptionLevel => Res.GetString("45316F9C-D23D-4AD2-BFFA-A3154879EFC8", "Exception Level");
			public static string ExceptionLevelError => Res.GetString("45495E61-99ED-4F36-829D-97706E57A408", "Error");
			public static string ExceptionLevelWarning => Res.GetString("E048CDC9-5DC9-4693-B9E1-830811BC495A", "Warning");
			public static string ExceptionLevelInfo => Res.GetString("C396FBC4-C683-45C1-A519-33F30F2227F3", "Info");
			public static string ExceptionType => Res.GetString("68F2D54C-59CA-426E-B175-C671DD743049", "Exception Type");
			public static string ExceptionDescription => Res.GetString("C06EA9A0-F741-4149-BFE9-7098F7E118B3", "Exception Description");
			public static string ExceptionParams => Res.GetString("24B4E3BD-0855-476A-B58B-68CD48DF60D8", "Exception Parameters");
		}

		public static class Pointers
		{
			public static string PointersSection => Res.GetString("095642CA-9764-4FE4-B829-9D05BFEB60BE", "Pointers");
			public static string SequenceNumeric => Res.GetString("525C800D-CC28-44FD-AEB2-DCE055B7DFF1", "Sequence Numeric");
			public static string DocumentSectionCode => Res.GetString("18C259BA-E5F0-4540-827A-A0ED2ED7628F", "Document Section Code");
			public static string TagID => Res.GetString("83495913-666A-4879-8993-6563F266D061", "Tag ID");
			public static string NaturalKey => Res.GetString("2ACAA3E9-F8A3-4956-8976-A429C061AA9D", "Natural Key");
		}

		public static class Errors
		{
			public static string ErrorsSection => Res.GetString("FD919A21-13AA-40EB-8394-E85963B5C0D7", "Errors");
			public static string ValidationCode => Res.GetString("89F7DE7D-ED31-48D6-9B60-0357CDDBF452", "Validation Code");
			public static string Description => Res.GetString("5580CC67-4196-401F-8A81-57F2D779FA43", "Description");
			public static string NaturalKey => Res.GetString("E286871D-643C-4F34-9E44-3543A69CAEA2", "Natural Key");
		}

		public static class Constraints
		{
			public static string ConstraintsSection => Res.GetString("1ECB9261-7D64-4932-BDD7-F392134F8726", "Constraints");
			public static string ValidationCode => Res.GetString("1B950AB8-6468-4F36-BE8D-7D08047E390A", "Validation Code");
			public static string Description => Res.GetString("DF8344AE-A712-473A-A6E8-EB3E71BDFEF4", "Description");
			public static string ConstraintId => Res.GetString("3A1C9343-9E2B-488B-B0A4-6E7D76D18728", "Constraint ID");
			public static string ConstraintType => Res.GetString("343BC74B-44F1-4156-BF47-3F0CE13DC7CE", "Constraint Type");
			public static string ConstraintStatus => Res.GetString("103B76DF-A314-4023-926E-AFC7135C2F5D", "Constraint Status");
		}

		public static class Warnings
		{
			public static string WarningsSection => Res.GetString("71E6D75E-9A7D-48B3-8C23-C5C34A209EE5", "Warnings");
			public static string ValidationCode => Res.GetString("AADFD86D-141A-41B5-BA59-03E28E116029", "Validation Code");
			public static string Description => Res.GetString("E47973F7-AF48-4A5B-B10E-EA4B48446BDB", "Description");
		}

		public static class Common
		{
			public static string Date => Res.GetString("70A51701-F484-469A-AD92-B0349F1016DB", "Date");
			public static string Status => Res.GetString("84987103-BF82-4AC5-A308-6B7EFDCE75E1", "Status");
			public static string Success => Res.GetString("9999764F-59E8-48C4-A0F3-116F01B72B53", "Success");
			public static string Failed => Res.GetString("C66F1DF8-982C-4EBC-AA4D-34CF7427C18C", "Failed");
		}

		public static class Manifest
		{
			public static string IssuingDate => Res.GetString("4B3DF2DC-F91C-4F9F-91B0-F2B42FCCD3ED", "Issuing Date");
			public static string FunctionalReference => Res.GetString("4C1B2F78-BE7B-41FA-BBC1-FBCF5B33BFA6", "Functional Reference");
			public static string StatusSection => Res.GetString("D78D3691-E573-40CD-BE0E-32A3076E5B2C", "Statuses");
			public static string StatusDate => Res.GetString("38B34FB1-BF69-444A-AF51-AE1A63FCCFDE", "Status Date");
			public static string StatusText => Res.GetString("367C409F-EBB0-4DC9-90E8-72C457C78CC9", "Status Text");
			public static string StatusPointersSection => Res.GetString("7F4B640B-ECE2-49B4-B8F6-42A671820622", "Status Pointers");
			public static string StatusPointersSequenceNumeric => Res.GetString("165DB0B5-7DB7-41FC-AE84-BBF42E6EFB16", "Sequence Numeric");
			public static string StatusPointersDocumentSectionCode => Res.GetString("61140C29-44C7-4CDC-922A-9666256CD489", "Document Section Code");
			public static string StatusPointersTagID => Res.GetString("EB0EB134-F852-44A8-AAB9-6CC6513E1DDD", "Tag ID");
			public static string ValidationCode => Res.GetString("C5495FA9-8579-49EB-BC65-A8A162D0EB9E", "Validation Code");
			public static string ErrorPointersSection => Res.GetString("D4A01A82-5CB1-4E30-B11B-ABAD29D5E1B7", "Error Pointers");
			public static string ValidationName => Res.GetString("46BAD9C8-E4CB-4B4D-AD2A-2D60384FA585", "Validation Name");
			public static string ValidationListName => Res.GetString("F4DEA6C5-63DD-4E47-BB10-35C4235C924B", "Validation List Name");
			public static string ValidationListId => Res.GetString("D3301CAD-A678-4E97-B994-67B8CB2DDA0B", "List ID");
			public static string ValidationListURI => Res.GetString("36E25CD3-7490-4511-A938-285650E3EC1E", "List URI");
			public static string ValidationListAgencyId => Res.GetString("2F334D23-88F3-448E-8DB2-30E4F742CE70", "List Agency ID");
			public static string ValidationListAgencyName => Res.GetString("B7A67A8C-2950-477A-9CA6-FBB5227829B8", "List Agency Name");
			public static string ValidationListVersionId => Res.GetString("6FB48B04-67EC-454F-AEAA-1831DACFA7A1", "List Version ID");
			public static string ValidationLanguageId => Res.GetString("6FD4D17C-E8DB-4A03-A53A-937A31230E59", "List Language ID");
			public static string ValidationListSchemeUri => Res.GetString("C8E220A1-76C3-42FB-9E88-1BD6F078C21B", "List Scheme URI");
			public static string ManifestNumber => Res.GetString("DBFAE408-9E2D-491B-95C3-D9E851F61172", "Manifest Number");
			public static string ParentDealNumber => Res.GetString("A6F36AE5-0201-4AF3-860B-B4F051189396", "Parent Deal #");
		}

		public static class GatePassMovement
		{
			public static string GatePassNumber => Res.GetString("40C40A90-4B0B-4BCA-A6CE-F488583B4EFC", "Gatepass Number");
			public static string ReturnedCode => Res.GetString("A19EF039-B462-48AE-AC20-D7B33183F354", "Returned Code");
			public static string CargoIdentifierType => Res.GetString("894AF9E9-9307-47E1-9557-D2086FBBF576", "Cargo Identifier Type");
			public static string IdentifierKey1 => Res.GetString("218BE9F3-2DE0-4729-B209-C5085A8E544C", "Identifier Key 1");
			public static string IdentifierKey2 => Res.GetString("D2C92625-45AA-4B49-B8F8-6D728ABC7784", "Identifier Key 2");
			public static string IdentifierKey3 => Res.GetString("5BAE64AD-EEFC-40AB-ABD1-3C4D8F1FFEB1", "Identifier Key 3");
			public static string FeedbackIndicationsSection => Res.GetString("F5EEBE25-2B35-40CD-A3D8-9A4EC019681D", "Feedback Indications");
			public static string IndicationType => Res.GetString("59D78333-F70A-4309-8FB5-3FE1FD65C1C3", "Indication Type");
			public static string IndicationUnit => Res.GetString("DA631AC0-0E9D-4314-B73E-8C5B9B1CA81C", "Indication Unit");
			public static string RequestDate => Res.GetString("045E6023-3D77-47C2-B898-4DFA8B544761", "Request Date");
			public static string UpdateCode => Res.GetString("8743E1FE-47DE-4E37-B1F1-251A79070694", "Update Code");
			public static string ActionNew => Res.GetString("B4D34072-9C8D-4D69-B3A3-C3596A23AC47", "1 - New");
			public static string ActionCancel => Res.GetString("53D61EEC-FF2D-492F-8FB7-C2B980A5247D", "2 - Cancel");
			public static string OriginSiteCode => Res.GetString("B12B5408-E513-44A9-AF58-061E1AB77AF1", "Origin Site Code");
			public static string DesignatedSiteCode => Res.GetString("204D4336-6289-4666-BD0D-42828D5F4C92", "Designated Site Code");
			public static string TransportationTypeCode => Res.GetString("8D7BC0FD-A09B-4921-BFD9-C7E79151A598", "Transportation Type Code");
		}

		public static class DeliveryOrder
		{
			public static string DeliveryOrderNumber => Res.GetString("C65BB6E3-B994-4AEC-AA95-6B95AE4F2A3A", "Delivery Order Number");
			public static string RequestDate => Res.GetString("10029D63-F320-4035-BFEC-8357B8A06380", "Request Date");
			public static string ActionCode => Res.GetString("76E201B9-A390-423E-817A-CF8E5D16D79B", "Action Code");

			public static string ActionNew => Res.GetString("B6BB28E0-4AEA-46D4-A4DB-E088788EEC1F", "New");
			public static string ActionCancel => Res.GetString("93B4BE31-E0D9-4BF1-A50F-3CDB9095AB6C", "Cancel");
			public static string ReceiverVAT => Res.GetString("AB4B22A4-C5EB-401A-B58D-94C0BCDF0F12", "Receiver VAT");
			public static string ReceiverName => Res.GetString("04200934-E183-4447-8B0E-069200427F7C", "Receiver Name");
		}

		public static class Attachment
		{
			public static string FileName => Res.GetString("9A564E3D-EA5B-46F1-B95A-9592CF9FD7DA", "File Name");
			public static string DocumentType => Res.GetString("2B4C3569-9605-4136-91FF-4B6BF8D56BB6", "Document Type");
			public static string ExternalAttachmentID => Res.GetString("7184DB06-CFD1-4BD5-9A17-898E8413912A", "External Attachment ID");
			public static string FieldID => Res.GetString("10C5C51A-6FE4-4825-9E46-7C9096A1C75E", "Meta Data Code");
			public static string FieldData => Res.GetString("8528017D-026C-4D77-B34E-A307136D1036", "Meta Data Value");
			public static string DocNumber => Res.GetString("4CD1123D-8374-44D4-8FAA-CF597A6B2F31", "CW Document Number");
			public static string CustomsDocID => Res.GetString("F5A0940D-7E14-48A3-BF0F-9A9E43F4C306", "Customs Document ID");
			public static string EnglishDescription => Res.GetString("37967B6D-81E4-4EE6-85CC-ADE9DC579062", "English Description");
		}

		public static class Declaration
		{
			public static string StatusDate => Res.GetString("F2454C00-63B2-4C6D-9E5D-2F17AFCB0CE2", "Status Date");
			public static string AcceptanceDate => Res.GetString("D21A54E2-5AFC-408D-AA2E-627818EED9F1", "Acceptance Date Time");
			public static string DeclarationNumber => Res.GetString("9D409855-C7E2-4498-B118-2CD6F6664462", "Declaration Number");
			public static string Version => Res.GetString("BE9B7E3A-C9AD-4B17-9D0E-FB91BE732E5B", "Version");
			public static string ExternalId => Res.GetString("BE84FB87-8273-4556-B805-FBFB2C14CD03", "External Id");
		}

		public static class ManifestQuery
		{
			public static string CargoIdentifierType => Res.GetString("55D463C0-CF96-4D18-8B2E-BB37E485F641", "Cargo Identifier Type");
			public static string CargoIdentifierKey1 => Res.GetString("9800AB5D-8B92-4B12-A366-D02BCE025441", "Cargo Identifier Key 1");
			public static string CargoIdentifierKey2 => Res.GetString("7044285D-3F42-4249-989F-A03288E60200", "Cargo Identifier Key 2");
		}
	}
}
