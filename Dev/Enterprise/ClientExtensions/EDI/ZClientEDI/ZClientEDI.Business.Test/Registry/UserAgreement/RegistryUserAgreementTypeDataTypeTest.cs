using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(RegistryUserAgreementTypeDataType))]
	class RegistryUserAgreementTypeDataTypeTest : NonPersistentBusinessObjectRegistryDataTypeTestCase<RegistryUserAgreementTypeDataType>
	{
		#region Implementation

		protected override RegistryUserAgreementTypeDataType GetNewDataType()
		{
			return new RegistryUserAgreementTypeDataType();
		}

		protected override string ExpectedEditorName
		{
			get { return "RegistryUserAgreementTypeRegistryEditor"; }
		}

		protected override ValidSampleAndBinaryValueInDB[] GetValidSamples()
		{
			var collection1 = new RegistryUserAgreementTypeCollection();
			var type1 = (RegistryUserAgreementType)collection1.AddNew();
			type1.Code = "CD1";
			type1.Level = "USR";
			type1.Description = (NoResString)"Description 1";

			var type2 = (RegistryUserAgreementType)collection1.AddNew();
			type2.Code = "CD2";
			type2.Level = "COP";
			type2.Description = (NoResString)"Description 2";

			var collection2 = new RegistryUserAgreementTypeCollection();
			var type3 = (RegistryUserAgreementType)collection2.AddNew();
			type3.Code = "TP3";
			type3.Level = "USR";
			type3.Description = (NoResString)"Howdiedoo";

			const string xmlString1 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfRegistryUserAgreementType xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><RegistryUserAgreementType><CodeMaxLength>3</CodeMaxLength><Code>CD1</Code><Description>Description 1</Description><Level>USR</Level></RegistryUserAgreementType><RegistryUserAgreementType><CodeMaxLength>3</CodeMaxLength><Code>CD2</Code><Description>Description 2</Description><Level>COP</Level></RegistryUserAgreementType></ArrayOfRegistryUserAgreementType>";
			const string xmlString2 = @"<?xml version=""1.0"" encoding=""utf-16""?><ArrayOfRegistryUserAgreementType xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><RegistryUserAgreementType><CodeMaxLength>3</CodeMaxLength><Code>TP3</Code><Description>Howdiedoo</Description><Level>USR</Level></RegistryUserAgreementType></ArrayOfRegistryUserAgreementType>";

			return new[]
			{
				new ValidSampleAndBinaryValueInDB(collection1, xmlString1),
				new ValidSampleAndBinaryValueInDB(collection2, xmlString2)
			};
		}

		#endregion
	}
}
