using CargoWise.ComponentModel;
using CargoWise.Types;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class SpatialTypeValidationTest : TestCaseWithDummy
	{
		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestAllowSpatialTypesAttributeIsSetToZ0_Geography()
		{
			var allowedTypes = (Dummy.Z0_GeographyInfo as ZPropertyInfoGeography).AllowedSpatialTypes;

			AssertNotNull("Z0_Geography is marked with AllowSpatialTypesAttribute.", allowedTypes);
			AssertEquals("Z0_Geography is allowed with 2 types.", 2, allowedTypes.Count);
			AssertEquals("Z0_Geography is allowed to be set with Point.", "Point", allowedTypes[0]);
			AssertEquals("Z0_Geography is allowed to be set with Polygon.", "Polygon", allowedTypes[1]);
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestErrorIfSpatialTypeIsNotAllowed()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Assert("Precondition: Dummy bizo has no error.", !Dummy.HasErrors);

				Dummy.Z0_Geography = ZGeography.Empty;
				SpatialTypeValidation.ErrorIfSpatialTypeIsNotAllowed(Dummy.Z0_GeographyInfo);
				Assert("Dummy bizo has no error after Z0_Geography is set with Empty.", !Dummy.Z0_GeographyInfo.HasErrors());

				Dummy.Z0_Geography = ZGeography.CreatePoint(1, 2);
				SpatialTypeValidation.ErrorIfSpatialTypeIsNotAllowed(Dummy.Z0_GeographyInfo);
				Assert("Dummy bizo has no error after Z0_Geography is set with point.", !Dummy.Z0_GeographyInfo.HasErrors());

				Dummy.Z0_Geography = ZGeography.CreatePolygon("0 0,1 0,1 1,0 1,0 0").MakeValid();
				SpatialTypeValidation.ErrorIfSpatialTypeIsNotAllowed(Dummy.Z0_GeographyInfo);
				Assert("Dummy bizo has no error after Z0_Geography is set with valid polygon.", !Dummy.Z0_GeographyInfo.HasErrors());

				Dummy.Z0_Geography = ZGeography.CreatePolygon("0 0,1 1,0 1,1 0,0 0", true).MakeValid();
				SpatialTypeValidation.ErrorIfSpatialTypeIsNotAllowed(Dummy.Z0_GeographyInfo);
				Assert("Dummy bizo has error after Z0_Geography is set with multi polygon.", Dummy.Z0_GeographyInfo.HasErrors());
			}
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestMessageErrorIfSpatialTypeIsNotAllowed()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Assert("Precondition: Dummy bizo has no error.", !Dummy.HasErrors);

				Dummy.Z0_Geography = ZGeography.Empty;
				SpatialTypeValidation.MessageErrorIfSpatialTypeIsNotAllowed(Dummy.Z0_GeographyInfo);
				Assert("Dummy bizo has no Message error after Z0_Geography is set with Empty.", !Dummy.Z0_GeographyInfo.HasMessageErrors());

				Dummy.Z0_Geography = ZGeography.CreatePoint(1, 2);
				SpatialTypeValidation.MessageErrorIfSpatialTypeIsNotAllowed(Dummy.Z0_GeographyInfo);
				Assert("Dummy bizo has no Message error after Z0_Geography is set with point.", !Dummy.Z0_GeographyInfo.HasMessageErrors());

				Dummy.Z0_Geography = ZGeography.CreatePolygon("0 0,1 0,1 1,0 1,0 0").MakeValid();
				SpatialTypeValidation.MessageErrorIfSpatialTypeIsNotAllowed(Dummy.Z0_GeographyInfo);
				Assert("Dummy bizo has no Message error after Z0_Geography is set with valid polygon.", !Dummy.Z0_GeographyInfo.HasMessageErrors());

				Dummy.Z0_Geography = ZGeography.CreatePolygon("0 0,1 1,0 1,1 0,0 0", true).MakeValid();
				SpatialTypeValidation.MessageErrorIfSpatialTypeIsNotAllowed(Dummy.Z0_GeographyInfo);
				Assert("Dummy bizo has Message error after Z0_Geography is set with multi polygon.", Dummy.Z0_GeographyInfo.HasMessageErrors());
			}
		}

		[RequiresSoftware(RequiredSoftware.SqlServerSpatial110)]
		public void TestWarnIfSpatialTypeIsNotAllowed()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Assert("Precondition: Dummy bizo has no warnings.", !Dummy.HasWarnings);

				Dummy.Z0_Geography = ZGeography.Empty;
				SpatialTypeValidation.WarnIfSpatialTypeIsNotAllowed(Dummy.Z0_GeographyInfo);
				Assert("Dummy bizo has no warnings after Z0_Geography is set with Empty.", !Dummy.Z0_GeographyInfo.HasWarnings());

				Dummy.Z0_Geography = ZGeography.CreatePoint(1, 2);
				SpatialTypeValidation.WarnIfSpatialTypeIsNotAllowed(Dummy.Z0_GeographyInfo);
				Assert("Dummy bizo has no warnings after Z0_Geography is set with point.", !Dummy.Z0_GeographyInfo.HasWarnings());

				Dummy.Z0_Geography = ZGeography.CreatePolygon("0 0,1 0,1 1,0 1,0 0").MakeValid();
				SpatialTypeValidation.WarnIfSpatialTypeIsNotAllowed(Dummy.Z0_GeographyInfo);
				Assert("Dummy bizo has no warnings after Z0_Geography is set with valid polygon.", !Dummy.Z0_GeographyInfo.HasWarnings());

				Dummy.Z0_Geography = ZGeography.CreatePolygon("0 0,1 1,0 1,1 0,0 0", true).MakeValid();
				SpatialTypeValidation.WarnIfSpatialTypeIsNotAllowed(Dummy.Z0_GeographyInfo);
				Assert("Dummy bizo has warnings after Z0_Geography is set with multi polygon.", Dummy.Z0_GeographyInfo.HasWarnings());
			}
		}
	}
}
