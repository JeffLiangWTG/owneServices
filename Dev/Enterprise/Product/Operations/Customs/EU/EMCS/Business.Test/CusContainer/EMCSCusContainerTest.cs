using System;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	[TestedType(typeof(EMCSCusContainer))]
	sealed class EMCSCusContainerBaseOnlyTest : EMCSCusContainerTest<EMCSCusContainer, EMCSJobDeclaration>
	{
	}

	public abstract class EMCSCusContainerTest<TCusContainer, TJobDeclaration> : Customs.Business.Testing.BaseCusContainerTest<TCusContainer, TJobDeclaration>
		where TCusContainer : EMCSCusContainer
		where TJobDeclaration : EMCSJobDeclaration
	{
		public override void TestHumanReadableName()
		{
			AssertEquals("Transport", container.HumanReadableName);
		}

		public override void TestContainerValidation()
		{
			AssertType<EMCSCusContainerValidation>("Checking CusContainer.Validation", container.Validation);
		}

		public override void TestContainerNumber_CO_JE_UniqueIndex()
		{
			var container2 = container.Declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "GCFU0000019";
			container2.CO_ContainerNumber = "GCFU0000019";
			AssertHasMessageError(container2.CO_ContainerNumberInfo, "Identity must be unique.");
		}

		public void TestTypeDecider()
		{
			Assert("EMCSCusContainer only load from EMCSJobDeclaration.", true);
		}

		public void TestComments()
		{
			container.Comment = "An interesting comment";
			AssertEquals("An interesting comment", container.Comment);

			Factory.Save();

			var containerNotes = container.Notes;
			AssertEquals("Length", 1, containerNotes.DatabaseCount);

			var note = containerNotes.FindByDescription(PredefinedNoteTypes.Instance.ContainerComment.Description)[0];
			AssertEquals("An interesting comment", note.ST_NoteText);

			var otherFactory = new BusinessObjectFactory();
			var containerCopy = (EMCSCusContainer)otherFactory.Load(GetExpectedBusinessObjectType(), container.PK);

			AssertEquals("An interesting comment", containerCopy.Comment);

			containerCopy.Comment = "Changed the comment";
			otherFactory.Save();

			var thirdFactory = new BusinessObjectFactory();
			var container3 = (EMCSCusContainer)thirdFactory.Load(GetExpectedBusinessObjectType(), container.PK);

			AssertEquals("Changed the comment", container3.Comment);
		}

		public void TestSealDetails()
		{
			container.SealDetails = "Lots of details";
			AssertEquals("Lots of details", container.SealDetails);

			Factory.Save();

			var containerNotes = container.Notes;
			AssertEquals("Length", 1, containerNotes.DatabaseCount);

			var note = containerNotes.FindByDescription(PredefinedNoteTypes.Instance.ContainerSealDetails.Description)[0];
			AssertEquals("Lots of details", note.ST_NoteText);

			var otherFactory = new BusinessObjectFactory();
			var containerCopy = (EMCSCusContainer)otherFactory.Load(GetExpectedBusinessObjectType(), container.PK);
			AssertEquals("Lots of details", containerCopy.SealDetails);

			containerCopy.SealDetails = "Changed the details";
			otherFactory.Save();

			var thirdFactory = new BusinessObjectFactory();
			var container3 = (EMCSCusContainer)thirdFactory.Load(GetExpectedBusinessObjectType(), container.PK);

			AssertEquals("Changed the details", container3.SealDetails);
		}

		public new void TestContainerCase()
		{
			var container = (EMCSCusContainer)GetNewBusinessObject();

			container.CO_ContainerNumber = "ABC def";
			AssertEquals("It's not forced to uppercase", "ABC def", container.CO_ContainerNumber);
		}

		public new void TestSealCase()
		{
			var container = (EMCSCusContainer)GetNewBusinessObject();

			container.CO_Seal = "ABC def";
			AssertEquals("It's not forced to uppercase", "ABC def", container.CO_Seal);
		}

		public void TestSealDetails_Caption()
		{
			AssertEquals("Seal Information", DataBoundResourceStrings.GetDataForProperty(container.SealDetailsInfo).Caption);
		}

		public void TestComment_Caption()
		{
			AssertEquals("Complementary Information (Transport Details)", DataBoundResourceStrings.GetDataForProperty(container.CommentInfo).Caption);
		}

		protected override ICustomLabelsProvider GetNewCustomLabelsProvider(BusinessObject bo) => new BaseCusContainer.CustomLabelsProvider(((EMCSCusContainer)bo).Declaration);

		protected override BaseJobDeclaration GetJobDeclaration(BusinessObjectFactory factory)
		{
			var declaration = factory.New<EMCSJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			return declaration;
		}

		protected override Type ExpectedMetadataType => typeof(Metadata.Business.EMCSCusContainer);

		protected override BusinessObject GetNewBusinessObject()
		{
			var declaration = Factory.New<EMCSJobDeclaration>();
			declaration.DisableDefaultPackingInformation = true;
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;
			return declaration.CusContainers.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			var declaration = Factory.New<EMCSJobDeclaration>();
			container = declaration.CusContainers.AddNew();
		}

		EMCSCusContainer container;
	}
}
