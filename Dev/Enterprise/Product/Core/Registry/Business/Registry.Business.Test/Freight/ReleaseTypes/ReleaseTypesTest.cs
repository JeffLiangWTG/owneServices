using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(ReleaseTypes))]
	sealed class ReleaseTypesTest : RegistryBusinessObjectTemplateTestCase<ReleaseTypes>
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

		#region Number propagation

		public void TestNumberPropagation()
		{
			ReleaseTypes types = new ReleaseTypes();
			types.OriginalsNumber = 4;
			types.CopiesNumber = 5;
			ReleaseType rel = types.Types.AddNew();
			AssertEquals(4, rel.OriginalsNumber);
			AssertEquals(5, rel.CopiesNumber);
		}

		#endregion

		#region Implementation

		protected override ReleaseTypes GetBusinessObjectToClone()
		{
			ReleaseTypes result = new ReleaseTypes();

			result.OriginalsNumber = 3;
			result.CopiesNumber = 4;
			ReleaseType rel = result.Types.AddNew();
			rel.Code = "OBR";
			rel.Description = (NoResString)"Original Bill Required at Destination";
			rel.CodeMaxLength = 3;
			rel.OriginalsNumber = 3;
			rel.CopiesNumber = 4;
			rel.SystemDefined = true;

			rel = result.Types.AddNew();
			rel.Code = "AAA";
			rel.Description = (NoResString)"blah blah";
			rel.CodeMaxLength = 3;
			rel.OriginalsNumber = 1;
			rel.CopiesNumber = 5;
			rel.SystemDefined = false;

			return result;
		}

		protected override void CheckAllPropertiesAreEqual(ReleaseTypes originalBusinessObject, ReleaseTypes newBusinessObject, bool isClone)
		{
			base.CheckAllPropertiesAreEqual(originalBusinessObject, newBusinessObject, isClone);

			AssertEquals("Types.Count", originalBusinessObject.Types.Count, newBusinessObject.Types.Count);
			for (int i = 0; i < originalBusinessObject.Types.Count; i++)
			{
				AssertEquals("Types[" + i + "].Code", originalBusinessObject.Types[i].Code, newBusinessObject.Types[i].Code);
				AssertEquals("Types[" + i + "].Description", originalBusinessObject.Types[i].Description, newBusinessObject.Types[i].Description);
				AssertEquals("Types[" + i + "].OriginalsNumber", originalBusinessObject.Types[i].OriginalsNumber, newBusinessObject.Types[i].OriginalsNumber);
				AssertEquals("Types[" + i + "].CopiesNumber", originalBusinessObject.Types[i].CopiesNumber, newBusinessObject.Types[i].CopiesNumber);
			}
		}

		protected override ReleaseTypes GetBusinessObjectToSerialise()
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

		#endregion
	}
}
