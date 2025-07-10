using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AQISDocumentLookupsTest : TestCaseWithFactory
	{
		public void TestDocumentTypeListSortOrder()
		{
			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var documentType1 = CMRAqisDocumentType.New(Factory);
				documentType1.QD_AQISDocumentType = "CODE1";
				documentType1.QD_AQISDocumentDescription = "Description";

				var documentType2 = CMRAqisDocumentType.New(Factory);
				documentType2.QD_AQISDocumentType = "CODE2";
				documentType2.QD_AQISDocumentDescription = "A Description";
				Factory.Save();

				var document = new AQISDocument(Factory);
				AssertEquals("Elements are sorted by description", "CODE2, CODE1", document.Lookups.AQISDocumentTypeList.CodesAsString);
			}

			using (AUCustomsDataRegistry.Instance.UseRefDatabaseData.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var newFactory = new BusinessObjectFactory();
				var universalHelper = new UniversalReferenceTestDataHelper(newFactory);
				universalHelper.CreateNewOrGetExistingCusCodeType(AUConstants.RefCusCodeTypeCodes.CMRDT, "AQIS Document Code Type", Core.Constants.CountryCodes.Australia);
				universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRDT, "CODE1", "B Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRDT, "CODE2", "A Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				universalHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.Australia, AUConstants.RefCusCodeTypeCodes.CMRDT, "CODE3", "C Description", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
				newFactory.Save();

				var document = new AQISDocument(newFactory);
				AssertEquals("Elements are sorted by description", "CODE2, CODE1, CODE3", document.Lookups.AQISDocumentTypeList.CodesAsString);
			}
		}

		protected override void SetUp()
		{
			TestCaseHelper.ClearTable(CMRAqisDocumentType.Schema.TableName);
			base.SetUp();
		}
	}
}
