using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(ImportSADNumber))]
	sealed class ImportSADNumberTest : Customs.Business.Testing.CusSupportingInfoTest<ImportSADNumber>
	{
		public void TestIsLinkingToReadOnlyEMCSParent()
		{
			var messageForReadOnly = @"Should only be true with these two conditions:
a)The sad code links to a EMCS Declaration.
c)The message stauts of parent declaration is SNT or ACK.";

			var messageForCanDelete = @"Should only be false with these two conditions:
a)The sad code links to a EMCS Declaration.
c)The message stauts of parent declaration is SNT or ACK.";

			void AssertCanDeleteAndReadOnly(ImportSADNumber sadCode, bool expectedReadOnly, bool expectedCanDelete)
			{
				AssertEquals(messageForCanDelete, expectedCanDelete, sadCode.CanDelete);
				AssertEquals(messageForReadOnly, expectedReadOnly, sadCode.CSI_DescriptionInfo.ReadOnly);
			}

			AssertCanDeleteAndReadOnly(importSADNumber, false, true);

			var declaration = Factory.New<EMCSJobDeclaration>();
			var sadCodeWithEMCSParent = declaration.ImportSADNumbers.AddNew();

			declaration.JE_MessageStatus = ZString.Empty;
			Factory.InvalidateCachedProperties();
			AssertCanDeleteAndReadOnly(sadCodeWithEMCSParent, false, true);

			declaration.JE_MessageStatus = EDIMessage.Status.Sent;
			Factory.InvalidateCachedProperties();
			AssertCanDeleteAndReadOnly(sadCodeWithEMCSParent, true, false);

			declaration.JE_MessageStatus = EDIMessage.Status.Acknowledged;
			Factory.InvalidateCachedProperties();
			AssertCanDeleteAndReadOnly(sadCodeWithEMCSParent, true, false);
		}

		public void TestEntryNumberMaxLength()
		{
			AssertEquals(21, importSADNumber.CSI_DescriptionInfo.MaxLength);
		}

		public void TestSetDefaultValues()
		{
			CombineAssertions(() =>
			{
				AssertEquals("CSI_Type", CusSupportingInfoTypeList.Codes.ImportSad, importSADNumber.CSI_Type);
				AssertEquals("CSI_Code", CusSupportingInfoTypeList.Codes.ImportSad, importSADNumber.CSI_Code);
			});
		}

		protected override IEnumerable<ImportSADNumber> GetBizObjsForCorrectlyTypeDecideTest(BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<EMCSJobDeclaration>();
			yield return declaration.ImportSADNumbers.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.NewWithValidTestData<EMCSJobDeclaration>();
			return declaration.ImportSADNumbers.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			importSADNumber = Factory.New<ImportSADNumber>();
		}
		ImportSADNumber importSADNumber;
	}
}
