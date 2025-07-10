using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

class AdditionalInfoTestHelper
{
	public static void SetUpRefCusCodesForAttributeName(BusinessObjectFactory factory)
	{
		var exportAddRef = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ExportAddDocAdditionalReference;
		var attributeName = UniversalReferenceConstants.RefCusCodeListAttributeName.ReferenceNumber;

		var helper = new UniversalReferenceTestDataHelper(factory);
		var dataGroupingIT = helper.CreateNewOrGetExistingDataGrouping("IT", "Italy");
		helper.CreateNewOrGetExistingCusCodeType(exportAddRef, "Export Code Type");
		helper.CreateNewOrGetExistingRefCusCodeListAttributeName(attributeName, attributeName, exportAddRef, dataGroupingIT.ZZZ_DataGrouping);

		var cusCodeYYY = helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, exportAddRef, "AB01C", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		cusCodeYYY.Attributes.AddNew(attributeName, "Y");

		var cusCodeNNN = helper.CreateCusCodeList(dataGroupingIT.ZZZ_DataGrouping, exportAddRef, "XY01Z", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		cusCodeNNN.Attributes.AddNew(attributeName, "N");

		factory.Save();
	}
}
