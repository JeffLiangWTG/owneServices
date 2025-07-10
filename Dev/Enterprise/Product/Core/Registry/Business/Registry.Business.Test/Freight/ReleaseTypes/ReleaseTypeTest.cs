using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ReleaseType))]
	sealed class ReleaseTypeTest : RegistryBusinessObjectTestCaseBase
	{
		#region Validation

		public void TestValidateOriginalsNumber()
		{
			AssertNoErrors("Precondition: OriginalsNumber should not have errors.", BizObj.OriginalsNumberInfo);

			BizObj.OriginalsNumber = 10;
			AssertNoErrors(BizObj.OriginalsNumberInfo);

			BizObj.OriginalsNumber = -1;
			AssertHasError(BizObj.OriginalsNumberInfo, "Please enter an 'Originals Number' within the range 0 to 255.");

			BizObj.OriginalsNumber = 7;
			AssertNoErrors(BizObj.OriginalsNumberInfo);

			BizObj.OriginalsNumber = 444;
			AssertHasErrors(BizObj.OriginalsNumberInfo);

			BizObj.OriginalsNumber = 5;
			AssertNoErrors(BizObj.OriginalsNumberInfo);
		}

		public void TestValidateCopiesNumber()
		{
			AssertNoErrors("Precondition: CopiesNumber should not have errors.", BizObj.CopiesNumberInfo);

			BizObj.CopiesNumber = 100;
			AssertNoErrors(BizObj.CopiesNumberInfo);

			BizObj.CopiesNumber = -1;
			AssertHasError(BizObj.CopiesNumberInfo, "Please enter a 'Copies Number' within the range 0 to 255.");

			BizObj.CopiesNumber = 10;
			AssertNoErrors(BizObj.CopiesNumberInfo);

			BizObj.CopiesNumber = 333;
			AssertHasError(BizObj.CopiesNumberInfo, "Please enter a 'Copies Number' within the range 0 to 255.");

			BizObj.CopiesNumber = 5;
			AssertNoErrors(BizObj.CopiesNumberInfo);
		}

		#endregion

		public void TestCanDelete()
		{
			ReleaseType result = (ReleaseType)GetNewBusinessObject();
			ICanDelete canDelete = result;
			AssertEquals("ReasonForNotAbleToDelete", "This is a system defined value and cannot be deleted.", canDelete.ReasonForNotAbleToDelete);
			result.SystemDefined = false;
			AssertEquals("CanDelete", true, canDelete.CanDelete);
			result.SystemDefined = true;
			AssertEquals("CanDelete", false, canDelete.CanDelete);
		}

		public void TestReadOnlyStates()
		{
			ReleaseType result = (ReleaseType)GetNewBusinessObject();

			result.SystemDefined = false;
			AssertEquals("CodeInfo.ReadOnly", false, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", false, result.DescriptionInfo.ReadOnly);
			AssertEquals("EnglishDescriptionInfo.ReadOnly", false, result.EnglishDescriptionInfo.ReadOnly);

			result.SystemDefined = true;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("EnglishDescriptionInfo.ReadOnly", true, result.EnglishDescriptionInfo.ReadOnly);

			result.ReadOnly = true;
			result.ReadOnly = false;
			AssertEquals("CodeInfo.ReadOnly", true, result.CodeInfo.ReadOnly);
			AssertEquals("DescriptionInfo.ReadOnly", true, result.DescriptionInfo.ReadOnly);
			AssertEquals("EnglishDescriptionInfo.ReadOnly", true, result.EnglishDescriptionInfo.ReadOnly);
		}

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			ReleaseType result = new ReleaseType();

			result.Code = "OBR";
			result.Description = (NoResString)"Original Bill Required at Destination";
			result.CodeMaxLength = 3;
			result.OriginalsNumber = 3;
			result.CopiesNumber = 4;
			result.SystemDefined = true;

			return result;
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		new ReleaseType BizObj
		{
			get { return (ReleaseType)base.BizObj; }
		}

		#endregion
	}
}
