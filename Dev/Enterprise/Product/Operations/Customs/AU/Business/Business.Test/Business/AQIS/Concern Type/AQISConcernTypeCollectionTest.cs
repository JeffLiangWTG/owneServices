using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(AQISConcernTypeCollection))]
	sealed class AQISConcernTypeCollectionTest : AQISSingleValueCollectionTest<AQISConcernTypeCollection>
	{
		public void TestContainsConcernType()
		{
			AQISConcernType concernType1 = declaration.AQISConcernTypes.AddNew();
			concernType1.Code = "A";
			AQISConcernType concernType2 = declaration.AQISConcernTypes.AddNew();
			concernType2.Code = "B";
			Assert(declaration.AQISConcernTypes.ContainsConcernType("A"));
			Assert(declaration.AQISConcernTypes.ContainsConcernType("B"));
			Assert(!declaration.AQISConcernTypes.ContainsConcernType("C"));
		}

		public override ZPropertyInfo AddInfoProperty => declaration.AddInfo.ZA_AQISConcern_HiddenInfo;

		public override AQISSingleValueBusinessObject FirstBizObjToAdd
		{
			get
			{
				AQISConcernType concernType = declaration.AQISConcernTypes.AddNew();
				concernType.Code = "1";
				return concernType;
			}
		}

		public override AQISSingleValueBusinessObject SecondBizObjToAdd
		{
			get
			{
				AQISConcernType concernType = declaration.AQISConcernTypes.AddNew();
				concernType.Code = "2";
				return concernType;
			}
		}

		public override AQISSingleValueCollection CollectionToTestWith => declaration.AQISConcernTypes;

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AQISConcernType(Factory);

		protected override AQISConcernTypeCollection GetCollectionToTest() => new AQISConcernTypeCollection(Factory, declaration);

		JobDeclaration declaration;
		protected override void SetUp()
		{
			base.SetUp();
			declaration = JobDeclaration.New(Factory);
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			declaration.AddInfo.ZA_AQISConcern_Hidden = "BARK,BVOL";
		}
	}
}
