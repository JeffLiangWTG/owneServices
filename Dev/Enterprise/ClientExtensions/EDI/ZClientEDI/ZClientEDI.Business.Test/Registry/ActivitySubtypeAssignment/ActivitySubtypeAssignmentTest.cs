using System;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.Business.Test
{
	[TestedType(typeof(ActivitySubtypeAssignment))]
	internal sealed class ActivitySubtypeAssignmentTest : RegistryBusinessObjectTemplateTestCase<ActivitySubtypeAssignment>
	{
		public void TestValidateActivitySubtype()
		{
			var tree = new CodeDescriptionBoolTreeNodeCollection(true, 3, 4);
			tree.AddSystemChildren();
			const string ALL = CodeDescriptionBoolTreeNode.AllCode;
			var parent = tree.Find(ALL, ALL, ALL);
			tree.Add("AAA", (NoResString)"Description for AAA", true, false, parent);
			tree.Add("BBB", (NoResString)"Description for BBB", true, false, parent);
			ProcessManagementRegistry.Instance.WorkItemTypeTree.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, tree);

			var assignment = NewPopulatedBusinessObject();
			assignment.ValidateActivitySubtype();
			AssertHasErrors(assignment.ActivitySubtypeInfo);

			assignment.ActivitySubtype = "AA";
			assignment.ValidateActivitySubtype();
			AssertHasErrors(assignment.ActivitySubtypeInfo);

			assignment.ActivitySubtype = "AAA";
			assignment.ValidateActivitySubtype();
			AssertNoErrors(assignment.ActivitySubtypeInfo);
		}

		public void TestValidateDescription()
		{
			var assignment = NewPopulatedBusinessObject();
			assignment.ValidateDescription();
			AssertHasErrors(assignment.DescriptionInfo);

			assignment.Description = "Some Description";
			assignment.ValidateDescription();
			AssertNoErrors(assignment.DescriptionInfo);
		}

		public void TestValidateCapitalization()
		{
			var assignment = NewPopulatedBusinessObject();

			assignment.Capitalization = true;
			assignment.ValidateCapitalization();
			AssertNoErrors(assignment.CapitalizationInfo);
		}

		#region Implementation

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		protected override ActivitySubtypeAssignment GetBusinessObjectToClone()
		{
			return NewPopulatedBusinessObject();
		}

		protected override ActivitySubtypeAssignment GetBusinessObjectToSerialise()
		{
			return NewPopulatedBusinessObject();
		}

		ActivitySubtypeAssignment NewPopulatedBusinessObject()
		{
			var result = new ActivitySubtypeAssignment(new FallbackLevel(Guid.Empty, Guid.Empty, Guid.Empty), Factory);
			return result;
		}

		#endregion
	}
}
