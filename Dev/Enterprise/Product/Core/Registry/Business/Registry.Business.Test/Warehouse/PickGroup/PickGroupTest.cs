using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Registry.Business.Testing
{
	[TestedType(typeof(PickGroup))]
	sealed class PickGroupTest : RegistryBusinessObjectTemplateTestCase<PickGroup>
	{
		#region TestCodePropertyAttribute

		public void TestCodePropertyAttribute()
		{
			AssertEquals(PickGroup.Schema.PickSequence, CodePropertyAttribute.CodePropertyNameFromType(typeof(PickGroup)));
		}

		#endregion

		#region TestDescriptionPropertyAttribute

		public void TestDescriptionPropertyAttribute()
		{
			AssertEquals(PickGroup.Schema.Description, DescriptionPropertyAttribute.DescriptionPropertyNameFromType(typeof(PickGroup)));
		}

		#endregion

		#region Properties
		#region TestGroupDescriptionInfo

		public void TestGroupDescriptionInfo()
		{
			var pickGroup = new PickGroup();
			AssertEquals(256, pickGroup.DescriptionInfo.MaxLength);
		}

		#endregion

		#region TestPickSequence

		public void TestPickSequence()
		{
			var pickGroup = new PickGroup();
			pickGroup.PickSequence = 1;
			AssertEquals(new ZShort(1), pickGroup.PickSequence);
		}

		#endregion

		#endregion

		#region Validation

		#region TestDescription_IsMandatory

		public void TestDescription_IsMandatory()
		{
			var pickGroup = new PickGroup();
			pickGroup.Description = (NoResString)"";
			AssertHasError(pickGroup.DescriptionInfo, "Please enter a value.");

			pickGroup.Description = (NoResString)"ENGLISH";
			AssertNoErrors(pickGroup.DescriptionInfo);

			pickGroup.Description = (NoResString)"";
			AssertHasError(pickGroup.DescriptionInfo, "Please enter a value.");
		}

		#endregion

		#region TestPickSequence_ShouldNotBeZeroOrNegative

		public void TestPickSequence_ShouldNotBeZeroOrNegative()
		{
			var pickGroup = new PickGroup();
			pickGroup.PickSequence = 0;
			AssertHasError(pickGroup.PickSequenceInfo, "value cannot be zero.");

			pickGroup.PickSequence = -1;
			AssertHasError(pickGroup.PickSequenceInfo, "value cannot be negative.");

			pickGroup.PickSequence = 1;
			AssertNoErrors(pickGroup.PickSequenceInfo);
		}

		#endregion

		#region TestPickSequence_ShouldBeUnique

		public void TestPickSequence_ShouldBeUnique()
		{
			var collection = new PickGroupCollection();
			var pickGroup1 = collection.AddNew();
			pickGroup1.PickSequence = 1;

			var pickGroup2 = collection.AddNew();
			pickGroup2.PickSequence = 1;
			AssertHasError(pickGroup2.PickSequenceInfo, "Pick Sequence must be unique.");

			pickGroup2.PickSequence = 2;
			AssertNoErrors(pickGroup2.PickSequenceInfo);
		}

		#endregion

		#region TestRunPreSaveValidation

		public void TestRunPreSaveValidation()
		{
			var pickGroup = new PickGroup();
			AssertNoErrors("Precondition:", pickGroup.PickSequenceInfo);
			AssertNoErrors("Precondition:", pickGroup.DescriptionInfo);

			pickGroup.RunPreSaveValidation();
			AssertHasError(pickGroup.PickSequenceInfo, "value cannot be zero.");
			AssertHasError(pickGroup.DescriptionInfo, "Please enter a value.");
		}

		#endregion

		#endregion

		#region Serialisation

		#region TestSerialiseEmptyPickGroup

		public void TestSerialiseEmptyPickGroup()
		{
			var picKGroup = new PickGroup();

			var serializedValue = RegistryBusinessObjectTemplateTestCase.Serialize(picKGroup);
			AssertEquals(@"<?xml version=""1.0"" encoding=""utf-16""?><PickGroup><PickSequence>0</PickSequence><Description /></PickGroup>",
				Encoding.Unicode.GetString(serializedValue).Trim());

			var deserialisedPickGroup = RegistryBusinessObjectTemplateTestCase.Deserialize<PickGroup>(serializedValue);
			AssertEquals(new ZShort(0), deserialisedPickGroup.PickSequence);
			AssertEquals("", deserialisedPickGroup.Description);
		}

		#endregion

		#region TestSerialiseFullyPopulatedPickGroup

		public void TestSerialiseFullyPopulatedPickGroup()
		{
			var picKGroup = new PickGroup();
			picKGroup.PickSequence = 2;
			picKGroup.Description = (NoResString)"Default Desc";

			var serializedValue = RegistryBusinessObjectTemplateTestCase.Serialize(picKGroup);
			AssertEquals(@"<?xml version=""1.0"" encoding=""utf-16""?><PickGroup><PickSequence>2</PickSequence><Description>Default Desc</Description></PickGroup>",
				Encoding.Unicode.GetString(serializedValue).Trim());

			var deserialisedPickGroup = RegistryBusinessObjectTemplateTestCase.Deserialize<PickGroup>(serializedValue);
			AssertEquals(new ZShort(2), deserialisedPickGroup.PickSequence);
			AssertEquals("Default Desc", deserialisedPickGroup.Description);
		}

		#endregion

		#endregion

		#region ICodeDescription

		#region TestCode

		public void TestCode()
		{
			var pickGroup = new PickGroup();
			pickGroup.PickSequence = 3;
			AssertEquals("3", pickGroup.Code);
		}

		#endregion

		#region TestDescription

		public void TestDescription()
		{
			var pickGroup = new PickGroup();
			pickGroup.Description = (NoResString)"Desc";
			AssertEquals("Desc", pickGroup.Description);
		}

		#endregion

		#endregion

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return false; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return false; }
		}

		protected override PickGroup GetBusinessObjectToClone()
		{
			return new PickGroup();
		}

		protected override PickGroup GetBusinessObjectToSerialise()
		{
			return new PickGroup();
		}

		#endregion
	}
}
