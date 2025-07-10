using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.EU.Manifest.Business.Testing
{
	class ICSAsycudaPackValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIsWeightRequired()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont.PK;

			var consol = Factory.New<ForwardingConsol>();

			header.AMA_ParentTableCode = "JK";
			header.AMA_ParentId = consol.PK;
			header.SpecificCircumstanceIndicator = SpecificCircumstanceList.Codes.E;

			AssertNoErrors(pack.APA_WeightInfo);

			header.AMA_ParentId = ZGuid.NewZGuid();
			pack.Validation.ValidateAPA_Weight();
			AssertNoErrors(pack.APA_WeightInfo);

			header.SpecificCircumstanceIndicator = SpecificCircumstanceList.Codes.A;
			pack.Validation.ValidateAPA_Weight();
			AssertNoErrors(pack.APA_WeightInfo);

			header.AMA_ManifestType = EUManifestTypes.Codes.ICS;
			pack.Validation.ValidateAPA_Weight();
			AssertHasMessageErrorContaining(pack.APA_WeightInfo, "Weight (on Pack) cannot be zero.");

			pack.APA_Weight = 10m;
			pack.Validation.ValidateAPA_Weight();
			AssertNoErrors(pack.APA_WeightInfo);
		}

		public void TestCheckAPA_MarksAndNumbers()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var cont = header.Containers.AddNew();
			var pack = bill.Packs.AddNew();
			pack.ContainerPK = cont.PK;
			pack.APA_PackUQ = Core.Constants.PkgUnit.BulkBag;

			var consol = Factory.New<ForwardingConsol>();

			header.AMA_ParentTableCode = "JK";
			header.AMA_ParentId = consol.PK;
			header.SpecificCircumstanceIndicator = SpecificCircumstanceList.Codes.E;

			AssertNoErrors(pack.APA_MarksAndNumbersInfo);

			header.AMA_ParentId = ZGuid.NewZGuid();
			pack.Validation.ValidateAPA_MarksAndNumbers();
			AssertNoErrors(pack.APA_MarksAndNumbersInfo);

			header.SpecificCircumstanceIndicator = ZString.Empty;
			pack.Validation.ValidateAPA_MarksAndNumbers();
			AssertNoErrors(pack.APA_MarksAndNumbersInfo);

			header.AMA_ManifestType = EUManifestTypes.Codes.ICS;
			pack.Validation.ValidateAPA_MarksAndNumbers();
			AssertNoErrors(pack.APA_MarksAndNumbersInfo);

			pack.APA_PackUQ = Core.Constants.PkgUnit.BreakBulk;
			pack.Validation.ValidateAPA_MarksAndNumbers();
			AssertNoErrors(pack.APA_MarksAndNumbersInfo);

			pack.APA_PackUQ = Core.Constants.PkgUnit.Bag;
			pack.Validation.ValidateAPA_MarksAndNumbers();
			AssertHasMessageErrorContaining(pack.APA_MarksAndNumbersInfo, "You have not entered");
		}
	}
}
