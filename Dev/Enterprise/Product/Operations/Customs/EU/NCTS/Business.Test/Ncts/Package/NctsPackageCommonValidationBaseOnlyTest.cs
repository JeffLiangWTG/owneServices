using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPackageCommonValidationBaseOnlyTest : BusinessObjectValidationTestCase
	{
		public void TestCheckB5_UnitType_List()
		{
			var packType = Factory.SetupStandardPackCusCode();
			ValidationTestHelper.AssertInvalidCodeMessageError(Parent.B5_UnitTypeInfo, "~Z", packType);
		}

		public void TestCheckB5_MarksAndNumbers_Mandatory()
		{
			var bulkType = Factory.SetupBulkCusCode();
			var unpackType = Factory.SetupUnpackCusCode();
			CombineAssertions(() =>
			{
				Parent.B5_UnitType = "$%";
				Parent.Validation.ValidateB5_MarksAndNumbers();
				AssertHasMessageErrorContaining("No Marks", Parent.B5_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
				Parent.B5_UnitType = bulkType;
				Parent.Validation.ValidateB5_MarksAndNumbers();
				AssertNoMessageErrorContaining("Bulk type", Parent.B5_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
				Parent.B5_UnitType = unpackType;
				Parent.Validation.ValidateB5_MarksAndNumbers();
				AssertNoMessageErrorContaining("Unpack type", Parent.B5_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
				Parent.B5_UnitType = "$%";
				Parent.B5_MarksAndNumbers = "MARKS AND NUMBERS";
				AssertNoMessageErrorContaining("Marks entered", Parent.B5_MarksAndNumbersInfo, MandatoryValidation.YouHaveNotEntered);
			});
		}

		public void TestCheckB5_UnitCount_CannotBeZero()
		{
			var unpackedType = Factory.SetupUnpackCusCode();
			const string errorMessage = MandatoryValidation.ValueCannotBeZero;

			CombineAssertions(() =>
			{
				Parent.B5_UnitType = unpackedType;
				Parent.Validation.ValidateB5_UnitCount();
				AssertNoWarningContaining("B5_UnitType is unpacked and B5_UnitCount isn't 0", Parent.B5_UnitCountInfo, errorMessage);

				Parent.B5_UnitCount = 0;
				AssertHasMessageErrorContaining("B5_UnitType is unpacked and B5_UnitCount is 0", Parent.B5_UnitCountInfo, errorMessage);

				Parent.B5_UnitType = ZString.Empty;
				Parent.Validation.ValidateB5_UnitCount();
				AssertNoWarningContaining("B5_UnitType is empty", Parent.B5_UnitCountInfo, errorMessage);

				Parent.B5_UnitType = "AA";
				Parent.Validation.ValidateB5_UnitCount();
				AssertNoWarningContaining("B5_UnitType isn't unpacked", Parent.B5_UnitCountInfo, errorMessage);
			});
		}

		NctsPackageForTest parent;
		NctsPackageForTest Parent
		{
			get
			{
				if (parent is null)
				{
					var dummyBusinessObject = Factory.New<DummyBusinessObject>();
					parent = Factory.New<NctsPackageForTest>();
					parent.B5_ParentTableCode = dummyBusinessObject.TablePrefix;
					parent.B5_ParentID = dummyBusinessObject.PK;
				}
				return parent;
			}
		}
	}

	class NctsPackageForTest : NctsPackage
	{
		public NctsPackageForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override CusInvPackValidation GetNewPhase5Validation() => new NctsPackageCommonValidationForTest(this);

		protected override CusInvPackValidation GetNewPhase4Validation() => new NctsPackageCommonValidationForTest(this);

		public void AttachToParent(NctsCommonCargoDesc parent)
		{
			B5_ParentTableCode = parent.TablePrefix;
			B5_ParentID = parent.PK;
		}

		protected override bool AutomaticSequenceNumberEnabled => false;
	}

	class NctsPackageCommonValidationForTest : NctsPackageCommonValidation
	{
		public NctsPackageCommonValidationForTest(NctsPackage parent)
			: base(parent)
		{
		}
	}
}
