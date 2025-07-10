using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting
{
	sealed class LocalTransportJobTypeCodeDescriptionPairProviderTest : CodeDescriptionPairListProviderTest
	{
		#region TestIsReturningCorrectCollection

		public override void TestIsReturningCorrectCollection()
		{
			var factory = new BusinessObjectFactory();
			var type = factory.New(ObjectFactory.GetType<ICommonCartageType>());
			var hiddenType = factory.New(ObjectFactory.GetType<ICommonCartageType>());

			type[LocalCartageJobTypeSchema.E3_JobType] = "XXXX";
			type[LocalCartageJobTypeSchema.E3_Description] = "Description of XXXX";
			type[LocalCartageJobTypeSchema.E3_GE] = GlbDepartment.CurrentDepartment.PK;

			hiddenType[LocalCartageJobTypeSchema.E3_JobType] = "HIDE";
			hiddenType[LocalCartageJobTypeSchema.E3_Description] = "Description of HIDE";
			hiddenType[LocalCartageJobTypeSchema.E3_IsHidden] = true;
			hiddenType[LocalCartageJobTypeSchema.E3_GE] = GlbDepartment.CurrentDepartment.PK;

			factory.Save();

			var list = CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList();
			Assert(list.ContainsCode("XXXX"));
			AssertEquals("Description of XXXX", list.GetDescriptionFromCode("XXXX"));
			Assert(!list.ContainsCode("HIDE"));
		}

		#endregion

		#region Implementation

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return new LocalTransportJobTypeCodeDescriptionPairProvider();
		}

		#endregion
	}
}
