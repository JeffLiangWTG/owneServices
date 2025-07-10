using CargoWise.Types;
using Enterprise.Customs.Business.AccumulativeAmendment;

namespace Enterprise.Customs.KR.Messaging.Testing
{
	sealed class LocalExportAmendmentTypeCodeListPartial : NUnit.Framework.TestCase
	{
		public void TestDataItemIDDescription()
		{
			AssertEquals(nameof(EntityAmendType.Delete), LocalExportAmendmentTypeCodeList.GetLocalExportAmendTypeForEntry(LocalExportAmendmentTypeCodeList.Codes.Delete));

			AssertEquals(nameof(EntityAmendType.Update), LocalExportAmendmentTypeCodeList.GetLocalExportAmendTypeForEntry(LocalExportAmendmentTypeCodeList.Codes.Update));

			AssertEquals(nameof(EntityAmendType.Add), LocalExportAmendmentTypeCodeList.GetLocalExportAmendTypeForEntry(ZString.Empty));
		}
	}
}
